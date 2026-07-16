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
* History is retained for ``MAX_DAYS`` days even though the dashboard only shows
  the most recent 10. Keeping more lets us look back further without re-running.
* A leg is skipped (``--check`` prints ``skip``) when its fingerprint -- the pinned
  version plus a hash of the benchmark source -- matches the last recorded one, so
  unchanged baselines aren't re-run every day.

Only the Python standard library is used so the script runs on a clean runner.
The history document is persisted between runs via the GitHub Actions cache; see
``.github/workflows/track-benchmarks.yml``.
"""

from __future__ import annotations

import argparse
import datetime as dt
import glob
import hashlib
import json
import os
import pathlib
import sys

SCHEMA_VERSION = 1
MAX_DAYS = 365  # days of history retained on disk (dashboard zooms across all of it)

# This tracker is specific to the tracking projects, so its locations are constants.
# From scripts/infra/perf/benchmarks/track.py, the repo root is parents[4].
BENCH_ROOT = pathlib.Path(__file__).resolve().parents[4] / "benchmarks"
PROJECT_DIR = BENCH_ROOT / "SkiaSharp.Benchmarks.Tracking"
SOURCE_PROJECT_DIR = BENCH_ROOT / "SkiaSharp.Benchmarks.Tracking.Source"


def _results_dir(role: str) -> pathlib.Path:
    # The "pr" role runs the source-build project, which writes to its own artifacts dir.
    base = SOURCE_PROJECT_DIR if role == "pr" else PROJECT_DIR
    return base / "BenchmarkDotNet.Artifacts" / "results"


def _source_hash() -> str:
    """Short hash of the benchmark source (.cs + .csproj) — changes when a
    benchmark is edited, so an unchanged baseline can be detected and skipped."""
    root = PROJECT_DIR
    files = sorted(p for p in root.rglob("*")
                   if p.suffix in (".cs", ".csproj")
                   and "bin" not in p.parts and "obj" not in p.parts)
    h = hashlib.sha256()
    for p in files:
        h.update(p.relative_to(root).as_posix().encode())
        h.update(b"\0")
        h.update(p.read_bytes())
    return h.hexdigest()[:16]


def fingerprint(version: str) -> str:
    """A leg is unchanged when its pinned version and benchmark source are unchanged."""
    return f"{version}|{_source_hash()}"



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

    Records a superset of BenchmarkDotNet statistics so future dashboard work stays
    HTML-only (no re-collection): mean/median/min/max/p95/stdDev nanoseconds, allocated
    bytes, and GC gen-0/1/2 collection counts. The dashboard reads whichever it needs and
    ignores the rest.
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
            pct = stats.get("Percentiles") or {}
            memory = bench.get("Memory") or {}
            merged[name] = {
                "meanNs": _num(stats.get("Mean")),
                "medianNs": _num(stats.get("Median")),
                "minNs": _num(stats.get("Min")),
                "maxNs": _num(stats.get("Max")),
                "p95Ns": _num(pct.get("P95")),
                "stdDevNs": _num(stats.get("StandardDeviation")),
                "allocatedBytes": _num(memory.get("BytesAllocatedPerOperation")),
                "gen0": _num(memory.get("Gen0Collections")),
                "gen1": _num(memory.get("Gen1Collections")),
                "gen2": _num(memory.get("Gen2Collections")),
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
    p.add_argument("--history", required=True,
                   help="Path to the per-(OS, role) JSON history document.")
    p.add_argument("--version", required=True,
                   help="SkiaSharp package version being benchmarked.")
    p.add_argument("--os", default="?", help="OS label (stored for the dashboard).")
    p.add_argument("--role", default="nightly", help="Version role (stored for the dashboard).")
    p.add_argument("--date", default=None,
                   help="Override the data point's date (YYYY-MM-DD, UTC). Defaults to today. "
                        "Used to BACKFILL a historical point dated by the version's publish date; "
                        "the normal daily path omits it and gets today.")
    p.add_argument("--check", action="store_true",
                   help="Print 'skip' if this leg's fingerprint matches the last recorded "
                        "one (unchanged version + benchmark source), else 'run'. Records nothing.")
    return p.parse_args(argv)


def _last_fingerprint(history_path: str) -> str:
    if not os.path.exists(history_path):
        return ""
    try:
        with open(history_path, "r", encoding="utf-8") as fh:
            days = (json.load(fh) or {}).get("days") or []
        return days[-1].get("fingerprint", "") if days else ""
    except (json.JSONDecodeError, OSError):
        return ""


def main(argv: list[str]) -> int:
    args = parse_args(argv)
    fp = fingerprint(args.version)

    if args.check:
        last = _last_fingerprint(args.history)
        print("skip" if (last and fp == last) else "run")
        return 0

    _log(f"Collecting benchmark results for {args.os}...")
    benchmarks = parse_results(str(_results_dir(args.role)))
    if not benchmarks:
        _log("  no benchmarks parsed; leaving history unchanged")
        return 1

    history = load_history(args.history, args.os, args.role)
    if args.date is not None:
        try:
            day = dt.datetime.strptime(args.date, "%Y-%m-%d").strftime("%Y-%m-%d")
        except ValueError:
            _log(f"  invalid --date {args.date!r}; expected YYYY-MM-DD")
            return 2
    else:
        day = dt.datetime.now(dt.timezone.utc).strftime("%Y-%m-%d")
    upsert_day(history, {
        "date": day,
        "version": args.version,
        "fingerprint": fp,
        "benchmarks": benchmarks,
    }, MAX_DAYS)
    save_history(args.history, history)
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
