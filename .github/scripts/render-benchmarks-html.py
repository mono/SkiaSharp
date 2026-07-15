#!/usr/bin/env python3
"""Produce the self-contained HTML dashboard and the index.json manifest.

Usage: render-benchmarks-html.py <histories-dir> <template-html> <out-html> <out-index-json>

* Reads every ``benchmarks-*.json`` in ``<histories-dir>``.
* Injects them as ``window.__BENCH_DATA__`` into a copy of ``<template-html>`` so the
  result is an OFFLINE single file (download from a run artifact, open in a browser).
* Writes ``<out-index-json>`` — the manifest the *live* (non-embedded) page fetches
  from the ``aw-data`` branch to discover the history files.

Only the standard library is used.
"""

from __future__ import annotations

import datetime as dt
import glob
import json
import os
import sys


def main(argv: list[str]) -> int:
    if len(argv) != 4:
        print("usage: render-benchmarks-html.py <histories-dir> <template-html> <out-html> <out-index-json>",
              file=sys.stderr)
        return 2
    hist_dir, template, out_html, out_index = argv

    files = sorted(glob.glob(os.path.join(hist_dir, "benchmarks-*.json")))
    data: dict[str, dict] = {}
    for path in files:
        try:
            with open(path, "r", encoding="utf-8") as fh:
                data[os.path.basename(path)] = json.load(fh)
        except (json.JSONDecodeError, OSError) as err:
            print(f"  ! skipping {path}: {err}", file=sys.stderr)

    with open(template, "r", encoding="utf-8") as fh:
        html = fh.read()

    # Inject the embedded data blob right before the final </body>. json.dumps is
    # HTML-safe here (numbers/strings from our own tooling); still escape </ in case a
    # benchmark name ever contains it. We target the LAST </body> so a literal "</body>"
    # elsewhere (e.g. a comment) can't be matched by mistake.
    blob = json.dumps(data, separators=(",", ":")).replace("</", "<\\/")
    inject = f"<script>window.__BENCH_DATA__ = {blob};</script>\n</body>"
    idx = html.rfind("</body>")
    if idx == -1:
        print("  ! template has no </body>; appending data at end", file=sys.stderr)
        html += inject
    else:
        html = html[:idx] + inject + html[idx + len("</body>"):]
    with open(out_html, "w", encoding="utf-8") as fh:
        fh.write(html)

    manifest = {
        "generatedUtc": dt.datetime.now(dt.timezone.utc).strftime("%Y-%m-%dT%H:%M:%SZ"),
        "files": [os.path.basename(p) for p in files],
    }
    with open(out_index, "w", encoding="utf-8") as fh:
        json.dump(manifest, fh, indent=2)

    print(f"  wrote {out_html} ({os.path.getsize(out_html) // 1024} KB, {len(data)} histories) "
          f"and {out_index} ({len(manifest['files'])} files)")
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
