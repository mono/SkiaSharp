#!/usr/bin/env python3
"""Audit or publish the exact tested packages to NuGet.org."""

from __future__ import annotations

import argparse
import json
import os
from pathlib import Path
import shutil
import sys
import tempfile
import time

import release_publish as publish


# dnceng/internal is the immutable pipeline home for the Build pipeline that
# registers each BAR build (skiasharp-package, definition 1642). There is
# currently no protected NuGet.org publisher pipeline definition, so its ID
# must be supplied explicitly via SKIASHARP_NUGET_PUBLISH_PIPELINE_ID.
AZURE_ORG = "https://dev.azure.com/dnceng"
AZURE_PROJECT = "internal"
AZURE_WEB_ORG = "dnceng"
PUBLISH_PIPELINE_ID_ENV = "SKIASHARP_NUGET_PUBLISH_PIPELINE_ID"

# The folder-qualified Build pipeline resource that the protected publisher
# must be pinned to. This is the only pipeline that produces BAR builds for
# SkiaSharp, so every publication request must reference it exactly.
RESOURCE_ALIAS = "SkiaSharp"
RESOURCE_NAME = "skiasharp-package"
RESOURCE_FOLDER = r"\dotnet\skiasharp"
RESOURCE_PATH = r"\dotnet\skiasharp\skiasharp-package"


def pipeline_resource_path(pipeline: dict) -> str:
    name = str(pipeline.get("name") or "").replace("/", "\\")
    if name.startswith("\\"):
        return name
    folder = str(pipeline.get("folder") or "").replace("/", "\\").rstrip("\\")
    return f"{folder}\\{name}" if folder else name


def publish_pipeline_id() -> int:
    raw = os.environ.get(PUBLISH_PIPELINE_ID_ENV)
    if not raw or not raw.strip():
        raise publish.PublishError(
            "the protected NuGet.org publisher pipeline is not configured: "
            f"set {PUBLISH_PIPELINE_ID_ENV} to its dnceng/internal pipeline "
            "definition ID before running push-release-packages.py"
        )
    try:
        return int(raw.strip())
    except ValueError as error:
        raise publish.PublishError(
            f"{PUBLISH_PIPELINE_ID_ENV} must be an integer pipeline "
            f"definition ID, got {raw!r}"
        ) from error


class AzurePublish:
    def __init__(self) -> None:
        self.az_path = shutil.which("az")
        if not self.az_path:
            raise publish.PublishError("Azure CLI 'az' was not found on PATH")
        self.pipeline_id = publish_pipeline_id()

    def json(self, args: list[str], *, timeout: int = 120):
        return publish.run_json(
            [self.az_path, *args],
            timeout=timeout,
        )

    @staticmethod
    def request_body(
        bar_build_id: int,
        build_run_id: int,
        build_number: str,
        *,
        stable: bool,
        preview: bool,
    ) -> dict:
        return {
            "previewRun": preview,
            "resources": {
                "pipelines": {
                    RESOURCE_ALIAS: {
                        "version": build_number,
                    }
                }
            },
            "templateParameters": {
                "selectedResource": RESOURCE_ALIAS,
                "buildRunId": build_run_id,
                "barBuildId": bar_build_id,
                "pushPackages": True,
                "pushStable": stable,
            },
        }

    def invoke_run(self, body: dict) -> dict:
        path: Path | None = None
        try:
            with tempfile.NamedTemporaryFile(
                mode="w",
                encoding="utf-8",
                suffix=".json",
                delete=False,
            ) as stream:
                json.dump(body, stream)
                path = Path(stream.name)
            return self.json(
                [
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
                    f"pipelineId={self.pipeline_id}",
                    "--http-method",
                    "POST",
                    "--in-file",
                    str(path),
                    "--api-version",
                    "7.1",
                    "-o",
                    "json",
                ],
                timeout=240,
            )
        finally:
            if path:
                path.unlink(missing_ok=True)

    def preview(
        self,
        bar_build_id: int,
        build_run_id: int,
        build_number: str,
        *,
        stable: bool,
    ) -> bool:
        response = self.invoke_run(
            self.request_body(
                bar_build_id,
                build_run_id,
                build_number,
                stable=stable,
                preview=True,
            )
        )
        return response.get("id") == -1 and bool(response.get("finalYaml"))

    def queue(
        self,
        bar_build_id: int,
        build_run_id: int,
        build_number: str,
        *,
        stable: bool,
    ) -> dict:
        return self.invoke_run(
            self.request_body(
                bar_build_id,
                build_run_id,
                build_number,
                stable=stable,
                preview=False,
            )
        )

    def run_detail(self, run_id: int) -> dict:
        return self.json(
            [
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
                f"pipelineId={self.pipeline_id}",
                f"runId={run_id}",
                "--api-version",
                "7.1",
                "-o",
                "json",
            ]
        )

    def matching_runs(
        self,
        bar_build_id: int,
        build_run_id: int,
        build_number: str,
        *,
        stable: bool,
    ) -> list[dict]:
        builds = self.json(
            [
                "pipelines",
                "runs",
                "list",
                "--pipeline-ids",
                str(self.pipeline_id),
                "--org",
                AZURE_ORG,
                "--project",
                AZURE_PROJECT,
                "--top",
                "100",
                "-o",
                "json",
            ]
        )
        matched = []
        for build in builds:
            detail = self.run_detail(int(build["id"]))
            try:
                validate_run_detail(
                    detail,
                    bar_build_id=bar_build_id,
                    build_run_id=build_run_id,
                    build_number=build_number,
                    stable=stable,
                )
            except publish.PublishError:
                continue
            matched.append(
                {
                    "runId": int(build["id"]),
                    "name": build.get("buildNumber"),
                    "status": build.get("status"),
                    "result": build.get("result"),
                    "queueTime": build.get("queueTime"),
                    "url": run_url(int(build["id"])),
                }
            )
        return sorted(
            matched,
            key=lambda item: item["runId"],
            reverse=True,
        )


def run_url(run_id: int) -> str:
    return (
        f"https://dev.azure.com/{AZURE_WEB_ORG}/{AZURE_PROJECT}/"
        f"_build/results?buildId={run_id}&view=results"
    )


def validate_run_detail(
    detail: dict,
    *,
    bar_build_id: int,
    build_run_id: int,
    build_number: str,
    stable: bool,
) -> None:
    resource = (
        (detail.get("resources") or {})
        .get("pipelines", {})
        .get(RESOURCE_ALIAS, {})
    )
    if not resource:
        raise publish.PublishError(
            "publication run has not resolved its skiasharp-package resource"
        )
    pipeline = resource.get("pipeline") or {}
    parameters = detail.get("templateParameters") or {}
    if pipeline_resource_path(pipeline) != RESOURCE_PATH:
        raise publish.PublishError(
            "publication run resource is "
            f"{pipeline.get('folder')!r}\\{pipeline.get('name')!r}, "
            f"expected {RESOURCE_PATH!r}"
        )
    # Resolved pipeline resources expose the source run ID in pipeline.id.
    if int(pipeline.get("id") or 0) != build_run_id:
        raise publish.PublishError(
            "publication run uses a different Build run"
        )
    if resource.get("version") != build_number:
        raise publish.PublishError(
            f"publication run uses {resource.get('version')}, "
            f"expected {build_number}"
        )
    if parameters.get("selectedResource") != RESOURCE_ALIAS:
        raise publish.PublishError(
            "publication run selected a different pipeline resource"
        )
    if int(parameters.get("buildRunId") or 0) != build_run_id:
        raise publish.PublishError(
            "publication run template parameters use a different Build run"
        )
    if int(parameters.get("barBuildId") or 0) != bar_build_id:
        raise publish.PublishError(
            "publication run uses a different BAR build"
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


def run_evidence(detail: dict, run_id: int) -> dict:
    resource = detail["resources"]["pipelines"][RESOURCE_ALIAS]
    pipeline = resource["pipeline"]
    parameters = detail["templateParameters"]
    return {
        "runId": run_id,
        "name": detail.get("name"),
        "status": detail.get("state"),
        "result": detail.get("result"),
        "url": run_url(run_id),
        "resourcePath": pipeline_resource_path(pipeline),
        "buildRunId": int(pipeline["id"]),
        "buildNumber": resource["version"],
        "barBuildId": int(parameters["barBuildId"]),
        "destination": (
            "stable" if str(parameters["pushStable"]).lower() == "true"
            else "preview"
        ),
    }


def load_release(args):
    repo = publish.GitRepository.discover()
    release = publish.ReleaseVersion.parse(args.release_branch)
    status = publish.status_report(repo.root, release.branch)
    handoff = publish.validate_status_handoff(
        status,
        release,
        expected_sha=args.expect_source_sha,
        expected_build_run=args.expect_build_run,
        expected_tests_run=args.expect_tests_run,
        expected_bar_build=args.expect_bar_build,
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
        "--expect-build-run",
        str(args.expect_build_run),
        "--expect-tests-run",
        str(args.expect_tests_run),
        "--expect-bar-build",
        str(args.expect_bar_build),
    ]
    return publish.shell_command(command)


def resume_command(args, source_sha: str, publish_run_id: int) -> str:
    return publish.shell_command(
        [
            sys.executable,
            ".agents/skills/release-publish/scripts/push-release-packages.py",
            args.release_branch,
            "--expect-source-sha",
            source_sha,
            "--expect-build-run",
            str(args.expect_build_run),
            "--expect-tests-run",
            str(args.expect_tests_run),
            "--expect-bar-build",
            str(args.expect_bar_build),
            "--publish-run",
            str(publish_run_id),
            "--wait",
            "--wait-minutes",
            str(args.wait_minutes),
        ]
    )


def package_states(nuget_state: str, latest: dict | None) -> dict:
    if nuget_state == "ready":
        return {
            "publish": "done",
            "verify": "done",
            "detail": "Both exact public packages are on NuGet.org",
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
    bar_build_id = args.expect_bar_build
    build_run_id = args.expect_build_run
    build_number = handoff["build"]["buildNumber"]
    if args.publish_run:
        detail = azure.run_detail(args.publish_run)
        validate_run_detail(
            detail,
            bar_build_id=bar_build_id,
            build_run_id=build_run_id,
            build_number=build_number,
            stable=release.stable,
        )
        latest = run_evidence(detail, args.publish_run)
    else:
        runs = azure.matching_runs(
            bar_build_id,
            build_run_id,
            build_number,
            stable=release.stable,
        )
        if len(runs) > 1:
            run_ids = ", ".join(str(run["runId"]) for run in runs)
            raise publish.PublishError(
                "multiple publication runs match this exact BAR build "
                f"{bar_build_id} ({run_ids}); rerun with --publish-run to "
                "pick the exact run"
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
            bar_build_id,
            build_run_id,
            build_number,
            stable=release.stable,
        )
        if validate_request
        else None
    )
    if preview_valid is False:
        raise publish.PublishError("Azure publish pipeline preview failed")

    states = package_states(nuget["state"], latest)
    next_action = states["nextAction"]
    if (
        args.publish_run
        and nuget["state"] != "ready"
        and latest
        and latest.get("status") == "completed"
        and latest.get("result") != "succeeded"
    ):
        next_action = "retry-publish-run"

    return {
        "schemaVersion": 1,
        "dryRun": bool(args.dry_run),
        "release": {
            "branch": release.branch,
            "version": release.raw,
            "type": release.release_type,
            "sourceSha": source_sha,
            "buildRunId": build_run_id,
            "testsRunId": args.expect_tests_run,
            "barBuildId": bar_build_id,
            "migration": status.get("migration"),
            "defaultChannelIds": handoff["bar"].get(
                "defaultChannelIds"
            ),
            "buildNumber": build_number,
            "barAssets": handoff["bar"]["assets"],
            "nonShippingAssets": handoff["bar"].get(
                "nonShippingAssets"
            ),
            "testPackages": handoff["versions"]["test"],
            "publicPackages": handoff["versions"]["public"],
        },
        "pipelineRequestValid": preview_valid,
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
                    else f"NuGet.org state is {nuget['state']}"
                ),
            ),
        ],
        "nextAction": next_action,
        "warnings": status.get("warnings") or [],
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
        stable = publish.ReleaseVersion.parse(args.release_branch).stable
        queued = azure.queue(
            state["release"]["barBuildId"],
            state["release"]["buildRunId"],
            state["release"]["buildNumber"],
            stable=stable,
        )
        queued_run_id = int(queued["id"])
        queued_url = run_url(queued_run_id)
        try:
            detail = azure.run_detail(queued_run_id)
            validate_run_detail(
                detail,
                bar_build_id=state["release"]["barBuildId"],
                build_run_id=state["release"]["buildRunId"],
                build_number=state["release"]["buildNumber"],
                stable=stable,
            )
        except publish.PublishError as error:
            raise publish.PublishError(
                f"queued publication run {queued_run_id} failed post-queue "
                f"validation; do not approve it and cancel the run: "
                f"{queued_url}: {error}"
            ) from error
        evidence = run_evidence(detail, queued_run_id)
        url = evidence["url"]
        print(
            f"Queued protected Azure publication run {queued_run_id}: "
            f"{url}",
            file=sys.stderr,
            flush=True,
        )
        state["dryRun"] = False
        state["publishRun"] = evidence
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
            missing = [
                {
                    "package": package_id,
                    "version": package["version"],
                }
                for package_id, package in state["nuget"]["packages"].items()
                if not package["available"]
            ]
            state["wait"] = {
                "timedOut": True,
                "minutes": args.wait_minutes,
                "missingPackages": missing,
            }
            state["warnings"].append(
                (
                    f"Azure run {args.publish_run} succeeded, but NuGet.org "
                    "indexing"
                    if run.get("status") == "completed"
                    and run.get("result") == "succeeded"
                    else f"Azure run {args.publish_run} and NuGet.org indexing"
                )
                + f" did not complete within {args.wait_minutes} minutes"
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
    parser.add_argument("--expect-build-run", required=True, type=int)
    parser.add_argument("--expect-tests-run", required=True, type=int)
    parser.add_argument("--expect-bar-build", required=True, type=int)
    parser.add_argument("--publish-run", type=int)
    parser.add_argument("--dry-run", action="store_true")
    parser.add_argument("--wait", action="store_true")
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
