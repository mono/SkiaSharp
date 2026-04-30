#!/usr/bin/env python3
"""
Fetch SkiaSharp release data and manage the website release notes structure.

This script collects raw data. AI does the formatting using TEMPLATE.md.

Commands:
    # Diff a branch and write raw PR data to documentation/docfx/releases/{version}.md
    python3 generate-release-notes.py --branch main
    python3 generate-release-notes.py --branch release/3.119.x
    python3 generate-release-notes.py --branch release/4.147.0-preview.1

    # Regenerate TOC.yml and index.md from files on disk + create upcoming version file
    python3 generate-release-notes.py --update-toc

The --branch command writes directly to documentation/docfx/releases/{version}.md
with a YAML front-matter header containing metadata (branch, version, status, diff
range, PR count) followed by the raw PR list. AI then rewrites this file with
polished content. TOC and index are regenerated automatically.

Requirements: git, Python 3.7+
"""

from __future__ import annotations

import argparse
import re
import subprocess
import sys
from collections import defaultdict
from datetime import datetime, timezone
from pathlib import Path
from typing import Optional, Tuple


REPO = "mono/SkiaSharp"
RELEASES_DIR = Path("documentation/docfx/releases")

SKIA_REPO = "mono/skia"
SKIA_PR_PATTERNS = [
    re.compile(r"(?:companion|related)\s+(?:skia\s+)?pr[:\s]+https?://github\.com/mono/skia/pull/(\d+)", re.IGNORECASE),
    re.compile(r"https?://github\.com/mono/skia/pull/(\d+)"),
]

# Noreply email pattern: {id}+{username}@users.noreply.github.com
_NOREPLY_RE = re.compile(r"^\d+\+(.+)@users\.noreply\.github\.com$")


# ── Helpers ──────────────────────────────────────────────────────────


def _removeprefix(s, prefix):
    # type: (str, str) -> str
    """str.removeprefix polyfill for Python < 3.9."""
    if s.startswith(prefix):
        return s[len(prefix):]
    return s


def run(args, check=True):
    # type: (list[str], bool) -> str
    """Run a command and return stdout."""
    result = subprocess.run(args, capture_output=True, text=True, check=check)
    return result.stdout.strip()


def extract_base_version(tag):
    # type: (str) -> str
    """v3.119.2-preview.2.3 -> 3.119.2"""
    return tag.lstrip("v").split("-")[0]


def minor_group(version):
    # type: (str) -> str
    """3.119.2 -> 3.119"""
    parts = version.split(".")
    return "{}.{}".format(parts[0], parts[1]) if len(parts) >= 2 else parts[0]


def version_key(version):
    # type: (str) -> list[int]
    """Sortable key from version string."""
    return [int(n) for n in re.findall(r"\d+", version)]


def get_upcoming_version():
    # type: () -> Optional[str]
    """Read SKIASHARP_VERSION from azure-templates-variables.yml."""
    path = Path("scripts/azure-templates-variables.yml")
    if path.exists():
        for line in path.read_text().splitlines():
            m = re.match(r"\s*SKIASHARP_VERSION:\s*(\S+)", line)
            if m:
                return m.group(1)
    return None


def get_version_from_remote_branch(branch):
    # type: (str) -> Optional[str]
    """Read SKIASHARP_VERSION from a remote branch without checking it out."""
    try:
        content = run(["git", "show",
                       "origin/{}:scripts/azure-templates-variables.yml".format(branch)])
        for line in content.splitlines():
            m = re.match(r"\s*SKIASHARP_VERSION:\s*(\S+)", line)
            if m:
                return m.group(1)
    except subprocess.CalledProcessError:
        pass
    return None


def _is_stable_branch(branch):
    # type: (str) -> bool
    """Check if a branch is a stable release (not preview, not .x)."""
    if branch.endswith(".x"):
        return False
    return not re.search(r"-preview\.", branch)


def _login_from_email(email):
    # type: (str) -> str
    """Extract GitHub login from a commit email.

    Handles noreply format: 12345+username@users.noreply.github.com -> username
    Falls back to the local part of the email.
    """
    m = _NOREPLY_RE.match(email)
    if m:
        return m.group(1)
    return email.split("@")[0]


# ── Effort computation ───────────────────────────────────────────────


def _get_pr_effort_from_git(pr_num, target_ref="origin/main", git_dir="."):
    # type: (int, str, str) -> Tuple[int, set[str], set[str]]
    """Get commit count, working days, and author names from a PR via git refs.

    Fetches refs/pull/{N}/head, then uses merge-base with the target branch
    to determine the base commit. Works for both squash-merged and
    regular-merged PRs.

    Returns (commit_count, set_of_date_strings, set_of_author_names).
    """
    ref_head = "refs/pull/{}/head".format(pr_num)
    local_ref = "refs/pr-tmp/{}".format(pr_num)

    try:
        run(["git", "-C", git_dir, "fetch", "origin",
             "{}:{}".format(ref_head, local_ref), "--quiet"])
    except subprocess.CalledProcessError:
        return 0, set(), set()

    try:
        head_sha = run(["git", "-C", git_dir, "rev-parse", local_ref])
        base_sha = run(["git", "-C", git_dir, "merge-base",
                        local_ref, target_ref])
    except subprocess.CalledProcessError:
        run(["git", "-C", git_dir, "update-ref", "-d", local_ref], check=False)
        return 0, set(), set()

    log_output = run([
        "git", "-C", git_dir, "log",
        "--format=%ad\t%an", "--date=short",
        "{}..{}".format(base_sha, head_sha),
    ], check=False)

    run(["git", "-C", git_dir, "update-ref", "-d", local_ref], check=False)

    if not log_output:
        return 0, set(), set()

    count = 0
    days = set()  # type: set[str]
    authors = set()  # type: set[str]
    for line in log_output.splitlines():
        parts = line.split("\t", 1)
        if len(parts) != 2:
            continue
        date_str, author_name = parts
        count += 1
        days.add(date_str)
        authors.add(author_name)

    return count, days, authors


def compute_pr_effort(pr):
    # type: (dict) -> dict
    """Compute commit count and unique working days from git refs.

    Uses refs/pull/N/merge to get base..head range for commit counting.
    If the PR body references a companion mono/skia PR, fetches that PR's
    commits too (filtered to SkiaSharp contributor authors only).
    """
    pr_num = pr.get("number", 0)
    commit_count, unique_days, pr_author_names = _get_pr_effort_from_git(pr_num)

    skia_pr_num = None
    body = pr.get("body") or ""
    for pattern in SKIA_PR_PATTERNS:
        m = pattern.search(body)
        if m:
            skia_pr_num = m.group(1)
            break

    skia_commits = 0
    if skia_pr_num:
        skia_commits, skia_days = _fetch_skia_pr_effort(
            skia_pr_num, pr_author_names)
        unique_days |= skia_days

    return {
        "commitCount": commit_count + skia_commits,
        "workingDays": len(unique_days),
        "skiaPr": int(skia_pr_num) if skia_pr_num else None,
    }


def _fetch_skia_pr_effort(pr_num, author_names):
    # type: (str, set[str]) -> Tuple[int, set[str]]
    """Fetch effort from a mono/skia PR using git refs on the submodule.

    Fetches refs/pull/N/head and uses merge-base to find the range,
    then filters commits to only those by SkiaSharp PR authors
    (excluding upstream Google committers).

    Returns (commit_count, set_of_date_strings).
    """
    skia_dir = Path("externals/skia")
    if not (skia_dir / ".git").exists():
        return 0, set()

    ref_head = "refs/pull/{}/head".format(pr_num)
    local_ref = "refs/pr-tmp/skia-{}".format(pr_num)

    try:
        run(["git", "-C", str(skia_dir), "fetch", "origin",
             "{}:{}".format(ref_head, local_ref), "--quiet"])
    except subprocess.CalledProcessError:
        return 0, set()

    # Use merge-base with the skiasharp branch (the usual target for mono/skia PRs)
    base_sha = None
    for target in ["origin/skiasharp", "origin/main"]:
        try:
            base_sha = run(["git", "-C", str(skia_dir), "merge-base",
                            local_ref, target])
            break
        except subprocess.CalledProcessError:
            continue

    if not base_sha:
        run(["git", "-C", str(skia_dir), "update-ref", "-d", local_ref],
            check=False)
        return 0, set()

    try:
        head_sha = run(["git", "-C", str(skia_dir), "rev-parse", local_ref])
    except subprocess.CalledProcessError:
        run(["git", "-C", str(skia_dir), "update-ref", "-d", local_ref],
            check=False)
        return 0, set()

    log_output = run([
        "git", "-C", str(skia_dir), "log",
        "--format=%ad\t%an", "--date=short",
        "{}..{}".format(base_sha, head_sha),
    ], check=False)

    run(["git", "-C", str(skia_dir), "update-ref", "-d", local_ref],
        check=False)

    if not log_output:
        return 0, set()

    count = 0
    days = set()  # type: set[str]
    for line in log_output.splitlines():
        parts = line.split("\t", 1)
        if len(parts) != 2:
            continue
        date_str, author_name = parts
        if author_name in author_names:
            count += 1
            days.add(date_str)

    return count, days


# ── Branch diffing ──────────────────────────────────────────────────


def list_remote_release_branches():
    # type: () -> list[str]
    """List all remote release branches (without origin/ prefix).

    Returns branch names like 'release/3.119.x', 'release/3.119.0-preview.1'.
    """
    output = run(["git", "branch", "-r"], check=False)
    branches = []
    for line in output.splitlines():
        line = line.strip()
        if "->" in line:
            continue
        if line.startswith("origin/release/"):
            branches.append(line[len("origin/"):])
    return branches


def release_branch_sort_key(branch):
    # type: (str) -> tuple
    """Compute semver sort key for a release branch.

    Sorting order:
      release/X.Y.x             -> (X, Y, -1, 0, 0)  -- base, sorts first
      release/X.Y.Z-preview.N   -> (X, Y,  Z, 0, N)
      release/X.Y.Z             -> (X, Y,  Z, 1, 0)  -- stable after previews
    """
    m = re.match(r"release/(\d+)\.(\d+)\.(x|\d+(?:-preview\.\d+)?)$", branch)
    if not m:
        return (0, 0, 0, 0, 0)

    major = int(m.group(1))
    minor_num = int(m.group(2))
    rest = m.group(3)

    if rest == "x":
        return (major, minor_num, -1, 0, 0)

    m2 = re.match(r"(\d+)(?:-preview\.(\d+))?$", rest)
    if not m2:
        return (major, minor_num, 0, 0, 0)

    patch = int(m2.group(1))
    preview = m2.group(2)

    if preview is not None:
        return (major, minor_num, patch, 0, int(preview))
    return (major, minor_num, patch, 1, 0)


def version_from_branch(branch):
    # type: (str) -> str
    """Extract display version from a release branch name.

    release/4.147.0-preview.1 -> 4.147.0
    release/3.119.2            -> 3.119.2
    release/3.119.x            -> 3.119.x
    """
    m = re.match(r"release/(.+)$", branch)
    if not m:
        return branch
    ver = m.group(1)
    if ver.endswith(".x"):
        return ver
    return ver.split("-")[0]


def find_previous_stable_base(all_branches, major, minor_num, patch):
    # type: (list[str], int, int, int) -> Optional[str]
    """Find the previous stable release branch to use as the cumulative diff base.

    For version X.Y.Z:
    1. If Z > 0: look for release/X.Y.(Z-1) stable, fall back to latest preview
    2. If Z == 0: look for the latest branch from any previous minor/major

    Falls back to None if nothing found.
    """
    minor = "{}.{}".format(major, minor_num)

    # Case 1: Z > 0 — look for stable release of previous patch
    if patch > 0:
        prev_version = "{}.{}".format(minor, patch - 1)
        # Prefer stable
        prev_stable = "release/{}".format(prev_version)
        if prev_stable in all_branches:
            return prev_stable
        # No stable — find latest preview of that patch
        prev_candidates = [b for b in all_branches
                           if b.startswith("release/{}-preview.".format(
                               prev_version))]
        if prev_candidates:
            prev_candidates.sort(key=release_branch_sort_key)
            return prev_candidates[-1]
        # No previous patch at all — fall back to any earlier patch
        earlier = [b for b in all_branches
                   if b.startswith("release/{}.".format(minor))
                   and not b.endswith(".x")]
        earlier = [b for b in earlier
                   if release_branch_sort_key(b) <
                   (major, minor_num, patch, 0, 0)]
        if earlier:
            earlier.sort(key=release_branch_sort_key)
            return earlier[-1]

    # Case 2: Z == 0 (or no previous patch found) — search previous minors
    all_versioned = [b for b in all_branches if not b.endswith(".x")]
    all_versioned.sort(key=release_branch_sort_key)

    # Find the latest branch that sorts before our minor
    target_key = (major, minor_num, -2, 0, 0)  # before even .x
    candidates = [b for b in all_versioned
                  if release_branch_sort_key(b) < target_key]
    if candidates:
        return candidates[-1]

    return None


def determine_diff_range(branch):
    # type: (str) -> Tuple[str, str, str]
    """Determine the git diff range for a branch.

    Uses cumulative diffs: always diffs from the previous stable base,
    so that previews produce a full rollup of all changes.

    Returns (from_ref, to_ref, version_display).
    """
    all_branches = list_remote_release_branches()

    # ── main ─────────────────────────────────────────────────────
    if branch == "main":
        version = get_upcoming_version()
        if not version:
            raise RuntimeError(
                "Cannot read SKIASHARP_VERSION from "
                "scripts/azure-templates-variables.yml")
        minor = minor_group(version)

        # Find latest release branch for the same minor
        candidates = [b for b in all_branches
                      if b.startswith("release/{}.".format(minor))
                      and not b.endswith(".x")]

        # Fallback: latest release branch across ALL minors
        if not candidates:
            candidates = [b for b in all_branches if not b.endswith(".x")]

        if not candidates:
            raise RuntimeError(
                "No release branches found. Cannot determine diff range "
                "for main. Ensure release branches are fetched.")

        candidates.sort(key=release_branch_sort_key)
        latest = candidates[-1]
        return "origin/{}".format(latest), "origin/main", version

    # ── servicing branch (release/X.Y.x) ────────────────────────
    m_svc = re.match(r"release/(\d+)\.(\d+)\.x$", branch)
    if m_svc:
        major = int(m_svc.group(1))
        minor_num = int(m_svc.group(2))
        minor = "{}.{}".format(major, minor_num)

        # Read version from the remote branch
        version = get_version_from_remote_branch(branch)
        if not version:
            version = "{}.0".format(minor)

        # All versioned branches for this minor (exclude .x)
        candidates = [b for b in all_branches
                      if b.startswith("release/{}.".format(minor))
                      and b != branch
                      and not b.endswith(".x")]

        if candidates:
            candidates.sort(key=release_branch_sort_key)
            latest = candidates[-1]
            return ("origin/{}".format(latest),
                    "origin/{}".format(branch),
                    version)

        # No versioned branches — use previous minor
        base = find_previous_stable_base(all_branches, major, minor_num, 0)
        if base:
            return ("origin/{}".format(base),
                    "origin/{}".format(branch),
                    version)

        base_sha = run(["git", "merge-base",
                        "origin/{}".format(branch), "origin/main"])
        return base_sha, "origin/{}".format(branch), version

    # ── versioned branch (release/X.Y.Z or release/X.Y.Z-preview.N) ─
    m_ver = re.match(r"release/(\d+)\.(\d+)\.(\d+)", branch)
    if not m_ver:
        raise RuntimeError("Cannot parse branch: {}".format(branch))

    major = int(m_ver.group(1))
    minor_num = int(m_ver.group(2))
    patch = int(m_ver.group(3))
    version = version_from_branch(branch)  # strips preview suffix

    # Find the cumulative base: previous stable (or previous minor for Z==0)
    base = find_previous_stable_base(all_branches, major, minor_num, patch)

    if base:
        return ("origin/{}".format(base),
                "origin/{}".format(branch),
                version)

    # Last resort: merge-base with main
    base_sha = run(["git", "merge-base",
                    "origin/{}".format(branch), "origin/main"])
    return base_sha, "origin/{}".format(branch), version


def get_prs_from_diff(from_ref, to_ref):
    # type: (str, str) -> list[dict]
    """Extract merged PRs from git log between two refs.

    Parses PR numbers, titles, authors, and bodies from commit messages.
    No GitHub API calls needed — everything comes from git.
    """
    # Use a format that gives us everything: hash, author email, subject, body
    # Separator between commits: a line that won't appear in commit messages
    SEP = "---COMMIT-END-7f3b---"
    log = run(["git", "log",
               "--format=%H%n%ae%n%s%n%b{}".format(SEP),
               "{}..{}".format(from_ref, to_ref)])

    prs = []
    seen = set()  # type: set[int]

    for block in log.split(SEP):
        block = block.strip()
        if not block:
            continue
        lines = block.split("\n", 3)
        if len(lines) < 3:
            continue

        commit_hash = lines[0].strip()
        author_email = lines[1].strip()
        subject = lines[2].strip()
        body = lines[3].strip() if len(lines) > 3 else ""

        # Extract PR number from subject: "Some title (#1234)"
        m = re.search(r"\(#(\d+)\)\s*$", subject)
        if not m:
            continue
        num = int(m.group(1))
        if num in seen:
            continue
        seen.add(num)

        # Title is the subject minus the PR ref
        title = re.sub(r"\s*\(#\d+\)\s*$", "", subject)

        # Author login from email
        login = _login_from_email(author_email)

        prs.append({
            "title": title,
            "author": {"login": login},
            "url": "https://github.com/{}/pull/{}".format(REPO, num),
            "number": num,
            "body": body,
        })

    if not prs:
        return []

    # Compute effort for each PR (uses git refs/pull/N/merge)
    result = []
    for i, pr in enumerate(prs, 1):
        try:
            pr.update(compute_pr_effort(pr))
        except subprocess.CalledProcessError:
            pass  # effort stays unset, that's fine
        result.append(pr)
        if i % 20 == 0:
            print("  Processed {}/{} PRs...".format(i, len(prs)),
                  file=sys.stderr)

    return result


def format_pr_list(prs, metadata):
    # type: (list[dict], dict) -> str
    """Format the PR list as markdown with metadata header."""
    now = datetime.now(timezone.utc).strftime("%Y-%m-%dT%H:%M:%SZ")
    lines = [
        "<!--",
        "  Generated: {} by generate-release-notes.py".format(now),
        "",
        "  This is raw PR data. Rewrite this entire file with polished release",
        "  notes using documentation/docfx/releases/TEMPLATE.md as the reference.",
        "",
        "  version: {}".format(metadata["version"]),
        "  status:  {}".format(metadata["status"]),
        "  branch:  {}".format(metadata["branch"]),
        "  diff:    {}..{}".format(metadata["from"], metadata["to"]),
        "  prs:     {}".format(len(prs)),
        "-->",
        "",
    ]

    if not prs:
        lines.append("*No changes found.*")
    else:
        for pr in prs:
            title = pr.get("title", "")
            author = (pr.get("author") or {}).get("login", "unknown")
            url = pr.get("url", "")
            is_community = author != "mattleibow"
            community_str = " [community ✨]" if is_community else ""
            commits = pr.get("commitCount", 0)
            days = pr.get("workingDays", 0)
            effort = " ({} commit{}, {} day{})".format(
                commits, "s" if commits != 1 else "",
                days, "s" if days != 1 else "") if commits else ""
            skia_pr = pr.get("skiaPr")
            skia_str = " (skia: mono/skia#{})".format(skia_pr) if skia_pr else ""

            lines.append("- {} by @{} in {}{}{}{}".format(
                title, author, url, community_str, effort, skia_str))

    lines.append("")
    return "\n".join(lines)


# ── TOC and index generation ────────────────────────────────────────


def get_version_files():
    # type: () -> tuple[list[str], list[str]]
    """List version strings from existing markdown files.
    
    Returns (versions, next_versions) where next_versions are the
    base versions that have a -unreleased.md file.
    """
    versions = []
    next_versions = []
    for f in RELEASES_DIR.iterdir():
        if f.suffix == ".md" and f.name not in ("index.md", "TEMPLATE.md"):
            stem = f.stem
            if stem.endswith("-unreleased"):
                next_versions.append(stem[:-11])  # strip "-unreleased"
            else:
                versions.append(stem)
    versions.sort(key=version_key, reverse=True)
    next_versions.sort(key=version_key, reverse=True)
    return versions, next_versions


def generate_toc(versions, next_versions):
    # type: (list[str], list[str]) -> str
    """Generate TOC.yml grouped by major.minor, obsolete under one node."""
    next_set = set(next_versions)
    groups = defaultdict(list)
    for v in versions:
        groups[minor_group(v)].append(v)

    current = []
    obsolete = []
    for g in sorted(groups.keys(), key=lambda x: version_key(x), reverse=True):
        if int(g.split(".")[0]) < 3:
            obsolete.append(g)
        else:
            current.append(g)

    lines = ["- name: Overview", "  href: index.md"]

    for g in current:
        members = groups[g]
        lines.append("- name: Version {}.x".format(g))
        lines.append("  href: {}.md".format(members[0]))
        lines.append("  items:")
        for v in members:
            if v in next_set:
                lines.append("    - name: Version {} (Unreleased)".format(v))
                lines.append("      href: {}-unreleased.md".format(v))
            lines.append("    - name: Version {}".format(v))
            lines.append("      href: {}.md".format(v))

    if obsolete:
        lines.append("- name: Obsolete Versions")
        lines.append("  href: {}.md".format(groups[obsolete[0]][0]))
        lines.append("  items:")
        for g in obsolete:
            members = groups[g]
            lines.append("    - name: Version {}.x".format(g))
            lines.append("      href: {}.md".format(members[0]))
            if len(members) > 1:
                lines.append("      items:")
                for v in members:
                    lines.append("        - name: Version {}".format(v))
                    lines.append("          href: {}.md".format(v))

    return "\n".join(lines) + "\n"


def generate_index(versions, next_versions):
    # type: (list[str], list[str]) -> str
    """Generate index.md with version list grouped by major."""
    next_set = set(next_versions)
    lines = [
        "# Release Notes",
        "",
        "Release notes for all SkiaSharp versions.",
        "",
    ]

    major_groups = defaultdict(list)
    for v in versions:
        major_groups[v.split(".")[0]].append(v)

    for major in sorted(major_groups.keys(), key=int, reverse=True):
        lines.extend(["### SkiaSharp {}.x".format(major), ""])

        minor_groups_map = defaultdict(list)
        for v in major_groups[major]:
            minor_groups_map[minor_group(v)].append(v)

        for g in sorted(minor_groups_map.keys(),
                        key=lambda x: version_key(x), reverse=True):
            members = minor_groups_map[g]
            lines.append("- **Version {}.x**".format(g))
            for v in members:
                if v in next_set:
                    lines.append("  - [Version {} (Unreleased)]({}-unreleased.md)".format(v, v))
                lines.append("  - [Version {}]({}.md)".format(v, v))
        lines.append("")

    return "\n".join(lines)


# ── Commands ─────────────────────────────────────────────────────────


def cmd_update_toc():
    """Regenerate TOC.yml and index.md."""
    if not RELEASES_DIR.is_dir():
        print("Error: {} does not exist".format(RELEASES_DIR), file=sys.stderr)
        sys.exit(1)

    versions, next_versions = get_version_files()

    (RELEASES_DIR / "TOC.yml").write_text(generate_toc(versions, next_versions))
    print("Updated {}".format(RELEASES_DIR / "TOC.yml"))

    (RELEASES_DIR / "index.md").write_text(generate_index(versions, next_versions))
    print("Updated {}".format(RELEASES_DIR / "index.md"))


def cmd_branch(branch):
    # type: (str) -> None
    """Diff a branch against its predecessor and write raw data to the version file."""
    branch = _removeprefix(branch, "origin/")

    print("Fetching remote branches...")
    try:
        # Unshallow if needed (CI runners use shallow clones)
        run(["git", "fetch", "origin", "--unshallow", "--quiet"], check=False)
        # Fetch all release branches and main explicitly
        run(["git", "fetch", "origin",
             "refs/heads/release/*:refs/remotes/origin/release/*",
             "refs/heads/main:refs/remotes/origin/main",
             "--quiet"], check=True)
    except subprocess.CalledProcessError:
        print("ERROR: git fetch failed. Cannot determine branch diff range.")
        sys.exit(1)

    # Verify release branches are visible
    all_branches = list_remote_release_branches()
    if not all_branches:
        print("ERROR: No release branches found after fetch.")
        sys.exit(1)

    from_ref, to_ref, version = determine_diff_range(branch)

    # Human-readable display (strip origin/ prefix, shorten SHAs)
    from_display = _removeprefix(from_ref, "origin/")
    to_display = _removeprefix(to_ref, "origin/")
    if re.match(r"^[0-9a-f]{7,}$", from_display):
        from_display = from_display[:12]

    # Determine release status: unreleased / preview / stable
    is_main = (branch == "main")
    is_servicing = branch.endswith(".x")
    status = "unreleased"
    if not is_main and not is_servicing:
        tags = run(["git", "tag", "-l", "v{}*".format(version)], check=False)
        has_stable = False
        has_preview = False
        for tag in tags.splitlines():
            tag = tag.strip()
            if not tag:
                continue
            if "-preview" in tag or "-rc" in tag:
                has_preview = True
            else:
                has_stable = True
        if has_stable:
            status = "stable"
        elif has_preview:
            status = "preview"

    print("Branch: {}".format(branch))
    print("Version: {}".format(version))
    print("Status: {}".format(status))
    print("Diff: {}..{}".format(from_display, to_display))

    prs = get_prs_from_diff(from_ref, to_ref)
    print("Found {} PR(s)".format(len(prs)))

    # Determine output file:
    # - "main" branches (main, release/X.Y.x) → {version}-unreleased.md (unreleased)
    # - versioned branches (release/X.Y.Z*) → {version}.md (released)
    is_main_branch = (branch == "main")
    is_servicing = bool(re.match(r"release/\d+\.\d+\.x$", branch))

    if is_main_branch or is_servicing:
        filename = "{}-unreleased.md".format(version)
    else:
        filename = "{}.md".format(version)

    metadata = {
        "branch": branch,
        "version": version,
        "status": status,
        "from": from_display,
        "to": to_display,
    }
    content = format_pr_list(prs, metadata)

    output_path = RELEASES_DIR / filename
    RELEASES_DIR.mkdir(parents=True, exist_ok=True)
    output_path.write_text(content)
    print("Wrote {}".format(output_path))

    files_to_polish = [str(output_path)]

    # When a versioned branch is pushed, also regenerate the unreleased
    # file(s) — the diff range for main or the .x branch may have changed
    # because a new release branch now exists.
    if not is_main_branch and not is_servicing:
        extra_files = _regen_unreleased(branch)
        files_to_polish.extend(extra_files)

    # Regenerate TOC and index
    cmd_update_toc()

    # Print summary for the AI agent
    print("")
    print("========================================")
    print("Files to polish:")
    for f in files_to_polish:
        print("  - {}".format(f))
    print("========================================")


def _regen_unreleased(trigger_branch):
    # type: (str) -> list[str]
    """Regenerate unreleased files after a versioned branch push.

    When a new release/X.Y.Z branch appears, the diff ranges for
    main and/or the servicing release/X.Y.x branch may have changed.

    Returns a list of file paths that were written.
    """
    m = re.match(r"release/(\d+)\.(\d+)\.\d+", trigger_branch)
    if not m:
        return []

    written_files = []  # type: list[str]

    major = int(m.group(1))
    minor_num = int(m.group(2))
    minor = "{}.{}".format(major, minor_num)

    all_branches = list_remote_release_branches()

    # Regenerate the servicing branch (.x) if it exists
    svc_branch = "release/{}.x".format(minor)
    if svc_branch in all_branches:
        print("\nRegenerating unreleased for {}...".format(svc_branch))
        try:
            from_ref, to_ref, svc_version = determine_diff_range(svc_branch)
            from_display = _removeprefix(from_ref, "origin/")
            if re.match(r"^[0-9a-f]{7,}$", from_display):
                from_display = from_display[:12]
            to_display = _removeprefix(to_ref, "origin/")

            prs = get_prs_from_diff(from_ref, to_ref)
            metadata = {
                "branch": svc_branch,
                "version": svc_version,
                "status": "unreleased",
                "from": from_display,
                "to": to_display,
            }
            svc_path = RELEASES_DIR / "{}-unreleased.md".format(svc_version)
            svc_path.write_text(format_pr_list(prs, metadata))
            print("Wrote {} ({} PRs)".format(svc_path, len(prs)))
            written_files.append(str(svc_path))
        except (RuntimeError, subprocess.CalledProcessError) as e:
            print("  WARNING: Could not regenerate {}: {}".format(
                svc_branch, e), file=sys.stderr)

    # Regenerate main's unreleased file — but only if the trigger branch
    # is in the same minor as main's upcoming version. A push to a 3.119.x
    # preview doesn't change main's diff range if main is on 4.147.x.
    main_version = get_upcoming_version()
    main_minor = minor_group(main_version) if main_version else None
    if main_minor and main_minor == minor:
        print("\nRegenerating unreleased for main...")
        try:
            from_ref, to_ref, main_version = determine_diff_range("main")
            from_display = _removeprefix(from_ref, "origin/")
            if re.match(r"^[0-9a-f]{7,}$", from_display):
                from_display = from_display[:12]
            to_display = _removeprefix(to_ref, "origin/")

            prs = get_prs_from_diff(from_ref, to_ref)
            metadata = {
                "branch": "main",
                "version": main_version,
                "status": "unreleased",
                "from": from_display,
                "to": to_display,
            }
            main_path = RELEASES_DIR / "{}-unreleased.md".format(main_version)
            main_path.write_text(format_pr_list(prs, metadata))
            print("Wrote {} ({} PRs)".format(main_path, len(prs)))
            written_files.append(str(main_path))
        except (RuntimeError, subprocess.CalledProcessError) as e:
            print("  WARNING: Could not regenerate main: {}".format(e),
                  file=sys.stderr)

    return written_files


# ── Main ─────────────────────────────────────────────────────────────


def main():
    parser = argparse.ArgumentParser(
        description="Fetch SkiaSharp release data for the website",
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog=(
            "Examples:\n"
            "  %(prog)s --branch main                       "
            "Raw PR data -> releases/4.147.0.md\n"
            "  %(prog)s --branch release/3.119.x            "
            "Raw PR data -> releases/3.119.5.md\n"
            "  %(prog)s --update-toc                        "
            "Regenerate TOC + index\n"
        ),
    )
    parser.add_argument(
        "--branch",
        help="Diff branch against its predecessor and write raw PR data")
    parser.add_argument(
        "--update-toc", action="store_true",
        help="Regenerate TOC.yml + index.md")

    args = parser.parse_args()

    if not args.branch and not args.update_toc:
        parser.print_help()
        sys.exit(1)

    if args.branch and args.update_toc:
        parser.error("Specify only one of --branch or --update-toc")

    if args.update_toc:
        cmd_update_toc()
    elif args.branch:
        cmd_branch(args.branch)


if __name__ == "__main__":
    main()
