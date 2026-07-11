#!/usr/bin/env python3
"""
Fetch SkiaSharp release data and manage the website release notes structure.

READ FIRST: documentation/dev/release-notes-and-api-diffs.md is the behavior
SPEC for this script (and the sibling API-diff cake target). Change the spec
first, then make this code match it — do not patch new behavior in and leave the
spec stale.

This script collects the per-page FACTS and writes them as JSON. It decides every
filename, diff range, and released-vs-unreleased split, but it does NOT own the
page's Markdown: it emits ``_sources/<version>.data.json`` and never writes a
``.md``. ``release-notes-render.py`` turns data.json + the agent's prose.json into the page;
the AI only writes prose — it never creates, names, or deletes pages, and never
computes diff ranges. See "Division of responsibility" below.

Division of responsibility
--------------------------
SCRIPT (here): decides every filename, diff range, released-vs-unreleased split,
which previews roll up, supersession banners, and the facts each page is built
from. All of this is deterministic and lives in code (see the page model below +
the docstrings on _page_filename, determine_diff_range, and the live-head set that
release-notes-index.py records for release-notes-render.py to prune against).

AI / SKILL.md: reads each file in the final "Files to polish:" list, reads that
page's ``_sources/<version>.data.json``, and writes ``_sources/<version>.prose.json``
(prose only — theme, highlights, breaking summaries, category bullets, contributor
and preview summaries). Nothing structural — it never edits this script and never
hand-writes the page. If the output looks wrong (a missing/unexpected page, a bad
range), the Polish phase STOPS and reports; a maintainer fixes the script here.

Output streams (the Prepare-phase contract)
--------------------------------------------
This generator runs VERBOSE: all progress and diagnostics — "Processing…", "Found N
PRs", "Skipping (unchanged)", warnings, errors, and the final list of pages to
polish — stream to STDERR via ``log()``, so a CI job log shows the work (and any
disk/timeout failure) as it happens (spec §2.2/§2.3). Nothing is printed to STDOUT.

The machine-readable result the Polish phase consumes — the list of pages whose
``data.json`` changed — is ALWAYS written to a file: ``output/files-to-polish.txt``
by default, or the path given to ``--polish-list``. It is a plain list, one
repo-relative path per line; an empty file means nothing changed. Because the list
lives in a file (not a stream), verbose progress can flow freely (spec §2.3).

Page model (two files per in-flight version — released + unreleased coexist)
---------------------------------------------------------------------------
A version's "released" and "unreleased" states are orthogonal and get SEPARATE
pages that coexist while the version is in flight:

  * RELEASED  ``{version}.md``            <- a VERSIONED branch (release/X.Y.Z and
    its -rc/-preview prereleases; highest/canonical wins across a full run). Full
    cumulative ROLLUP from the previous-stable base, honoring versions.json
    `compare_to`, carrying preview-milestone sections + supersede banners.

  * UNRELEASED ``{version}-unreleased.md`` <- a HEAD branch (main, or servicing
    release/X.Y.x). Small DELTA from the last release to the head ("what may ship
    next") — NOT a rollup, so it does NOT honor versions.json `compare_to` and has
    no preview milestones. Omitted entirely when the delta is empty.

So e.g. 4.150.0 has BOTH 4.150.0.md (rollup from release/4.150.0-preview.1) and
4.150.0-unreleased.md (release/4.150.0-preview.1..main delta). They never collide;
the stale-head prune (release-notes-index.py records the live set, release-notes-render.py --all
deletes the rest) only removes a `-unreleased` page once its line advances higher.

When a released page rolls up tagged previews, its data.json groups the PRs into
per-preview BUCKETS (each PR under the preview it first shipped in, via git ancestry;
see bucket_prs_by_milestone). The buckets exhaustively partition the diff range, so the
AI renders one "## Preview N" section per bucket and merges them for the Highlights —
there is no separate flat list to drift. Pages with no previews stay a single flat list.

Commands:
    # Regenerate every branch (main + all release/*); skip unchanged files
    python3 release-notes-data.py

    # Rewrite every page, even unchanged ones (after a format/skill change)
    python3 release-notes-data.py --force

    # Bound to a version range — or a single version when min == max
    python3 release-notes-data.py --min-version 4.147.0 --max-version 4.151.0
    python3 release-notes-data.py --min-version 4.148.0 --max-version 4.148.0

The default (no scope flags) iterates every branch and emits each changed
version's data.json + page, writing only files whose PR count or diff range has
changed (idempotent) — this is what the automated workflow runs.
``--min-version``/``--max-version`` bound the walk to a version range (set both
equal to regenerate one version); ``--force`` rewrites even unchanged pages.

Reads scripts/infra/docs/versions.json (if present) for comparison overrides and
supersession markers. versions.json is the single source of truth: only the
versions listed there get a non-default baseline or a superseded marker.

Requirements: git, Python 3.7+
"""

from __future__ import annotations

import argparse
import hashlib
import json
import re
import subprocess
import sys
import urllib.request
from collections import defaultdict
from datetime import datetime, timezone
from pathlib import Path
from typing import Optional, Tuple


REPO = "mono/SkiaSharp"
RELEASES_DIR = Path("documentation/docfx/releases")

# The Prepare phase ALWAYS writes the machine-readable "Files to polish" list to a
# file (overridable with --polish-list). output/ is gitignored, so the list stays
# out of the working-tree patch the Prepare job hands to the Polish agent.
DEFAULT_POLISH_LIST = Path("output/files-to-polish.txt")

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
_VERSIONS_CONFIG = {}  # type: dict[str, list[dict]]

VERSIONS_JSON_PATH = Path("scripts/infra/docs/versions.json")

# HarfBuzz-owned paths (spec §1.5/§4.5). A HarfBuzz family page lists ONLY the
# commits touching these — the HarfBuzzSharp binding + its native assets, the
# native libHarfBuzzSharp build, and HarfBuzz tests — NEVER any SkiaSharp.* source
# (the SkiaSharp-versioned managed shaper source/SkiaSharp.HarfBuzz stays on the
# SkiaSharp pages). Passed to ``git log -- <pathspec>`` to filter a co-shipping
# SkiaSharp range down to its HarfBuzz changes. ``:(glob)`` magic makes ``*``/``**``
# behave like globs (``*`` stops at ``/``, ``**`` spans), matching the spec list.
HARFBUZZ_PATHSPECS = [
    ":(glob)binding/HarfBuzzSharp/**",
    ":(glob)binding/HarfBuzzSharp.NativeAssets.*/**",
    "binding/libHarfBuzzSharp.json",
    "binding/IncludeNativeAssets.HarfBuzzSharp.targets",
    ":(glob)native/*/libHarfBuzzSharp/**",
    ":(glob)tests/Tests/HarfBuzzSharp/**",
]


def load_versions_config(family="skiasharp"):
    # type: (str) -> list[dict]
    """Load a family's overrides from versions.json (cached per family).

    versions.json is family-bucketed (spec §1.2): ``{ "skiasharp": { "<line>":
    {...} }, "harfbuzzsharp": {...} }``. Both families and both engines share this
    one file; ``family`` selects the bucket — ``skiasharp`` for the SkiaSharp
    pages, ``harfbuzzsharp`` for the HarfBuzz peer family (spec §1.5). The bucket
    is flattened to the internal ``list[dict]`` shape (each entry carries its
    ``version`` key) the rest of this module expects. A missing bucket is empty.
    """
    cached = _VERSIONS_CONFIG.get(family)
    if cached is not None:
        return cached
    entries = []  # type: list[dict]
    if VERSIONS_JSON_PATH.exists():
        with open(VERSIONS_JSON_PATH) as f:
            data = json.load(f)
        bucket = data.get(family, {})
        if not isinstance(bucket, dict):
            raise ValueError(
                "versions.json: '%s' must be an object keyed by line "
                "(spec §1.2); got %s" % (family, type(bucket).__name__))
        for line, fields in bucket.items():
            entry = dict(fields)
            entry["version"] = line
            entries.append(entry)
    _VERSIONS_CONFIG[family] = entries
    return entries


def _versions_config_lookup(version, family="skiasharp"):
    # type: (str, str) -> Optional[dict]
    """Find a family override entry matching a line core (X.Y.Z)."""
    for entry in load_versions_config(family):
        if entry.get("version") == version:
            return entry
    return None


# Support-channel config — the top-level "support" block in versions.json, read
# ONLY by the release-notes TOC/index (the API-diff engine ignores it). Cached
# after first load.
_SUPPORT_CONFIG = None  # type: Optional[dict]

# History floor cache (spec §1.4). Read from the top-level ``history_floor`` block
# in versions.json, keyed by family: ``{"skiasharp": "3.0.0"}``.
_HISTORY_FLOOR = None  # type: Optional[dict]


def history_floor(family="skiasharp"):
    # type: (str) -> Optional[str]
    """The configured minimum line core for ``family``, or None when unset."""
    global _HISTORY_FLOOR
    if _HISTORY_FLOOR is None:
        _HISTORY_FLOOR = {}
        if VERSIONS_JSON_PATH.exists():
            with open(VERSIONS_JSON_PATH) as f:
                block = json.load(f).get("history_floor") or {}
            if isinstance(block, dict):
                _HISTORY_FLOOR = {
                    k: v for k, v in block.items() if isinstance(v, str) and v}
    return _HISTORY_FLOOR.get(family)


def is_below_history_floor(version, family="skiasharp"):
    # type: (str, str) -> bool
    """True when ``version``'s line core sorts below the family's history floor.

    Used to skip regenerating obsolete back-catalogue pages (spec §1.4). With no
    floor configured this is always False, so every line is processed as before.
    """
    floor = history_floor(family)
    if not floor:
        return False
    return _core_tuple(version) < _core_tuple(floor)


# Co-release map cache + path (spec §3.6). The map crosses from the API-diff engine
# into this engine to emit the deterministic SkiaSharp-page -> HarfBuzz-folder link.
_CO_RELEASE_MAP = None  # type: Optional[dict]
CO_RELEASE_MAP_PATH = RELEASES_DIR / "_sources" / "co-release-map.json"


def load_co_release_map():
    # type: () -> dict
    """Load the co-release map sidecar (cached) as ``{skia_line: hb_line}``.

    The sidecar is a plain ``{ "skia_line": "hb_line" }`` object (spec §3.6). The
    api-diff LINK is derived here (``harfbuzzsharp/<hb_line>/index.md``), not stored.
    A legacy array-of-objects format (``{skia_line, hb_line, hb_link}``) is tolerated
    for back-compat. Returns an empty map when the sidecar is missing — the HarfBuzz
    link is simply omitted then (the script owns the link, never the AI; spec §4.4).
    """
    global _CO_RELEASE_MAP
    if _CO_RELEASE_MAP is not None:
        return _CO_RELEASE_MAP
    mapping = {}  # type: dict
    if CO_RELEASE_MAP_PATH.exists():
        with open(CO_RELEASE_MAP_PATH) as f:
            data = json.load(f)
        if isinstance(data, dict):
            for line, hb in data.items():
                if line and hb:
                    mapping[line] = hb
        elif isinstance(data, list):  # legacy array format
            for entry in data:
                line = entry.get("skia_line")
                hb = entry.get("hb_line")
                if line and hb:
                    mapping[line] = hb
        else:
            raise ValueError(
                "co-release-map.json: expected a JSON object (spec §3.6); got %s"
                % type(data).__name__)
    _CO_RELEASE_MAP = mapping
    return _CO_RELEASE_MAP


def _sha256_bytes(data):
    # type: (bytes) -> str
    """``sha256:<hex>`` digest of raw bytes (companion-file content key, §4.7)."""
    return "sha256:" + hashlib.sha256(data).hexdigest()


def load_notes_sidecar(stem, base_dir):
    # type: (str, Path) -> Optional[dict]
    """The maintainer-authored manual additions sidecar (spec §3.7).

    A ``_sources/<stem>.notes.md`` beside the page's other inputs: freeform
    Markdown a human injects to survive re-polish. Returns ``{'path':
    <page-relative>, 'sha256': <hash>}`` when present, else None. Only the BYTES
    are hashed — Python never parses the content (the Polish AI reads it, §4.7).
    The path is page-relative (``_sources/<stem>.notes.md``), so the AI resolves
    it straight from the page.
    """
    notes_path = base_dir / "_sources" / "{}.notes.md".format(stem)
    if not notes_path.is_file():
        return None
    return {"path": "_sources/{}.notes.md".format(stem),
            "sha256": _sha256_bytes(notes_path.read_bytes())}


def load_breaking_companions(line, base_dir):
    # type: (str, Path) -> Optional[dict]
    """The API breaking-diff companions for a line (spec §3.3/§4.7).

    Globs ``<line>/**/<assembly>.breaking.md`` under the line's API-diff folder.
    ``api-diff.cake`` writes a ``.breaking.md`` ONLY when real breaking changes
    exist (it deletes an empty one, §5.2), so any match means this line broke
    something. Returns ``{'paths': [<page-relative>, …], 'sha256': <combined
    hash>}`` or None. The combined hash covers the sorted ``(path, bytes)`` pairs,
    so it is order-stable and flips when any breaking file is added, removed, or
    changed — which is what re-polishes the page (§4.6).
    """
    folder = base_dir / line
    if not folder.is_dir():
        return None
    files = sorted(folder.glob("**/*.breaking.md"))
    if not files:
        return None
    h = hashlib.sha256()
    paths = []  # type: list[str]
    for f in files:
        rel = f.relative_to(base_dir).as_posix()
        paths.append(rel)
        h.update(rel.encode("utf-8"))
        h.update(b"\0")
        h.update(f.read_bytes())
        h.update(b"\0")
    return {"paths": paths, "sha256": "sha256:" + h.hexdigest()}


def _removeprefix(s, prefix):
    # type: (str, str) -> str
    """str.removeprefix polyfill for Python < 3.9."""
    if s.startswith(prefix):
        return s[len(prefix):]
    return s


def log(*args, **kwargs):
    # type: (...) -> None
    """Human-facing progress and diagnostics — always written to STDERR.

    This generator is verbose: ``log()`` is the ONLY output stream (nothing goes to
    STDOUT), so a long download or a disk/timeout failure is visible in the CI job
    log as it happens (spec §2.2). The machine-readable "Files to polish" list does
    NOT ride on a stream — it is always written to a file (spec §2.3) — so callers
    never have to parse it out of progress text.
    """
    kwargs["file"] = sys.stderr
    print(*args, **kwargs)


def write_polish_list(files, path=None):
    # type: (list, ...) -> None
    """Write the machine-readable "Files to polish" list to a file (spec §2.3).

    Always writes a file — ``output/files-to-polish.txt`` by default, or *path* if
    given. One repo-relative page path per line; an **empty file** means nothing
    changed this run. This is the Prepare phase's only machine-readable output, so
    the Polish agent reads a file instead of scraping stdout. The parent directory
    is created if needed.
    """
    path = Path(path) if path else DEFAULT_POLISH_LIST
    path.parent.mkdir(parents=True, exist_ok=True)
    with path.open("w", encoding="utf-8") as fh:
        for f in files:
            fh.write("{}\n".format(f))
    log("Wrote files-to-polish list ({} file{}) -> {}".format(
        len(files), "" if len(files) == 1 else "s", path))
    for f in files:
        log(f)


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


# Chrome's public release schedule (Chromium Dash). Used to drive the release
# cadence section with the real phase dates for the milestones currently in
# flight. The four SkiaSharp cadence phases map onto these schedule fields:
#   Beta Promotion -> earliest_beta   Early Stable  -> early_stable
#   Stable Cut     -> stable_cut      Stable Release -> stable_date
CHROME_SCHEDULE_URL = (
    "https://chromiumdash.appspot.com/fetch_milestone_schedule?mstone={}")

_MONTH_ABBR = ["Jan", "Feb", "Mar", "Apr", "May", "Jun",
               "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"]


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


def _version_is_superseded(version, family="skiasharp"):
    # type: (str, str) -> bool
    """True if versions.json marks this line ``status: superseded`` (spec §1.2)."""
    entry = _versions_config_lookup(version, family)
    return bool(entry and entry.get("status") == "superseded")


def resolve_superseded_by(version, family="skiasharp"):
    # type: (str, str) -> Optional[str]
    """Return the version that supersedes ``version``, or None.

    versions.json is the SINGLE source of truth: a version is superseded only
    when its config entry sets ``status: superseded``, and the successor is the
    configured ``superseded_by``. There is no auto-detection, so the superseded
    set is identical to Cake's ``IsVersionSuperseded`` (which likewise treats
    only an explicit ``status: superseded`` entry as superseded). Every other
    version — listed with only ``compare_to`` or not listed at all — is a real
    release and is never reported as superseded. To skip a version everywhere,
    add it to versions.json. ``family`` selects the bucket so the rule applies
    per family (spec §1.2/§1.5).
    """
    entry = _versions_config_lookup(version, family)
    if entry and entry.get("status") == "superseded":
        return entry.get("superseded_by")
    return None


def detect_supersedes(version, family="skiasharp"):
    # type: (str, str) -> list[str]
    """Return the versions that ``version`` rolls up (the inverse link).

    Reads versions.json directly: every entry marked ``status: superseded``
    whose ``superseded_by`` is exactly ``version``. This is the back-link that
    makes the supersede relationship two-way — the superseded page points
    forward ("Superseded by X"), and this lets the successor page point back
    ("Supersedes Y"). Covers a skipped minor (4.148.0 supersedes 4.147.0) and an
    abandoned patch preview (3.119.4 supersedes 3.119.3). Returns [] when this
    version supersedes nothing. ``family`` selects the bucket (spec §1.5).
    """
    rolled = [entry["version"] for entry in load_versions_config(family)
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

_AUTHOR_CACHE_PATH = RELEASES_DIR / "_sources" / "pr-authors.json"
_GRAPHQL_BATCH = 50


def load_author_cache():
    # type: () -> dict
    """Load the PR-number -> GitHub-login cache (releases/_sources/pr-authors.json).

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
      2. cache (releases/_sources/pr-authors.json) — previously resolved from the API.
      3. GitHub GraphQL — the authoritative PR author, batched then cached.

    PRs that still cannot be resolved keep ``login=None``; build_data_json then
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
        log("  Resolving {} PR author(s) via GitHub API...".format(
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
        log("  Fetching {} skia commit(s) to resolve companion links...".format(
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
# The original hand-authored pages (PR #3763) ended with a list of per-preview
# sections that release-notes-render.py now emits, e.g.:
#
#     ## Preview 3 (February 5, 2026)
#     [Full Changelog](.../compare/v3.119.2-preview.2.3...v3.119.2-preview.3.1)
#
# SKILL.md rules 9/10 still mandate this ("Rollup at top … Previews are minimal:
# one sentence + Full Changelog link each, at the bottom"). When pages were migrated
# to the script (#4174) these sections were lost, because the script never told
# the AI which previews existed. We restore the feature by enumerating the
# previews DETERMINISTICALLY here and emitting them into data.json.
#
# SOURCE OF TRUTH: published git tags (vX.Y.Z-<stage>.N[.B]). That is literally
# where previews, their dates, and their compare endpoints are published — and
# exactly what the rendered compare links point at. This is NOT a heuristic and is
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
    the earliest one — matching the trailing ``## Preview N (date)`` sections release-notes-render.py emits. Tags are deduplicated to one entry per (core, stage, number),
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
    result.reverse()  # newest first, matching the rendered page ordering
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
    rendered (newest-first) ordering; each is ``{"milestone": <dict|None>, "prs": [...]}``.
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
            # Skip a line versions.json marks superseded (spec §1.2/§1.3): its
            # work rolls up into the next emitted line, so it must never act as a
            # baseline even if it happens to carry a stable tag (e.g. 3.119.4
            # skips the superseded 3.119.3 and bases on 3.119.2).
            if _version_is_superseded(prev_version):
                continue
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

        # No branch yet for the upcoming minor — the unreleased delta is still a
        # SMALL "what may ship next", never a rollup (spec §4.2). Base on the
        # latest release cut strictly BELOW main's line (the immediately previous
        # release line's tip), regardless of stable/preview/superseded. Using the
        # last *stable* here (the old behaviour) wrongly skipped preview-only
        # predecessors like 4.148 and turned main's page into a multi-minor
        # rollback to 3.119.4 — bug D.
        all_versioned = [b for b in all_branches if not b.endswith(".x")]
        all_versioned.sort(key=release_branch_sort_key)

        if not all_versioned:
            raise RuntimeError(
                "No release branches found. Cannot determine diff range "
                "for main. Ensure release branches are fetched.")

        below = [b for b in all_versioned
                 if version_key(version_from_branch(b)) < version_key(version)]
        latest = below[-1] if below else all_versioned[-1]
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


# Each PR is classified product / mixed / internal by the files it touched, so Polish
# can FOCUS on product, INSPECT mixed, and MENTION internal (§4.4) — a lexical filter
# instead of re-judging every PR from its title each run (the source of leak variance).
#
#   product  — touches SHIPPED code (managed API, native code, Views). A real change a
#              consumer can see. Companion test/benchmark/generated files are ignored.
#   mixed    — touches only BUILD config (native/): may change the shipped binary via a
#              compile define (e.g. a rasteriser flag), or may be pure infra (Docker
#              image, SDK pin). Polish guesses from the title/context in data.json
#              (it does not open the PR) — surface a behaviour change, drop pure infra.
#   internal — touches NEITHER: a pure repository process (CI, workflows, agent skills,
#              docs site, tests, samples, build/meta files). Dropped into the collapse line.
_SHIP_PATH_PREFIXES = ("binding/", "externals/", "source/")
_BUILD_PATH_PREFIXES = ("native/",)


def _pr_category(files):
    # type: (set) -> str
    """Classify a PR ``product`` / ``mixed`` / ``internal`` by touched files (§4.4)."""
    if any(f.startswith(_SHIP_PATH_PREFIXES) for f in files):
        return "product"
    if any(f.startswith(_BUILD_PATH_PREFIXES) for f in files):
        return "mixed"
    return "internal"


# Automation accounts — never credited as human contributors (§4.5). The workflow
# already skips these when authoring, but data.json still records them (they open
# release-notes and bump PRs), so Polish must exclude them from the contributor table.
_BOT_LOGINS = frozenset({"github-actions[bot]", "github-actions", "copilot", "dependabot"})


def _is_bot_login(login):
    # type: (Optional[str]) -> bool
    """True for automation accounts (exact match or any ``[bot]`` suffix)."""
    if not login:
        return False
    low = login.lower()
    return low in _BOT_LOGINS or low.endswith("[bot]")


def _contributor_roster(prs):
    # type: (list) -> list
    """Authoritative external-contributor roster for the credits table (§4.5).

    Returns ``[(login, [pr_number, ...]), ...]`` for every distinct non-maintainer,
    non-bot author with a *linkable* login, ordered by PR count (most first) then
    login. Polish renders EXACTLY one table row per entry and never omits one — the
    old ad-hoc reconstruction from the body prose silently dropped real contributors
    (e.g. a headline author whose PRs were folded into a thematic bullet). Authors
    with no resolvable login are excluded because they cannot be safely @-linked.
    """
    by_login = {}  # type: dict
    for pr in prs:
        login = (pr.get("author") or {}).get("login")
        if not login or login == "mattleibow" or _is_bot_login(login):
            continue
        by_login.setdefault(login, []).append(pr.get("number"))
    roster = [(login, sorted(n for n in nums if n)) for login, nums in by_login.items()]
    roster.sort(key=lambda e: (-len(e[1]), e[0].lower()))
    return roster


def _files_by_commit(from_ref, to_ref):
    # type: (str, str) -> dict
    """Map each commit hash in ``from_ref..to_ref`` to the set of files it touched.

    One ``git log --name-only`` call (no pathspec filter — we want each PR's FULL
    file set to classify it product vs internal). A ``\\x1e`` record separator
    prefixes each hash line so file lines can never be mistaken for a hash.

    We SPLIT on ``\\x1e`` rather than matching a leading ``\\x1e`` per line: git
    lists commits newest-first, and ``run()`` returns ``stdout.strip()`` — and
    ``\\x1e`` is whitespace to Python's ``str.strip()``, so the FIRST (newest)
    record loses its leading separator. A ``startswith("\\x1e")`` scan would then
    silently drop that newest commit's file list, tagging it ``internal`` — which
    is exactly how the latest automated ``[skia-sync]`` submodule bump (usually the
    newest commit in the range) kept getting mis-tagged. Splitting recovers every
    record whether or not its leading separator survived.
    """
    out = run(["git", "log", "--no-renames", "--name-only",
               "--format=%x1e%H", "{}..{}".format(from_ref, to_ref)])
    files_by = {}  # type: dict
    for record in out.split("\x1e"):
        record = record.strip()
        if not record:
            continue
        # First line is the commit hash; the rest (past the blank line git emits
        # before --name-only output) are the touched paths. Merge commits have no
        # --name-only output, so they map to an empty set (still `internal`).
        lines = record.split("\n")
        commit = lines[0].strip()
        if not commit:
            continue
        files_by[commit] = {ln.strip() for ln in lines[1:] if ln.strip()}
    return files_by


def get_prs_from_diff(from_ref, to_ref, paths=None):
    # type: (str, str, Optional[list[str]]) -> list[dict]
    """Extract merged PRs from git log between two refs.

    Parses PR numbers, titles, authors, and bodies from commit messages.
    No GitHub API calls needed — everything comes from git: a single ``git log``
    plus regex parsing, sub-second even for hundreds of commits.

    ``paths`` (optional) restricts the log to commits touching those git
    pathspecs — used by the HarfBuzz family to filter a co-shipping SkiaSharp
    range down to HarfBuzz-owned changes (spec §4.5).
    """
    # Use a format that gives us everything: hash, author email, author name,
    # subject, body. The name is kept as a safe fallback credit for PRs whose
    # GitHub login cannot be proven (see resolve_pr_authors / build_data_json).
    SEP = "---COMMIT-END-7f3b---"
    cmd = ["git", "log",
           "--format=%H%n%ae%n%an%n%s%n%b{}".format(SEP),
           "{}..{}".format(from_ref, to_ref)]
    if paths:
        cmd.append("--")
        cmd.extend(paths)
    log = run(cmd)

    # One extra git call gives every commit's full file set, used to tag each PR
    # product vs internal (§4.4). Unfiltered by ``paths`` on purpose — a PR's
    # classification depends on everything it changed, not the co-ship subset.
    files_by = _files_by_commit(from_ref, to_ref)

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
            "category": _pr_category(files_by.get(commit_hash, set())),
        })

    return prs


def _friendly_date(iso):
    # type: (Optional[str]) -> Optional[str]
    """'2026-06-12' -> 'June 12, 2026'. Passes through anything not ISO."""
    if not iso or not re.match(r"^\d{4}-\d{2}-\d{2}$", iso):
        return iso
    y, m, d = (int(x) for x in iso.split("-"))
    return "{} {}, {}".format(datetime(y, m, 1).strftime("%B"), d, y)


def _release_date_display(version):
    # type: (str) -> Optional[str]
    """Human release date (e.g. 'December 3, 2024') from the ``vX.Y.Z`` tag, or None.

    Used to fill the stable-page banner's ``Released <date>`` field deterministically
    so Polish never has to guess it. Prefers the tag's creator date (annotated tag);
    falls back to the tagged commit's date; returns None if the tag is not present.
    """
    tag = "v{}".format(version)
    out = ""
    try:
        out = run(["git", "for-each-ref", "--format=%(creatordate:short)",
                   "refs/tags/{}".format(tag)]).strip()
    except Exception:
        out = ""
    if not re.match(r"^\d{4}-\d{2}-\d{2}$", out):
        try:
            out = run(["git", "log", "-1", "--format=%cs", tag]).strip()
        except Exception:
            out = ""
    if not re.match(r"^\d{4}-\d{2}-\d{2}$", out):
        return None
    y, m, d = (int(x) for x in out.split("-"))
    return "{} {}, {}".format(datetime(y, m, 1).strftime("%B"), d, y)


# Deterministic sidecar (`<version>.data.json`) FORMAT VERSION — the v2 pipeline
# (data.json + prose.json + release-notes-render.py) keys change-detection on the whole
# data.json dict. Bump when the data.json schema changes.
_DATA_JSON_FORMAT_VERSION = 3

_PREVIEW_KEY_STAGE = {
    "Release Candidate": "rc", "Preview": "p", "Alpha": "a", "Beta": "b",
}


def _preview_key(core, label):
    # type: (str, str) -> str
    """Unique, stable short key for a preview/RC milestone.

    Core-qualified so previews rolled up from different lines never collide, and
    keyed on the label's own stem so parallel experimental trains (an ``svg`` and a
    ``gpu`` preview of the same core) stay distinct:

        ('4.148.0', 'Release Candidate 1') -> '4.148.0-rc1'
        ('3.118.0', 'Preview 1')           -> '3.118.0-p1'
        ('1.53.2',  'Gpu 1')               -> '1.53.2-gpu1'
        ('1.53.2',  'Svg 1')               -> '1.53.2-svg1'

    Known stages get a short abbreviation (rc/p/a/b); any other stem uses a slug of
    itself rather than a shared fallback, so distinct nonstandard stems never merge.
    """
    parts = (label or "").rsplit(" ", 1)
    stem = parts[0] if len(parts) == 2 else (label or "")
    num = parts[1] if len(parts) == 2 else ""
    abbrev = _PREVIEW_KEY_STAGE.get(stem) or re.sub(r"[^a-z0-9]", "", stem.lower()) or "m"
    return "{}-{}{}".format(core, abbrev, num).strip().lower()


def _pr_is_community(pr):
    # type: (dict) -> bool
    """Community test for crediting an author (§4.5)."""
    login = (pr.get("author") or {}).get("login")
    return bool(login) and login != "mattleibow" and not _is_bot_login(login)


def build_data_json(prs, metadata):
    # type: (list[dict], dict) -> dict
    """Emit the deterministic facts a release page is built from (v2 pipeline).

    This is the machine-owned half of the split introduced to stop the polish
    agent owning page structure: everything here is fact (PRs, tags, roster,
    previews, banner date, links, breaking sources). The agent reads it and
    writes only prose (`prose.json`); ``release-notes-render.py`` assembles the page.

    Reuses the exact helpers the data.json builder uses (``_pr_category`` tags,
    ``_contributor_roster``, ``bucket_prs_by_milestone``, ``_release_date_display``)
    so the two emitters can never disagree about the facts.
    """
    version = metadata["version"]
    status = metadata["status"]
    family = metadata.get("family", "skiasharp")
    pkg = metadata.get("package", "SkiaSharp")
    nuget = "https://www.nuget.org/packages/{}".format(pkg)

    released = _release_date_display(version) if status == "stable" else None

    # Banner facts — the renderer owns the shape, we own date + links.
    if status == "stable":
        kind, nuget_url = "stable", "{}/{}".format(nuget, version)
        preview_nuget = None
    elif status == "preview" or metadata.get("superseded_by"):
        kind, nuget_url = "preview", None
        preview_nuget = "{}/{}-preview".format(nuget, version)
    elif status == "unreleased":
        kind, nuget_url, preview_nuget = "unreleased", None, None
    else:
        kind, nuget_url, preview_nuget = status, "{}/{}".format(nuget, version), None
    if family == "harfbuzzsharp":
        kind = "harfbuzz"
    banner = {
        "kind": kind,
        "date": released,
        "nuget_url": nuget_url,
        "preview_nuget_url": preview_nuget,
        "github_release_url": (
            "https://github.com/mono/SkiaSharp/releases/tag/v{}".format(version)
            if status == "stable" else None),
    }
    # HarfBuzz never releases on its own — it ships inside a SkiaSharp release
    # (spec §1.5), so it has no date or tag of its own. Anchor the banner to the
    # introducing SkiaSharp release: carry `ships_with` (for the "Ships with
    # SkiaSharp X" banner) and point the GitHub-release link at the SkiaSharp tag.
    if family == "harfbuzzsharp":
        ships = metadata.get("ships_with") or {}
        banner["date"] = None
        banner["ships_with"] = ships
        banner["github_release_url"] = (
            "https://github.com/mono/SkiaSharp/releases/tag/v{}".format(ships["version"])
            if ships.get("version") and status == "stable" else None)

    # Flat PR map + community flag (renderer derives ❤️ credit from this).
    pr_map = {}
    for pr in prs:
        num = pr.get("number")
        if not num:
            continue
        pr_map[str(num)] = {
            "url": pr.get("url", ""),
            "title": pr.get("title", ""),
            "author": (pr.get("author") or {}).get("login"),
            "community": _pr_is_community(pr),
            "tag": pr.get("category", "product"),
        }

    contributors = [
        {"login": login,
         "url": "https://github.com/{}".format(login),
         "prs": nums}
        for login, nums in _contributor_roster(prs)
    ]

    previews = []
    for b in metadata.get("pr_buckets") or []:
        m = b.get("milestone") or {}
        label = m.get("label")
        if not label or not m.get("tag"):
            continue  # skip the synthetic leftover/unreleased bucket
        core = m.get("version") or ((_parse_tag(m.get("tag")) or {}).get("core") or "")
        previews.append({
            "key": _preview_key(core, label),
            "label": label,
            "date": _friendly_date(m.get("date")),
            "changelog_url": m.get("compare_url"),
            "prs": [pr.get("number") for pr in b.get("prs") or [] if pr.get("number")],
        })
    # Preview keys are the handle prose maps summaries onto, so they must be unique
    # within a page. Fail loudly rather than let two previews collide and silently
    # share (or drop) a summary — see the `previews` data.json field in the spec.
    _pk = [p["key"] for p in previews]
    _dup = sorted({k for k in _pk if _pk.count(k) > 1})
    if _dup:
        raise ValueError(
            "duplicate preview keys for {}: {} — previews {}".format(
                metadata.get("version"), _dup,
                [(p["label"], p["key"]) for p in previews]))

    # Breaking-change *sources* the agent turns into prose: the API breaking diff
    # (signature removals) and the manual notes sidecar (behavioural breaks that
    # no diff can detect). We point at them AND record their content sha256, so
    # editing a companion changes data.json — which is the whole change-detection
    # key (§4.6). The agent reads the referenced files and summarises them.
    companions = metadata.get("companions") or {}
    breaking_candidates = []
    if companions.get("breaking"):
        bc = companions["breaking"]
        for p in bc.get("paths", []):
            breaking_candidates.append(
                {"source": "api-breaking-diff", "path": p,
                 "sha256": bc.get("sha256", ""), "prs": []})
    if companions.get("notes"):
        nc = companions["notes"]
        breaking_candidates.append(
            {"source": "notes-sidecar", "path": nc.get("path"),
             "sha256": nc.get("sha256", ""), "prs": []})

    supersedes = []
    for s in metadata.get("supersedes") or []:
        supersedes.append({
            "version": s,
            "href": "{}.md".format(s),
            "note": "Rolls up preview-only work that was never released as "
                    "stable — those changes are included cumulatively below.",
        })
    superseded_by = None
    if metadata.get("superseded_by"):
        sb = metadata["superseded_by"]
        superseded_by = {
            "version": sb, "href": "{}.md".format(sb),
            "note": "Never released as stable — these changes rolled up into {}.".format(sb),
        }

    # The "API changes" banner links BOTH families' diffs for the co-release: a
    # SkiaSharp page shows its own diff + the co-shipped HarfBuzz diff (from the
    # co-release map), and a HarfBuzz page shows its own diff + the SkiaSharp
    # release it ships within. Own-family first.
    api_links = []
    if family == "harfbuzzsharp":
        if metadata.get("api_diff_link"):
            api_links.append({"label": "HarfBuzzSharp API diff",
                              "href": metadata["api_diff_link"]})
        ships = metadata.get("ships_with") or {}
        # Only when this HarfBuzz line is published (its own diff exists) does the
        # canonical SkiaSharp release's diff folder exist too.
        if metadata.get("api_diff_link") and ships.get("version"):
            api_links.append({"label": "SkiaSharp API diff",
                              "href": "../{}/index.md".format(ships["version"])})
    else:
        if metadata.get("api_diff_link"):
            api_links.append({"label": "SkiaSharp API diff",
                              "href": metadata["api_diff_link"]})
        hb = metadata.get("harfbuzz")
        if hb and hb.get("api_diff_link"):
            api_links.append({"label": "HarfBuzzSharp API diff",
                              "href": hb["api_diff_link"]})

    tallies = {
        "product": sum(1 for p in prs if p.get("category") == "product"),
        "mixed": sum(1 for p in prs if p.get("category") == "mixed"),
        "internal": sum(1 for p in prs if p.get("category") == "internal"),
    }

    return {
        "format": _DATA_JSON_FORMAT_VERSION,
        "version": version,
        "family": family,
        "status": status,
        "banner": banner,
        "harfbuzz": metadata.get("harfbuzz"),
        "supersedes": supersedes,
        "superseded_by": superseded_by,
        "api_links": api_links,
        "tallies": tallies,
        "breaking_candidates": breaking_candidates,
        "contributors": contributors,
        "previews": previews,
        "prs": pr_map,
    }


def _sources_dir(page_path):
    # type: (object) -> Path
    """The ``_sources/`` folder holding a page's inputs (spec §4.6).

    Rendered pages sit at the top of their family dir (``releases/<v>.md`` or
    ``releases/harfbuzzsharp/<v>.md``); everything the maintainer or the pipeline
    *feeds* the renderer — data.json, prose.json, and the manual notes.md sidecar
    — lives one level down in a sibling ``_sources/`` so the page list stays clean
    and every input for a page is in one place. Non-page files never collide with
    the ``*.md`` page globs, which are all non-recursive.
    """
    return Path(str(page_path)).parent / "_sources"


def _data_json_path(page_path):
    # type: (object) -> Path
    """The committed data.json for a page: ``_sources/<stem>.data.json``."""
    p = Path(str(page_path))
    return _sources_dir(p) / (p.stem + ".data.json")


def _prose_json_path(page_path):
    # type: (object) -> Path
    """The agent-authored prose for a page: ``_sources/<stem>.prose.json``."""
    p = Path(str(page_path))
    return _sources_dir(p) / (p.stem + ".prose.json")


def _prune_page_and_sources(page_path):
    # type: (Path) -> None
    """Remove a page and the generated inputs it owns in ``_sources/``.

    Called when a page is pruned (an in-flight line stopped being a head). The
    manual ``notes.md`` sidecar is deliberately left in place — it is human-owned,
    and an orphan is surfaced by ``warn_orphan_notes_sidecars`` rather than deleted.
    """
    page_path = Path(str(page_path))
    if page_path.exists():
        page_path.unlink()
    for gen in (_data_json_path(page_path), _prose_json_path(page_path)):
        if gen.exists():
            gen.unlink()


def _data_json_unchanged(data_path, new_data):
    # type: (Path, dict) -> bool
    """True when the committed data.json equals the freshly-computed facts.

    data.json is the change-detection key (§4.6): it has no timestamp, so an
    identical run yields an identical dict and the page is skipped. Any change to
    the PRs, roster, previews, links, or a companion's folded sha256 flips it and
    the page is re-polished. A missing or unparseable file counts as changed.
    """
    if not data_path.exists():
        return False
    try:
        old = json.loads(data_path.read_text())
    except (ValueError, OSError):
        return False
    return old == new_data


# ── page-set discovery (shared by release-notes-index.py + release-notes-render.py) ──────


def get_version_files():
    # type: () -> tuple[list[str], list[str]]
    """List version strings from existing markdown files.
    
    Returns (versions, next_versions) where next_versions are the
    base versions that have a -unreleased.md file.
    """
    versions = []
    next_versions = []
    for f in RELEASES_DIR.iterdir():
        if (f.suffix == ".md" and f.name != "index.md"
                and not f.name.endswith(".notes.md")):
            stem = f.stem
            if stem.endswith("-unreleased"):
                next_versions.append(stem[:-11])  # strip "-unreleased"
            else:
                versions.append(stem)
    versions.sort(key=version_key, reverse=True)
    next_versions.sort(key=version_key, reverse=True)
    return versions, next_versions


def cadence_milestones():
    # type: () -> tuple[int, int, int]
    """(next_major, current_milestone, next_milestone) for the release cadence.

    The SkiaSharp minor number IS the Chrome/Skia milestone. The current
    milestone is the highest one on the newest major line; the next is
    current + 1 (the one we cut next).

    Derived from the ``_sources/*.data.json`` files, NOT the rendered ``.md``:
    release-notes-index.py (which fetches the two Chrome schedules into index.json) runs in
    the Prepare phase *before* release-notes-render.py creates the ``.md`` pages, while
    release-notes-render.py (which lays out the cadence timeline offline and asserts both
    schedules are present) runs after. A ``.md``-based set would make the two
    phases disagree during a scoped regen — release-notes-index.py would fetch m(N-1)/mN
    while the just-regenerated page needs m(N+1) — leaving index.json missing a
    schedule the render then demands (the old "run prepare twice" symptom). The
    data.json set is written by release-notes-data.py *before* release-notes-index.py and is identical
    in both phases, so both sides always agree on the in-flight pair.
    """
    src = RELEASES_DIR / "_sources"
    keys = []  # type: list[tuple]
    if src.is_dir():
        for f in src.iterdir():
            if not f.name.endswith(".data.json"):
                continue
            stem = f.name[:-len(".data.json")]
            if stem.endswith("-unreleased"):
                stem = stem[:-len("-unreleased")]
            k = version_key(stem)
            if len(k) >= 2:
                keys.append(k)
    next_major = max((k[0] for k in keys), default=4)
    line_ms = [k[1] for k in keys if k[0] == next_major]
    cur_ms = max(line_ms) if line_ms else 1
    return next_major, cur_ms, cur_ms + 1


# ── orphan-sidecar warnings ─────────────────────────────────────────


def warn_orphan_notes_sidecars():
    # type: () -> list[str]
    """Warn about ``*.notes.md`` sidecars with no matching hub page (spec §3.7).

    A manual additions sidecar attaches to a page by sharing its stem: it is
    ``_sources/<stem>.notes.md`` beside the page's other inputs, one level down
    from the ``<stem>.md`` hub page. A sidecar whose ``<stem>.md`` hub page does
    not exist (neither a released ``<line>.md`` nor an in-flight
    ``<line>-unreleased.md``, since either would be the stem) is a maintainer typo
    — Python **warns** and ignores it, writing nothing on its behalf. Call this at
    the end of a generation run, once every page that will exist is on disk.

    Checks both families: SkiaSharp sidecars under ``releases/_sources/`` and
    HarfBuzz sidecars under ``releases/harfbuzzsharp/_sources/``. The hub page
    lives one directory UP from the sidecar. Returns the orphan paths (for tests);
    the side effect is the warning log.
    """
    orphans = []  # type: list[str]
    for base_dir in (RELEASES_DIR, RELEASES_DIR / "harfbuzzsharp"):
        src_dir = base_dir / "_sources"
        if not src_dir.is_dir():
            continue
        for f in sorted(src_dir.iterdir()):
            if not f.is_file() or not f.name.endswith(".notes.md"):
                continue
            stem = f.name[:-len(".notes.md")]
            if not (base_dir / "{}.md".format(stem)).is_file():
                log("WARNING: orphan manual notes sidecar {} has no matching "
                    "page {}.md — ignoring it (spec §3.7). Did you mean a "
                    "different stem?".format(f.name, stem))
                orphans.append(str(f))
    return orphans


# ── Commands ─────────────────────────────────────────────────────────


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
        versioned branch wins across a full run; see _canonical_branches_by_version).

      * A HEAD branch (``main`` or servicing ``release/X.Y.x``) renders the
        UNRELEASED ``{version}-unreleased.md`` — a SMALL DELTA from the last
        release to the head (what may ship next), NOT a rollup.

    The two coexist for an in-flight version (e.g. 4.150.0.md from
    release/4.150.0-preview.1 AND 4.150.0-unreleased.md from main). They never
    collide (distinct filenames); the stale-head prune (release-notes-index.py records the
    live-head set, release-notes-render.py --all deletes the rest) only removes an
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


def _write_page(branch, all_branches, verbose=False, force=False,
                min_core=None, max_core=None):
    # type: (str, list[str], bool, bool, Optional[str]) -> Optional[str]
    """Generate one release page's data from a branch. Returns its page path, or None.

    Resolves the diff range, status and supersession links, then writes the page's
    ``_sources/<stem>.data.json`` facts unless the committed data.json already
    encodes an identical dict (idempotent — data.json is the change key, §4.6). It
    NEVER writes the ``.md`` (release-notes-render.py does). Returns the page path (added to
    the Files-to-polish list), or None when the page was skipped (unchanged), pruned
    (empty unreleased delta), or the diff range could not be determined.

    ``min_core``/``max_core`` (inclusive ``(maj, min, patch, sub)`` tuples from
    ``_core_tuple``) bound the run to a version RANGE, so the back-catalogue can
    be regenerated in chunks (or a single version when they are equal).
    """
    try:
        from_ref, to_ref, version = determine_diff_range(branch)
    except (RuntimeError, subprocess.CalledProcessError) as e:
        log("  WARNING: Could not determine diff range for {}: {}".format(
            branch, e), file=sys.stderr)
        return None

    if min_core is not None or max_core is not None:
        vc = _core_tuple(version)
        if min_core is not None and vc < min_core:
            log("  Skipping {} (below --min-version).".format(version))
            return None
        if max_core is not None and vc > max_core:
            log("  Skipping {} (above --max-version).".format(version))
            return None

    # History floor (spec §1.4): below the configured floor we do not regenerate
    # the page at all. Its already-committed page (and API-diff folder) stay as
    # they are — this is a performance skip of the obsolete back-catalogue, not a
    # delete. No floor configured => never triggers.
    if is_below_history_floor(version):
        log("  Skipping {} (below history floor {}).".format(
            version, history_floor()))
        return None

    from_display = _removeprefix(from_ref, "origin/")
    to_display = _removeprefix(to_ref, "origin/")
    if re.match(r"^[0-9a-f]{7,}$", from_display):
        from_display = from_display[:12]
    diff_range_str = "{}..{}".format(from_display, to_display)

    status, superseded_by, supersedes = _compute_page_status(branch, version)

    if verbose:
        log("Branch: {}".format(branch))
        log("Version: {}".format(version))
        log("Status: {}".format(status))
        if superseded_by:
            log("Superseded by: {}".format(superseded_by))
        if supersedes:
            log("Supersedes: {}".format(", ".join(supersedes)))

    prs = get_prs_from_diff(from_ref, to_ref)
    log("  Found {} PR(s), diff: {}".format(len(prs), diff_range_str))

    output_path = RELEASES_DIR / _page_filename(branch, version)

    # An UNRELEASED head page (main / release/X.Y.x) with an EMPTY delta has
    # nothing unreleased — the head is at the last release. Don't emit an empty
    # "what may ship next" page; prune any stale one so the released {v}.md
    # stands alone. Released pages are always written (a shipped version with no
    # changes since its base is still a real, if empty, release record).
    is_head = (branch == "main") or branch.endswith(".x")
    if is_head and not prs:
        if output_path.exists():
            _prune_page_and_sources(output_path)
            log("  Removed empty {} (nothing unreleased)".format(output_path))
        return None

    # Deterministic, script-owned API-changes facts (spec §2.2/§4.4), computed
    # BEFORE the unchanged check so the link's presence is part of the content
    # key (§4.6) — a page that newly gains a <line>/ folder is rewritten to inject
    # the link. A RELEASED page is an emitted line (§1.4) with a sibling
    # <line>/index.md API-diff index (§3.3); unreleased head deltas are not
    # emitted lines, so they get no folder and no link. The HarfBuzz cross-link
    # comes from the co-release map sidecar (§3.6).
    api_diff_link = None
    if not is_head and (RELEASES_DIR / version).is_dir():
        api_diff_link = "{}/index.md".format(version)
    # HarfBuzzSharp ships INSIDE this SkiaSharp release (spec §1.5): fold its
    # co-release info into this page's data.json so it renders as a "HarfBuzzSharp
    # X.Y.Z" section here — there is no separate HarfBuzz family/page. The version
    # + API-diff link come from the co-release map; the HarfBuzz-specific changes
    # are the PRs in this same window that touched HarfBuzz paths (a subset of the
    # page's PRs, so their details already live in the shared PR map).
    harfbuzz = None
    hb_line = load_co_release_map().get(version)
    if not is_head and hb_line:
        hb_prs = get_prs_from_diff(from_ref, to_ref, paths=HARFBUZZ_PATHSPECS)
        harfbuzz = {
            "version": hb_line,
            "api_diff_link": "harfbuzzsharp/{}/index.md".format(hb_line),
            "prs": [p["number"] for p in hb_prs if p.get("number")],
        }

    # Companion files (spec §3.7/§4.7): the manual additions sidecar (keyed by the
    # page STEM) and the API breaking-diff (under the line's <version>/ folder).
    # Their content hashes are folded into data.json (build_data_json), so a
    # companion-only edit changes data.json and re-polishes just this page (§4.6).
    stem = _page_filename(branch, version)[:-len(".md")]
    notes_comp = load_notes_sidecar(stem, RELEASES_DIR)
    breaking_comp = (load_breaking_companions(version, RELEASES_DIR)
                     if not is_head else None)

    # Resolve the true GitHub handles (API, cached in pr-authors.json). This is
    # needed to build data.json, and it's cheap in steady state — only genuinely
    # new PRs miss the cache and hit the network; every old version is a cache hit.
    resolve_pr_authors(prs)
    resolve_skia_links(prs)

    # Enumerate the preview/rc milestones this page rolls up (regression R3), so
    # the trailing "## Preview N (date)" sections render. The lower bound is the
    # diff base, so a page naturally includes a skipped predecessor minor's
    # previews (the 4.148 page lists the 4.147 previews).
    base_version = None
    if from_display.startswith("release/"):
        base_version = version_from_branch(from_display)
    elif re.match(r"^\d+\.\d+\.\d+", from_display):
        base_version = from_display
    else:
        # from_display is a bare commit SHA — happens when versions.json
        # compare_to resolved to a `v<compare_to>` TAG (no release/* branch). The
        # base core is then exactly that compare_to value; recover it so the
        # milestone window stays bounded.
        ce = _versions_config_lookup(version)
        if ce and ce.get("compare_to"):
            base_version = ce["compare_to"]
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
    if api_diff_link:
        metadata["api_diff_link"] = api_diff_link
    if harfbuzz:
        metadata["harfbuzz"] = harfbuzz
    companions = {}  # type: dict
    if notes_comp:
        companions["notes"] = notes_comp
    if api_diff_link:
        companions["apidiff"] = {"path": api_diff_link}
    if breaking_comp:
        companions["breaking"] = breaking_comp
    if companions:
        metadata["companions"] = companions

    # Build the deterministic facts and compare to the committed data.json — that
    # IS the change-detection key (§4.6). data.json carries no timestamp, so an
    # identical run produces a byte-identical dict and the page is skipped. The
    # generator NEVER writes the .md — release-notes-render.py produces it from
    # data.json + prose.json during Polish.
    data = build_data_json(prs, metadata)
    data_path = _data_json_path(output_path)
    if not force and _data_json_unchanged(data_path, data):
        log("  Skipping {} (unchanged)".format(output_path))
        return None

    data_path.parent.mkdir(parents=True, exist_ok=True)
    data_path.write_text(json.dumps(data, indent=2) + "\n")
    log("  Wrote {} ({} PRs)".format(data_path, len(prs)))
    return str(output_path)


# ── Main ─────────────────────────────────────────────────────────────


def cmd_generate(force=False, polish_list_path=None,
                 min_core=None, max_core=None):
    # type: (bool, str, Optional[tuple], Optional[tuple]) -> None
    """Regenerate release pages for every branch (main + all release/*).

    The default (and only) generation mode: it iterates over every known branch
    and regenerates its raw PR data, but only WRITES files whose content actually
    changed (same PR count AND same diff range == no write), so the "Files to
    polish" output lists only files that genuinely changed and the AI never
    re-polishes pages that are already up to date. ``--force`` overrides the skip;
    ``min_core``/``max_core`` bound the run to a version range (set them equal to
    regenerate a single version).

    Superseded versions (e.g. 4.147, which was skipped for 4.148) are still
    generated — they keep their own page with a "superseded by" label. The
    supersede marker only affects which version is used as a *baseline* when
    diffing others (handled in determine_diff_range / _is_valid_stable_base),
    never whether a page is produced.
    """
    log("Fetching remote branches...")
    try:
        run(["git", "fetch", "origin", "--unshallow", "--quiet"], check=False)
        run(["git", "fetch", "origin",
             "refs/heads/release/*:refs/remotes/origin/release/*",
             "refs/heads/main:refs/remotes/origin/main",
             "--quiet"], check=True)
    except subprocess.CalledProcessError:
        log("ERROR: git fetch failed.")
        sys.exit(1)

    all_branches = list_remote_release_branches()
    if not all_branches:
        log("ERROR: No release branches found after fetch.")
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
            log("WARNING: Cannot determine main version, skipping main.")
            continue

        log("\n--- Processing: {} ---".format(branch))
        path = _write_page(branch, all_branches, force=force,
                           min_core=min_core, max_core=max_core)
        if path:
            files_to_polish.append(path)
            processed_count += 1
        else:
            skipped_count += 1

    # HarfBuzzSharp is NOT a separate family: it ships inside its SkiaSharp release
    # (spec §1.5), so each SkiaSharp page's data.json carries a `harfbuzz` block and
    # renders a "HarfBuzzSharp X.Y.Z" section. There is no separate HarfBuzz pass,
    # so a version-scoped run naturally covers HarfBuzz too.

    # Flag any manual notes sidecar whose hub page is missing (a maintainer
    # typo, spec §3.7).
    warn_orphan_notes_sidecars()

    # Print summary for the AI agent
    log("")
    log("Processed: {}, Skipped/unchanged: {}".format(
        processed_count, skipped_count))
    write_polish_list(files_to_polish, polish_list_path)

def main():
    parser = argparse.ArgumentParser(
        description="Fetch SkiaSharp release data for the website and emit each "
                    "changed version's data.json + page.",
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog=(
            "Examples:\n"
            "  %(prog)s                                     "
            "Regenerate every version (skip unchanged)\n"
            "  %(prog)s --force                             "
            "Rewrite every version, even unchanged ones\n"
            "  %(prog)s --min-version 4.147.0 --max-version 4.151.0   "
            "Only the 4.x range\n"
            "  %(prog)s --min-version 4.148.0 --max-version 4.148.0   "
            "Just one version\n"
        ),
    )
    parser.add_argument(
        "--force", action="store_true",
        help="Rewrite data.json even when unchanged (e.g. to "
             "re-render the whole back-catalogue after a format or skill change)")
    parser.add_argument(
        "--polish-list", metavar="FILE", default=None,
        help="Write the 'Files to polish' list to FILE (one repo-relative path "
             "per line; empty file = nothing changed). Defaults to "
             "output/files-to-polish.txt.")
    parser.add_argument(
        "--min-version", metavar="CORE", default=None,
        help="Lower bound (inclusive), e.g. '3.116.0'. Versions below it are left "
             "untouched. Combine with --max-version to regenerate a range, or set "
             "both equal to regenerate a single version.")
    parser.add_argument(
        "--max-version", metavar="CORE", default=None,
        help="Upper bound (inclusive), e.g. '4.148.0'. Versions above it are left "
             "untouched.")

    args = parser.parse_args()

    min_core = _core_tuple(args.min_version) if args.min_version else None
    max_core = _core_tuple(args.max_version) if args.max_version else None
    cmd_generate(force=args.force, polish_list_path=args.polish_list,
                 min_core=min_core, max_core=max_core)


if __name__ == "__main__":
    main()
