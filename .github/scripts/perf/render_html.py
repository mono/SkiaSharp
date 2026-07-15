#!/usr/bin/env python3
"""Produce the self-contained unified perf dashboard (time + allocations + size).

Usage:
    render_html.py <template-html> <out-html> <benchmarks-dir> [<sizes-dir>]

* Reads every ``benchmarks-*.json`` in ``<benchmarks-dir>`` and, when given, every
  ``*.json`` in ``<sizes-dir>`` (i.e. ``artifact-sizes.json``).
* Injects them as ``window.__PERF_DATA__ = {benchmarks:{...}, sizes:{...}}`` into a
  copy of ``<template-html>`` so the result is an OFFLINE single file (download it from
  a run artifact and open in a browser). The chart lib (ECharts) still loads from a CDN,
  so rendering needs internet, but the data travels in the file.

Either dataset may be empty — the dashboard degrades gracefully (e.g. the Size tab shows
"no data yet" until the size tracker persists to the branch). Only the standard library
is used.
"""

from __future__ import annotations

import glob
import json
import os
import sys


def _load_dir(path: str | None, pattern: str) -> dict[str, dict]:
    out: dict[str, dict] = {}
    if not path or not os.path.isdir(path):
        return out
    for p in sorted(glob.glob(os.path.join(path, pattern))):
        try:
            with open(p, "r", encoding="utf-8") as fh:
                out[os.path.basename(p)] = json.load(fh)
        except (json.JSONDecodeError, OSError) as err:
            print(f"  ! skipping {p}: {err}", file=sys.stderr)
    return out


def main(argv: list[str]) -> int:
    if len(argv) not in (3, 4):
        print("usage: render_html.py <template-html> <out-html> <benchmarks-dir> [<sizes-dir>]",
              file=sys.stderr)
        return 2
    template, out_html, bench_dir = argv[0], argv[1], argv[2]
    sizes_dir = argv[3] if len(argv) == 4 else None

    data = {
        "benchmarks": _load_dir(bench_dir, "benchmarks-*.json"),
        "sizes": _load_dir(sizes_dir, "*.json"),
    }

    with open(template, "r", encoding="utf-8") as fh:
        html = fh.read()

    # Inject before the LAST </body> so a literal "</body>" in a comment can't match.
    blob = json.dumps(data, separators=(",", ":")).replace("</", "<\\/")
    inject = f"<script>window.__PERF_DATA__ = {blob};</script>\n</body>"
    idx = html.rfind("</body>")
    html = html + inject if idx == -1 else html[:idx] + inject + html[idx + len("</body>"):]

    with open(out_html, "w", encoding="utf-8") as fh:
        fh.write(html)

    print(f"  wrote {out_html} ({os.path.getsize(out_html) // 1024} KB; "
          f"{len(data['benchmarks'])} benchmark + {len(data['sizes'])} size file(s))")
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
