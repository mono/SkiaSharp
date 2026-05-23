#!/usr/bin/env python3
"""
Query all Component Governance (CG) alerts for the SkiaSharp-Native pipeline.

Automatically queries the latest builds from main AND active release branches,
then deduplicates alerts across all builds to give a complete picture.

Prerequisites:
  - az CLI installed and authenticated
  - az devops extension installed
  - Default org/project configured: az devops configure --defaults organization=https://devdiv.visualstudio.com project=DevDiv

Usage:
  python3 query-cg-alerts.py [--build-id BUILD_ID] [--branch BRANCH] [--json] [--quiet]

  # Query all branches (default)
  python3 query-cg-alerts.py

  # Query only a specific branch
  python3 query-cg-alerts.py --branch main

  # Query a specific build
  python3 query-cg-alerts.py --build-id 14176611

Output:
  Prints a summary of all unique CG alerts grouped by severity, component, and branch.
  With --json, outputs structured JSON suitable for the security audit report.
"""

import argparse
import json
import re
import subprocess
import sys
import urllib.request
from collections import defaultdict

ORG = "https://devdiv.visualstudio.com"
PROJECT = "DevDiv"
PIPELINE_ID = 26493  # SkiaSharp-Native

# Branches to query by default (main + any active release branches)
DEFAULT_BRANCHES = ["main", "release/*"]


def get_token() -> str:
    """Get Azure DevOps access token via az CLI."""
    result = subprocess.run(
        ["az", "account", "get-access-token",
         "--resource", "499b84ac-1321-427f-aa17-267ca6975798",
         "--query", "accessToken", "-o", "tsv"],
        capture_output=True, text=True, timeout=30
    )
    if result.returncode != 0:
        print("ERROR: Failed to get access token. Run 'az login' first.", file=sys.stderr)
        sys.exit(1)
    return result.stdout.strip()


def api_get(token: str, url: str):
    """Make an authenticated GET request to AzDO REST API."""
    req = urllib.request.Request(url)
    req.add_header("Authorization", f"Bearer {token}")
    req.add_header("Accept", "application/json")
    try:
        resp = urllib.request.urlopen(req, timeout=30)
        return json.loads(resp.read().decode("utf-8"))
    except Exception as e:
        print(f"ERROR: API request failed: {url}\n  {e}", file=sys.stderr)
        return None


def run_az(args: list[str], parse_json=True):
    """Run an az CLI command and return parsed output."""
    result = subprocess.run(
        ["az"] + args,
        capture_output=True, text=True, timeout=60
    )
    if result.returncode != 0:
        print(f"ERROR: az {' '.join(args[:5])}... failed: {result.stderr[:200]}", file=sys.stderr)
        return None
    if parse_json:
        return json.loads(result.stdout)
    return result.stdout.strip()


def get_builds_for_branches(token: str, branches: list[str], top_per_branch: int = 1) -> list[dict]:
    """Get the latest completed builds for the given branches.
    
    Supports wildcards: "release/*" matches any release/X.Y.Z or release/X.Y.x branch.
    """
    # Get recent builds (enough to cover all active branches)
    url = (f"{ORG}/{PROJECT}/_apis/build/builds"
           f"?definitions={PIPELINE_ID}&statusFilter=completed&$top=20&api-version=7.1")
    data = api_get(token, url)
    if not data:
        return []
    
    all_builds = data.get("value", [])
    
    # Match branches
    matched = {}
    for build in all_builds:
        branch = build.get("sourceBranch", "").replace("refs/heads/", "")
        for pattern in branches:
            if pattern.endswith("*"):
                prefix = pattern[:-1]
                if branch.startswith(prefix) and branch not in matched:
                    matched[branch] = build
            elif branch == pattern and branch not in matched:
                matched[branch] = build
    
    return list(matched.values())


def get_cg_log_ids(token: str, build_id: int) -> dict[str, tuple[str, int]]:
    """Get representative CG log IDs from the build timeline."""
    url = (f"{ORG}/{PROJECT}/_apis/build/builds/{build_id}/timeline?api-version=7.1")
    timeline = api_get(token, url)
    if not timeline:
        return {}

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


def parse_cg_log(token: str, build_id: int, log_id: int) -> list[dict]:
    """Parse CVEs from a CG log using REST API directly."""
    url = f"{ORG}/{PROJECT}/_apis/build/builds/{build_id}/logs/{log_id}?api-version=7.1"
    req = urllib.request.Request(url)
    req.add_header("Authorization", f"Bearer {token}")
    try:
        resp = urllib.request.urlopen(req, timeout=30)
        content = resp.read().decode("utf-8")
    except Exception as e:
        print(f"  WARNING: Failed to fetch log {log_id}: {e}", file=sys.stderr)
        return []

    alerts = []
    for line in content.split("\n"):
        # Format: |CVE-XXXX-XXXXX |component |severity |date|
        parts = [p.strip() for p in line.split("|")]
        data_parts = [p for p in parts if p and not re.match(r"^\d{4}-\d{2}-\d{2}T", p)]
        if len(data_parts) >= 3:
            cve_match = re.search(r"(CVE-\d{4}-\d+|GHSA-[\w-]+)", data_parts[0])
            if cve_match:
                alerts.append({
                    "id": cve_match.group(1),
                    "component": data_parts[1],
                    "severity": data_parts[2]
                })

    return alerts


def main():
    parser = argparse.ArgumentParser(description="Query CG alerts for SkiaSharp-Native")
    parser.add_argument("--build-id", type=int, help="Specific build ID (skips branch discovery)")
    parser.add_argument("--branch", type=str, help="Query only this branch (e.g., 'main' or 'release/3.119.x')")
    parser.add_argument("--json", action="store_true", help="Output as JSON")
    parser.add_argument("--quiet", action="store_true", help="Suppress progress messages")
    args = parser.parse_args()

    token = get_token()

    # Determine which builds to query
    builds_to_query = []  # list of (build_id, branch, build_number)

    if args.build_id:
        # Query specific build
        url = f"{ORG}/{PROJECT}/_apis/build/builds/{args.build_id}?api-version=7.1"
        build = api_get(token, url)
        if build:
            branch = build.get("sourceBranch", "").replace("refs/heads/", "")
            builds_to_query.append((args.build_id, branch, build.get("buildNumber", "unknown")))
        else:
            print(f"ERROR: Build {args.build_id} not found", file=sys.stderr)
            sys.exit(1)
    else:
        # Discover builds from branches
        branches = [args.branch] if args.branch else DEFAULT_BRANCHES
        discovered = get_builds_for_branches(token, branches)
        if not discovered:
            print("ERROR: No completed builds found for specified branches", file=sys.stderr)
            sys.exit(1)
        for b in discovered:
            branch = b.get("sourceBranch", "").replace("refs/heads/", "")
            builds_to_query.append((b["id"], branch, b.get("buildNumber", "unknown")))

    if not args.quiet:
        print(f"Querying {len(builds_to_query)} build(s):", file=sys.stderr)
        for bid, branch, bnum in builds_to_query:
            print(f"  {branch}: build {bid} ({bnum})", file=sys.stderr)

    # Collect alerts from all builds
    all_alerts = {}  # id -> {component, severity, branches, sources}

    for build_id, branch, build_num in builds_to_query:
        if not args.quiet:
            print(f"\n--- {branch} (build {build_id}) ---", file=sys.stderr)

        reps = get_cg_log_ids(token, build_id)
        if not reps:
            if not args.quiet:
                print(f"  WARNING: No CG logs found in build {build_id}", file=sys.stderr)
            continue

        if not args.quiet:
            print(f"  Sampling {len(reps)} container types: {', '.join(sorted(reps.keys()))}", file=sys.stderr)

        for category, (job_name, log_id) in sorted(reps.items()):
            if not args.quiet:
                print(f"    Parsing {category} (log {log_id})...", file=sys.stderr)
            alerts = parse_cg_log(token, build_id, log_id)
            for alert in alerts:
                key = alert["id"]
                if key not in all_alerts:
                    all_alerts[key] = {
                        "id": alert["id"],
                        "component": alert["component"],
                        "severity": alert["severity"],
                        "sources": [category],
                        "branches": [branch]
                    }
                else:
                    if category not in all_alerts[key]["sources"]:
                        all_alerts[key]["sources"].append(category)
                    if branch not in all_alerts[key]["branches"]:
                        all_alerts[key]["branches"].append(branch)

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
            "builds": [{"id": bid, "branch": br, "number": num} for bid, br, num in builds_to_query],
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

        branches_str = ", ".join(b for _, b, _ in builds_to_query)
        print(f"\n{'='*70}")
        print(f"CG ALERTS SUMMARY — SkiaSharp-Native (pipeline {PIPELINE_ID})")
        print(f"Branches: {branches_str}")
        print(f"Total unique alerts: {len(all_alerts)}")
        print(f"{'='*70}")

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
                    by_comp[a["component"]].append(a)
                for comp in sorted(by_comp.keys()):
                    comp_alerts = by_comp[comp]
                    cves = sorted(a["id"] for a in comp_alerts)
                    # Show branch info if not all branches
                    all_branches = set()
                    for a in comp_alerts:
                        all_branches.update(a["branches"])
                    branch_note = ""
                    if len(builds_to_query) > 1:
                        branch_note = f" [{', '.join(sorted(all_branches))}]"
                    if len(cves) <= 3:
                        print(f"    {comp}: {', '.join(cves)}{branch_note}")
                    else:
                        print(f"    {comp} ({len(cves)} CVEs): {', '.join(cves[:3])}...{branch_note}")

        # Summary by category
        print(f"\n{'='*70}")
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

        # Branch coverage
        if len(builds_to_query) > 1:
            print(f"\nBRANCH COVERAGE:")
            for _, branch, _ in builds_to_query:
                branch_alerts = [a for a in all_alerts.values() if branch in a["branches"]]
                print(f"  {branch}: {len(branch_alerts)} alerts")
            
            # Show branch-exclusive alerts
            for _, branch, _ in builds_to_query:
                exclusive = [a for a in all_alerts.values() 
                           if a["branches"] == [branch]]
                if exclusive:
                    print(f"\n  Only in {branch} ({len(exclusive)}):")
                    for a in sorted(exclusive, key=lambda x: x["id"])[:10]:
                        print(f"    {a['id']} [{a['severity']}] {a['component']}")
                    if len(exclusive) > 10:
                        print(f"    ... and {len(exclusive) - 10} more")


if __name__ == "__main__":
    main()
