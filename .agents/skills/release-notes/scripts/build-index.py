#!/usr/bin/env python3
"""Build the release-notes TOC.yml and index.md from the FINAL rendered pages.

    build-index.py

The release-notes pipeline is three scripts, one job each:

    generate-release-notes.py  -> <version>.data.json   (facts, per version)
    render-notes.py            -> <version>.md           (the pages)
    build-index.py             -> TOC.yml + index.md      (this script)

TOC.yml and index.md are AGGREGATES of the whole page set — they list and link
every version page — so they must be built LAST, after every <version>.md exists
on disk. Running this as a separate final step (instead of inside the generator)
is why a brand-new version can't go missing from the TOC: the generator writes a
version's data.json, the renderer writes its .md, and only then does this script
scan the finished pages.

It reuses the generator's small, stable low-level helpers (git/version parsing)
so there is one source of truth for those; everything TOC/index-specific lives
here.
"""

from __future__ import annotations

import importlib.util
import json
import re
import sys
import urllib.request
from collections import defaultdict
from pathlib import Path

# ── reuse the generator's shared low-level helpers (one source of truth) ─────
_GEN = Path(__file__).with_name("generate-release-notes.py")
_spec = importlib.util.spec_from_file_location("_rn_generate", str(_GEN))
_gen = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(_gen)

run = _gen.run
log = _gen.log
version_key = _gen.version_key
minor_group = _gen.minor_group
get_upcoming_version = _gen.get_upcoming_version
get_version_from_remote_branch = _gen.get_version_from_remote_branch
list_remote_release_branches = _gen.list_remote_release_branches
RELEASES_DIR = _gen.RELEASES_DIR
VERSIONS_JSON_PATH = _gen.VERSIONS_JSON_PATH
CHROME_SCHEDULE_URL = _gen.CHROME_SCHEDULE_URL
_MONTH_ABBR = _gen._MONTH_ABBR

# Module-level cache for load_support_config (its own, not the generator's).
_SUPPORT_CONFIG = None


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
            _gen._prune_page_and_sources(f)
            removed.append(str(f))
    return removed

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

def format_schedule_date(iso):
    # type: (str) -> str
    """``2026-06-03T00:00:00`` -> ``Jun 3`` (no leading zero)."""
    _, month, day = iso[:10].split("-")
    return "{} {}".format(_MONTH_ABBR[int(month) - 1], int(day))

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


def main():
    cmd_update_toc()


if __name__ == "__main__":
    main()
