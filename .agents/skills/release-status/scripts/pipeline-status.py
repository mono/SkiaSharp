#!/usr/bin/env python3
"""Trace the SkiaSharp release pipeline chain for a given branch or SHA.

Usage:
    pipeline-status.py <branch-or-sha>

Examples:
    pipeline-status.py release/3.119.4
    pipeline-status.py f568ac94dd7
"""

import json
import re
import subprocess
import sys

ORG = "https://devdiv.visualstudio.com"
PROJECT = "DevDiv"

PIPELINES = [
    {"name": "SkiaSharp-Native", "id": 26493, "desc": "native binaries"},
    {"name": "SkiaSharp", "id": 10789, "desc": "managed build, signing & publishing"},
    {"name": "SkiaSharp-Tests", "id": 15756, "desc": "device & unit tests"},
]

ICONS = {
    "succeeded": "✅",
    "partiallySucceeded": "⚠️",
    "failed": "❌",
    "canceled": "🚫",
    "inProgress": "🔄",
    "notStarted": "⏳",
}

# Compliance/security tasks that commonly fail but don't block builds.
# These are filtered from job failure counts and reported separately.
COMPLIANCE_TASKS = {
    "binskim",
    "component governance",
    "container security sbom",
    "1es pt pre-job",
    "post-job: component governance",
    "componentgovernancecomponentdetection",
    "credscan",
    "guardian",
}


def az(args: list[str], timeout: int = 30) -> str:
    result = subprocess.run(
        ["az"] + args, capture_output=True, text=True, timeout=timeout
    )
    return result.stdout.strip()


def get_runs(pipeline_id: int, branch: str) -> list[dict]:
    out = az([
        "pipelines", "runs", "list",
        "--pipeline-ids", str(pipeline_id),
        "--branch", branch,
        "--org", ORG, "--project", PROJECT,
        "--query", "[].{id:id, status:status, result:result, buildNumber:buildNumber}",
        "--top", "5", "-o", "json",
    ])
    return json.loads(out) if out else []


def get_trigger_info(build_id: int) -> dict:
    out = az([
        "pipelines", "runs", "show",
        "--id", str(build_id),
        "--org", ORG, "--project", PROJECT,
        "--query", "triggerInfo", "-o", "json",
    ])
    return json.loads(out) if out else {}


def get_timeline(build_id: int) -> list[dict]:
    """Fetch the build timeline (stages, jobs, tasks) from the ADO REST API."""
    out = az([
        "devops", "invoke",
        "--area", "build",
        "--resource", "timeline",
        "--route-parameters", f"project={PROJECT}", f"buildId={build_id}",
        "--org", ORG,
        "--api-version", "7.0",
        "-o", "json",
    ], timeout=60)
    if not out:
        return []
    data = json.loads(out)
    return data.get("records", [])


def is_compliance_task(name: str) -> bool:
    """Check if a task/job name matches a known compliance/security task."""
    lower = name.lower()
    for pattern in COMPLIANCE_TASKS:
        if pattern in lower:
            return True
    return False


def format_job_summary(records: list[dict], cont: str) -> None:
    """Print a summary of job-level status from timeline records."""
    # Filter to only Job-type records (not Stage or Task)
    jobs = [r for r in records if r.get("type") == "Job"]

    if not jobs:
        return

    completed = []
    running = []
    pending = []

    for job in jobs:
        name = job.get("name", "Unknown")
        state = job.get("state", "")
        result = job.get("result", "")

        if state == "completed":
            completed.append({"name": name, "result": result})
        elif state == "inProgress":
            running.append(name)
        else:
            # pending, notStarted, or any other state
            pending.append(name)

    # Count compliance task failures at the Task level
    tasks = [r for r in records if r.get("type") == "Task"]
    compliance_failures = sum(
        1 for t in tasks
        if t.get("state") == "completed"
        and t.get("result") in ("failed", "succeededWithIssues")
        and is_compliance_task(t.get("name", ""))
    )

    # Build the summary line
    parts = []
    if completed:
        parts.append(f"{len(completed)} ✅ completed")
    if running:
        parts.append(f"{len(running)} 🔄 running")
    if pending:
        parts.append(f"{len(pending)} ⏳ pending")

    print(f"{cont}")
    print(f"{cont} Jobs: {' | '.join(parts)}")

    if running:
        names = ", ".join(running[:8])
        suffix = f", … (+{len(running) - 8} more)" if len(running) > 8 else ""
        print(f"{cont} Running: {names}{suffix}")

    if pending:
        names = ", ".join(pending[:8])
        suffix = f", … (+{len(pending) - 8} more)" if len(pending) > 8 else ""
        print(f"{cont} Pending: {names}{suffix}")

    if compliance_failures > 0:
        print(f"{cont} ⚠️  {compliance_failures} compliance tasks failed (non-blocking)")


def icon_for(run: dict) -> str:
    if run["status"] == "completed":
        return ICONS.get(run.get("result", ""), "❓")
    return ICONS.get(run["status"], "⏳")


def resolve_branch(ref: str) -> str:
    if re.match(r"^[0-9a-f]{7,40}$", ref):
        result = subprocess.run(
            ["git", "branch", "-r", "--contains", ref],
            capture_output=True, text=True,
        )
        for line in result.stdout.splitlines():
            m = re.search(r"origin/(release/\S+)", line)
            if m:
                print(f"Resolved SHA {ref} → branch: {m.group(1)}")
                return m.group(1)
        sys.exit(f"ERROR: No release branch found containing SHA {ref}")
    return ref


def main():
    if len(sys.argv) < 2:
        sys.exit("Usage: pipeline-status.py <branch-or-sha>")

    branch = resolve_branch(sys.argv[1])

    print(f"\n{'═' * 63}")
    print(f" Pipeline Chain Status: {branch}")
    print(f"{'═' * 63}\n")

    all_runs: list[tuple[dict, list[dict]]] = []
    links: list[str] = []

    for i, pipe in enumerate(PIPELINES):
        prefix = "┌─" if i == 0 else "├─" if i < len(PIPELINES) - 1 else "└─"
        cont = "│ " if i < len(PIPELINES) - 1 else "  "

        print(f"{prefix} {pipe['name']} (ID {pipe['id']}) — {pipe['desc']}")

        runs = get_runs(pipe["id"], branch)
        all_runs.append((pipe, runs))

        if not runs:
            print(f"{cont} No runs found — not yet triggered")
        else:
            for r in runs:
                print(f"{cont} {icon_for(r)} id={r['id']:<10}  {r['status']:<12}  "
                      f"{r.get('result') or 'pending':<20}  {r['buildNumber']}")

            # Show job-level details for in-progress builds
            latest_run = runs[0]
            if latest_run["status"] == "inProgress":
                try:
                    records = get_timeline(latest_run["id"])
                    if records:
                        format_job_summary(records, cont)
                except (json.JSONDecodeError, subprocess.TimeoutExpired, OSError):
                    print(f"{cont} (could not fetch job details)")

            # Show trigger info (skip for first pipeline — it has no upstream)
            if i > 0:
                trigger = get_trigger_info(runs[0]["id"])
                if trigger and trigger.get("source"):
                    src = trigger.get("source", "?")
                    pid = trigger.get("pipelineId", "?")
                    print(f"{cont} ↑ triggered by {src} build {pid}")
            links.append(f"  {pipe['name']}: https://devdiv.visualstudio.com/DevDiv/_build/results?buildId={runs[0]['id']}")

        print(f"{cont}" if i < len(PIPELINES) - 1 else "")

    print(f"{'═' * 63}\n")

    # Summary
    latest = [(runs[0] if runs else None) for _, runs in all_runs]
    if all(r and r["status"] == "completed" and r.get("result") in ("succeeded", "partiallySucceeded") for r in latest):
        print("Summary: ✅ All pipelines completed. Packages should be on internal feed.")
    elif latest[0] and not latest[1]:
        print("Summary: ⏳ Waiting for SkiaSharp to be triggered by SkiaSharp-Native.")
    elif not latest[0]:
        print("Summary: ⏳ No native build found yet.")
    else:
        print("Summary: 🔄 Pipeline chain in progress or has failures.")

    if links:
        print(f"\nADO Links:")
        print("\n".join(links))


if __name__ == "__main__":
    main()
