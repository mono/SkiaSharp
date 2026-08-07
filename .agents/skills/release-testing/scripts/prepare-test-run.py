#!/usr/bin/env python3
"""Restore pinned tools and reset release integration-test output."""

import argparse
import json
from pathlib import Path
import re
import shutil
import subprocess
import sys


OUTPUT_PATH = Path("output/logs/testlogs/integration")


class PreparationError(RuntimeError):
    pass


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
    raise PreparationError("command returned no valid JSON")


def numeric_version(value: str) -> tuple[int, ...]:
    if not re.fullmatch(r"\d+(?:\.\d+)*", value):
        raise PreparationError(f"invalid Apple version: {value}")
    return tuple(int(part) for part in value.split("."))


def xcode_target_majors(xcode_version: str) -> tuple[int, int]:
    major = numeric_version(xcode_version)[0]
    if major == 26:
        return 15, 26
    if major >= 27:
        return 18, 26
    raise PreparationError(
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
    except PreparationError:
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
    candidates = {
        str((simulator.get("deviceType") or {}).get("name") or "")
        for simulator in simulators
        if simulator_runtime_version(simulator) == version
        and (simulator.get("deviceType") or {}).get("productFamily") == "iPhone"
        and simulator.get("isAvailable", True)
    }
    candidates.discard("")
    return candidates


def preferred_device_type(simulators: list[dict], version: str) -> str:
    candidates = ios_device_types(simulators, version)
    if not candidates:
        raise PreparationError(
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

    return max(candidates, key=score)


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
            raise PreparationError(
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
    if minimum == maximum:
        raise PreparationError(
            "minimum and maximum iOS coverage resolved to the same runtime"
        )
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


def run(args, *, cwd):
    try:
        result = subprocess.run(
            args,
            cwd=cwd,
            capture_output=True,
            text=True,
        )
    except FileNotFoundError as error:
        raise PreparationError(
            f"{args[0]} was not found on PATH"
        ) from error
    if result.returncode != 0:
        detail = result.stderr.strip() or result.stdout.strip() or "no output"
        raise PreparationError(
            f"command failed ({result.returncode}): {' '.join(args)}\n{detail}"
        )
    return result


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
        raise PreparationError(
            "dotnet apple did not report a selected Xcode"
        )
    xcode_version = str(
        selected.get("Version") or selected.get("version") or ""
    )
    xcode_path = str(selected.get("Path") or selected.get("path") or "")
    if not xcode_version or not xcode_path:
        raise PreparationError(
            "selected Xcode is missing its version or path"
        )
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


def reset_output(root: Path) -> Path:
    output = (root / OUTPUT_PATH).resolve()
    expected_parent = (root / "output/logs/testlogs").resolve()
    if output.parent != expected_parent:
        raise PreparationError(f"unexpected output path: {output}")
    output.mkdir(parents=True, exist_ok=True)
    for child in output.iterdir():
        if child.is_dir() and not child.is_symlink():
            shutil.rmtree(child)
        else:
            child.unlink()
    return output


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--detect-apple-targets", action="store_true")
    args = parser.parse_args()
    try:
        root = Path(
            run(
                ["git", "rev-parse", "--show-toplevel"],
                cwd=Path.cwd(),
            ).stdout.strip()
        )
        if args.detect_apple_targets:
            if sys.platform != "darwin":
                raise PreparationError(
                    "Apple targets can be detected only on macOS"
                )
            print(json.dumps(detect_apple_targets(root), indent=2))
            return 0
        run(["dotnet", "tool", "restore"], cwd=root)
        run(
            ["dotnet", "tool", "run", "android", "--", "--help"],
            cwd=root,
        )
        tools = ["android"]
        if sys.platform == "darwin":
            run(
                ["dotnet", "tool", "run", "apple", "--", "--help"],
                cwd=root,
            )
            tools.append("apple")
        output = reset_output(root)
        print(
            json.dumps(
                {
                    "toolsRestored": True,
                    "toolsVerified": tools,
                    "outputDirectory": str(output),
                    "outputReset": True,
                },
                indent=2,
            )
        )
    except PreparationError as error:
        print(f"ERROR: {error}", file=sys.stderr)
        return 1
    return 0


if __name__ == "__main__":
    sys.exit(main())
