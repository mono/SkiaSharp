#!/usr/bin/env python3
"""Plan the default SkiaSharp release integration-test matrix."""

from __future__ import annotations

import argparse
import json
from pathlib import Path
import platform
import re
import shlex
import sys
import tempfile

import release_test_common as common

SCRIPT_DIR = Path(__file__).resolve().parent
RELEASE_PROJECT = (
    SCRIPT_DIR.parents[3]
    / "utils"
    / "SkiaSharp.ReleaseTool"
    / "SkiaSharp.ReleaseTool.csproj"
)


PlanError = common.ReleaseTestError


parse_json_output = common.parse_json_output


def format_command(
    argv: list[str],
    *,
    platform_name: str | None = None,
) -> str:
    platform_name = platform_name or sys.platform
    if platform_name != "win32":
        return shlex.join(argv)

    def quote(argument: str) -> str:
        if re.fullmatch(r"[A-Za-z0-9_./:\\=+-]+", argument):
            return argument
        return "'" + argument.replace("'", "''") + "'"

    formatted = " ".join(quote(argument) for argument in argv)
    return f"& {formatted}" if formatted.startswith("'") else formatted


def receipt_report(root: Path, version: str) -> dict:
    with tempfile.TemporaryDirectory(prefix="skiasharp-release-smoke-") as temp:
        output = Path(temp) / "finish-plan.json"
        common.run_checked(
            [
                "dotnet",
                "restore",
                str(RELEASE_PROJECT),
                "--locked-mode",
            ],
            cwd=root,
            timeout=600,
        )
        common.run_checked(
            [
                "dotnet",
                "build",
                str(RELEASE_PROJECT),
                "--configuration",
                "Release",
                "--no-restore",
            ],
            cwd=root,
            timeout=600,
        )
        common.run_checked(
            [
                "dotnet",
                "run",
                "--no-build",
                "--no-restore",
                "--configuration",
                "Release",
                "--project",
                str(RELEASE_PROJECT),
                "--",
                "finish",
                "plan",
                "--version",
                version,
                "--output",
                str(output),
            ],
            cwd=root,
            timeout=600,
        )
        return json.loads(output.read_text(encoding="utf-8"))


def runner_command(
    runner_script: str,
    runner_args: list[str],
    *,
    skia_version: str,
    harfbuzz_version: str,
) -> str:
    return format_command(
        [
            sys.executable,
            f".agents/skills/release-testing/scripts/{runner_script}",
            *runner_args,
            "--skiasharp",
            skia_version,
            "--harfbuzzsharp",
            harfbuzz_version,
        ]
    )


def matrix_item(
    *,
    item_id: str,
    label: str,
    target: str,
    runner_script: str,
    runner_args: list[str],
    skia_version: str,
    harfbuzz_version: str,
    estimated_minutes: int,
    visual: bool = False,
) -> dict:
    item = {
        "id": item_id,
        "label": label,
        "target": target,
        "estimatedMinutes": estimated_minutes,
        "command": runner_command(
            runner_script,
            runner_args,
            skia_version=skia_version,
            harfbuzz_version=harfbuzz_version,
        ),
    }
    if visual:
        item["expectedArtifacts"] = [
            "output/logs/testlogs/integration/*.png"
        ]
    return item


def build_matrix(
    skia_version: str,
    harfbuzz_version: str,
    host_os: str,
) -> tuple[list[dict], list[str]]:
    matrix: list[dict] = []
    missing: list[str] = []

    def add(
        item_id: str,
        label: str,
        target: str,
        runner_script: str,
        runner_args: list[str],
        minutes: int,
        visual: bool = False,
    ) -> None:
        matrix.append(
            matrix_item(
                item_id=item_id,
                label=label,
                target=target,
                runner_script=runner_script,
                runner_args=runner_args,
                skia_version=skia_version,
                harfbuzz_version=harfbuzz_version,
                estimated_minutes=minutes,
                visual=visual,
            )
        )

    add(
        "smoke",
        "Native loading smoke tests",
        ".NET",
        "run-host-tests.py",
        ["smoke"],
        1,
    )
    add(
        "console",
        "Console application tests",
        ".NET",
        "run-host-tests.py",
        ["console"],
        1,
    )
    add(
        "linux",
        "Linux container tests",
        "Docker Linux",
        "run-host-tests.py",
        ["linux"],
        2,
    )
    add(
        "blazor",
        "Blazor WebAssembly rendering tests",
        "Chromium",
        "run-host-tests.py",
        ["blazor"],
        3,
        True,
    )
    add(
        f"android-{common.ANDROID_MIN_VERSION}",
        "MAUI Android minimum",
        f"Android {common.ANDROID_MIN_VERSION}",
        "run-android-tests.py",
        [common.ANDROID_MIN_VERSION],
        5,
        True,
    )
    add(
        f"android-{common.ANDROID_MAX_VERSION}",
        "MAUI Android maximum",
        f"Android {common.ANDROID_MAX_VERSION}",
        "run-android-tests.py",
        [common.ANDROID_MAX_VERSION],
        5,
        True,
    )

    if host_os == "macOS":
        add(
            "maccatalyst",
            "MAUI Mac Catalyst rendering tests",
            "Current macOS",
            "run-host-tests.py",
            ["maccatalyst"],
            3,
            True,
        )
        add(
            f"ios-{common.IOS_MIN_VERSION}",
            "MAUI iOS minimum test target",
            f"iOS {common.IOS_MIN_VERSION}",
            "run-ios-tests.py",
            [common.IOS_MIN_VERSION],
            4,
            True,
        )
        add(
            f"ios-{common.IOS_MAX_VERSION}",
            "MAUI iOS maximum test target",
            f"iOS {common.IOS_MAX_VERSION}",
            "run-ios-tests.py",
            [common.IOS_MAX_VERSION],
            4,
            True,
        )
    else:
        missing.append("iOS and Mac Catalyst require a macOS host")

    if host_os == "Windows":
        add(
            "windows",
            "MAUI Windows rendering tests",
            "Windows",
            "run-host-tests.py",
            ["windows"],
            4,
            True,
        )
    else:
        missing.append("MAUI Windows requires a Windows host")

    return matrix, missing


def release_summary(plan: dict) -> dict:
    receipt = plan["receipt"]
    release = plan["release"]
    return {
        "branch": release["branch"],
        "commit": receipt["sourceCommit"],
        "state": "public",
        "warnings": plan.get("warnings") or [],
        "publicPackages": {
            "SkiaSharp": receipt["skiaSharpVersion"],
            "HarfBuzzSharp": receipt["harfBuzzSharpVersion"],
        },
        "verifiedPackageCount": len(receipt["packages"]),
    }


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("release_version")
    args = parser.parse_args()

    try:
        root = common.repository_root()
        plan = receipt_report(root, args.release_version)

        host_os = (
            "macOS"
            if sys.platform == "darwin"
            else "Windows"
            if sys.platform == "win32"
            else "Linux"
        )
        receipt = plan["receipt"]
        matrix, missing = build_matrix(
            receipt["skiaSharpVersion"],
            receipt["harfBuzzSharpVersion"],
            host_os,
        )
        print(
            json.dumps(
                {
                    "schemaVersion": 6,
                    "release": release_summary(plan),
                    "readyToPlan": True,
                    "nextAction": "approve-test-matrix",
                    "host": {
                        "os": host_os,
                        "architecture": platform.machine().lower(),
                    },
                    "matrix": matrix,
                    "defaultSelection": [
                        item["id"] for item in matrix
                    ],
                    "missingCoverage": missing,
                    "source": "NuGet.org",
                },
                indent=2,
            )
        )
    except PlanError as error:
        print(f"ERROR: {error}", file=sys.stderr)
        return 1
    return 0


if __name__ == "__main__":
    sys.exit(main())
