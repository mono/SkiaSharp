#!/usr/bin/env python3
"""Render a release-notes page from deterministic data + agent prose.

    render-notes.py <data.json> <slots.json> [out.md]

`data.json`  — facts emitted by generate-release-notes.py (PRs, roster, banner
               date, links, previews). Never written by the agent.
`slots.json` — prose the polish agent produced (theme, highlights, breaking,
               category bullets, contributor summaries, preview summaries).

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

import json
import re
import sys
from pathlib import Path

HEADLINE_WORD_CAP = 20
BODY_WORD_CAP = 60


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


def banner_line(data, slots):
    """The one-line status banner. Shape is fixed here; the agent supplies the
    theme words only (`slots.theme`)."""
    b = data.get("banner") or {}
    theme = (slots.get("theme") or "").strip()
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


# ── page assembly (the single source of layout truth) ────────────────────────

def render(data, slots):
    L = []  # lines

    L.append("<!-- RELEASE-NOTES DATA (generated, do not edit) format:{} version:{} -->"
             .format(data.get("format"), data.get("version")))
    L.append("# {}".format(data.get("title")))
    L.append(banner_line(data, slots))
    for s in data.get("supersedes") or []:
        L.append("> **Supersedes [{}]({})** · {}".format(s["version"], s["href"], s.get("note", "")))
    sb = data.get("superseded_by")
    if sb:
        L.append("> **Superseded by [{}]({})** · {}".format(sb["version"], sb["href"], sb.get("note", "")))
    if data.get("api_links"):
        api = " · ".join("[{}]({})".format(l["label"], l["href"]) for l in data["api_links"])
        L.append("> **API changes** · {}".format(api))

    L.append("")
    L.append("## Highlights")
    L.append("")
    hl = slots.get("highlights_headline") or ""
    body = slots.get("highlights_body")
    L.append(hl + (" " + body if body else ""))

    L.append("")
    L.append("## Breaking Changes")
    L.append("")
    if slots.get("breaking"):
        for b in slots["breaking"]:
            L.append("- **{}** — {}{}".format(b["title"], b["body"], pr_refs(b.get("prs"), data)))
    else:
        L.append(data.get("breaking_none_text", "*None in this release.*"))

    for cat in slots.get("categories") or []:
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
        summaries = slots.get("contributor_summaries") or {}
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

    prev_summaries = slots.get("preview_summaries") or {}
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

    text = "\n".join(L)
    text = re.sub(r"\n{3,}", "\n\n", text)
    if not text.endswith("\n"):
        text += "\n"
    return text


# ── enforcement ──────────────────────────────────────────────────────────────

def _words(text):
    return len(re.findall(r"\S+", text or ""))


def validate(data, slots):
    errors = []

    theme = (slots.get("theme") or "").strip()
    if (data.get("banner", {}) or {}).get("kind") != "harfbuzz" and not theme:
        errors.append("slots.theme is empty — the banner needs a short human theme.")

    hl = slots.get("highlights_headline") or ""
    if not hl.strip():
        errors.append("slots.highlights_headline is required.")
    elif _words(hl) > HEADLINE_WORD_CAP:
        errors.append(
            "highlights_headline is {} words (cap {}). "
            "State the single most important thing; don't enumerate."
            .format(_words(hl), HEADLINE_WORD_CAP))
    body = slots.get("highlights_body")
    if body and _words(body) > BODY_WORD_CAP:
        errors.append(
            "highlights_body is {} words (cap {}). "
            "Name 2-3 themes in prose; the full detail lives in the sections below."
            .format(_words(body), BODY_WORD_CAP))

    roster = {c["login"] for c in data.get("contributors", [])}
    summaries = slots.get("contributor_summaries") or {}
    missing = sorted(roster - set(summaries))
    if missing:
        errors.append(
            "every contributor in the roster needs a one-line summary; missing: "
            + ", ".join("@" + m for m in missing))
    empty = sorted(k for k in roster if not (summaries.get(k) or "").strip())
    if empty:
        errors.append("these contributor summaries are blank: "
                      + ", ".join("@" + m for m in empty))

    allowed = set(data.get("allowed_categories", []))
    if allowed:
        for cat in slots.get("categories", []):
            if cat.get("heading") not in allowed:
                errors.append(
                    "category heading '{}' is not one of the allowed sections: {}"
                    .format(cat.get("heading"), ", ".join(sorted(allowed))))

    prev_keys = {p["key"] for p in data.get("previews", [])}
    prev_sum = slots.get("preview_summaries") or {}
    missing_prev = sorted(prev_keys - set(prev_sum))
    if missing_prev:
        errors.append("every preview/RC needs a one-line summary; missing: "
                      + ", ".join(missing_prev))

    return errors


def main(argv):
    if len(argv) < 3:
        print(__doc__)
        return 2
    data = json.loads(Path(argv[1]).read_text())
    slots = json.loads(Path(argv[2]).read_text())
    errors = validate(data, slots)
    if errors:
        print("SLOT VALIDATION FAILED:", file=sys.stderr)
        for e in errors:
            print("  - {}".format(e), file=sys.stderr)
        return 1
    text = render(data, slots)
    if len(argv) >= 4:
        Path(argv[3]).write_text(text)
        print("wrote {} ({} words)".format(argv[3], _words(text)))
    else:
        sys.stdout.write(text)
    return 0


if __name__ == "__main__":
    raise SystemExit(main(sys.argv))
