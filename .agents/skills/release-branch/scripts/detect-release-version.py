#!/usr/bin/env python3
"""Detect the next preview version from one integration branch."""

import argparse
import json
from pathlib import Path
import re
import subprocess
import sys


VARIABLES_PATH = "scripts/azure-templates-variables.yml"


class DetectionError(RuntimeError):
    pass


def run(args, *, cwd, check=True):
    result = subprocess.run(args, cwd=cwd, capture_output=True, text=True)
    if check and result.returncode != 0:
        detail = result.stderr.strip() or result.stdout.strip() or "no output"
        raise DetectionError(detail)
    return result


def repository_root():
    result = run(
        ["git", "rev-parse", "--show-toplevel"],
        cwd=Path.cwd(),
    )
    return Path(result.stdout.strip())


def normalize_integration_branch(value):
    for prefix in ("refs/remotes/origin/", "refs/heads/", "origin/"):
        if value.startswith(prefix):
            value = value[len(prefix):]
            break
    if value == "main" or re.fullmatch(r"release/\d+\.\d+\.x", value):
        return value
    raise DetectionError(
        "integration branch must be main or release/X.Y.x"
    )


def parse_state(text):
    version = re.search(
        r"^\s*SKIASHARP_VERSION:\s*['\"]?([^'\"\s]+)",
        text,
        re.MULTILINE,
    )
    label = re.search(
        r"^\s*PREVIEW_LABEL:\s*['\"]?([^'\"\r\n]+)",
        text,
        re.MULTILINE,
    )
    if not version or not label:
        raise DetectionError(
            "could not parse SKIASHARP_VERSION and PREVIEW_LABEL"
        )
    return version.group(1), label.group(1).strip()


def calculate_next_preview(version, label, branches):
    if label != "preview.0":
        raise DetectionError(
            f"PREVIEW_LABEL is '{label}', expected 'preview.0'"
        )
    if f"release/{version}" in branches:
        raise DetectionError(f"stable branch release/{version} already exists")
    if any(branch.startswith(f"release/{version}-rc.") for branch in branches):
        raise DetectionError(f"an RC branch for {version} already exists")
    pattern = re.compile(rf"^release/{re.escape(version)}-preview\.(\d+)$")
    iterations = [
        int(match.group(1))
        for branch in branches
        if (match := pattern.fullmatch(branch))
    ]
    return f"{version}-preview.{max(iterations, default=0) + 1}"


def detect(root, integration_branch):
    branch = normalize_integration_branch(integration_branch)
    run(["git", "fetch", "origin", "--prune"], cwd=root)
    ref = f"refs/remotes/origin/{branch}"
    if run(
        ["git", "show-ref", "--verify", "--quiet", ref],
        cwd=root,
        check=False,
    ).returncode != 0:
        raise DetectionError(f"{branch} does not exist on origin")
    variables = run(
        ["git", "show", f"{ref}:{VARIABLES_PATH}"],
        cwd=root,
    ).stdout
    version, label = parse_state(variables)
    branch_output = run(
        [
            "git",
            "for-each-ref",
            "--format=%(refname:strip=3)",
            "refs/remotes/origin/release/",
        ],
        cwd=root,
    ).stdout
    release_version = calculate_next_preview(
        version,
        label,
        branch_output.splitlines(),
    )
    return {
        "integrationBranch": branch,
        "currentVersion": version,
        "previewLabel": label,
        "releaseVersion": release_version,
        "nextCommand": (
            "python3 .agents/skills/release-branch/scripts/"
            f"create-release-branches.py {release_version} --dry-run"
        ),
    }


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("integration_branch")
    args = parser.parse_args()
    try:
        print(
            json.dumps(
                detect(repository_root(), args.integration_branch),
                indent=2,
            )
        )
    except DetectionError as error:
        print(f"ERROR: {error}", file=sys.stderr)
        return 1
    return 0


if __name__ == "__main__":
    sys.exit(main())
