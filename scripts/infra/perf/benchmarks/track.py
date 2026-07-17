#!/usr/bin/env python3
"""Append a BenchmarkDotNet run to a per-OS rolling benchmark history.

This is the benchmark-tracking counterpart to ``track-artifact-sizes.py``. It
reads the ``*-report-full.json`` files BenchmarkDotNet writes to its artifacts
``results`` directory and records, for every benchmark, the mean/median time (in
nanoseconds) and the allocated bytes per operation.

Design notes
------------
* One history file **per operating system** (Linux / Windows / macOS). Each CI
  matrix leg owns its own file, so the OSes never mix.
* A benchmark is keyed by its BenchmarkDotNet ``FullName``
  (``Namespace.Type.Method(Param: value)``). That name is the primary key of the
  time series -- renaming a benchmark starts a new series (by design).
* Each run records its own point, keyed by a full ISO-8601 UTC **datetime** (the
  ``date`` field). This is a loosening of the earlier day granularity: the workflow
  now runs several times a day, so intra-day points accumulate and the dashboard's
  time axis shows hours. Re-running at the exact same timestamp replaces in place.
* Every point also records two descriptors (metadata only -- neither is used to skip):
  a ``fingerprint`` (``version | hash(benchmark source)``) so a perf change with an
  unchanged fingerprint points at the environment rather than the code; and an ``env``
  block (runner CPU, physical/logical cores, arch, OS, runtime -- straight from the
  BenchmarkDotNet report's HostEnvironmentInfo) so a hardware/runtime change is visible.
* History is retained for ``MAX_AGE_DAYS`` days, so changing the run cadence only
  changes point density -- never the length of the retained window.

Only the Python standard library is used so the script runs on a clean runner.
The history document is persisted between runs on the ``aw-data`` branch; see
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
MAX_AGE_DAYS = 365  # age of history retained on disk (dashboard zooms across all of it)

# This tracker is specific to the tracking projects, so its locations are constants.
# From scripts/infra/perf/benchmarks/track.py, the repo root is parents[4].
BENCH_ROOT = pathlib.Path(__file__).resolve().parents[4] / "benchmarks"
PROJECT_DIR = BENCH_ROOT / "SkiaSharp.Benchmarks.Tracking"
SOURCE_PROJECT_DIR = BENCH_ROOT / "SkiaSharp.Benchmarks.Tracking.Source"


def _results_dir(role: str) -> pathlib.Path:
    # The "pr" role runs the source-build project, which writes to its own artifacts dir.
    base = SOURCE_PROJECT_DIR if role == "pr" else PROJECT_DIR
    return base / "BenchmarkDotNet.Artifacts" / "results"


def _point_time(entry: dict) -> dt.datetime | None:
    """Parse a point's ``date`` into an aware UTC datetime.

    Tolerates both the legacy day granularity (``YYYY-MM-DD``) and the current full
    ISO-8601 UTC datetime (``YYYY-MM-DDTHH:MM:SSZ``), so old and new points coexist.
    Returns ``None`` when the value is missing or unparseable.
    """
    raw = (entry or {}).get("date")
    if not isinstance(raw, str) or not raw:
        return None
    s = raw.strip()
    if s.endswith("Z"):  # fromisoformat only learned 'Z' in 3.11
        s = s[:-1] + "+00:00"
    try:
        parsed = dt.datetime.fromisoformat(s)
    except ValueError:
        return None
    if parsed.tzinfo is None:
        parsed = parsed.replace(tzinfo=dt.timezone.utc)
    return parsed


def _source_hash() -> str:
    """Short hash of the benchmark source (.cs + .csproj). Combined with the pinned
    version it forms a point's ``fingerprint`` -- recorded so the dashboard can tell a
    same-code/same-version perf shift (environment) from a real code/version change."""
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
    """Identifies the benchmarked code state: pinned version + benchmark source hash.
    Recorded on every point (never used to skip a run)."""
    return f"{version}|{_source_hash()}"


def _extract_env(doc: dict) -> dict:
    """Compact host/environment descriptor from a BDN report's ``HostEnvironmentInfo``.

    Recorded per point so the dashboard can attribute a perf shift to a changed runner
    (different CPU model, core count, arch, OS or runtime) rather than a code regression.
    Keys BDN did not provide are dropped so the record stays clean.
    """
    h = doc.get("HostEnvironmentInfo") or {}
    env = {
        "cpu": h.get("ProcessorName"),
        "physicalCpus": h.get("PhysicalProcessorCount"),
        "physicalCores": h.get("PhysicalCoreCount"),
        "logicalCores": h.get("LogicalCoreCount"),
        "arch": h.get("Architecture"),
        "os": h.get("OsVersion"),
        "runtime": h.get("RuntimeVersion"),
        "dotnetSdk": h.get("DotNetCliVersion"),
        "bdnVersion": h.get("BenchmarkDotNetVersion"),
    }
    return {k: v for k, v in env.items() if v is not None}



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

def parse_results(results_dir: str) -> tuple[dict[str, dict], dict]:
    """Merge every ``*-report-full.json`` in ``results_dir`` into one map.

    Returns ``(benchmarks, env)`` where ``env`` is the host descriptor (see
    ``_extract_env``) taken from the first report -- every report in a run shares the
    same host, so one is representative.

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
    env: dict = {}
    for path in files:
        try:
            with open(path, "r", encoding="utf-8") as fh:
                doc = json.load(fh)
        except (json.JSONDecodeError, OSError) as err:
            _log(f"  ! skipping unreadable report {path}: {err}")
            continue

        if not env:
            env = _extract_env(doc)

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
    return merged, env


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
         f"{len(history['days'])} point(s))")


def upsert_day(history: dict, entry: dict, max_age_days: int) -> None:
    """Insert/replace the point for its exact ``date`` key, keep chronological, and
    retain points within ``max_age_days``.

    ``date`` is now a full datetime, so each run is its own point; an idempotent
    re-run at the same timestamp replaces in place. Retention is age-based (not a
    fixed count), so changing the run cadence never shrinks the retained window.
    """
    _epoch = dt.datetime.min.replace(tzinfo=dt.timezone.utc)
    points = [d for d in history.get("days", []) if d.get("date") != entry["date"]]
    points.append(entry)
    points.sort(key=lambda d: _point_time(d) or _epoch)
    cutoff = dt.datetime.now(dt.timezone.utc) - dt.timedelta(days=max_age_days)
    # Keep points newer than the cutoff; unparseable dates are kept (never silently dropped).
    history["days"] = [d for d in points
                       if (t := _point_time(d)) is None or t >= cutoff]


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
    return p.parse_args(argv)


def main(argv: list[str]) -> int:
    args = parse_args(argv)

    _log(f"Collecting benchmark results for {args.os}...")
    benchmarks, env = parse_results(str(_results_dir(args.role)))
    if not benchmarks:
        _log("  no benchmarks parsed; leaving history unchanged")
        return 1

    history = load_history(args.history, args.os, args.role)
    now = dt.datetime.now(dt.timezone.utc).strftime("%Y-%m-%dT%H:%M:%SZ")
    point = {
        "date": now,
        "version": args.version,
        "fingerprint": fingerprint(args.version),
        "benchmarks": benchmarks,
    }
    if env:
        point["env"] = env
    upsert_day(history, point, MAX_AGE_DAYS)
    save_history(args.history, history)
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
