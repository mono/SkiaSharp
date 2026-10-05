import argparse
import datetime
import hashlib
import json
import os
from pathlib import Path
import platform
import plistlib
import re
import signal
import struct
import subprocess
import sys
import time


APP_NAME = "SkiaSharp.Tests.Devices"
APP_BUNDLE = "com.companyname.SkiaSharpTests"
COMMAND_LIMIT = 8 * 1024 * 1024
REPORT_LIMIT = 5 * 1024 * 1024
REPORT_COUNT = 8


def limit_output():
    import resource

    resource.setrlimit(resource.RLIMIT_FSIZE, (COMMAND_LIMIT, COMMAND_LIMIT))


def capture_command(output, name, command):
    result = {"command": command}
    try:
        with (output / (name + ".json")).open("wb") as stdout:
            with (output / (name + ".stderr.txt")).open("wb") as stderr:
                with subprocess.Popen(
                    command, stdout=stdout, stderr=stderr,
                    preexec_fn=limit_output, start_new_session=True,
                ) as process:
                    try:
                        exit_code = process.wait(timeout=25)
                    except subprocess.TimeoutExpired:
                        os.killpg(process.pid, signal.SIGKILL)
                        process.wait()
                        raise
        result["exit_code"] = exit_code
        if exit_code:
            result["error"] = "Diagnostic command failed; see its stderr file."
    except (OSError, subprocess.TimeoutExpired) as error:
        result["error"] = str(error)
    return result


def simulator_identity(home, device):
    with (home / "Library/Developer/CoreSimulator/Devices" / device / "device.plist").open("rb") as source:
        simulator = plistlib.load(source)
    return {"runtime": simulator["runtime"], "device_type": simulator["deviceType"]}


def stream_logs(output, device):
    command = [
        "/usr/bin/xcrun", "simctl", "spawn", device, "log", "stream",
        "--style", "ndjson", "--level", "debug",
        "--predicate", f'process == "{APP_NAME}"',
    ]
    metadata = {"command": command, "device": device, "start": time.time(), "limit_seconds": 1800}
    stop = output / "stream-stop"
    try:
        with (output / "stream.jsonl").open("wb") as stdout:
            with (output / "stream.stderr.txt").open("wb") as stderr:
                with subprocess.Popen(
                    command, stdout=stdout, stderr=stderr,
                    preexec_fn=limit_output, start_new_session=True,
                ) as process:
                    metadata["collector_pid"] = process.pid
                    try:
                        time.sleep(1)
                        if process.poll() is not None:
                            raise RuntimeError("Scoped simulator log stream exited before readiness.")
                        (output / "stream-ready.json").write_text(
                            json.dumps(metadata, indent=2) + "\n", encoding="utf-8",
                        )
                        while process.poll() is None and not stop.exists():
                            if time.time() - metadata["start"] >= metadata["limit_seconds"]:
                                raise TimeoutError("Scoped log stream reached its 30-minute diagnostic limit.")
                            time.sleep(0.2)
                        if process.poll() is not None and not stop.exists():
                            raise RuntimeError("Scoped log stream exited before the test session ended.")
                    finally:
                        if process.poll() is None:
                            os.killpg(process.pid, signal.SIGTERM)
                            try:
                                process.wait(timeout=5)
                            except subprocess.TimeoutExpired:
                                os.killpg(process.pid, signal.SIGKILL)
                                process.wait()
                        metadata["exit_code"] = process.returncode
    except (OSError, RuntimeError, TimeoutError) as error:
        metadata["error"] = str(error)
        print("ERROR: " + str(error), file=sys.stderr)
    metadata["end"] = time.time()
    (output / "stream-capture.json").write_text(json.dumps(metadata, indent=2) + "\n", encoding="utf-8")
    return 1 if "error" in metadata else 0


def causal_provenance(project, home, device):
    root = project.parent
    apps = list((root / "bin/Debug/net10.0-ios/iossimulator-x64").glob("*.app"))
    if len(apps) != 1:
        raise ValueError("Expected exactly one existing x64 iOS test app; found " + str(len(apps)))
    app = apps[0]
    with (app / "Info.plist").open("rb") as source:
        info = plistlib.load(source)
    if info.get("CFBundleIdentifier") != APP_BUNDLE or info.get("CFBundleExecutable") != APP_NAME:
        raise ValueError("Built app identity does not match the owned iOS test app.")
    executable = app / APP_NAME
    with executable.open("rb") as source:
        magic, cpu = struct.unpack("<II", source.read(8))
    if magic != 0xFEEDFACF or cpu != 0x01000007:
        raise ValueError("Diagnostic app is not a thin x86_64 Mach-O executable.")
    simulator = simulator_identity(home, device)
    assets = json.loads((root / "obj/project.assets.json").read_text(encoding="utf-8"))
    packages = {
        name: {"sha512": value.get("sha512"), "path": value.get("path")}
        for name, value in assets["libraries"].items()
        if name.startswith(("DeviceRunners.", "Microsoft.Maui.", "Microsoft.iOS."))
    }
    binaries = {}
    for path in app.rglob("*"):
        if path.is_file() and (path.name == APP_NAME or path.name.startswith(("libSkiaSharp", "libHarfBuzzSharp", "libmonosgen"))):
            with path.open("rb") as source:
                binaries[str(path.relative_to(app))] = hashlib.file_digest(source, "sha256").hexdigest()
    return {
        "diagnostic_only": True,
        "normal_gate_evidence": False,
        "host_architecture": platform.machine(),
        "app": str(app), "app_architecture": "x86_64", "binary_sha256": binaries,
        **simulator,
        "app_build": {key: info.get(key) for key in (
            "DTSDKName", "DTSDKBuild", "DTPlatformVersion", "DTPlatformBuild", "DTXcodeBuild",
        )},
        "packages": packages,
        "ci": {key: os.environ.get(key) for key in (
            "BUILD_BUILDID", "BUILD_BUILDNUMBER", "BUILD_SOURCEVERSION", "BUILD_SOURCEBRANCH",
            "RESOURCES_PIPELINE_SKIASHARP_RUNID", "RESOURCES_PIPELINE_SKIASHARP_RUNNAME",
            "RESOURCES_PIPELINE_SKIASHARP_SOURCECOMMIT", "DOWNLOAD_BUILD_ID",
        )},
        "requested_app_environment": {key: os.environ.get("SIMCTL_CHILD_" + key) for key in (
            "NSZombieEnabled", "MONO_LOG_LEVEL", "MONO_LOG_MASK",
        )},
        "environment_verification": "Requested through inherited simctl launch environment; no app-side getenv verification.",
    }


def collect_reports(output, home, device, start, pids):
    simulator = home / "Library/Developer/CoreSimulator/Devices" / device / "data/Library/Logs"
    roots = [
        home / "Library/Logs/DiagnosticReports",
        home / "Library/Logs/DiagnosticReports/Retired",
        simulator / "CrashReporter",
        simulator / "DiagnosticReports",
    ]
    copied = []
    errors = []
    for index, root in enumerate(roots):
        try:
            candidates = sorted(root.glob(APP_NAME + "*"), key=lambda p: p.stat().st_mtime, reverse=True)
            for source in candidates[:40]:
                if len(copied) >= REPORT_COUNT:
                    return copied, errors
                if source.is_symlink() or not source.is_file() or source.suffix not in (".ips", ".crash"):
                    continue
                if not start <= source.stat().st_mtime <= time.time():
                    continue
                if source.stat().st_size > REPORT_LIMIT:
                    errors.append("Report exceeds 5 MiB limit: " + source.name)
                    continue
                with source.open("rb") as report:
                    data = report.read(REPORT_LIMIT + 1)
                if len(data) > REPORT_LIMIT:
                    errors.append("Report grew beyond 5 MiB limit: " + source.name)
                    continue
                text = data.decode("utf-8", errors="replace")
                matches_pid = any(
                    re.search(r'"pid"\s*:\s*' + str(pid) + r'\b', text)
                    or re.search(r"Process:\s*" + re.escape(APP_NAME) + r"\s+\[" + str(pid) + r"\]", text)
                    for pid in pids
                )
                if device not in text and not matches_pid:
                    continue
                destination = output / ("report-" + str(index) + "-" + source.name)
                destination.write_bytes(data)
                copied.append(destination.name)
        except OSError as error:
            errors.append(str(error))
    return copied, errors


def main():
    parser = argparse.ArgumentParser(description="Capture bounded, app-specific iOS failure diagnostics.")
    parser.add_argument("--device", required=True)
    parser.add_argument("--start", type=int, required=True)
    parser.add_argument("--output", type=Path, required=True)
    parser.add_argument("--stream", action="store_true")
    parser.add_argument("--causal", action="store_true")
    parser.add_argument("--project", type=Path)
    parser.add_argument("--expected-runtime")
    args = parser.parse_args()
    if sys.platform != "darwin":
        parser.error("iOS diagnostic capture requires macOS.")
    if not re.fullmatch(r"[0-9A-Fa-f]{8}(-[0-9A-Fa-f]{4}){3}-[0-9A-Fa-f]{12}", args.device):
        parser.error("Expected a simulator UDID.")

    output = args.output / "diagnostics"
    output.mkdir(parents=True, exist_ok=True)
    if args.stream:
        try:
            simulator = simulator_identity(Path.home(), args.device)
            if args.expected_runtime and simulator["runtime"] != args.expected_runtime:
                raise ValueError("Selected runtime differs from the approved crash environment: " + simulator["runtime"])
        except (OSError, KeyError, ValueError) as error:
            (output / "stream-capture.json").write_text(
                json.dumps({"device": args.device, "error": str(error)}, indent=2) + "\n", encoding="utf-8",
            )
            print("ERROR: " + str(error), file=sys.stderr)
            return 1
        return stream_logs(output, args.device)
    start = datetime.datetime.fromtimestamp(args.start).strftime("%Y-%m-%d %H:%M:%S")
    end = datetime.datetime.now().strftime("%Y-%m-%d %H:%M:%S")
    device_log = args.output / "ios-device-log.txt"
    pids = []
    if device_log.exists():
        pids = sorted(set(map(int, re.findall(
            re.escape(APP_NAME) + r"\[(\d+):", device_log.read_text(encoding="utf-8"),
        ))))
    identity = (
        f'(eventMessage CONTAINS[c] "{APP_NAME}" OR '
        f'eventMessage CONTAINS[c] "{APP_BUNDLE}" OR '
        f'eventMessage CONTAINS[c] "{args.device}")'
    )
    system = (
        '(process == "ReportCrash" OR process == "runningboardd" OR '
        'process == "SpringBoard" OR process == "launchd" OR '
        'process == "CoreSimulatorService" OR process == "Simulator")'
    )
    common = ["log", "show", "--style", "json", "--start", start, "--end", end, "--info", "--debug", "--predicate"]
    commands = [
        capture_command(output, "simulator", [
            "/usr/bin/xcrun", "simctl", "spawn", args.device, *common,
            f'process == "{APP_NAME}" OR ({system} AND {identity})',
        ]),
        capture_command(output, "host", [
            "/usr/bin/log", *common[1:], f"{system} AND {identity}",
        ]),
    ]
    reports, errors = collect_reports(output, Path.home(), args.device, args.start, pids)
    metadata = {
        "device": args.device, "app": APP_NAME, "bundle": APP_BUNDLE,
        "start": start, "end": end, "app_pids": pids,
        "commands": commands, "reports": reports, "errors": errors,
        "app_stdout_stderr": "Unavailable: DeviceRunners owns app launch; no relaunch or runner change.",
    }
    if args.causal:
        try:
            if not args.project:
                raise ValueError("Causal capture requires the exact test project path.")
            metadata["causal"] = causal_provenance(args.project, Path.home(), args.device)
        except (OSError, ValueError, KeyError, struct.error) as error:
            errors.append("Causal provenance capture failed: " + str(error))
    (output / "capture.json").write_text(json.dumps(metadata, indent=2) + "\n", encoding="utf-8")
    print(f"iOS diagnostics: {len(reports)} matching crash reports; app PIDs {pids}; output {output}")
    if not reports:
        print("WARNING: No matching OS crash report found; scoped unified logs retained.", file=sys.stderr)
    for error in errors + [c["error"] for c in commands if "error" in c]:
        print("WARNING: " + error, file=sys.stderr)
    return 1 if errors or any("error" in c for c in commands) else 0


if __name__ == "__main__":
    sys.exit(main())
