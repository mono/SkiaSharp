#!/usr/bin/env python3
"""Run one approved iOS SkiaSharp release-test matrix item."""

import argparse
from pathlib import Path
import re
import sys
import uuid
import release_test_common as common


def apple_simulators(root: Path) -> list[dict]:
    return common.run_json(["dotnet", "tool", "run", "apple", "--", "simulator", "list", "--available", "--format", "json"], cwd=root)


def ios_simulator_version(simulator: dict) -> str:
    runtime = simulator.get("runtime") or {}
    version = str(runtime.get("version") or "")
    if version:
        return version
    name = str(runtime.get("name") or "")
    return name.removeprefix("iOS ") if name.startswith("iOS ") else ""


def ios_versions(simulators: list[dict]) -> set[str]:
    return {
        version
        for simulator in simulators
        if (version := ios_simulator_version(simulator))
        and str((simulator.get("runtime") or {}).get("name") or "").startswith("iOS ")
        and simulator.get("isAvailable", True)
        and (simulator.get("runtime") or {}).get("isAvailable", True)
    }


def ios_device_types(simulators: list[dict], version: str) -> set[str]:
    devices = {
        str((simulator.get("deviceType") or {}).get("name") or "")
        for simulator in simulators
        if ios_simulator_version(simulator) == version
        and (simulator.get("deviceType") or {}).get("productFamily") == "iPhone"
        and simulator.get("isAvailable", True)
    }
    devices.discard("")
    return devices


def resolve_ios_device_type(simulators: list[dict], version: str, requested: str | None) -> str:
    devices = ios_device_types(simulators, version)
    if requested:
        if requested not in devices:
            available = ", ".join(sorted(devices)) or "none"
            raise common.ReleaseTestError(f"iOS {version} does not support device type {requested}; available iPhones: {available}")
        return requested
    if not devices:
        raise common.ReleaseTestError(f"iOS {version} has no available iPhone simulator device type")

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


def run_ios(root: Path, args, version: str) -> None:
    common.require_workload(root, "maui")
    common.require_appium_driver(root, "xcuitest")
    simulators = apple_simulators(root)
    if version not in ios_versions(simulators):
        raise common.ReleaseTestError(f"iOS {version} is not installed")
    runtime = f"iOS {version}"
    device_type = resolve_ios_device_type(simulators, version, args.device)
    simulator_name = f"SkiaSharp Release iOS {version} {uuid.uuid4().hex[:8]}"
    simulator_id = simulator_name
    try:
        simulator = common.run_json(
            [
                "dotnet",
                "tool",
                "run",
                "apple",
                "--",
                "simulator",
                "create",
                simulator_name,
                "--device-type",
                device_type,
                "--runtime",
                runtime,
                "--format",
                "json",
            ],
            cwd=root,
        )
        simulator_id = str(simulator.get("udid") or "")
        if not simulator_id:
            raise common.ReleaseTestError("the temporary iOS simulator has no UDID")
        print(f"Selected {simulator_name} ({runtime}) [{simulator_id}]", flush=True)
        common.run_streaming(["dotnet", "tool", "run", "apple", "--", "simulator", "boot", simulator_id, "--wait", "--timeout", "180"], cwd=root)
        common.run_test(root, "MauiiOSTests", args, properties={"iOSDevice": simulator_name, "iOSVersion": version})
    finally:
        common.run_streaming(["dotnet", "tool", "run", "apple", "--", "simulator", "delete", simulator_id, "--force"], cwd=root, check=False)


def execute(root: Path, args) -> None:
    if sys.platform != "darwin":
        raise common.ReleaseTestError("iOS release tests require macOS")
    run_ios(root, args, args.version)


def create_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("version")
    parser.add_argument("--device")
    common.add_package_arguments(parser)
    return parser


if __name__ == "__main__":
    args = create_parser().parse_args()
    args.command = f"ios-{args.version}"
    sys.exit(common.execute_item(args, execute))
