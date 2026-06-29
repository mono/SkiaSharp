#!/usr/bin/env python3
"""Merge BenchmarkDotNet JSON results from several runs into one Markdown report.

The benchmarks CI workflow runs the benchmarks once per (operating system, SkiaSharp
version) combination and uploads each run as an artifact. BenchmarkDotNet cannot host two
versions of the same assembly in a single process, so the comparison is assembled here
afterwards.

Expected input layout (the workflow produces it):

    <root>/
        <any-artifact-folder>/
            meta.json                         # {"os": "...", "version": "..."}
            **/*-report-full-compressed.json  # BenchmarkDotNet JSON export

Usage:
    merge-benchmarks.py <root-dir> [--baseline <version>] [--output report.md]

The baseline version (when present for a given OS) is used to compute a ratio column so
regressions and improvements are obvious at a glance.
"""

import argparse
import json
import os
import sys
from collections import OrderedDict, defaultdict


def find_meta(folder):
    meta_path = os.path.join(folder, "meta.json")
    if not os.path.isfile(meta_path):
        return None
    try:
        with open(meta_path, encoding="utf-8") as f:
            return json.load(f)
    except (OSError, ValueError):
        return None


def find_result_jsons(folder):
    for dirpath, _dirs, files in os.walk(folder):
        for name in files:
            if name.endswith("-report-full-compressed.json"):
                yield os.path.join(dirpath, name)


def load_records(root):
    """Return list of records: os, version, type, method, params, mean_ns."""
    records = []
    for entry in sorted(os.listdir(root)):
        folder = os.path.join(root, entry)
        if not os.path.isdir(folder):
            continue
        meta = find_meta(folder)
        if meta is None:
            continue
        os_name = meta.get("os", "unknown")
        version = meta.get("version", "unknown")
        for json_path in find_result_jsons(folder):
            try:
                with open(json_path, encoding="utf-8") as f:
                    data = json.load(f)
            except (OSError, ValueError):
                continue
            for b in data.get("Benchmarks", []):
                stats = b.get("Statistics") or {}
                mean = stats.get("Mean")
                if mean is None:
                    continue
                records.append({
                    "os": os_name,
                    "version": version,
                    "type": b.get("Type", "?"),
                    "method": b.get("Method", "?"),
                    "params": b.get("Parameters", "") or "",
                    "mean_ns": float(mean),
                })
    return records


def fmt_us(mean_ns):
    return f"{mean_ns / 1000.0:,.2f}"


def build_report(records, baseline):
    if not records:
        return "No benchmark results were found."

    # Preserve first-seen order for versions, then move baseline first if present.
    versions = list(OrderedDict((r["version"], None) for r in records).keys())
    if baseline in versions:
        versions.remove(baseline)
        versions.insert(0, baseline)

    oses = list(OrderedDict((r["os"], None) for r in records).keys())

    lines = []
    lines.append("# Benchmark comparison")
    lines.append("")
    lines.append("Mean time per operation in microseconds (lower is better). "
                 "Ratio is relative to the baseline column"
                 + (f" (`{baseline}`)." if baseline else "."))
    lines.append("")

    for os_name in oses:
        lines.append(f"## {os_name}")
        lines.append("")

        # rows keyed by (type, method, params); columns keyed by version
        table = defaultdict(dict)
        for r in records:
            if r["os"] != os_name:
                continue
            key = (r["type"], r["method"], r["params"])
            table[key][r["version"]] = r["mean_ns"]

        os_versions = [v for v in versions if any(v in cols for cols in table.values())]

        header = ["Benchmark", "Parameters"] + os_versions
        if baseline in os_versions and len(os_versions) > 1:
            header += ["vs baseline"]
        lines.append("| " + " | ".join(header) + " |")
        lines.append("|" + "|".join(["---"] * len(header)) + "|")

        for key in sorted(table.keys()):
            type_name, method, params = key
            short_type = type_name.split(".")[-1]
            cols = table[key]
            row = [f"{short_type}.{method}", params or "—"]
            for v in os_versions:
                row.append(fmt_us(cols[v]) if v in cols else "—")
            if baseline in os_versions and len(os_versions) > 1 and baseline in cols:
                base = cols[baseline]
                ratios = []
                for v in os_versions:
                    if v == baseline or v not in cols:
                        continue
                    ratios.append(f"{v}: {cols[v] / base:.2f}x")
                row.append("; ".join(ratios) if ratios else "—")
            elif baseline in os_versions and len(os_versions) > 1:
                row.append("—")
            lines.append("| " + " | ".join(row) + " |")

        lines.append("")

    return "\n".join(lines)


def main(argv):
    parser = argparse.ArgumentParser(description=__doc__,
                                     formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("root", help="Directory containing the downloaded result artifacts")
    parser.add_argument("--baseline", default="", help="Version to treat as the baseline column")
    parser.add_argument("--output", default="", help="Write the Markdown report to this file")
    args = parser.parse_args(argv)

    if not os.path.isdir(args.root):
        print(f"error: not a directory: {args.root}", file=sys.stderr)
        return 1

    records = load_records(args.root)
    report = build_report(records, args.baseline)

    if args.output:
        with open(args.output, "w", encoding="utf-8") as f:
            f.write(report + "\n")
    print(report)
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
