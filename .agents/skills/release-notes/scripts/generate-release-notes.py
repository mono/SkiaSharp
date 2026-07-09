#!/usr/bin/env python3
"""
Fetch SkiaSharp release data and manage the website release notes structure.

READ FIRST: documentation/dev/release-notes-and-api-diffs.md is the behavior
SPEC for this script (and the sibling API-diff cake target). Change the spec
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
Polish phase STOPS and reports; a maintainer fixes the script here.

Output streams (the Prepare-phase contract)
--------------------------------------------
This generator runs VERBOSE: all progress and diagnostics — "Processing…", "Found N
PRs", "Skipping (unchanged)", warnings, errors, and the final list of pages to
polish — stream to STDERR via ``log()``, so a CI job log shows the work (and any
disk/timeout failure) as it happens (spec §2.2/§2.3). Nothing is printed to STDOUT.

The machine-readable result the Polish phase consumes — the list of pages whose raw
data changed — is ALWAYS written to a file: ``output/files-to-polish.txt`` by
default, or the path given to ``--polish-list``. It is a plain list, one
repo-relative path per line; an empty file means nothing changed. Because the list
lives in a file (not a stream), verbose progress can flow freely (spec §2.3).

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
    # Regenerate every branch (main + all release/*); skip unchanged files
    python3 generate-release-notes.py

    # Rewrite every page, even unchanged ones (after a format/skill change)
    python3 generate-release-notes.py --force

    # Bound to a version range — or a single version when min == max
    python3 generate-release-notes.py --min-version 4.147.0 --max-version 4.151.0
    python3 generate-release-notes.py --min-version 4.148.0 --max-version 4.148.0

    # Regenerate TOC.yml and index.md only
    python3 generate-release-notes.py --update-toc

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


def load_support_config():
    # type: () -> dict
    """Load the SkiaSharp support config from versions.json (spec §3.5).

    SkiaSharp ships NuGet packages on two release paths (not a multi-tier channel
    product), so the top-level ``support`` block is two lists of ``major.minor``
    line cores (the SkiaSharp minor IS the Chrome/Skia milestone):

      * ``stable``  — the supported stable line(s): the current Chrome Stable
        milestone, or the Chrome Extended-stable milestone during the promotion
        gap (a preview about to go stable).
      * ``preview`` — the in-flight preview/RC line(s): the Chrome Beta milestone,
        or newer when previewing ahead in Dev/Canary.

    Either field may be given as a single string or a list. Returns a normalized
    dict carrying the raw lists plus a derived ``supported`` set (their union) and
    a ``channels`` map (line -> "Stable"/"Preview" label). A missing/empty block
    yields an empty ``supported`` set, so callers fall back to the legacy "every
    3.x+ line is top-level/supported" behavior.
    """
    global _SUPPORT_CONFIG
    if _SUPPORT_CONFIG is not None:
        return _SUPPORT_CONFIG

    def _as_lines(value):
        # type: (object) -> list[str]
        if value is None:
            return []
        if isinstance(value, str):
            return [value]
        if isinstance(value, list):
            return [str(v) for v in value]
        raise ValueError(
            "versions.json: 'support' list fields must be a string or array "
            "(spec §3.5); got %s" % type(value).__name__)

    stable = []  # type: list[str]
    preview = []  # type: list[str]
    if VERSIONS_JSON_PATH.exists():
        with open(VERSIONS_JSON_PATH) as f:
            data = json.load(f)
        block = data.get("support", {}) or {}
        if not isinstance(block, dict):
            raise ValueError(
                "versions.json: 'support' must be an object (spec §3.5); got %s"
                % type(block).__name__)
        stable = _as_lines(block.get("stable"))
        preview = _as_lines(block.get("preview"))
    supported = set()  # type: set[str]
    channels = {}  # type: dict[str, str]
    for line in stable:
        supported.add(line)
        channels.setdefault(line, "Stable")
    for line in preview:
        supported.add(line)
        channels.setdefault(line, "Preview")
    _SUPPORT_CONFIG = {
        "stable": stable,
        "preview": preview,
        "supported": supported,
        "channels": channels,
    }
    return _SUPPORT_CONFIG


# History floor (spec §1.4) — an optional per-family minimum line core below which
# NO page is regenerated and NO API diff is emitted, so a full ``--all`` run skips
# the obsolete back-catalogue (e.g. every 1.x/2.x line) it would otherwise rebuild
# from the NuGet feed on each run. It is a PERFORMANCE floor, not a delete: pages
# and API-diff folders already committed below the floor are left untouched (the
# Cake engine likewise skips clearing them), so history stays intact — it is simply
# not rebuilt. Absent/empty ``history_floor`` block => no floor (legacy behavior:
# every line is regenerated). Read from the top-level ``history_floor`` block in
# versions.json, keyed by family (spec §1.5): ``{"skiasharp": "3.0.0"}``.
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


def classify_support_tier(group, support=None):
    # type: (str, Optional[dict]) -> str
    """Classify a minor group ("3.119") into a TOC/index support tier (spec §3.5).

    Returns one of:

      * ``"supported"`` — a stable or preview line (spec §3.5), rendered
        prominently at the top level.
      * ``"obsolete"`` — a 1.x or 2.x line, folded into "Obsolete Versions".
      * ``"unsupported"`` — every other 3.x+ line, folded into "Out of Support
        Versions".

    With no ``support`` block configured (empty ``supported`` set) every 3.x+ line
    is treated as supported, preserving the legacy flat layout.
    """
    if support is None:
        support = load_support_config()
    if int(group.split(".")[0]) < 3:
        return "obsolete"
    supported = support.get("supported") or set()
    if not supported:
        return "supported"
    return "supported" if group in supported else "unsupported"


# Co-release map sidecar (spec §3.6), written by the Cake API-diff engine and
# read here to emit the deterministic SkiaSharp-page -> HarfBuzz-folder link. It is
# the ONLY thing that crosses from the API-diff engine into this engine
# (spec §2.2). Absent sidecar (e.g. Cake not yet run) -> no HarfBuzz link, no error.
_CO_RELEASE_MAP = None  # type: Optional[dict]

CO_RELEASE_MAP_PATH = RELEASES_DIR / "co-release-map.json"


def load_co_release_map():
    # type: () -> dict
    """Load the co-release map sidecar (cached) as ``{skia_line: entry}``.

    Each entry carries ``hb_line`` and ``hb_link`` (spec §3.6). Returns an empty
    map when the sidecar is missing — the HarfBuzz link is simply omitted in that
    case (the script owns the link, never the AI; spec §4.4).
    """
    global _CO_RELEASE_MAP
    if _CO_RELEASE_MAP is not None:
        return _CO_RELEASE_MAP
    mapping = {}  # type: dict
    if CO_RELEASE_MAP_PATH.exists():
        with open(CO_RELEASE_MAP_PATH) as f:
            data = json.load(f)
        if not isinstance(data, list):
            raise ValueError(
                "co-release-map.json: expected a JSON array (spec §3.6); got %s"
                % type(data).__name__)
        for entry in data:
            line = entry.get("skia_line")
            if line:
                mapping[line] = entry
    _CO_RELEASE_MAP = mapping
    return _CO_RELEASE_MAP


def harfbuzz_lines_from_map():
    # type: () -> list[dict]
    """Invert the co-release map into the HarfBuzz family line set (spec §4.5).

    The map records, per SkiaSharp line, the HarfBuzz line it co-ships (§3.6).
    The HarfBuzz family is line-driven exactly as the SkiaSharp family is
    branch-driven: grouping the SkiaSharp lines by their ``hb_line`` yields one
    entry per HarfBuzz line — so the page set equals the line set, just like
    SkiaSharp. Each entry carries:

      * ``hb_line``        — the HarfBuzz line (full granularity); its page/folder key.
      * ``skia_lines``     — every SkiaSharp line that co-ships it, ascending.
      * ``canonical_skia`` — the EARLIEST (introducing) SkiaSharp line; the page is
                             dated/back-linked by that release and its SkiaSharp
                             range is the HarfBuzz git window (§1.5/§4.5).
      * ``hb_link``        — the folder-index link recorded by the map (§3.6).

    Ordered by ``hb_line`` ascending. Empty when the sidecar is absent — no map,
    no HarfBuzz pages (the engine simply has nothing to emit for that family).
    """
    groups = {}  # type: dict[str, dict]
    for skia_line, entry in load_co_release_map().items():
        hb_line = entry.get("hb_line")
        if not hb_line:
            continue
        g = groups.setdefault(hb_line, {
            "hb_line": hb_line,
            "skia_lines": [],
            "hb_link": entry.get("hb_link"),
        })
        g["skia_lines"].append(skia_line)
    result = []  # type: list[dict]
    for hb_line in sorted(groups, key=version_key):
        g = groups[hb_line]
        g["skia_lines"].sort(key=version_key)
        g["canonical_skia"] = g["skia_lines"][0]
        result.append(g)
    return result


def _sha256_bytes(data):
    # type: (bytes) -> str
    """``sha256:<hex>`` digest of raw bytes (companion-file content key, §4.7)."""
    return "sha256:" + hashlib.sha256(data).hexdigest()


def load_notes_sidecar(stem, base_dir):
    # type: (str, Path) -> Optional[dict]
    """The maintainer-authored manual additions sidecar (spec §3.7).

    A ``<stem>.notes.md`` co-located with the hub page: freeform Markdown a human
    injects to survive re-polish. Returns ``{'path': <page-relative>, 'sha256':
    <hash>}`` when present, else None. Only the BYTES are hashed — Python never
    parses the content (the Polish AI reads it, §4.7). The path is page-relative
    (same directory as the hub), so the AI resolves it straight from the page.
    """
    notes_path = base_dir / "{}.notes.md".format(stem)
    if not notes_path.is_file():
        return None
    return {"path": "{}.notes.md".format(stem),
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


def _api_signature(metadata):
    # type: (dict) -> str
    """Compact, deterministic signature of a page's script-owned API-changes line.

    The API-changes line (§4.4) is part of the content key (§4.6): a page that
    newly gains or loses an API-diff folder, or whose HarfBuzz co-release mapping
    or canonical SkiaSharp back-link changes, must be rewritten so the link is
    injected/dropped. This collapses those facts into one string compared by
    ``_is_content_unchanged``:

      * ``self=<link>``  — this line's own API-diff folder index (when it exists).
      * ``hb=<hb_line>`` — the co-shipped HarfBuzz line (SkiaSharp page only).
      * ``ships=<skia>`` — the canonical introducing SkiaSharp release (HarfBuzz page).

    Empty when the page carries no API-changes line at all. Including it is what
    backfills the link across historical pages on the first run after a folder
    appears (§4.6).
    """
    parts = []
    if metadata.get("api_diff_link"):
        parts.append("self={}".format(metadata["api_diff_link"]))
    hb = metadata.get("harfbuzz")
    if hb and hb.get("hb_line"):
        parts.append("hb={}".format(hb["hb_line"]))
    sw = metadata.get("ships_with")
    if sw and sw.get("version"):
        parts.append("ships={}".format(sw["version"]))
    return ";".join(parts)


def _is_content_unchanged(output_path, new_prs_count, new_diff_range,
                          new_status=None, new_superseded_by=None,
                          new_supersedes=None, new_api_sig=None,
                          new_notes_hash=None, new_breaking_hash=None):
    # type: (Path, int, str, Optional[str], Optional[str], Optional[list[str]], Optional[str], Optional[str], Optional[str]) -> bool
    """Check whether the existing file already encodes identical raw data.

    Compares the fields the script controls: PR count, diff range, the
    supersession metadata (status + the ``superseded:`` / ``supersedes:``
    markers), and the script-owned API-changes link signature (§4.6). The
    supersession fields must be part of this check because toggling a version's
    supersession in versions.json can change ONLY the page banner without
    touching the diff range — when a preview line is newly marked superseded its
    OWN diff/PR set is unchanged (supersession only affects it as a baseline for
    LATER versions), but its page must still gain a "Superseded by" banner. The
    API signature is included for the same reason: a page that newly gains (or
    loses) an API-diff folder, or whose HarfBuzz mapping changes, must be
    rewritten to inject (or drop) the API-changes line even when its PRs are
    identical. Returns True only when every tracked field matches, so those
    metadata-only changes still force a rewrite. The companion-file hashes
    (``notes:`` and ``breaking:``, §4.7) are compared for the same reason: editing
    the manual additions sidecar or a change in the line's breaking diff must
    re-polish the page even when its PR set is unchanged.

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

    # format: <n> — the raw-data FORMAT VERSION (§4.6). An existing page written by
    # an older generator either lacks this line or carries a lower number; either way
    # its raw-data block predates the current structure/instructions, so it must be
    # rewritten even though its PR set is unchanged. Absent => treat as 0 (mismatch).
    m_fmt = re.search(r"^\s*format:\s*(\d+)", content, re.MULTILINE)
    existing_fmt = int(m_fmt.group(1)) if m_fmt else 0
    if existing_fmt != _RAWDATA_FORMAT_VERSION:
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

    # api: <signature> — the script-owned API-changes link (§4.4/§4.6). Compare
    # only when the caller supplies the new signature. An existing page written
    # before this field existed has no ``api:`` line; treat that as "" so a page
    # that should now carry a link is rewritten to backfill it.
    if new_api_sig is not None:
        m_api = re.search(r"^\s*api:\s*(.*)$", content, re.MULTILINE)
        existing_api = m_api.group(1).strip() if m_api else ""
        if existing_api != (new_api_sig or ""):
            return False

    # notes: <hash> / breaking: <hash> — the companion-file content key (§4.7).
    # These are emitted only when the companion exists, so an absent line means
    # "no companion"; treat it as "" so a page that newly GAINS a sidecar or a
    # breaking diff is rewritten to backfill the manifest (§4.6). NOTE: use
    # ``[ \t]*`` (never ``\s*``) around the value — in MULTILINE mode ``\s*``
    # would span the newline and swallow the next line for an empty value.
    if new_notes_hash is not None:
        m_notes = re.search(r"^[ \t]*notes:[ \t]*(\S+)", content, re.MULTILINE)
        existing_notes = m_notes.group(1) if m_notes else ""
        if existing_notes != (new_notes_hash or ""):
            return False
    if new_breaking_hash is not None:
        m_brk = re.search(r"^[ \t]*breaking:[ \t]*(\S+)", content, re.MULTILINE)
        existing_brk = m_brk.group(1) if m_brk else ""
        if existing_brk != (new_breaking_hash or ""):
            return False

    return True


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


def format_schedule_date(iso):
    # type: (str) -> str
    """``2026-06-03T00:00:00`` -> ``Jun 3`` (no leading zero)."""
    _, month, day = iso[:10].split("-")
    return "{} {}".format(_MONTH_ABBR[int(month) - 1], int(day))


def fetch_chrome_schedule(milestone, timeout=8):
    # type: (int, int) -> dict
    """Return real Chrome phase dates for ``milestone`` from Chromium Dash.

    Returns ``{"beta", "early_stable", "stable_cut", "stable"}`` -> ISO date
    string. The schedule is required, so any problem (offline, timeout,
    HTTP/JSON error, missing milestone or missing phase date) raises
    ``RuntimeError`` and fails generation loudly rather than emitting a
    placeholder — a retry once connectivity is back recreates the page.
    """
    url = CHROME_SCHEDULE_URL.format(milestone)
    try:
        req = urllib.request.Request(url, headers={"User-Agent": "SkiaSharp-docs"})
        with urllib.request.urlopen(req, timeout=timeout) as resp:
            status = getattr(resp, "status", 200)
            if status != 200:
                raise RuntimeError("HTTP {}".format(status))
            payload = json.loads(resp.read().decode("utf-8"))
    except Exception as e:
        raise RuntimeError(
            "Could not fetch Chrome schedule for m{} from {}: {}".format(
                milestone, url, e))
    mstones = payload.get("mstones") or []
    if not mstones:
        raise RuntimeError(
            "Chrome schedule for m{} returned no milestone data".format(milestone))
    ms = mstones[0]
    fields = {
        "beta": "earliest_beta",
        "early_stable": "early_stable",
        "stable_cut": "stable_cut",
        "stable": "stable_date",
    }
    schedule = {}
    for phase, key in fields.items():
        value = ms.get(key)
        if not value:
            raise RuntimeError(
                "Chrome schedule for m{} is missing '{}'".format(milestone, key))
        schedule[phase] = value
    return schedule


def render_cadence_timeline(cur_ms, next_ms, cur_base, next_base):
    # type: (int, int, str, str) -> list[str]
    """Build the schedule-timeline table for the release cadence section.

    Pulls the *real* Chrome schedule for the two milestones currently in flight
    (``cur_ms`` / ``next_ms``) from Chromium Dash and renders the four cadence
    phases for each, sorted by actual date. If the schedule cannot be fetched,
    ``fetch_chrome_schedule`` raises and generation fails (by design) — there is
    no placeholder fallback.
    """
    # (cadence phase, schedule key, package suffix appended to the base)
    phases = [
        ("Beta Promotion", "beta", ".0-preview.1"),
        ("Early Stable", "early_stable", ".0-preview.2"),
        ("Stable Cut", "stable_cut", ".0-rc.1"),
        ("Stable Release", "stable", ".0"),
    ]

    cur_sched = fetch_chrome_schedule(cur_ms)
    next_sched = fetch_chrome_schedule(next_ms)

    events = []  # type: list[tuple[str, str, str]]
    for ms_num, base, sched in (
            (cur_ms, cur_base, cur_sched), (next_ms, next_base, next_sched)):
        for label, key, suffix in phases:
            iso = sched[key]
            events.append((iso,
                           "m{} {}".format(ms_num, label),
                           "`{}{}`".format(base, suffix)))
    events.sort(key=lambda e: e[0])

    header = (
        "**Schedule for the two milestones currently in flight "
        "(m{} and m{}), from the "
        "[Chromium release schedule](https://chromiumdash.appspot.com/schedule):**"
        .format(cur_ms, next_ms))
    rows = ["| Date | Event | Package |", "|------|-------|---------|"]
    rows += ["| {} | {} | {} |".format(format_schedule_date(iso), ev, pkg)
             for iso, ev, pkg in events]
    return [header, ""] + rows


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

_AUTHOR_CACHE_PATH = Path(".agents/skills/release-notes/scripts/pr-authors.json")
_GRAPHQL_BATCH = 50


def load_author_cache():
    # type: () -> dict
    """Load the PR-number -> GitHub-login cache ((.agents/skills/release-notes/scripts/pr-authors.json).

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
      2. cache ((.agents/skills/release-notes/scripts/pr-authors.json) — previously resolved from the API.
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
# The original hand-authored pages (PR #3763) and TEMPLATE.md end with a list of
# per-preview sections, e.g.:
#
#     ## Preview 3 (February 5, 2026)
#     [Full Changelog](.../compare/v3.119.2-preview.2.3...v3.119.2-preview.3.1)
#
# SKILL.md rules 9/10 still mandate this ("Rollup at top … Previews are minimal:
# one sentence + Full Changelog link each, at the bottom"). When pages were migrated
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
#              image, SDK pin). Polish guesses from the title/context in the raw-data
#              block (it does not open the PR) — surface a behaviour change, drop pure infra.
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
# already skips these when authoring, but the raw data still records them (they open
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
    """
    out = run(["git", "log", "--no-renames", "--name-only",
               "--format=%x1e%H", "{}..{}".format(from_ref, to_ref)])
    files_by = {}  # type: dict
    cur = None
    for line in out.split("\n"):
        if line.startswith("\x1e"):
            cur = line[1:].strip()
            files_by[cur] = set()
        elif line.strip() and cur is not None:
            files_by[cur].add(line.strip())
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
    # GitHub login cannot be proven (see resolve_pr_authors / format_pr_list).
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
    # The community marker invites Polish to credit the author, so it must NOT appear
    # on bot or maintainer PRs (§4.5): the maintainer is never credited, and bots are
    # never credited — leaving it on a bot line makes Polish emit a ❤️ @bot bullet.
    is_community = login and login != "mattleibow" and not _is_bot_login(login)
    community_str = " [community ✨]" if is_community else ""
    skia_pr = pr.get("skiaPr")
    skia_str = " (skia: mono/skia#{})".format(skia_pr) if skia_pr else ""
    by = "by @{}".format(login) if login else "by {}".format(name)
    # Deterministic product / mixed / internal tag (§4.4). Polish writes up [product],
    # collapses [internal] into one trailing line, and for [mixed] takes a best guess
    # from the title/context here (no need to open the PR) — surface a behaviour change,
    # drop pure build/infra.
    cat = pr.get("category", "product")
    tag = "[{}] ".format(cat)
    return "{}{} {} in {}{}{}".format(tag, title, by, url, community_str, skia_str)


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


# Raw-data block FORMAT VERSION — part of the content key (§4.6). `_is_content_unchanged`
# compares this against the `format:` line in the existing page; a mismatch (or an older
# page with no `format:` line) forces a rewrite even when the PR set and diff range are
# unchanged. BUMP THIS whenever the raw-data block's structure or the Polish instructions
# embedded in it change materially, so a single `--all` run rolls the new format out to
# every otherwise-quiet page (e.g. a stable line with no new PRs) instead of leaving it
# stranded on the old format until its next PR lands.
#   1 — original (binary [product]/[internal] tag, no contributor roster)
#   2 — three-way [product]/[mixed]/[internal] tag + authoritative contributor roster
#   3 — v2 pipeline: page is RENDERED from <version>.data.json + <version>.slots.json
#       by render-notes.py (no in-page raw-data block). Bumping to 3 forces every
#       page off the legacy raw-data format onto the rendered v2 format on first run.
_RAWDATA_FORMAT_VERSION = 3

# Deterministic sidecar (`<version>.data.json`) FORMAT VERSION — the v2 pipeline
# (data.json + slots.json + render-notes.py) consumes this instead of parsing the
# raw-data HTML comment. Bump when the data.json schema changes.
_DATA_JSON_FORMAT_VERSION = 3

# The canonical, closed set of category headings a page may use. The renderer
# (render-notes.py) enforces that every slots.json category heading is one of
# these, so the section taxonomy is owned here (data) not in prose.
RELEASE_CATEGORIES = [
    "Engine", "API Surface", "Bug Fixes",
    "Lifecycle & Internals", "Platform", "Security",
]

_PREVIEW_KEY_STAGE = {
    "Release Candidate": "rc", "Preview": "p", "Alpha": "a", "Beta": "b",
}


def _preview_key(label):
    # type: (str) -> str
    """Stable short key for a preview/RC label ('Release Candidate 1' -> 'rc1')."""
    parts = (label or "").rsplit(" ", 1)
    stem = parts[0] if len(parts) == 2 else label
    num = parts[1] if len(parts) == 2 else ""
    return "{}{}".format(_PREVIEW_KEY_STAGE.get(stem, "m"), num).strip().lower()


def _pr_is_community(pr):
    # type: (dict) -> bool
    """Same community test used for the raw-data marker (§4.5)."""
    login = (pr.get("author") or {}).get("login")
    return bool(login) and login != "mattleibow" and not _is_bot_login(login)


def build_data_json(prs, metadata):
    # type: (list[dict], dict) -> dict
    """Emit the deterministic facts a release page is built from (v2 pipeline).

    This is the machine-owned half of the split introduced to stop the polish
    agent owning page structure: everything here is fact (PRs, tags, roster,
    previews, banner date, links, breaking sources). The agent reads it and
    writes only prose (`slots.json`); ``render-notes.py`` assembles the page.

    Reuses the exact helpers the raw-data block uses (``_pr_category`` tags,
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
        previews.append({
            "key": _preview_key(label),
            "label": label,
            "date": _friendly_date(m.get("date")),
            "changelog_url": m.get("compare_url"),
            "prs": [pr.get("number") for pr in b.get("prs") or [] if pr.get("number")],
        })

    # Breaking-change *sources* the agent turns into prose: the API breaking diff
    # (signature removals) and the manual notes sidecar (behavioural breaks that
    # no diff can detect). We point at them; the agent reads and summarises.
    companions = metadata.get("companions") or {}
    breaking_candidates = []
    if companions.get("breaking"):
        for p in companions["breaking"].get("paths", []):
            breaking_candidates.append(
                {"source": "api-breaking-diff", "path": p, "prs": []})
    if companions.get("notes"):
        breaking_candidates.append(
            {"source": "notes-sidecar", "path": companions["notes"].get("path"),
             "prs": []})

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

    api_links = []
    if metadata.get("api_diff_link"):
        api_links.append({"label": "SkiaSharp API diff",
                          "href": metadata["api_diff_link"]})
    hb = metadata.get("harfbuzz")
    if hb and hb.get("link"):
        api_links.append({"label": "HarfBuzzSharp {}".format(hb.get("version", "")),
                          "href": hb["link"]})

    landmarks = _derive_landmarks(prs, bool(companions.get("breaking")), status)

    tallies = {
        "product": sum(1 for p in prs if p.get("category") == "product"),
        "mixed": sum(1 for p in prs if p.get("category") == "mixed"),
        "internal": sum(1 for p in prs if p.get("category") == "internal"),
    }

    return {
        "format": _DATA_JSON_FORMAT_VERSION,
        "version": version,
        "family": family,
        "package": pkg,
        "title": ("HarfBuzzSharp {}".format(version)
                  if family == "harfbuzzsharp" else "Version {}".format(version)),
        "status": status,
        "banner": banner,
        "supersedes": supersedes,
        "superseded_by": superseded_by,
        "api_links": api_links,
        "breaking_none_text": ("*None in this preview line.*"
                               if status in ("preview", "unreleased")
                               else "*None in this release.*"),
        "allowed_categories": RELEASE_CATEGORIES,
        "landmarks": landmarks,
        "tallies": tallies,
        "breaking_candidates": breaking_candidates,
        "contributors": contributors,
        "previews": previews,
        "prs": pr_map,
    }


def _derive_landmarks(prs, has_breaking, status):
    # type: (list[dict], bool, str) -> list[str]
    """A short, deterministic hint list for the Highlights slot.

    Deliberately NOT the full PR list — Highlights is written from these
    landmarks, so the agent structurally cannot enumerate every change.
    """
    marks = []
    for pr in prs:
        title = pr.get("title", "")
        if re.search(r"[Uu]pdate to Skia milestone|\[skia\].*milestone", title):
            marks.append(title.replace("[skia] ", "").strip())
            break
    if has_breaking:
        marks.append("Behavioural or API breaking changes — see Breaking Changes")
    # A couple of the biggest product PRs by title as generic seeds.
    for pr in prs:
        if pr.get("category") == "product" and len(marks) < 5:
            t = pr.get("title", "")
            if t and not t.startswith("[skia]") and t not in marks:
                marks.append(t)
    return marks[:5]


def _write_data_json_sidecar(page_path, prs, metadata):
    # type: (object, list[dict], dict) -> None
    """Write the deterministic ``<version>.data.json`` next to the page.

    Additive: does not change the page itself. The v2 render pipeline
    (render-notes.py) consumes this; the legacy raw-data block still ships in the
    page for the current polish path until the workflow is switched over.
    """
    try:
        data = build_data_json(prs, metadata)
    except Exception as exc:  # never let the sidecar break page generation
        log("  WARN: data.json sidecar skipped ({})".format(exc))
        return
    from pathlib import Path as _P
    p = _P(str(page_path))
    sidecar = p.with_suffix(".data.json")
    sidecar.write_text(json.dumps(data, indent=2) + "\n")
    log("  Wrote {}".format(sidecar))


def format_pr_list(prs, metadata):
    # type: (list[dict], dict) -> str
    """Format the PR list as markdown with raw data in an HTML comment.

    The raw PR data is preserved inside an HTML comment block so that AI
    can regenerate polished notes from it at any time. Below the comment,
    a skeleton placeholder is written for AI to fill in.
    """
    now = datetime.now(timezone.utc).strftime("%Y-%m-%dT%H:%M:%SZ")
    version = metadata["version"]
    api_sig = _api_signature(metadata)

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
        "  format:    {}".format(_RAWDATA_FORMAT_VERSION),
        "  diff:      {}..{}".format(metadata["from"], metadata["to"]),
        "  prs:       {}".format(len(prs)),
        "  tags:      {} [product] · {} [mixed] · {} [internal] of {} PRs".format(
            sum(1 for p in prs if p.get("category") == "product"),
            sum(1 for p in prs if p.get("category") == "mixed"),
            sum(1 for p in prs if p.get("category") == "internal"),
            len(prs)),
        "  legend:    [product] = write up · [internal] = DROP (roll into one trailing "
        "\"Plus various … internal tooling improvements.\" line) · [mixed] = build/infra: "
        "guess from the title here — surface a behaviour change, otherwise drop",
        "  api:       {}".format(api_sig),
        "",
    ]

    superseded_by = metadata.get("superseded_by")
    supersedes = metadata.get("supersedes")
    # Release date (deterministic, from the vX.Y.Z tag) — reused both in the raw-data
    # `released:` field AND to scaffold the stable banner below, so the AI never has to
    # supply the date itself.
    released_display = (_release_date_display(version)
                        if metadata.get("status") == "stable" else None)
    meta_extra = []
    if released_display:
        meta_extra.append(
            "  released:   {} (use verbatim in the banner)".format(released_display))
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

    # Companion files (spec §4.7): record each present companion's content hash
    # (notes/breaking — the content key, §4.6) and a human-readable manifest of
    # page-relative paths the Polish AI opens and SUMMARIZES. Content is
    # referenced, never embedded, so the block stays small.
    companions = metadata.get("companions") or {}
    notes = companions.get("notes")
    apidiff = companions.get("apidiff")
    breaking = companions.get("breaking")
    comp_block = []  # type: list[str]
    if notes:
        comp_block.append("  notes:     {}".format(notes["sha256"]))
    if breaking:
        comp_block.append("  breaking:  {}".format(breaking["sha256"]))
    if notes or apidiff or breaking:
        comp_block.append("")
        comp_block.append(
            "  companions (open and read these during polish, §4.7 — "
            "summarize, don't dump):")
        if notes:
            comp_block.append("    - notes:    {}  (manual additions)".format(
                notes["path"]))
        if apidiff:
            comp_block.append("    - api diff: {}  (index of all per-assembly diffs; flags breaking)".format(
                apidiff["path"]))
        if breaking:
            for p in breaking["paths"]:
                comp_block.append("    - breaking: {}  (breaking signatures)".format(p))
    # Splice just before the trailing blank separator (the last element of
    # `lines`) so the hashes sit with the metadata and the manifest leads into
    # the PR list.
    insert_at = len(lines) - 1
    for extra in comp_block:
        lines.insert(insert_at, extra)
        insert_at += 1

    # Authoritative contributor roster (§4.5): the exact set of external, non-bot
    # authors to credit. Rendered here so Polish builds the credits table from a
    # deterministic list instead of reconstructing it from body prose (which kept
    # dropping real contributors). One table row per entry, none omitted.
    roster = _contributor_roster(prs)
    if roster:
        roster_block = [
            "  contributors: {} external contributor(s) — render EXACTLY one credits-table "
            "row each (§4.5), never omit one; summarize each one's PRs (found by "
            "\"by @login\" below) and link the PR numbers:".format(len(roster)),
        ]
        for login, nums in roster:
            roster_block.append("    @{}  ({} PR{}): {}".format(
                login, len(nums), "" if len(nums) == 1 else "s",
                " ".join("#{}".format(n) for n in nums)))
        insert_at = len(lines) - 1
        for extra in roster_block:
            lines.insert(insert_at, extra)
            insert_at += 1

    if not prs:
        lines.append("  *No changes found.*")
    else:
        # Each PR line is tagged [product] / [mixed] / [internal] (§4.4). Remind the
        # AI in context so keep/inspect/drop is a lexical filter, not a re-judgment.
        lines.append("  Each PR is tagged [product] (write it up, categorized), "
                     "[mixed] (build/infra — guess from the title: surface a behaviour")
        lines.append('  change, otherwise drop) or [internal] (DROP — never a bullet each; '
                     'roll ALL into one trailing "Plus various … internal tooling')
        lines.append('  improvements." line, or omit it when there are none).')
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
    family = metadata.get("family", "skiasharp")
    pkg = metadata.get("package", "SkiaSharp")
    nuget = "https://www.nuget.org/packages/{}".format(pkg)
    # Sibling supersession links resolve within the page's own family directory:
    # SkiaSharp pages at releases/, HarfBuzz pages at releases/harfbuzzsharp/.
    base_dir = (RELEASES_DIR / "harfbuzzsharp"
                if family == "harfbuzzsharp" else RELEASES_DIR)
    if family == "harfbuzzsharp":
        lines.append("# HarfBuzzSharp {}".format(version))
    else:
        lines.append("# Version {}".format(version))
    lines.append("")

    # Status-appropriate header
    status = metadata["status"]
    ships_with = metadata.get("ships_with") or {}
    if superseded_by:
        if (base_dir / "{}.md".format(superseded_by)).exists():
            sup_link = "[{sup}]({sup}.md)".format(sup=superseded_by)
        elif (base_dir / "{}-unreleased.md".format(superseded_by)).exists():
            sup_link = "[{sup}]({sup}-unreleased.md)".format(sup=superseded_by)
        else:
            sup_link = superseded_by
        lines.append(
            "> **Preview only** · Superseded by "
            "{sup_link} · Never released as stable — these changes "
            "rolled up into {sup} "
            "· [NuGet]({nuget}/{ver}-preview)".format(
                sup_link=sup_link, sup=superseded_by, ver=version, nuget=nuget))
    elif family == "harfbuzzsharp":
        # HarfBuzz never releases on its own — it ships inside a SkiaSharp
        # release (spec §1.5). The banner is dated/anchored by the canonical
        # (introducing) SkiaSharp line; the AI prepends a {theme} (TEMPLATE.md).
        skia = ships_with.get("version", "")
        skia_link = ships_with.get("link", "")
        if status == "unreleased":
            lines.append(
                "> **Upcoming release** · In development · Ships with the "
                "upcoming SkiaSharp {} · Not yet available on NuGet".format(skia))
        else:
            lines.append(
                "> Ships with [SkiaSharp {s}]({sl}) · [NuGet]({nuget}/{ver}) · "
                "[GitHub Release](https://github.com/mono/SkiaSharp/releases/"
                "tag/v{s})".format(s=skia, sl=skia_link, nuget=nuget, ver=version))
    elif status == "unreleased":
        lines.append(
            "> **Upcoming release** · In development "
            "· Not yet available on NuGet")
    elif status == "preview":
        lines.append(
            "> **Preview release** · Preview only "
            "· [NuGet]({nuget}/{ver}-preview)".format(nuget=nuget, ver=version))
    else:
        # Stable page: scaffold the ENTIRE banner deterministically — the release
        # date (from the tag) and both links are script-owned, so the AI only has to
        # replace the <THEME> token with a 2-4 word editorial phrase. This is the one
        # banner the AI used to build from scratch (theme + date + links), which made
        # it the one that occasionally shipped as a bare "> [NuGet]" skeleton; giving
        # it the same scaffold as every other status removes that failure mode, and a
        # leftover literal <THEME> is a greppable self-review failure (unlike a bare
        # but valid-looking NuGet link). See rule 7 / TEMPLATE / spec §4.4.
        released_seg = ("Released {} · ".format(released_display)
                        if released_display else "")
        lines.append(
            "> **<THEME>** · {rel}[NuGet]({nuget}/{ver}) · "
            "[GitHub Release](https://github.com/mono/SkiaSharp/releases/tag/"
            "v{ver})".format(rel=released_seg, nuget=nuget, ver=version))
    lines.append("")

    # Back-link making the supersede relationship two-way: this release rolls up
    # one or more preview-only versions that never shipped stable. The superseded
    # page already links forward ("Superseded by X"); this points back to it.
    if supersedes:
        sup_links = []
        for sup in supersedes:
            if (base_dir / "{}.md".format(sup)).exists():
                sup_links.append("[{s}]({s}.md)".format(s=sup))
            elif (base_dir / "{}-unreleased.md".format(sup)).exists():
                sup_links.append("[{s}]({s}-unreleased.md)".format(s=sup))
            else:
                sup_links.append(sup)
        lines.append(
            "> **Supersedes {}** · Rolls up preview-only work that was never "
            "released as stable — those changes are included cumulatively "
            "below.".format(", ".join(sup_links)))
        lines.append("")

    # Deterministic, script-owned API-changes line (spec §2.2/§4.4). Final links
    # the AI must keep verbatim and narrate around — it never writes or edits
    # links itself. The targets differ by family: a SkiaSharp page links its own
    # <line>/index.md (§3.3) and, when the release co-ships HarfBuzz, the HarfBuzz
    # hub page harfbuzzsharp/<hb-line>.md (§1.5/§3.6); a HarfBuzz page links its
    # own <hb-line>/index.md (§3.4) and back at its canonical SkiaSharp release.
    api_diff_link = metadata.get("api_diff_link")
    harfbuzz = metadata.get("harfbuzz")
    if family == "harfbuzzsharp":
        if api_diff_link:
            line = "> **API changes** · [HarfBuzzSharp API diff]({})".format(
                api_diff_link)
            if ships_with.get("version"):
                line += " · Ships with [SkiaSharp {s}]({l})".format(
                    s=ships_with["version"], l=ships_with.get("link", ""))
            lines.append(line)
            lines.append("")
    elif api_diff_link or harfbuzz:
        parts = []
        if api_diff_link:
            parts.append("[SkiaSharp API diff]({})".format(api_diff_link))
        if harfbuzz:
            hb_line = harfbuzz.get("hb_line", "")
            parts.append("[HarfBuzzSharp {hb}](harfbuzzsharp/{hb}.md)".format(
                hb=hb_line))
        lines.append("> **API changes** · " + " · ".join(parts))
        lines.append("")

    ai_lines = [
        "<!-- AI: Use the raw PR data in the comment above to write polished",
        "     release notes here. Follow .agents/skills/release-notes/references/TEMPLATE.md",
        "     for structure and tone.",
    ]
    if metadata.get("status") == "stable":
        ai_lines.append(
            "     BANNER: the `> **<THEME>**` line above is scaffolded with the correct")
        ai_lines.append(
            "     release date and both links — replace ONLY the literal <THEME> token")
        ai_lines.append(
            "     with a 2-4 word editorial phrase (e.g. 'First stable v4 release'). Keep")
        ai_lines.append(
            "     the date and the NuGet/GitHub Release links verbatim; never leave <THEME>.")
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
    if family == "harfbuzzsharp":
        ai_lines.append(
            "     This is a HarfBuzzSharp page: title '# HarfBuzzSharp {}', and the"
            .format(version))
        ai_lines.append(
            "     data is already filtered to HarfBuzz-touching changes — do not")
        ai_lines.append(
            "     add SkiaSharp content. The banner shows it ships with a SkiaSharp")
        ai_lines.append(
            "     release; keep that and the 'API changes' line verbatim.")
    if api_diff_link or harfbuzz:
        ai_lines.append(
            '     Keep the "API changes" links above verbatim (do NOT edit or add')
        ai_lines.append(
            "     links). Narrate around them in prose.")
        if harfbuzz:
            ai_lines.append(
                "     Mention that HarfBuzz {} ships alongside this release and"
                .format(harfbuzz.get("hb_line", "")))
            ai_lines.append(
                "     point readers at its linked page for the binding details.")
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


def get_harfbuzz_version_files():
    # type: () -> tuple[list[str], list[str]]
    """List HarfBuzz hub-page lines under releases/harfbuzzsharp/ (spec §3.4).

    Returns ``(versions, next_versions)``: released ``<hb>.md`` lines and
    in-flight ``<hb>-unreleased.md`` lines, newest-first. Ignores the per-line
    API-diff subfolders and any generated index.md, so only the human hub pages
    feed the TOC/index HarfBuzz section.
    """
    hb_dir = RELEASES_DIR / "harfbuzzsharp"
    versions = []  # type: list[str]
    next_versions = []  # type: list[str]
    if hb_dir.is_dir():
        for f in hb_dir.iterdir():
            if (not f.is_file() or f.suffix != ".md" or f.name == "index.md"
                    or f.name.endswith(".notes.md")):
                continue
            stem = f.stem
            if stem.endswith("-unreleased"):
                next_versions.append(stem[:-len("-unreleased")])
            else:
                versions.append(stem)
    versions.sort(key=version_key, reverse=True)
    next_versions.sort(key=version_key, reverse=True)
    return versions, next_versions


def cleanup_stale_unreleased():
    # type: () -> list[str]
    """Delete ``{version}-unreleased.md`` pages whose line is no longer a head.

    An unreleased page models "what may ship next" for an in-flight LINE, and a
    line is in-flight only while it is the version of an active HEAD — either
    ``main`` (the upcoming version) or a servicing ``release/X.Y.x`` branch. Any
    ``-unreleased`` page whose version is not one of those live heads is an
    orphan and is removed (spec §4.2). This prunes BOTH a page left behind in the
    same minor and one left behind across minors — e.g. once the head advances
    4.148 → 4.150 and no ``release/4.148.x`` servicing branch exists, the stale
    ``4.148.0-unreleased.md`` is removed; if that servicing branch DOES exist the
    page is kept, because 4.148 is still a live (serviced) head.

    Released ``{version}.md`` pages are never touched here — only the unreleased
    deltas. Returns the removed file paths. To avoid clobbering on a degenerate
    checkout, nothing is deleted when no release branches can be enumerated.
    """
    all_branches = list_remote_release_branches()
    if not all_branches:
        return []

    # Live in-flight lines = main's upcoming version + every servicing .x head.
    live = set()  # type: set[str]
    main_version = get_upcoming_version()
    if main_version:
        live.add(main_version)
    for b in all_branches:
        if not b.endswith(".x"):
            continue
        m = re.match(r"release/(\d+)\.(\d+)\.x$", b)
        if not m:
            continue
        svc_version = (get_version_from_remote_branch(b)
                       or "{}.{}.0".format(m.group(1), m.group(2)))
        live.add(svc_version)

    removed = []
    for f in sorted(RELEASES_DIR.iterdir()):
        if (f.suffix != ".md" or not f.stem.endswith("-unreleased")
                or f.name.endswith(".notes.md")):
            continue
        version = f.stem[:-len("-unreleased")]
        if version not in live:
            f.unlink()
            removed.append(str(f))
    return removed


def warn_orphan_notes_sidecars():
    # type: () -> list[str]
    """Warn about ``*.notes.md`` sidecars with no matching hub page (spec §3.7).

    A manual additions sidecar attaches to a page by sharing its stem: it is
    ``<stem>.notes.md`` beside ``<stem>.md``. A sidecar whose ``<stem>.md`` hub
    page does not exist (neither a released ``<line>.md`` nor an in-flight
    ``<line>-unreleased.md``, since either would be the stem) is a maintainer typo
    — Python **warns** and ignores it, writing nothing on its behalf. Call this at
    the end of a generation run, once every page that will exist is on disk.

    Checks both families: SkiaSharp sidecars under ``releases/`` and HarfBuzz
    sidecars under ``releases/harfbuzzsharp/``. Returns the orphan paths (for
    tests); the side effect is the warning log.
    """
    orphans = []  # type: list[str]
    for base_dir in (RELEASES_DIR, RELEASES_DIR / "harfbuzzsharp"):
        if not base_dir.is_dir():
            continue
        for f in sorted(base_dir.iterdir()):
            if not f.is_file() or not f.name.endswith(".notes.md"):
                continue
            stem = f.name[:-len(".notes.md")]
            if not (base_dir / "{}.md".format(stem)).is_file():
                log("WARNING: orphan manual notes sidecar {} has no matching "
                    "page {}.md — ignoring it (spec §3.7). Did you mean a "
                    "different stem?".format(f.name, stem))
                orphans.append(str(f))
    return orphans


def _toc_folded_section(title, groups, stable_groups, unreleased_groups):
    # type: (str, list[str], dict, dict) -> list[str]
    """Render a collapsed parent TOC node nesting its minor groups (spec §3.5).

    Used for the "Out of Support Versions" and "Obsolete Versions" folds: each
    minor group becomes a child node, a single-release minor collapsing to one
    node while a multi-release minor nests its individual patch releases. This
    mirrors the supported top-level layout one level deeper so the sidebar fold
    stays tidy instead of degrading into a flat wall of patch links. Returns the
    YAML lines (empty when ``groups`` is empty).
    """
    out = []  # type: list[str]
    if not groups:
        return out
    head_members = stable_groups.get(groups[0]) or unreleased_groups.get(groups[0])
    head = ("{}.md".format(head_members[0]) if groups[0] in stable_groups
            else "{}-unreleased.md".format(head_members[0]))
    out.append("- name: {}".format(title))
    out.append("  href: {}".format(head))
    out.append("  items:")
    for g in groups:
        stable = stable_groups.get(g, [])
        unreleased = unreleased_groups.get(g, [])
        entries = [(v, True) for v in unreleased] + [(v, False) for v in stable]
        if not entries:
            continue
        entries.sort(key=lambda t: version_key(t[0]), reverse=True)
        g_header = ("{}.md".format(stable[0]) if stable
                    else "{}-unreleased.md".format(unreleased[0]))
        out.append("    - name: Version {}.x".format(g))
        out.append("      href: {}".format(g_header))
        if len(entries) > 1:
            out.append("      items:")
            for v, is_unrel in entries:
                if is_unrel:
                    out.append("        - name: Version {} (Unreleased)".format(v))
                    out.append("          href: {}-unreleased.md".format(v))
                else:
                    out.append("        - name: Version {}".format(v))
                    out.append("          href: {}.md".format(v))
    return out


def generate_toc(versions, next_versions, hb_versions=None, hb_next_versions=None):
    # type: (list[str], list[str], Optional[list[str]], Optional[list[str]]) -> str
    """Generate TOC.yml grouped by major.minor and support tier (spec §3.5).

    SkiaSharp minor groups are split into three tiers by their support status
    (``classify_support_tier``): supported lines (stable / preview) render at the
    top level, while the remaining 3.x+ lines fold under "Out of Support Versions"
    and 1.x/2.x lines fold under "Obsolete Versions".

    Unreleased pages are listed in their minor group even when no stable page
    of that exact version exists yet (e.g. 3.119.5-unreleased before 3.119.5
    ships, or 4.148.0-unreleased before 4.148.0 ships).

    ``hb_versions``/``hb_next_versions`` are the HarfBuzz peer-family lines
    (released and in-flight); when present they render as a sibling "HarfBuzz"
    node whose emitted HarfBuzz lines are grouped into ``HarfBuzzSharp X.Y.x``
    minor subgroups, mirroring the SkiaSharp version groups (spec §3.5).
    """
    stable_groups = defaultdict(list)
    unreleased_groups = defaultdict(list)
    for v in versions:
        stable_groups[minor_group(v)].append(v)
    for v in next_versions:
        unreleased_groups[minor_group(v)].append(v)

    support = load_support_config()
    supported = []
    unsupported = []
    obsolete = []
    for g in sorted(set(stable_groups) | set(unreleased_groups),
                    key=lambda x: version_key(x), reverse=True):
        tier = classify_support_tier(g, support)
        if tier == "obsolete":
            obsolete.append(g)
        elif tier == "unsupported":
            unsupported.append(g)
        else:
            supported.append(g)

    lines = ["- name: Overview", "  href: index.md"]

    for g in supported:
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

    lines.extend(_toc_folded_section(
        "Out of Support Versions", unsupported, stable_groups, unreleased_groups))
    lines.extend(_toc_folded_section(
        "Obsolete Versions", obsolete, stable_groups, unreleased_groups))

    # HarfBuzz peer family — sibling node grouping HarfBuzz lines by minor
    # (spec §3.5), mirroring the SkiaSharp version groups so the node is a tidy
    # set of "HarfBuzzSharp X.Y.x" subgroups instead of one flat list. Hub pages
    # live under harfbuzzsharp/<hb>.md.
    hb_versions = hb_versions or []
    hb_next_versions = hb_next_versions or []
    if hb_versions or hb_next_versions:
        hb_header = ("harfbuzzsharp/{}.md".format(hb_versions[0]) if hb_versions
                     else "harfbuzzsharp/{}-unreleased.md".format(hb_next_versions[0]))
        lines.append("- name: HarfBuzz")
        lines.append("  href: {}".format(hb_header))
        lines.append("  items:")

        hb_stable_groups = defaultdict(list)
        hb_unrel_groups = defaultdict(list)
        for v in hb_versions:
            hb_stable_groups[minor_group(v)].append(v)
        for v in hb_next_versions:
            hb_unrel_groups[minor_group(v)].append(v)

        for g in sorted(set(hb_stable_groups) | set(hb_unrel_groups),
                        key=lambda x: version_key(x), reverse=True):
            g_stable = hb_stable_groups.get(g, [])
            g_unrel = hb_unrel_groups.get(g, [])
            g_header = ("harfbuzzsharp/{}.md".format(g_stable[0]) if g_stable
                        else "harfbuzzsharp/{}-unreleased.md".format(g_unrel[0]))
            lines.append("    - name: HarfBuzzSharp {}.x".format(g))
            lines.append("      href: {}".format(g_header))
            lines.append("      items:")
            g_entries = ([(v, True) for v in g_unrel]
                         + [(v, False) for v in g_stable])
            g_entries.sort(key=lambda t: version_key(t[0]), reverse=True)
            for v, is_unrel in g_entries:
                if is_unrel:
                    lines.append("        - name: HarfBuzzSharp {} (Unreleased)".format(v))
                    lines.append("          href: harfbuzzsharp/{}-unreleased.md".format(v))
                else:
                    lines.append("        - name: HarfBuzzSharp {}".format(v))
                    lines.append("          href: harfbuzzsharp/{}.md".format(v))

    return "\n".join(lines) + "\n"


def generate_index(versions, next_versions, hb_versions=None, hb_next_versions=None):
    # type: (list[str], list[str], Optional[list[str]], Optional[list[str]]) -> str
    """Generate index.md grouped by support tier (spec §3.5).

    When a ``support`` block is configured the page opens with a "Release
    cadence" section (how the 4.x line tracks Chrome's release cycle, the
    versioning scheme, and the schedule reference) followed by a "Support
    overview" — a short lifecycle legend (stable / preview / out of support /
    obsolete) and a table of the currently-supported lines and their latest
    release — so a reader sees what to use at a glance. SkiaSharp lines are then
    split by their support status (``classify_support_tier``): the supported
    lines (stable / preview) are listed prominently, each tagged with its path,
    while the remaining 3.x+ lines and the obsolete 1.x/2.x lines fold into
    collapsed ``<details>`` blocks so the page leads with what is supported.

    Unreleased pages are listed even when no stable page of that exact version
    exists yet. ``hb_versions``/``hb_next_versions`` render a trailing
    "HarfBuzzSharp" section linking the peer-family hub pages (spec §3.5).
    """
    support = load_support_config()
    channels = support.get("channels", {})

    entries = [(v, False) for v in versions] + [(v, True) for v in next_versions]
    minor_map = defaultdict(list)
    for v, is_unrel in entries:
        minor_map[minor_group(v)].append((v, is_unrel))

    supported_groups = []
    unsupported_groups = []
    obsolete_groups = []
    for g in sorted(minor_map.keys(), key=lambda x: version_key(x), reverse=True):
        tier = classify_support_tier(g, support)
        if tier == "obsolete":
            obsolete_groups.append(g)
        elif tier == "unsupported":
            unsupported_groups.append(g)
        else:
            supported_groups.append(g)

    def render_group(g, with_label):
        # type: (str, bool) -> list[str]
        members = sorted(minor_map[g], key=lambda t: version_key(t[0]), reverse=True)
        label = channels.get(g)
        if with_label and label:
            out = ["- **Version {}.x** — {}".format(g, label)]
        else:
            out = ["- **Version {}.x**".format(g)]
        for v, is_unrel in members:
            if is_unrel:
                out.append("  - [Version {} (Unreleased)]({}-unreleased.md)".format(v, v))
            else:
                out.append("  - [Version {}]({}.md)".format(v, v))
        return out

    def details(summary, body):
        # type: (str, list[str]) -> list[str]
        return ["<details>", "<summary>{}</summary>".format(summary), ""] \
            + body + ["", "</details>", ""]

    def latest_link(g):
        # type: (str) -> str
        """Markdown link to the newest page in line ``g`` for the overview table.

        Prefers the newest *released* page; falls back to the newest unreleased
        page when a line has shipped no stable page yet (spec §3.5).
        """
        members = sorted(minor_map[g], key=lambda t: version_key(t[0]), reverse=True)
        released = [(v, u) for v, u in members if not u]
        v, is_unrel = released[0] if released else members[0]
        if is_unrel:
            return "[{} (Unreleased)]({}-unreleased.md)".format(v, v)
        return "[{}]({}.md)".format(v, v)

    # The support block drives the top "what is supported right now" overview.
    # Without it (legacy/empty config) the page keeps the plain flat layout.
    configured = bool(support.get("supported"))

    if configured:
        intro = (
            "Release notes for SkiaSharp. SkiaSharp ships as NuGet packages whose "
            "minor version is the Chrome/Skia milestone it builds on. Two release "
            "lines are supported at a time — a **stable** line for production and a "
            "**preview** line for the milestone currently being stabilized — "
            "mirroring "
            "[Chrome's release channels](https://developer.chrome.com/docs/web-platform/chrome-release-channels) "
            "(stable / extended-stable and beta). Everything else stays published "
            "for reference but is no longer serviced.")
    else:
        intro = "Release notes for all SkiaSharp versions."

    lines = ["# Release Notes", "", intro, ""]

    if configured:
        # Forward-looking version examples derived from what is on disk: the
        # current milestone is the highest one in the major line, and the next is
        # current + 1 (the one we will cut next). Used for both the example
        # timeline (N / N+1) and the "Versioning" examples so they stay ahead of
        # what has already shipped instead of being pinned to a fixed milestone.
        sk_keys = [version_key(v) for v in (list(versions) + list(next_versions))
                   if v and len(version_key(v)) >= 2]
        next_major = max((k[0] for k in sk_keys), default=4)
        line_ms = [k[1] for k in sk_keys if k[0] == next_major]
        cur_ms = max(line_ms) if line_ms else 1
        next_ms = cur_ms + 1
        cur_base = "{}.{}".format(next_major, cur_ms)    # e.g. "4.150"
        next_base = "{}.{}".format(next_major, next_ms)  # e.g. "4.151"
        next_ver = next_base + ".0"
        scheme = "`" + str(next_major) + ".{chrome_milestone}.{patch}`"
        lines.extend([
            "## Release cadence",
            "",
            "SkiaSharp 4.x follows Chrome's release cycle. Each SkiaSharp minor "
            "version corresponds to a Chrome/Skia milestone and progresses through "
            "four phases:",
            "",
            "| Chrome Event | SkiaSharp Release | Purpose |",
            "|---|---|---|",
            "| Beta Promotion | Preview 1 | Merge upstream Skia, ship initial preview |",
            "| Early Stable | Preview 2 | Bug fixes and API additions from preview feedback |",
            "| Stable Cut | RC | Critical bug fixes only, no new features |",
            "| Stable Release | Stable | Ship to NuGet.org, tag and create GitHub Release |",
            "",
        ])
        lines.extend(render_cadence_timeline(cur_ms, next_ms, cur_base, next_base))
        lines.extend([
            "",
            "Two milestones are always in flight — as one enters its RC/stable "
            "phase, the next begins its preview phase.",
            "",
            "> [!NOTE]",
            "> Starting with Chrome 153 (September 2026), Chrome moves from a "
            "4-week to a 3-week release cycle. Because SkiaSharp's cadence is "
            "driven by Chrome's actual schedule events, the phases above will "
            "naturally compress — preview through stable will complete in ~3 weeks "
            "instead of ~4.",
            "",
            "### Versioning",
            "",
            "Packages follow the scheme " + scheme + " — the "
            "middle number **is** the Chrome milestone number. For example, "
            "`" + next_ver + "` ships alongside Chrome " + str(next_ms) + "'s "
            "stable release.",
            "",
            "- Preview: `" + next_ver + "-preview.1`, `" + next_ver + "-preview.2`",
            "- Release candidate: `" + next_ver + "-rc.1`",
            "- Stable: `" + next_ver + "`",
            "",
            "Prerelease suffixes follow "
            "[NuGet semver conventions](https://learn.microsoft.com/nuget/concepts/package-versioning#pre-release-versions).",
            "",
            "### Schedule reference",
            "",
            "The full Chrome release calendar is published at "
            "[Chromium's release schedule](https://chromiumdash.appspot.com/schedule). "
            "SkiaSharp milestones are synced automatically from this schedule — "
            "check the [GitHub milestones](https://github.com/mono/SkiaSharp/milestones) "
            "for upcoming release dates.",
            "",
        ])
        lines.extend([
            "## Support overview",
            "",
            "- **Stable** — the line we recommend for production apps. Tracks "
            "Chrome's Stable / Extended Stable channel.",
            "- **Preview** — prerelease NuGets for the next milestone, so you can "
            "test ahead of its stable release. Tracks Chrome's Beta channel.",
            "- **Out of support** — older 3.x / 4.x lines, still listed below for "
            "reference but no longer serviced.",
            "- **Obsolete** — SkiaSharp 1.x and 2.x, no longer maintained.",
            "",
        ])
        # Currently-supported table: Stable rows first, then Preview; newest line
        # first within each (supported_groups is already version-descending and the
        # sort below is stable).
        channel_order = {"Stable": 0, "Preview": 1}
        table_groups = sorted(
            supported_groups,
            key=lambda g: channel_order.get(channels.get(g), 9))
        lines.append("| Path | Version line | Latest release |")
        lines.append("|------|--------------|----------------|")
        for g in table_groups:
            lines.append("| {} | {}.x | {} |".format(
                channels.get(g, "Supported"), g, latest_link(g)))
        lines.append("")

    if supported_groups:
        lines.extend(["## Supported versions", ""])
        for g in supported_groups:
            lines.extend(render_group(g, with_label=True))
        lines.append("")

    if unsupported_groups:
        lines.extend(["## Out of support", ""])
        lines.append(
            "These SkiaSharp 3.x and 4.x lines are no longer supported. They "
            "remain available for reference.")
        lines.append("")
        body = []  # type: list[str]
        for g in unsupported_groups:
            body.extend(render_group(g, with_label=False))
        lines.extend(details("Show out-of-support releases", body))

    if obsolete_groups:
        lines.extend(["## Obsolete versions", ""])
        lines.append("SkiaSharp 1.x and 2.x are obsolete and no longer maintained.")
        lines.append("")
        body = []
        for g in obsolete_groups:
            body.extend(render_group(g, with_label=False))
        lines.extend(details("Show obsolete releases", body))

    # HarfBuzz peer family (spec §3.5) — its own section, grouped by HB minor.
    hb_versions = hb_versions or []
    hb_next_versions = hb_next_versions or []
    if hb_versions or hb_next_versions:
        lines.extend(["## HarfBuzzSharp", ""])
        hb_entries = ([(v, False) for v in hb_versions]
                      + [(v, True) for v in hb_next_versions])
        hb_minor_map = defaultdict(list)
        for v, is_unrel in hb_entries:
            hb_minor_map[minor_group(v)].append((v, is_unrel))
        for g in sorted(hb_minor_map.keys(),
                        key=lambda x: version_key(x), reverse=True):
            members = sorted(hb_minor_map[g],
                             key=lambda t: version_key(t[0]), reverse=True)
            lines.append("- **HarfBuzzSharp {}.x**".format(g))
            for v, is_unrel in members:
                if is_unrel:
                    lines.append("  - [HarfBuzzSharp {} (Unreleased)](harfbuzzsharp/{}-unreleased.md)".format(v, v))
                else:
                    lines.append("  - [HarfBuzzSharp {}](harfbuzzsharp/{}.md)".format(v, v))
        lines.append("")

    return "\n".join(lines)


# ── Commands ─────────────────────────────────────────────────────────


def cmd_update_toc():
    """Regenerate TOC.yml and index.md (and prune stale unreleased pages)."""
    if not RELEASES_DIR.is_dir():
        log("Error: {} does not exist".format(RELEASES_DIR), file=sys.stderr)
        sys.exit(1)

    for removed in cleanup_stale_unreleased():
        log("Removed stale {}".format(removed))

    versions, next_versions = get_version_files()
    hb_versions, hb_next_versions = get_harfbuzz_version_files()

    (RELEASES_DIR / "TOC.yml").write_text(
        generate_toc(versions, next_versions, hb_versions, hb_next_versions))
    log("Updated {}".format(RELEASES_DIR / "TOC.yml"))

    (RELEASES_DIR / "index.md").write_text(
        generate_index(versions, next_versions, hb_versions, hb_next_versions))
    log("Updated {}".format(RELEASES_DIR / "index.md"))


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


def _write_page(branch, all_branches, verbose=False, force=False,
                min_core=None, max_core=None):
    # type: (str, list[str], bool, bool, Optional[str]) -> Optional[str]
    """Generate one release page from a branch. Returns its path, or None.

    Single code path shared by ``--branch`` and ``--all``: resolves the diff
    range, status and supersession links, then writes
    ``documentation/docfx/releases/{file}`` unless the existing file already
    encodes identical raw data (idempotent). Returns the written path, or None
    when the page was skipped (unchanged) or the diff range could not be
    determined.

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
            output_path.unlink()
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
    harfbuzz = None
    hb = load_co_release_map().get(version)
    if hb and hb.get("hb_line"):
        harfbuzz = hb
    api_sig = _api_signature({"api_diff_link": api_diff_link,
                              "harfbuzz": harfbuzz})

    # Companion files (spec §3.7/§4.7): the manual additions sidecar (keyed by the
    # page STEM so it attaches to this exact page) and the API breaking-diff
    # (under the line's <version>/ folder). Their hashes join the content key so a
    # companion-only edit re-polishes just this page (§4.6). Computed BEFORE the
    # unchanged check for the same backfill reason as api_sig.
    stem = _page_filename(branch, version)[:-len(".md")]
    notes_comp = load_notes_sidecar(stem, RELEASES_DIR)
    breaking_comp = (load_breaking_companions(version, RELEASES_DIR)
                     if not is_head else None)
    notes_hash = notes_comp["sha256"] if notes_comp else ""
    breaking_hash = breaking_comp["sha256"] if breaking_comp else ""

    if not force and _is_content_unchanged(output_path, len(prs), diff_range_str,
                                           status, superseded_by, supersedes,
                                           api_sig, notes_hash, breaking_hash):
        log("  Skipping {} (unchanged)".format(output_path))
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
    else:
        # from_display is a bare commit SHA — happens when versions.json
        # compare_to resolved to a `v<compare_to>` TAG (no release/* branch). The
        # base core is then exactly that compare_to value; recover it so the
        # milestone window stays bounded (bug C: a None base let the window
        # broaden to every preview up to the page version).
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

    # Inject the API-changes facts computed above (before the unchanged check).
    if api_diff_link:
        metadata["api_diff_link"] = api_diff_link
    if harfbuzz:
        metadata["harfbuzz"] = harfbuzz

    # Companion-file manifest for the raw-data block (§4.7). The api-diff index is
    # a companion the AI reads too, listed via api_diff_link (not hashed — its
    # change signal is carried by prs/diff/api, §4.6).
    companions = {}  # type: dict
    if notes_comp:
        companions["notes"] = notes_comp
    if api_diff_link:
        companions["apidiff"] = {"path": api_diff_link}
    if breaking_comp:
        companions["breaking"] = breaking_comp
    if companions:
        metadata["companions"] = companions

    RELEASES_DIR.mkdir(parents=True, exist_ok=True)
    output_path.write_text(format_pr_list(prs, metadata))
    _write_data_json_sidecar(output_path, prs, metadata)
    log("  Wrote {} ({} PRs)".format(output_path, len(prs)))
    return str(output_path)


def _skia_branch_for_line(skia_line, all_branches):
    # type: (str, list[str]) -> Optional[str]
    """Resolve a SkiaSharp line core to the branch that defines its diff range.

    Prefers the canonical versioned branch (the SkiaSharp released page's
    source). When the line is still in-flight — no versioned branch, it exists
    only as a head — falls back to the head carrying it: ``main`` for the
    upcoming version, else the servicing ``release/X.Y.x`` whose version matches.
    Returns None when no branch defines the line. Used by the HarfBuzz family to
    find the SkiaSharp range that introduced a HarfBuzz line (spec §4.5).
    """
    canonical = _canonical_branches_by_version(all_branches).get(skia_line)
    if canonical:
        return canonical
    if skia_line == get_upcoming_version():
        return "main"
    for b in all_branches:
        if not b.endswith(".x"):
            continue
        m = re.match(r"release/(\d+)\.(\d+)\.x$", b)
        if not m:
            continue
        svc = (get_version_from_remote_branch(b)
               or "{}.{}.0".format(m.group(1), m.group(2)))
        if svc == skia_line:
            return b
    return None


def _render_harfbuzz_no_changes(hb_line, canonical_skia, api_diff_link):
    # type: (str, str, Optional[str]) -> str
    """Deterministic *No changes* HarfBuzz page (no AI raw-data block; spec §4.5).

    A published HarfBuzz line whose SkiaSharp window has no HarfBuzz-touching PRs
    (a rebuild, or a bump with no notable PR) still gets a permanent, fully
    rendered page so the line set has no gaps. It is timestamp-free so it stays
    byte-stable across runs (idempotent by text equality) and carries no raw-data
    block, so it is never listed under "Files to polish".
    """
    skia_link = "../{}.md".format(canonical_skia)
    lines = ["# HarfBuzzSharp {}".format(hb_line), ""]
    lines.append(
        "> Ships with [SkiaSharp {s}]({l}) · "
        "[NuGet](https://www.nuget.org/packages/HarfBuzzSharp/{hb}) · "
        "[GitHub Release](https://github.com/mono/SkiaSharp/releases/tag/v{s})"
        .format(s=canonical_skia, l=skia_link, hb=hb_line))
    lines.append("")
    if api_diff_link:
        lines.append(
            "> **API changes** · [HarfBuzzSharp API diff]({a}) · "
            "Ships with [SkiaSharp {s}]({l})".format(
                a=api_diff_link, s=canonical_skia, l=skia_link))
        lines.append("")
    lines.append(
        "No HarfBuzzSharp binding changes shipped in this release — it rebuilds "
        "the same HarfBuzz as the previous line.")
    lines.append("")
    return "\n".join(lines)


def _write_harfbuzz_page(hb, all_branches, force=False):
    # type: (dict, list[str], bool) -> Tuple[Optional[str], Optional[str]]
    """Generate one HarfBuzz family page from a co-release-map line group (§4.5).

    Mirrors ``_write_page`` (the SkiaSharp path) — same diff/PR/milestone/format
    machinery — differing only in line discovery (the inverted map), range
    resolution (the canonical SkiaSharp release's range, filtered to HarfBuzz
    files), and family metadata. All deterministic and script-owned.

    Returns ``(polish_path, owned_filename)``:
      * ``polish_path``    — the page to hand the AI, or None when skipped,
                             written as a deterministic *No changes* page (no AI
                             block), or pruned.
      * ``owned_filename`` — the file this line owns this run (``<hb>.md`` or
                             ``<hb>-unreleased.md``), or None when it owns none
                             (lets the caller prune orphaned ``-unreleased`` pages).
    """
    hb_line = hb["hb_line"]
    canonical_skia = hb["canonical_skia"]
    # History floor (spec §1.4): a HarfBuzz line whose introducing SkiaSharp
    # release is below the SkiaSharp floor — or which is itself below a HarfBuzz
    # floor — is left as committed and not regenerated.
    if (is_below_history_floor(canonical_skia, "skiasharp")
            or is_below_history_floor(hb_line, "harfbuzzsharp")):
        return None, None
    hb_dir = RELEASES_DIR / "harfbuzzsharp"
    # A HarfBuzz line is "published" (released) exactly when the API-diff
    # engine emitted its diff folder; otherwise it is in-flight (spec §3.4/§4.5).
    published = (hb_dir / hb_line).is_dir()

    skia_branch = _skia_branch_for_line(canonical_skia, all_branches)
    if not skia_branch:
        log("  WARNING: no SkiaSharp branch for HarfBuzz {} (introducing line "
              "{}); skipping".format(hb_line, canonical_skia), file=sys.stderr)
        return None, None

    try:
        from_ref, to_ref, _ = determine_diff_range(skia_branch)
    except (RuntimeError, subprocess.CalledProcessError) as e:
        log("  WARNING: could not resolve range for HarfBuzz {} via {}: {}"
              .format(hb_line, skia_branch, e), file=sys.stderr)
        return None, None

    from_display = _removeprefix(from_ref, "origin/")
    to_display = _removeprefix(to_ref, "origin/")
    if re.match(r"^[0-9a-f]{7,}$", from_display):
        from_display = from_display[:12]
    diff_range_str = "{}..{}".format(from_display, to_display)

    # Filter the SkiaSharp range down to HarfBuzz-owned commits (spec §1.5/§4.5).
    prs = get_prs_from_diff(from_ref, to_ref, paths=HARFBUZZ_PATHSPECS)

    owned = ("{}.md".format(hb_line) if published
             else "{}-unreleased.md".format(hb_line))
    output_path = hb_dir / owned
    api_diff_link = "{}/index.md".format(hb_line) if published else None

    # The manual additions sidecar (spec §3.7), keyed by this page's STEM. When a
    # maintainer has written one, it OVERRIDES both No-changes short-circuits
    # below (§4.5): the hand-written content is exactly what needs surfacing, so
    # the line gets a real polished page instead of a prune / deterministic page.
    stem = owned[:-len(".md")]
    notes_comp = load_notes_sidecar(stem, hb_dir)

    # An in-flight HarfBuzz line earns a page ONLY when its introducing SkiaSharp
    # head actually carries HarfBuzz changes (spec §4.5). An empty delta means the
    # head merely rebuilds an already-released HarfBuzz — no page; prune a stale one.
    if not published and not prs and not notes_comp:
        if output_path.exists():
            output_path.unlink()
            log("  Removed empty {} (nothing unreleased)".format(output_path))
        return None, None

    # Published line, no HarfBuzz-touching PRs -> deterministic *No changes* page
    # (no AI block, never polished; spec §4.5). Idempotent by exact text equality.
    if published and not prs and not notes_comp:
        text = _render_harfbuzz_no_changes(hb_line, canonical_skia, api_diff_link)
        if not force and output_path.exists() and output_path.read_text() == text:
            log("  Skipping {} (unchanged, no changes)".format(output_path))
            return None, owned
        hb_dir.mkdir(parents=True, exist_ok=True)
        output_path.write_text(text)
        log("  Wrote {} (no HarfBuzz changes)".format(output_path))
        return None, owned

    status = "stable" if published else "unreleased"
    superseded_by = resolve_superseded_by(hb_line, "harfbuzzsharp")
    if superseded_by:
        status = "preview"
    supersedes = detect_supersedes(hb_line, "harfbuzzsharp")

    metadata = {
        "branch": skia_branch,
        "version": hb_line,
        "status": status,
        "from": from_display,
        "to": to_display,
        "family": "harfbuzzsharp",
        "package": "HarfBuzzSharp",
        # HarfBuzz has no release date of its own — it is anchored to the
        # canonical (introducing) SkiaSharp release (spec §1.5).
        "ships_with": {"version": canonical_skia,
                       "link": "../{}.md".format(canonical_skia)},
    }
    if api_diff_link:
        metadata["api_diff_link"] = api_diff_link
    if superseded_by:
        metadata["superseded_by"] = superseded_by
    if supersedes:
        metadata["supersedes"] = supersedes
    api_sig = _api_signature(metadata)

    # Companion files (spec §3.7/§4.7), same as the SkiaSharp path: the manual
    # additions sidecar (resolved above) plus the API breaking-diff under this
    # HarfBuzz line's folder. Hashes join the content key (§4.6).
    breaking_comp = (load_breaking_companions(hb_line, hb_dir)
                     if published else None)
    notes_hash = notes_comp["sha256"] if notes_comp else ""
    breaking_hash = breaking_comp["sha256"] if breaking_comp else ""

    if not force and _is_content_unchanged(output_path, len(prs), diff_range_str,
                                           status, superseded_by, supersedes,
                                           api_sig, notes_hash, breaking_hash):
        log("  Skipping {} (unchanged)".format(output_path))
        return None, owned

    resolve_pr_authors(prs)
    resolve_skia_links(prs)

    # Preview milestones are the SkiaSharp previews in the window where HarfBuzz
    # work landed (spec §4.5): reuse the SkiaSharp machinery keyed on the
    # canonical SkiaSharp line, then bucket the HarfBuzz-filtered PRs into them.
    base_version = None
    if from_display.startswith("release/"):
        base_version = version_from_branch(from_display)
    elif re.match(r"^\d+\.\d+\.\d+", from_display):
        base_version = from_display
    preview_milestones = collect_preview_milestones(canonical_skia, base_version)
    if preview_milestones:
        metadata["preview_milestones"] = preview_milestones
        metadata["pr_buckets"] = bucket_prs_by_milestone(
            prs, preview_milestones, from_ref)

    # Companion-file manifest for the raw-data block (§4.7).
    companions = {}  # type: dict
    if notes_comp:
        companions["notes"] = notes_comp
    if api_diff_link:
        companions["apidiff"] = {"path": api_diff_link}
    if breaking_comp:
        companions["breaking"] = breaking_comp
    if companions:
        metadata["companions"] = companions

    hb_dir.mkdir(parents=True, exist_ok=True)
    output_path.write_text(format_pr_list(prs, metadata))
    _write_data_json_sidecar(output_path, prs, metadata)
    log("  Wrote {} ({} PRs)".format(output_path, len(prs)))
    return str(output_path), owned


# ── Main ─────────────────────────────────────────────────────────────


def _process_harfbuzz_family(all_branches, force=False):
    # type: (list[str], bool) -> Tuple[list[str], int, int]
    """Generate the HarfBuzz peer-family pages (spec §4.5) and prune orphans.

    Drives one page per HarfBuzz line discovered from the co-release map
    (``harfbuzz_lines_from_map``), with the same machinery as the SkiaSharp
    family. Returns ``(files_to_polish, processed, skipped)``. Released
    ``<hb>.md`` pages are cumulative and never pruned (like SkiaSharp); orphaned
    in-flight ``<hb>-unreleased.md`` pages — a line that has since shipped, or
    whose introducing head moved on — are removed.
    """
    hb_groups = harfbuzz_lines_from_map()
    if not hb_groups:
        return [], 0, 0

    hb_dir = RELEASES_DIR / "harfbuzzsharp"
    files_to_polish = []  # type: list[str]
    processed = 0
    skipped = 0
    valid_unreleased = set()  # type: set[str]

    for hb in hb_groups:
        log("\n--- Processing HarfBuzz: {} (ships with {}) ---".format(
            hb["hb_line"], hb["canonical_skia"]))
        path, owned = _write_harfbuzz_page(hb, all_branches, force=force)
        if owned and owned.endswith("-unreleased.md"):
            valid_unreleased.add(owned)
        if path:
            files_to_polish.append(path)
            processed += 1
        else:
            skipped += 1

    # Prune orphaned in-flight pages: any harfbuzzsharp/<x>-unreleased.md not
    # owned by a current in-flight line (it shipped -> now <x>.md, or its head
    # moved on). Released pages are cumulative and kept (spec §4.5).
    if hb_dir.is_dir():
        for f in sorted(hb_dir.glob("*-unreleased.md")):
            if f.name not in valid_unreleased:
                f.unlink()
                log("Removed stale {}".format(f))

    return files_to_polish, processed, skipped


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

    # HarfBuzz peer family — same pipeline, line-driven from the co-release map
    # (spec §4.5). Runs after the SkiaSharp pass so the canonical SkiaSharp pages
    # its cross-links target already exist this run. Skipped entirely when a run
    # is scoped to a SkiaSharp version subset via --min/--max-version
    # (a scoped validation run wants only the SkiaSharp pages in range).
    if min_core is not None or max_core is not None:
        hb_polish, hb_processed, hb_skipped = [], 0, 0
    else:
        hb_polish, hb_processed, hb_skipped = _process_harfbuzz_family(
            all_branches, force=force)
    files_to_polish.extend(hb_polish)
    processed_count += hb_processed
    skipped_count += hb_skipped

    # Regenerate TOC and index
    cmd_update_toc()

    # Every page that will exist is now on disk — flag any manual notes sidecar
    # whose hub page is missing (a maintainer typo, spec §3.7).
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
            "  %(prog)s --update-toc                        "
            "Only regenerate TOC + index\n"
        ),
    )
    parser.add_argument(
        "--update-toc", action="store_true",
        help="Only regenerate TOC.yml + index.md (nothing else)")
    parser.add_argument(
        "--force", action="store_true",
        help="Rewrite pages even when the raw data is unchanged (e.g. to "
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

    if args.update_toc:
        cmd_update_toc()
    else:
        min_core = _core_tuple(args.min_version) if args.min_version else None
        max_core = _core_tuple(args.max_version) if args.max_version else None
        cmd_generate(force=args.force, polish_list_path=args.polish_list,
                     min_core=min_core, max_core=max_core)


if __name__ == "__main__":
    main()
