#!/usr/bin/env python3
"""Collect CI build status for SkiaSharp main and recent release branches.

Usage:
    ci-status.py [--branches N] [--builds N] [--output FILE] [--json FILE]

Options:
    --branches N    Number of most recent release branches to include (default: 3)
    --builds N      Number of recent builds per pipeline per branch (default: 5)
    --output FILE   Write a formatted markdown report to FILE
    --json FILE     Write raw structured JSON data to FILE (for AI analysis)
    --no-issues     Skip fetching errors/warnings (faster)

Queries Azure DevOps for:
  Public CI:  SkiaSharp (Public) — xamarin/public, def 4
  Internal:   SkiaSharp-Native (26493) → SkiaSharp (10789) → SkiaSharp-Tests (15756)
"""

import argparse
import json
import os
import subprocess
import sys
import urllib.request
from datetime import datetime, timezone
from collections import defaultdict

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

# GitHub Actions workflows to track
# Each entry: repo, workflow file name, display name, relevant branches
GITHUB_WORKFLOWS = [
    {"repo": "mono/SkiaSharp", "workflow": "build-site.yml", "name": "Docs - Deploy"},
    {"repo": "mono/SkiaSharp", "workflow": "build-site-go-live.yml", "name": "Docs - Go Live!"},
    {"repo": "mono/SkiaSharp", "workflow": "samples.yml", "name": "Publish Samples"},
    {"repo": "mono/SkiaSharp", "workflow": "api-diff.yml", "name": "API Diff"},
    {"repo": "mono/SkiaSharp", "workflow": "auto-docs-submodule-sync.yml", "name": "Auto Docs Submodule Sync"},
    {"repo": "mono/SkiaSharp", "workflow": "update-release-notes.lock.yml", "name": "Update Release Notes"},
    {"repo": "mono/SkiaSharp", "workflow": "auto-skia-sync.lock.yml", "name": "Skia Upstream Sync"},
    {"repo": "mono/SkiaSharp", "workflow": "nightly-fix-finder.lock.yml", "name": "Nightly Fix Finder"},
    {"repo": "mono/SkiaSharp-API-docs", "workflow": "auto-api-docs-writer.lock.yml", "name": "Auto API Docs Writer"},
    {"repo": "mono/SkiaSharp-API-docs", "workflow": "automerge-docs.yml", "name": "Automerge Docs"},
    {"repo": "mono/SkiaSharp-API-docs", "workflow": "go-live.yml", "name": "Go Live"},
]


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
        "startTime:startTime, finishTime:finishTime, "
        "sourceVersion:sourceVersion, reason:reason, "
        "sourceBranch:sourceBranch}",
        "--top", str(top), "-o", "json",
    ])
    return json.loads(out) if out else []


def get_build_changes(build_id: int, org: str, project: str) -> list[dict]:
    """Fetch associated changes (commits) for a build via REST API."""
    org_base = org.rstrip("/")
    url = f"{org_base}/{project}/_apis/build/builds/{build_id}/changes?api-version=7.1&$top=10"

    try:
        token = _get_token()
        req = urllib.request.Request(url)
        if token:
            req.add_header("Authorization", f"Bearer {token}")
        with urllib.request.urlopen(req, timeout=15) as resp:
            data = json.loads(resp.read())
    except Exception:
        return []

    changes = []
    for c in data.get("value", []):
        changes.append({
            "id": c.get("id", "")[:12],
            "author": c.get("author", {}).get("displayName", "unknown"),
            "message": (c.get("message", "") or "").split("\n")[0][:100],
            "timestamp": c.get("timestamp"),
        })
    return changes


_cached_token = None


def _get_token() -> str:
    """Get ADO access token (cached for the script lifetime)."""
    global _cached_token
    if _cached_token is not None:
        return _cached_token
    try:
        _cached_token = subprocess.run(
            ["az", "account", "get-access-token", "--resource", "499b84ac-1321-427f-aa17-267ca6975798",
             "--query", "accessToken", "-o", "tsv"],
            capture_output=True, text=True, timeout=10
        ).stdout.strip()
    except Exception:
        _cached_token = ""
    return _cached_token


def get_build_issues(build_id: int, org: str, project: str) -> list[dict]:
    """Fetch timeline issues (errors/warnings) for a build.

    Returns a list of dicts with keys: type, message, task_name.
    Collects all issues from failed or warning records.
    """
    org_base = org.rstrip("/")
    url = f"{org_base}/{project}/_apis/build/builds/{build_id}/timeline?api-version=7.1"

    try:
        token = _get_token()
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


def gh(args: list[str]) -> str:
    """Run gh CLI and return stdout."""
    result = subprocess.run(
        ["gh"] + args, capture_output=True, text=True, timeout=30
    )
    if result.returncode != 0:
        return ""
    return result.stdout.strip()


def get_github_workflow_runs(repo: str, workflow: str, branch: str | None, top: int = 5) -> list[dict]:
    """Get recent workflow runs from GitHub Actions via gh CLI.

    If branch is None, fetches runs across all branches.
    """
    cmd = [
        "run", "list",
        "--repo", repo,
        "--workflow", workflow,
        "--limit", str(top),
        "--json", "databaseId,headBranch,status,conclusion,displayTitle,createdAt,updatedAt,url,event,headSha,workflowName",
    ]
    if branch:
        cmd.extend(["--branch", branch])

    out = gh(cmd)
    if not out:
        return []
    try:
        return json.loads(out)
    except (json.JSONDecodeError, ValueError):
        return []


def gh_icon_for(run: dict) -> str:
    """Map GitHub Actions status/conclusion to an icon."""
    status = run.get("status", "")
    conclusion = run.get("conclusion", "")

    if status == "in_progress":
        return "🔄"
    if status == "queued" or status == "waiting" or status == "pending":
        return "⏳"

    # Completed runs — check conclusion
    if conclusion == "success":
        return "✅"
    elif conclusion == "failure":
        return "❌"
    elif conclusion == "cancelled":
        return "🚫"
    elif conclusion == "skipped":
        return "⏭️"
    elif conclusion == "startup_failure":
        return "❌"
    else:
        return "❓"


def gh_result_for(run: dict) -> str:
    """Map GitHub Actions status/conclusion to a normalized result string."""
    status = run.get("status", "")
    conclusion = run.get("conclusion", "")

    if status in ("in_progress", "queued", "waiting", "pending"):
        return status

    return conclusion or status


def collect_github_data(branches: list[str], top_builds: int) -> list[dict]:
    """Collect GitHub Actions workflow run data for all tracked workflows.

    Returns a list of workflow data dicts, each containing runs per branch.
    """
    workflows_data = []

    for wf in GITHUB_WORKFLOWS:
        wf_data = {
            "repo": wf["repo"],
            "workflow": wf["workflow"],
            "name": wf["name"],
            "branches": [],
        }

        for branch in branches:
            runs = get_github_workflow_runs(wf["repo"], wf["workflow"], branch, top=top_builds)
            branch_runs = []

            for r in runs:
                # Calculate duration if possible
                duration_min = None
                if r.get("createdAt") and r.get("updatedAt"):
                    try:
                        created = datetime.fromisoformat(r["createdAt"].replace("Z", "+00:00"))
                        updated = datetime.fromisoformat(r["updatedAt"].replace("Z", "+00:00"))
                        duration_min = round((updated - created).total_seconds() / 60, 1)
                    except (ValueError, TypeError):
                        pass

                run_data = {
                    "id": r.get("databaseId"),
                    "displayTitle": r.get("displayTitle", ""),
                    "status": r.get("status", ""),
                    "conclusion": r.get("conclusion", ""),
                    "result": gh_result_for(r),
                    "createdAt": r.get("createdAt"),
                    "updatedAt": r.get("updatedAt"),
                    "durationMinutes": duration_min,
                    "headSha": (r.get("headSha") or "")[:12],
                    "headBranch": r.get("headBranch", ""),
                    "event": r.get("event", ""),
                    "url": r.get("url", ""),
                    "icon": gh_icon_for(r),
                }
                branch_runs.append(run_data)

            # Compute stats
            stats = {"total": 0, "succeeded": 0, "failed": 0, "other": 0}
            for run in branch_runs:
                stats["total"] += 1
                if run["conclusion"] == "success":
                    stats["succeeded"] += 1
                elif run["conclusion"] == "failure":
                    stats["failed"] += 1
                else:
                    stats["other"] += 1

            if stats["total"] > 0:
                stats["pass_rate"] = round(stats["succeeded"] / stats["total"] * 100, 1)
            else:
                stats["pass_rate"] = None

            wf_data["branches"].append({
                "name": branch,
                "runs": branch_runs,
                "stats": stats,
            })

        workflows_data.append(wf_data)

    return workflows_data


def get_release_branches(top: int = 3) -> list[str]:
    """Get the most recent release branches by commit date.

    Includes both versioned release branches (release/X.Y.Z-preview.N)
    and servicing branches (release/X.Y.x) which are the main equivalent
    for their respective major.minor.
    """
    result = subprocess.run(
        ["git", "branch", "-r", "--sort=-committerdate"],
        capture_output=True, text=True,
    )
    branches = []
    for line in result.stdout.splitlines():
        line = line.strip()
        if "origin/release/" in line:
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


def build_url(build_id: int, org: str, project: str) -> str:
    """Generate ADO build URL."""
    org_base = org.rstrip("/")
    return f"{org_base}/{project}/_build/results?buildId={build_id}"


def collect_data(branches: list[str], top_builds: int, show_issues: bool) -> dict:
    """Collect all CI data into a structured dict for rendering and AI analysis."""
    timestamp = datetime.now(timezone.utc).strftime("%Y-%m-%d %H:%M UTC")

    data = {
        "timestamp": timestamp,
        "window": {"builds_per_pipeline": top_builds, "branches_count": len(branches)},
        "pipelines": [
            {"name": p["name"], "id": p["id"], "org": p["org"], "project": p["project"],
             "group": "public" if p in PUBLIC_PIPELINES else "internal"}
            for p in PUBLIC_PIPELINES + INTERNAL_PIPELINES
        ],
        "github_workflows": GITHUB_WORKFLOWS,
        "branches": [],
    }

    for branch in branches:
        branch_data = {"name": branch, "pipeline_groups": []}

        for group_label, pipelines in [("Public CI", PUBLIC_PIPELINES), ("Internal Release Chain", INTERNAL_PIPELINES)]:
            group_data = {"label": group_label, "pipelines": []}

            for pipe in pipelines:
                runs = get_runs(pipe["id"], branch, pipe["org"], pipe["project"], top=top_builds)
                pipe_data = {
                    "name": pipe["name"],
                    "id": pipe["id"],
                    "org": pipe["org"],
                    "project": pipe["project"],
                    "runs": [],
                    "stats": {"total": 0, "succeeded": 0, "failed": 0, "partial": 0, "other": 0},
                }

                for idx, r in enumerate(runs):
                    # Calculate duration
                    duration_min = None
                    if r.get("startTime") and r.get("finishTime"):
                        try:
                            start = datetime.fromisoformat(r["startTime"].replace("Z", "+00:00"))
                            finish = datetime.fromisoformat(r["finishTime"].replace("Z", "+00:00"))
                            duration_min = round((finish - start).total_seconds() / 60, 1)
                        except (ValueError, TypeError):
                            pass

                    run_data = {
                        "id": r["id"],
                        "buildNumber": r["buildNumber"],
                        "status": r["status"],
                        "result": r.get("result"),
                        "startTime": r.get("startTime"),
                        "finishTime": r.get("finishTime"),
                        "durationMinutes": duration_min,
                        "sourceVersion": r.get("sourceVersion", "")[:12] if r.get("sourceVersion") else None,
                        "reason": r.get("reason"),
                        "icon": icon_for(r),
                        "url": build_url(r["id"], pipe["org"], pipe["project"]),
                        "issues": None,
                        "changes": None,
                    }

                    # Collect issues for the most recent failed/partial run
                    if show_issues and idx == 0 and r.get("result") in ("failed", "partiallySucceeded"):
                        run_data["issues"] = get_build_issues(r["id"], pipe["org"], pipe["project"])
                        # Also get associated commits for the failing build
                        run_data["changes"] = get_build_changes(r["id"], pipe["org"], pipe["project"])

                    # Update stats
                    pipe_data["stats"]["total"] += 1
                    result = r.get("result", "")
                    if result == "succeeded":
                        pipe_data["stats"]["succeeded"] += 1
                    elif result == "failed":
                        pipe_data["stats"]["failed"] += 1
                    elif result == "partiallySucceeded":
                        pipe_data["stats"]["partial"] += 1
                    else:
                        pipe_data["stats"]["other"] += 1

                    pipe_data["runs"].append(run_data)

                # Compute pass rate
                total = pipe_data["stats"]["total"]
                if total > 0:
                    pipe_data["stats"]["pass_rate"] = round(
                        pipe_data["stats"]["succeeded"] / total * 100, 1
                    )
                else:
                    pipe_data["stats"]["pass_rate"] = None

                # Detect green→red transition
                pipe_data["regression"] = _detect_regression(pipe_data["runs"])

                group_data["pipelines"].append(pipe_data)
            branch_data["pipeline_groups"].append(group_data)

        data["branches"].append(branch_data)

    # Collect GitHub Actions data
    data["github_actions"] = collect_github_data(branches, top_builds)

    return data


def _detect_regression(runs: list[dict]) -> dict | None:
    """Detect a green→red transition in a list of runs (ordered newest first).

    Returns info about the regression point, or None if no transition found.
    """
    if len(runs) < 2:
        return None

    # Find the first transition from green→red (scanning from oldest to newest)
    reversed_runs = list(reversed(runs))
    for i in range(len(reversed_runs) - 1):
        curr = reversed_runs[i]
        next_run = reversed_runs[i + 1]
        if curr.get("result") == "succeeded" and next_run.get("result") in ("failed", "partiallySucceeded"):
            return {
                "last_green_build_id": curr["id"],
                "last_green_build_number": curr["buildNumber"],
                "first_red_build_id": next_run["id"],
                "first_red_build_number": next_run["buildNumber"],
                "first_red_url": next_run["url"],
            }

    return None


def render_console(data: dict):
    """Render collected data to console (tree format)."""
    print(f"{'═' * 70}")
    print(f" SkiaSharp CI Status — {data['timestamp']}")
    print(f"{'═' * 70}")
    print(f" Public CI:  {' | '.join(p['name'] for p in PUBLIC_PIPELINES)}")
    print(f" Internal:   {' → '.join(p['name'] for p in INTERNAL_PIPELINES)}")
    print(f" Branches:   {len(data['branches'])} ({', '.join(b['name'] for b in data['branches'])})")
    print(f"{'═' * 70}")

    for branch_data in data["branches"]:
        print(f"\n┌─ Branch: {branch_data['name']}")
        print(f"│")

        for group in branch_data["pipeline_groups"]:
            print(f"│  ┌ {group['label']}")

            for i, pipe in enumerate(group["pipelines"]):
                is_last = i == len(group["pipelines"]) - 1
                prefix = "│  │ └─" if is_last else "│  │ ├─"
                cont = "│  │    " if not is_last else "│  │    "

                if not pipe["runs"]:
                    print(f"{prefix} {pipe['name']}: no builds found")
                else:
                    print(f"{prefix} {pipe['name']} (last {len(pipe['runs'])}):")
                    for run in pipe["runs"]:
                        result_text = run["result"] or run["status"]
                        started = format_time(run["startTime"])
                        print(
                            f"{cont}   {run['icon']} {run['buildNumber']:<35} "
                            f"{result_text:<22} {started}  [id:{run['id']}]"
                        )

                        if run["issues"]:
                            errors = [i for i in run["issues"] if i["type"] == "error"]
                            warnings = [i for i in run["issues"] if i["type"] == "warning"]
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

        print(f"│")

    print(f"{'═' * 70}")

    # Health summary
    print("\n📊 Health Summary:")
    all_pipelines_names = [p["name"] for p in PUBLIC_PIPELINES + INTERNAL_PIPELINES]
    for branch_data in data["branches"]:
        statuses = []
        for group in branch_data["pipeline_groups"]:
            for pipe in group["pipelines"]:
                if pipe["runs"]:
                    statuses.append(f"{pipe['runs'][0]['icon']} {pipe['name']}")
                else:
                    statuses.append(f"⏳ {pipe['name']}")
        print(f"  {branch_data['name']:<40} {' | '.join(statuses)}")
    print()

    # GitHub Actions summary
    if data.get("github_actions"):
        print(f"{'═' * 70}")
        print(f" GitHub Actions Status")
        print(f"{'═' * 70}")

        for wf_data in data["github_actions"]:
            print(f"\n┌─ {wf_data['name']} ({wf_data['repo']})")
            print(f"│  Workflow: {wf_data['workflow']}")
            print(f"│")

            for branch_info in wf_data["branches"]:
                if not branch_info["runs"]:
                    print(f"│  {branch_info['name']}: no runs found")
                    continue

                print(f"│  ┌ {branch_info['name']} (last {len(branch_info['runs'])})")
                for run in branch_info["runs"]:
                    started = format_time(run["createdAt"])
                    title = run["displayTitle"][:50] if run["displayTitle"] else ""
                    print(
                        f"│  │   {run['icon']} {run['result']:<12} "
                        f"{started}  {title}  [id:{run['id']}]"
                    )
                print(f"│  └")

            print(f"└")
        print()
        print(f"{'═' * 70}")

        # GitHub Actions health line
        print("\n🐙 GitHub Actions Health:")
        for wf_data in data["github_actions"]:
            branch_statuses = []
            for branch_info in wf_data["branches"]:
                if branch_info["runs"]:
                    latest = branch_info["runs"][0]
                    branch_statuses.append(f"{latest['icon']} {branch_info['name']}")
                else:
                    branch_statuses.append(f"⏳ {branch_info['name']}")
            print(f"  {wf_data['name']:<35} {' | '.join(branch_statuses)}")
        print()


def render_markdown(data: dict, output_path: str):
    """Render collected data as a markdown report with AI analysis sections."""
    lines = []

    lines.append(f"# SkiaSharp CI Status Report")
    lines.append(f"")
    lines.append(f"> Generated: **{data['timestamp']}**  ")
    lines.append(f"> Window: last {data['window']['builds_per_pipeline']} builds × "
                 f"{data['window']['branches_count']} branches")
    lines.append(f"")

    # Health summary table at the top
    lines.append(f"## Health Summary")
    lines.append(f"")
    pipe_names = [p["name"] for p in PUBLIC_PIPELINES + INTERNAL_PIPELINES]
    header = "| Branch | " + " | ".join(pipe_names) + " | Risk |"
    separator = "|--------|" + "|".join(["-----" for _ in pipe_names]) + "|------|"
    lines.append(header)
    lines.append(separator)

    for branch_data in data["branches"]:
        cells = []
        any_failed = False
        all_green = True
        for group in branch_data["pipeline_groups"]:
            for pipe in group["pipelines"]:
                if pipe["runs"]:
                    run = pipe["runs"][0]
                    result = run["result"] or run["status"]
                    rate = pipe["stats"]["pass_rate"]
                    # Show pass rate in cell only when there are failures in the window
                    rate_str = f" ({rate:.0f}%)" if rate is not None and rate < 100 and result != "succeeded" else ""
                    cells.append(f"{run['icon']} {result}{rate_str}")
                    if result == "failed":
                        any_failed = True
                        all_green = False
                    elif result != "succeeded":
                        all_green = False
                else:
                    cells.append("⏳ no runs")
                    all_green = False

        risk = "🟢 Low" if all_green else ("🔴 High" if any_failed else "🟡 Medium")
        row = f"| `{branch_data['name']}` | " + " | ".join(cells) + f" | {risk} |"
        lines.append(row)

    lines.append(f"")

    # Regressions section
    regressions = []
    for branch_data in data["branches"]:
        for group in branch_data["pipeline_groups"]:
            for pipe in group["pipelines"]:
                if pipe.get("regression"):
                    regressions.append((branch_data["name"], pipe["name"], pipe["regression"]))

    if regressions:
        lines.append(f"## Regressions Detected")
        lines.append(f"")
        lines.append(f"| Branch | Pipeline | Last Green | First Red |")
        lines.append(f"|--------|----------|-----------|-----------|")
        for branch_name, pipe_name, reg in regressions:
            lines.append(
                f"| `{branch_name}` | {pipe_name} | "
                f"`{reg['last_green_build_number']}` | "
                f"[`{reg['first_red_build_number']}`]({reg['first_red_url']}) |"
            )
        lines.append(f"")

    lines.append(f"---")
    lines.append(f"")

    # Detailed per-branch sections
    for branch_data in data["branches"]:
        lines.append(f"## `{branch_data['name']}`")
        lines.append(f"")

        for group in branch_data["pipeline_groups"]:
            lines.append(f"### {group['label']}")
            lines.append(f"")

            for pipe in group["pipelines"]:
                stats = pipe["stats"]
                rate_str = f" — pass rate: {stats['pass_rate']:.0f}%" if stats["pass_rate"] is not None else ""
                lines.append(f"#### {pipe['name']}{rate_str}")
                lines.append(f"")

                if not pipe["runs"]:
                    lines.append(f"_No builds found._")
                    lines.append(f"")
                    continue

                # Build history table
                lines.append(f"| Status | Build | Result | Duration | Commit | Started | Link |")
                lines.append(f"|--------|-------|--------|----------|--------|---------|------|")
                for run in pipe["runs"]:
                    result_text = run["result"] or run["status"]
                    started = format_time(run["startTime"])
                    link = f"[{run['id']}]({run['url']})"
                    dur = f"{run['durationMinutes']}m" if run.get("durationMinutes") else "—"
                    commit = f"`{run['sourceVersion']}`" if run.get("sourceVersion") else "—"
                    lines.append(f"| {run['icon']} | `{run['buildNumber']}` | {result_text} | {dur} | {commit} | {started} | {link} |")
                lines.append(f"")

                # Issues for most recent run
                latest = pipe["runs"][0]
                if latest.get("issues"):
                    errors = [i for i in latest["issues"] if i["type"] == "error"]
                    warnings = [i for i in latest["issues"] if i["type"] == "warning"]

                    if errors:
                        lines.append(f"<details>")
                        lines.append(f"<summary>❌ Errors ({len(errors)})</summary>")
                        lines.append(f"")
                        lines.append(f"| Task | Message |")
                        lines.append(f"|------|---------|")
                        for e in errors:
                            msg = e["message"].replace("|", "\\|").replace("\n", " ")
                            lines.append(f"| {e['task_name']} | {msg} |")
                        lines.append(f"")
                        lines.append(f"</details>")
                        lines.append(f"")

                    if warnings:
                        lines.append(f"<details>")
                        lines.append(f"<summary>⚠️ Warnings ({len(warnings)})</summary>")
                        lines.append(f"")
                        lines.append(f"| Task | Message |")
                        lines.append(f"|------|---------|")
                        for w in warnings:
                            msg = w["message"].replace("|", "\\|").replace("\n", " ")
                            lines.append(f"| {w['task_name']} | {msg} |")
                        lines.append(f"")
                        lines.append(f"</details>")
                        lines.append(f"")

                # Associated commits for failing build
                if latest.get("changes"):
                    lines.append(f"<details>")
                    lines.append(f"<summary>📝 Associated commits ({len(latest['changes'])})</summary>")
                    lines.append(f"")
                    lines.append(f"| Commit | Author | Message |")
                    lines.append(f"|--------|--------|---------|")
                    for c in latest["changes"]:
                        msg = c["message"].replace("|", "\\|")
                        lines.append(f"| `{c['id']}` | {c['author']} | {msg} |")
                    lines.append(f"")
                    lines.append(f"</details>")
                    lines.append(f"")

        lines.append(f"---")
        lines.append(f"")

    # GitHub Actions section
    if data.get("github_actions"):
        lines.append(f"## GitHub Actions")
        lines.append(f"")

        # Summary table
        lines.append(f"| Workflow | Repository | " + " | ".join(b["name"] for b in data["github_actions"][0]["branches"]) + " |")
        lines.append(f"|----------|------------|" + "|".join(["------" for _ in data["github_actions"][0]["branches"]]) + "|")
        for wf_data in data["github_actions"]:
            cells = []
            for branch_info in wf_data["branches"]:
                if branch_info["runs"]:
                    latest = branch_info["runs"][0]
                    rate = branch_info["stats"]["pass_rate"]
                    rate_str = f" ({rate:.0f}%)" if rate is not None and rate < 100 else ""
                    cells.append(f"{latest['icon']} {latest['result']}{rate_str}")
                else:
                    cells.append("— no runs")
            lines.append(f"| {wf_data['name']} | `{wf_data['repo']}` | " + " | ".join(cells) + " |")
        lines.append(f"")

        # Detailed per-workflow sections
        for wf_data in data["github_actions"]:
            lines.append(f"### {wf_data['name']} (`{wf_data['repo']}`)")
            lines.append(f"")

            for branch_info in wf_data["branches"]:
                if not branch_info["runs"]:
                    continue

                stats = branch_info["stats"]
                rate_str = f" — pass rate: {stats['pass_rate']:.0f}%" if stats["pass_rate"] is not None else ""
                lines.append(f"#### `{branch_info['name']}`{rate_str}")
                lines.append(f"")
                lines.append(f"| Status | Title | Result | Duration | Commit | Started | Link |")
                lines.append(f"|--------|-------|--------|----------|--------|---------|------|")
                for run in branch_info["runs"]:
                    started = format_time(run["createdAt"])
                    title = run["displayTitle"][:60] if run["displayTitle"] else "—"
                    dur = f"{run['durationMinutes']}m" if run.get("durationMinutes") else "—"
                    commit = f"`{run['headSha']}`" if run.get("headSha") else "—"
                    link = f"[{run['id']}]({run['url']})" if run.get("url") else str(run.get("id", "—"))
                    lines.append(f"| {run['icon']} | {title} | {run['result']} | {dur} | {commit} | {started} | {link} |")
                lines.append(f"")

        lines.append(f"---")
        lines.append(f"")

    # Footer
    lines.append(f"## Pipeline Links")
    lines.append(f"")
    lines.append(f"| Pipeline | Org | Definition |")
    lines.append(f"|----------|-----|------------|")
    for pipe in PUBLIC_PIPELINES + INTERNAL_PIPELINES:
        org_short = "xamarin/public" if pipe["org"] == ORG_XAMARIN else "devdiv/DevDiv"
        url = f"{pipe['org'].rstrip('/')}/{pipe['project']}/_build?definitionId={pipe['id']}"
        lines.append(f"| {pipe['name']} | {org_short} | [{pipe['id']}]({url}) |")
    lines.append(f"")

    # GitHub workflow links
    if GITHUB_WORKFLOWS:
        lines.append(f"| Workflow | Repository | File |")
        lines.append(f"|----------|------------|------|")
        for wf in GITHUB_WORKFLOWS:
            url = f"https://github.com/{wf['repo']}/actions/workflows/{wf['workflow']}"
            lines.append(f"| {wf['name']} | `{wf['repo']}` | [{wf['workflow']}]({url}) |")
        lines.append(f"")

    content = "\n".join(lines)
    os.makedirs(os.path.dirname(output_path) or ".", exist_ok=True)
    with open(output_path, "w") as f:
        f.write(content)
    print(f"\n📄 Markdown report written to: {output_path}")


def render_json(data: dict, output_path: str):
    """Write raw structured JSON data for AI consumption."""
    os.makedirs(os.path.dirname(output_path) or ".", exist_ok=True)
    with open(output_path, "w") as f:
        json.dump(data, f, indent=2)
    print(f"📊 JSON data written to: {output_path}")


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
    parser.add_argument(
        "--output", type=str, default=None,
        help="Write a markdown report to the specified file path"
    )
    parser.add_argument(
        "--json", type=str, default=None, dest="json_output",
        help="Write raw structured JSON data to the specified file (for AI analysis)"
    )
    args = parser.parse_args()

    # Collect branches to check
    branches = ["main"]
    release_branches = get_release_branches(top=args.branches)
    branches.extend(release_branches)

    # Collect all data
    show_issues = not args.no_issues
    data = collect_data(branches, args.builds, show_issues)

    # Always render console output
    render_console(data)

    # Optionally write markdown report
    if args.output:
        render_markdown(data, args.output)

    # Optionally write JSON data
    if args.json_output:
        render_json(data, args.json_output)


if __name__ == "__main__":
    main()
