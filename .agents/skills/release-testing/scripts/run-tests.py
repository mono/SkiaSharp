#!/usr/bin/env python3
"""Run one approved SkiaSharp release integration-test matrix item."""

from __future__ import annotations

import argparse
import json
import os
from pathlib import Path
import platform
import re
import shlex
import shutil
import socket
import subprocess
import sys
import time
import uuid


TEST_PROJECT = (
    "tests/SkiaSharp.Tests.Integration/"
    "SkiaSharp.Tests.Integration.csproj"
)
REQUIRED_APPIUM_VERSION = "3.6.0"
HEARTBEAT_SECONDS = 5
REQUIRED_APPIUM_DRIVERS = {
    "mac2": "4.0.5",
    "uiautomator2": "8.2.2",
    "xcuitest": "12.1.2",
}
ANDROID_IMAGE_FLAVORS = {
    "google_apis": 4,
    "google_apis_ps16k": 3,
    "google_apis_playstore": 2,
    "google_apis_playstore_ps16k": 1,
}


class TestRunError(RuntimeError):
    """A release test setup, execution, or cleanup step failed."""


def display(args: list[str]) -> str:
    return (
        subprocess.list2cmdline(args)
        if sys.platform == "win32"
        else shlex.join(args)
    )


def display_duration(seconds: float) -> str:
    total = max(0, int(seconds))
    minutes, seconds = divmod(total, 60)
    hours, minutes = divmod(minutes, 60)
    if hours:
        return f"{hours}h {minutes:02d}m {seconds:02d}s"
    if minutes:
        return f"{minutes}m {seconds:02d}s"
    return f"{seconds}s"


def resolve_command(args: list[str]) -> list[str]:
    executable = shutil.which(args[0])
    if not executable:
        return args
    resolved = [executable, *args[1:]]
    if sys.platform == "win32" and Path(executable).suffix.lower() in {
        ".bat",
        ".cmd",
    }:
        return [
            shutil.which("cmd.exe") or "cmd.exe",
            "/d",
            "/s",
            "/c",
            subprocess.list2cmdline(resolved),
        ]
    return resolved


def run(
    args: list[str],
    *,
    cwd: Path,
    capture: bool = False,
    check: bool = True,
) -> subprocess.CompletedProcess[str]:
    args = resolve_command(args)
    command = display(args)
    started = time.monotonic()
    print(f"[release-test] command started: {command}", flush=True)
    try:
        process = subprocess.Popen(
            args,
            cwd=cwd,
            text=True,
            stdout=subprocess.PIPE if capture else None,
            stderr=subprocess.PIPE if capture else None,
        )
    except FileNotFoundError as error:
        raise TestRunError(f"{args[0]} was not found on PATH") from error
    while True:
        try:
            stdout, stderr = process.communicate(timeout=HEARTBEAT_SECONDS)
            break
        except subprocess.TimeoutExpired:
            elapsed = display_duration(time.monotonic() - started)
            print(
                f"[release-test] command still running after {elapsed}: "
                f"{command}",
                flush=True,
            )
        except KeyboardInterrupt:
            process.terminate()
            try:
                process.wait(timeout=5)
            except subprocess.TimeoutExpired:
                process.kill()
            raise
    result = subprocess.CompletedProcess(
        args=args,
        returncode=process.returncode,
        stdout=stdout,
        stderr=stderr,
    )
    elapsed = display_duration(time.monotonic() - started)
    print(
        f"[release-test] command finished after {elapsed} "
        f"(exit {result.returncode}): {command}",
        flush=True,
    )
    if check and result.returncode != 0:
        detail = (
            (result.stderr or "").strip()
            or (result.stdout or "").strip()
            or "no output"
        )
        raise TestRunError(
            f"command failed ({result.returncode}): {display(args)}\n{detail}"
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
    raise TestRunError("command returned no valid JSON")


def run_json(args: list[str], *, cwd: Path):
    return parse_json_output(run(args, cwd=cwd, capture=True).stdout)


def repo_root() -> Path:
    result = subprocess.run(
        ["git", "rev-parse", "--show-toplevel"],
        capture_output=True,
        text=True,
    )
    if result.returncode != 0:
        raise TestRunError("could not locate the repository root")
    return Path(result.stdout.strip())


def installed_workloads(root: Path) -> set[str]:
    data = run_json(
        ["dotnet", "workload", "list", "--machine-readable"],
        cwd=root,
    )
    return set(data.get("installed", []))


def require_workload(root: Path, workload: str) -> None:
    if workload not in installed_workloads(root):
        raise TestRunError(
            f"the {workload} workload is not installed"
        )


def configure_android_environment(
    root: Path,
    environ: dict[str, str] | None = None,
) -> dict[str, str]:
    environ = environ if environ is not None else os.environ
    android_home = run(
        [
            "dotnet",
            "tool",
            "run",
            "android",
            "--",
            "sdk",
            "find",
        ],
        cwd=root,
        capture=True,
    ).stdout.strip()
    if not android_home or not Path(android_home).is_dir():
        raise TestRunError(
            f"Android SDK directory was not found: {android_home or '(empty)'}"
        )

    java_home = run(
        [
            "dotnet",
            "tool",
            "run",
            "android",
            "--",
            "jdk",
            "find",
        ],
        cwd=root,
        capture=True,
    ).stdout.strip()
    if not java_home or not Path(java_home).is_dir():
        raise TestRunError(
            f"Java SDK directory was not found: {java_home or '(empty)'}"
        )

    environ["ANDROID_HOME"] = android_home
    environ["JAVA_HOME"] = java_home
    print(f"Using ANDROID_HOME={android_home}", flush=True)
    print(f"Using JAVA_HOME={java_home}", flush=True)
    return {
        "ANDROID_HOME": android_home,
        "JAVA_HOME": java_home,
    }


def require_appium_port_available() -> None:
    try:
        with socket.create_connection(
            ("127.0.0.1", 4723),
            timeout=1,
        ):
            pass
    except OSError:
        return
    raise TestRunError(
        "Appium is already running on port 4723; stop it so the release "
        "runner can use its verified server and drivers"
    )


def validate_appium_driver(
    server_version: str,
    drivers: dict,
    driver: str,
) -> None:
    if server_version != REQUIRED_APPIUM_VERSION:
        raise TestRunError(
            f"Appium {REQUIRED_APPIUM_VERSION} is required; "
            f"found {server_version or 'unknown'}"
        )
    installed = drivers.get(driver) or {}
    if not installed.get("installed"):
        raise TestRunError(
            f"the Appium {driver} driver is not installed"
        )
    required_version = REQUIRED_APPIUM_DRIVERS[driver]
    installed_version = str(installed.get("version") or "")
    if installed_version != required_version:
        raise TestRunError(
            f"Appium {driver} {required_version} is required; "
            f"found {installed_version or 'unknown'}"
        )


def require_appium_driver(root: Path, driver: str) -> None:
    require_appium_port_available()
    if not shutil.which("appium"):
        raise TestRunError("Appium is not installed or is not on PATH")
    server_version = run(
        ["appium", "--version"],
        cwd=root,
        capture=True,
    ).stdout.strip()
    drivers = run_json(
        ["appium", "driver", "list", "--installed", "--json"],
        cwd=root,
    )
    validate_appium_driver(server_version, drivers, driver)
    run(["appium", "driver", "doctor", driver], cwd=root)


def test_args(
    test_class: str,
    *,
    skia: str,
    harfbuzz: str,
    properties: dict[str, str] | None = None,
) -> list[str]:
    args = [
        "dotnet",
        "test",
        TEST_PROJECT,
        f"-p:SkiaSharpVersion={skia}",
        f"-p:HarfBuzzSharpVersion={harfbuzz}",
        "-p:BaseFramework=net10.0",
        "-p:SdkVersion=10.0.100",
        "-p:SdkAllowPrerelease=false",
    ]
    for name, value in (properties or {}).items():
        args.append(f"-p:{name}={value}")
    args.extend(
        [
            "--",
            "--filter-class",
            f"SkiaSharp.Tests.Integration.{test_class}",
        ]
    )
    return args


def run_test(
    root: Path,
    test_class: str,
    args,
    *,
    properties: dict[str, str] | None = None,
) -> None:
    run(
        test_args(
            test_class,
            skia=args.skia,
            harfbuzz=args.harfbuzz,
            properties=properties,
        ),
        cwd=root,
    )


def numeric_version(value: str) -> tuple[int, ...]:
    match = re.match(r"\d+(?:\.\d+)*", value)
    if not match:
        raise TestRunError(f"invalid platform version: {value}")
    return tuple(int(part) for part in match.group().split("."))


def android_image_version(path: str) -> tuple[int, ...] | None:
    parts = path.split(";")
    if len(parts) != 4 or not parts[1].startswith("android-"):
        return None
    try:
        return numeric_version(parts[1].removeprefix("android-"))
    except TestRunError:
        return None


def select_android_image(
    packages: list[dict],
    *,
    selector: str,
    architecture: str,
) -> tuple[str, str]:
    requested = numeric_version(selector)
    candidates: list[tuple[tuple, str, str]] = []
    for package in packages:
        path = str(package.get("path") or "")
        parts = path.split(";")
        version = android_image_version(path)
        if (
            version is None
            or version != requested
            or parts[3] != architecture
        ):
            continue
        flavor_score = ANDROID_IMAGE_FLAVORS.get(parts[2])
        if flavor_score is None:
            continue
        platform = parts[1].removeprefix("android-")
        prerelease = re.search(r"(?:beta|rc)(\d+)", platform)
        stable = prerelease is None
        package_revision = numeric_version(str(package.get("version") or "0"))
        score = (
            stable,
            int(prerelease.group(1)) if prerelease else 0,
            package_revision,
            flavor_score,
        )
        candidates.append((score, path, ".".join(map(str, version))))
    if not candidates:
        raise TestRunError(
            f"Android {selector} is not installed for "
            f"{architecture}"
        )
    _, path, version = max(candidates)
    return path, version


def android_devices(root: Path) -> list[dict]:
    return run_json(
        [
            "dotnet",
            "tool",
            "run",
            "android",
            "--",
            "device",
            "list",
            "--format",
            "json",
        ],
        cwd=root,
    )


def android_device_api(root: Path, device_id: str) -> str:
    devices = run_json(
        [
            "dotnet",
            "tool",
            "run",
            "android",
            "--",
            "device",
            "info",
            "--id",
            device_id,
            "--property",
            r"^ro\.build\.version\.sdk$",
            "--format",
            "json",
        ],
        cwd=root,
    )
    if len(devices) != 1:
        raise TestRunError(
            f"expected one Android device matching {device_id}"
        )
    return str(
        (devices[0].get("properties") or {}).get(
            "ro.build.version.sdk",
            "",
        )
    )


def run_android_test(
    root: Path,
    args,
    *,
    device_id: str,
    device_name: str,
    expected_api: str,
) -> None:
    actual_api = android_device_api(root, device_id)
    if actual_api != expected_api:
        raise TestRunError(
            f"Android device {device_id} is API {actual_api}; "
            f"expected API {expected_api}"
        )
    print(
        f"Using Android device {device_id} ({device_name}), API {actual_api}",
        flush=True,
    )
    run_test(
        root,
        "MauiAndroidTests",
        args,
        properties={
            "AndroidDevice": device_name,
            "AndroidDeviceId": device_id,
            "AndroidApiLevel": expected_api,
        },
    )


def run_android(root: Path, args, selector: str) -> None:
    require_workload(root, "maui")
    configure_android_environment(root)

    sdk = run_json(
        [
            "dotnet",
            "tool",
            "run",
            "android",
            "--",
            "sdk",
            "list",
            "--installed",
            "--format",
            "json",
        ],
        cwd=root,
    )
    packages = sdk.get("InstalledPackages") or []
    installed_paths = {
        str(package.get("path") or "")
        for package in packages
    }
    if "platform-tools" not in installed_paths:
        raise TestRunError(
            "Android SDK package platform-tools is not installed"
        )
    expected_api = str(numeric_version(selector)[0])
    connected = android_devices(root)
    selected = None
    if args.device_id:
        selected = next(
            (
                device
                for device in connected
                if device.get("serial") == args.device_id
            ),
            None,
        )
        if selected is None:
            raise TestRunError(
                f"Android device {args.device_id} is not connected"
            )
    else:
        emulators = [
            device for device in connected if device.get("isEmulator")
        ]
        if len(emulators) == 1:
            selected = emulators[0]
        elif len(emulators) > 1:
            serials = ", ".join(
                str(device.get("serial")) for device in emulators
            )
            raise TestRunError(
                f"multiple Android emulators are running ({serials}); "
                "select one with --device-id"
            )
    require_appium_driver(root, "uiautomator2")
    if selected:
        device_id = str(selected.get("serial"))
        device_name = str(
            selected.get("model")
            or selected.get("device")
            or device_id
        )
        run_android_test(
            root,
            args,
            device_id=device_id,
            device_name=device_name,
            expected_api=expected_api,
        )
        return

    if "emulator" not in installed_paths:
        raise TestRunError("Android SDK package emulator is not installed")
    architecture = (
        "arm64-v8a"
        if platform.machine().lower() in ("arm64", "aarch64")
        else "x86_64"
    )
    image, version = select_android_image(
        packages,
        selector=selector,
        architecture=architecture,
    )
    device = args.device or "pixel"
    avd_name = (
        f"SkiaSharp_Release_Android_{version.replace('.', '_')}_"
        f"{uuid.uuid4().hex[:8]}"
    )
    print(
        f"Selected Android {version} using {image} and device {device}",
        flush=True,
    )
    try:
        run(
            [
                "dotnet",
                "tool",
                "run",
                "android",
                "--",
                "avd",
                "create",
                "--name",
                avd_name,
                "--sdk",
                image,
                "--device",
                device,
                "--force",
            ],
            cwd=root,
        )
        run(
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
                "5554",
                "--wipe",
                "--no-window",
                "--no-snapshot",
                "--no-audio",
                "--no-boot-anim",
                "--no-animations",
                "--gpu",
                (
                    "swiftshader_indirect"
                    if sys.platform.startswith("linux")
                    else "host"
                    if sys.platform == "darwin"
                    else "guest"
                ),
                *(
                    ["--accel", "on"]
                    if sys.platform == "darwin"
                    else []
                ),
                "--wait",
                "--timeout",
                "300",
            ],
            cwd=root,
        )
        run_android_test(
            root,
            args,
            device_id="emulator-5554",
            device_name=avd_name,
            expected_api=expected_api,
        )
    finally:
        run(
            [
                "dotnet",
                "tool",
                "run",
                "android",
                "--",
                "avd",
                "delete",
                "--name",
                avd_name,
                "--force",
            ],
            cwd=root,
            check=False,
        )


def apple_simulators(root: Path) -> list[dict]:
    return run_json(
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
    )


def ios_simulator_version(simulator: dict) -> str:
    runtime = simulator.get("runtime") or {}
    version = str(runtime.get("version") or "")
    if version:
        return version
    name = str(runtime.get("name") or "")
    return name.removeprefix("iOS ") if name.startswith("iOS ") else ""


def installed_ios_versions(root: Path) -> set[str]:
    return {
        version
        for simulator in apple_simulators(root)
        if (version := ios_simulator_version(simulator))
        and str((simulator.get("runtime") or {}).get("name") or "").startswith(
            "iOS "
        )
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


def resolve_ios_device_type(
    simulators: list[dict],
    version: str,
    requested: str | None,
) -> str:
    devices = ios_device_types(simulators, version)
    if requested:
        if requested not in devices:
            available = ", ".join(sorted(devices)) or "none"
            raise TestRunError(
                f"iOS {version} does not support device type {requested}; "
                f"available iPhones: {available}"
            )
        return requested
    if not devices:
        raise TestRunError(
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


def run_ios(root: Path, args, selector: str) -> None:
    if sys.platform != "darwin":
        raise TestRunError("MAUI iOS release tests require macOS")
    require_workload(root, "maui")
    require_appium_driver(root, "xcuitest")
    version = selector
    simulators = apple_simulators(root)
    if version not in {
        ios_simulator_version(simulator)
        for simulator in simulators
    }:
        raise TestRunError(
            f"iOS {version} is not installed"
        )
    runtime = f"iOS {version}"
    device_type = resolve_ios_device_type(
        simulators,
        version,
        args.device,
    )
    simulator_name = (
        f"SkiaSharp Release iOS {version} {uuid.uuid4().hex[:8]}"
    )
    simulator_id = simulator_name
    try:
        simulator = run_json(
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
            raise TestRunError(
                "the temporary iOS simulator has no UDID"
            )
        print(
            f"Selected {simulator_name} ({runtime}) "
            f"[{simulator_id}]",
            flush=True,
        )
        run(
            [
                "dotnet",
                "tool",
                "run",
                "apple",
                "--",
                "simulator",
                "boot",
                simulator_id,
                "--wait",
                "--timeout",
                "180",
            ],
            cwd=root,
        )
        run_test(
            root,
            "MauiiOSTests",
            args,
            properties={
                "iOSDevice": simulator_name,
                "iOSVersion": version,
            },
        )
    finally:
        run(
            [
                "dotnet",
                "tool",
                "run",
                "apple",
                "--",
                "simulator",
                "delete",
                simulator_id,
                "--force",
            ],
            cwd=root,
            check=False,
        )


def create_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument(
        "command",
        help=(
            "smoke, console, linux, blazor, maccatalyst, windows, "
            "android-VERSION, or ios-VERSION"
        ),
    )
    parser.add_argument("--skiasharp", dest="skia", required=True)
    parser.add_argument("--harfbuzzsharp", dest="harfbuzz", required=True)
    parser.add_argument("--device")
    parser.add_argument("--device-id")
    return parser


def mobile_command(command: str) -> tuple[str, str] | None:
    match = re.fullmatch(
        r"(android|ios)-(\d+(?:\.\d+)*)",
        command,
    )
    return (match.group(1), match.group(2)) if match else None


def main() -> int:
    args = create_parser().parse_args()
    started = time.monotonic()
    print(
        f"[release-test] item started: {args.command}; "
        f"SkiaSharp={args.skia}; HarfBuzzSharp={args.harfbuzz}",
        flush=True,
    )
    try:
        root = repo_root()
        mobile = mobile_command(args.command)
        if args.device and not mobile:
            raise TestRunError(
                f"--device is not supported for {args.command}"
            )
        if args.device_id and (not mobile or mobile[0] != "android"):
            raise TestRunError(
                "--device-id is supported only for Android commands"
            )
        if args.command == "smoke":
            run_test(root, "SmokeTests", args)
        elif args.command == "console":
            run_test(root, "ConsoleTests", args)
        elif args.command == "linux":
            docker = run(
                ["docker", "info", "--format", "{{.OSType}}"],
                cwd=root,
                capture=True,
            ).stdout.strip()
            if docker != "linux":
                raise TestRunError("Docker Linux daemon is required")
            run_test(root, "LinuxConsoleTests", args)
        elif args.command == "blazor":
            require_workload(root, "wasm-tools")
            run_test(root, "BlazorTests", args)
        elif args.command == "maccatalyst":
            if sys.platform != "darwin":
                raise TestRunError("Mac Catalyst release tests require macOS")
            require_workload(root, "maui")
            require_appium_driver(root, "mac2")
            run_test(root, "MauiMacCatalystTests", args)
        elif mobile:
            if mobile[0] == "android":
                run_android(root, args, mobile[1])
            else:
                run_ios(root, args, mobile[1])
        elif args.command == "windows":
            if sys.platform != "win32":
                raise TestRunError("MAUI Windows release tests require Windows")
            require_workload(root, "maui")
            if not shutil.which("WinAppDriver.exe"):
                raise TestRunError("WinAppDriver.exe was not found on PATH")
            run_test(root, "MauiWindowsTests", args)
        else:
            raise TestRunError(f"unsupported command: {args.command}")
    except TestRunError as error:
        elapsed = display_duration(time.monotonic() - started)
        summary = str(error).splitlines()[0]
        print(
            f"[release-test] item failed after {elapsed}: "
            f"{args.command}: {summary}",
            file=sys.stderr,
            flush=True,
        )
        print(f"ERROR: {error}", file=sys.stderr)
        return 1
    elapsed = display_duration(time.monotonic() - started)
    print(
        f"[release-test] item passed after {elapsed}: {args.command}",
        flush=True,
    )
    return 0


if __name__ == "__main__":
    sys.exit(main())
