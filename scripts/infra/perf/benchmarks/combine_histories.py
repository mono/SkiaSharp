#!/usr/bin/env python3
"""Combine many single-day benchmark history files into one merged summary, by unioning days.

The backfill workflow runs one matrix leg per (OS, version) and uploads one
``benchmarks-<OS>-nightly-<version>.json`` artifact each — every file the usual
``{schema, os, role, days:[...]}`` shape, but carrying just the one backfilled day. This
merges them into the schema-2 ``benchmarks/index.json`` the dashboard reads:

    {"schema": 2, "generatedUtc": "...", "legs": {"<OS>|<role>": {os, role, days:[...]}, ...}}

Unlike ``merge_summaries.py`` (which keys legs by ``OS|role`` and OVERWRITES on collision, so
it can't fold many one-day files for the same leg together), this **unions** each leg's days
(dedup by date, incoming wins, chronological). ``--seed`` loads an existing merged index first
so pre-existing legs/days — e.g. the current ``aw-data`` curr-stable/latest/prev-* and the
already-recorded nightly day — are preserved and extended, not lost. Only the standard library.

Usage: combine_histories.py <histories-dir> <out-json> [--seed <existing-index.json>]
"""

from __future__ import annotations

import argparse
import datetime as dt
import glob
import json
import os
import sys


def _upsert_days(leg: dict, incoming: list[dict]) -> None:
    """Merge ``incoming`` day entries into ``leg['days']`` (dedup by date, incoming wins)."""
    by_date = {d["date"]: d for d in leg.get("days", []) if d.get("date")}
    for day in incoming:
        if day.get("date"):
            by_date[day["date"]] = day
    leg["days"] = [by_date[k] for k in sorted(by_date)]


def _load(path: str) -> dict | None:
    try:
        with open(path, "r", encoding="utf-8") as fh:
            return json.load(fh)
    except (json.JSONDecodeError, OSError) as err:
        print(f"  ! skipping {path}: {err}", file=sys.stderr)
        return None


def main(argv: list[str]) -> int:
    p = argparse.ArgumentParser(description=__doc__,
                                formatter_class=argparse.RawDescriptionHelpFormatter)
    p.add_argument("histories_dir")
    p.add_argument("out_json")
    p.add_argument("--seed", default=None,
                   help="Existing merged index.json whose legs/days seed the result.")
    args = p.parse_args(argv)

    legs: dict[str, dict] = {}
    if args.seed:
        seed = _load(args.seed) or {}
        for key, doc in (seed.get("legs") or {}).items():
            legs[key] = {
                "schema": doc.get("schema", 1),
                "os": doc.get("os", key.split("|")[0]),
                "role": doc.get("role", key.split("|")[-1]),
                "days": list(doc.get("days", [])),
            }
        print(f"  seeded {len(legs)} leg(s) from {args.seed}", file=sys.stderr)

    files = sorted(glob.glob(os.path.join(args.histories_dir, "benchmarks-*.json")))
    for path in files:
        doc = _load(path)
        if not isinstance(doc, dict):
            continue
        os_name, role = doc.get("os", "?"), doc.get("role", "nightly")
        key = f"{os_name}|{role}"
        leg = legs.setdefault(key, {"schema": doc.get("schema", 1),
                                    "os": os_name, "role": role, "days": []})
        _upsert_days(leg, doc.get("days", []))

    merged = {
        "schema": 2,
        "generatedUtc": dt.datetime.now(dt.timezone.utc).strftime("%Y-%m-%dT%H:%M:%SZ"),
        "legs": legs,
    }
    with open(args.out_json, "w", encoding="utf-8") as fh:
        json.dump(merged, fh, indent=2, sort_keys=True)
    total_days = sum(len(l.get("days", [])) for l in legs.values())
    print(f"  combined {len(files)} file(s) -> {len(legs)} leg(s), {total_days} day(s) "
          f"-> {args.out_json}", file=sys.stderr)
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
