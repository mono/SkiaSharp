#!/usr/bin/env python3
"""Merge the per-(OS, role) benchmark history files into one summary document.

Usage: merge_summaries.py <histories-dir> <out-json> [--drop-role ROLE]

Reads every ``benchmarks-*.json`` in ``<histories-dir>`` (each is
``{schema, os, role, days}``) and writes a single merged document:

    {"schema": 2, "generatedUtc": "...", "legs": {"<OS>|<role>": {os, role, days}, ...}}

``--drop-role`` omits a role (used to keep the ephemeral ``pr`` leg out of the persisted
branch summary). The dashboard reads this merged file directly (benchmarks/index.json).
Only the standard library is used.
"""

from __future__ import annotations

import argparse
import datetime as dt
import glob
import json
import os
import sys


def main(argv: list[str]) -> int:
    p = argparse.ArgumentParser(description=__doc__,
                                formatter_class=argparse.RawDescriptionHelpFormatter)
    p.add_argument("histories_dir")
    p.add_argument("out_json")
    p.add_argument("--drop-role", default=None, help="Role to exclude (e.g. 'pr').")
    args = p.parse_args(argv)

    legs: dict[str, dict] = {}
    for path in sorted(glob.glob(os.path.join(args.histories_dir, "benchmarks-*.json"))):
        try:
            with open(path, "r", encoding="utf-8") as fh:
                doc = json.load(fh)
        except (json.JSONDecodeError, OSError) as err:
            print(f"  ! skipping {path}: {err}", file=sys.stderr)
            continue
        role = doc.get("role", "nightly")
        if args.drop_role and role == args.drop_role:
            continue
        legs[f"{doc.get('os', '?')}|{role}"] = doc

    merged = {
        "schema": 2,
        "generatedUtc": dt.datetime.now(dt.timezone.utc).strftime("%Y-%m-%dT%H:%M:%SZ"),
        "legs": legs,
    }
    with open(args.out_json, "w", encoding="utf-8") as fh:
        json.dump(merged, fh, indent=2, sort_keys=True)
    print(f"  merged {len(legs)} leg(s) -> {args.out_json}")
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
