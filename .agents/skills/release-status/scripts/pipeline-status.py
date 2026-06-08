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


def az(args: list[str]) -> str:
    result = subprocess.run(
        ["az"] + args, capture_output=True, text=True, timeout=30
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
