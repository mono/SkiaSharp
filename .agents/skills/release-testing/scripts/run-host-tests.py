#!/usr/bin/env python3
"""Run one approved non-mobile SkiaSharp release-test matrix item."""

import argparse
import os
from pathlib import Path
import shutil
import sys

import release_test_common as common


MAC2_XCODE_MAJOR = 26


def xcode_version(value: str) -> tuple[int, ...]:
    try:
        return tuple(int(part) for part in value.split("."))
    except ValueError as error:
        raise common.ReleaseTestError(
            f"invalid Xcode version reported by dotnet apple: {value}"
        ) from error


def select_mac2_xcode(xcodes: list[dict]) -> dict | None:
    matching = [
        xcode
        for xcode in xcodes
        if xcode_version(
            str(xcode.get("Version") or xcode.get("version") or "0")
        )[0]
        == MAC2_XCODE_MAJOR
    ]
    return (
        max(
            matching,
            key=lambda item: xcode_version(
                str(item.get("Version") or item.get("version"))
            ),
        )
        if matching
        else None
    )


def configure_mac2_xcode(root: Path) -> str | None:
    xcodes = common.run_json(
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
    )
    selected = select_mac2_xcode(xcodes)
    if selected is None:
        print(
            "No Xcode 26.x installation found for Mac2; "
            "using the default Xcode.",
            flush=True,
        )
        return None
    xcode_path = Path(str(selected.get("Path") or selected.get("path") or ""))
    developer_dir = xcode_path / "Contents" / "Developer"
    if not developer_dir.is_dir():
        raise common.ReleaseTestError(
            f"Xcode developer directory was not found: {developer_dir}"
        )
    os.environ["DEVELOPER_DIR"] = str(developer_dir)
    version = str(selected.get("Version") or selected.get("version"))
    print(
        f"Using Xcode {version} for Mac2: {developer_dir}",
        flush=True,
    )
    return str(developer_dir)


def execute(root: Path, args) -> None:
    if args.command == "smoke":
        common.run_test(root, "SmokeTests", args)
    elif args.command == "console":
        common.run_test(root, "ConsoleTests", args)
    elif args.command == "linux":
        docker = common.run_streaming(
            ["docker", "info", "--format", "{{.OSType}}"],
            cwd=root,
            capture=True,
        ).stdout.strip()
        if docker != "linux":
            raise common.ReleaseTestError("Docker Linux daemon is required")
        common.run_test(root, "LinuxConsoleTests", args)
    elif args.command == "blazor":
        common.require_workload(root, "wasm-tools")
        common.run_test(root, "BlazorTests", args)
    elif args.command == "maccatalyst":
        if sys.platform != "darwin":
            raise common.ReleaseTestError(
                "Mac Catalyst release tests require macOS"
            )
        # TODO(appium/appium-mac2-driver#410): Remove this override after
        # WebDriverAgentMac builds with Xcode 27.
        configure_mac2_xcode(root)
        common.require_workload(root, "maui")
        common.require_appium_driver(root, "mac2")
        common.run_test(root, "MauiMacCatalystTests", args)
    elif args.command == "windows":
        if sys.platform != "win32":
            raise common.ReleaseTestError(
                "MAUI Windows release tests require Windows"
            )
        common.require_workload(root, "maui")
        if not shutil.which("WinAppDriver.exe"):
            raise common.ReleaseTestError(
                "WinAppDriver.exe was not found on PATH"
            )
        common.run_test(root, "MauiWindowsTests", args)
    else:
        raise common.ReleaseTestError(
            f"unsupported host command: {args.command}"
        )


def create_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument(
        "command",
        choices=(
            "smoke",
            "console",
            "linux",
            "blazor",
            "maccatalyst",
            "windows",
        ),
    )
    common.add_package_arguments(parser)
    return parser


if __name__ == "__main__":
    sys.exit(common.execute_item(create_parser().parse_args(), execute))
