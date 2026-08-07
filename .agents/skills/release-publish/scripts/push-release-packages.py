#!/usr/bin/env python3
"""Audit or publish the exact tested packages to NuGet.org."""

from __future__ import annotations

import argparse
import json
from pathlib import Path
import shutil
import sys
import tempfile

import release_publish as publish


AZURE_ORG = "https://devdiv.visualstudio.com"
AZURE_PROJECT = "DevDiv"
PUBLISH_PIPELINE_ID = 25298
AZURE_WEB_ORG = "devdiv"


class AzurePublish:
    def __init__(self) -> None:
        self.az_path = shutil.which("az")
        if not self.az_path:
            raise publish.PublishError("Azure CLI 'az' was not found on PATH")

    def json(self, args: list[str], *, timeout: int = 120):
        return publish.run_json(
            [self.az_path, *args],
            timeout=timeout,
        )

    @staticmethod
    def request_body(
        managed_build_number: str,
        *,
        stable: bool,
        preview: bool,
    ) -> dict:
        return {
            "previewRun": preview,
            "resources": {
                "pipelines": {
                    "SkiaSharp": {
                        "version": managed_build_number,
                    }
                }
            },
            "templateParameters": {
                "selectedResource": "SkiaSharp",
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
                    f"pipelineId={PUBLISH_PIPELINE_ID}",
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
        managed_build_number: str,
        *,
        stable: bool,
    ) -> bool:
        response = self.invoke_run(
            self.request_body(
                managed_build_number,
                stable=stable,
                preview=True,
            )
        )
        return response.get("id") == -1 and bool(response.get("finalYaml"))

    def queue(
        self,
        managed_build_number: str,
        *,
        stable: bool,
    ) -> dict:
        return self.invoke_run(
            self.request_body(
                managed_build_number,
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
                f"pipelineId={PUBLISH_PIPELINE_ID}",
                f"runId={run_id}",
                "--api-version",
                "7.1",
                "-o",
                "json",
            ]
        )

    def matching_runs(
        self,
        managed_run_id: int,
        build_number: str,
        *,
        stable: bool,
    ) -> list[dict]:
        expected_name = f"SkiaSharp {build_number}"
        builds = self.json(
            [
                "pipelines",
                "runs",
                "list",
                "--pipeline-ids",
                str(PUBLISH_PIPELINE_ID),
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
        candidates = [
            build
            for build in builds
            if build.get("buildNumber") == expected_name
            or build.get("status") != "completed"
        ]
        matched = []
        for build in candidates:
            detail = self.run_detail(int(build["id"]))
            resource = (
                (detail.get("resources") or {})
                .get("pipelines", {})
                .get("SkiaSharp", {})
            )
            pipeline = resource.get("pipeline") or {}
            parameters = detail.get("templateParameters") or {}
            if int(pipeline.get("id") or 0) != managed_run_id:
                continue
            if resource.get("version") != build_number:
                continue
            if parameters.get("selectedResource") != "SkiaSharp":
                continue
            if str(parameters.get("pushPackages")).lower() != "true":
                continue
            if (
                str(parameters.get("pushStable")).lower()
                != str(stable).lower()
            ):
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
    return publish.shell_command(command)


def wait_command(args, source_sha: str, publish_run_id: int) -> str:
    return publish.shell_command(
        [
            sys.executable,
            ".agents/skills/release-publish/scripts/"
            "wait-release-packages.py",
            args.release_branch,
            "--expect-source-sha",
            source_sha,
            "--expect-managed-run",
            str(args.expect_managed_run),
            "--expect-tests-run",
            str(args.expect_tests_run),
            "--publish-run",
            str(publish_run_id),
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
    runs = azure.matching_runs(
        args.expect_managed_run,
        handoff["managed"]["buildNumber"],
        stable=release.stable,
    )
    latest = runs[0] if runs else None
    preview_valid = (
        None
        if nuget["state"] == "ready"
        else azure.preview(
            handoff["managed"]["buildNumber"],
            stable=release.stable,
        )
        if validate_request
        else None
    )
    if preview_valid is False:
        raise publish.PublishError("Azure publish pipeline preview failed")

    states = package_states(nuget["state"], latest)
    next_action = states["nextAction"]

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
        "waitCommand": (
            wait_command(args, source_sha, latest["runId"])
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
    should_queue = not latest or (
        latest.get("status") == "completed"
        and latest.get("result") != "succeeded"
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
        state["waitCommand"] = wait_command(
            args,
            state["release"]["sourceSha"],
            queued_run_id,
        )
        return state
    if latest:
        state["dryRun"] = False
        return state
    raise publish.PublishError("failed to queue the Azure publication run")


def create_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("release_branch")
    parser.add_argument("--expect-source-sha", required=True)
    parser.add_argument("--expect-managed-run", required=True, type=int)
    parser.add_argument("--expect-tests-run", required=True, type=int)
    parser.add_argument("--dry-run", action="store_true")
    return parser


def main() -> int:
    args = create_parser().parse_args()
    try:
        report = current_state(args) if args.dry_run else execute(args)
        print(json.dumps(report, indent=2))
    except (publish.PublishError, OSError) as error:
        print(f"ERROR: {error}", file=sys.stderr)
        return 1
    return 0


if __name__ == "__main__":
    sys.exit(main())
