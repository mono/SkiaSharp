#!/usr/bin/env python3
"""Shared process and JSON helpers for release-testing scripts."""

from __future__ import annotations
import argparse
import json
from pathlib import Path
import re
import shlex
import shutil
import socket
import subprocess
import sys
import time

ANDROID_MIN_VERSION = "26"
ANDROID_MAX_VERSION = "37.1"
IOS_MIN_VERSION = "18.6"
IOS_MAX_VERSION = "26.5"
TEST_PROJECT = "tests/SkiaSharp.Tests.Integration/SkiaSharp.Tests.Integration.csproj"
APPIUM_COMMAND = ["npm", "exec", "--no", "--", "appium"]
MINIMUM_APPIUM_VERSION = "3.6.0"
MINIMUM_APPIUM_DRIVERS = {"mac2": "4.1.1", "uiautomator2": "8.2.2", "xcuitest": "12.1.2"}
SEMVER_PATTERN = re.compile(r"^v?(\d+)\.(\d+)\.(\d+)(?:-([0-9A-Za-z.-]+))?(?:\+[0-9A-Za-z.-]+)?$")
HEARTBEAT_SECONDS = 5


class ReleaseTestError(RuntimeError):
    """A release-testing script could not complete safely."""


def run_checked(args: list[str], *, cwd: Path | None = None, timeout: int | None = None) -> subprocess.CompletedProcess[str]:
    try:
        result = subprocess.run(args, cwd=cwd, capture_output=True, text=True, timeout=timeout)
    except FileNotFoundError as error:
        raise ReleaseTestError(f"{args[0]} was not found on PATH") from error
    except subprocess.TimeoutExpired as error:
        raise ReleaseTestError(f"command timed out after {timeout}s: {' '.join(args)}") from error
    if result.returncode != 0:
        detail = result.stderr.strip() or result.stdout.strip() or "no output"
        raise ReleaseTestError(f"command failed ({result.returncode}): {' '.join(args)}\n{detail}")
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
    raise ReleaseTestError("command returned no valid JSON")


def repository_root(*, cwd: Path | None = None) -> Path:
    return Path(run_checked(["git", "rev-parse", "--show-toplevel"], cwd=cwd, timeout=30).stdout.strip())


def display(args: list[str]) -> str:
    return subprocess.list2cmdline(args) if sys.platform == "win32" else shlex.join(args)


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
    if sys.platform == "win32" and Path(executable).suffix.lower() in {".bat", ".cmd"}:
        return [shutil.which("cmd.exe") or "cmd.exe", "/d", "/s", "/c", subprocess.list2cmdline(resolved)]
    return resolved


def run_streaming(args: list[str], *, cwd: Path, capture: bool = False, check: bool = True) -> subprocess.CompletedProcess[str]:
    args = resolve_command(args)
    command = display(args)
    started = time.monotonic()
    print(f"[release-test] command started: {command}", flush=True)
    try:
        process = subprocess.Popen(args, cwd=cwd, text=True, stdout=subprocess.PIPE if capture else None, stderr=subprocess.PIPE if capture else None)
    except FileNotFoundError as error:
        raise ReleaseTestError(f"{args[0]} was not found on PATH") from error
    while True:
        try:
            stdout, stderr = process.communicate(timeout=HEARTBEAT_SECONDS)
            break
        except subprocess.TimeoutExpired:
            elapsed = display_duration(time.monotonic() - started)
            print(f"[release-test] command still running after {elapsed}: {command}", flush=True)
        except KeyboardInterrupt:
            process.terminate()
            try:
                process.wait(timeout=5)
            except subprocess.TimeoutExpired:
                process.kill()
            raise
    result = subprocess.CompletedProcess(args=args, returncode=process.returncode, stdout=stdout, stderr=stderr)
    elapsed = display_duration(time.monotonic() - started)
    print(f"[release-test] command finished after {elapsed} (exit {result.returncode}): {command}", flush=True)
    if check and result.returncode != 0:
        detail = (result.stderr or "").strip() or (result.stdout or "").strip() or "no output"
        raise ReleaseTestError(f"command failed ({result.returncode}): {display(args)}\n{detail}")
    return result


def run_json(args: list[str], *, cwd: Path):
    return parse_json_output(run_streaming(args, cwd=cwd, capture=True).stdout)


def installed_workloads(root: Path) -> set[str]:
    data = run_json(["dotnet", "workload", "list", "--machine-readable"], cwd=root)
    return set(data.get("installed", []))


def require_workload(root: Path, workload: str) -> None:
    if workload not in installed_workloads(root):
        raise ReleaseTestError(f"the {workload} workload is not installed")


def require_appium_port_available() -> None:
    try:
        with socket.create_connection(("127.0.0.1", 4723), timeout=1):
            pass
    except OSError:
        return
    raise ReleaseTestError("Appium is already running on port 4723")


def appium_version_output(text: str) -> str:
    versions = {line.strip().removeprefix("v") for line in text.splitlines() if SEMVER_PATTERN.fullmatch(line.strip())}
    if len(versions) != 1:
        raise ReleaseTestError("Appium returned no unambiguous semantic version")
    return versions.pop()


def is_at_least_version(installed: str, minimum: str) -> bool:
    installed_match = SEMVER_PATTERN.fullmatch(installed)
    minimum_match = SEMVER_PATTERN.fullmatch(minimum)
    if not installed_match or not minimum_match or minimum_match.group(4):
        return False
    installed_core = tuple(int(value) for value in installed_match.groups()[:3])
    minimum_core = tuple(int(value) for value in minimum_match.groups()[:3])
    return installed_match.group(4) is None and installed_core >= minimum_core


def validate_appium_driver(server_version: str, drivers: dict, driver: str) -> None:
    if not is_at_least_version(server_version, MINIMUM_APPIUM_VERSION):
        raise ReleaseTestError(f"Appium {MINIMUM_APPIUM_VERSION} or newer is required; found {server_version or 'unknown'}")
    installed = drivers.get(driver) or {}
    if not installed.get("installed"):
        raise ReleaseTestError(f"the Appium {driver} driver is not installed")
    minimum_version = MINIMUM_APPIUM_DRIVERS[driver]
    installed_version = str(installed.get("version") or "")
    if not is_at_least_version(installed_version, minimum_version):
        raise ReleaseTestError(f"Appium {driver} {minimum_version} or newer is required; found {installed_version or 'unknown'}")


def require_appium_driver(root: Path, driver: str) -> None:
    require_appium_port_available()
    if not shutil.which("npm"):
        raise ReleaseTestError("npm is not installed or is not on PATH")
    try:
        server_output = run_streaming([*APPIUM_COMMAND, "--version"], cwd=root, capture=True).stdout
    except ReleaseTestError as error:
        raise ReleaseTestError(f"Appium is not installed in the current npm context or globally\n{error}") from error
    server_version = appium_version_output(server_output)
    drivers = run_json([*APPIUM_COMMAND, "driver", "list", "--installed", "--json"], cwd=root)
    validate_appium_driver(server_version, drivers, driver)
    run_streaming([*APPIUM_COMMAND, "driver", "doctor", driver], cwd=root)


def add_package_arguments(parser: argparse.ArgumentParser) -> None:
    parser.add_argument("--skiasharp", dest="skia", required=True)
    parser.add_argument("--harfbuzzsharp", dest="harfbuzz", required=True)
    parser.add_argument("--package-source", required=True)


def test_args(test_class: str, *, skia: str, harfbuzz: str, package_source: str, properties: dict[str, str] | None = None) -> list[str]:
    args = [
        "dotnet",
        "test",
        TEST_PROJECT,
        f"-p:SkiaSharpVersion={skia}",
        f"-p:HarfBuzzSharpVersion={harfbuzz}",
        f"-p:PackageSource={package_source}",
        "-p:BaseFramework=net10.0",
        "-p:SdkVersion=10.0.400",
        "-p:SdkAllowPrerelease=false",
    ]
    for name, value in (properties or {}).items():
        args.append(f"-p:{name}={value}")
    args.extend(["--", "--filter-class", f"SkiaSharp.Tests.Integration.{test_class}"])
    return args


def run_test(root: Path, test_class: str, args, *, properties: dict[str, str] | None = None) -> None:
    run_streaming(test_args(test_class, skia=args.skia, harfbuzz=args.harfbuzz, package_source=args.package_source, properties=properties), cwd=root)


def execute_item(args, action) -> int:
    started = time.monotonic()
    print(
        f"[release-test] item started: {args.command}; SkiaSharp={args.skia}; " f"HarfBuzzSharp={args.harfbuzz}; PackageSource={args.package_source}",
        flush=True,
    )
    try:
        action(repository_root(), args)
    except ReleaseTestError as error:
        elapsed = display_duration(time.monotonic() - started)
        summary = str(error).splitlines()[0]
        print(f"[release-test] item failed after {elapsed}: {args.command}: {summary}", file=sys.stderr, flush=True)
        print(f"ERROR: {error}", file=sys.stderr)
        return 1
    elapsed = display_duration(time.monotonic() - started)
    print(f"[release-test] item passed after {elapsed}: {args.command}", flush=True)
    return 0
