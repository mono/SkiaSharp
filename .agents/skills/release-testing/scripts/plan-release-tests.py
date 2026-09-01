#!/usr/bin/env python3
"""Plan the SkiaSharp BAR-package release-approval test matrix."""

from __future__ import annotations
import argparse
import json
import platform
import re
import shlex
import sys
import release_test_common as common
import release_test_darc as darc
import release_test_nuget as nuget

PlanError = common.ReleaseTestError


def format_command(argv: list[str], *, platform_name: str | None = None) -> str:
    platform_name = platform_name or sys.platform
    if platform_name != "win32":
        return shlex.join(argv)

    def quote(argument: str) -> str:
        if re.fullmatch(r"[A-Za-z0-9_./:\\=+-]+", argument):
            return argument
        return "'" + argument.replace("'", "''") + "'"

    formatted = " ".join(quote(argument) for argument in argv)
    return f"& {formatted}" if formatted.startswith("'") else formatted


def package_sources(receipt: dict) -> dict:
    return {"barLocation": receipt["packageFeed"], "guidFeed": receipt["resolvedPackageSource"]}


def receipt_report(version: str, *, bar_id: int | None = None, max_age: int = 30) -> dict:
    source = darc.resolve_build(version, bar_id=bar_id, max_age=max_age)
    flat_container = nuget.resolve_flat_container(source.package_feed)
    resolved_package_source = nuget.service_index_from_flat_container(flat_container)
    skia = nuget.read_package("SkiaSharp", version, flat_container)
    bridge = nuget.read_package("SkiaSharp.HarfBuzz", version, flat_container)
    if len(bridge.harfbuzz_versions) != 1 or not nuget.is_concrete_version(bridge.harfbuzz_versions[0]):
        raise PlanError(f"SkiaSharp.HarfBuzz {version} does not pin one concrete HarfBuzzSharp dependency")
    harfbuzz_version = bridge.harfbuzz_versions[0]
    harfbuzz = nuget.read_package("HarfBuzzSharp", harfbuzz_version, flat_container)
    packages = [skia, bridge, harfbuzz]
    if len({item.branch for item in packages}) != 1 or len({item.commit for item in packages}) != 1:
        raise PlanError("CI package source metadata does not match")
    if skia.branch.removeprefix("refs/heads/") != source.branch or skia.commit != source.commit:
        raise PlanError("BAR build and package source metadata do not match")
    return {
        "barBuildId": source.id,
        "buildNumber": source.build_number,
        "buildLink": source.build_link,
        "sourceBranch": source.branch,
        "sourceCommit": source.commit,
        "packageFeed": source.package_feed,
        "resolvedPackageSource": resolved_package_source,
        "skiaSharpVersion": version,
        "harfBuzzSharpVersion": harfbuzz_version,
    }


def runner_command(runner_script: str, runner_args: list[str], *, skia_version: str, harfbuzz_version: str, package_source: str) -> str:
    return format_command(
        [
            sys.executable,
            f".agents/skills/release-testing/scripts/{runner_script}",
            *runner_args,
            "--skiasharp",
            skia_version,
            "--harfbuzzsharp",
            harfbuzz_version,
            "--package-source",
            package_source,
        ]
    )


def build_matrix(skia_version: str, harfbuzz_version: str, package_source: str, host_os: str) -> tuple[list[dict], list[str]]:
    matrix: list[dict] = []
    missing: list[str] = []

    def add(item_id: str, label: str, target: str, runner_script: str, runner_args: list[str], minutes: int) -> None:
        matrix.append(
            {
                "id": item_id,
                "label": label,
                "target": target,
                "estimatedMinutes": minutes,
                "command": runner_command(
                    runner_script, runner_args, skia_version=skia_version, harfbuzz_version=harfbuzz_version, package_source=package_source
                ),
            }
        )

    add("smoke", "Native loading smoke tests", ".NET", "run-host-tests.py", ["smoke"], 1)
    add("console", "Console application tests", ".NET", "run-host-tests.py", ["console"], 1)
    add("linux", "Linux container tests", "Docker Linux", "run-host-tests.py", ["linux"], 2)
    add("blazor", "Blazor WebAssembly rendering tests", "Chromium", "run-host-tests.py", ["blazor"], 3)
    add(
        f"android-{common.ANDROID_MIN_VERSION}",
        "MAUI Android minimum",
        f"Android {common.ANDROID_MIN_VERSION}",
        "run-android-tests.py",
        [common.ANDROID_MIN_VERSION],
        5,
    )
    add(
        f"android-{common.ANDROID_MAX_VERSION}",
        "MAUI Android maximum",
        f"Android {common.ANDROID_MAX_VERSION}",
        "run-android-tests.py",
        [common.ANDROID_MAX_VERSION],
        5,
    )

    if host_os == "macOS":
        add("maccatalyst", "MAUI Mac Catalyst rendering tests", "Current macOS", "run-host-tests.py", ["maccatalyst"], 3)
        add(f"ios-{common.IOS_MIN_VERSION}", "MAUI iOS minimum test target", f"iOS {common.IOS_MIN_VERSION}", "run-ios-tests.py", [common.IOS_MIN_VERSION], 4)
        add(f"ios-{common.IOS_MAX_VERSION}", "MAUI iOS maximum test target", f"iOS {common.IOS_MAX_VERSION}", "run-ios-tests.py", [common.IOS_MAX_VERSION], 4)
    else:
        missing.append("iOS and Mac Catalyst require a macOS host")

    if host_os == "Windows":
        add("windows", "MAUI Windows rendering tests", "Windows", "run-host-tests.py", ["windows"], 4)
    else:
        missing.append("MAUI Windows requires a Windows host")

    return matrix, missing


def release_summary(receipt: dict) -> dict:
    return {
        "branch": receipt["sourceBranch"],
        "commit": receipt["sourceCommit"],
        "barBuildId": receipt["barBuildId"],
        "buildNumber": receipt["buildNumber"],
        "buildLink": receipt["buildLink"],
        "ciPackages": {"SkiaSharp": receipt["skiaSharpVersion"], "HarfBuzzSharp": receipt["harfBuzzSharpVersion"]},
    }


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("package_version")
    parser.add_argument("--bar-id", type=int)
    parser.add_argument("--max-age", type=int, default=30)
    args = parser.parse_args()

    try:
        receipt = receipt_report(args.package_version, bar_id=args.bar_id, max_age=args.max_age)

        host_os = "macOS" if sys.platform == "darwin" else "Windows" if sys.platform == "win32" else "Linux"
        matrix, missing = build_matrix(receipt["skiaSharpVersion"], receipt["harfBuzzSharpVersion"], receipt["resolvedPackageSource"], host_os)
        print(
            json.dumps(
                {
                    "release": release_summary(receipt),
                    "host": {"os": host_os, "architecture": platform.machine().lower()},
                    "matrix": matrix,
                    "missingCoverage": missing,
                    "packageSources": package_sources(receipt),
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
