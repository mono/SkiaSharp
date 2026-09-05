#!/usr/bin/env python3
"""Run one approved Android SkiaSharp release-test matrix item."""

import argparse
import os
from pathlib import Path
import platform
import re
import sys
import uuid
import release_test_common as common

ANDROID_IMAGE_FLAVORS = {"google_apis": 4, "google_apis_ps16k": 3, "google_apis_playstore": 2, "google_apis_playstore_ps16k": 1}


def configure_android_environment(root: Path, environ: dict[str, str] | None = None) -> None:
    environ = environ if environ is not None else os.environ
    android_home = common.run_streaming(["dotnet", "tool", "run", "android", "--", "sdk", "find"], cwd=root, capture=True).stdout.strip()
    if not android_home or not Path(android_home).is_dir():
        raise common.ReleaseTestError(f"Android SDK directory was not found: {android_home or '(empty)'}")

    java_home = common.run_streaming(["dotnet", "tool", "run", "android", "--", "jdk", "find"], cwd=root, capture=True).stdout.strip()
    if not java_home or not Path(java_home).is_dir():
        raise common.ReleaseTestError(f"Java SDK directory was not found: {java_home or '(empty)'}")

    environ["ANDROID_HOME"] = android_home
    environ["JAVA_HOME"] = java_home
    print(f"Using ANDROID_HOME={android_home}", flush=True)
    print(f"Using JAVA_HOME={java_home}", flush=True)


def numeric_version(value: str) -> tuple[int, ...]:
    match = re.match(r"\d+(?:\.\d+)*", value)
    if not match:
        raise common.ReleaseTestError(f"invalid platform version: {value}")
    return tuple(int(part) for part in match.group().split("."))


def android_image_version(path: str) -> tuple[int, ...] | None:
    parts = path.split(";")
    if len(parts) != 4 or not parts[1].startswith("android-"):
        return None
    try:
        return numeric_version(parts[1].removeprefix("android-"))
    except common.ReleaseTestError:
        return None


def select_android_image(packages: list[dict], *, selector: str, architecture: str) -> tuple[str, str]:
    requested = numeric_version(selector)
    candidates: list[tuple[tuple, str, str]] = []
    for package in packages:
        path = str(package.get("path") or "")
        parts = path.split(";")
        version = android_image_version(path)
        if version is None or version != requested or parts[3] != architecture:
            continue
        flavor_score = ANDROID_IMAGE_FLAVORS.get(parts[2])
        if flavor_score is None:
            continue
        platform_version = parts[1].removeprefix("android-")
        prerelease = re.search(r"(?:beta|rc)(\d+)", platform_version)
        stable = prerelease is None
        package_revision = numeric_version(str(package.get("version") or "0"))
        score = (stable, int(prerelease.group(1)) if prerelease else 0, package_revision, flavor_score)
        candidates.append((score, path, ".".join(map(str, version))))
    if not candidates:
        raise common.ReleaseTestError(f"Android {selector} is not installed for {architecture}")
    _, path, version = max(candidates)
    return path, version


def android_devices(root: Path) -> list[dict]:
    return common.run_json(["dotnet", "tool", "run", "android", "--", "device", "list", "--format", "json"], cwd=root)


def android_device_api(root: Path, device_id: str) -> str:
    devices = common.run_json(
        ["dotnet", "tool", "run", "android", "--", "device", "info", "--id", device_id, "--property", r"^ro\.build\.version\.sdk$", "--format", "json"],
        cwd=root,
    )
    if len(devices) != 1:
        raise common.ReleaseTestError(f"expected one Android device matching {device_id}")
    return str((devices[0].get("properties") or {}).get("ro.build.version.sdk", ""))


def select_emulator_port(devices: list[dict]) -> int:
    used_ports = {
        int(match.group(1))
        for device in devices
        if (match := re.fullmatch(r"emulator-(\d+)", str(device.get("serial") or "")))
    }
    for port in range(5554, 5683, 2):
        if port not in used_ports:
            return port
    raise common.ReleaseTestError("no Android emulator port is available from 5554 through 5682")


def run_android_test(root: Path, args, *, device_id: str, device_name: str, expected_api: str) -> None:
    actual_api = android_device_api(root, device_id)
    if actual_api != expected_api:
        raise common.ReleaseTestError(f"Android device {device_id} is API {actual_api}; expected API {expected_api}")
    print(f"Using Android device {device_id} ({device_name}), API {actual_api}", flush=True)
    common.run_test(root, "MauiAndroidTests", args, properties={"AndroidDevice": device_name, "AndroidDeviceId": device_id, "AndroidApiLevel": expected_api})


def execute(root: Path, args) -> None:
    selector = args.version
    common.require_workload(root, "maui")
    configure_android_environment(root)

    sdk = common.run_json(["dotnet", "tool", "run", "android", "--", "sdk", "list", "--installed", "--format", "json"], cwd=root)
    packages = sdk.get("InstalledPackages") or []
    installed_paths = {str(package.get("path") or "") for package in packages}
    if "platform-tools" not in installed_paths:
        raise common.ReleaseTestError("Android SDK package platform-tools is not installed")
    expected_api = str(numeric_version(selector)[0])
    connected = android_devices(root)
    selected = None
    if args.device_id:
        selected = next((device for device in connected if device.get("serial") == args.device_id), None)
        if selected is None:
            raise common.ReleaseTestError(f"Android device {args.device_id} is not connected")
    common.require_appium_driver(root, "uiautomator2")
    if selected:
        device_id = str(selected.get("serial"))
        device_name = str(selected.get("model") or selected.get("device") or device_id)
        run_android_test(root, args, device_id=device_id, device_name=device_name, expected_api=expected_api)
        return

    if "emulator" not in installed_paths:
        raise common.ReleaseTestError("Android SDK package emulator is not installed")
    architecture = "arm64-v8a" if platform.machine().lower() in ("arm64", "aarch64") else "x86_64"
    image, version = select_android_image(packages, selector=selector, architecture=architecture)
    device = args.device or "pixel"
    avd_name = f"SkiaSharp_Release_Android_{version.replace('.', '_')}_{uuid.uuid4().hex[:8]}"
    emulator_port = select_emulator_port(connected)
    device_id = f"emulator-{emulator_port}"
    print(f"Selected Android {version} using {image} and device {device} on {device_id}", flush=True)
    try:
        common.run_streaming(
            ["dotnet", "tool", "run", "android", "--", "avd", "create", "--name", avd_name, "--sdk", image, "--device", device, "--force"], cwd=root
        )
        common.run_streaming(
            [
                "dotnet",
                "tool",
                "run",
                "android",
                "--",
                "avd",
                "start",
                "--name",
                avd_name,
                "--port",
                str(emulator_port),
                "--wipe",
                "--no-window",
                "--no-snapshot",
                "--no-audio",
                "--no-boot-anim",
                "--no-animations",
                "--gpu",
                ("swiftshader_indirect" if sys.platform.startswith("linux") else "host" if sys.platform == "darwin" else "guest"),
                *(["--accel", "on"] if sys.platform == "darwin" else []),
                "--wait",
                "--timeout",
                "300",
            ],
            cwd=root,
        )
        run_android_test(root, args, device_id=device_id, device_name=avd_name, expected_api=expected_api)
    finally:
        common.run_streaming(["dotnet", "tool", "run", "android", "--", "avd", "delete", "--name", avd_name, "--force"], cwd=root, check=False)


def create_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("version")
    parser.add_argument("--device")
    parser.add_argument("--device-id")
    common.add_package_arguments(parser)
    return parser


if __name__ == "__main__":
    args = create_parser().parse_args()
    args.command = f"android-{args.version}"
    sys.exit(common.execute_item(args, execute))
