#!/usr/bin/env python3
"""Queue the SkiaSharp NuGet.org publish pipeline for a managed build number."""

import argparse
import json
import locale
import os
import re
import shutil
import subprocess
import tempfile
from pathlib import Path
from typing import Any


ORG = "https://dev.azure.com/devdiv"
PROJECT = "DevDiv"
PUBLISH_PIPELINE_ID = 25298
PUBLISH_RUN_URL = "https://dev.azure.com/devdiv/DevDiv/_build/results?buildId={run_id}"
VERSION = r"\d+\.\d+\.\d+(?:\.\d+)?"
STABLE_BUILD = re.compile(rf"^(?P<base>{VERSION})-stable\.\d+\+(?P=base)$")
PRERELEASE_BUILD = re.compile(
    rf"^(?P<release>{VERSION}-(?:preview|rc)\.\d+)\.\d+\+(?P=release)$"
)


def cli_command(executable: str, args: list[str]) -> list[str]:
    resolved = shutil.which(executable)
    if not resolved:
        raise FileNotFoundError(f"Required CLI '{executable}' was not found on PATH.")
    return [resolved, *args]


def decode_output(value: bytes | str | None) -> str:
    if value is None or isinstance(value, str):
        return value or ""
    encodings = ["utf-8-sig", locale.getpreferredencoding(False)]
    if os.name == "nt":
        encodings.append("mbcs")
    for encoding in dict.fromkeys(encodings):
        try:
            return value.decode(encoding, errors="strict")
        except (LookupError, UnicodeDecodeError):
            pass
    raise RuntimeError(f"Could not decode Azure CLI output using {', '.join(encodings)}.")


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
        detail = decode_output(error.stderr).strip()
        raise RuntimeError(
            f"Azure CLI failed with exit code {error.returncode}: "
            f"{detail or 'no error output'}"
        ) from error

    output = decode_output(result.stdout).strip()
    if not output:
        detail = decode_output(result.stderr).strip()
        raise RuntimeError(
            f"Azure CLI returned no output: {detail or 'no error output'}"
        )
    try:
        return json.loads(output)
    except json.JSONDecodeError as error:
        raise RuntimeError(f"Azure CLI returned malformed JSON: {error}") from error


def classify_build_number(build_number: str) -> bool:
    if STABLE_BUILD.fullmatch(build_number):
        return True
    if PRERELEASE_BUILD.fullmatch(build_number):
        return False
    if build_number.isdigit():
        raise ValueError(
            "Expected the managed build number string, not a numeric run/build ID."
        )
    raise ValueError(
        "Managed build number must match X.Y.Z[.F]-stable.B+X.Y.Z[.F], "
        "X.Y.Z[.F]-preview.N.B+X.Y.Z[.F]-preview.N, or the equivalent RC form."
    )


def build_queue_body(build_number: str) -> dict[str, Any]:
    push_stable = classify_build_number(build_number)
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


def queue_publish(build_number: str) -> tuple[int, str]:
    body = build_queue_body(build_number)
    with tempfile.TemporaryDirectory(prefix="skiasharp-publish-") as directory:
        request = Path(directory) / "request.json"
        request.write_text(json.dumps(body), encoding="utf-8")
        response = run_az_json(
            [
                "devops", "invoke", "--org", ORG,
                "--area", "pipelines", "--resource", "runs",
                "--route-parameters",
                f"project={PROJECT}",
                f"pipelineId={PUBLISH_PIPELINE_ID}",
                "--http-method", "POST", "--api-version", "7.1",
                "--in-file", str(request), "--encoding", "utf-8",
                "--only-show-errors", "-o", "json",
            ]
        )

    if not isinstance(response, dict):
        raise RuntimeError("Azure CLI returned an invalid queue response.")
    run_id = response.get("id")
    if not isinstance(run_id, int) or isinstance(run_id, bool):
        raise RuntimeError("Azure did not return a numeric publish run ID.")
    return run_id, PUBLISH_RUN_URL.format(run_id=run_id)


def main() -> None:
    parser = argparse.ArgumentParser(
        description=(
            "Queue publish pipeline 25298 using a verified managed build number. "
            "Source and duplicate-run verification happen before this command."
        )
    )
    parser.add_argument("managed_build_number")
    parser.add_argument(
        "--confirm-queue",
        action="store_true",
        help="Required after the user explicitly confirms the publish queue.",
    )
    args = parser.parse_args()
    if not args.confirm_queue:
        parser.error("--confirm-queue is required after explicit user confirmation.")

    push_stable = classify_build_number(args.managed_build_number)
    run_id, url = queue_publish(args.managed_build_number)
    print(f"Managed build number: {args.managed_build_number}")
    print(f"Publish stage: Push {'Stable' if push_stable else 'Preview'}")
    print(f"Publish run ID: {run_id}")
    print(f"Publish run URL: {url}")


if __name__ == "__main__":
    main()
