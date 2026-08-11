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
    publish.ReleaseVersion.parse(release_branch)
    if status.get("nextAction") != "start-release-testing":
        raise DetectionError(
            f"release-status is not ready: {status.get('nextAction')}"
        )
    managed = status.get("managedRun") or {}
    tests = status.get("testsRun") or {}
    versions = status.get("packageVersions") or {}
    source_sha = status.get("commit")
    managed_id = managed.get("runId")
    tests_id = tests.get("runId")
    if not source_sha or not managed_id or not tests_id:
        raise DetectionError("release-status returned an incomplete run handoff")
    if not versions.get("test") or not versions.get("public"):
        raise DetectionError("release-status returned no package versions")
    common = [
        release_branch,
        "--expect-source-sha",
        source_sha,
        "--expect-managed-run",
        str(managed_id),
        "--expect-tests-run",
        str(tests_id),
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
        "managedRunId": managed_id,
        "testsRunId": tests_id,
        "buildNumber": managed.get("buildNumber"),
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
