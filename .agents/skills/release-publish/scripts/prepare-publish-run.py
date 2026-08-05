#!/usr/bin/env python3
"""Validate a managed build and prepare the Azure publish-pipeline request."""

import argparse
import json
import locale
import os
import re
import shutil
import subprocess
from pathlib import Path
from typing import Any


ORG = "https://dev.azure.com/devdiv"
PROJECT = "DevDiv"
MANAGED_PIPELINE_ID = 10789
PUBLISH_PIPELINE_ID = 25298
ACTIVE_STATUSES = {
    "cancelling",
    "inprogress",
    "notstarted",
    "postponed",
    "queued",
}


def cli_command(executable: str, args: list[str]) -> list[str]:
    resolved = shutil.which(executable)
    if not resolved:
        raise FileNotFoundError(f"Required CLI '{executable}' was not found on PATH.")
    return [resolved, *args]


def decode_cli_output(value: bytes | str | None, stream_name: str) -> str:
    if value is None:
        return ""
    if isinstance(value, str):
        return value

    encodings = ["utf-8-sig"]
    try:
        encodings.append(locale.getencoding())
    except AttributeError:
        encodings.append(locale.getpreferredencoding(False))
    if os.name == "nt":
        encodings.append("mbcs")

    attempted = []
    for encoding in encodings:
        normalized = encoding.lower()
        if normalized in attempted:
            continue
        attempted.append(normalized)
        try:
            return value.decode(encoding, errors="strict")
        except (LookupError, UnicodeDecodeError):
            continue

    raise RuntimeError(
        f"Could not decode Azure CLI {stream_name} using "
        f"{', '.join(attempted)}."
    )


def run_az_json(args: list[str], timeout: int = 60) -> Any:
    command = cli_command("az", args)
    try:
        result = subprocess.run(
            command,
            capture_output=True,
            timeout=timeout,
            check=True,
        )
    except subprocess.CalledProcessError as error:
        detail = decode_cli_output(error.stderr, "stderr").strip() or "no error output"
        raise RuntimeError(
            f"Azure CLI failed with exit code {error.returncode}: {detail}"
        ) from error

    output = decode_cli_output(result.stdout, "stdout").strip()
    if not output:
        detail = decode_cli_output(result.stderr, "stderr").strip() or "no error output"
        raise RuntimeError(f"Azure CLI returned no output: {detail}")

    try:
        return json.loads(output)
    except json.JSONDecodeError as error:
        raise RuntimeError(f"Azure CLI returned malformed JSON: {error}") from error


def release_kind(version: str) -> tuple[str, bool]:
    match = re.fullmatch(
        r"(?P<base>\d+\.\d+\.\d+(?:\.\d+)?)(?:-(?P<label>(?:preview|rc)\.\d+))?",
        version,
    )
    if not match:
        raise ValueError(
            "Release version must be stable X.Y.Z[.F], preview "
            "X.Y.Z[.F]-preview.N, or RC X.Y.Z[.F]-rc.N."
        )
    label = match.group("label")
    return (label or "stable", label is None)


def expected_build_number_pattern(version: str) -> tuple[re.Pattern[str], bool]:
    label, push_stable = release_kind(version)
    if push_stable:
        pattern = rf"{re.escape(version)}-{label}\.\d+\+{re.escape(version)}"
    else:
        pattern = rf"{re.escape(version)}\.\d+\+{re.escape(version)}"
    return re.compile(pattern), push_stable


def validate_managed_run(
    run: dict[str, Any],
    managed_run_id: int,
    release_version: str,
    release_commit: str,
) -> tuple[str, bool]:
    if not re.fullmatch(r"[0-9a-fA-F]{40}", release_commit):
        raise ValueError("Release commit must be the full 40-character commit SHA.")

    definition = run.get("definition") or {}
    definition_id = definition.get("id")
    if definition_id != MANAGED_PIPELINE_ID:
        raise ValueError(
            f"Run {managed_run_id} belongs to pipeline {definition_id}, "
            f"not managed pipeline {MANAGED_PIPELINE_ID}."
        )

    if run.get("status") != "completed" or run.get("result") != "succeeded":
        raise ValueError(
            f"Managed run {managed_run_id} must be completed/succeeded; "
            f"got {run.get('status')}/{run.get('result')}."
        )

    expected_branch = f"refs/heads/release/{release_version}"
    if run.get("sourceBranch") != expected_branch:
        raise ValueError(
            f"Managed run branch is {run.get('sourceBranch')!r}; "
            f"expected {expected_branch!r}."
        )

    if str(run.get("sourceVersion", "")).lower() != release_commit.lower():
        raise ValueError(
            f"Managed run commit is {run.get('sourceVersion')!r}; "
            f"expected {release_commit!r}."
        )

    build_number = run.get("buildNumber")
    pattern, push_stable = expected_build_number_pattern(release_version)
    if not isinstance(build_number, str) or not pattern.fullmatch(build_number):
        label, _ = release_kind(release_version)
        raise ValueError(
            f"Managed run build number {build_number!r} does not match the "
            f"expected {label!r} release label for {release_version!r}."
        )

    return build_number, push_stable


def find_active_publish_runs(runs: list[dict[str, Any]]) -> list[dict[str, Any]]:
    return [
        run
        for run in runs
        if str(run.get("status", "")).replace("_", "").lower() in ACTIVE_STATUSES
    ]


def build_queue_body(build_number: str, push_stable: bool) -> dict[str, Any]:
    return {
        "resources": {
            "pipelines": {
                "SkiaSharp": {
                    "version": build_number,
                }
            }
        },
        "templateParameters": {
            "selectedResource": "SkiaSharp",
            "pushPackages": True,
            "pushStable": push_stable,
        },
    }


def prepare_queue_request(
    managed_run_id: int,
    release_version: str,
    release_commit: str,
) -> tuple[dict[str, Any], str, bool]:
    managed_run = run_az_json(
        [
            "pipelines",
            "runs",
            "show",
            "--org",
            ORG,
            "--project",
            PROJECT,
            "--id",
            str(managed_run_id),
            "--query",
            "{id:id,definition:definition,buildNumber:buildNumber,status:status,"
            "result:result,sourceBranch:sourceBranch,sourceVersion:sourceVersion}",
            "--only-show-errors",
            "-o",
            "json",
        ]
    )
    if not isinstance(managed_run, dict):
        raise RuntimeError("Azure CLI returned an invalid managed-run response.")
    build_number, push_stable = validate_managed_run(
        managed_run,
        managed_run_id,
        release_version,
        release_commit,
    )

    publish_runs = run_az_json(
        [
            "pipelines",
            "runs",
            "list",
            "--org",
            ORG,
            "--project",
            PROJECT,
            "--pipeline-ids",
            str(PUBLISH_PIPELINE_ID),
            "--top",
            "100",
            "--query",
            "[].{id:id,status:status,buildNumber:buildNumber}",
            "--only-show-errors",
            "-o",
            "json",
        ]
    )
    if not isinstance(publish_runs, list):
        raise RuntimeError("Azure CLI returned an invalid publish-runs response.")
    active_runs = find_active_publish_runs(publish_runs)
    if active_runs:
        summary = ", ".join(
            f"{run.get('id')} ({run.get('status')}, {run.get('buildNumber')})"
            for run in active_runs
        )
        raise RuntimeError(
            f"Publish pipeline {PUBLISH_PIPELINE_ID} already has active run(s): "
            f"{summary}. Do not queue a duplicate."
        )

    return build_queue_body(build_number, push_stable), build_number, push_stable


def main() -> None:
    parser = argparse.ArgumentParser(
        description=(
            "Validate a SkiaSharp managed run and write the Azure publish-pipeline "
            "request body. This script never queues or approves a run."
        )
    )
    parser.add_argument("--managed-run-id", type=int, required=True)
    parser.add_argument("--release-version", required=True)
    parser.add_argument("--release-commit", required=True)
    parser.add_argument("--output", type=Path, required=True)
    args = parser.parse_args()

    # Never leave a previously validated request available after a failed revalidation.
    args.output.unlink(missing_ok=True)

    body, build_number, push_stable = prepare_queue_request(
        args.managed_run_id,
        args.release_version,
        args.release_commit,
    )

    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(
        json.dumps(body, indent=2, sort_keys=True) + "\n",
        encoding="utf-8",
    )

    push_type = "Stable" if push_stable else "Preview"
    print(f"Managed run ID: {args.managed_run_id}")
    print(f"Managed build number: {build_number}")
    print(f"Release branch: release/{args.release_version}")
    print(f"Release commit: {args.release_commit}")
    print(f"Publish stage: Push {push_type}")
    print(f"Queue request: {args.output}")
    print("The numeric managed run ID was used only for validation.")
    print("Review this summary and obtain explicit user confirmation before queueing.")


if __name__ == "__main__":
    main()
