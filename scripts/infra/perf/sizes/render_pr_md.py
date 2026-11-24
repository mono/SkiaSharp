#!/usr/bin/env python3
"""Render a PR artifact-size diff as a Markdown comment.

Compares the packages a PR build produced (``measure_pr.py`` output) against the latest
nightly baseline captured on the ``aw-data`` branch (a ``sizes/raw/<date>.json`` snapshot,
which carries a full per-file breakdown). Emits a comment body with:

  * a total ``.nupkg`` size delta,
  * a per-package table (baseline vs PR, Δ bytes, Δ%), with ⚠️ on notable growth, and
  * a collapsible per-file breakdown (grown / new / removed files, native binaries labelled)
    for every package whose size moved.

Informational only — this never fails a check. The comment is posted/updated by
``.github/workflows/track-artifact-sizes-pr.yml``.
"""

from __future__ import annotations

import argparse
import json
import os
import re
import sys

sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))  # perf/
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))  # sizes/
from render_md import friendly_native_label, human  # noqa: E402
from track import _is_native_entry  # noqa: E402

MARKER = "<!-- skiasharp-pr-artifact-sizes -->"

# Below this, a size change is treated as noise (matches the nightly tracker) and the
# package is considered unchanged.
NOISE_BYTES = 50 * 1024
# A grown package is flagged ⚠️ once it exceeds *either* of these.
WARN_BYTES = 500 * 1024
WARN_PCT = 2.0
# Per-file rows below this are hidden from the breakdown (keeps the comment bounded).
FILE_NOISE_BYTES = 1024
# Cap the per-file rows shown per package.
MAX_FILE_ROWS = 25

# OPC packaging metadata whose file name embeds a fresh GUID every pack, so it always
# reads as new/removed — pure noise in a per-file diff (its ~1 KB is still counted in the
# .nupkg total). Skipped from the per-file breakdown only.
_NOISE_FILE_RE = re.compile(
    r"^package/services/metadata/core-properties/.+\.psmdcp$", re.IGNORECASE)


def _is_noise_file(path: str) -> bool:
    return bool(_NOISE_FILE_RE.match(path))

WARN = "⚠️"
GREW = "🔴"
SHRANK = "🟢"


# --------------------------------------------------------------------------- #
# Formatting
# --------------------------------------------------------------------------- #

def signed_human(delta: int) -> str:
    """A signed, human-readable byte delta, e.g. ``+1.2 MB`` / ``-300 KB`` / ``±0``."""
    if delta == 0:
        return "±0"
    return ("+" if delta > 0 else "−") + human(abs(delta))


def pct(base: int | None, pr: int | None) -> float | None:
    if not base or pr is None:
        return None
    return (pr - base) / base * 100.0


def pct_str(base: int | None, pr: int | None) -> str:
    p = pct(base, pr)
    if p is None:
        return "&middot;"
    return f"{'+' if p > 0 else ''}{p:.1f}%"


def is_warn(base: int | None, pr: int | None) -> bool:
    if base is None or pr is None:
        return False
    delta = pr - base
    if delta <= 0:
        return False
    p = pct(base, pr)
    return delta >= WARN_BYTES or (p is not None and p >= WARN_PCT)


# --------------------------------------------------------------------------- #
# Baseline helpers
# --------------------------------------------------------------------------- #

def baseline_files(baseline_pkg: dict) -> dict[str, int]:
    return baseline_pkg.get("files", {}) if baseline_pkg else {}


def baseline_nupkg(baseline_pkg: dict) -> int | None:
    return baseline_pkg.get("nupkg") if baseline_pkg else None


# --------------------------------------------------------------------------- #
# Rendering
# --------------------------------------------------------------------------- #

def render(pr: dict, baseline: dict | None, *, build_url: str | None) -> str:
    pr_pkgs: dict[str, dict] = pr.get("packages", {})
    base_pkgs: dict[str, dict] = (baseline or {}).get("packages", {})

    lines: list[str] = [MARKER, "## 📦 Artifact size report", ""]

    build_id = pr.get("buildId")
    build_ref = f"[`{build_id}`]({build_url})" if build_url else f"`{build_id}`"
    if baseline:
        lines.append(
            f"Packages from this PR (build {build_ref}) vs the latest nightly baseline "
            f"`{baseline.get('version', '?')}` (observed {baseline.get('date', '?')}).")
    else:
        lines.append(
            f"Packages from this PR (build {build_ref}). "
            "_No nightly baseline was available, so only absolute sizes are shown._")
    lines.append("")

    ids = sorted(set(pr_pkgs) | set(base_pkgs))

    # ---- Total .nupkg size -------------------------------------------------
    total_base = sum(baseline_nupkg(base_pkgs.get(i, {})) or 0 for i in ids)
    total_pr = sum((pr_pkgs.get(i, {}) or {}).get("nupkg") or 0 for i in ids)
    total_delta = total_pr - total_base
    if baseline:
        lines += [
            f"**Total `.nupkg` size:** {human(total_base)} → {human(total_pr)} "
            f"({signed_human(total_delta)}, {pct_str(total_base, total_pr)})",
            "",
        ]
    else:
        lines += [f"**Total `.nupkg` size:** {human(total_pr)}", ""]

    # ---- Per-package table -------------------------------------------------
    changed_rows: list[tuple[int, str]] = []  # (sort magnitude, row)
    unchanged = 0
    for pid in ids:
        base_sz = baseline_nupkg(base_pkgs.get(pid, {}))
        pr_sz = (pr_pkgs.get(pid, {}) or {}).get("nupkg")
        if pr_sz is None:
            row = (f"| `{pid}` | {human(base_sz)} | _removed_ | {signed_human(-(base_sz or 0))} "
                   f"| {pct_str(base_sz, 0)} |")
            changed_rows.append((base_sz or 0, row))
            continue
        if base_sz is None:
            row = f"| `{pid}` | _new_ | {human(pr_sz)} | {signed_human(pr_sz)} | &middot; |"
            changed_rows.append((pr_sz, row))
            continue
        delta = pr_sz - base_sz
        if abs(delta) < NOISE_BYTES:
            unchanged += 1
            continue
        flag = f" {WARN}" if is_warn(base_sz, pr_sz) else ""
        dot = GREW if delta > 0 else SHRANK
        row = (f"| `{pid}`{flag} | {human(base_sz)} | {human(pr_sz)} | "
               f"{dot} {signed_human(delta)} | {pct_str(base_sz, pr_sz)} |")
        changed_rows.append((abs(delta), row))

    if changed_rows:
        changed_rows.sort(key=lambda r: r[0], reverse=True)
        lines += [
            "### Packages",
            "",
            f"> {WARN} marks growth over {human(WARN_BYTES)} or {WARN_PCT:.0f}%. "
            f"Changes under {human(NOISE_BYTES)} are treated as noise.",
            "",
            "| Package | baseline | this PR | Δ | Δ% |",
            "|:--|--:|--:|--:|--:|",
            *[r for _, r in changed_rows],
            "",
        ]
        if unchanged:
            lines.append(f"_+{unchanged} package(s) unchanged (< {human(NOISE_BYTES)})._")
            lines.append("")
    elif baseline:
        lines += ["### Packages", "",
                  f"✅ No package changed by more than {human(NOISE_BYTES)}.", ""]

    # ---- Per-file breakdown ------------------------------------------------
    if baseline:
        file_blocks: list[str] = []
        for pid in ids:
            pr_pkg = pr_pkgs.get(pid)
            base_pkg = base_pkgs.get(pid)
            if pr_pkg is None or base_pkg is None:
                continue  # whole-package add/remove already shown in the table
            base_sz = baseline_nupkg(base_pkg)
            pr_sz = pr_pkg.get("nupkg")
            if base_sz is not None and pr_sz is not None and abs(pr_sz - base_sz) < NOISE_BYTES:
                continue
            block = _render_file_block(pid, pr_pkg, base_pkg)
            if block:
                file_blocks.append(block)
        if file_blocks:
            lines += [
                "<details>",
                "<summary>Per-file changes</summary>",
                "",
                *[line for block in file_blocks for line in block.splitlines()],
                "</details>",
                "",
            ]

    lines.append(
        "<sub>Informational only — this never blocks the PR. "
        "Native binaries are labelled by os/arch.</sub>")
    return "\n".join(lines) + "\n"


def _render_file_block(pid: str, pr_pkg: dict, base_pkg: dict) -> str:
    pr_files: dict[str, int] = pr_pkg.get("files", {})
    base_files: dict[str, int] = baseline_files(base_pkg)
    rows: list[tuple[int, str]] = []
    for path in sorted(set(pr_files) | set(base_files)):
        if _is_noise_file(path):
            continue
        b = base_files.get(path)
        p = pr_files.get(path)
        if b is not None and p is not None:
            delta = p - b
            if abs(delta) < FILE_NOISE_BYTES:
                continue
            dot = GREW if delta > 0 else SHRANK
            state = f"{human(b)} → {human(p)} ({dot} {signed_human(delta)})"
            mag = abs(delta)
        elif p is not None:
            state = f"_new_ → {human(p)}"
            mag = p
        else:
            state = f"{human(b)} → _removed_"
            mag = b or 0
        label = f"`{path}`"
        if _is_native_entry(path):
            label = f"🧬 `{friendly_native_label(path)}` (`{path}`)"
        rows.append((mag, f"| {label} | {state} |"))
    if not rows:
        return ""
    rows.sort(key=lambda r: r[0], reverse=True)
    shown = rows[:MAX_FILE_ROWS]
    more = len(rows) - len(shown)
    out = [f"#### `{pid}`", "", "| File | Size |", "|:--|--:|",
           *[r for _, r in shown]]
    if more:
        out.append(f"| _+{more} more file(s)_ | |")
    out.append("")
    return "\n".join(out)


# --------------------------------------------------------------------------- #
# Main
# --------------------------------------------------------------------------- #

def parse_args(argv: list[str]) -> argparse.Namespace:
    p = argparse.ArgumentParser(description=__doc__,
                                formatter_class=argparse.RawDescriptionHelpFormatter)
    p.add_argument("--pr-sizes", required=True, help="measure_pr.py output JSON.")
    p.add_argument("--baseline", default=None,
                   help="Baseline nightly raw snapshot JSON (sizes/raw/<date>.json). "
                        "Omit / point at a missing file to render absolute sizes only.")
    p.add_argument("--build-url", default=None, help="AzDO build results URL for the header link.")
    p.add_argument("--output", default=None,
                   help="Write the Markdown here (default: stdout). Also appended to "
                        "$GITHUB_STEP_SUMMARY when set.")
    return p.parse_args(argv)


def main(argv: list[str]) -> int:
    args = parse_args(argv)
    with open(args.pr_sizes, "r", encoding="utf-8") as fh:
        pr = json.load(fh)

    baseline = None
    if args.baseline and os.path.exists(args.baseline):
        try:
            with open(args.baseline, "r", encoding="utf-8") as fh:
                baseline = json.load(fh)
        except (json.JSONDecodeError, OSError) as err:
            print(f"warning: could not read baseline ({err}); absolute-only", file=sys.stderr)

    markdown = render(pr, baseline, build_url=args.build_url)

    if args.output:
        with open(args.output, "w", encoding="utf-8") as fh:
            fh.write(markdown)
    else:
        print(markdown)

    summary = os.environ.get("GITHUB_STEP_SUMMARY")
    if summary:
        with open(summary, "a", encoding="utf-8") as fh:
            fh.write(markdown)
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
