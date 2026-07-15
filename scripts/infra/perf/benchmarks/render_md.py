#!/usr/bin/env python3
"""Render per-(OS, role) benchmark histories as a Markdown dashboard.

Consumes ``benchmarks-<OS>-<role>.json`` documents produced by track-benchmarks.py
(one per operating system AND version role) and renders, per OS, a table whose
columns run newest -> oldest, left -> right:

    [ nightly today ... nightly 10 days ago ] [ curr-stable ] [ prev-stable ] [ prev-major ]

so a regression (today slower than an older build/release) shows as a red dot on
the left. This mirrors track-artifact-sizes' layout (nightly days + released roles).

A small cross-OS snapshot of the latest nightly is included too, but note it is NOT
hardware-normalized yet, so cross-OS numbers are placeholders for now.

Only the Python standard library is used.
"""

from __future__ import annotations

import glob
import json
import os
import sys

OS_ORDER = ("Linux", "Windows", "macOS")
NAME_PREFIX = "SkiaSharp.Benchmarks.Tracking."
DISPLAY_DAYS = 10

# Released roles, newest -> oldest software (shown to the right of the nightly days).
ROLE_ORDER = ("curr-stable", "prev-stable", "prev-major")
ROLE_LABELS = {"curr-stable": "curr&#8209;stable", "prev-stable": "prev&#8209;stable", "prev-major": "prev&#8209;major"}

TREND_PCT = 0.05
TREND_SLOWER = "🔴"
TREND_FASTER = "🟢"


def human_time(ns):
    if ns is None:
        return "&middot;"
    if ns < 1000:
        return f"{ns:.1f} ns"
    if ns < 1_000_000:
        return f"{ns / 1_000:.2f} µs"
    if ns < 1_000_000_000:
        return f"{ns / 1_000_000:.2f} ms"
    return f"{ns / 1_000_000_000:.2f} s"


def human_bytes(value):
    if value is None:
        return "&middot;"
    b = int(round(value))
    if b < 1024:
        return f"{b} B"
    kb = b / 1024.0
    return f"{kb:.1f} KB" if kb < 1024 else f"{kb / 1024:.1f} MB"


def short_name(full):
    return full[len(NAME_PREFIX):] if full.startswith(NAME_PREFIX) else full


def _trend(curr, older):
    if curr is None or older is None or older == 0:
        return ""
    diff = (curr - older) / older
    if abs(diff) < TREND_PCT:
        return ""
    return f" {TREND_SLOWER}" if diff > 0 else f" {TREND_FASTER}"


def row_cells(values, fmt=human_time):
    """Columns newest -> oldest; compare each cell to the nearest non-empty cell to its right."""
    cells = [""] * len(values)
    older = None
    for i in range(len(values) - 1, -1, -1):
        v = values[i]
        cells[i] = fmt(v) + _trend(v, older)
        if v is not None:
            older = v
    return cells


# --------------------------------------------------------------------------- #
# Loading: histories[os][role] = history doc
# --------------------------------------------------------------------------- #

def load_histories(input_dir):
    out = {}
    for path in sorted(glob.glob(os.path.join(input_dir, "benchmarks-*.json"))):
        try:
            with open(path, "r", encoding="utf-8") as fh:
                data = json.load(fh)
        except (json.JSONDecodeError, OSError) as err:
            print(f"  ! skipping {path}: {err}", file=sys.stderr)
            continue
        out.setdefault(data.get("os", "?"), {})[data.get("role", "nightly")] = data
    return out


def ordered_oses(histories):
    known = [o for o in OS_ORDER if o in histories]
    return known + sorted(o for o in histories if o not in OS_ORDER)


def latest_day(hist):
    days = (hist or {}).get("days") or []
    return days[-1] if days else None


def metric(day, name, key="meanNs"):
    if not day:
        return None
    return (day.get("benchmarks", {}).get(name) or {}).get(key)


# --------------------------------------------------------------------------- #
# Column model (per OS)
# --------------------------------------------------------------------------- #

class Column:
    def __init__(self, key, header, kind):
        self.key, self.header, self.kind = key, header, kind


def build_columns(roles):
    cols = []
    # "pr" (this branch, source build) goes first when present.
    pr = roles.get("pr")
    if pr:
        label = (latest_day(pr) or {}).get("version", "pr")
        cols.append(Column(("pr", None), f"⭐ this PR<br>`{label}`", "role"))
    nightly = roles.get("nightly")
    for day in reversed((nightly or {}).get("days", [])[-DISPLAY_DAYS:]):
        cols.append(Column(("nightly", day["date"]), f"🌙 {day['date'][5:]}", "nightly"))
    for role in ROLE_ORDER:
        rh = roles.get(role)
        ver = (latest_day(rh) or {}).get("version", "?") if rh else None
        if rh:
            cols.append(Column((role, None), f"{ROLE_LABELS[role]}<br>`{ver}`", "role"))
    return cols


def column_value(roles, col, name, key="meanNs"):
    if col.kind == "nightly":
        for day in (roles.get("nightly") or {}).get("days", []):
            if day["date"] == col.key[1]:
                return metric(day, name, key)
        return None
    return metric(latest_day(roles.get(col.key[0])), name, key)


def benchmark_universe(roles):
    names = set()
    for role in ("pr", "nightly") + ROLE_ORDER:
        names.update((latest_day(roles.get(role)) or {}).get("benchmarks", {}).keys())
    return sorted(names)


# --------------------------------------------------------------------------- #
# Rendering
# --------------------------------------------------------------------------- #

def render_header(histories, oses):
    lines = ["## 📊 SkiaSharp benchmark tracker", ""]
    rows = []
    for o in oses:
        roles = histories[o]
        nd = latest_day(roles.get("nightly"))
        if not nd:
            continue
        vers = " · ".join(
            f"{ROLE_LABELS[r].replace('&#8209;','-')} `{(latest_day(roles.get(r)) or {}).get('version','?')}`"
            for r in ROLE_ORDER if roles.get(r))
        rows.append(f"| {o} | `{nd.get('version','?')}` | {vers or '&middot;'} |")
    if not rows:
        return lines + ["_No data collected yet._", ""]
    lines += ["| OS | nightly | released baselines |", "|:--|:--|:--|", *rows, "",
              f"> Columns run newest → oldest. When present, **⭐ this PR** (a full source "
              f"build of SkiaSharp — native + managed — from the branch) is leftmost, then "
              f"🌙 the last {DISPLAY_DAYS} nightlies from the "
              f"[EAP feed](https://aka.ms/skiasharp-eap/index.json), then released baselines "
              f"(from nuget.org). Each dot compares a cell to the older column on its right: "
              f"{TREND_FASTER} faster · {TREND_SLOWER} slower; < {TREND_PCT:.0%} is noise.", ""]
    return lines


def render_cross_os_snapshot(histories, oses):
    names = sorted({n for o in oses for n in (latest_day(histories[o].get("nightly")) or {}).get("benchmarks", {})})
    if not names:
        return []
    lines = ["### 🌙 Latest nightly across OS (placeholder — not hardware-normalized)", "",
             "> Cross-OS absolute times are confounded by differing runner hardware; a proper "
             "normalized view is planned. Treat these as directional only.", "",
             "| Benchmark | " + " | ".join(oses) + " | Alloc |", "|:--|" + "|".join(["--:"] * len(oses)) + "|--:|"]
    for name in names:
        tcells = [human_time(metric(latest_day(histories[o].get("nightly")), name)) for o in oses]
        allocs = [metric(latest_day(histories[o].get("nightly")), name, "allocatedBytes") for o in oses]
        alloc = next((a for a in allocs if a is not None), None)
        lines.append(f"| `{short_name(name)}` | " + " | ".join(tcells) + f" | {human_bytes(alloc)} |")
    lines.append("")
    return lines


def render_os_table(roles, os_name, key="meanNs", fmt=human_time):
    cols = build_columns(roles)
    if not cols:
        return [f"### {os_name}", "", "_No data yet._", ""]
    lines = [f"### {os_name}", "",
             "| Benchmark | " + " | ".join(c.header for c in cols) + " |",
             "|:--|" + "|".join(["--:"] * len(cols)) + "|"]
    for name in benchmark_universe(roles):
        values = [column_value(roles, c, name, key) for c in cols]
        lines.append(f"| `{short_name(name)}` | " + " | ".join(row_cells(values, fmt)) + " |")
    lines.append("")
    return lines


def render(histories):
    oses = ordered_oses(histories)
    lines = render_header(histories, oses)
    if not any(latest_day(histories[o].get("nightly")) for o in oses):
        return "\n".join(lines) + "\n"
    lines += render_cross_os_snapshot(histories, oses)
    lines += ["### 📈 Per-OS: time — nightly trend + released baselines", ""]
    for o in oses:
        lines += render_os_table(histories[o], o)
    # Allocations are deterministic and hardware-independent — the signal we gate hardest
    # on — so surface them with the same column model. A red dot here is a real regression.
    lines += ["### 📦 Per-OS: allocations (bytes/op)", ""]
    for o in oses:
        lines += render_os_table(histories[o], o, key="allocatedBytes", fmt=human_bytes)
    return "\n".join(lines) + "\n"


def main(argv):
    if len(argv) != 1:
        print("usage: render-benchmarks.py <input-dir>", file=sys.stderr)
        return 2
    histories = load_histories(argv[0])
    if not histories:
        print(f"No benchmarks-*.json files found under {argv[0]!r}", file=sys.stderr)
        return 1
    md = render(histories)
    print(md)
    target = os.environ.get("GITHUB_STEP_SUMMARY")
    if target:
        with open(target, "a", encoding="utf-8") as fh:
            fh.write(md)
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
