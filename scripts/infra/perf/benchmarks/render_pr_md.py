#!/usr/bin/env python3
"""Render a COMPACT Markdown PR comment summarising this PR's benchmark deltas.

Reads the same per-(OS, role) ``benchmarks-<OS>-<role>.json`` histories the benchmark
``report`` job already downloads, compares the ⭐ ``pr`` role (a full source build of the
PR checkout) against a baseline role (``nightly`` by default -- the CI trend that tracks
main one build behind), and emits a short, informational comment:

  * a **Highlights** section with the biggest movers (time + allocations), and
  * a collapsed ``<details>`` block with the full per-OS deltas.

Only benchmarks that move beyond the shared 5% noise threshold are surfaced, so the
comment stays a summary rather than a table dump. It carries an HTML marker
(``<!-- skiasharp-pr-benchmarks -->``) so the workflow's github-script step can
find-update-or-create a single comment per PR. Informational only -- it never blocks a PR.

The time/byte formatters, the trend dots and the history loader are imported from
``render_md`` (and the shared ``_common`` log helper) -- this script only adds the
PR-vs-baseline comparison and the compact layout on top; it duplicates none of that logic.

Only the Python standard library is used.
"""

from __future__ import annotations

import argparse
import os
import sys
from collections import namedtuple

# Run by path from CI (not as a package): make the sibling perf modules importable.
_HERE = os.path.dirname(os.path.abspath(__file__))
_PERF = os.path.dirname(_HERE)
for _p in (_HERE, _PERF):
    if _p not in sys.path:
        sys.path.insert(0, _p)

import _common  # noqa: E402  (shared stderr log helper)
import render_md as rmd  # noqa: E402  (formatters, history loader, trend dots)

MARKER = "<!-- skiasharp-pr-benchmarks -->"

# Baseline preference: the first present role wins. ``nightly`` is the closest apples-to-
# apples reference (the published CI build tracking main's source one build behind); the
# released stables are fallbacks for when a nightly leg produced nothing this run.
BASELINE_PREFERENCE = ("nightly", "curr-stable", "latest", "prev-stable", "prev-major")

# Human labels for a chosen baseline role (plain hyphens for prose; the non-breaking
# variants in render_md are for narrow table headers).
BASELINE_LABELS = {
    "nightly": "🌙 nightly",
    "curr-stable": "curr-stable",
    "latest": "latest",
    "prev-stable": "prev-stable",
    "prev-major": "prev-major",
}

# Metrics we diff: (json key, section title, formatter, worse-word, better-word). Lower is
# better for both, so a positive delta (pr > baseline) is the "worse" direction.
METRICS = (
    ("meanNs", "⏱️ Time", rmd.human_time, "slower", "faster"),
    ("allocatedBytes", "📦 Allocations", rmd.human_bytes, "more alloc", "less alloc"),
)

TOP_MOVERS = 5        # bullets per metric in the Highlights section
MAX_DETAIL_ROWS = 25  # cap per OS/metric detail table so a broad move can't dump everything

Mover = namedtuple("Mover", "os name pr base delta")


# --------------------------------------------------------------------------- #
# Comparison primitives
# --------------------------------------------------------------------------- #

def _pct(pr, base):
    """Signed fractional change of ``pr`` vs ``base`` (>0 = slower/larger = worse)."""
    if pr is None or base is None or base == 0:
        return None
    return (pr - base) / base


def _dot(delta):
    """Trend dot for a signed delta: 🔴 worse (slower/larger), 🟢 better (faster/smaller)."""
    return rmd.TREND_SLOWER if delta > 0 else rmd.TREND_FASTER


def pick_baseline(roles, forced=None):
    """``(role, latest-day)`` of the baseline to compare against, or ``(None, None)``.

    ``forced`` pins a specific role when present; otherwise the first role in
    ``BASELINE_PREFERENCE`` that actually has data wins.
    """
    order = (forced,) if forced else BASELINE_PREFERENCE
    for role in order:
        day = rmd.latest_day(roles.get(role))
        if day:
            return role, day
    return None, None


def build_context(histories, forced=None):
    """``(oses, ctx)`` where ``ctx[os] = {"pr", "baseRole", "base"}`` — the latest ⭐ pr
    day and the resolved baseline day for each OS that has any data."""
    oses = rmd.ordered_oses(histories)
    ctx = {}
    for o in oses:
        roles = histories[o]
        role, base = pick_baseline(roles, forced)
        ctx[o] = {"pr": rmd.latest_day(roles.get("pr")), "baseRole": role, "base": base}
    return oses, ctx


def collect_movers(ctx, oses, key):
    """Every benchmark (across ``oses``) whose ⭐ pr value moved beyond the shared noise
    threshold vs its baseline, as ``Mover``s (unsorted)."""
    movers = []
    for o in oses:
        c = ctx[o]
        pr, base = c["pr"], c["base"]
        if not pr or not base:
            continue
        names = set(pr.get("benchmarks", {})) | set(base.get("benchmarks", {}))
        for name in names:
            pv = rmd.metric(pr, name, key)
            bv = rmd.metric(base, name, key)
            delta = _pct(pv, bv)
            if delta is None or abs(delta) < rmd.TREND_PCT:
                continue
            movers.append(Mover(o, name, pv, bv, delta))
    return movers


# --------------------------------------------------------------------------- #
# Rendering
# --------------------------------------------------------------------------- #

def _title(pr_number, run_url):
    head = "## 📊 SkiaSharp benchmarks"
    if pr_number:
        link = f"[PR #{pr_number}]({run_url})" if run_url else f"PR #{pr_number}"
        head += f" — {link}"
    return head


def _no_pr_body(pr_number, run_url):
    lines = [MARKER, "", _title(pr_number, run_url), "",
             "_No ⭐ source-build (`pr`) benchmark results were produced for this run, so "
             "there is nothing to compare yet._ The source-build legs are best-effort and "
             "may be skipped (native unchanged) or may have failed — see the run for details.",
             ""]
    if run_url:
        lines += [f"📈 [View the benchmark run &amp; `perf-dashboard` artifact →]({run_url})", ""]
    return "\n".join(lines) + "\n"


def _baseline_label_for(ctx, oses):
    used = {ctx[o]["baseRole"] for o in oses if ctx[o]["pr"] and ctx[o]["base"]}
    keyer = lambda r: BASELINE_PREFERENCE.index(r) if r in BASELINE_PREFERENCE else 99
    return " / ".join(BASELINE_LABELS.get(r, r) for r in sorted(used, key=keyer)) or "baseline"


def _highlights(ctx, oses):
    """The visible summary: per-metric mover counts + the biggest movers as bullets.
    Returns ``(lines, any_mover)``."""
    lines = []
    any_mover = False
    for key, mtitle, fmt, worse, better in METRICS:
        movers = sorted(collect_movers(ctx, oses, key), key=lambda m: abs(m.delta), reverse=True)
        if not movers:
            continue
        any_mover = True
        regr = sum(1 for m in movers if m.delta > 0)
        impr = len(movers) - regr
        lines.append(f"**{mtitle}** — {rmd.TREND_SLOWER} {regr} {worse} · "
                     f"{rmd.TREND_FASTER} {impr} {better}")
        for m in movers[:TOP_MOVERS]:
            lines.append(f"- {_dot(m.delta)} `{rmd.short_name(m.name)}` · {m.os} · "
                         f"{fmt(m.base)} → {fmt(m.pr)} ({m.delta:+.0%})")
        extra = len(movers) - TOP_MOVERS
        if extra > 0:
            lines.append(f"- …and {extra} more (see details below)")
        lines.append("")
    return lines, any_mover


def _details(ctx, oses):
    """The collapsed full per-OS deltas (only moved rows)."""
    lines = ["<details>", "<summary>Full per-OS benchmark deltas</summary>", ""]
    for o in oses:
        c = ctx[o]
        if not c["pr"] or not c["base"]:
            continue
        os_blocks = []
        for key, mtitle, fmt, *_ in METRICS:
            rows = sorted(collect_movers(ctx, [o], key), key=lambda m: abs(m.delta), reverse=True)
            if not rows:
                continue
            base_label = BASELINE_LABELS.get(c["baseRole"], c["baseRole"])
            block = [f"**{mtitle}** (vs {base_label} `{c['base'].get('version', '?')}`)", "",
                     "| Benchmark | baseline | this PR | Δ |", "|:--|--:|--:|--:|"]
            for m in rows[:MAX_DETAIL_ROWS]:
                block.append(f"| `{rmd.short_name(m.name)}` | {fmt(m.base)} | {fmt(m.pr)} | "
                             f"{_dot(m.delta)} {m.delta:+.0%} |")
            hidden = len(rows) - MAX_DETAIL_ROWS
            if hidden > 0:
                block.append(f"| _…and {hidden} more_ |  |  |  |")
            block.append("")
            os_blocks += block
        if os_blocks:
            lines += [f"#### {o}", ""] + os_blocks
    lines += ["</details>", ""]
    return lines


def render(histories, pr_number=None, run_url=None, forced_baseline=None):
    oses, ctx = build_context(histories, forced_baseline)
    if not any(ctx[o]["pr"] for o in oses):
        return _no_pr_body(pr_number, run_url)

    compared = [o for o in oses if ctx[o]["pr"]]
    lines = [MARKER, "", _title(pr_number, run_url), "",
             f"⭐ **this PR** (full source build) vs **{_baseline_label_for(ctx, oses)}** · "
             f"{' · '.join(compared)}", "",
             f"> Informational only — this never blocks the PR. {rmd.TREND_FASTER} faster / "
             f"less allocation · {rmd.TREND_SLOWER} slower / more allocation; moves under "
             f"{rmd.TREND_PCT:.0%} are hidden as noise.", ""]

    highlights, any_mover = _highlights(ctx, oses)
    if any_mover:
        lines += ["### Highlights", "", *highlights]
        lines += _details(ctx, oses)
    else:
        lines += [f"✅ No benchmarks moved beyond the {rmd.TREND_PCT:.0%} noise threshold "
                  f"vs the baseline — this PR is performance-neutral.", ""]

    if run_url:
        lines += [f"📈 [Full interactive `perf-dashboard` &amp; run details →]({run_url})", ""]
    return "\n".join(lines) + "\n"


# --------------------------------------------------------------------------- #
# Main
# --------------------------------------------------------------------------- #

def parse_args(argv):
    p = argparse.ArgumentParser(description=__doc__,
                                formatter_class=argparse.RawDescriptionHelpFormatter)
    p.add_argument("histories_dir", help="Directory of benchmarks-<OS>-<role>.json histories.")
    p.add_argument("--pr-number", default=os.environ.get("PR_NUMBER"),
                   help="PR number for the comment header (env: PR_NUMBER).")
    p.add_argument("--run-url", default=os.environ.get("RUN_URL"),
                   help="Workflow run URL for the header/footer links (env: RUN_URL).")
    p.add_argument("--baseline", default=None,
                   help="Force a baseline role (default: auto — nightly, then released stables).")
    p.add_argument("--output", default=None,
                   help="Write the Markdown here (it is always also printed to stdout).")
    return p.parse_args(argv)


def main(argv):
    args = parse_args(argv)
    histories = rmd.load_histories(args.histories_dir)
    md = render(histories, pr_number=args.pr_number, run_url=args.run_url,
                forced_baseline=args.baseline)
    print(md)
    if args.output:
        with open(args.output, "w", encoding="utf-8") as fh:
            fh.write(md)

    oses, ctx = build_context(histories, args.baseline)
    compared = [o for o in oses if ctx[o]["pr"]]
    total = sum(len(collect_movers(ctx, oses, k)) for k, *_ in METRICS)
    _common.log(f"  rendered PR benchmark comment: {len(compared)} OS compared, "
                f"{total} mover(s) beyond {rmd.TREND_PCT:.0%}")
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
