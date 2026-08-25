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

import release_test_common as common

SCRIPT_DIR = Path(__file__).resolve().parent
STATUS_SCRIPT = (
    SCRIPT_DIR.parent.parent
    / "release-status"
    / "scripts"
    / "pipeline-status.py"
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


def status_report(root: Path, target: str) -> dict:
    return parse_json_output(
        common.run_checked(
            [sys.executable, str(STATUS_SCRIPT), target],
            cwd=root,
            timeout=180,
        ).stdout
    )


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
    status: dict,
    host_os: str,
) -> tuple[list[dict], list[str]]:
    versions = status["packageVersions"]["test"]
    skia = versions["SkiaSharp"]
    harfbuzz = versions["HarfBuzzSharp"]
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
                skia_version=skia,
                harfbuzz_version=harfbuzz,
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


def release_summary(status: dict, *, status_override: bool) -> dict:
    build = status.get("buildRun") or {}
    tests = status.get("testsRun") or {}
    bar = status.get("barBuild") or {}
    versions = status.get("packageVersions") or {}
    return {
        "branch": status.get("branch"),
        "commit": status.get("commit"),
        "state": status.get("state"),
        "nextAction": status.get("nextAction"),
        "statusOverride": status_override,
        "warnings": status.get("warnings") or [],
        "migration": status.get("migration"),
        "buildRunId": build.get("runId"),
        "testsRunId": tests.get("runId"),
        "barBuildId": bar.get("id"),
        "defaultChannelIds": bar.get("defaultChannelIds"),
        "buildNumber": build.get("buildNumber"),
        "sourceBranch": build.get("sourceBranch"),
        "sourceVersion": build.get("sourceVersion"),
        "buildRunUrl": build.get("url"),
        "testsRunUrl": tests.get("url"),
        "barAssets": bar.get("assets"),
        "testPackages": versions.get("test"),
        "publicPackages": versions.get("public"),
    }


def plan_eligibility(
    status: dict,
    *,
    allow_incomplete_ci: bool,
) -> tuple[bool, bool]:
    next_action = status.get("nextAction")
    ready = next_action == "start-release-testing"
    if not ready and not allow_incomplete_ci:
        return False, False
    if not ready and next_action not in {
        "wait-for-tests-trigger",
        "wait-for-tests",
    }:
        raise PlanError(
            "--allow-incomplete-ci may override only the tests wait"
        )
    build_state = (status.get("buildRun") or {}).get("state")
    bar_state = (status.get("barBuild") or {}).get("state")
    if build_state not in ("succeeded", "warning"):
        raise PlanError(
            "Cannot override an incomplete/failed combined Build"
        )
    if bar_state != "ready":
        raise PlanError(
            "Cannot plan tests until the exact BAR assets have locations"
        )
    return True, not ready


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("release_branch_or_commit")
    parser.add_argument("--allow-incomplete-ci", action="store_true")
    args = parser.parse_args()

    try:
        root = common.repository_root()
        status = status_report(root, args.release_branch_or_commit)
        eligible, status_override = plan_eligibility(
            status,
            allow_incomplete_ci=args.allow_incomplete_ci,
        )
        if not eligible:
            print(
                json.dumps(
                    {
                        "schemaVersion": 5,
                        "release": release_summary(
                            status,
                            status_override=False,
                        ),
                        "readyToPlan": False,
                        "nextAction": status.get("nextAction"),
                        "matrix": [],
                        "overrideFlag": "--allow-incomplete-ci",
                    },
                    indent=2,
                )
            )
            return 0

        host_os = (
            "macOS"
            if sys.platform == "darwin"
            else "Windows"
            if sys.platform == "win32"
            else "Linux"
        )
        matrix, missing = build_matrix(status, host_os)
        print(
            json.dumps(
                {
                    "schemaVersion": 5,
                    "release": release_summary(
                        status,
                        status_override=status_override,
                    ),
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
