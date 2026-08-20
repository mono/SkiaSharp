#!/usr/bin/env python3
"""Resolve the exact tested release handoff for publication."""

from __future__ import annotations

import argparse
import json
from pathlib import Path
import sys

import release_publish as publish


class DetectionError(RuntimeError):
    """The tested release handoff could not be resolved safely."""


def repository_root() -> Path:
    return publish.GitRepository.discover().root


def detect(root: Path, target: str) -> dict:
    status = publish.status_report(root, target)
    release_branch = status.get("branch") or ""
    release = publish.ReleaseVersion.parse(release_branch)
    if status.get("nextAction") != "start-release-testing":
        raise DetectionError(
            f"release-status is not ready: {status.get('nextAction')}"
        )
    build = status.get("buildRun") or {}
    tests = status.get("testsRun") or {}
    bar = status.get("barBuild") or {}
    versions = status.get("packageVersions") or {}
    source_sha = status.get("commit")
    build_id = build.get("runId")
    tests_id = tests.get("runId")
    bar_id = bar.get("id")
    if not source_sha or not build_id or not tests_id or not bar_id:
        raise DetectionError("release-status returned an incomplete run handoff")
    try:
        publish.validate_status_handoff(
            status,
            release,
            expected_sha=source_sha,
            expected_build_run=build_id,
            expected_tests_run=tests_id,
            expected_bar_build=bar_id,
        )
    except publish.PublishError as error:
        raise DetectionError(str(error)) from error
    common = [
        release_branch,
        "--expect-source-sha",
        source_sha,
        "--expect-build-run",
        str(build_id),
        "--expect-tests-run",
        str(tests_id),
        "--expect-bar-build",
        str(bar_id),
    ]
    push_command = [
        sys.executable,
        ".agents/skills/release-publish/scripts/push-release-packages.py",
        *common,
        "--dry-run",
    ]
    draft_command = [
        sys.executable,
        ".agents/skills/release-publish/scripts/create-release-draft.py",
        *common,
        "--dry-run",
    ]
    return {
        "schemaVersion": 1,
        "input": target,
        "releaseBranch": release_branch,
        "sourceSha": source_sha,
        "buildRunId": build_id,
        "testsRunId": tests_id,
        "barBuildId": bar_id,
        "barAssets": bar.get("assets"),
        "buildNumber": build.get("buildNumber"),
        "testPackages": versions["test"],
        "publicPackages": versions["public"],
        "warnings": status.get("warnings") or [],
        "pushAuditCommand": publish.shell_command(push_command),
        "draftAuditCommand": publish.shell_command(draft_command),
        "nextAction": "audit-package-publication",
    }


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("release_branch_or_commit")
    args = parser.parse_args()
    try:
        print(
            json.dumps(
                detect(repository_root(), args.release_branch_or_commit),
                indent=2,
            )
        )
    except (DetectionError, publish.PublishError) as error:
        print(f"ERROR: {error}", file=sys.stderr)
        return 1
    return 0


if __name__ == "__main__":
    sys.exit(main())
