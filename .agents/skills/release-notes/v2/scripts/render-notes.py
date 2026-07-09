#!/usr/bin/env python3
"""Render a release-notes page from deterministic data + agent prose.

    render-notes.py <data.json> <slots.json> [out.md]

`data.json`  — facts emitted by generate-release-notes.py (PRs, roster, banner
               date, links, previews). Never written by the agent.
`slots.json` — prose the polish agent produced (theme, highlights, breaking,
               category bullets, contributor summaries, preview summaries).

Structure — headings, tables, the banner shape, @handles, ❤️, and PR links —
lives in page.md.jinja2 and in the formatting globals below, so the agent
cannot drop a heading or malform a handle: it only supplies prose strings.

The script also *enforces* the few caps that used to be unreliable prose rules
(highlights length, theme present, every rostered contributor summarised). A
violation is a hard error, surfaced to the agent, not silently shipped.
"""

from __future__ import annotations

import json
import re
import sys
from pathlib import Path

try:
    from jinja2 import Environment, FileSystemLoader, StrictUndefined
except ImportError:  # pragma: no cover
    print("jinja2 is required: pip3 install -r requirements.txt", file=sys.stderr)
    sys.exit(2)

SCRIPTS_DIR = Path(__file__).parent

HEADLINE_WORD_CAP = 20
BODY_WORD_CAP = 60


# ── formatting globals (structure the agent must never hand-write) ───────────

def _pr(data, num):
    return (data.get("prs") or {}).get(str(num))


def pr_links(nums, data):
    """`[#123](url), [#124](url)` — bare list of PR links, no hearts."""
    out = []
    for n in nums or []:
        pr = _pr(data, n)
        url = pr["url"] if pr else f"https://github.com/mono/SkiaSharp/pull/{n}"
        out.append(f"[#{n}]({url})")
    return ", ".join(out)


def pr_refs(nums, data):
    """Trailing ` ([#123](url))` for a bullet, or '' when there are none."""
    links = pr_links(nums, data)
    return f" ({links})" if links else ""


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
        handles.append(f"[@{login}](https://github.com/{login})")
    links = pr_links(nums, data)
    if handles and links:
        return f" — ❤️ {', '.join(handles)} ({links})"
    if links:
        return f" ({links})"
    return ""


def banner_line(data, slots):
    """The one-line status banner. Shape is fixed here; the agent supplies the
    theme words only (`slots.theme`)."""
    b = data.get("banner") or {}
    theme = (slots.get("theme") or "").strip()
    date = b.get("date")
    parts = []
    if theme:
        parts.append(f"**{theme}**")
    if date:
        parts.append(f"Released {date}")
    links = []
    if b.get("nuget_url"):
        parts_label = "NuGet"
        links.append(f"[{parts_label}]({b['nuget_url']})")
    if b.get("preview_nuget_url"):
        links.append(f"[NuGet (prerelease)]({b['preview_nuget_url']})")
    if b.get("github_release_url"):
        links.append(f"[GitHub Release]({b['github_release_url']})")
    return "> " + " · ".join(parts + links)


# ── enforcement ──────────────────────────────────────────────────────────────

def _words(text):
    return len(re.findall(r"\S+", text or ""))


def validate(data, slots):
    errors = []

    theme = (slots.get("theme") or "").strip()
    if data.get("banner", {}).get("kind") != "harfbuzz" and not theme:
        errors.append("slots.theme is empty — the banner needs a short human theme.")

    hl = slots.get("highlights_headline") or ""
    if not hl.strip():
        errors.append("slots.highlights_headline is required.")
    elif _words(hl) > HEADLINE_WORD_CAP:
        errors.append(
            f"highlights_headline is {_words(hl)} words (cap {HEADLINE_WORD_CAP}). "
            "State the single most important thing; don't enumerate.")
    body = slots.get("highlights_body")
    if body and _words(body) > BODY_WORD_CAP:
        errors.append(
            f"highlights_body is {_words(body)} words (cap {BODY_WORD_CAP}). "
            "Name 2-3 themes in prose; the full detail lives in the sections below.")

    roster = {c["login"] for c in data.get("contributors", [])}
    summaries = slots.get("contributor_summaries") or {}
    missing = sorted(roster - set(summaries))
    if missing:
        errors.append(
            "every contributor in the roster needs a one-line summary; missing: "
            + ", ".join("@" + m for m in missing))

    allowed = set(data.get("allowed_categories", []))
    if allowed:
        for cat in slots.get("categories", []):
            if cat.get("heading") not in allowed:
                errors.append(
                    f"category heading '{cat.get('heading')}' is not one of the "
                    f"allowed sections: {', '.join(sorted(allowed))}")

    prev_keys = {p["key"] for p in data.get("previews", [])}
    prev_sum = slots.get("preview_summaries") or {}
    missing_prev = sorted(prev_keys - set(prev_sum))
    if missing_prev:
        errors.append(
            "every preview/RC needs a one-line summary; missing: "
            + ", ".join(missing_prev))

    return errors


def render(data, slots):
    env = Environment(
        loader=FileSystemLoader(str(SCRIPTS_DIR)),
        trim_blocks=False,
        lstrip_blocks=False,
        undefined=StrictUndefined,
        keep_trailing_newline=True,
    )
    env.globals.update(
        pr_links=pr_links,
        pr_refs=pr_refs,
        credit=credit,
        banner_line=banner_line,
    )
    tmpl = env.get_template("page.md.jinja2")
    text = tmpl.render(data=data, slots=slots)
    # collapse any run of 3+ blank lines the section loops may introduce
    text = re.sub(r"\n{3,}", "\n\n", text)
    if not text.endswith("\n"):
        text += "\n"
    return text


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
            print(f"  - {e}", file=sys.stderr)
        return 1
    text = render(data, slots)
    if len(argv) >= 4:
        Path(argv[3]).write_text(text)
        print(f"wrote {argv[3]} ({_words(text)} words)")
    else:
        sys.stdout.write(text)
    return 0


if __name__ == "__main__":
    raise SystemExit(main(sys.argv))
