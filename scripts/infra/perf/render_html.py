#!/usr/bin/env python3
"""Produce the self-contained unified perf dashboard from two merged summary files.

Usage: render_html.py <template-html> <out-html> <benchmarks-json> <sizes-json>

* ``<benchmarks-json>`` — the merged benchmark summary (a ``{"legs": {...}}`` document,
  as persisted at benchmarks/index.json). Pass ``-`` or a missing path for none.
* ``<sizes-json>`` — the size summary document (as persisted at sizes/index.json). Pass
  ``-`` or a missing path for none.

Injects ``window.__PERF_DATA__ = {benchmarks: <bench|{}>, sizes: <size|null>}`` into a
copy of the template so the result is an OFFLINE snapshot of this run (open in a browser;
ECharts still loads from its CDN). With no embedded data the same template fetches the
live merged files from the aw-data branch. Only the standard library is used.
"""

from __future__ import annotations

import json
import os
import sys


def _load(path: str):
    if not path or path == "-" or not os.path.isfile(path):
        return None
    try:
        with open(path, "r", encoding="utf-8") as fh:
            return json.load(fh)
    except (json.JSONDecodeError, OSError) as err:
        print(f"  ! could not read {path}: {err}", file=sys.stderr)
        return None


def main(argv: list[str]) -> int:
    if len(argv) != 4:
        print("usage: render_html.py <template-html> <out-html> <benchmarks-json> <sizes-json>",
              file=sys.stderr)
        return 2
    template, out_html, bench_path, sizes_path = argv

    data = {"benchmarks": _load(bench_path) or {}, "sizes": _load(sizes_path)}

    with open(template, "r", encoding="utf-8") as fh:
        html = fh.read()

    blob = json.dumps(data, separators=(",", ":")).replace("</", "<\\/")
    inject = f"<script>window.__PERF_DATA__ = {blob};</script>\n</body>"
    idx = html.rfind("</body>")
    html = html + inject if idx == -1 else html[:idx] + inject + html[idx + len("</body>"):]

    with open(out_html, "w", encoding="utf-8") as fh:
        fh.write(html)

    legs = (data["benchmarks"] or {}).get("legs", {})
    print(f"  wrote {out_html} ({os.path.getsize(out_html) // 1024} KB; "
          f"{len(legs)} benchmark leg(s), sizes={'yes' if data['sizes'] else 'no'})")
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
