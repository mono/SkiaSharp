import argparse
import datetime
import json
import os
from pathlib import Path
import re
import signal
import subprocess
import sys
import time


APP_NAME = "SkiaSharp.Tests.Devices"
APP_BUNDLE = "com.companyname.SkiaSharpTests"
COMMAND_LIMIT = 8 * 1024 * 1024
REPORT_LIMIT = 5 * 1024 * 1024
REPORT_COUNT = 8


def capture_command(output, name, command):
    import resource

    def limit_output():
        resource.setrlimit(resource.RLIMIT_FSIZE, (COMMAND_LIMIT, COMMAND_LIMIT))

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
    args = parser.parse_args()
    if sys.platform != "darwin":
        parser.error("iOS diagnostic capture requires macOS.")
    if not re.fullmatch(r"[0-9A-Fa-f]{8}(-[0-9A-Fa-f]{4}){3}-[0-9A-Fa-f]{12}", args.device):
        parser.error("Expected a simulator UDID.")

    output = args.output / "diagnostics"
    output.mkdir(parents=True, exist_ok=True)
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
    (output / "capture.json").write_text(json.dumps(metadata, indent=2) + "\n", encoding="utf-8")
    print(f"iOS diagnostics: {len(reports)} matching crash reports; app PIDs {pids}; output {output}")
    if not reports:
        print("WARNING: No matching OS crash report found; scoped unified logs retained.", file=sys.stderr)
    for error in errors + [c["error"] for c in commands if "error" in c]:
        print("WARNING: " + error, file=sys.stderr)
    return 1 if errors or any("error" in c for c in commands) else 0


if __name__ == "__main__":
    sys.exit(main())
