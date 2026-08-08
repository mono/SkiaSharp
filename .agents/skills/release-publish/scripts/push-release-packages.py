#!/usr/bin/env python3
"""Audit or publish the exact tested packages to NuGet.org."""

from __future__ import annotations

import argparse
import json
import sys
import time

import release_publish as publish


AzurePublish = publish.AzurePublish
run_url = publish.azure_publish_run_url


def load_release(args):
    repo = publish.GitRepository.discover()
    release = publish.ReleaseVersion.parse(args.release_branch)
    status = publish.status_report(repo.root, release.branch)
    handoff = publish.validate_status_handoff(
        status,
        release,
        expected_sha=args.expect_source_sha,
        expected_managed_run=args.expect_managed_run,
        expected_tests_run=args.expect_tests_run,
    )
    source_sha = repo.resolve_release_sha(
        release.branch,
        args.expect_source_sha,
    )
    return repo, release, status, handoff, source_sha


def execution_command(args, source_sha: str) -> str:
    command = [
        sys.executable,
        ".agents/skills/release-publish/scripts/push-release-packages.py",
        args.release_branch,
        "--expect-source-sha",
        source_sha,
        "--expect-managed-run",
        str(args.expect_managed_run),
        "--expect-tests-run",
        str(args.expect_tests_run),
    ]
    if args.verification == "azure":
        command.extend(["--verification", "azure"])
    return publish.shell_command(command)


def resume_command(args, source_sha: str, publish_run_id: int) -> str:
    return publish.shell_command(
        [
            sys.executable,
            ".agents/skills/release-publish/scripts/push-release-packages.py",
            args.release_branch,
            "--expect-source-sha",
            source_sha,
            "--expect-managed-run",
            str(args.expect_managed_run),
            "--expect-tests-run",
            str(args.expect_tests_run),
            "--publish-run",
            str(publish_run_id),
            "--wait",
            "--wait-minutes",
            str(args.wait_minutes),
            "--verification",
            args.verification,
        ]
    )


def package_states(
    nuget_state: str,
    latest: dict | None,
    verification: str = "nuget",
) -> dict:
    if nuget_state == "ready":
        return {
            "publish": "done",
            "verify": "done",
            "detail": "Both exact public packages are on NuGet.org",
            "nextAction": "start-release-draft",
        }
    if (
        verification == "azure"
        and latest
        and latest["status"] == "completed"
        and latest["result"] == "succeeded"
    ):
        return {
            "publish": "done",
            "verify": "done",
            "detail": (
                "Exact Azure publication run succeeded; NuGet.org indexing "
                "was not required"
            ),
            "nextAction": "start-release-draft",
        }
    if latest and latest["status"] != "completed":
        return {
            "publish": "running",
            "verify": "running",
            "detail": (
                "Exact publish run is queued/running and may await approval"
            ),
            "nextAction": "approve-publish-run",
        }
    if latest and latest["result"] == "succeeded":
        return {
            "publish": "running",
            "verify": "running",
            "detail": "Publish succeeded; waiting for both NuGet packages",
            "nextAction": "wait-for-nuget",
        }
    return {
        "publish": "pending",
        "verify": "blocked",
        "detail": (
            "Retry the exact failed/canceled publish run"
            if latest
            else "Queue the exact live Azure publish run"
        ),
        "nextAction": "confirm-publish-packages",
    }


def current_state(args, *, validate_request: bool = True) -> dict:
    repo, release, status, handoff, source_sha = load_release(args)
    nuget = publish.NuGet().check(handoff["versions"])
    azure = AzurePublish()
    if args.publish_run:
        detail = azure.run_detail(args.publish_run)
        validate_run_detail(
            detail,
            managed_run_id=args.expect_managed_run,
            managed_build_number=handoff["managed"]["buildNumber"],
            stable=release.stable,
        )
        latest = {
            "runId": args.publish_run,
            "name": detail.get("name"),
            "status": detail.get("state"),
            "result": detail.get("result"),
            "url": run_url(args.publish_run),
        }
    else:
        runs = azure.matching_runs(
            args.expect_managed_run,
            handoff["managed"]["buildNumber"],
            stable=release.stable,
        )
        latest = runs[0] if runs else None
    needs_queue = (
        args.publish_run is None
        and nuget["state"] != "ready"
        and (
            not latest
            or latest.get("status") == "completed"
            and latest.get("result") != "succeeded"
        )
    )
    preview_valid = (
        None
        if not needs_queue
        else azure.preview(
            handoff["managed"]["buildNumber"],
            stable=release.stable,
        )
        if validate_request
        else None
    )
    if preview_valid is False:
        raise publish.PublishError("Azure publish pipeline preview failed")

    states = package_states(
        nuget["state"],
        latest,
        verification=args.verification,
    )
    next_action = states["nextAction"]
    if (
        args.publish_run
        and nuget["state"] != "ready"
        and latest
        and latest.get("status") == "completed"
        and latest.get("result") != "succeeded"
    ):
        next_action = "retry-publish-run"

    warnings = list(status.get("warnings") or [])
    if (
        args.verification == "azure"
        and next_action == "start-release-draft"
        and nuget["state"] != "ready"
    ):
        warnings.append(
            "Azure publication succeeded; exact NuGet.org indexing was not "
            "verified"
        )

    return {
        "schemaVersion": 1,
        "dryRun": bool(args.dry_run),
        "release": {
            "branch": release.branch,
            "version": release.raw,
            "type": release.release_type,
            "sourceSha": source_sha,
            "managedRunId": args.expect_managed_run,
            "testsRunId": args.expect_tests_run,
            "buildNumber": handoff["managed"]["buildNumber"],
            "testPackages": handoff["versions"]["test"],
            "publicPackages": handoff["versions"]["public"],
        },
        "pipelineRequestValid": preview_valid,
        "verificationMode": args.verification,
        "publishRun": latest,
        "nuget": nuget,
        "operations": [
            publish.operation(
                "publish-packages",
                states["publish"],
                states["detail"],
                url=latest.get("url") if latest else None,
            ),
            publish.operation(
                "verify-nuget",
                states["verify"],
                (
                    "Both exact public package versions are indexed"
                    if nuget["state"] == "ready"
                    else (
                        "Exact Azure publication run succeeded; NuGet.org "
                        "indexing was not required"
                    )
                    if args.verification == "azure"
                    and latest
                    and latest.get("status") == "completed"
                    and latest.get("result") == "succeeded"
                    else f"NuGet.org state is {nuget['state']}"
                ),
            ),
        ],
        "nextAction": next_action,
        "warnings": warnings,
        "executionCommand": (
            execution_command(args, source_sha)
            if next_action == "confirm-publish-packages"
            else None
        ),
        "resumeCommand": (
            resume_command(args, source_sha, latest["runId"])
            if latest
            and next_action in {
                "approve-publish-run",
                "wait-for-nuget",
            }
            else None
        ),
    }


def execute(args) -> dict:
    state = current_state(args)
    if state["nuget"]["state"] == "ready":
        state["dryRun"] = False
        return state
    latest = state.get("publishRun")
    should_queue = args.publish_run is None and (
        not latest
        or (
            latest.get("status") == "completed"
            and latest.get("result") != "succeeded"
        )
    )
    azure = AzurePublish()
    if should_queue:
        queued = azure.queue(
            state["release"]["buildNumber"],
            stable=publish.ReleaseVersion.parse(
                args.release_branch
            ).stable,
        )
        queued_run_id = int(queued["id"])
        url = run_url(queued_run_id)
        print(
            f"Queued protected Azure publication run {queued_run_id}: "
            f"{url}",
            file=sys.stderr,
            flush=True,
        )
        state["dryRun"] = False
        state["publishRun"] = {
            "runId": queued_run_id,
            "name": queued.get("name"),
            "status": queued.get("state"),
            "result": queued.get("result"),
            "url": url,
        }
        state["operations"][0] = publish.operation(
            "publish-packages",
            "running",
            "Exact publish run is queued and requires human approval",
            url=url,
        )
        state["nextAction"] = "approve-publish-run"
        state["executionCommand"] = None
        state["resumeCommand"] = resume_command(
            args,
            state["release"]["sourceSha"],
            queued_run_id,
        )
        return state
    if latest:
        state["dryRun"] = False
        return state
    raise publish.PublishError("failed to queue the Azure publication run")


def validate_run_detail(
    detail: dict,
    *,
    managed_run_id: int,
    managed_build_number: str,
    stable: bool,
) -> None:
    resource = (
        (detail.get("resources") or {})
        .get("pipelines", {})
        .get("SkiaSharp", {})
    )
    if not resource:
        if detail.get("state") == "completed":
            return
        raise publish.PublishError(
            "publication run has not resolved its SkiaSharp resource"
        )
    pipeline = resource.get("pipeline") or {}
    parameters = detail.get("templateParameters") or {}
    # Pipeline resources expose the selected run ID in pipeline.id.
    if int(pipeline.get("id") or 0) != managed_run_id:
        raise publish.PublishError(
            "publication run uses a different managed run"
        )
    if resource.get("version") != managed_build_number:
        raise publish.PublishError(
            f"publication run uses {resource.get('version')}, "
            f"expected {managed_build_number}"
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


def execute_and_wait(args) -> dict:
    state = execute(args)
    if state["nuget"]["state"] == "ready":
        return state
    run = state.get("publishRun")
    if not run:
        raise publish.PublishError("no exact publication run is available")
    args.publish_run = int(run["runId"])
    url = run_url(args.publish_run)
    print(
        f"Waiting for protected Azure publication run "
        f"{args.publish_run}: {url}",
        file=sys.stderr,
        flush=True,
    )
    deadline = time.monotonic() + args.wait_minutes * 60
    while True:
        state = current_state(args, validate_request=False)
        state["dryRun"] = False
        if state["nextAction"] == "start-release-draft":
            return state
        run = state["publishRun"]
        if (
            run.get("status") == "completed"
            and run.get("result") != "succeeded"
        ):
            raise publish.PublishError(
                f"publish run {args.publish_run} "
                f"{run.get('result')}: {url}"
            )
        if time.monotonic() >= deadline:
            missing = (
                [
                    {
                        "package": package_id,
                        "version": package["version"],
                    }
                    for package_id, package in state["nuget"][
                        "packages"
                    ].items()
                    if not package["available"]
                ]
                if args.verification == "nuget"
                else []
            )
            state["wait"] = {
                "timedOut": True,
                "minutes": args.wait_minutes,
                "missingPackages": missing,
            }
            if args.verification == "azure":
                wait_subject = f"Azure run {args.publish_run}"
            elif (
                run.get("status") == "completed"
                and run.get("result") == "succeeded"
            ):
                wait_subject = (
                    f"Azure run {args.publish_run} succeeded, but NuGet.org "
                    "indexing"
                )
            else:
                wait_subject = (
                    f"Azure run {args.publish_run} and NuGet.org indexing"
                )
            state["warnings"].append(
                f"{wait_subject} did not complete within "
                f"{args.wait_minutes} minutes"
            )
            return state
        print(
            f"Waiting: run {args.publish_run} {run.get('status')}; "
            f"NuGet={state['nuget']['state']}; {url}",
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
    parser.add_argument("--publish-run", type=int)
    parser.add_argument("--dry-run", action="store_true")
    parser.add_argument("--wait", action="store_true")
    parser.add_argument(
        "--verification",
        choices=("nuget", "azure"),
        default="nuget",
        help=(
            "Require exact NuGet.org indexing (default), or treat a successful "
            "protected Azure publication run as sufficient"
        ),
    )
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
        report = (
            current_state(args)
            if args.dry_run
            else execute_and_wait(args)
            if args.wait
            else execute(args)
        )
        print(json.dumps(report, indent=2))
    except (publish.PublishError, OSError) as error:
        print(f"ERROR: {error}", file=sys.stderr)
        return 1
    return 0


if __name__ == "__main__":
    sys.exit(main())
