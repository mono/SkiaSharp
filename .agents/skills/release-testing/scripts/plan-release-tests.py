#!/usr/bin/env python3
"""Plan the default SkiaSharp release integration-test matrix."""

from __future__ import annotations

import argparse
import json
from pathlib import Path
import platform
import re
import shlex
import subprocess
import sys


SCRIPT_DIR = Path(__file__).resolve().parent
PREPARE_SCRIPT = SCRIPT_DIR / "prepare-test-run.py"
STATUS_SCRIPT = (
    SCRIPT_DIR.parent.parent
    / "release-status"
    / "scripts"
    / "pipeline-status.py"
)


class PlanError(RuntimeError):
    """The test matrix could not be planned safely."""


def run(
    args: list[str],
    *,
    cwd: Path | None = None,
    timeout: int = 180,
) -> subprocess.CompletedProcess[str]:
    try:
        result = subprocess.run(
            args,
            cwd=cwd,
            capture_output=True,
            text=True,
            timeout=timeout,
        )
    except FileNotFoundError as error:
        raise PlanError(f"{args[0]} was not found on PATH") from error
    except subprocess.TimeoutExpired as error:
        raise PlanError(
            f"command timed out after {timeout}s: {' '.join(args)}"
        ) from error
    if result.returncode != 0:
        detail = result.stderr.strip() or result.stdout.strip() or "no output"
        raise PlanError(
            f"command failed ({result.returncode}): {' '.join(args)}\n{detail}"
        )
    return result


def parse_json_output(text: str):
    decoder = json.JSONDecoder()
    for index, character in enumerate(text):
        if character not in "[{":
            continue
        try:
            value, _ = decoder.raw_decode(text[index:])
            return value
        except json.JSONDecodeError:
            pass
    raise PlanError("command returned no valid JSON")


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
        run(
            [sys.executable, str(STATUS_SCRIPT), target],
            cwd=root,
        ).stdout
    )


def apple_targets_report(root: Path) -> dict:
    return parse_json_output(
        run(
            [
                sys.executable,
                str(PREPARE_SCRIPT),
                "--detect-apple-targets",
            ],
            cwd=root,
        ).stdout
    )


def runner_command(
    runner_args: list[str],
    *,
    skia_version: str,
    harfbuzz_version: str,
) -> str:
    return format_command(
        [
            sys.executable,
            ".agents/skills/release-testing/scripts/run-tests.py",
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
    *,
    apple_targets: dict | None = None,
    apple_error: str | None = None,
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
        runner_args: list[str],
        minutes: int,
        visual: bool = False,
    ) -> None:
        matrix.append(
            matrix_item(
                item_id=item_id,
                label=label,
                target=target,
                runner_args=runner_args,
                skia_version=skia,
                harfbuzz_version=harfbuzz,
                estimated_minutes=minutes,
                visual=visual,
            )
        )

    add("smoke", "Native loading smoke tests", ".NET", ["smoke"], 1)
    add("console", "Console application tests", ".NET", ["console"], 1)
    add("linux", "Linux container tests", "Docker Linux", ["linux"], 2)
    add(
        "blazor",
        "Blazor WebAssembly rendering tests",
        "Chromium",
        ["blazor"],
        3,
        True,
    )
    add(
        "android-26",
        "MAUI Android minimum",
        "Android 26",
        ["android-26"],
        5,
        True,
    )
    add(
        "android-37.1",
        "MAUI Android maximum",
        "Android 37.1",
        ["android-37.1"],
        5,
        True,
    )

    if host_os == "macOS":
        add(
            "maccatalyst",
            "MAUI Mac Catalyst rendering tests",
            "Current macOS",
            ["maccatalyst"],
            3,
            True,
        )
        if apple_targets:
            for coverage in ("minimum", "maximum"):
                target = apple_targets[coverage]
                version = target["version"]
                device = target["device"]
                item_id = f"ios-{version}"
                add(
                    item_id,
                    f"MAUI iOS {coverage}",
                    f"iOS {version} / {device}",
                    [item_id, "--device", device],
                    4,
                    True,
                )
        else:
            missing.append(
                "iOS simulator coverage could not be resolved"
                + (f": {apple_error}" if apple_error else "")
            )
    else:
        missing.append("iOS and Mac Catalyst require a macOS host")

    if host_os == "Windows":
        add(
            "windows",
            "MAUI Windows rendering tests",
            "Windows",
            ["windows"],
            4,
            True,
        )
    else:
        missing.append("MAUI Windows requires a Windows host")

    return matrix, missing


def preparation_command(apple_targets: dict | None) -> str:
    command = [
        sys.executable,
        ".agents/skills/release-testing/scripts/prepare-test-run.py",
    ]
    if apple_targets:
        command.extend(
            [
                "--expect-xcode",
                apple_targets["xcodeVersion"],
                "--expect-ios-min",
                apple_targets["minimum"]["version"],
                "--expect-ios-min-device",
                apple_targets["minimum"]["device"],
                "--expect-ios-max",
                apple_targets["maximum"]["version"],
                "--expect-ios-max-device",
                apple_targets["maximum"]["device"],
            ]
        )
    return format_command(command)


def release_summary(status: dict, *, status_override: bool) -> dict:
    managed = status.get("managedRun") or {}
    tests = status.get("testsRun") or {}
    versions = status.get("packageVersions") or {}
    return {
        "branch": status.get("branch"),
        "commit": status.get("commit"),
        "state": status.get("state"),
        "nextAction": status.get("nextAction"),
        "statusOverride": status_override,
        "warnings": status.get("warnings") or [],
        "managedRunId": managed.get("runId"),
        "testsRunId": tests.get("runId"),
        "buildNumber": managed.get("buildNumber"),
        "sourceBranch": managed.get("sourceBranch"),
        "sourceVersion": managed.get("sourceVersion"),
        "managedRunUrl": managed.get("url"),
        "testsRunUrl": tests.get("url"),
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
    managed_state = (status.get("managedRun") or {}).get("state")
    feed_state = (status.get("packageFeed") or {}).get("state")
    if managed_state not in ("succeeded", "warning"):
        raise PlanError(
            "Cannot override an incomplete/failed managed package build"
        )
    if feed_state != "ready":
        raise PlanError("Cannot plan tests until both packages are available")
    return True, not ready


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("release_branch_or_commit")
    parser.add_argument("--allow-incomplete-ci", action="store_true")
    args = parser.parse_args()

    try:
        root = Path(
            run(["git", "rev-parse", "--show-toplevel"]).stdout.strip()
        )
        status = status_report(root, args.release_branch_or_commit)
        eligible, status_override = plan_eligibility(
            status,
            allow_incomplete_ci=args.allow_incomplete_ci,
        )
        if not eligible:
            print(
                json.dumps(
                    {
                        "schemaVersion": 6,
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
        apple_targets = None
        apple_error = None
        if host_os == "macOS":
            try:
                apple_targets = apple_targets_report(root)
            except PlanError as error:
                apple_error = str(error)
        matrix, missing = build_matrix(
            status,
            host_os,
            apple_targets=apple_targets,
            apple_error=apple_error,
        )
        print(
            json.dumps(
                {
                    "schemaVersion": 6,
                    "release": release_summary(
                        status,
                        status_override=status_override,
                    ),
                    "readyToPlan": True,
                    "nextAction": "approve-test-matrix",
                    "host": {
                        "os": host_os,
                        "architecture": platform.machine().lower(),
                        "apple": apple_targets,
                    },
                    "matrix": matrix,
                    "defaultSelection": [
                        item["id"] for item in matrix
                    ],
                    "missingCoverage": missing,
                    "preparationCommand": preparation_command(apple_targets),
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
