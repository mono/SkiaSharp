#!/usr/bin/env python3
"""Render a release-notes page from deterministic data + agent prose.

    render-notes.py <data.json> <prose.json> [out.md]   # normal page
    render-notes.py <data.json> [out.md]                 # no-changes page (no prose)
    render-notes.py --all                                # regenerate every page + TOC.yml + index.md

`data.json`  — facts emitted by build-data.py (PRs, roster, banner
               date, links, previews). Never written by the agent.
`prose.json` — prose the polish agent produced (theme, highlights, breaking,
               category bullets, contributor summaries, preview summaries).

A page whose data.json is flagged `no_changes` (a rebuild-only HarfBuzz line,
spec §4.5) carries no prose: the host renders it from data.json alone.

Structure — headings, tables, the banner shape, @handles, ❤️, and PR links —
lives entirely in this file, so the agent cannot drop a heading or malform a
handle: it only supplies prose strings. The renderer also *enforces* the few
caps that used to be unreliable prose rules (highlights length, theme present,
every rostered contributor summarised). A violation is a hard error, surfaced to
the agent, not silently shipped.

Deliberately DEPENDENCY-FREE (no jinja2): the CI agent job runs with no network,
so it cannot `pip install` anything. Pure stdlib keeps the render bulletproof.
The page structure below is the single source of truth for the layout.
"""

from __future__ import annotations

import importlib.util
import json
import re
import sys
from collections import defaultdict
from pathlib import Path

# ── reuse build-data.py's shared low-level helpers (one source of truth) ─────
# render-notes.py owns ALL Markdown: the per-page bodies AND the TOC.yml/index.md
# aggregates. Those aggregates need version parsing, the page-set discovery, and
# the support config — all of which live in build-data.py. Importing it is
# offline-safe (module load does no network); the network-sourced Chrome schedule
# arrives via _sources/index.json, which build-index.py wrote in Prepare.
_GEN = Path(__file__).with_name("build-data.py")
_spec = importlib.util.spec_from_file_location("_rn_build_data", str(_GEN))
_gen = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(_gen)

log = _gen.log
version_key = _gen.version_key
minor_group = _gen.minor_group
RELEASES_DIR = _gen.RELEASES_DIR
VERSIONS_JSON_PATH = _gen.VERSIONS_JSON_PATH
_MONTH_ABBR = _gen._MONTH_ABBR
get_version_files = _gen.get_version_files
get_harfbuzz_version_files = _gen.get_harfbuzz_version_files
cadence_milestones = _gen.cadence_milestones
_data_json_path = _gen._data_json_path
_prose_json_path = _gen._prose_json_path

# Module-level cache for load_support_config (its own, not the generator's).
_SUPPORT_CONFIG = None

HEADLINE_WORD_CAP = 20
BODY_WORD_CAP = 60

# The canonical, closed set of category headings a page may use, in render order.
# The renderer owns this (identical for every version — presentation, not
# per-version data). validate() rejects any prose.json heading not in this set.
# KEEP IN SYNC with the category table in SKILL.md, which is where the agent
# learns the list and what each section is for.
RELEASE_CATEGORIES = [
    "Engine", "API Surface", "Bug Fixes",
    "Lifecycle & Internals", "Platform", "Security",
]

# The one fixed line a "no changes" HarfBuzz page carries as its whole body.
NO_CHANGES_BODY = (
    "No HarfBuzzSharp binding changes shipped in this release — it rebuilds "
    "the same HarfBuzz as the previous line.")


def page_title(data):
    """The H1 — built from version + family (not stored per-version)."""
    version = data.get("version", "")
    if data.get("family") == "harfbuzzsharp":
        return "HarfBuzzSharp {}".format(version)
    return "Version {}".format(version)


def breaking_none_text(data):
    """The 'no breaking changes' line — derived from status, not stored."""
    if data.get("status") in ("preview", "unreleased"):
        return "*None in this preview line.*"
    return "*None in this release.*"


# ── formatting helpers (structure the agent must never hand-write) ───────────

def _pr(data, num):
    return (data.get("prs") or {}).get(str(num))


def pr_links(nums, data):
    """`[#123](url), [#124](url)` — bare list of PR links, no hearts."""
    out = []
    for n in nums or []:
        pr = _pr(data, n)
        url = pr["url"] if pr else "https://github.com/mono/SkiaSharp/pull/{}".format(n)
        out.append("[#{}]({})".format(n, url))
    return ", ".join(out)


def pr_refs(nums, data):
    """Trailing ` ([#123](url))` for a bullet, or '' when there are none."""
    links = pr_links(nums, data)
    return " ({})".format(links) if links else ""


def credit(nums, data):
    """Trailing credit for a category bullet.

    Community authors are named with a ❤️ and a linked handle; the maintainer
    and bots are never credited. PR links always render. All of this is derived
    from `data`, so the agent can neither invent nor drop an attribution.
    """
    nums = nums or []
    handles = []
    seen = set()
    for n in nums:
        pr = _pr(data, n)
        if not pr or not pr.get("community"):
            continue
        login = pr.get("author")
        if not login or login in seen:
            continue
        seen.add(login)
        handles.append("[@{0}](https://github.com/{0})".format(login))
    links = pr_links(nums, data)
    if handles and links:
        return " — ❤️ {} ({})".format(", ".join(handles), links)
    if links:
        return " ({})".format(links)
    return ""


def banner_line(data, prose):
    """The one-line status banner. Shape is fixed here; the agent supplies the
    theme words only (`prose.theme`)."""
    b = data.get("banner") or {}
    if b.get("kind") == "harfbuzz" or data.get("family") == "harfbuzzsharp":
        return _harfbuzz_banner_line(data)
    theme = (prose.get("theme") or "").strip()
    date = b.get("date")
    parts = []
    if theme:
        parts.append("**{}**".format(theme))
    if date:
        parts.append("Released {}".format(date))
    links = []
    if b.get("nuget_url"):
        links.append("[NuGet]({})".format(b["nuget_url"]))
    if b.get("preview_nuget_url"):
        links.append("[NuGet (prerelease)]({})".format(b["preview_nuget_url"]))
    if b.get("github_release_url"):
        links.append("[GitHub Release]({})".format(b["github_release_url"]))
    return "> " + " · ".join(parts + links)


def _harfbuzz_banner_line(data):
    """HarfBuzz banner. HarfBuzz never ships on its own — it rides a SkiaSharp
    release (spec §1.5) — so instead of a theme + date the banner anchors to the
    introducing SkiaSharp version. No theme word is needed or expected."""
    b = data.get("banner") or {}
    ships = b.get("ships_with") or {}
    parts = []
    if data.get("status") in ("unreleased", "preview"):
        parts.append("Upcoming release")
    if ships.get("version"):
        parts.append("Ships with [SkiaSharp {}]({})".format(
            ships["version"], ships.get("link", "")))
    if b.get("nuget_url"):
        parts.append("[NuGet]({})".format(b["nuget_url"]))
    if b.get("preview_nuget_url"):
        parts.append("[NuGet (prerelease)]({})".format(b["preview_nuget_url"]))
    if b.get("github_release_url"):
        parts.append("[GitHub Release]({})".format(b["github_release_url"]))
    return "> " + " · ".join(parts)


# ── page assembly (the single source of layout truth) ────────────────────────

def render(data, prose):
    L = []  # lines

    L.append("<!-- RELEASE-NOTES DATA (generated, do not edit) format:{} version:{} -->"
             .format(data.get("format"), data.get("version")))
    L.append("# {}".format(page_title(data)))
    L.append(banner_line(data, prose))
    for s in data.get("supersedes") or []:
        L.append("> **Supersedes [{}]({})** · {}".format(s["version"], s["href"], s.get("note", "")))
    sb = data.get("superseded_by")
    if sb:
        L.append("> **Superseded by [{}]({})** · {}".format(sb["version"], sb["href"], sb.get("note", "")))
    if data.get("api_links"):
        api = " · ".join("[{}]({})".format(l["label"], l["href"]) for l in data["api_links"])
        L.append("> **API changes** · {}".format(api))

    # A "no changes" page (a published HarfBuzz line whose SkiaSharp window had no
    # HarfBuzz-touching PRs — a rebuild) needs no prose at all: the banner + API
    # link above, then one fixed sentence. It is fully deterministic, so it is
    # rendered by the host from data.json alone, never handed to the agent (§4.5).
    if data.get("no_changes"):
        L.append("")
        L.append(NO_CHANGES_BODY)
        return _finish_text(L)

    L.append("")
    L.append("## Highlights")
    L.append("")
    hl = prose.get("highlights_headline") or ""
    body = prose.get("highlights_body")
    L.append(hl + (" " + body if body else ""))

    L.append("")
    L.append("## Breaking Changes")
    L.append("")
    if prose.get("breaking"):
        for b in prose["breaking"]:
            L.append("- **{}** — {}{}".format(b["title"], b["body"], pr_refs(b.get("prs"), data)))
    else:
        L.append(breaking_none_text(data))

    for cat in prose.get("categories") or []:
        L.append("")
        L.append("## {}".format(cat["heading"]))
        L.append("")
        for item in cat.get("bullets") or []:
            L.append("- **{}** — {}{}".format(item["lead"], item["detail"], credit(item.get("prs"), data)))

    if data.get("platform_support"):
        L.append("")
        L.append("## Platform Support")
        L.append("")
        L.append("| Platform | Minimum Version |")
        L.append("|----------|-----------------|")
        for row in data["platform_support"]:
            L.append("| {} | {} |".format(row["platform"], row["version"]))

    if data.get("contributors"):
        summaries = prose.get("contributor_summaries") or {}
        L.append("")
        L.append("## Community Contributors ❤️")
        L.append("")
        L.append("Thank you to everyone who contributed to this release!")
        L.append("")
        L.append("| Contributor | Contributions |")
        L.append("|-------------|---------------|")
        for c in data["contributors"]:
            L.append("| [@{}]({}) | {} ({}) |".format(
                c["login"], c["url"], summaries.get(c["login"], ""), pr_links(c.get("prs"), data)))

    if data.get("links"):
        L.append("")
        L.append("## Links")
        L.append("")
        for l in data["links"]:
            L.append("- [{}]({})".format(l["label"], l["href"]))

    prev_summaries = prose.get("preview_summaries") or {}
    for p in data.get("previews") or []:
        L.append("")
        head = "## {}".format(p["label"])
        if p.get("date"):
            head += " ({})".format(p["date"])
        L.append(head)
        L.append("")
        L.append(prev_summaries.get(p["key"], ""))
        if p.get("changelog_url"):
            L.append("")
            L.append("[Full changelog]({})".format(p["changelog_url"]))

    return _finish_text(L)


def _finish_text(L):
    """Join the assembled lines into the final page text (collapse blank runs,
    ensure a trailing newline). Shared by the full and the no-changes paths."""
    text = "\n".join(L)
    text = re.sub(r"\n{3,}", "\n\n", text)
    if not text.endswith("\n"):
        text += "\n"
    return text


# ── enforcement ──────────────────────────────────────────────────────────────

def _words(text):
    return len(re.findall(r"\S+", text or ""))


# Highlights is capped by LENGTH, not by comma count: a feature-rich major
# release legitimately reads as a short list, and an arbitrary comma limit just
# punishes those. Keeping the whole block well under ~100 words (headline + body)
# is what keeps it a lead-in rather than a dump — the full detail is in the
# sections below.
HIGHLIGHTS_TOTAL_WORD_CAP = 100


def validate(data, prose):
    errors = []

    theme = (prose.get("theme") or "").strip()
    if (data.get("banner", {}) or {}).get("kind") != "harfbuzz" and not theme:
        errors.append("prose.theme is empty — the banner needs a short human theme.")

    hl = prose.get("highlights_headline") or ""
    body = prose.get("highlights_body")
    if not hl.strip():
        errors.append("prose.highlights_headline is required.")
    elif _words(hl) > HEADLINE_WORD_CAP:
        errors.append(
            "highlights_headline is {} words (cap {}). "
            "State the single most important thing; don't dump the whole changelog."
            .format(_words(hl), HEADLINE_WORD_CAP))
    if body and _words(body) > BODY_WORD_CAP:
        errors.append(
            "highlights_body is {} words (cap {}). "
            "Name the biggest themes in prose; the full detail lives in the sections below."
            .format(_words(body), BODY_WORD_CAP))
    total = _words(hl) + _words(body or "")
    if total > HIGHLIGHTS_TOTAL_WORD_CAP:
        errors.append(
            "Highlights is {} words total (cap {}). It's a lead-in, not the changelog — "
            "trim to the few things that matter most.".format(total, HIGHLIGHTS_TOTAL_WORD_CAP))

    return _finish_validate(errors, data, prose)


def _finish_validate(errors, data, prose):
    roster = {c["login"] for c in data.get("contributors", [])}
    summaries = prose.get("contributor_summaries") or {}
    missing = sorted(roster - set(summaries))
    if missing:
        errors.append(
            "every contributor in the roster needs a one-line summary; missing: "
            + ", ".join("@" + m for m in missing))
    empty = sorted(k for k in roster if not (summaries.get(k) or "").strip())
    if empty:
        errors.append("these contributor summaries are blank: "
                      + ", ".join("@" + m for m in empty))

    allowed = set(RELEASE_CATEGORIES)
    if allowed:
        for cat in prose.get("categories", []):
            if cat.get("heading") not in allowed:
                errors.append(
                    "category heading '{}' is not one of the allowed sections: {}"
                    .format(cat.get("heading"), ", ".join(sorted(allowed))))

    prev_keys = {p["key"] for p in data.get("previews", [])}
    prev_sum = prose.get("preview_summaries") or {}
    missing_prev = sorted(prev_keys - set(prev_sum))
    if missing_prev:
        errors.append("every preview/RC needs a one-line summary; missing: "
                      + ", ".join(missing_prev))

    return errors



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


def format_schedule_date(iso):
    # type: (str) -> str
    """``2026-06-03T00:00:00`` -> ``Jun 3`` (no leading zero)."""
    _, month, day = iso[:10].split("-")
    return "{} {}".format(_MONTH_ABBR[int(month) - 1], int(day))


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


def generate_index(versions, next_versions, hb_versions=None, hb_next_versions=None,
                   schedule_by_ms=None):
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
        next_major, cur_ms, next_ms = cadence_milestones()
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
        lines.extend(render_cadence_timeline(
            cur_ms, next_ms, cur_base, next_base, schedule_by_ms))
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


def render_cadence_timeline(cur_ms, next_ms, cur_base, next_base, schedule_by_ms):
    # type: (int, int, str, str, dict) -> list[str]
    """Schedule-timeline table for the release-cadence section (offline).

    The two Chrome schedules were fetched by build-index.py in the Prepare phase
    and committed in _sources/index.json, so this runs with no network. A missing
    schedule is a hard error — re-run build-index.py to refresh index.json.
    """
    phases = [
        ("Beta Promotion", "beta", ".0-preview.1"),
        ("Early Stable", "early_stable", ".0-preview.2"),
        ("Stable Cut", "stable_cut", ".0-rc.1"),
        ("Stable Release", "stable", ".0"),
    ]
    schedule_by_ms = schedule_by_ms or {}
    cur_sched = schedule_by_ms.get(str(cur_ms))
    next_sched = schedule_by_ms.get(str(next_ms))
    if not cur_sched or not next_sched:
        raise RuntimeError(
            "index.json is missing the Chrome schedule for m{} or m{} - re-run "
            "build-index.py (the network Prepare step) to refresh "
            "_sources/index.json.".format(cur_ms, next_ms))
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


def load_index_json():
    # type: () -> dict
    """The committed network-sourced index data (Chrome schedule); {} if absent."""
    path = RELEASES_DIR / "_sources" / "index.json"
    if not path.is_file():
        return {}
    try:
        return json.loads(path.read_text())
    except (ValueError, OSError):
        return {}


def _page_for_data(data_path):
    # type: (Path) -> Path
    """``<dir>/_sources/<stem>.data.json`` -> ``<dir>/<stem>.md``."""
    data_path = Path(data_path)
    stem = data_path.name[:-len(".data.json")]
    return data_path.parent.parent / (stem + ".md")


def _prune_stale_unreleased(live):
    # type: (set) -> int
    """Delete SkiaSharp ``<v>-unreleased.md`` pages whose line is no longer live.

    ``live`` is the set of live-head version cores that build-index recorded in
    index.json (it needed the remote branch list — a network the render does not
    have). We delete the page and its generated inputs (data.json, prose.json);
    the human-owned notes.md is left for the orphan warning. Released ``<v>.md``
    pages are never touched, and only the SkiaSharp family is pruned here —
    HarfBuzz ``-unreleased`` ownership is decided during generation from the
    co-release map. An empty ``live`` means "unknown" -> prune nothing.
    """
    if not live or not RELEASES_DIR.is_dir():
        return 0
    pruned = 0
    for f in sorted(RELEASES_DIR.glob("*-unreleased.md")):  # non-recursive: skips _sources/ + hb/
        version = f.stem[:-len("-unreleased")]
        if version in live:
            continue
        f.unlink()
        for extra in (_data_json_path(f), _prose_json_path(f)):
            if extra.exists():
                extra.unlink()
        pruned += 1
        log("  Pruned stale {}".format(f))
    return pruned


def render_all():
    # type: () -> int
    """Regenerate EVERY page and the TOC/index from committed JSON (offline).

    The final Polish step: after the agent has written each page's prose.json,
    one --all pass prunes any now-stale ``-unreleased`` page (per the live-head set
    build-index recorded in index.json), re-renders every ``<version>.md`` from its
    data.json + prose.json (and the deterministic no-changes pages from data.json
    alone), then builds TOC.yml + index.md from the finished page set and the
    committed Chrome schedule. Pure JSON -> Markdown, so it is fast and re-runnable.
    """
    index = load_index_json()
    _prune_stale_unreleased(set(index.get("live_unreleased") or []))

    src_dirs = [RELEASES_DIR / "_sources", RELEASES_DIR / "harfbuzzsharp" / "_sources"]
    rendered = 0
    invalid = []  # pages whose committed prose.json does not validate — a hard error
    for sd in src_dirs:
        if not sd.is_dir():
            continue
        for dp in sorted(sd.glob("*.data.json")):
            data = json.loads(dp.read_text())
            page = _page_for_data(dp)
            if data.get("no_changes"):
                page.write_text(render(data, {}))
                rendered += 1
                continue
            pp = dp.with_name(dp.name[:-len(".data.json")] + ".prose.json")
            if not pp.is_file():
                # Missing prose is tolerated (warn + keep the committed .md): a
                # scoped/partial run legitimately renders the whole set while only
                # polishing a subset, so pages outside this run's scope have no
                # fresh prose. Invalid prose, below, is NOT tolerated.
                log("  WARNING: no prose.json for {} - keeping committed page".format(page))
                continue
            prose = json.loads(pp.read_text())
            errs = validate(data, prose)
            if errs:
                # A committed prose.json that fails validation must never ship —
                # it would violate the page caps/roster invariant. Record it and
                # fail the whole --all pass so CI does not commit a bad page.
                invalid.append((page, errs))
                log("  ERROR: prose for {} failed validation: {}".format(
                    page, "; ".join(errs)))
                continue
            page.write_text(render(data, prose))
            rendered += 1
    log("Rendered {} pages".format(rendered))

    versions, next_versions = get_version_files()
    hb_versions, hb_next_versions = get_harfbuzz_version_files()
    schedule = index.get("chrome_schedule") or {}
    (RELEASES_DIR / "TOC.yml").write_text(
        generate_toc(versions, next_versions, hb_versions, hb_next_versions))
    (RELEASES_DIR / "index.md").write_text(
        generate_index(versions, next_versions, hb_versions, hb_next_versions, schedule))
    log("Wrote {} and {}".format(RELEASES_DIR / "TOC.yml", RELEASES_DIR / "index.md"))
    if invalid:
        log("PROSE VALIDATION FAILED for {} page(s); fix the prose.json and re-run "
            "--all:".format(len(invalid)))
        for page, errs in invalid:
            log("  - {}: {}".format(page, "; ".join(errs)))
    return len(invalid)


def main(argv):
    flags = [a for a in argv[1:] if a.startswith("-")]
    args = [a for a in argv[1:] if not a.startswith("-")]

    # --all: the final Polish pass. Regenerate every page + TOC/index from the
    # committed JSON, offline. Takes no positional args. Returns non-zero if any
    # committed prose.json failed validation (a bad page must never ship).
    if "--all" in flags:
        return 1 if render_all() else 0

    if not args:
        print(__doc__)
        return 2
    data = json.loads(Path(args[0]).read_text())

    # A "no changes" page is fully deterministic: render it from data alone (no
    # prose, no validation). Usage: `render-notes.py <data.json> [out.md]`.
    if data.get("no_changes"):
        text = render(data, {})
        out = args[1] if len(args) >= 2 else None
        if out:
            Path(out).write_text(text)
            print("wrote {} (no changes, {} words)".format(out, _words(text)))
        else:
            sys.stdout.write(text)
        return 0

    if len(args) < 2:
        print(__doc__)
        return 2
    prose = json.loads(Path(args[1]).read_text())
    errors = validate(data, prose)
    if errors:
        print("PROSE VALIDATION FAILED:", file=sys.stderr)
        for e in errors:
            print("  - {}".format(e), file=sys.stderr)
        return 1
    text = render(data, prose)
    out = args[2] if len(args) >= 3 else None
    if out:
        Path(out).write_text(text)
        print("wrote {} ({} words)".format(out, _words(text)))
    else:
        sys.stdout.write(text)
    return 0


if __name__ == "__main__":
    raise SystemExit(main(sys.argv))
