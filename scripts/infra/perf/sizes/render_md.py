#!/usr/bin/env python3
"""Render the artifact-size history JSON as a Markdown dashboard.

Output is written to ``$GITHUB_STEP_SUMMARY`` when that variable is set (so the
tables show up on the workflow run summary) and always echoed to stdout.

Two tables are produced across the same 14 columns
(``--max-nightly`` daily nightly builds + 4 released reference roles):

  1. **All packages** -- top-level ``.nupkg`` size for every package.
  2. **Native packages** -- per os/arch size of each native binary. Packages
     whose binaries did not change across the window are collapsed.

The history document is produced by ``track-artifact-sizes.py``.
"""

from __future__ import annotations

import argparse
import json
import os
import re
import sys

ROLES = ("prev-major", "prev-stable", "curr-stable", "latest")

# Column display order for the released reference roles: newest → oldest, so
# that (together with the nightly days) the table reads newest on the left and
# oldest on the right.
COLUMN_ROLE_ORDER = ("latest", "curr-stable", "prev-stable", "prev-major")

ROLE_LABELS = {
    "prev-major": "prev&#8209;major",
    "prev-stable": "prev&#8209;stable",
    "curr-stable": "curr&#8209;stable",
    "latest": "latest",
}

# Trend dots comparing a cell to the next-older column (to its right):
# green = smaller (improvement), red = larger (regression).
TREND_SMALLER = "🟢"
TREND_LARGER = "🔴"

# Size changes smaller than this are treated as noise and left unmarked, so
# tiny byte-level churn (e.g. non-deterministic rebuilds) doesn't paint the
# table red/green. 50 KB ≈ 0.05 MB, about half the display's rounding bucket.
TREND_MIN_DELTA_BYTES = 50 * 1024


# --------------------------------------------------------------------------- #
# Formatting helpers
# --------------------------------------------------------------------------- #

def human(size: int | None) -> str:
    """Format a byte count as a compact human-readable string."""
    if size is None:
        return "&middot;"
    if size < 1024:
        return f"{size} B"
    units = ["KB", "MB", "GB"]
    value = float(size)
    for unit in units:
        value /= 1024.0
        if value < 1024.0 or unit == units[-1]:
            return f"{value:.1f} {unit}"
    return f"{value:.1f} GB"


def trend_marker(curr: int | None, older: int | None) -> str:
    """Coloured dot comparing a cell to the next-older column (to its right).

    🟢 = smaller than the older version (improvement), 🔴 = larger (regression).
    Empty when there is no older value, the size is unchanged, or the change is
    below ``TREND_MIN_DELTA_BYTES`` (treated as noise).
    """
    if curr is None or older is None:
        return ""
    diff = curr - older
    if abs(diff) < TREND_MIN_DELTA_BYTES:
        return ""
    return f" {TREND_LARGER}" if diff > 0 else f" {TREND_SMALLER}"


def row_cells(values: list[int | None]) -> list[str]:
    """Render a row of size cells. Columns run newest → oldest (left → right),
    so each cell is compared to the nearest non-empty cell to its *right* (the
    older version) and tagged 🟢 (smaller) / 🔴 (larger)."""
    cells: list[str] = [""] * len(values)
    older: int | None = None
    for i in range(len(values) - 1, -1, -1):
        val = values[i]
        cells[i] = human(val) + trend_marker(val, older)
        if val is not None:
            older = val
    return cells


def friendly_native_label(path: str) -> str:
    """Turn a native archive path into a compact os/arch label.

    ``runtimes/win-x64/native/libSkiaSharp.dll``                 -> ``win-x64``
    ``runtimes/ios/native/libSkiaSharp.framework/libSkiaSharp``  -> ``ios (framework)``
    ``runtimes/maccatalyst/native/libSkiaSharp.framework.zip``   -> ``maccatalyst (framework)``
    ``buildTransitive/.../libSkiaSharp.a/3.1.56/mt,simd/libSkiaSharp.a`` -> ``wasm 3.1.56/mt,simd``
    """
    m = re.match(r"runtimes/([^/]+)/native/(.+)", path)
    if m:
        rid, rest = m.group(1), m.group(2)
        if ".framework" in rest:
            return f"{rid} (framework)"
        return rid
    # WebAssembly static libs: ``.../lib<X>.a/<emver>/<variant>/<leaf>``. The
    # leaf is kept because some packages ship two files per variant.
    m = re.search(r"lib\w+\.a/([^/]+)/([^/]+)/([^/]+)$", path)
    if m:
        emver, variant, leaf = m.groups()
        return f"wasm {emver}/{variant} · {leaf}"
    # Fallback: last two path segments.
    return "/".join(path.split("/")[-2:])


# --------------------------------------------------------------------------- #
# Column model
# --------------------------------------------------------------------------- #

class Column:
    """One table column: a nightly day or a released reference role."""

    def __init__(self, key: str, header: str, kind: str):
        self.key = key          # date (nightly) or role name (released)
        self.header = header    # markdown header text
        self.kind = kind        # "nightly" | "released"


def build_columns(history: dict) -> list[Column]:
    """Columns run newest → oldest, left → right:

        [ nightly today ... nightly oldest ] [ latest ] [ curr-stable ]
        [ prev-stable ] [ prev-major ]
    """
    columns: list[Column] = []
    # Nightly days, newest first (history is stored oldest -> newest).
    for entry in reversed(history.get("nightly", [])):
        date = entry.get("date", "")
        short_date = date[5:] if len(date) >= 10 else date
        columns.append(Column(date, f"🌙 {short_date}", "nightly"))
    # Released reference roles, newest -> oldest.
    for role in COLUMN_ROLE_ORDER:
        columns.append(Column(role, ROLE_LABELS[role], "released"))
    return columns


def nightly_by_date(history: dict) -> dict[str, dict]:
    return {e["date"]: e for e in history.get("nightly", [])}


def package_nupkg(history: dict, column: Column, package_id: str,
                  nights: dict[str, dict]) -> int | None:
    if column.kind == "nightly":
        entry = nights.get(column.key, {})
        pkg = entry.get("packages", {}).get(package_id)
        return pkg.get("nupkg") if pkg else None
    version = history.get("roles", {}).get(column.key, {}).get(package_id)
    if not version:
        return None
    cached = history.get("releasedCache", {}).get(f"{package_id}@{version}")
    return cached.get("nupkg") if cached else None


def package_natives(history: dict, column: Column, package_id: str,
                    nights: dict[str, dict]) -> dict[str, int]:
    if column.kind == "nightly":
        entry = nights.get(column.key, {})
        pkg = entry.get("packages", {}).get(package_id)
        return pkg.get("natives", {}) if pkg else {}
    version = history.get("roles", {}).get(column.key, {}).get(package_id)
    if not version:
        return {}
    cached = history.get("releasedCache", {}).get(f"{package_id}@{version}")
    return cached.get("natives", {}) if cached else {}


# --------------------------------------------------------------------------- #
# Universe discovery
# --------------------------------------------------------------------------- #

def all_package_ids(history: dict) -> list[str]:
    ids: set[str] = set()
    for entry in history.get("nightly", []):
        ids.update(entry.get("packages", {}).keys())
    for role_map in history.get("roles", {}).values():
        ids.update(role_map.keys())
    return sorted(ids)


def is_native_package(history: dict, package_id: str) -> bool:
    for entry in history.get("nightly", []):
        pkg = entry.get("packages", {}).get(package_id)
        if pkg and pkg.get("natives"):
            return True
    for role, role_map in history.get("roles", {}).items():
        version = role_map.get(package_id)
        if version:
            cached = history.get("releasedCache", {}).get(f"{package_id}@{version}")
            if cached and cached.get("natives"):
                return True
    return False


# --------------------------------------------------------------------------- #
# Table rendering
# --------------------------------------------------------------------------- #

def _legend() -> list[str]:
    return [
        "> Columns run newest → oldest (left → right): 🌙 daily nightlies from "
        "the [EAP feed](https://aka.ms/skiasharp-eap/index.json), then the "
        "released reference versions `latest → curr-stable → prev-stable → "
        "prev-major` from NuGet.org. "
        f"Each dot compares a cell to the older column on its right: "
        f"{TREND_SMALLER} smaller (improvement) · {TREND_LARGER} larger (regression); "
        "changes under 50 KB are treated as noise and left unmarked.",
        "",
    ]


def render_all_packages_table(history: dict, columns: list[Column],
                              nights: dict[str, dict]) -> list[str]:
    lines = ["### 📦 All packages — `.nupkg` size", ""]
    lines += _legend()
    header = "| Package | " + " | ".join(c.header for c in columns) + " |"
    sep = "|:--|" + "|".join(["--:"] * len(columns)) + "|"
    lines += [header, sep]

    for pid in all_package_ids(history):
        values = [package_nupkg(history, col, pid, nights) for col in columns]
        cells = row_cells(values)
        lines.append(f"| `{pid}` | " + " | ".join(cells) + " |")
    lines.append("")
    return lines


def render_native_tables(history: dict, columns: list[Column],
                         nights: dict[str, dict]) -> list[str]:
    lines = ["### 🧬 Native packages — per os/arch binary size", ""]
    lines += _legend()
    native_ids = [p for p in all_package_ids(history) if is_native_package(history, p)]
    if not native_ids:
        return lines + ["_No native packages recorded yet._", ""]

    changed_blocks: list[list[str]] = []
    unchanged_blocks: list[list[str]] = []
    for pid in native_ids:
        block, changed = _render_one_native_table(history, columns, pid, nights)
        (changed_blocks if changed else unchanged_blocks).append(block)

    for block in changed_blocks:
        lines += block

    if unchanged_blocks:
        lines += [
            "<details>",
            f"<summary>Unchanged native packages ({len(unchanged_blocks)})</summary>",
            "",
        ]
        for block in unchanged_blocks:
            lines += block
        lines += ["</details>", ""]
    return lines


def _render_one_native_table(history: dict, columns: list[Column], pid: str,
                             nights: dict[str, dict]) -> tuple[list[str], bool]:
    # Collect every native path ever seen for this package.
    paths: set[str] = set()
    for col in columns:
        paths.update(package_natives(history, col, pid, nights).keys())
    ordered_paths = sorted(paths)

    header = "| os/arch | " + " | ".join(c.header for c in columns) + " |"
    sep = "|:--|" + "|".join(["--:"] * len(columns)) + "|"
    rows = [f"#### `{pid}`", "", header, sep]

    changed = False
    for path in ordered_paths:
        values = [package_natives(history, col, pid, nights).get(path) for col in columns]
        nightly_values = [v for v, col in zip(values, columns) if col.kind == "nightly"]
        if _row_changed(nightly_values):
            changed = True
        cells = row_cells(values)
        rows.append(f"| {friendly_native_label(path)} | " + " | ".join(cells) + " |")
    rows.append("")
    return rows, changed


def _row_changed(nightly_values: list[int | None]) -> bool:
    """A row "changed" if, across the nightly window, its size varied or the
    binary was added/removed (an arch appeared or disappeared). Released
    reference columns are shown for comparison but do not drive this flag."""
    present = [v for v in nightly_values if v is not None]
    if len(set(present)) > 1:
        return True
    if present and any(v is None for v in nightly_values):
        return True
    return False


# --------------------------------------------------------------------------- #
# Header / highlights
# --------------------------------------------------------------------------- #

def render_header(history: dict, nights: dict[str, dict]) -> list[str]:
    lines = ["## 📊 SkiaSharp artifact size tracker", ""]
    nightly = history.get("nightly", [])
    if not nightly:
        return lines + ["_No data collected yet._", ""]

    newest = nightly[-1]
    lines += [
        f"**Latest nightly:** `{newest.get('version', '?')}` "
        f"(observed {newest.get('date', '?')}) &middot; "
        f"{len(newest.get('packages', {}))} packages &middot; "
        f"tracking {len(nightly)} day(s) + {len(ROLES)} released refs",
        f"_Updated {history.get('updatedUtc', 'unknown')}._",
        "",
    ]

    # Released reference versions legend (representative version lines).
    roles = history.get("roles", {})
    legend_pkgs = [("SkiaSharp", "SkiaSharp"), ("HarfBuzzSharp", "HarfBuzzSharp")]
    legend_rows = []
    for label, pid in legend_pkgs:
        cols = [roles.get(role, {}).get(pid, "&middot;") for role in COLUMN_ROLE_ORDER]
        if any(c != "&middot;" for c in cols):
            legend_rows.append(f"| {label} | " + " | ".join(f"`{c}`" for c in cols) + " |")
    if legend_rows:
        lines += [
            "**Released reference versions** (from NuGet.org)",
            "",
            "| line | " + " | ".join(ROLE_LABELS[r] for r in COLUMN_ROLE_ORDER) + " |",
            "|:--|" + "|".join([":--"] * len(COLUMN_ROLE_ORDER)) + "|",
            *legend_rows,
            "",
        ]

    lines += _render_movers(history, nights)
    return lines


def _render_movers(history: dict, nights: dict[str, dict]) -> list[str]:
    nightly = history.get("nightly", [])
    if len(nightly) < 2:
        return []
    newest, prev = nightly[-1], nightly[-2]
    movers = []
    for pid in all_package_ids(history):
        curr = (newest.get("packages", {}).get(pid) or {}).get("nupkg")
        was = (prev.get("packages", {}).get(pid) or {}).get("nupkg")
        if curr is not None and was is not None and abs(curr - was) >= TREND_MIN_DELTA_BYTES:
            movers.append((abs(curr - was), curr - was, pid))
    if not movers:
        return []
    movers.sort(reverse=True)
    lines = [
        f"**Biggest day-over-day movers** ({prev.get('date')} → {newest.get('date')})",
        "",
    ]
    for _mag, diff, pid in movers[:5]:
        dot = TREND_LARGER if diff > 0 else TREND_SMALLER
        lines.append(f"- {dot} `{pid}` {'+' if diff > 0 else '-'}{human(abs(diff))}")
    lines.append("")
    return lines


# --------------------------------------------------------------------------- #
# Main
# --------------------------------------------------------------------------- #

def render(history: dict) -> str:
    columns = build_columns(history)
    nights = nightly_by_date(history)
    lines: list[str] = []
    lines += render_header(history, nights)
    if history.get("nightly") or history.get("roles"):
        lines += render_all_packages_table(history, columns, nights)
        lines += render_native_tables(history, columns, nights)
    return "\n".join(lines) + "\n"


def parse_args(argv: list[str]) -> argparse.Namespace:
    p = argparse.ArgumentParser(description=__doc__,
                                formatter_class=argparse.RawDescriptionHelpFormatter)
    p.add_argument("--history", default="artifact-sizes.json",
                   help="Path to the JSON history document.")
    p.add_argument("--output", default=None,
                   help="Write the Markdown here (default: $GITHUB_STEP_SUMMARY, else stdout only).")
    return p.parse_args(argv)


def main(argv: list[str]) -> int:
    args = parse_args(argv)
    if not os.path.exists(args.history):
        print(f"History file not found: {args.history}", file=sys.stderr)
        return 1
    with open(args.history, "r", encoding="utf-8") as fh:
        history = json.load(fh)

    markdown = render(history)
    print(markdown)

    target = args.output or os.environ.get("GITHUB_STEP_SUMMARY")
    if target:
        with open(target, "a", encoding="utf-8") as fh:
            fh.write(markdown)
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
