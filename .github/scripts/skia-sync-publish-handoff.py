#!/usr/bin/env python3
"""Transfer original sync commit graphs to the credential-isolated publishing job."""

import argparse
import json
import os
from pathlib import Path
import shutil
import subprocess


ENV_KEYS = (
    "SKIA_SYNC_CURRENT",
    "SKIA_SYNC_TARGET",
    "SKIA_SYNC_UPSTREAM_REF",
    "SKIA_SYNC_IS_RELEASE",
    "SKIA_SYNC_BASE_BRANCH",
    "SKIA_SYNC_SKIA_BASE_BRANCH",
    "SKIA_SYNC_HEAD_BRANCH",
    "SKIA_SYNC_PARENT_BASE_SHA",
    "SKIA_SYNC_SKIA_BASE_SHA",
    "SKIA_SYNC_SKIA_BASE_UPSTREAM_SHA",
    "SKIA_SYNC_BASE_UPSTREAM_SHA",
    "SKIA_SYNC_TARGET_UPSTREAM_SHA",
)
REPORTS = (
    "skia-sync-skia-summary.md",
    "skia-sync-skiasharp-summary.md",
    "skia-breaking-change-analysis.md",
    "skia-validation-review.md",
    "skia-dependency-decisions.md",
    "skia-dependency-changes.json",
    "skia-fork-patch-audit.md",
    "native-build.log",
    "native-build-exit-code.txt",
    "managed-build.log",
    "managed-build-exit-code.txt",
    "test-output.txt",
    "test-exit-code.txt",
)


def git(repo, *args):
    subprocess.run(["git", "-C", str(repo), *args], check=True)


def contains(repo, ancestor, descendant):
    return subprocess.run(
        ["git", "-C", str(repo), "merge-base", "--is-ancestor", ancestor, descendant]
    ).returncode == 0


def validate_environment(environment):
    if not isinstance(environment, dict) or set(environment) != set(ENV_KEYS):
        raise ValueError("Sync handoff has unexpected or missing environment keys")
    for key, value in environment.items():
        if not isinstance(value, str) or not value or any(c in value for c in "\r\n\0"):
            raise ValueError(f"Sync handoff has an invalid value for {key}")


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("mode", choices=("pack", "restore"))
    parser.add_argument("directory", type=Path)
    args = parser.parse_args()
    directory = args.directory.resolve()
    workspace = Path(os.environ["GITHUB_WORKSPACE"])
    skia = workspace / "externals/skia"

    if args.mode == "pack":
        environment = {key: os.environ[key] for key in ENV_KEYS}
        validate_environment(environment)
        directory.mkdir(parents=True, exist_ok=False)
        branch = environment["SKIA_SYNC_HEAD_BRANCH"]
        for name, repo, base in (
            ("parent", workspace, environment["SKIA_SYNC_PARENT_BASE_SHA"]),
            ("skia", skia, environment["SKIA_SYNC_SKIA_BASE_SHA"]),
        ):
            git(repo, "bundle", "create", str(directory / f"{name}.bundle"),
                f"refs/heads/{branch}", f"^{base}")
        reports = directory / "reports"
        reports.mkdir()
        source = Path(os.environ["SKIA_SYNC_ARTIFACT_DIR"])
        for name in REPORTS:
            path = source / name
            if path.is_symlink() or not path.is_file() or path.stat().st_size == 0:
                raise ValueError(f"Required sync report is missing, empty, or a symlink: {path}")
            if name.endswith("-exit-code.txt") and path.read_text().strip() != "0":
                raise ValueError(f"Sync validation did not pass: {path}")
            shutil.copyfile(path, reports / name)
        (directory / "environment.json").write_text(json.dumps(environment), encoding="utf-8")
        return

    environment = json.loads((directory / "environment.json").read_text(encoding="utf-8"))
    validate_environment(environment)
    for key in ("HEAD_BRANCH", "BASE_BRANCH", "SKIA_BASE_BRANCH"):
        if environment[f"SKIA_SYNC_{key}"] != os.environ[f"EXPECTED_{key}"]:
            raise ValueError(f"Sync handoff {key} does not match workflow resolution")
    branch = environment["SKIA_SYNC_HEAD_BRANCH"]
    git(workspace, "check-ref-format", "--branch", branch)
    for name, repo, base_key in (
        ("parent", workspace, "SKIA_SYNC_BASE_BRANCH"),
        ("skia", skia, "SKIA_SYNC_SKIA_BASE_BRANCH"),
    ):
        base = environment[base_key]
        git(repo, "fetch", "--no-tags", "--no-recurse-submodules", "origin",
            f"+refs/heads/{base}:refs/remotes/origin/{base}")
        bundle = str(directory / f"{name}.bundle")
        git(repo, "bundle", "verify", bundle)
        git(repo, "fetch", "--no-tags", "--no-recurse-submodules",
            bundle, f"refs/heads/{branch}:refs/heads/{branch}")
        # A base that advanced during the run would be silently dropped from the PR.
        if not contains(repo, f"refs/remotes/origin/{base}", f"refs/heads/{branch}"):
            raise ValueError(
                f"{name} base '{base}' advanced since the sync started; "
                "the sync head does not contain its live tip. Rerun the sync."
            )
    environment.update(
        SKIA_SYNC_ARTIFACT_DIR=str(directory / "reports"),
        SKIA_SYNC_SKILL_DIR=str(workspace / ".agents/skills/update-skia"),
    )
    with Path(os.environ["GITHUB_ENV"]).open("a", encoding="utf-8") as env_file:
        for key, value in environment.items():
            env_file.write(f"{key}={value}\n")


if __name__ == "__main__":
    main()
