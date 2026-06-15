#!/usr/bin/env python3
"""
Fetch SkiaSharp release data and manage the website release notes structure.

This script collects raw data. AI does the formatting using TEMPLATE.md.

Commands:
    # Diff a branch and write raw PR data to documentation/docfx/releases/{version}.md
    python3 generate-release-notes.py --branch main
    python3 generate-release-notes.py --branch release/3.119.x
    python3 generate-release-notes.py --branch release/4.147.0-preview.1

    # Process ALL branches (main + all release/*), skip unchanged files
    python3 generate-release-notes.py --all

    # Regenerate TOC.yml and index.md from files on disk + create upcoming version file
    python3 generate-release-notes.py --update-toc

The --branch command writes directly to documentation/docfx/releases/{version}.md
with a YAML front-matter header containing metadata (branch, version, status, diff
range, PR count) followed by the raw PR list. AI then rewrites this file with
polished content. TOC and index are regenerated automatically.

The --all command iterates every branch and only writes files whose PR count or
diff range has changed (idempotent). Use this for automated workflows.

Reads scripts/versions.json (if present) for comparison overrides and supersession
markers, falling back to auto-detection for versions not listed in the config.

Requirements: git, Python 3.7+
"""

from __future__ import annotations

import argparse
import json
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

# Versions config (loaded lazily from scripts/versions.json)
_VERSIONS_CONFIG = None  # type: Optional[list[dict]]

VERSIONS_JSON_PATH = Path("scripts/versions.json")


def load_versions_config():
    # type: () -> list[dict]
    """Load scripts/versions.json override config (cached)."""
    global _VERSIONS_CONFIG
    if _VERSIONS_CONFIG is not None:
        return _VERSIONS_CONFIG
    if VERSIONS_JSON_PATH.exists():
        with open(VERSIONS_JSON_PATH) as f:
            data = json.load(f)
        _VERSIONS_CONFIG = data.get("versions", [])
    else:
        _VERSIONS_CONFIG = []
    return _VERSIONS_CONFIG


def _versions_config_lookup(version):
    # type: (str) -> Optional[dict]
    """Find an entry in versions.json matching a base version (X.Y.Z)."""
    config = load_versions_config()
    for entry in config:
        if entry.get("version") == version:
            return entry
    return None


def _is_content_unchanged(output_path, new_prs_count, new_diff_range):
    # type: (Path, int, str) -> bool
    """Check if the existing file has the same PR count and diff range.

    Reads the metadata from the HTML comment block at the top of the file.
    Returns True if the content would be identical (skip writing).
    """
    if not output_path.exists():
        return False
    content = output_path.read_text()
    # Extract prs count
    m_prs = re.search(r"^\s*prs:\s*(\d+)", content, re.MULTILINE)
    # Extract diff range
    m_diff = re.search(r"^\s*diff:\s*(\S+)", content, re.MULTILINE)
    if not m_prs or not m_diff:
        return False
    existing_prs = int(m_prs.group(1))
    existing_diff = m_diff.group(1)
    return existing_prs == new_prs_count and existing_diff == new_diff_range


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


def _version_has_stable_tag(version):
    # type: (str) -> bool
    """True if a stable git tag (vX.Y.Z, no -preview/-rc) exists for version."""
    tags = run(["git", "tag", "-l", "v{}*".format(version)], check=False)
    for tag in tags.splitlines():
        tag = tag.strip()
        if not tag:
            continue
        if "-preview" in tag or "-rc" in tag:
            continue
        # Match vX.Y.Z exactly or with a trailing build suffix (vX.Y.Z.N)
        rest = tag[len("v" + version):] if tag.startswith("v" + version) else None
        if rest is not None and (rest == "" or rest.startswith(".")):
            return True
    return False


def _all_known_base_versions(all_branches):
    # type: (list[str]) -> set[str]
    """Collect every base version (X.Y.Z) known from release branches and tags."""
    versions = set()
    for b in all_branches:
        if b.endswith(".x"):
            continue
        versions.add(version_from_branch(b))
    tags = run(["git", "tag", "-l", "v*"], check=False)
    for tag in tags.splitlines():
        tag = tag.strip()
        if tag.startswith("v"):
            versions.add(extract_base_version(tag))
    return versions


def _main_upcoming_version():
    # type: () -> Optional[str]
    """Main's in-development version (origin/main, falling back to local)."""
    return get_version_from_remote_branch("main") or get_upcoming_version()


def detect_superseded_by(version, all_branches):
    # type: (str, list[str]) -> Optional[str]
    """Auto-detect whether a preview-only version was skipped for a later one.

    A version is "superseded" when it never shipped a stable tag AND a strictly
    later version exists. Returns the smallest later base version, preferring
    one that actually shipped stable. This covers a skipped minor
    (4.147.0 -> 4.148.0), an abandoned patch preview (3.119.3 -> 3.119.4), and
    a minor skipped on main itself: when main's in-development version is newer
    and the checked version's minor line was never branched for servicing
    (no release/X.Y.x), main's version supersedes it — e.g. main on 4.148.0
    with only release/4.147.0-preview.* and no release/4.147.x.
    Returns None while the version is still the latest line (just a preview).
    """
    if _version_has_stable_tag(version):
        return None
    vkey = version_key(version)
    later = {v for v in _all_known_base_versions(all_branches)
             if version_key(v) > vkey}

    # Main's in-development version supersedes a preview-only version when that
    # version's minor line was never branched for servicing (it was skipped).
    main_version = _main_upcoming_version()
    if main_version and version_key(main_version) > vkey:
        servicing = "release/{}.x".format(minor_group(version))
        if servicing not in all_branches:
            later.add(main_version)

    if not later:
        return None
    stable_later = [v for v in later if _version_has_stable_tag(v)]
    pool = sorted(stable_later or later, key=version_key)
    return pool[0]


def resolve_superseded_by(version, all_branches):
    # type: (str, list[str]) -> Optional[str]
    """Whether a version was superseded. Checks versions.json first, then auto-detects."""
    entry = _versions_config_lookup(version)
    if entry:
        if entry.get("status") == "superseded" and entry.get("superseded_by"):
            return entry["superseded_by"]
        # If config explicitly lists this version without superseded status, not superseded
        if entry.get("status") and entry["status"] != "superseded":
            return None
    return detect_superseded_by(version, all_branches)


def detect_supersedes(version, all_branches):
    # type: (str, list[str]) -> list[str]
    """Inverse of detect_superseded_by: the versions this release rolls up.

    Returns every earlier base version whose auto-detected successor is exactly
    `version` — i.e. the preview-only / skipped versions that were superseded by
    this one and whose work is rolled up cumulatively. This is the back-link that
    makes the supersede relationship two-way: the superseded page already points
    forward ("Superseded by X"), and this lets the successor page point back
    ("Supersedes Y"). Covers a skipped minor (4.148.0 supersedes 4.147.0) and an
    abandoned patch preview (3.119.4 supersedes 3.119.3). Returns [] when this
    version supersedes nothing.
    """
    vkey = version_key(version)
    rolled = []
    for cand in _all_known_base_versions(all_branches):
        if cand == version or version_key(cand) >= vkey:
            continue
        if resolve_superseded_by(cand, all_branches) == version:
            rolled.append(cand)
    rolled.sort(key=version_key)
    return rolled


def _is_valid_stable_base(branch):
    # type: (str) -> bool
    """True if a release branch may serve as a cumulative "previous stable" base.

    A branch is rejected when:
      1. versions.json explicitly marks its version as superseded (the config is
         the authoritative override — e.g. 4.147 → never a base for 4.148), or
      2. its version never shipped a stable git tag (the automatic fallback for
         versions not listed in the config).

    This makes cross-minor rollups reach back to the last version that actually
    released as stable, so preview-only/skipped minors are excluded.
    """
    version = version_from_branch(branch)
    entry = _versions_config_lookup(version)
    if entry and entry.get("status") == "superseded":
        return False
    return _version_has_stable_tag(version)


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
    1. If Z > 0: use the most recent previous patch that shipped stable,
       skipping preview-only / superseded patches (cumulative rollup).
    2. If Z == 0: look for the latest branch from a previous minor/major that
       actually shipped as stable, skipping any minor that was superseded or
       never produced a stable tag (so its work rolls up into this version).

    Falls back to None if nothing found.
    """
    minor = "{}.{}".format(major, minor_num)

    # Case 1: Z > 0 — the cumulative base is the most recent PREVIOUS patch
    # that actually shipped as stable. Preview-only / superseded patches are
    # skipped so the next stable rolls up their work (e.g. 3.119.4 bases on
    # 3.119.2, absorbing the preview-only 3.119.3). Falls through to Case 2
    # (previous minor) when no previous patch shipped stable.
    if patch > 0:
        for p in range(patch - 1, -1, -1):
            prev_version = "{}.{}".format(minor, p)
            # A stable release/X.Y.Z branch (exact, no -preview) or a stable
            # tag both signal that this patch shipped (or is shipping) stable.
            prev_stable = "release/{}".format(prev_version)
            has_stable_branch = prev_stable in all_branches
            if not has_stable_branch and not _version_has_stable_tag(prev_version):
                continue
            if has_stable_branch:
                return prev_stable
            # Stable tag but no exact branch — use its latest preview branch.
            prev_candidates = [b for b in all_branches
                               if b.startswith("release/{}-preview.".format(
                                   prev_version))]
            if prev_candidates:
                prev_candidates.sort(key=release_branch_sort_key)
                return prev_candidates[-1]
        # No previous patch shipped stable — fall through to Case 2.

    # Case 2: Z == 0 (or no previous patch found) — search previous minors
    all_versioned = [b for b in all_branches if not b.endswith(".x")]
    all_versioned.sort(key=release_branch_sort_key)

    # Find the latest branch that sorts before our minor
    target_key = (major, minor_num, -2, 0, 0)  # before even .x
    candidates = [b for b in all_versioned
                  if release_branch_sort_key(b) < target_key]
    if candidates:
        # Prefer the latest branch that genuinely shipped as stable, so a
        # skipped/preview-only minor (e.g. 4.147) does not become the base
        # and its work is rolled up into this version instead.
        stable = [b for b in candidates
                  if _is_valid_stable_base(b)]
        if stable:
            return stable[-1]
        # No stable predecessor found — fall back to the latest candidate.
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

        # Check versions.json for explicit compare_to override for main's version
        config_entry = _versions_config_lookup(version)
        if config_entry and config_entry.get("compare_to"):
            compare_to = config_entry["compare_to"]
            compare_branches = [b for b in all_branches
                                if b.startswith("release/{}".format(compare_to))
                                and not b.endswith(".x")]
            if compare_branches:
                compare_branches.sort(key=release_branch_sort_key)
                return ("origin/{}".format(compare_branches[-1]),
                        "origin/main", version)

        # Find latest release branch for the same minor. These are the active
        # line for the upcoming version, so the latest one is always the base
        # (main shows work beyond the most recent preview cut).
        same_minor = [b for b in all_branches
                      if b.startswith("release/{}.".format(minor))
                      and not b.endswith(".x")]

        if same_minor:
            same_minor.sort(key=release_branch_sort_key)
            return ("origin/{}".format(same_minor[-1]),
                    "origin/main", version)

        # No branch yet for the upcoming minor — fall back across ALL minors,
        # preferring the latest that genuinely shipped as stable so a skipped
        # / preview-only minor does not shrink main's rollup.
        all_versioned = [b for b in all_branches if not b.endswith(".x")]
        all_versioned.sort(key=release_branch_sort_key)

        if not all_versioned:
            raise RuntimeError(
                "No release branches found. Cannot determine diff range "
                "for main. Ensure release branches are fetched.")

        stable = [b for b in all_versioned
                  if _is_valid_stable_base(b)]
        if stable:
            latest = stable[-1]
        else:
            latest = all_versioned[-1]
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

    # Check versions.json for explicit compare_to override
    config_entry = _versions_config_lookup(version)
    if config_entry and config_entry.get("compare_to"):
        compare_to = config_entry["compare_to"]
        # Find the best matching release branch for the compare_to version
        compare_branches = [b for b in all_branches
                            if b.startswith("release/{}".format(compare_to))
                            and not b.endswith(".x")]
        if compare_branches:
            compare_branches.sort(key=release_branch_sort_key)
            return ("origin/{}".format(compare_branches[-1]),
                    "origin/{}".format(branch),
                    version)
        # No branch — try stable branch directly
        exact_branch = "release/{}".format(compare_to)
        if exact_branch in all_branches:
            return ("origin/{}".format(exact_branch),
                    "origin/{}".format(branch),
                    version)
        # Last fallback for config override: look for a tag
        tag_sha = run(["git", "rev-parse", "v{}".format(compare_to)],
                      check=False).strip()
        if tag_sha:
            return (tag_sha, "origin/{}".format(branch), version)

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
    """Format the PR list as markdown with raw data in an HTML comment.

    The raw PR data is preserved inside an HTML comment block so that AI
    can regenerate polished notes from it at any time. Below the comment,
    a skeleton placeholder is written for AI to fill in.
    """
    now = datetime.now(timezone.utc).strftime("%Y-%m-%dT%H:%M:%SZ")
    version = metadata["version"]

    # Build the raw data comment block
    lines = [
        "<!--",
        "  RAW PR DATA — Do not remove this comment block.",
        "  AI uses this data to generate the polished release notes below.",
        "  Re-run the script to refresh this data from git history.",
        "",
        "  Generated: {} by generate-release-notes.py".format(now),
        "  version:   {}".format(version),
        "  status:    {}".format(metadata["status"]),
        "  branch:    {}".format(metadata["branch"]),
        "  diff:      {}..{}".format(metadata["from"], metadata["to"]),
        "  prs:       {}".format(len(prs)),
        "",
    ]

    superseded_by = metadata.get("superseded_by")
    supersedes = metadata.get("supersedes")
    meta_extra = []
    if superseded_by:
        meta_extra.append(
            "  superseded: {} (preview only, never released as stable)".format(
                superseded_by))
    if supersedes:
        meta_extra.append(
            "  supersedes: {} (preview only, rolled up)".format(
                ", ".join(supersedes)))
    # Insert right after the status line for visibility.
    for i, extra in enumerate(meta_extra):
        lines.insert(8 + i, extra)

    if not prs:
        lines.append("  *No changes found.*")
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

            lines.append("  - {} by @{} in {}{}{}{}".format(
                title, author, url, community_str, effort, skia_str))

    lines.append("-->")
    lines.append("")

    # Add skeleton content for AI to polish
    lines.append("# Version {}".format(version))
    lines.append("")

    # Status-appropriate header
    status = metadata["status"]
    if superseded_by:
        if (RELEASES_DIR / "{}.md".format(superseded_by)).exists():
            sup_link = "[{sup}]({sup}.md)".format(sup=superseded_by)
        elif (RELEASES_DIR / "{}-unreleased.md".format(superseded_by)).exists():
            sup_link = "[{sup}]({sup}-unreleased.md)".format(sup=superseded_by)
        else:
            sup_link = superseded_by
        lines.append(
            "> **Preview only** · Superseded by "
            "{sup_link} · Never released as stable — these changes "
            "rolled up into {sup} "
            "· [NuGet](https://www.nuget.org/packages/SkiaSharp/"
            "{ver}-preview)".format(sup_link=sup_link, sup=superseded_by, ver=version))
    elif status == "unreleased":
        lines.append(
            "> **Upcoming release** · In development "
            "· Not yet available on NuGet")
    elif status == "preview":
        lines.append(
            "> **Preview release** · Preview only "
            "· [NuGet](https://www.nuget.org/packages/SkiaSharp/"
            "{}-preview)".format(version))
    else:
        lines.append(
            "> [NuGet](https://www.nuget.org/packages/SkiaSharp/{})".format(
                version))
    lines.append("")

    # Back-link making the supersede relationship two-way: this release rolls up
    # one or more preview-only versions that never shipped stable. The superseded
    # page already links forward ("Superseded by X"); this points back to it.
    if supersedes:
        sup_links = []
        for sup in supersedes:
            if (RELEASES_DIR / "{}.md".format(sup)).exists():
                sup_links.append("[{s}]({s}.md)".format(s=sup))
            elif (RELEASES_DIR / "{}-unreleased.md".format(sup)).exists():
                sup_links.append("[{s}]({s}-unreleased.md)".format(s=sup))
            else:
                sup_links.append(sup)
        lines.append(
            "> **Supersedes {}** · Rolls up preview-only work that was never "
            "released as stable — those changes are included cumulatively "
            "below.".format(", ".join(sup_links)))
        lines.append("")

    lines.append(
        "<!-- AI: Use the raw PR data in the comment above to write polished")
    lines.append(
        "     release notes here. Follow documentation/docfx/releases/"
        "TEMPLATE.md")
    if supersedes:
        lines.append("     for structure and tone.")
        lines.append(
            "     This release SUPERSEDES {} (preview-only, rolled up). Keep "
            "the \"Supersedes\" note above and mention in the Highlights that "
            "this release rolls up that skipped preview work cumulatively. -->"
            .format(", ".join(supersedes)))
    else:
        lines.append("     for structure and tone. -->")
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


def cleanup_stale_unreleased():
    # type: () -> list[str]
    """Delete {version}-unreleased.md files whose stable {version}.md exists.

    Once a version has a published page, its in-progress "unreleased" page is
    obsolete: the servicing line has moved on to the next patch (e.g. once
    3.119.4.md ships, 3.119.4-unreleased.md is stale and the line tracks
    3.119.5-unreleased.md instead). Returns the removed file paths.
    """
    removed = []
    for f in sorted(RELEASES_DIR.iterdir()):
        if f.suffix != ".md" or not f.stem.endswith("-unreleased"):
            continue
        version = f.stem[:-11]  # strip "-unreleased"
        if (RELEASES_DIR / "{}.md".format(version)).exists():
            f.unlink()
            removed.append(str(f))
    return removed


def generate_toc(versions, next_versions):
    # type: (list[str], list[str]) -> str
    """Generate TOC.yml grouped by major.minor, obsolete under one node.

    Unreleased pages are listed in their minor group even when no stable page
    of that exact version exists yet (e.g. 3.119.5-unreleased before 3.119.5
    ships, or 4.148.0-unreleased before 4.148.0 ships).
    """
    stable_groups = defaultdict(list)
    unreleased_groups = defaultdict(list)
    for v in versions:
        stable_groups[minor_group(v)].append(v)
    for v in next_versions:
        unreleased_groups[minor_group(v)].append(v)

    current = []
    obsolete = []
    for g in sorted(set(stable_groups) | set(unreleased_groups),
                    key=lambda x: version_key(x), reverse=True):
        if int(g.split(".")[0]) < 3:
            obsolete.append(g)
        else:
            current.append(g)

    lines = ["- name: Overview", "  href: index.md"]

    for g in current:
        stable = stable_groups.get(g, [])
        unreleased = unreleased_groups.get(g, [])
        header = "{}.md".format(stable[0]) if stable \
            else "{}-unreleased.md".format(unreleased[0])
        lines.append("- name: Version {}.x".format(g))
        lines.append("  href: {}".format(header))
        lines.append("  items:")
        entries = [(v, True) for v in unreleased] + [(v, False) for v in stable]
        entries.sort(key=lambda t: version_key(t[0]), reverse=True)
        for v, is_unrel in entries:
            if is_unrel:
                lines.append("    - name: Version {} (Unreleased)".format(v))
                lines.append("      href: {}-unreleased.md".format(v))
            else:
                lines.append("    - name: Version {}".format(v))
                lines.append("      href: {}.md".format(v))

    if obsolete:
        first = stable_groups.get(obsolete[0]) or unreleased_groups.get(obsolete[0])
        lines.append("- name: Obsolete Versions")
        lines.append("  href: {}.md".format(first[0]))
        lines.append("  items:")
        for g in obsolete:
            members = stable_groups.get(g, [])
            if not members:
                continue
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
    """Generate index.md with version list grouped by major.

    Unreleased pages are listed even when no stable page of that exact version
    exists yet.
    """
    lines = [
        "# Release Notes",
        "",
        "Release notes for all SkiaSharp versions.",
        "",
    ]

    entries = [(v, False) for v in versions] + [(v, True) for v in next_versions]

    major_groups = defaultdict(list)
    for v, is_unrel in entries:
        major_groups[v.split(".")[0]].append((v, is_unrel))

    for major in sorted(major_groups.keys(), key=int, reverse=True):
        lines.extend(["### SkiaSharp {}.x".format(major), ""])

        minor_groups_map = defaultdict(list)
        for v, is_unrel in major_groups[major]:
            minor_groups_map[minor_group(v)].append((v, is_unrel))

        for g in sorted(minor_groups_map.keys(),
                        key=lambda x: version_key(x), reverse=True):
            members = sorted(minor_groups_map[g],
                             key=lambda t: version_key(t[0]), reverse=True)
            lines.append("- **Version {}.x**".format(g))
            for v, is_unrel in members:
                if is_unrel:
                    lines.append("  - [Version {} (Unreleased)]({}-unreleased.md)".format(v, v))
                else:
                    lines.append("  - [Version {}]({}.md)".format(v, v))
        lines.append("")

    return "\n".join(lines)


# ── Commands ─────────────────────────────────────────────────────────


def cmd_update_toc():
    """Regenerate TOC.yml and index.md (and prune stale unreleased pages)."""
    if not RELEASES_DIR.is_dir():
        print("Error: {} does not exist".format(RELEASES_DIR), file=sys.stderr)
        sys.exit(1)

    for removed in cleanup_stale_unreleased():
        print("Removed stale {}".format(removed))

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

    # Supersede marker: a minor that never ships stable (e.g. 4.147.0 skipped
    # for 4.148.0). Auto-detected once a newer minor line appears. Forces
    # "preview" status and a "superseded" label so the page makes clear it
    # never released as stable.
    superseded_by = None
    if not is_main and not is_servicing:
        superseded_by = resolve_superseded_by(version, all_branches)
        if superseded_by:
            status = "preview"

    # Inverse relationship (two-way link): the preview-only versions that this
    # release supersedes / rolls up. Applies to any successor — main, a
    # servicing .x line, or a stable release branch.
    supersedes = detect_supersedes(version, all_branches)

    print("Branch: {}".format(branch))
    print("Version: {}".format(version))
    print("Status: {}".format(status))
    if superseded_by:
        print("Superseded by: {}".format(superseded_by))
    if supersedes:
        print("Supersedes: {}".format(", ".join(supersedes)))
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
    if superseded_by:
        metadata["superseded_by"] = superseded_by
    if supersedes:
        metadata["supersedes"] = supersedes

    output_path = RELEASES_DIR / filename
    RELEASES_DIR.mkdir(parents=True, exist_ok=True)

    # Skip writing if the existing file already has the same data
    diff_range_str = "{}..{}".format(from_display, to_display)
    if _is_content_unchanged(output_path, len(prs), diff_range_str):
        print("Skipping {} (unchanged: {} PRs, diff {})".format(
            output_path, len(prs), diff_range_str))
        files_to_polish = []
    else:
        content = format_pr_list(prs, metadata)
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


def cmd_all():
    # type: () -> None
    """Process all branches (main + all release/*). Skip unchanged files.

    This is the idempotent "regenerate everything" mode used by the automated
    workflow. It iterates over every known branch and regenerates its raw PR
    data, but only WRITES files whose content actually changed (same PR count
    AND same diff range == no write). The "Files to polish" output therefore
    only lists files that genuinely changed, so the AI never re-polishes pages
    that are already up to date.

    Superseded versions (e.g. 4.147, which was skipped for 4.148) are still
    generated — they keep their own page with a "superseded by" label. The
    supersede marker only affects which version is used as a *baseline* when
    diffing others (handled in determine_diff_range / _is_valid_stable_base),
    never whether a page is produced.
    """
    print("Fetching remote branches...")
    try:
        run(["git", "fetch", "origin", "--unshallow", "--quiet"], check=False)
        run(["git", "fetch", "origin",
             "refs/heads/release/*:refs/remotes/origin/release/*",
             "refs/heads/main:refs/remotes/origin/main",
             "--quiet"], check=True)
    except subprocess.CalledProcessError:
        print("ERROR: git fetch failed.")
        sys.exit(1)

    all_branches = list_remote_release_branches()
    if not all_branches:
        print("ERROR: No release branches found after fetch.")
        sys.exit(1)

    # Process main plus every release branch. Superseded versions are NOT
    # filtered out here — they still get a page (see docstring).
    branches_to_process = ["main"] + all_branches

    files_to_polish = []
    skipped_count = 0
    processed_count = 0

    for branch in branches_to_process:
        if branch == "main":
            version = get_upcoming_version()
            if not version:
                print("WARNING: Cannot determine main version, skipping main.")
                continue
        else:
            version = version_from_branch(branch)

        print("\n--- Processing: {} (version: {}) ---".format(branch, version))
        try:
            from_ref, to_ref, ver = determine_diff_range(branch)
        except (RuntimeError, subprocess.CalledProcessError) as e:
            print("  WARNING: Could not determine diff range: {}".format(e))
            continue

        from_display = _removeprefix(from_ref, "origin/")
        to_display = _removeprefix(to_ref, "origin/")
        if re.match(r"^[0-9a-f]{7,}$", from_display):
            from_display = from_display[:12]

        # Determine output filename
        is_main_branch = (branch == "main")
        is_servicing = branch.endswith(".x")
        if is_main_branch or is_servicing:
            filename = "{}-unreleased.md".format(ver)
        else:
            filename = "{}.md".format(ver)

        output_path = RELEASES_DIR / filename

        # Determine status
        status = "unreleased"
        if not is_main_branch and not is_servicing:
            tags = run(["git", "tag", "-l", "v{}*".format(ver)], check=False)
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

        # Get PRs
        prs = get_prs_from_diff(from_ref, to_ref)
        print("  Found {} PR(s), diff: {}..{}".format(
            len(prs), from_display, to_display))

        # Skip-unchanged check
        diff_range_str = "{}..{}".format(from_display, to_display)
        if _is_content_unchanged(output_path, len(prs), diff_range_str):
            print("  Skipping {} (unchanged)".format(output_path))
            skipped_count += 1
            continue

        # Supersede markers
        superseded_by = None
        if not is_main_branch and not is_servicing:
            superseded_by = resolve_superseded_by(ver, all_branches)
            if superseded_by:
                status = "preview"
        supersedes = detect_supersedes(ver, all_branches)

        metadata = {
            "branch": branch,
            "version": ver,
            "status": status,
            "from": from_display,
            "to": to_display,
        }
        if superseded_by:
            metadata["superseded_by"] = superseded_by
        if supersedes:
            metadata["supersedes"] = supersedes

        content = format_pr_list(prs, metadata)
        RELEASES_DIR.mkdir(parents=True, exist_ok=True)
        output_path.write_text(content)
        print("  Wrote {}".format(output_path))
        files_to_polish.append(str(output_path))
        processed_count += 1

    # Regenerate TOC and index
    cmd_update_toc()

    # Print summary for the AI agent
    print("")
    print("========================================")
    print("Processed: {}, Skipped (unchanged): {}".format(
        processed_count, skipped_count))
    print("Files to polish:")
    if files_to_polish:
        for f in files_to_polish:
            print("  - {}".format(f))
    else:
        print("  (none — all files up to date)")
    print("========================================")


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
            "  %(prog)s --all                               "
            "Process all branches (skip unchanged)\n"
            "  %(prog)s --update-toc                        "
            "Regenerate TOC + index\n"
        ),
    )
    parser.add_argument(
        "--branch",
        help="Diff branch against its predecessor and write raw PR data")
    parser.add_argument(
        "--all", action="store_true",
        help="Process all branches (main + release/*), skip unchanged files")
    parser.add_argument(
        "--update-toc", action="store_true",
        help="Regenerate TOC.yml + index.md")

    args = parser.parse_args()

    if not args.branch and not args.update_toc and not args.all:
        parser.print_help()
        sys.exit(1)

    num_modes = sum([bool(args.branch), args.update_toc, args.all])
    if num_modes > 1:
        parser.error("Specify only one of --branch, --all, or --update-toc")

    if args.update_toc:
        cmd_update_toc()
    elif args.all:
        cmd_all()
    elif args.branch:
        cmd_branch(args.branch)


if __name__ == "__main__":
    main()
