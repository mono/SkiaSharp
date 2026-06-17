#!/usr/bin/env python3
"""
Fetch SkiaSharp release data and manage the website release notes structure.

READ FIRST: documentation/dev/release-notes-and-changelogs.md is the behavior
SPEC for this script (and the sibling API-changelog cake target). Change the spec
first, then make this code match it — do not patch new behavior in and leave the
spec stale.

This script collects raw data and OWNS the page structure. AI only polishes the
prose — it never creates, names, or deletes pages, and never computes diff ranges.
See "Division of responsibility" below; the formatting itself uses TEMPLATE.md.

Division of responsibility
--------------------------
SCRIPT (here): decides every filename, diff range, released-vs-unreleased split,
which previews roll up, supersession banners, and which stale pages to prune. All
of this is deterministic and lives in code (see the page model below + the
docstrings on _page_filename, determine_diff_range, cleanup_stale_unreleased).

AI / SKILL.md: reads each file in the final "Files to polish:" list and rewrites
its body from the embedded raw-data block. Nothing structural — it never edits this
script. If the output looks wrong (a missing/unexpected page, a bad range), the
polish step STOPS and reports; a maintainer fixes the script here.

Page model (two files per in-flight version — released + unreleased coexist)
---------------------------------------------------------------------------
A version's "released" and "unreleased" states are orthogonal and get SEPARATE
pages that coexist while the version is in flight:

  * RELEASED  ``{version}.md``            <- a VERSIONED branch (release/X.Y.Z and
    its -rc/-preview prereleases; highest/canonical wins under --all). Full
    cumulative ROLLUP from the previous-stable base, honoring versions.json
    `compare_to`, carrying preview-milestone sections + supersede banners.

  * UNRELEASED ``{version}-unreleased.md`` <- a HEAD branch (main, or servicing
    release/X.Y.x). Small DELTA from the last release to the head ("what may ship
    next") — NOT a rollup, so it does NOT honor versions.json `compare_to` and has
    no preview milestones. Omitted entirely when the delta is empty.

So e.g. 4.150.0 has BOTH 4.150.0.md (rollup from release/4.150.0-preview.1) and
4.150.0-unreleased.md (release/4.150.0-preview.1..main delta). They never collide;
cleanup only prunes a `-unreleased` page once its line advances to a higher version
in the same minor.

When a released page rolls up tagged previews, its raw-data block groups the PRs into
per-preview BUCKETS (each PR under the preview it first shipped in, via git ancestry;
see bucket_prs_by_milestone). The buckets exhaustively partition the diff range, so the
AI renders one "## Preview N" section per bucket and merges them for the Highlights —
there is no separate flat list to drift. Pages with no previews stay a single flat list.

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

Reads scripts/infra/docs/versions.json (if present) for comparison overrides and
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

SKIA_PR_PATTERNS = [
    re.compile(r"(?:companion|related)\s+(?:skia\s+)?pr[:\s]+https?://github\.com/mono/skia/pull/(\d+)", re.IGNORECASE),
    re.compile(r"https?://github\.com/mono/skia/pull/(\d+)"),
    re.compile(r"mono/skia#(\d+)"),
]

# A skia bump commit in mono/skia names its own PR in its subject, either as a
# squash "(#N)" suffix or a "Merge pull request #N" merge subject. Used to
# recover the companion link locally from the submodule when the SkiaSharp PR
# body didn't spell it out (see resolve_skia_links).
_SKIA_SELF_PR_PATTERNS = [
    re.compile(r"^Merge pull request #(\d+)"),
    re.compile(r"\(#(\d+)\)\s*$"),
]
_SKIA_SHA_RE = re.compile(r"^[0-9a-f]{40}$")
SKIA_SUBMODULE = Path("externals/skia")
SKIA_REMOTE_URL = "https://github.com/mono/skia.git"

# Noreply email pattern: {id}+{username}@users.noreply.github.com
_NOREPLY_RE = re.compile(r"^\d+\+(.+)@users\.noreply\.github\.com$")

# Versions config (loaded lazily from scripts/infra/docs/versions.json)
_VERSIONS_CONFIG = None  # type: Optional[list[dict]]

VERSIONS_JSON_PATH = Path("scripts/infra/docs/versions.json")


def load_versions_config():
    # type: () -> list[dict]
    """Load the ``skiasharp`` overrides from versions.json (cached).

    versions.json is family-bucketed (spec §1.2): ``{ "skiasharp": { "<line>":
    {...} }, "harfbuzzsharp": {...} }``. The release-notes engine only writes
    SkiaSharp human pages, so it reads the ``skiasharp`` bucket exclusively and
    flattens it to the internal ``list[dict]`` shape (each entry carries its
    ``version`` key) the rest of this module expects.
    """
    global _VERSIONS_CONFIG
    if _VERSIONS_CONFIG is not None:
        return _VERSIONS_CONFIG
    entries = []  # type: list[dict]
    if VERSIONS_JSON_PATH.exists():
        with open(VERSIONS_JSON_PATH) as f:
            data = json.load(f)
        bucket = data.get("skiasharp", {})
        if not isinstance(bucket, dict):
            raise ValueError(
                "versions.json: 'skiasharp' must be an object keyed by line "
                "(spec §1.2); got %s" % type(bucket).__name__)
        for line, fields in bucket.items():
            entry = dict(fields)
            entry["version"] = line
            entries.append(entry)
    _VERSIONS_CONFIG = entries
    return _VERSIONS_CONFIG


def _versions_config_lookup(version):
    # type: (str) -> Optional[dict]
    """Find a ``skiasharp`` override entry matching a line core (X.Y.Z)."""
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

_AUTHOR_CACHE_PATH = Path("scripts/infra/docs/pr-authors.json")
_GRAPHQL_BATCH = 50


def load_author_cache():
    # type: () -> dict
    """Load the PR-number -> GitHub-login cache (scripts/infra/docs/pr-authors.json).

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
      2. cache (scripts/infra/docs/pr-authors.json) — previously resolved from the API.
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


def _ensure_skia_repo():
    # type: () -> bool
    """Make ``externals/skia`` usable as a local object store for ``git log``.

    Works whether the submodule is already checked out (a ``.git`` gitlink file)
    or absent (we ``git init`` an empty repo there and wire up the origin
    remote). Only commit metadata is ever fetched into it; the working tree is
    never populated, so this stays cheap.
    """
    gitdir = SKIA_SUBMODULE / ".git"
    if not gitdir.exists():
        SKIA_SUBMODULE.mkdir(parents=True, exist_ok=True)
        run(["git", "init", "-q", str(SKIA_SUBMODULE)], check=False)
    if not gitdir.exists():
        return False
    remotes = run(["git", "-C", str(SKIA_SUBMODULE), "remote"], check=False).split()
    if "origin" not in remotes:
        run(["git", "-C", str(SKIA_SUBMODULE), "remote", "add", "origin",
             SKIA_REMOTE_URL], check=False)
    return True


def resolve_skia_links(prs):
    # type: (list[dict]) -> list[dict]
    """Fill in companion mono/skia PR numbers for skia-bump PRs, purely locally.

    Most SkiaSharp PRs that bump the ``externals/skia`` submodule don't spell out
    the companion PR in their body (dependency bumps, milestone merges). For
    those we read the bumped skia commit's own subject from the submodule and
    parse its ``(#N)`` / ``Merge pull request #N`` self-reference.

    Whether a PR bumped the submodule — and to which skia SHA — is determined
    locally from the superproject tree (``git rev-parse <commit>:externals/skia``
    vs its parent), for free. The only network is a single batched, blobless,
    shallow ``git fetch`` of just the bumped SHAs into the submodule (no working
    tree, no per-PR fetch); already-present commits are skipped, so ``--all``
    pays for each skia commit at most once. Anything unresolved keeps
    ``skiaPr=None``. Mutates and returns ``prs``.
    """
    pending = []  # type: list[tuple[dict, str]]
    for pr in prs:
        if pr.get("skiaPr"):
            continue
        commit = pr.get("commit")
        if not commit:
            continue
        revs = run(["git", "rev-parse",
                    "{}:externals/skia".format(commit),
                    "{}^:externals/skia".format(commit)], check=False).split()
        if len(revs) != 2:
            continue
        new, old = revs[0], revs[1]
        if new == old or not _SKIA_SHA_RE.match(new):
            continue
        pending.append((pr, new))

    if not pending or not _ensure_skia_repo():
        return prs

    missing = sorted({sha for _, sha in pending if subprocess.run(
        ["git", "-C", str(SKIA_SUBMODULE), "cat-file", "-e", sha],
        capture_output=True).returncode != 0})
    if missing:
        print("  Fetching {} skia commit(s) to resolve companion links...".format(
            len(missing)), file=sys.stderr)
        run(["git", "-C", str(SKIA_SUBMODULE), "fetch", "-q", "--depth=1",
             "--filter=blob:none", "origin"] + missing, check=False)

    for pr, sha in pending:
        subject = run(["git", "-C", str(SKIA_SUBMODULE), "log", "-1",
                       "--format=%s", sha], check=False)
        for pat in _SKIA_SELF_PR_PATTERNS:
            m = pat.search(subject)
            if m:
                pr["skiaPr"] = int(m.group(1))
                break
    return prs



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


# ── Preview-milestone enumeration ───────────────────────────────────
#
# WHY THIS EXISTS (regression R3):
# The original hand-authored pages (PR #3763) and TEMPLATE.md end with a list of
# per-preview sections, e.g.:
#
#     ## Preview 3 (February 5, 2026)
#     [Full Changelog](.../compare/v3.119.2-preview.2.3...v3.119.2-preview.3.1)
#
# SKILL.md rules 9/10 still mandate this ("Rollup at top … Previews are minimal:
# one sentence + changelog link each, at the bottom"). When pages were migrated
# to the script (#4174) these sections were lost, because the script never told
# the AI which previews existed. We restore the feature by enumerating the
# previews DETERMINISTICALLY here and emitting them into the raw-data block.
#
# SOURCE OF TRUTH: published git tags (vX.Y.Z-<stage>.N[.B]). That is literally
# where previews, their dates, and their compare endpoints are published — and
# exactly what TEMPLATE's compare links point at. This is NOT a heuristic and is
# unrelated to supersession (which stays config-driven in versions.json): tags
# answer "which previews shipped and when", versions.json answers "which version
# supersedes which".

_FRIENDLY_STAGE = {
    "alpha": "Alpha", "beta": "Beta",
    "preview": "Preview", "rc": "Release Candidate",
}


def _core_tuple(core):
    # type: (str) -> tuple
    """(major, minor, patch, subpatch) ints from a dotted core like ``4.147.0``."""
    parts = (core.split(".") + ["0", "0", "0", "0"])[:4]
    return tuple(int(p) if p.isdigit() else 0 for p in parts)


def _parse_tag(tag):
    # type: (str) -> Optional[dict]
    """Parse a ``vX.Y.Z[.W][-stage.N[.B]]`` tag into its components.

    Returns a dict with tag, core, core_tuple, label, stage, num and a sort key
    (reusing ``release_branch_sort_key`` so stage ordering stays identical to
    branches), or None when the tag is not a recognised ``vMAJOR.MINOR…`` tag.
    """
    if not tag.startswith("v"):
        return None
    name = tag[1:]
    core, _, label = name.partition("-")
    parts = core.split(".")
    if len(parts) < 2 or not parts[0].isdigit() or not parts[1].isdigit():
        return None
    stage = num = None
    if label:
        m = re.match(r"([A-Za-z]+)", label)
        stage = m.group(1).lower() if m else None
        nums = re.findall(r"\d+", label)
        num = int(nums[0]) if nums else 0
    return {
        "tag": tag,
        "core": core,
        "core_tuple": _core_tuple(core),
        "label": label,
        "stage": stage,
        "num": num,
        "key": release_branch_sort_key("release/" + name),
    }


def _tag_date(tag):
    # type: (str) -> str
    """ISO date (YYYY-MM-DD) of a tag's commit, or '' when unknown."""
    return run(["git", "log", "-1", "--format=%ad", "--date=short", tag],
               check=False).strip()


def collect_preview_milestones(page_version, base_version):
    # type: (str, Optional[str]) -> list[dict]
    """Preview/rc milestones rolled up into a page, newest first (regression R3).

    Enumerates every published prerelease tag whose CORE version is greater than
    the page's diff base and not greater than the page version itself
    (``base_core < tag_core <= page_core``). Bounding by core (not git ancestry)
    is deliberate: it lets a page roll up the previews of a skipped predecessor
    minor — e.g. the 4.148 page lists the 4.147 previews because 4.147 never
    shipped stable and its work rolled into 4.148 (the same reason its PRs are in
    the diff range). The 4.147 page lists its own previews too; the overlap is
    intentional and the superseded/successor banners cross-link the pages.

    Each milestone carries a human label ("Preview 3"), the tag's commit date and
    a compare link — chained to the previous milestone, or to the diff base for
    the earliest one — matching the trailing ``## Preview N (date)`` sections in
    TEMPLATE.md. Tags are deduplicated to one entry per (core, stage, number),
    keeping the latest build, so re-tagged builds (preview.3.1, preview.3.2)
    collapse to a single milestone.

    Returns [] when there are no in-range prereleases (e.g. a pure stable patch).
    """
    raw = run(["git", "tag", "-l", "v*"], check=False)
    parsed = []
    for t in raw.splitlines():
        t = t.strip()
        if not t:
            continue
        p = _parse_tag(t)
        if p:
            parsed.append(p)
    if not parsed:
        return []

    # Global ascending order — used to find the earliest milestone's predecessor
    # (the diff-base tag it should compare from).
    parsed.sort(key=lambda p: p["key"])

    page_core = _core_tuple(page_version)
    base_core = _core_tuple(base_version) if base_version else (0, 0, 0, 0)

    # In-range prereleases, deduped to the latest build per milestone.
    milestones = {}  # type: dict[tuple, dict]
    for p in parsed:
        if not p["stage"]:
            continue  # stable tag — not a preview milestone
        if not (base_core < p["core_tuple"] <= page_core):
            continue
        ident = (p["core_tuple"], p["stage"], p["num"])
        cur = milestones.get(ident)
        if cur is None or p["key"] > cur["key"]:
            milestones[ident] = p

    if not milestones:
        return []

    ordered = sorted(milestones.values(), key=lambda m: m["key"])  # ascending

    # Predecessor (compare-from) for the EARLIEST milestone: the highest global
    # tag strictly below it. For a page based on a shipped stable this is that
    # stable tag (v3.119.4); for a page based on an in-flight predecessor it is
    # that predecessor's latest prerelease tag (v4.148.0-rc.1.N). Later
    # milestones chain to the prior MILESTONE (not the prior build) so re-tagged
    # builds never produce a tiny intra-milestone diff link.
    earliest_key = ordered[0]["key"]
    global_pred = None
    for p in parsed:
        if p["key"] < earliest_key:
            global_pred = p["tag"]
        else:
            break

    result = []
    for idx, m in enumerate(ordered):
        prev_tag = ordered[idx - 1]["tag"] if idx > 0 else global_pred
        compare_url = (
            "https://github.com/{}/compare/{}...{}".format(REPO, prev_tag, m["tag"])
            if prev_tag else None)
        result.append({
            "version": m["core"],
            "label": "{} {}".format(
                _FRIENDLY_STAGE.get(m["stage"], (m["stage"] or "").title()),
                m["num"]),
            "tag": m["tag"],
            "from_tag": prev_tag,
            "date": _tag_date(m["tag"]),
            "compare_url": compare_url,
        })
    result.reverse()  # newest first, matching TEMPLATE ordering
    return result


def bucket_prs_by_milestone(prs, milestones, from_ref):
    # type: (list[dict], list[dict], str) -> list[dict]
    """Partition ``prs`` into per-preview buckets the AI can summarize directly.

    A page's diff range can span several prerelease tags (the ``preview
    milestones`` list). A single flat PR list makes the AI guess which work
    landed in which preview; instead, assign every PR to the EARLIEST milestone
    whose range contains its commit — i.e. the preview it FIRST shipped in — so
    each trailing ``## Preview N`` section has its own concrete PRs.

    The buckets EXHAUSTIVELY partition the range (every PR lands in exactly one),
    so together they ARE the full list — there is no separate flat list to drift
    from. The AI merges the buckets for the top-level Highlights.

    Bucketing is by git ancestry, not tag metadata: for each milestone (oldest
    first) the bucket is ``(previous boundary .. this tag]`` intersected with the
    page's PR set. The earliest milestone's boundary is the page diff base
    (``from_ref``) so base→first-preview work is captured. Anything left after the
    last tag (commits not yet in any tagged preview, e.g. the final release cut)
    falls into a trailing untagged bucket. Returns buckets NEWEST first to match
    TEMPLATE ordering; each is ``{"milestone": <dict|None>, "prs": [...]}``.
    """
    if not milestones:
        return [{"milestone": None, "prs": list(prs)}]

    page_numbers = {pr["number"] for pr in prs}
    ascending = list(reversed(milestones))  # oldest first
    assigned = set()  # type: set[int]
    buckets_asc = []
    for i, m in enumerate(ascending):
        bucket_from = from_ref if i == 0 else ascending[i - 1]["tag"]
        in_range = {p["number"]
                    for p in get_prs_from_diff(bucket_from, m["tag"])}
        in_range &= page_numbers
        bucket_prs = [pr for pr in prs
                      if pr["number"] in in_range and pr["number"] not in assigned]
        assigned.update(pr["number"] for pr in bucket_prs)
        buckets_asc.append({"milestone": m, "prs": bucket_prs})

    leftover = [pr for pr in prs if pr["number"] not in assigned]
    result = list(reversed(buckets_asc))  # newest first
    if leftover:
        # Commits past the last tagged preview (e.g. the final release cut) —
        # newest of all, so render first, with no milestone header.
        result.insert(0, {"milestone": None, "prs": leftover})
    return result


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

        # NOTE: main is the UNRELEASED HEAD. Its page is the SMALL DELTA from the
        # last release to main ("what may ship next"), NOT a rollup — so it does
        # NOT honor versions.json `compare_to` (that drives the RELEASED rollup
        # pages produced by the versioned branches). Restored from the original
        # pre-churn script; R2 had wrongly forced a full rollup here. The full
        # 4.150.0 rollup lives on 4.150.0.md (from release/4.150.0-preview.1).

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

        # Read version from the remote branch (SKIASHARP_VERSION).
        version = get_version_from_remote_branch(branch)
        if not version:
            version = "{}.0".format(minor)

        # NOTE: a servicing .x branch is the UNRELEASED HEAD of its minor line.
        # Its page is the SMALL DELTA from the latest release branch in the minor
        # to the .x head ("what may ship in the next patch"), NOT a rollup — so
        # it does NOT honor versions.json `compare_to`. Restored from the
        # original pre-churn script; R2 had wrongly forced a full rollup here.
        # The full rollup lives on the released {version}.md (from the canonical
        # versioned branch, e.g. release/4.148.0-rc.1 -> 4.148.0.md).

        # All versioned branches for this minor (exclude .x). The latest is the
        # last release cut on the line, so the delta starts there.
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

        # No versioned branches on this minor yet — fall back to the previous
        # stable base (the delta then spans from the last actual release).
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
    No GitHub API calls needed — everything comes from git: a single ``git log``
    plus regex parsing, sub-second even for hundreds of commits.
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

        # A companion mono/skia PR link, if the body references one. Parsed
        # locally from the body — no network — so it stays a cheap hint for the
        # AI when writing notes about Skia bumps.
        skia_pr = None
        for pattern in SKIA_PR_PATTERNS:
            sm = pattern.search(body)
            if sm:
                skia_pr = int(sm.group(1))
                break

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
            "commit": commit_hash,
            "skiaPr": skia_pr,
        })

    return prs


def _format_pr_bullet(pr):
    # type: (dict) -> str
    """One PR rendered as the raw-data bullet text (without the leading "- ").

    Shared by the flat list and the per-preview buckets so both credit authors
    identically: a linkable ``@handle`` only when the GitHub login is known,
    otherwise the plain commit name (no ``@``, no link) so the notes never
    mention — and notify — the wrong GitHub user.
    """
    title = pr.get("title", "")
    author_info = pr.get("author") or {}
    login = author_info.get("login")
    name = author_info.get("name") or "unknown"
    url = pr.get("url", "")
    community_str = " [community ✨]" if login != "mattleibow" else ""
    skia_pr = pr.get("skiaPr")
    skia_str = " (skia: mono/skia#{})".format(skia_pr) if skia_pr else ""
    by = "by @{}".format(login) if login else "by {}".format(name)
    return "{} {} in {}{}{}".format(title, by, url, community_str, skia_str)


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
        # When the page rolls up tagged previews, group the PRs into per-preview
        # buckets (each PR under the preview it first shipped in) so the AI can
        # write a concrete "## Preview N" section per milestone AND merge the
        # buckets for the top-level Highlights. The buckets partition the whole
        # range, so they ARE the full list — no separate flat list is emitted.
        # Pages with no previews (unreleased deltas, plain stable patches) keep a
        # single flat list.
        buckets = metadata.get("pr_buckets")
        if buckets:
            lines.append("  PRs grouped by the preview they first shipped in "
                         "(newest first). Render each milestone as a trailing")
            lines.append('  "## <label> (<date>)" section per TEMPLATE.md, and '
                         "merge them all for the top-level Highlights:")
            for b in buckets:
                m = b.get("milestone")
                lines.append("")
                if m:
                    lines.append(
                        "  ## {label} · {version} · {date} · {url}  ({n} PRs)".format(
                            label=m["label"], version=m["version"],
                            date=m.get("date") or "", url=m.get("compare_url") or "",
                            n=len(b["prs"])))
                else:
                    lines.append(
                        "  ## Unreleased — not yet in a tagged preview  "
                        "({n} PRs)".format(n=len(b["prs"])))
                if b["prs"]:
                    for pr in b["prs"]:
                        lines.append("    - {}".format(_format_pr_bullet(pr)))
                else:
                    lines.append("    *(no PRs)*")
        else:
            for pr in prs:
                lines.append("  - {}".format(_format_pr_bullet(pr)))

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

    ai_lines = [
        "<!-- AI: Use the raw PR data in the comment above to write polished",
        "     release notes here. Follow documentation/docfx/releases/TEMPLATE.md",
        "     for structure and tone.",
    ]
    if metadata.get("preview_milestones"):
        ai_lines.append(
            "     Render each PREVIEW MILESTONE listed above as a minimal trailing")
        ai_lines.append(
            '     "## <label> (<date>)" section — one sentence + its compare link,')
        ai_lines.append(
            "     newest first, after the rolled-up categories (TEMPLATE rules 9/10).")
    if supersedes:
        ai_lines.append(
            "     This release SUPERSEDES {} (preview-only, rolled up). Keep the"
            .format(", ".join(supersedes)))
        ai_lines.append(
            '     "Supersedes" note above and mention in the Highlights that this')
        ai_lines.append(
            "     release rolls up that skipped preview work cumulatively.")
    ai_lines.append("-->")
    lines.extend(ai_lines)
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
    """Delete ``{version}-unreleased.md`` pages whose line has moved on.

    Released and unreleased pages of the SAME version coexist by design (Option
    A): a version can be on NuGet as a shipped prerelease (released ``{v}.md``)
    AND still have newer head commits (``{v}-unreleased.md`` delta). So a stale
    unreleased page is NOT simply "``{v}.md`` exists" — that would wrongly delete
    the in-flight delta.

    Deterministic rule (filesystem only): an unreleased page is stale once its
    line has advanced to a STRICTLY HIGHER version in the SAME minor. That is,
    delete ``{v}-unreleased.md`` only when both:
      * a released ``{v}.md`` exists (the version itself has a page), AND
      * some other page (released or unreleased) in the same minor has a version
        greater than ``v`` — the head has moved past ``v``.

    Examples:
      * 4.148.0.md + 4.148.0-unreleased.md, no higher 4.148.x page  -> KEEP both.
      * 3.119.4.md + 3.119.4-unreleased.md, and 3.119.5-unreleased.md exists
        -> the line advanced to 3.119.5, so 3.119.4-unreleased.md is removed.
    Returns the removed file paths.
    """
    pages = [f.stem for f in RELEASES_DIR.iterdir()
             if f.suffix == ".md" and f.name not in ("index.md", "TEMPLATE.md")]
    # Page version (strip the -unreleased suffix) for every page on disk.
    page_versions = [
        p[:-len("-unreleased")] if p.endswith("-unreleased") else p
        for p in pages
    ]

    removed = []
    for f in sorted(RELEASES_DIR.iterdir()):
        if f.suffix != ".md" or not f.stem.endswith("-unreleased"):
            continue
        version = f.stem[:-len("-unreleased")]
        # No released counterpart -> this is the only page for the version; keep.
        if not (RELEASES_DIR / "{}.md".format(version)).exists():
            continue
        # Released counterpart exists. Keep the unreleased delta while this
        # version is still the head of its minor line; remove it only once a
        # strictly higher version in the same minor has a page.
        mg = minor_group(version)
        moved_on = any(
            ov != version
            and minor_group(ov) == mg
            and version_key(ov) > version_key(version)
            for ov in page_versions
        )
        if moved_on:
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
    """Output file for a branch: ``{version}.md`` vs ``{version}-unreleased.md``.

    ORIGINAL deterministic two-file rule (restored — see plan/SKILL). The two
    pages model the orthogonal "released" vs "unreleased" states of a version:

      * A VERSIONED branch (``release/X.Y.Z`` and its ``-rc``/``-preview``
        prereleases) renders the RELEASED ``{version}.md`` — the full cumulative
        rollup of the shipped prerelease/stable, with preview-milestone sections
        and supersede banners. One page per version (the canonical / highest
        versioned branch wins under --all; see _canonical_branches_by_version).

      * A HEAD branch (``main`` or servicing ``release/X.Y.x``) renders the
        UNRELEASED ``{version}-unreleased.md`` — a SMALL DELTA from the last
        release to the head (what may ship next), NOT a rollup.

    The two coexist for an in-flight version (e.g. 4.150.0.md from
    release/4.150.0-preview.1 AND 4.150.0-unreleased.md from main). They never
    collide (distinct filenames); cleanup_stale_unreleased only prunes an
    unreleased page once its line advances to a higher version.
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

    # An UNRELEASED head page (main / release/X.Y.x) with an EMPTY delta has
    # nothing unreleased — the head is at the last release. Don't emit an empty
    # "what may ship next" page; prune any stale one so the released {v}.md
    # stands alone. Released pages are always written (a shipped version with no
    # changes since its base is still a real, if empty, release record).
    is_head = (branch == "main") or branch.endswith(".x")
    if is_head and not prs:
        if output_path.exists():
            output_path.unlink()
            print("  Removed empty {} (nothing unreleased)".format(output_path))
        return None

    if not force and _is_content_unchanged(output_path, len(prs), diff_range_str,
                                            status, superseded_by, supersedes):
        print("  Skipping {} (unchanged)".format(output_path))
        return None

    # The page is changing, so it's worth the one network step now: resolving
    # the true GitHub handles (API, cached). Doing this AFTER the unchanged
    # check keeps an all-unchanged --all run cheap: skipped pages never pay the
    # network cost.
    resolve_pr_authors(prs)
    resolve_skia_links(prs)

    # Enumerate the preview/rc milestones this page rolls up (regression R3), so
    # the AI can render the trailing "## Preview N (date)" sections that TEMPLATE
    # and SKILL rules 9/10 require. The lower bound is the diff base, so a page
    # naturally includes a skipped predecessor minor's previews (the 4.148 page
    # lists the 4.147 previews — the same work its diff range already rolls up).
    base_version = None
    if from_display.startswith("release/"):
        base_version = version_from_branch(from_display)
    elif re.match(r"^\d+\.\d+\.\d+", from_display):
        base_version = from_display
    preview_milestones = collect_preview_milestones(version, base_version)

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
    if preview_milestones:
        metadata["preview_milestones"] = preview_milestones
        # Partition the PRs into per-preview buckets (each PR under the preview
        # it first shipped in) so the AI can summarize each milestone directly.
        metadata["pr_buckets"] = bucket_prs_by_milestone(
            prs, preview_milestones, from_ref)

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
    #   - main                         -> {upcoming}-unreleased.md   (delta head)
    #   - each servicing release/X.Y.x -> {X.Y.0}-unreleased.md      (delta head)
    #   - ONE canonical branch per versioned base version -> {version}.md (released)
    # Released and unreleased pages of the same version COEXIST (distinct
    # filenames) and never collide, so no branch is deferred. Superseded
    # versions still keep their own released page with a "superseded by" label.
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
