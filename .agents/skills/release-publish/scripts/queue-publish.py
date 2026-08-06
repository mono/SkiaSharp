#!/usr/bin/env python3
"""Queue the SkiaSharp NuGet.org publish pipeline for a managed build number."""

import argparse
import json
import re
import shutil
import subprocess
import tempfile
from pathlib import Path


ORG = "https://dev.azure.com/devdiv"
PROJECT = "DevDiv"
PIPELINE_ID = 25298
RUN_URL = "https://dev.azure.com/devdiv/DevDiv/_build/results?buildId={run_id}"
VERSION = r"\d+\.\d+\.\d+(?:\.\d+)?"
STABLE_BUILD = re.compile(rf"^(?P<base>{VERSION})-stable\.\d+\+(?P=base)$")
PRERELEASE_BUILD = re.compile(
    rf"^(?P<release>{VERSION}-(?:preview|rc)\.\d+)\.\d+\+(?P=release)$"
)


def classify_build_number(build_number: str) -> bool:
    if STABLE_BUILD.fullmatch(build_number):
        return True
    if PRERELEASE_BUILD.fullmatch(build_number):
        return False
    if build_number.isdigit():
        raise ValueError("Expected a build number, not a numeric run/build ID.")
    raise ValueError(
        "Managed build number must match X.Y.Z[.F]-stable.B+X.Y.Z[.F], "
        "X.Y.Z[.F]-preview.N.B+X.Y.Z[.F]-preview.N, or the equivalent RC form."
    )


def build_queue_body(build_number: str) -> dict:
    push_stable = classify_build_number(build_number)
    return {
        "resources": {"pipelines": {"SkiaSharp": {"version": build_number}}},
        "templateParameters": {
            "selectedResource": "SkiaSharp",
            "pushPackages": True,
            "pushStable": push_stable,
        },
    }


def queue_publish(build_number: str) -> tuple[int, str]:
    az = shutil.which("az")
    if not az:
        raise FileNotFoundError("Required CLI 'az' was not found on PATH.")

    body = build_queue_body(build_number)
    with tempfile.TemporaryDirectory(prefix="skiasharp-publish-") as directory:
        request = Path(directory) / "request.json"
        request.write_text(json.dumps(body), encoding="utf-8")
        try:
            result = subprocess.run(
                [
                    az, "devops", "invoke", "--org", ORG,
                    "--area", "pipelines", "--resource", "runs",
                    "--route-parameters", f"project={PROJECT}",
                    f"pipelineId={PIPELINE_ID}",
                    "--http-method", "POST", "--api-version", "7.1",
                    "--in-file", str(request), "--encoding", "utf-8",
                    "--only-show-errors", "--query", "id", "-o", "tsv",
                ],
                capture_output=True,
                text=True,
                timeout=60,
                check=True,
            )
        except subprocess.CalledProcessError as error:
            detail = (error.stderr or "").strip() or "no error output"
            raise RuntimeError(f"Azure CLI failed: {detail}") from error

    output = result.stdout.strip()
    if not output.isdigit():
        raise RuntimeError("Azure CLI did not return a numeric publish run ID.")
    run_id = int(output)
    return run_id, RUN_URL.format(run_id=run_id)


def main() -> None:
    parser = argparse.ArgumentParser(
        description="Queue publish pipeline 25298 for a verified managed build number."
    )
    parser.add_argument("managed_build_number")
    args = parser.parse_args()

    run_id, url = queue_publish(args.managed_build_number)
    print(f"Publish run ID: {run_id}")
    print(f"Publish run URL: {url}")


if __name__ == "__main__":
    main()
