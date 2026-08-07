#!/usr/bin/env python3
"""Shared process and JSON helpers for release-testing scripts."""

from __future__ import annotations

import json
from pathlib import Path
import subprocess


ANDROID_MIN_VERSION = "26"
ANDROID_MAX_VERSION = "37.1"
IOS_MIN_VERSION = "18.6"
IOS_MAX_VERSION = "26.5"


class ReleaseTestError(RuntimeError):
    """A release-testing script could not complete safely."""


def run_checked(
    args: list[str],
    *,
    cwd: Path | None = None,
    timeout: int | None = None,
) -> subprocess.CompletedProcess[str]:
    try:
        result = subprocess.run(
            args,
            cwd=cwd,
            capture_output=True,
            text=True,
            timeout=timeout,
        )
    except FileNotFoundError as error:
        raise ReleaseTestError(f"{args[0]} was not found on PATH") from error
    except subprocess.TimeoutExpired as error:
        raise ReleaseTestError(
            f"command timed out after {timeout}s: {' '.join(args)}"
        ) from error
    if result.returncode != 0:
        detail = result.stderr.strip() or result.stdout.strip() or "no output"
        raise ReleaseTestError(
            f"command failed ({result.returncode}): {' '.join(args)}\n{detail}"
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
    raise ReleaseTestError("command returned no valid JSON")


def repository_root(*, cwd: Path | None = None) -> Path:
    return Path(
        run_checked(
            ["git", "rev-parse", "--show-toplevel"],
            cwd=cwd,
            timeout=30,
        ).stdout.strip()
    )
