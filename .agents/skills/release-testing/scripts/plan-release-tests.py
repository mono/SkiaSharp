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
SCRIPT_DIR = Path(__file__).resolve().parent
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


def numeric_version(value: str) -> tuple[int, ...]:
    if not re.fullmatch(r"\d+(?:\.\d+)*", value):
        raise PlanError(f"invalid Apple version: {value}")
    return tuple(int(part) for part in value.split("."))


def xcode_target_majors(xcode_version: str) -> tuple[int, int]:
    major = numeric_version(xcode_version)[0]
    if major == 26:
        return 15, 26
    if major >= 27:
        return 18, 26
    raise PlanError(
        f"Xcode {xcode_version} has no release-test target policy"
    )


def simulator_runtime_version(simulator: dict) -> str | None:
    runtime = simulator.get("runtime") or {}
    version = str(runtime.get("version") or "")
    if not version:
        name = str(runtime.get("name") or "")
        version = name.removeprefix("iOS ") if name.startswith("iOS ") else ""
    try:
        numeric_version(version)
    except PlanError:
        return None
    return version


def available_ios_versions(simulators: list[dict]) -> list[str]:
    versions = {
        version
        for simulator in simulators
        if (version := simulator_runtime_version(simulator))
        and str((simulator.get("runtime") or {}).get("name") or "").startswith(
            "iOS "
        )
        and simulator.get("isAvailable", True)
        and (simulator.get("runtime") or {}).get("isAvailable", True)
    }
    return sorted(versions, key=numeric_version)


def ios_device_types(simulators: list[dict], version: str) -> set[str]:
    devices = {
        str((simulator.get("deviceType") or {}).get("name") or "")
        for simulator in simulators
        if simulator_runtime_version(simulator) == version
        and (simulator.get("deviceType") or {}).get("productFamily") == "iPhone"
        and simulator.get("isAvailable", True)
    }
    devices.discard("")
    return devices


def preferred_device_type(simulators: list[dict], version: str) -> str:
    devices = ios_device_types(simulators, version)
    if not devices:
        raise PlanError(
            f"iOS {version} has no available iPhone simulator device type"
        )

    def score(name: str) -> tuple:
        standard = re.fullmatch(r"iPhone\s+(\d+)", name)
        if standard:
            return 4, int(standard.group(1)), name
        compact = re.fullmatch(r"iPhone\s+(\d+)e", name)
        if compact:
            return 3, int(compact.group(1)), name
        numbered = re.search(r"iPhone\s+(\d+)", name)
        if numbered:
            return 2, int(numbered.group(1)), name
        return 1, 0, name

    return max(devices, key=score)


def select_apple_targets(
    xcode_version: str,
    xcode_path: str,
    simulators: list[dict],
) -> dict:
    minimum_major, maximum_major = xcode_target_majors(xcode_version)
    versions = available_ios_versions(simulators)

    def select(major: int, *, maximum: bool) -> str:
        matches = [
            version
            for version in versions
            if numeric_version(version)[0] == major
        ]
        if not matches:
            raise PlanError(
                f"Xcode {xcode_version} requires an installed iOS {major}.x "
                f"runtime for {'maximum' if maximum else 'minimum'} coverage"
            )
        return (
            max(matches, key=numeric_version)
            if maximum
            else min(matches, key=numeric_version)
        )

    minimum = select(minimum_major, maximum=False)
    maximum = select(maximum_major, maximum=True)
    return {
        "xcodeVersion": xcode_version,
        "minimum": {
            "version": minimum,
            "device": preferred_device_type(simulators, minimum),
        },
        "maximum": {
            "version": maximum,
            "device": preferred_device_type(simulators, maximum),
        },
        "availableVersions": versions,
        "developerDirectory": str(
            Path(xcode_path) / "Contents" / "Developer"
        ),
    }


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


def detect_apple_targets(root: Path) -> dict:
    xcodes = parse_json_output(
        run(
            [
                "dotnet",
                "tool",
                "run",
                "apple",
                "--",
                "xcode",
                "list",
                "--format",
                "json",
            ],
            cwd=root,
        ).stdout
    )
    selected = next(
        (
            item
            for item in xcodes
            if item.get("Selected") is True
            or item.get("selected") is True
        ),
        None,
    )
    if not selected:
        raise PlanError("dotnet apple did not report a selected Xcode")
    xcode_version = str(
        selected.get("Version") or selected.get("version") or ""
    )
    xcode_path = str(selected.get("Path") or selected.get("path") or "")
    if not xcode_version or not xcode_path:
        raise PlanError("selected Xcode is missing its version or path")
    simulators = parse_json_output(
        run(
            [
                "dotnet",
                "tool",
                "run",
                "apple",
                "--",
                "simulator",
                "list",
                "--available",
                "--format",
                "json",
            ],
            cwd=root,
        ).stdout
    )
    return select_apple_targets(xcode_version, xcode_path, simulators)


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
        apple_targets = None
        apple_error = None
        if host_os == "macOS":
            try:
                apple_targets = detect_apple_targets(root)
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
