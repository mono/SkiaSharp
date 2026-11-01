#!/usr/bin/env python3
"""Shared log and exit-record handling for the focused validation scripts."""

from __future__ import annotations

import argparse
from datetime import datetime, timezone
import os
from pathlib import Path
import shlex
import subprocess
import sys


ARTIFACTS = {
    "native": ("native-build.log", "native-build-exit-code.txt"),
    "managed": ("managed-build.log", "managed-build-exit-code.txt"),
    "tests": ("test-output.txt", "test-exit-code.txt"),
}


def run_validation(
    stage: str, parser: argparse.ArgumentParser, args: argparse.Namespace, commands: list[list[str]],
) -> int:
    repo_root, artifact_dir = validation_paths(parser, args)
    stages = list(ARTIFACTS)
    invalidated = stages[stages.index(stage):]
    artifact_dir.mkdir(parents=True, exist_ok=True)
    for name in invalidated:
        (artifact_dir / ARTIFACTS[name][1]).unlink(missing_ok=True)

    log_name, receipt_name = ARTIFACTS[stage]
    log_path = artifact_dir / log_name
    receipt_path = artifact_dir / receipt_name
    if log_path.exists():
        timestamp = datetime.now(timezone.utc).strftime("%Y%m%dT%H%M%S%fZ")
        log_path.rename(artifact_dir / f"{log_path.stem}-attempt-{timestamp}{log_path.suffix}")

    print(f"Combined output: {log_path}", flush=True)
    with log_path.open("wb") as log:
        for command in commands:
            log.write(f"$ {shlex.join(command)}\n".encode("utf-8"))
            log.flush()
            try:
                result = subprocess.run(command, cwd=repo_root, stdout=log, stderr=subprocess.STDOUT, check=False)
                # Match the shell convention for a child terminated by a signal.
                exit_code = result.returncode if result.returncode >= 0 else 128 - result.returncode
            except OSError as error:
                exit_code = 127 if isinstance(error, FileNotFoundError) else 126
                message = f"Unable to launch {command[0]}: {error}"
                log.write(f"{message}\n".encode("utf-8"))
                print(message, file=sys.stderr)
            if exit_code:
                break
        log.write(f"\nValidation exited {exit_code}\n".encode("utf-8"))

    receipt_path.write_text(f"{exit_code}\n", encoding="utf-8")
    print(f"Validation exited {exit_code}; receipt: {receipt_path}", flush=True)
    return exit_code


def argument_parser(description: str) -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(description=description)
    parser.add_argument("--repo-root", type=Path)
    parser.add_argument("--artifact-dir", type=Path)
    return parser


def validation_paths(parser: argparse.ArgumentParser, args: argparse.Namespace) -> tuple[Path, Path]:
    artifact_dir = (
        args.artifact_dir or os.environ.get("SKIA_SYNC_ARTIFACT_DIR") or os.environ.get("ARTIFACT_DIR")
        or Path(os.environ.get("TMPDIR", "/tmp")) / "skia-sync-agent"
    )

    repo_root = args.repo_root.resolve() if args.repo_root else Path(__file__).resolve().parents[4]
    return repo_root, Path(artifact_dir).resolve()
