#!/usr/bin/env python3
"""Run one approved non-mobile SkiaSharp release-test matrix item."""

import argparse
from pathlib import Path
import shutil
import sys
import release_test_common as common


def execute(root: Path, args) -> None:
    if args.command == "smoke":
        common.run_test(root, "SmokeTests", args)
    elif args.command == "console":
        common.run_test(root, "ConsoleTests", args)
    elif args.command == "linux":
        docker = common.run_streaming(["docker", "info", "--format", "{{.OSType}}"], cwd=root, capture=True).stdout.strip()
        if docker != "linux":
            raise common.ReleaseTestError("Docker Linux daemon is required")
        common.run_test(root, "LinuxConsoleTests", args)
    elif args.command == "blazor":
        common.require_workload(root, "wasm-tools")
        common.run_test(root, "BlazorTests", args)
    elif args.command == "maccatalyst":
        if sys.platform != "darwin":
            raise common.ReleaseTestError("Mac Catalyst release tests require macOS")
        common.require_workload(root, "maui")
        common.require_appium_driver(root, "mac2")
        common.run_test(root, "MauiMacCatalystTests", args)
    elif args.command == "windows":
        if sys.platform != "win32":
            raise common.ReleaseTestError("MAUI Windows release tests require Windows")
        common.require_workload(root, "maui")
        if not shutil.which("WinAppDriver.exe"):
            raise common.ReleaseTestError("WinAppDriver.exe was not found on PATH")
        common.run_test(root, "MauiWindowsTests", args)
    else:
        raise common.ReleaseTestError(f"unsupported host command: {args.command}")


def create_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("command", choices=("smoke", "console", "linux", "blazor", "maccatalyst", "windows"))
    common.add_package_arguments(parser)
    return parser


if __name__ == "__main__":
    sys.exit(common.execute_item(create_parser().parse_args(), execute))
