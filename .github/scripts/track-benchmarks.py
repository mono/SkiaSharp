#!/usr/bin/env python3
"""Append a BenchmarkDotNet run to a per-OS rolling benchmark history.

This is the benchmark-tracking counterpart to ``track-artifact-sizes.py``. It
reads the ``*-report-full.json`` files BenchmarkDotNet writes to its artifacts
``results`` directory and records, for every benchmark, the mean/median time (in
nanoseconds) and the allocated bytes per operation.

Design notes
------------
* One history file **per operating system** (Linux / Windows / macOS). Each CI
  matrix leg owns its own file and its own Actions cache, so the OSes never mix.
* A benchmark is keyed by its BenchmarkDotNet ``FullName``
  (``Namespace.Type.Method(Param: value)``). That name is the primary key of the
  time series -- renaming a benchmark starts a new series (by design).
* History is retained for ``--max-days`` days (default 60) even though the
  dashboard only displays the most recent 10. Keeping more lets us look back
  further later without re-running anything.

Only the Python standard library is used so the script runs on a clean runner.
The history document is persisted between runs via the GitHub Actions cache; see
``.github/workflows/track-benchmarks.yml``.
"""

from __future__ import annotations

import argparse
import datetime as dt
import glob
import json
import os
import sys

SCHEMA_VERSION = 1


# --------------------------------------------------------------------------- #
# Helpers
# --------------------------------------------------------------------------- #

def _log(msg: str) -> None:
    print(msg, flush=True)


def _num(value) -> float | None:
    """Coerce a JSON value to float, tolerating None/strings/missing."""
    if value is None:
        return None
    try:
        return float(value)
    except (TypeError, ValueError):
        return None


# --------------------------------------------------------------------------- #
# BenchmarkDotNet result parsing
# --------------------------------------------------------------------------- #

def parse_results(results_dir: str) -> dict[str, dict]:
    """Merge every ``*-report-full.json`` in ``results_dir`` into one map.

    Returns ``{ fullName: { meanNs, medianNs, stdDevNs, allocatedBytes } }``.
    """
    pattern = os.path.join(results_dir, "*-report-full.json")
    files = sorted(glob.glob(pattern))
    if not files:
        raise FileNotFoundError(
            f"No '*-report-full.json' files under {results_dir!r}. "
            "Did BenchmarkDotNet run with the JSON exporter enabled?"
        )

    merged: dict[str, dict] = {}
    for path in files:
        try:
            with open(path, "r", encoding="utf-8") as fh:
                doc = json.load(fh)
        except (json.JSONDecodeError, OSError) as err:
            _log(f"  ! skipping unreadable report {path}: {err}")
            continue

        for bench in doc.get("Benchmarks", []):
            name = bench.get("FullName")
            if not name:
                continue
            stats = bench.get("Statistics") or {}
            memory = bench.get("Memory") or {}
            merged[name] = {
                "meanNs": _num(stats.get("Mean")),
                "medianNs": _num(stats.get("Median")),
                "stdDevNs": _num(stats.get("StandardDeviation")),
                "allocatedBytes": _num(memory.get("BytesAllocatedPerOperation")),
            }

    _log(f"  parsed {len(merged)} benchmark(s) from {len(files)} report file(s)")
    return merged


# --------------------------------------------------------------------------- #
# History document
# --------------------------------------------------------------------------- #

def load_history(path: str, os_name: str, role: str) -> dict:
    if os.path.exists(path):
        try:
            with open(path, "r", encoding="utf-8") as fh:
                data = json.load(fh)
            if isinstance(data, dict) and data.get("schema") == SCHEMA_VERSION:
                data.setdefault("os", os_name)
                data.setdefault("role", role)
                data.setdefault("days", [])
                return data
            _log(f"  history schema mismatch (found {data.get('schema')}); starting fresh")
        except (json.JSONDecodeError, OSError) as err:
            _log(f"  could not read history ({err}); starting fresh")
    return {"schema": SCHEMA_VERSION, "os": os_name, "role": role, "days": []}


def save_history(path: str, history: dict) -> None:
    history["schema"] = SCHEMA_VERSION
    history["updatedUtc"] = dt.datetime.now(dt.timezone.utc).strftime("%Y-%m-%dT%H:%M:%SZ")
    tmp = f"{path}.tmp"
    with open(tmp, "w", encoding="utf-8") as fh:
        json.dump(history, fh, indent=2, sort_keys=True)
    os.replace(tmp, path)
    _log(f"  wrote history -> {path} ({os.path.getsize(path) / 1024:.0f} KB, "
         f"{len(history['days'])} day(s))")


def upsert_day(history: dict, entry: dict, max_days: int) -> None:
    """Insert/replace the entry for its date, keep chronological, retain N days."""
    days = [d for d in history.get("days", []) if d.get("date") != entry["date"]]
    days.append(entry)
    days.sort(key=lambda d: d["date"])
    history["days"] = days[-max_days:]


# --------------------------------------------------------------------------- #
# Main
# --------------------------------------------------------------------------- #

def parse_args(argv: list[str]) -> argparse.Namespace:
    p = argparse.ArgumentParser(description=__doc__,
                                formatter_class=argparse.RawDescriptionHelpFormatter)
    p.add_argument("--results", required=True,
                   help="BenchmarkDotNet results directory (contains *-report-full.json).")
    p.add_argument("--history", required=True,
                   help="Path to the per-OS JSON history document (read + written).")
    p.add_argument("--os", required=True,
                   help="Operating system label for this run (e.g. Linux/Windows/macOS).")
    p.add_argument("--role", default="nightly",
                   help="Version role this run measures (nightly/curr-stable/prev-stable/prev-major).")
    p.add_argument("--version", default="unknown",
                   help="SkiaSharp package version that was benchmarked (metadata).")
    p.add_argument("--date", default=None,
                   help="Observation date (YYYY-MM-DD; default: today UTC).")
    p.add_argument("--max-days", type=int, default=60,
                   help="Number of days to retain in history (default: 60).")
    return p.parse_args(argv)


def main(argv: list[str]) -> int:
    args = parse_args(argv)

    _log(f"Collecting benchmark results for {args.os}...")
    benchmarks = parse_results(args.results)
    if not benchmarks:
        _log("  no benchmarks parsed; leaving history unchanged")
        return 1

    history = load_history(args.history, args.os, args.role)
    today = args.date or dt.datetime.now(dt.timezone.utc).strftime("%Y-%m-%d")
    upsert_day(history, {
        "date": today,
        "version": args.version,
        "benchmarks": benchmarks,
    }, args.max_days)
    save_history(args.history, history)
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
