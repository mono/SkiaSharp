#!/usr/bin/env python3
"""Best-effort download of one persisted perf dataset folder from the aw-data branch.

Usage: fetch_branch_data.py <raw-base> <subdir> <out-dir>

Reads ``<raw-base>/<subdir>/index.json`` (the manifest written when the folder is
persisted) and downloads every file it lists into ``<out-dir>``. Missing data (e.g. the
branch has no ``sizes/`` yet) is not an error — the caller just embeds an empty dataset
and the dashboard degrades gracefully.

Used so each perf workflow can embed the OTHER dimension (the one it doesn't produce)
into its unified dashboard artifact: the benchmarks workflow fetches ``sizes/``; the
sizes workflow fetches ``benchmarks/``. Only the standard library is used.
"""

from __future__ import annotations

import json
import os
import sys
import urllib.error
import urllib.request

UA = "skiasharp-perf-branch-fetch/1.0 (+https://github.com/mono/SkiaSharp)"


def _get(url: str, timeout: int = 60) -> bytes | None:
    try:
        req = urllib.request.Request(url, headers={"User-Agent": UA})
        with urllib.request.urlopen(req, timeout=timeout) as resp:
            return resp.read()
    except (urllib.error.URLError, TimeoutError, ConnectionError):
        return None


def main(argv: list[str]) -> int:
    if len(argv) != 3:
        print("usage: fetch_branch_data.py <raw-base> <subdir> <out-dir>", file=sys.stderr)
        return 2
    raw_base, subdir, out_dir = argv
    base = f"{raw_base.rstrip('/')}/{subdir.strip('/')}/"
    os.makedirs(out_dir, exist_ok=True)

    idx_raw = _get(base + "index.json")
    if not idx_raw:
        print(f"  no {subdir}/index.json on the data branch (nothing to fetch)")
        return 0
    try:
        files = (json.loads(idx_raw) or {}).get("files", [])
    except json.JSONDecodeError:
        print(f"  {subdir}/index.json was not valid JSON; skipping")
        return 0

    got = 0
    for name in files:
        data = _get(base + name)
        if data is None:
            print(f"  ! could not fetch {subdir}/{name}")
            continue
        with open(os.path.join(out_dir, os.path.basename(name)), "wb") as fh:
            fh.write(data)
        got += 1
    print(f"  fetched {got}/{len(files)} file(s) from {subdir}/ -> {out_dir}")
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
