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
    """Collect all CI data into a structured dict."""
    all_pipelines = PUBLIC_PIPELINES + INTERNAL_PIPELINES
    timestamp = datetime.now(timezone.utc).strftime("%Y-%m-%d %H:%M UTC")

    data = {
        "timestamp": timestamp,
        "branches": [],
    }

    for branch in branches:
        branch_data = {"name": branch, "pipeline_groups": []}

        for group_label, pipelines in [("Public CI", PUBLIC_PIPELINES), ("Internal Release Chain", INTERNAL_PIPELINES)]:
            group_data = {"label": group_label, "pipelines": []}

            for pipe in pipelines:
                runs = get_runs(pipe["id"], branch, pipe["org"], pipe["project"], top=top_builds)
                pipe_data = {"name": pipe["name"], "org": pipe["org"], "project": pipe["project"], "runs": []}

                for idx, r in enumerate(runs):
                    run_data = {
                        "id": r["id"],
                        "buildNumber": r["buildNumber"],
                        "status": r["status"],
                        "result": r.get("result"),
                        "startTime": r.get("startTime"),
                        "icon": icon_for(r),
                        "url": build_url(r["id"], pipe["org"], pipe["project"]),
                        "issues": None,
                    }
                    # Collect issues for the most recent failed/partial run
                    if show_issues and idx == 0 and r.get("result") in ("failed", "partiallySucceeded"):
                        run_data["issues"] = get_build_issues(r["id"], pipe["org"], pipe["project"])

                    pipe_data["runs"].append(run_data)

                group_data["pipelines"].append(pipe_data)
            branch_data["pipeline_groups"].append(group_data)

        data["branches"].append(branch_data)

    return data


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


def render_markdown(data: dict, output_path: str):
    """Render collected data as a markdown report."""
    lines = []

    lines.append(f"# SkiaSharp CI Status Report")
    lines.append(f"")
    lines.append(f"> Generated: **{data['timestamp']}**")
    lines.append(f"")

    # Health summary table at the top
    lines.append(f"## Health Summary")
    lines.append(f"")
    pipe_names = [p["name"] for p in PUBLIC_PIPELINES + INTERNAL_PIPELINES]
    header = "| Branch | " + " | ".join(pipe_names) + " |"
    separator = "|--------|" + "|".join(["-----" for _ in pipe_names]) + "|"
    lines.append(header)
    lines.append(separator)

    for branch_data in data["branches"]:
        cells = []
        for group in branch_data["pipeline_groups"]:
            for pipe in group["pipelines"]:
                if pipe["runs"]:
                    run = pipe["runs"][0]
                    result = run["result"] or run["status"]
                    cells.append(f"{run['icon']} {result}")
                else:
                    cells.append("⏳ no runs")
        row = f"| `{branch_data['name']}` | " + " | ".join(cells) + " |"
        lines.append(row)

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
                lines.append(f"#### {pipe['name']}")
                lines.append(f"")

                if not pipe["runs"]:
                    lines.append(f"_No builds found._")
                    lines.append(f"")
                    continue

                # Build history table
                lines.append(f"| Status | Build | Result | Started | Link |")
                lines.append(f"|--------|-------|--------|---------|------|")
                for run in pipe["runs"]:
                    result_text = run["result"] or run["status"]
                    started = format_time(run["startTime"])
                    link = f"[{run['id']}]({run['url']})"
                    lines.append(f"| {run['icon']} | `{run['buildNumber']}` | {result_text} | {started} | {link} |")
                lines.append(f"")

                # Issues for most recent run
                latest = pipe["runs"][0]
                if latest["issues"]:
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

    content = "\n".join(lines)
    with open(output_path, "w") as f:
        f.write(content)
    print(f"\n📄 Markdown report written to: {output_path}")


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


if __name__ == "__main__":
    main()
