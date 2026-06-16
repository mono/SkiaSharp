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

Reads scripts/versions.json (if present) for comparison overrides and
supersession markers. versions.json is the single source of truth: only the
versions listed there get a non-default baseline or a superseded marker.

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


def _is_content_unchanged(output_path, new_prs_count, new_diff_range,
                          new_status=None, new_superseded_by=None,
                          new_supersedes=None):
    # type: (Path, int, str, Optional[str], Optional[str], Optional[list[str]]) -> bool
    """Check whether the existing file already encodes identical raw data.

    Compares the fields the script controls: PR count, diff range, and the
    supersession metadata (status + the ``superseded:`` / ``supersedes:``
    markers). The supersession fields must be part of this check because
    toggling a version's supersession in versions.json can change ONLY the page
    banner without touching the diff range — when a preview line is newly marked
    superseded its OWN diff/PR set is unchanged (supersession only affects it as
    a baseline for LATER versions), but its page must still gain a "Superseded
    by" banner. Returns True only when every tracked field matches, so those
    metadata-only changes still force a rewrite.

    Reads the metadata from the HTML comment block at the top of the file.
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
    if int(m_prs.group(1)) != new_prs_count or m_diff.group(1) != new_diff_range:
        return False

    # status: only compare when the caller supplies the new value.
    if new_status is not None:
        m_status = re.search(r"^\s*status:\s*(\S+)", content, re.MULTILINE)
        existing_status = m_status.group(1) if m_status else None
        if existing_status != new_status:
            return False

    # superseded: <version> (...) — the marker carries the successor version as
    # its first token; absent line means "not superseded".
    m_sup_by = re.search(r"^\s*superseded:\s*(\S+)", content, re.MULTILINE)
    existing_superseded_by = m_sup_by.group(1) if m_sup_by else None
    if (new_superseded_by or None) != existing_superseded_by:
        return False

    # supersedes: <v1>, <v2> (...) — compare the set of rolled-up versions.
    m_supersedes = re.search(r"^\s*supersedes:\s*([^(\n]+)", content, re.MULTILINE)
    existing_supersedes = (
        [s.strip() for s in m_supersedes.group(1).split(",") if s.strip()]
        if m_supersedes else [])
    if sorted(new_supersedes or []) != sorted(existing_supersedes):
        return False

    return True


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


def resolve_superseded_by(version):
    # type: (str) -> Optional[str]
    """Return the version that supersedes ``version``, or None.

    versions.json is the SINGLE source of truth: a version is superseded only
    when its config entry sets ``status: superseded``, and the successor is the
    configured ``superseded_by``. There is no auto-detection, so the superseded
    set is identical to Cake's ``IsVersionSuperseded`` (which likewise treats
    only an explicit ``status: superseded`` entry as superseded). Every other
    version — listed with only ``compare_to`` or not listed at all — is a real
    release and is never reported as superseded. To skip a version everywhere,
    add it to versions.json.
    """
    entry = _versions_config_lookup(version)
    if entry and entry.get("status") == "superseded":
        return entry.get("superseded_by")
    return None


def detect_supersedes(version):
    # type: (str) -> list[str]
    """Return the versions that ``version`` rolls up (the inverse link).

    Reads versions.json directly: every entry marked ``status: superseded``
    whose ``superseded_by`` is exactly ``version``. This is the back-link that
    makes the supersede relationship two-way — the superseded page points
    forward ("Superseded by X"), and this lets the successor page point back
    ("Supersedes Y"). Covers a skipped minor (4.148.0 supersedes 4.147.0) and an
    abandoned patch preview (3.119.4 supersedes 3.119.3). Returns [] when this
    version supersedes nothing.
    """
    rolled = [entry["version"] for entry in load_versions_config()
              if entry.get("status") == "superseded"
              and entry.get("superseded_by") == version
              and entry.get("version")]
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
    # type: (str) -> Optional[str]
    """Extract a GitHub login from a commit email — ONLY when it is certain.

    GitHub usernames are not stored in git history. The one exception is the
    privacy "noreply" address, which embeds the real login:

        12345+username@users.noreply.github.com  ->  username

    For that format we return the login with full confidence. For every other
    address (corporate or personal email) the part before ``@`` is NOT a GitHub
    handle — guessing it would mis-credit the contributor or, worse, turn into an
    ``@mention`` that pings an unrelated real user (``jon``, ``martin``, ``i`` …).
    So we return None and let the caller resolve the true login from the GitHub
    API instead (see resolve_pr_authors).
    """
    m = _NOREPLY_RE.match(email)
    if m:
        return m.group(1)
    return None


# ── GitHub author resolution ─────────────────────────────────────────

_AUTHOR_CACHE_PATH = Path("scripts/pr-authors.json")
_GRAPHQL_BATCH = 50


def load_author_cache():
    # type: () -> dict
    """Load the PR-number -> GitHub-login cache (scripts/pr-authors.json).

    This cache is the durable record of every author resolved from the GitHub
    API, so ordinary regenerations stay fully offline: a warm cache means
    resolve_pr_authors makes zero network calls. A null value records that
    GitHub could not map the PR to an account (e.g. a deleted user) so we do not
    re-query a known-unresolvable PR on every run. Delete an entry to force it to
    be looked up again (useful once a contributor registers their commit email).
    """
    try:
        return json.loads(_AUTHOR_CACHE_PATH.read_text())
    except (OSError, ValueError):
        return {}


def save_author_cache(cache):
    # type: (dict) -> None
    """Persist the PR-number -> login cache, sorted by PR number for stable diffs."""
    try:
        ordered = {k: cache[k] for k in sorted(cache, key=lambda n: int(n))}
    except ValueError:
        ordered = cache
    _AUTHOR_CACHE_PATH.parent.mkdir(parents=True, exist_ok=True)
    _AUTHOR_CACHE_PATH.write_text(json.dumps(ordered, indent=2) + "\n")


def _graphql_pr_authors(numbers):
    # type: (list[int]) -> dict
    """Resolve PR-author logins via one batched GitHub GraphQL query.

    Returns ``{pr_number: login_or_None}`` for every number the API answered
    for. PRs that error or are missing from the response are omitted, so the
    caller never caches a transient failure and can retry later. Requires the
    ``gh`` CLI to be installed and authenticated (the workflow already provides
    ``GITHUB_TOKEN``); any failure yields an empty dict and callers fall back to
    the plain author name.
    """
    owner, name = REPO.split("/")
    aliases = "\n".join(
        "p{n}: pullRequest(number: {n}) {{ author {{ login }} }}".format(n=n)
        for n in numbers)
    query = 'query {{ repository(owner: "{}", name: "{}") {{\n{}\n}} }}'.format(
        owner, name, aliases)
    try:
        # check=False: a single missing/blocked PR makes gh exit non-zero, but
        # it still prints valid ``data`` for every PR it *could* resolve. Read
        # that stdout rather than discarding the whole batch on a partial error.
        out = run(["gh", "api", "graphql", "-f", "query=" + query], check=False)
    except FileNotFoundError:
        return {}  # gh not installed
    try:
        repo = json.loads(out)["data"]["repository"]
    except (ValueError, KeyError, TypeError):
        return {}
    resolved = {}  # type: dict
    for n in numbers:
        node = repo.get("p{}".format(n))
        if node is None:
            continue  # PR not found / errored — leave uncached for retry
        author = node.get("author")
        resolved[n] = author.get("login") if author else None
    return resolved


def resolve_pr_authors(prs):
    # type: (list[dict]) -> list[dict]
    """Fill in trustworthy GitHub logins for PRs that lack a noreply login.

    Confidence order:
      1. noreply login — already set by get_prs_from_diff, always correct.
      2. cache (scripts/pr-authors.json) — previously resolved from the API.
      3. GitHub GraphQL — the authoritative PR author, batched then cached.

    PRs that still cannot be resolved keep ``login=None``; format_pr_list then
    credits the plain commit name with no ``@mention``, so the notes never link
    or ping the wrong person. Mutates and returns ``prs``.
    """
    need = [pr for pr in prs if not (pr.get("author") or {}).get("login")]
    if not need:
        return prs

    cache = load_author_cache()
    to_query = sorted({pr["number"] for pr in need
                       if str(pr["number"]) not in cache})

    if to_query:
        print("  Resolving {} PR author(s) via GitHub API...".format(
            len(to_query)), file=sys.stderr)
        dirty = False
        for i in range(0, len(to_query), _GRAPHQL_BATCH):
            resolved = _graphql_pr_authors(to_query[i:i + _GRAPHQL_BATCH])
            for num, login in resolved.items():
                cache[str(num)] = login
                dirty = True
        if dirty:
            save_author_cache(cache)

    for pr in need:
        login = cache.get(str(pr["number"]))
        if login:
            pr["author"]["login"] = login
    return prs


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

    Uses refs/pull/N/head to get the base..head range for commit counting.
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


# Prerelease stage ranks. Lower sorts earlier; every prerelease ranks below a
# stable release of the same patch (which uses STABLE_STAGE). Unknown labels
# rank just below stable so a recognised-but-unmapped prerelease still sorts as
# a prerelease, never as stable.
_PRERELEASE_STAGE = {"alpha": 0, "beta": 1, "preview": 2, "rc": 3}
_UNKNOWN_STAGE = 8
_STABLE_STAGE = 9


def release_branch_sort_key(branch):
    # type: (str) -> tuple
    """Compute a semver-ish sort key for a release branch (ascending).

    Order within a minor:
      release/X.Y.x                  -> base/servicing, sorts before all patches
      release/X.Y.Z[.W]-<pre>.<n>    -> prereleases: alpha < beta < preview < rc
      release/X.Y.Z[.W]              -> stable, sorts after every prerelease

    Robust to the shapes that actually occur in this repo: dotted and undotted
    prerelease labels (``preview.3``, ``preview28``, ``rc.1``, ``rc.147``),
    multi-segment versions (``1.68.1.1``) and unrecognised labels. Anything
    unparseable sorts to the very front (it can never be selected as "latest").

    The tuple is (major, minor, patch, subpatch, stage, prenum); keep it the
    same length everywhere so keys are always mutually comparable.
    """
    name = _removeprefix(branch, "release/")
    core, _, label = name.partition("-")  # label == "" when there is no '-'
    parts = core.split(".")
    if len(parts) < 2 or not parts[0].isdigit() or not parts[1].isdigit():
        return (-1, 0, 0, 0, 0, 0)

    major = int(parts[0])
    minor_num = int(parts[1])
    third = parts[2] if len(parts) > 2 else "0"

    if third == "x":
        # Servicing base sorts before every concrete patch of the same minor.
        return (major, minor_num, -1, 0, 0, 0)
    if not third.isdigit():
        return (-1, 0, 0, 0, 0, 0)

    patch = int(third)
    subpatch = int(parts[3]) if len(parts) > 3 and parts[3].isdigit() else 0

    if not label:
        return (major, minor_num, patch, subpatch, _STABLE_STAGE, 0)

    name_m = re.match(r"[A-Za-z]+", label)
    stage_name = name_m.group(0).lower() if name_m else ""
    nums = re.findall(r"\d+", label)
    prenum = int(nums[0]) if nums else 0
    stage = _PRERELEASE_STAGE.get(stage_name, _UNKNOWN_STAGE)
    return (major, minor_num, patch, subpatch, stage, prenum)


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


def find_previous_stable_base(all_branches, major, minor_num, patch, subpatch=0):
    # type: (list[str], int, int, int, int) -> Optional[str]
    """Find the previous stable release branch to use as the cumulative diff base.

    For version X.Y.Z[.W]:
    0. If W > 0 (a 4-segment build such as 1.68.1.1): use the previous build in
       the same patch line, walking down to the plain X.Y.Z release.
    1. If Z > 0: use the most recent previous patch that shipped stable,
       skipping preview-only / superseded patches (cumulative rollup).
    2. If Z == 0: look for the latest branch from a previous minor/major that
       actually shipped as stable, skipping any minor that was superseded or
       never produced a stable tag (so its work rolls up into this version).

    Falls back to None if nothing found.
    """
    minor = "{}.{}".format(major, minor_num)

    # Case 0: W > 0 — a 4-segment build (e.g. 1.68.1.1). The cumulative base is
    # the previous build in the same patch line (1.68.1.1 -> 1.68.1), so the 4th
    # segment is never dropped (which would wrongly base on 1.68.0). Falls
    # through to the patch-based search when the X.Y.Z line cannot be resolved.
    if subpatch > 0:
        patch_base = "{}.{}".format(minor, patch)
        for sp in range(subpatch - 1, 0, -1):
            cand = "release/{}.{}".format(patch_base, sp)
            if cand in all_branches:
                return cand
        plain = "release/{}".format(patch_base)
        if plain in all_branches:
            return plain
        prev_previews = [b for b in all_branches
                         if b.startswith("release/{}-preview.".format(patch_base))]
        if prev_previews:
            prev_previews.sort(key=release_branch_sort_key)
            return prev_previews[-1]
        # X.Y.Z line not found as a branch — fall through to the patch search.

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
    target_key = (major, minor_num, -2, 0, 0, 0)  # before even .x
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


def _resolve_compare_to(compare_to, to_ref, version, all_branches):
    # type: (str, str, str, list[str]) -> Optional[Tuple[str, str, str]]
    """Resolve an explicit versions.json ``compare_to`` baseline to a diff range.

    Tries, in order:
      1. the latest release branch whose base version equals ``compare_to`` (the
         version plus its previews), matched exactly so e.g. "3.116.1" never also
         matches release/3.116.10;
      2. the exact ``release/<compare_to>`` branch;
      3. a ``v<compare_to>`` git tag (baselines that exist only as a tag, never
         as a release/* branch).

    Returns (from_ref, ``to_ref``, ``version``) or None when the baseline cannot
    be resolved, so the caller can fall through to automatic base detection.
    """
    compare_branches = [b for b in all_branches
                        if not b.endswith(".x")
                        and version_from_branch(b) == compare_to]
    if compare_branches:
        compare_branches.sort(key=release_branch_sort_key)
        return ("origin/{}".format(compare_branches[-1]), to_ref, version)

    exact_branch = "release/{}".format(compare_to)
    if exact_branch in all_branches:
        return ("origin/{}".format(exact_branch), to_ref, version)

    tag_sha = run(["git", "rev-parse", "v{}".format(compare_to)],
                  check=False).strip()
    if tag_sha:
        return (tag_sha, to_ref, version)

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

        # Check versions.json for an explicit compare_to override.
        config_entry = _versions_config_lookup(version)
        if config_entry and config_entry.get("compare_to"):
            resolved = _resolve_compare_to(
                config_entry["compare_to"], "origin/main", version, all_branches)
            if resolved:
                return resolved
            # Override could not be resolved — fall through to auto-detection.

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

    # ── versioned branch (release/X.Y.Z[.W] or release/X.Y.Z-preview.N) ─
    m_ver = re.match(r"release/(\d+)\.(\d+)\.(\d+)(?:\.(\d+))?", branch)
    if not m_ver:
        raise RuntimeError("Cannot parse branch: {}".format(branch))

    major = int(m_ver.group(1))
    minor_num = int(m_ver.group(2))
    patch = int(m_ver.group(3))
    subpatch = int(m_ver.group(4)) if m_ver.group(4) else 0
    version = version_from_branch(branch)  # strips preview suffix

    # Check versions.json for an explicit compare_to override.
    config_entry = _versions_config_lookup(version)
    if config_entry and config_entry.get("compare_to"):
        resolved = _resolve_compare_to(
            config_entry["compare_to"], "origin/{}".format(branch), version,
            all_branches)
        if resolved:
            return resolved
        # Override could not be resolved — fall through to auto-detection.

    # Find the cumulative base: previous stable (or previous minor for Z==0)
    base = find_previous_stable_base(all_branches, major, minor_num, patch,
                                     subpatch)

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
    No GitHub API calls needed — everything comes from git. This is the cheap
    part: a single ``git log`` plus regex parsing (sub-second even for hundreds
    of commits). The expensive per-PR effort metrics are computed separately by
    ``add_pr_effort`` so callers can skip them for unchanged pages.
    """
    # Use a format that gives us everything: hash, author email, author name,
    # subject, body. The name is kept as a safe fallback credit for PRs whose
    # GitHub login cannot be proven (see resolve_pr_authors / format_pr_list).
    SEP = "---COMMIT-END-7f3b---"
    log = run(["git", "log",
               "--format=%H%n%ae%n%an%n%s%n%b{}".format(SEP),
               "{}..{}".format(from_ref, to_ref)])

    prs = []
    seen = set()  # type: set[int]

    for block in log.split(SEP):
        block = block.strip()
        if not block:
            continue
        lines = block.split("\n", 4)
        if len(lines) < 4:
            continue

        commit_hash = lines[0].strip()
        author_email = lines[1].strip()
        author_name = lines[2].strip()
        subject = lines[3].strip()
        body = lines[4].strip() if len(lines) > 4 else ""

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

        # A GitHub login is only certain from a noreply address; otherwise leave
        # it None and let resolve_pr_authors look up the real handle. We never
        # guess a handle from an email local part — that mis-credits real users.
        login = _login_from_email(author_email)

        prs.append({
            "title": title,
            "author": {
                "login": login,
                "name": author_name,
                "email": author_email,
            },
            "url": "https://github.com/{}/pull/{}".format(REPO, num),
            "number": num,
            "body": body,
        })

    return prs


def add_pr_effort(prs):
    # type: (list[dict]) -> list[dict]
    """Annotate each PR in-place with effort metrics (commit count, days).

    This is the SLOW part: it fetches ``refs/pull/{N}/head`` (and any companion
    mono/skia PR) per PR, so it costs ~1s per PR over the network. Call it ONLY
    for pages that are actually being (re)written — never for pages that the
    unchanged check will skip — otherwise an all-unchanged ``--all`` run pays
    minutes of network cost for output it then discards.
    """
    for i, pr in enumerate(prs, 1):
        try:
            pr.update(compute_pr_effort(pr))
        except subprocess.CalledProcessError:
            pass  # effort stays unset, that's fine
        if i % 20 == 0:
            print("  Processed {}/{} PRs...".format(i, len(prs)),
                  file=sys.stderr)
    return prs


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
            author_info = pr.get("author") or {}
            login = author_info.get("login")
            name = author_info.get("name") or "unknown"
            url = pr.get("url", "")
            is_community = login != "mattleibow"
            community_str = " [community ✨]" if is_community else ""
            commits = pr.get("commitCount", 0)
            days = pr.get("workingDays", 0)
            effort = " ({} commit{}, {} day{})".format(
                commits, "s" if commits != 1 else "",
                days, "s" if days != 1 else "") if commits else ""
            skia_pr = pr.get("skiaPr")
            skia_str = " (skia: mono/skia#{})".format(skia_pr) if skia_pr else ""

            # Credit a linkable @handle only when the login is known; otherwise
            # fall back to the plain commit name (no @, no link) so the notes
            # never mention — and notify — the wrong GitHub user.
            by = "by @{}".format(login) if login else "by {}".format(name)

            lines.append("  - {} {} in {}{}{}{}".format(
                title, by, url, community_str, effort, skia_str))

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


def _compute_page_status(branch, version):
    # type: (str, str) -> Tuple[str, Optional[str], list[str]]
    """Compute (status, superseded_by, supersedes) for a branch's page.

    ``status`` is "unreleased" for main and servicing (.x) branches. For a
    versioned branch it is derived from git tags — "stable" when a stable tag
    exists, otherwise "preview" when only prereleases exist — and forced to
    "preview" when versions.json marks the version superseded. The
    ``superseded_by`` / ``supersedes`` links are the two-way supersession
    relationship, read straight from versions.json (config-authoritative).
    """
    is_unreleased = (branch == "main") or branch.endswith(".x")
    status = "unreleased"
    superseded_by = None
    if not is_unreleased:
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
        # A version marked superseded in versions.json never shipped stable —
        # force "preview" and surface the "superseded by" banner.
        superseded_by = resolve_superseded_by(version)
        if superseded_by:
            status = "preview"
    supersedes = detect_supersedes(version)
    return status, superseded_by, supersedes


def _page_filename(branch, version):
    # type: (str, str) -> str
    """Output file for a branch.

    main and servicing (.x) branches render the in-development page
    ``{version}-unreleased.md``; a versioned branch renders the released page
    ``{version}.md``.
    """
    if branch == "main" or branch.endswith(".x"):
        return "{}-unreleased.md".format(version)
    return "{}.md".format(version)


def _canonical_branches_by_version(all_branches):
    # type: (list[str]) -> dict[str, str]
    """Map each base version to its canonical (highest-sorting) versioned branch.

    Servicing (.x) branches are excluded. The canonical branch is the one that
    renders a version's ``{version}.md`` page — the stable branch if it shipped,
    otherwise the latest rc/preview — so every page is produced by exactly one
    branch and parallel previews never overwrite each other.
    """
    canonical = {}  # type: dict[str, str]
    for b in all_branches:
        if b.endswith(".x"):
            continue
        ver = version_from_branch(b)
        cur = canonical.get(ver)
        if cur is None or release_branch_sort_key(b) > release_branch_sort_key(cur):
            canonical[ver] = b
    return canonical


def _write_page(branch, all_branches, verbose=False, force=False):
    # type: (str, list[str], bool, bool) -> Optional[str]
    """Generate one release page from a branch. Returns its path, or None.

    Single code path shared by ``--branch`` and ``--all``: resolves the diff
    range, status and supersession links, then writes
    ``documentation/docfx/releases/{file}`` unless the existing file already
    encodes identical raw data (idempotent). Returns the written path, or None
    when the page was skipped (unchanged) or the diff range could not be
    determined.
    """
    try:
        from_ref, to_ref, version = determine_diff_range(branch)
    except (RuntimeError, subprocess.CalledProcessError) as e:
        print("  WARNING: Could not determine diff range for {}: {}".format(
            branch, e), file=sys.stderr)
        return None

    from_display = _removeprefix(from_ref, "origin/")
    to_display = _removeprefix(to_ref, "origin/")
    if re.match(r"^[0-9a-f]{7,}$", from_display):
        from_display = from_display[:12]
    diff_range_str = "{}..{}".format(from_display, to_display)

    status, superseded_by, supersedes = _compute_page_status(branch, version)

    if verbose:
        print("Branch: {}".format(branch))
        print("Version: {}".format(version))
        print("Status: {}".format(status))
        if superseded_by:
            print("Superseded by: {}".format(superseded_by))
        if supersedes:
            print("Supersedes: {}".format(", ".join(supersedes)))

    prs = get_prs_from_diff(from_ref, to_ref)
    print("  Found {} PR(s), diff: {}".format(len(prs), diff_range_str))

    output_path = RELEASES_DIR / _page_filename(branch, version)
    if not force and _is_content_unchanged(output_path, len(prs), diff_range_str,
                                            status, superseded_by, supersedes):
        print("  Skipping {} (unchanged)".format(output_path))
        return None

    # The page is changing, so it's worth the network work now: resolving the
    # true GitHub handles (API, cached) and the per-PR effort fetches. Doing this
    # AFTER the unchanged check is what keeps an all-unchanged --all run cheap:
    # skipped pages never pay the network cost.
    resolve_pr_authors(prs)
    add_pr_effort(prs)

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

    RELEASES_DIR.mkdir(parents=True, exist_ok=True)
    output_path.write_text(format_pr_list(prs, metadata))
    print("  Wrote {} ({} PRs)".format(output_path, len(prs)))
    return str(output_path)


def cmd_branch(branch, force=False):
    # type: (str, bool) -> None
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

    all_branches = list_remote_release_branches()
    if not all_branches:
        print("ERROR: No release branches found after fetch.")
        sys.exit(1)

    # Resolve a versioned branch to the canonical branch for its version so a
    # manual --branch on a preview never clobbers the stable {version}.md page,
    # and the result matches exactly what --all would produce. main and
    # servicing (.x) branches are keyed by branch (not version) and processed
    # as-is.
    target = branch
    if branch != "main" and not branch.endswith(".x"):
        version = version_from_branch(branch)
        canonical = _canonical_branches_by_version(all_branches).get(version)
        if canonical and canonical != branch:
            print("Note: {} is not the canonical branch for {}; "
                  "processing {} instead.".format(branch, version, canonical))
            target = canonical

    files_to_polish = []
    path = _write_page(target, all_branches, verbose=True, force=force)
    if path:
        files_to_polish.append(path)

    # When a versioned branch is pushed, also regenerate the unreleased file(s)
    # — the diff range for main or the servicing .x branch may have changed now
    # that a new release branch exists.
    if target != "main" and not target.endswith(".x"):
        files_to_polish.extend(_regen_unreleased(target, all_branches, force=force))

    cmd_update_toc()

    print("")
    print("========================================")
    print("Files to polish:")
    for f in files_to_polish:
        print("  - {}".format(f))
    print("========================================")


def _regen_unreleased(trigger_branch, all_branches, force=False):
    # type: (str, list[str], bool) -> list[str]
    """Regenerate unreleased pages after a versioned branch push.

    When a new release/X.Y.Z branch appears, the diff ranges for main and/or the
    servicing release/X.Y.x branch may have moved. Regenerates whichever
    unreleased pages are affected and returns the paths actually written.
    """
    m = re.match(r"release/(\d+)\.(\d+)\.\d+", trigger_branch)
    if not m:
        return []
    minor = "{}.{}".format(m.group(1), m.group(2))
    written = []  # type: list[str]

    # Servicing (.x) line for the same minor, if it exists.
    svc_branch = "release/{}.x".format(minor)
    if svc_branch in all_branches:
        print("\nRegenerating unreleased for {}...".format(svc_branch))
        path = _write_page(svc_branch, all_branches, force=force)
        if path:
            written.append(path)

    # main — only when its upcoming version is in the same minor as the trigger
    # (a push to a 3.119.x preview doesn't move main's range if main is on 4.x).
    main_version = get_upcoming_version()
    if main_version and minor_group(main_version) == minor:
        print("\nRegenerating unreleased for main...")
        path = _write_page("main", all_branches, force=force)
        if path:
            written.append(path)

    return written


# ── Main ─────────────────────────────────────────────────────────────


def cmd_all(force=False):
    # type: (bool) -> None
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

    # Build the processing list. Each output file must be produced by exactly
    # ONE branch, otherwise branches that map to the same page overwrite each
    # other and the last one processed wins (e.g. release/3.119.2 and its
    # release/3.119.2-preview.* all render to 3.119.2.md). So:
    #   - main                         -> {upcoming}-unreleased.md
    #   - each servicing release/X.Y.x -> {X.Y.x}-unreleased.md  (distinct files)
    #   - ONE canonical branch per versioned base version -> {version}.md
    # Superseded versions are still included — they each keep their own page
    # with a "superseded by" label (see docstring).
    servicing_branches = [b for b in all_branches if b.endswith(".x")]
    canonical_branches = sorted(
        _canonical_branches_by_version(all_branches).values(),
        key=release_branch_sort_key)
    branches_to_process = ["main"] + servicing_branches + canonical_branches

    files_to_polish = []
    skipped_count = 0
    processed_count = 0

    for branch in branches_to_process:
        if branch == "main" and not get_upcoming_version():
            print("WARNING: Cannot determine main version, skipping main.")
            continue

        print("\n--- Processing: {} ---".format(branch))
        path = _write_page(branch, all_branches, force=force)
        if path:
            files_to_polish.append(path)
            processed_count += 1
        else:
            skipped_count += 1

    # Regenerate TOC and index
    cmd_update_toc()

    # Print summary for the AI agent
    print("")
    print("========================================")
    print("Processed: {}, Skipped/unchanged: {}".format(
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
    parser.add_argument(
        "--force", action="store_true",
        help="Rewrite pages even when the raw data is unchanged "
             "(use with --all or --branch to re-resolve author handles)")

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
        cmd_all(force=args.force)
    elif args.branch:
        cmd_branch(args.branch, force=args.force)


if __name__ == "__main__":
    main()
