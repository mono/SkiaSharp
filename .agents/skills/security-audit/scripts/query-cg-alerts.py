#!/usr/bin/env python3
"""
Query all Component Governance (CG) alerts for the SkiaSharp-Native pipeline.

No build ID required - automatically uses the latest completed build and extracts
alerts from representative CG logs (one per container type) to get full coverage.

Prerequisites:
  - az CLI installed and authenticated
  - az devops extension installed
  - Default org/project configured: az devops configure --defaults organization=https://devdiv.visualstudio.com project=DevDiv

Usage:
  python3 query-cg-alerts.py [--build-id BUILD_ID] [--json] [--quiet]

Output:
  Prints a summary of all unique CG alerts grouped by severity and component.
  With --json, outputs structured JSON suitable for the security audit report.
"""

import argparse
import json
import re
import subprocess
import sys
from collections import defaultdict

ORG = "https://devdiv.visualstudio.com"
PROJECT = "DevDiv"
PIPELINE_ID = 26493  # SkiaSharp-Native


def run_az(args: list[str], parse_json=True):
    """Run an az CLI command and return parsed output."""
    result = subprocess.run(
        ["az"] + args,
        capture_output=True, text=True, timeout=60
    )
    if result.returncode != 0:
        print(f"ERROR: az {' '.join(args[:5])}... failed: {result.stderr[:200]}", file=sys.stderr)
        sys.exit(1)
    if parse_json:
        return json.loads(result.stdout)
    return result.stdout.strip()


def get_latest_build_id() -> tuple[int, str]:
    """Get the latest completed build ID and number."""
    builds = run_az([
        "pipelines", "runs", "list",
        "--pipeline-id", str(PIPELINE_ID),
        "--org", ORG, "--project", PROJECT,
        "--top", "1", "--status", "completed",
        "-o", "json"
    ])
    if not builds:
        print("ERROR: No completed builds found", file=sys.stderr)
        sys.exit(1)
    return builds[0]["id"], builds[0].get("buildNumber", "unknown")


def get_cg_log_ids(build_id: int) -> dict[str, tuple[str, int]]:
    """Get representative CG log IDs from the build timeline."""
    timeline = run_az([
        "devops", "invoke",
        "--area", "build", "--resource", "timeline",
        "--route-parameters", f"project={PROJECT}", f"buildId={build_id}",
        "--org", ORG, "-o", "json"
    ])

    records = timeline.get("records", [])
    jobs = {r["id"]: r.get("name", "") for r in records}

    # Find all CG tasks with their parent job names
    cg_logs = {}
    for r in records:
        if "Component Governance" in r.get("name", "") and r.get("log"):
            log_id = r["log"]["id"]
            parent = r.get("parentId", "")
            job_name = jobs.get(parent, "unknown")
            result = r.get("result", "")
            if result in ("succeeded", "succeededWithIssues"):
                cg_logs[job_name] = log_id

    # Pick representative jobs (one per container type for full coverage)
    representatives = {}
    for job, log_id in cg_logs.items():
        jl = job.lower()
        if "alpine" in jl and "arm64" in jl and "nodeps" not in jl:
            representatives["alpine"] = (job, log_id)
        elif "loongarch64" in jl and "alpine" not in jl:
            representatives.setdefault("debian13", (job, log_id))
        elif "arm)" in jl and "alpine" not in jl and "bionic" not in jl:
            representatives.setdefault("debian11", (job, log_id))
        elif "wasm" in jl and "3.1.56" in job and "simd" not in jl and "thread" not in jl:
            representatives.setdefault("wasm", (job, log_id))
        elif "bionic" in jl and "arm64" in jl:
            representatives.setdefault("bionic", (job, log_id))

    return representatives


def parse_cg_log(build_id: int, log_id: int) -> list[dict]:
    """Parse CVEs from a CG log."""
    log_data = run_az([
        "devops", "invoke",
        "--area", "build", "--resource", "logs",
        "--route-parameters", f"project={PROJECT}", f"buildId={build_id}", f"logId={log_id}",
        "--org", ORG, "-o", "json"
    ])

    lines = log_data if isinstance(log_data, list) else log_data.get("value", [])
    alerts = []

    for line in lines:
        s = str(line)
        m = re.search(r"\|(CVE-[\d-]+|MVS-[\w-]+|GHSA-[\w-]+)\s*\|([^|]+)\|(\w+)", s)
        if m:
            alerts.append({
                "id": m.group(1).strip(),
                "component": m.group(2).strip(),
                "severity": m.group(3).strip()
            })

    return alerts


def main():
    parser = argparse.ArgumentParser(description="Query CG alerts for SkiaSharp-Native")
    parser.add_argument("--build-id", type=int, help="Specific build ID (default: latest)")
    parser.add_argument("--json", action="store_true", help="Output as JSON")
    parser.add_argument("--quiet", action="store_true", help="Suppress progress messages")
    args = parser.parse_args()

    # Get build ID
    if args.build_id:
        build_id = args.build_id
        build_num = "specified"
    else:
        build_id, build_num = get_latest_build_id()

    if not args.quiet:
        print(f"Build: {build_id} ({build_num})", file=sys.stderr)

    # Get representative log IDs
    reps = get_cg_log_ids(build_id)
    if not args.quiet:
        print(f"Sampling {len(reps)} container types: {', '.join(sorted(reps.keys()))}", file=sys.stderr)

    # Parse all logs and deduplicate
    all_alerts = {}  # id -> {component, severity, sources}
    for category, (job_name, log_id) in sorted(reps.items()):
        if not args.quiet:
            print(f"  Parsing {category} (log {log_id}: {job_name})...", file=sys.stderr)
        alerts = parse_cg_log(build_id, log_id)
        for alert in alerts:
            key = alert["id"]
            if key not in all_alerts:
                all_alerts[key] = {
                    "id": alert["id"],
                    "component": alert["component"],
                    "severity": alert["severity"],
                    "sources": [category]
                }
            else:
                if category not in all_alerts[key]["sources"]:
                    all_alerts[key]["sources"].append(category)

    # Categorize
    for alert in all_alerts.values():
        comp = alert["component"]
        if comp.startswith("Debian:12:") and any(x in comp for x in ["busybox", "file", "binutils", "zlib", "freetype", "gmp"]):
            alert["category"] = "Alpine 3.17 sysroot"
        elif comp.startswith("Debian:12:"):
            alert["category"] = "Debian 12 base image"
        elif comp.startswith("Debian:13:"):
            alert["category"] = "Debian 13 base image"
        elif "WindowsAppSDK" in comp:
            alert["category"] = "NuGet dependency"
        elif any(c in comp for c in ["hashbrown", "zerovec", "time 0."]):
            alert["category"] = "Rust crate (.NET SDK)"
        else:
            alert["category"] = "npm build tooling"

    # Output
    if args.json:
        output = {
            "buildId": build_id,
            "buildNumber": build_num,
            "pipelineId": PIPELINE_ID,
            "totalAlerts": len(all_alerts),
            "alerts": sorted(all_alerts.values(), key=lambda a: (
                {"Critical": 0, "High": 1, "Medium": 2, "Low": 3}.get(a["severity"], 9),
                a["category"],
                a["id"]
            ))
        }
        print(json.dumps(output, indent=2))
    else:
        # Group by severity
        by_sev = defaultdict(list)
        for alert in all_alerts.values():
            by_sev[alert["severity"]].append(alert)

        print(f"\n{'='*60}")
        print(f"CG ALERTS SUMMARY — Build {build_id} ({build_num})")
        print(f"Total unique alerts: {len(all_alerts)}")
        print(f"{'='*60}")

        for sev in ["Critical", "High", "Medium", "Low"]:
            if sev not in by_sev:
                continue
            alerts = by_sev[sev]
            print(f"\n### {sev} ({len(alerts)} alerts)")

            # Group by category
            by_cat = defaultdict(list)
            for a in alerts:
                by_cat[a["category"]].append(a)

            for cat in sorted(by_cat.keys()):
                cat_alerts = by_cat[cat]
                print(f"\n  [{cat}]")
                # Group by component
                by_comp = defaultdict(list)
                for a in cat_alerts:
                    by_comp[a["component"]].append(a["id"])
                for comp in sorted(by_comp.keys()):
                    cves = sorted(by_comp[comp])
                    if len(cves) <= 3:
                        print(f"    {comp}: {', '.join(cves)}")
                    else:
                        print(f"    {comp} ({len(cves)} CVEs): {', '.join(cves[:3])}...")

        # Summary by category
        print(f"\n{'='*60}")
        print("BY CATEGORY:")
        by_cat = defaultdict(list)
        for a in all_alerts.values():
            by_cat[a["category"]].append(a)
        for cat in sorted(by_cat.keys()):
            alerts = by_cat[cat]
            sevs = defaultdict(int)
            for a in alerts:
                sevs[a["severity"]] += 1
            sev_str = ", ".join(f"{s}: {c}" for s, c in sorted(sevs.items()))
            print(f"  {cat}: {len(alerts)} alerts ({sev_str})")


if __name__ == "__main__":
    main()
