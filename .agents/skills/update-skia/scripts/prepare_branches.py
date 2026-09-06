#!/usr/bin/env python3

"""Create matched SkiaSharp and paired Skia update branches from exact bases."""

import argparse
import os
import subprocess
from pathlib import Path


class PreparationError(RuntimeError):
    pass


def git(repo: Path, *args: str, check: bool = True) -> subprocess.CompletedProcess[str]:
    result = subprocess.run(
        ["git", "-C", str(repo), *args],
        capture_output=True,
        text=True,
    )
    if check and result.returncode != 0:
        detail = result.stderr.strip() or result.stdout.strip()
        raise PreparationError(f"git {' '.join(args)} failed in {repo}: {detail}")
    return result


def git_output(repo: Path, *args: str) -> str:
    return git(repo, *args).stdout.rstrip("\r\n")


def branch_exists(repo: Path, branch: str) -> bool:
    result = git(
        repo,
        "show-ref",
        "--verify",
        "--quiet",
        f"refs/heads/{branch}",
        check=False,
    )
    if result.returncode not in (0, 1):
        raise PreparationError(f"Could not inspect branch {branch} in {repo}.")
    return result.returncode == 0


def require_clean(repo: Path, *, ignore_submodules: bool = False) -> None:
    args = ["status", "--porcelain=v1", "--untracked-files=all"]
    if ignore_submodules:
        args.append("--ignore-submodules=all")
    status = git_output(repo, *args)
    if status:
        raise PreparationError(f"Worktree is not clean: {repo}\n{status}")


def require_ancestor(repo: Path, commit: str, remote_branch: str) -> None:
    result = git(
        repo,
        "merge-base",
        "--is-ancestor",
        commit,
        remote_branch,
        check=False,
    )
    if result.returncode != 0:
        raise PreparationError(f"{commit} is not contained by {remote_branch}.")


def prepare_branches(
    repo_root: Path,
    base_branch: str,
    parent_base_sha: str,
    skia_base_branch: str,
    skia_base_sha: str,
    head_branch: str,
) -> None:
    repo_root = repo_root.resolve()
    skia_root = repo_root / "externals" / "skia"

    git_output(repo_root, "rev-parse", "--show-toplevel")
    git_output(skia_root, "rev-parse", "--show-toplevel")
    git(repo_root, "check-ref-format", "--branch", head_branch)
    git(skia_root, "check-ref-format", "--branch", head_branch)

    require_clean(repo_root, ignore_submodules=True)
    require_clean(skia_root)
    if branch_exists(repo_root, head_branch):
        raise PreparationError(f"Parent branch already exists: {head_branch}")
    if branch_exists(skia_root, head_branch):
        raise PreparationError(f"Skia branch already exists: {head_branch}")

    git(
        repo_root,
        "fetch",
        "--no-tags",
        "origin",
        f"+refs/heads/{base_branch}:refs/remotes/origin/{base_branch}",
    )
    git(
        skia_root,
        "fetch",
        "--no-tags",
        "origin",
        f"+refs/heads/{skia_base_branch}:refs/remotes/origin/{skia_base_branch}",
    )

    parent_base_sha = git_output(
        repo_root, "rev-parse", "--verify", f"{parent_base_sha}^{{commit}}"
    )
    skia_base_sha = git_output(
        skia_root, "rev-parse", "--verify", f"{skia_base_sha}^{{commit}}"
    )
    require_ancestor(repo_root, parent_base_sha, f"origin/{base_branch}")
    require_ancestor(skia_root, skia_base_sha, f"origin/{skia_base_branch}")

    tree_entry = git_output(
        repo_root, "ls-tree", parent_base_sha, "--", "externals/skia"
    ).split()
    if len(tree_entry) < 3 or tree_entry[1] != "commit":
        raise PreparationError(
            f"{parent_base_sha} does not contain the externals/skia submodule."
        )
    if tree_entry[2] != skia_base_sha:
        raise PreparationError(
            "Resolved Skia base does not match the parent gitlink: "
            f"{skia_base_sha} != {tree_entry[2]}"
        )

    git(repo_root, "checkout", "-b", head_branch, parent_base_sha)
    git(repo_root, "submodule", "sync", "--recursive")
    git(repo_root, "submodule", "update", "--init", "--recursive")

    submodule_status = git_output(
        repo_root, "submodule", "status", "--recursive"
    )
    mismatches = [
        line for line in submodule_status.splitlines() if line and line[0] in "-+U"
    ]
    if mismatches:
        raise PreparationError(
            "Submodules do not match the selected parent:\n" + "\n".join(mismatches)
        )

    actual_skia_sha = git_output(skia_root, "rev-parse", "HEAD")
    if actual_skia_sha != skia_base_sha:
        raise PreparationError(
            f"Skia is at {actual_skia_sha}, expected {skia_base_sha}."
        )

    git(skia_root, "checkout", "-b", head_branch, skia_base_sha)
    print(f"Created {head_branch} from parent {parent_base_sha}.")
    print(f"Created externals/skia/{head_branch} from {skia_base_sha}.")


def main() -> int:
    parser = argparse.ArgumentParser(
        description="Create matched SkiaSharp and paired Skia update branches."
    )
    parser.add_argument(
        "--repo-root",
        type=Path,
        default=Path(os.environ.get("GITHUB_WORKSPACE", ".")),
    )
    parser.add_argument("--base-branch", default=os.environ.get("SKIA_SYNC_BASE_BRANCH"))
    parser.add_argument(
        "--parent-base-sha", default=os.environ.get("SKIA_SYNC_PARENT_BASE_SHA")
    )
    parser.add_argument(
        "--skia-base-branch", default=os.environ.get("SKIA_SYNC_SKIA_BASE_BRANCH")
    )
    parser.add_argument(
        "--skia-base-sha", default=os.environ.get("SKIA_SYNC_SKIA_BASE_SHA")
    )
    parser.add_argument("--head-branch", default=os.environ.get("SKIA_SYNC_HEAD_BRANCH"))
    args = parser.parse_args()

    required = {
        "--base-branch": args.base_branch,
        "--parent-base-sha": args.parent_base_sha,
        "--skia-base-branch": args.skia_base_branch,
        "--skia-base-sha": args.skia_base_sha,
        "--head-branch": args.head_branch,
    }
    missing = [name for name, value in required.items() if not value]
    if missing:
        parser.error("missing required values: " + ", ".join(missing))

    try:
        prepare_branches(
            args.repo_root,
            args.base_branch,
            args.parent_base_sha,
            args.skia_base_branch,
            args.skia_base_sha,
            args.head_branch,
        )
    except PreparationError as error:
        parser.exit(1, f"error: {error}\n")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
