#!/usr/bin/env python3
"""Trace the SkiaSharp release pipeline chain for a given branch or SHA.

Usage:
    pipeline-status.py <branch-or-sha>

Examples:
    pipeline-status.py release/3.119.4
    pipeline-status.py f568ac94dd7
"""

import json
import locale
import os
import re
import shutil
import subprocess
import sys

ORG = "https://devdiv.visualstudio.com"
PROJECT = "DevDiv"

PIPELINES = [
    {"name": "SkiaSharp-Native", "id": 26493, "desc": "native binaries"},
    {"name": "SkiaSharp", "id": 10789, "desc": "managed build, signing & publishing"},
    {"name": "SkiaSharp-Tests", "id": 15756, "desc": "device & unit tests"},
]

STATUS_MARKERS = {
    "succeeded": "[OK]",
    "partiallySucceeded": "[WARN]",
    "failed": "[FAIL]",
    "canceled": "[FAIL]",
    "inProgress": "[RUNNING]",
    "notStarted": "[WAITING]",
    "unknown": "[WARN]",
}


def ascii_safe(value: object) -> str:
    """Escape text so it is printable on every console encoding."""
    return str(value).encode("unicode_escape").decode("ascii")


def emit(value: object = "") -> None:
    """Print one line through the single ASCII-safe output path."""
    print(ascii_safe(value))


def cli_command(executable: str, args: list[str]) -> list[str]:
    """Resolve the platform's official CLI launcher."""
    resolved = shutil.which(executable)
    if not resolved:
        raise FileNotFoundError(f"Required CLI '{executable}' was not found on PATH.")
    return [resolved, *args]


def decode_subprocess_output(value: bytes | str | None, source: str) -> str:
    """Decode subprocess bytes without silently replacing characters."""
    if value is None:
        return ""
    if isinstance(value, str):
        return value

    encodings = ["utf-8-sig"]
    try:
        encodings.append(locale.getencoding())
    except AttributeError:
        encodings.append(locale.getpreferredencoding(False))
    if os.name == "nt":
        encodings.append("mbcs")

    attempted = []
    for encoding in encodings:
        normalized = encoding.lower()
        if normalized in attempted:
            continue
        attempted.append(normalized)
        try:
            return value.decode(encoding, errors="strict")
        except (LookupError, UnicodeDecodeError):
            continue

    raise RuntimeError(
        f"Could not decode {source} using {', '.join(attempted)}."
    )


def az(args: list[str], timeout: int = 30) -> str:
    command = cli_command("az", args)
    try:
        result = subprocess.run(
            command,
            capture_output=True,
            timeout=timeout,
            check=True,
        )
    except subprocess.CalledProcessError as error:
        detail = (
            decode_subprocess_output(error.stderr, "Azure CLI stderr").strip()
            or "no error output"
        )
        raise RuntimeError(
            f"Azure CLI failed with exit code {error.returncode}: {ascii_safe(detail)}"
        ) from error
    except subprocess.TimeoutExpired as error:
        raise RuntimeError(
            f"Azure CLI timed out after {timeout} seconds."
        ) from error

    output = decode_subprocess_output(result.stdout, "Azure CLI stdout").strip()
    if not output:
        detail = (
            decode_subprocess_output(result.stderr, "Azure CLI stderr").strip()
            or "no error output"
        )
        raise RuntimeError(
            f"Azure CLI returned no output: {ascii_safe(detail)}"
        )
    return output


def get_runs(pipeline_id: int, branch: str) -> list[dict]:
    out = az([
        "pipelines", "runs", "list",
        "--pipeline-ids", str(pipeline_id),
        "--branch", branch,
        "--org", ORG, "--project", PROJECT,
        "--query", "[].{id:id, status:status, result:result, buildNumber:buildNumber}",
        "--top", "5", "-o", "json",
    ])
    return json.loads(out) if out else []


def get_trigger_info(build_id: int) -> dict:
    out = az([
        "pipelines", "runs", "show",
        "--id", str(build_id),
        "--org", ORG, "--project", PROJECT,
        "--query", "triggerInfo", "-o", "json",
    ])
    return json.loads(out) if out else {}


def get_timeline(build_id: int) -> list[dict]:
    """Fetch the build timeline (stages, jobs, tasks) from the ADO REST API."""
    out = az([
        "devops", "invoke",
        "--area", "build",
        "--resource", "timeline",
        "--route-parameters", f"project={PROJECT}", f"buildId={build_id}",
        "--org", ORG,
        "--api-version", "7.0",
        "-o", "json",
    ], timeout=60)
    if not out:
        return []
    data = json.loads(out)
    return data.get("records", [])


def format_job_summary(records: list[dict], cont: str) -> None:
    """Print a summary of job-level status from timeline records."""
    # Filter to only Job-type records (not Stage or Task)
    jobs = [r for r in records if r.get("type") == "Job"]

    if not jobs:
        return

    completed = []
    failed = []
    running = []
    pending = []

    for job in jobs:
        name = job.get("name", "Unknown")
        state = job.get("state", "")
        result = job.get("result", "")

        if state == "completed":
            if result in ("failed", "canceled"):
                failed.append(name)
            else:
                completed.append(name)
        elif state == "inProgress":
            running.append(name)
        else:
            pending.append(name)

    # Build the summary line
    parts = []
    if completed:
        parts.append(f"{len(completed)} {STATUS_MARKERS['succeeded']} completed")
    if failed:
        parts.append(f"{len(failed)} {STATUS_MARKERS['failed']} failed")
    if running:
        parts.append(f"{len(running)} {STATUS_MARKERS['inProgress']} running")
    if pending:
        parts.append(f"{len(pending)} {STATUS_MARKERS['notStarted']} pending")

    emit(cont)
    emit(f"{cont} Jobs: {' | '.join(parts)}")

    if failed:
        names = ", ".join(failed[:8])
        suffix = f", ... (+{len(failed) - 8} more)" if len(failed) > 8 else ""
        emit(f"{cont} Failed: {names}{suffix}")

    if running:
        names = ", ".join(running[:8])
        suffix = f", ... (+{len(running) - 8} more)" if len(running) > 8 else ""
        emit(f"{cont} Running: {names}{suffix}")

    if pending:
        names = ", ".join(pending[:8])
        suffix = f", ... (+{len(pending) - 8} more)" if len(pending) > 8 else ""
        emit(f"{cont} Pending: {names}{suffix}")


def marker_for(run: dict) -> str:
    if run["status"] == "completed":
        return STATUS_MARKERS.get(run.get("result", ""), STATUS_MARKERS["unknown"])
    return STATUS_MARKERS.get(run["status"], STATUS_MARKERS["notStarted"])


def resolve_branch(ref: str) -> str:
    if re.match(r"^[0-9a-f]{7,40}$", ref):
        try:
            result = subprocess.run(
                ["git", "branch", "-r", "--contains", ref],
                capture_output=True,
                check=True,
            )
        except subprocess.CalledProcessError as error:
            detail = (
                decode_subprocess_output(error.stderr, "Git stderr").strip()
                or "no error output"
            )
            raise RuntimeError(
                f"Git failed with exit code {error.returncode}: {ascii_safe(detail)}"
            ) from error
        stdout = decode_subprocess_output(result.stdout, "Git stdout")
        for line in stdout.splitlines():
            m = re.search(r"origin/(release/\S+)", line)
            if m:
                emit(f"Resolved SHA {ref} -> branch: {m.group(1)}")
                return m.group(1)
        sys.exit(
            f"ERROR: No release branch found containing SHA {ascii_safe(ref)}"
        )
    return ref


def main():
    if len(sys.argv) < 2:
        sys.exit("Usage: pipeline-status.py <branch-or-sha>")

    branch = resolve_branch(sys.argv[1])

    emit()
    emit("=" * 63)
    emit(f" Pipeline Chain Status: {branch}")
    emit("=" * 63)
    emit()

    all_runs: list[tuple[dict, list[dict]]] = []
    links: list[str] = []

    for i, pipe in enumerate(PIPELINES):
        prefix = "+-"
        cont = "| "

        emit(f"{prefix} {pipe['name']} (ID {pipe['id']}) - {pipe['desc']}")

        runs = get_runs(pipe["id"], branch)
        all_runs.append((pipe, runs))

        if not runs:
            emit(f"{cont} No runs found - not yet triggered")
        else:
            for r in runs:
                emit(
                    f"{cont} {marker_for(r)} id={r['id']!s:<10}  "
                    f"{r['status']!s:<12}  "
                    f"{(r.get('result') or 'pending')!s:<20}  "
                    f"{r['buildNumber']}"
                )

            # Show job-level details for in-progress builds
            latest_run = runs[0]
            if latest_run["status"] == "inProgress":
                records = get_timeline(latest_run["id"])
                if records:
                    format_job_summary(records, cont)

            # Show trigger info (skip for first pipeline - it has no upstream)
            if i > 0:
                trigger = get_trigger_info(runs[0]["id"])
                if trigger and trigger.get("source"):
                    src = trigger.get("source", "?")
                    pid = trigger.get("pipelineId", "?")
                    emit(f"{cont} -> triggered by {src} build {pid}")
            links.append(f"  {pipe['name']}: https://devdiv.visualstudio.com/DevDiv/_build/results?buildId={runs[0]['id']}")

        emit(cont if i < len(PIPELINES) - 1 else "")

    emit("=" * 63)
    emit()

    # Summary
    latest = [(runs[0] if runs else None) for _, runs in all_runs]
    if all(r and r["status"] == "completed" and r.get("result") in ("succeeded", "partiallySucceeded") for r in latest):
        emit("Summary: [OK] All pipelines completed. Packages should be on internal feed.")
    elif latest[0] and not latest[1]:
        emit("Summary: [WAITING] Waiting for SkiaSharp to be triggered by SkiaSharp-Native.")
    elif not latest[0]:
        emit("Summary: [WAITING] No native build found yet.")
    else:
        emit("Summary: [RUNNING] Pipeline chain in progress or has failures.")

    if links:
        emit()
        emit("ADO Links:")
        for link in links:
            emit(link)


if __name__ == "__main__":
    main()
