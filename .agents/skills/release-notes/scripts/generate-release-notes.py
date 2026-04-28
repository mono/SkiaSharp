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

Requirements: gh (GitHub CLI), git, Python 3.7+
"""

from __future__ import annotations

import argparse
import json
import re
import subprocess
import sys
from collections import defaultdict
from pathlib import Path
from typing import Optional, Tuple


REPO = "mono/SkiaSharp"
RELEASES_DIR = Path("documentation/docfx/releases")

SKIA_REPO = "mono/skia"
SKIA_PR_PATTERNS = [
    re.compile(r"(?:companion|related)\s+(?:skia\s+)?pr[:\s]+https?://github\.com/mono/skia/pull/(\d+)", re.IGNORECASE),
    re.compile(r"https?://github\.com/mono/skia/pull/(\d+)"),
]


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


def gh(args):
    # type: (list[str]) -> str
    """Run a gh CLI command and return stdout."""
    return run(["gh"] + args)


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


# ── Effort computation ───────────────────────────────────────────────


def compute_pr_effort(pr):
    # type: (dict) -> dict
    """Compute commit count and unique working days from PR commit data.

    If the PR body references a companion mono/skia PR, fetches that PR's
    commits too and merges the effort (only counting authors from the
    SkiaSharp PR to exclude unrelated upstream Skia committers).
    """
    commits = pr.get("commits", [])
    commit_count = len(commits)
    unique_days = set()

    pr_author_names = set()  # type: set[str]
    for c in commits:
        for a in c.get("authors", []):
            name = a.get("name", "")
            if name:
                pr_author_names.add(name)

    for c in commits:
        date_str = c.get("committedDate") or c.get("authoredDate", "")
        if date_str:
            unique_days.add(date_str[:10])

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
    """Fetch effort from a mono/skia PR using git log on the submodule.

    Uses the skia submodule directly to avoid GitHub API commit truncation
    (the API caps at 100-250 commits, but skia merge PRs can have thousands).
    Filters commits by author name to count only work by the SkiaSharp PR
    contributors, excluding unrelated upstream Skia committers.

    Returns (commit_count, set_of_date_strings).
    """
    skia_dir = Path("externals/skia")
    if not (skia_dir / ".git").exists():
        return 0, set()

    try:
        meta = json.loads(
            gh(["api", "repos/{}/pulls/{}".format(SKIA_REPO, pr_num),
                "--jq", "{base_sha: .base.sha, head_sha: .head.sha}"]))
        base_sha = meta["base_sha"]
        head_sha = meta["head_sha"]
    except (subprocess.CalledProcessError, json.JSONDecodeError, KeyError):
        return 0, set()

    run(["git", "-C", str(skia_dir), "fetch", "origin", head_sha, "--quiet"],
        check=False)

    try:
        run(["git", "-C", str(skia_dir), "cat-file", "-e", head_sha])
        run(["git", "-C", str(skia_dir), "cat-file", "-e", base_sha])
    except subprocess.CalledProcessError:
        return 0, set()

    log_output = run([
        "git", "-C", str(skia_dir), "log",
        "--format=%ad\t%an", "--date=short",
        "{}..{}".format(base_sha, head_sha),
    ], check=False)

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


def determine_diff_range(branch):
    # type: (str) -> Tuple[str, str, str]
    """Determine the git diff range for a branch.

    Returns (from_ref, to_ref, version_display).
    Refs include 'origin/' prefix where appropriate; from_ref may be a
    bare commit SHA when merge-base or initial commit is used.
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
                      if b.startswith("release/{}.".format(minor))]

        # Fallback: latest release branch across ALL minors
        if not candidates:
            candidates = list(all_branches)

        if not candidates:
            # No release branches at all — refuse to diff entire history
            raise RuntimeError(
                "No release branches found. Cannot determine diff range "
                "for main. Ensure release branches are fetched.")

        candidates.sort(key=release_branch_sort_key)
        latest = candidates[-1]
        return "origin/{}".format(latest), "origin/main", version

    # ── servicing branch (release/X.Y.x) ────────────────────────
    m_svc = re.match(r"release/(\d+\.\d+)\.x$", branch)
    if m_svc:
        minor = m_svc.group(1)

        # All versioned branches for this minor (exclude the .x branch itself)
        candidates = [b for b in all_branches
                      if b.startswith("release/{}.".format(minor))
                      and b != branch
                      and not b.endswith(".x")]

        if not candidates:
            base = run(["git", "merge-base",
                        "origin/{}".format(branch), "origin/main"])
            return base, "origin/{}".format(branch), "{}.0".format(minor)

        candidates.sort(key=release_branch_sort_key)
        latest = candidates[-1]

        # Determine next patch version from the latest branch
        # If latest is a preview (no stable yet for that patch), target that patch
        # If latest is a stable, target patch + 1
        latest_version = version_from_branch(latest)
        latest_patch = int(latest_version.split(".")[-1]) if latest_version else 0
        latest_is_stable = not re.search(r"-preview\.", latest)
        if latest_is_stable:
            next_version = "{}.{}".format(minor, latest_patch + 1)
        else:
            next_version = "{}.{}".format(minor, latest_patch)

        return ("origin/{}".format(latest),
                "origin/{}".format(branch),
                next_version)

    # ── versioned branch (release/X.Y.Z or release/X.Y.Z-preview.N) ─
    m_ver = re.match(r"release/(\d+\.\d+)\.\S+", branch)
    if not m_ver:
        raise RuntimeError("Cannot parse branch: {}".format(branch))
    minor = m_ver.group(1)
    version = version_from_branch(branch)

    # All same-minor branches, sorted by semver
    # Exclude .x servicing branches — they are not predecessors of versioned branches
    candidates = [b for b in all_branches
                  if b.startswith("release/{}.".format(minor))
                  and not b.endswith(".x")]
    candidates.sort(key=release_branch_sort_key)

    if branch not in candidates:
        raise RuntimeError(
            "Branch {} not found in remote branches. "
            "Available: {}".format(
                branch, ", ".join(candidates) or "(none)"))

    idx = candidates.index(branch)
    if idx > 0:
        previous = candidates[idx - 1]
        return ("origin/{}".format(previous),
                "origin/{}".format(branch),
                version)

    # No previous in same minor -- use merge-base with main
    base = run(["git", "merge-base",
                "origin/{}".format(branch), "origin/main"])
    return base, "origin/{}".format(branch), version


def get_prs_from_diff(from_ref, to_ref):
    # type: (str, str) -> list[dict]
    """Extract merged PRs from git log between two refs.

    Parses PR numbers from '(#NNN)' at end of commit subject lines,
    then fetches full PR metadata from GitHub.
    """
    log = run(["git", "log", "--oneline",
               "{}..{}".format(from_ref, to_ref)])

    pr_numbers = []  # type: list[int]
    seen = set()  # type: set[int]
    for line in log.splitlines():
        m = re.search(r"\(#(\d+)\)\s*$", line)
        if m:
            num = int(m.group(1))
            if num not in seen:
                seen.add(num)
                pr_numbers.append(num)

    if not pr_numbers:
        return []

    prs = []
    failures = 0
    total = len(pr_numbers)
    for i, num in enumerate(pr_numbers, 1):
        try:
            raw = gh(["pr", "view", str(num), "--repo", REPO,
                      "--json",
                      "title,author,url,number,labels,mergedAt,commits,body"])
            pr = json.loads(raw)
            pr.update(compute_pr_effort(pr))
            prs.append(pr)
        except (subprocess.CalledProcessError, json.JSONDecodeError):
            failures += 1
            print("  WARNING: Failed to fetch PR #{} ({}/{})".format(
                num, failures, total), file=sys.stderr)
            continue
        if i % 20 == 0:
            print("  Fetched {}/{} PRs...".format(i, total), file=sys.stderr)

    if failures > 0:
        print("  WARNING: {} of {} PRs could not be fetched".format(
            failures, total), file=sys.stderr)
        if failures > total // 2:
            print("ERROR: More than half of PRs failed to fetch. "
                  "GitHub API may be down.", file=sys.stderr)
            sys.exit(1)

    return prs


def format_pr_list(prs, metadata):
    # type: (list[dict], dict) -> str
    """Format the PR list as markdown with a YAML front-matter header."""
    lines = [
        "---",
        "branch: {}".format(metadata["branch"]),
        "version: {}".format(metadata["version"]),
        "status: {}".format(metadata["status"]),
        "diff: {}..{}".format(metadata["from"], metadata["to"]),
        "pr_count: {}".format(len(prs)),
        "---",
        "",
        "<!-- REPLACE THIS ENTIRE FILE with polished release notes.",
        "     Use documentation/docfx/releases/TEMPLATE.md as the formatting reference.",
        "     Remove this comment, the YAML header above, and the raw PR list below.",
        "     Write the final content starting with: # Version {} -->".format(
            metadata["version"]),
        "",
    ]

    if not prs:
        lines.append("*No changes found.*")
    else:
        for pr in prs:
            title = pr.get("title", "")
            author = (pr.get("author") or {}).get("login", "unknown")
            url = pr.get("url", "")
            label_names = [la.get("name", "") for la in pr.get("labels", [])]
            labels_str = " [{}]".format(", ".join(label_names)) if label_names else ""
            commits = pr.get("commitCount", 0)
            days = pr.get("workingDays", 0)
            effort = " ({} commit{}, {} day{})".format(
                commits, "s" if commits != 1 else "",
                days, "s" if days != 1 else "")
            skia_pr = pr.get("skiaPr")
            skia_str = " (skia: mono/skia#{})".format(skia_pr) if skia_pr else ""

            lines.append("- {} by @{} in {}{}{}{}".format(
                title, author, url, labels_str, effort, skia_str))

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
        run(["git", "fetch", "origin", "--quiet"], check=True)
    except subprocess.CalledProcessError:
        print("ERROR: git fetch failed. Cannot determine branch diff "
              "range with stale data.")
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
        try:
            releases_raw = gh(["release", "list", "--repo", REPO, "--limit", "50",
                               "--json", "tagName,isPrerelease"])
            releases = json.loads(releases_raw)
            has_stable = False
            has_preview = False
            for rel in releases:
                tag_base = rel["tagName"].lstrip("v").split("-")[0]
                if tag_base == version:
                    if rel.get("isPrerelease", False):
                        has_preview = True
                    else:
                        has_stable = True
            if has_stable:
                status = "stable"
            elif has_preview:
                status = "preview"
        except (subprocess.CalledProcessError, json.JSONDecodeError):
            pass

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

    # Regenerate TOC and index
    cmd_update_toc()


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
