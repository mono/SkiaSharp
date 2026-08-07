#!/usr/bin/env python3
"""Wait for one exact Azure publication run and both NuGet.org packages."""

from __future__ import annotations

import argparse
import json
from pathlib import Path
import shutil
import sys
import time

import release_publish as publish


PUSH_SCRIPT = Path(__file__).with_name("push-release-packages.py")
AZURE_PROJECT = "DevDiv"
AZURE_WEB_ORG = "devdiv"
AZURE_ORG = "https://devdiv.visualstudio.com"
PUBLISH_PIPELINE_ID = 25298


def run_url(run_id: int) -> str:
    return (
        f"https://dev.azure.com/{AZURE_WEB_ORG}/{AZURE_PROJECT}/"
        f"_build/results?buildId={run_id}&view=results"
    )


def push_audit_command(args) -> list[str]:
    return [
        sys.executable,
        str(PUSH_SCRIPT),
        args.release_branch,
        "--expect-source-sha",
        args.expect_source_sha,
        "--expect-managed-run",
        str(args.expect_managed_run),
        "--expect-tests-run",
        str(args.expect_tests_run),
        "--dry-run",
    ]


def azure_run_detail(run_id: int) -> dict:
    az = shutil.which("az")
    if not az:
        raise publish.PublishError("Azure CLI 'az' was not found on PATH")
    return publish.run_json(
        [
            az,
            "devops",
            "invoke",
            "--org",
            AZURE_ORG,
            "--area",
            "pipelines",
            "--resource",
            "runs",
            "--route-parameters",
            f"project={AZURE_PROJECT}",
            f"pipelineId={PUBLISH_PIPELINE_ID}",
            f"runId={run_id}",
            "--api-version",
            "7.1",
            "-o",
            "json",
        ]
    )


def validate_run(detail: dict, report: dict, args) -> None:
    resource = (
        (detail.get("resources") or {})
        .get("pipelines", {})
        .get("SkiaSharp", {})
    )
    if not resource:
        return
    pipeline = resource.get("pipeline") or {}
    parameters = detail.get("templateParameters") or {}
    expected_build = report["release"]["buildNumber"]
    stable = report["release"]["type"] in {"stable", "hotfix stable"}
    if int(pipeline.get("id") or 0) != args.expect_managed_run:
        raise publish.PublishError(
            "publication run uses a different managed run"
        )
    if resource.get("version") != expected_build:
        raise publish.PublishError(
            f"publication run uses {resource.get('version')}, "
            f"expected {expected_build}"
        )
    if parameters.get("selectedResource") != "SkiaSharp":
        raise publish.PublishError(
            "publication run selected a different pipeline resource"
        )
    if str(parameters.get("pushPackages")).lower() != "true":
        raise publish.PublishError("publication run does not push packages")
    if (
        str(parameters.get("pushStable")).lower()
        != str(stable).lower()
    ):
        raise publish.PublishError(
            "publication run has the wrong Stable/Preview destination"
        )


def current_state(args) -> dict:
    report = publish.run_json(
        push_audit_command(args),
        timeout=300,
    )
    detail = azure_run_detail(args.publish_run)
    validate_run(detail, report, args)
    state = detail.get("state")
    result = detail.get("result")
    url = run_url(args.publish_run)
    report["publishRun"] = {
        "runId": args.publish_run,
        "name": detail.get("name"),
        "status": state,
        "result": result,
        "url": url,
    }
    report["approvalUrl"] = url
    if report["nuget"]["state"] == "ready":
        report["nextAction"] = "start-release-draft"
    elif state == "completed" and result == "succeeded":
        report["nextAction"] = "wait-for-nuget"
    elif state == "completed":
        report["nextAction"] = "retry-publish-run"
    else:
        report["nextAction"] = "approve-publish-run"
    report["operations"][0] = publish.operation(
        "publish-packages",
        "done"
        if report["nuget"]["state"] == "ready"
        else "failed"
        if state == "completed" and result != "succeeded"
        else "running",
        (
            "Both exact public package versions are on NuGet.org"
            if report["nuget"]["state"] == "ready"
            else f"Run {args.publish_run} {result}"
            if state == "completed"
            else "Waiting for human approval and protected publication"
        ),
        url=url,
    )
    report["executionCommand"] = (
        execution_command(args)
        if report["nextAction"] != "start-release-draft"
        else None
    )
    report.pop("waitCommand", None)
    return report


def execution_command(args) -> str:
    return publish.shell_command(
        [
            sys.executable,
            ".agents/skills/release-publish/scripts/"
            "wait-release-packages.py",
            args.release_branch,
            "--expect-source-sha",
            args.expect_source_sha,
            "--expect-managed-run",
            str(args.expect_managed_run),
            "--expect-tests-run",
            str(args.expect_tests_run),
            "--publish-run",
            str(args.publish_run),
            "--wait-minutes",
            str(args.wait_minutes),
        ]
    )


def execute(args) -> dict:
    url = run_url(args.publish_run)
    print(
        f"Waiting for protected Azure publication run {args.publish_run}: {url}",
        file=sys.stderr,
        flush=True,
    )
    deadline = time.monotonic() + args.wait_minutes * 60
    while True:
        report = current_state(args)
        report["dryRun"] = False
        if report["nextAction"] == "start-release-draft":
            return report
        run = report.get("publishRun")
        if run and run.get("status") == "completed":
            if run.get("result") != "succeeded":
                raise publish.PublishError(
                    f"publish run {args.publish_run} "
                    f"{run.get('result')}: {url}"
                )
        if time.monotonic() >= deadline:
            raise publish.PublishError(
                f"timed out after {args.wait_minutes} minutes waiting for "
                f"run {args.publish_run} and NuGet.org: {url}"
            )
        phase = (
            f"{run.get('status')}; NuGet={report['nuget']['state']}"
            if run
            else f"not visible; NuGet={report['nuget']['state']}"
        )
        print(
            f"Waiting: run {args.publish_run} {phase}; {url}",
            file=sys.stderr,
            flush=True,
        )
        time.sleep(args.poll_seconds)


def create_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("release_branch")
    parser.add_argument("--expect-source-sha", required=True)
    parser.add_argument("--expect-managed-run", required=True, type=int)
    parser.add_argument("--expect-tests-run", required=True, type=int)
    parser.add_argument("--publish-run", required=True, type=int)
    parser.add_argument("--dry-run", action="store_true")
    parser.add_argument("--wait-minutes", type=int, default=60)
    parser.add_argument(
        "--poll-seconds",
        type=int,
        default=30,
        help=argparse.SUPPRESS,
    )
    return parser


def main() -> int:
    args = create_parser().parse_args()
    try:
        if args.wait_minutes <= 0 or args.poll_seconds <= 0:
            raise publish.PublishError("wait and poll values must be positive")
        report = current_state(args) if args.dry_run else execute(args)
        print(json.dumps(report, indent=2))
    except (publish.PublishError, OSError) as error:
        print(f"ERROR: {error}", file=sys.stderr)
        return 1
    return 0


if __name__ == "__main__":
    sys.exit(main())
