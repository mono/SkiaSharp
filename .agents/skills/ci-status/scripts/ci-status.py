#!/usr/bin/env python3
"""Collect CI build status for SkiaSharp main and recent release branches.

Usage:
    ci-status.py [--branches N] [--builds N]

Options:
    --branches N    Number of most recent release branches to include (default: 3)
    --builds N      Number of recent builds per pipeline per branch (default: 5)

Queries Azure DevOps (devdiv/DevDiv) for:
  Public CI:  SkiaSharp Main Build (25328)
  Internal:   SkiaSharp-Native (26493) → SkiaSharp (10789) → SkiaSharp-Tests (15756)
"""

import argparse
import json
import subprocess
import sys
import urllib.request
from datetime import datetime, timezone

ORG_DEVDIV = "https://devdiv.visualstudio.com"
PROJECT_DEVDIV = "DevDiv"

ORG_XAMARIN = "https://dev.azure.com/xamarin"
PROJECT_XAMARIN = "public"

# Public CI pipeline — runs on every push/PR to main, develop, release/*
# Lives in the xamarin/public org
PUBLIC_PIPELINES = [
    {"name": "SkiaSharp (Public)", "id": 4, "org": ORG_XAMARIN, "project": PROJECT_XAMARIN},
]

# Internal release pipeline chain — runs on release/* branches
# Lives in devdiv/DevDiv org
INTERNAL_PIPELINES = [
    {"name": "SkiaSharp-Native", "id": 26493, "org": ORG_DEVDIV, "project": PROJECT_DEVDIV},
    {"name": "SkiaSharp", "id": 10789, "org": ORG_DEVDIV, "project": PROJECT_DEVDIV},
    {"name": "SkiaSharp-Tests", "id": 15756, "org": ORG_DEVDIV, "project": PROJECT_DEVDIV},
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
    """Run az CLI and return stdout."""
    result = subprocess.run(
        ["az"] + args, capture_output=True, text=True, timeout=30
    )
    if result.returncode != 0:
        return ""
    return result.stdout.strip()


def get_runs(pipeline_id: int, branch: str, org: str, project: str, top: int = 5) -> list[dict]:
    """Get recent runs for a pipeline on a branch."""
    out = az([
        "pipelines", "runs", "list",
        "--pipeline-ids", str(pipeline_id),
        "--branch", branch,
        "--org", org, "--project", project,
        "--query",
        "[].{id:id, status:status, result:result, buildNumber:buildNumber, "
        "startTime:startTime, finishTime:finishTime}",
        "--top", str(top), "-o", "json",
    ])
    return json.loads(out) if out else []


def get_build_issues(build_id: int, org: str, project: str) -> list[dict]:
    """Fetch timeline issues (errors/warnings) for a build.

    Returns a list of dicts with keys: type, message, task_name.
    Collects all issues from failed or warning records.
    """
    # URL format: {org}/{project}/_apis/build/builds/{buildId}/timeline?api-version=7.1
    org_base = org.rstrip("/")
    url = f"{org_base}/{project}/_apis/build/builds/{build_id}/timeline?api-version=7.1"

    try:
        # Try using az CLI token for auth
        token = subprocess.run(
            ["az", "account", "get-access-token", "--resource", "499b84ac-1321-427f-aa17-267ca6975798",
             "--query", "accessToken", "-o", "tsv"],
            capture_output=True, text=True, timeout=10
        ).stdout.strip()

        req = urllib.request.Request(url)
        if token:
            req.add_header("Authorization", f"Bearer {token}")

        with urllib.request.urlopen(req, timeout=15) as resp:
            data = json.loads(resp.read())
    except Exception:
        return []

    issues = []
    records = data.get("records", [])
    for record in records:
        record_issues = record.get("issues", [])
        if not record_issues:
            continue

        task_name = record.get("name", "unknown")
        record_result = record.get("result", "")

        # Only collect from failed or warning records
        if record_result not in ("failed", "succeededWithIssues"):
            continue

        for issue in record_issues:
            issue_type = issue.get("type", "").lower()  # "error" or "warning"
            message = issue.get("message", "").strip()
            if not message:
                continue
            issues.append({
                "type": issue_type,
                "message": message,
                "task_name": task_name,
            })

    return issues


def get_release_branches(top: int = 3) -> list[str]:
    """Get the most recent release branches by commit date."""
    result = subprocess.run(
        ["git", "branch", "-r", "--sort=-committerdate"],
        capture_output=True, text=True,
    )
    branches = []
    for line in result.stdout.splitlines():
        line = line.strip()
        # Match origin/release/X.Y.Z* branches (skip release/X.Y.x maintenance branches)
        if "origin/release/" in line and not line.endswith(".x"):
            branch = line.replace("origin/", "")
            branches.append(branch)
            if len(branches) >= top:
                break
    return branches


def icon_for(run: dict) -> str:
    if run["status"] == "completed":
        return ICONS.get(run.get("result", ""), "❓")
    return ICONS.get(run["status"], "⏳")


def format_time(time_str: str | None) -> str:
    """Format ISO time string to a short display form."""
    if not time_str:
        return "—"
    try:
        dt = datetime.fromisoformat(time_str.replace("Z", "+00:00"))
        return dt.strftime("%Y-%m-%d %H:%M")
    except (ValueError, TypeError):
        return time_str[:16] if time_str else "—"


def print_pipeline_group(pipelines: list[dict], branch: str, top_builds: int, group_label: str, show_issues: bool = True):
    """Print status for a group of pipelines on a branch."""
    print(f"│  ┌ {group_label}")

    for i, pipe in enumerate(pipelines):
        is_last = i == len(pipelines) - 1
        prefix = "│  │ └─" if is_last else "│  │ ├─"
        cont = "│  │    " if not is_last else "│  │    "

        runs = get_runs(pipe["id"], branch, pipe["org"], pipe["project"], top=top_builds)

        if not runs:
            print(f"{prefix} {pipe['name']}: no builds found")
        else:
            print(f"{prefix} {pipe['name']} (last {len(runs)}):")
            for idx, r in enumerate(runs):
                status_icon = icon_for(r)
                result_text = r.get("result") or r["status"]
                started = format_time(r.get("startTime"))
                print(
                    f"{cont}   {status_icon} {r['buildNumber']:<35} "
                    f"{result_text:<22} {started}  [id:{r['id']}]"
                )

                # Show all issues from the most recent run if it's not clean
                if show_issues and idx == 0 and r.get("result") in ("failed", "partiallySucceeded"):
                    issues = get_build_issues(r["id"], pipe["org"], pipe["project"])
                    if issues:
                        errors = [i for i in issues if i["type"] == "error"]
                        warnings = [i for i in issues if i["type"] == "warning"]
                        if errors:
                            print(f"{cont}   ┌── Errors ({len(errors)}):")
                            for e in errors:
                                msg = e["message"][:120]
                                print(f"{cont}   │ ❌ [{e['task_name']}] {msg}")
                            print(f"{cont}   └──")
                        if warnings:
                            print(f"{cont}   ┌── Warnings ({len(warnings)}):")
                            for w in warnings:
                                msg = w["message"][:120]
                                print(f"{cont}   │ ⚠️  [{w['task_name']}] {msg}")
                            print(f"{cont}   └──")

    print(f"│  └")


def print_branch_status(branch: str, top_builds: int, show_issues: bool = True):
    """Print status table for a single branch across all pipelines."""
    print(f"\n┌─ Branch: {branch}")
    print(f"│")

    # Public CI pipeline (runs on all branches)
    print_pipeline_group(PUBLIC_PIPELINES, branch, top_builds, "Public CI", show_issues)

    # Internal release chain (most relevant for release/* branches)
    print_pipeline_group(INTERNAL_PIPELINES, branch, top_builds, "Internal Release Chain", show_issues)

    print(f"│")


def main():
    parser = argparse.ArgumentParser(
        description="CI status for SkiaSharp main and release branches"
    )
    parser.add_argument(
        "--branches", type=int, default=3,
        help="Number of recent release branches to check (default: 3)"
    )
    parser.add_argument(
        "--builds", type=int, default=5,
        help="Number of recent builds per pipeline (default: 5)"
    )
    parser.add_argument(
        "--no-issues", action="store_true",
        help="Skip fetching errors/warnings from failed builds (faster)"
    )
    args = parser.parse_args()

    # Collect branches to check
    branches = ["main"]
    release_branches = get_release_branches(top=args.branches)
    branches.extend(release_branches)

    print(f"{'═' * 70}")
    print(f" SkiaSharp CI Status — {datetime.now(timezone.utc).strftime('%Y-%m-%d %H:%M UTC')}")
    print(f"{'═' * 70}")
    print(f" Public CI:  {' | '.join(p['name'] for p in PUBLIC_PIPELINES)}")
    print(f" Internal:   {' → '.join(p['name'] for p in INTERNAL_PIPELINES)}")
    print(f" Branches:   main + {len(release_branches)} recent release branches")
    print(f" Builds:     last {args.builds} per pipeline")
    print(f"{'═' * 70}")

    for branch in branches:
        print_branch_status(branch, args.builds, show_issues=not args.no_issues)

    print(f"{'═' * 70}")

    # Summary: quick health check
    print("\n📊 Health Summary:")
    all_pipelines = PUBLIC_PIPELINES + INTERNAL_PIPELINES
    for branch in branches:
        statuses = []
        for pipe in all_pipelines:
            runs = get_runs(pipe["id"], branch, pipe["org"], pipe["project"], top=1)
            if runs:
                statuses.append((pipe["name"], icon_for(runs[0]), runs[0].get("result") or runs[0]["status"]))
            else:
                statuses.append((pipe["name"], "⏳", "no runs"))
        summary_line = " | ".join(f"{s[1]} {s[0]}" for s in statuses)
        print(f"  {branch:<40} {summary_line}")

    print()


if __name__ == "__main__":
    main()
