#!/usr/bin/env python3

"""Stage Skia sync process assets outside the mutable product checkout."""

import argparse
import shutil
import subprocess
from pathlib import Path


REQUIRED_SKILL_ASSETS = (
    "SKILL.md",
    "references/phases/01-03-research.md",
    "references/phases/04-05-branch-and-merge.md",
    "references/phases/06-07-update-and-build.md",
    "references/phases/08-10-bindings-and-tests.md",
    "references/phases/11-11-ship.md",
    "scripts/audit_fork_patches.py",
    "scripts/regenerate_bindings.py",
    "scripts/update_versions.py",
)

REQUIRED_WORKFLOW_ASSETS = (
    "skia-sync-push-prs.sh",
    "skia_sync_stage_runtime.py",
)

# actions/checkout initializes every submodule, and skia-sync-prepare-skia.sh then
# aligns externals/skia to the resolved release gitlink. Skia remains mutable for
# the merge; depot_tools is mutable build state. Only docs must follow parent
# branch switches automatically.
IMMUTABLE_SUBMODULES = ("docs",)


def run_git(repo_root: Path, *args: str) -> str:
    result = subprocess.run(
        ["git", "-C", str(repo_root), *args],
        check=True,
        capture_output=True,
        text=True,
    )
    return result.stdout.strip()


def align_immutable_submodules(
    repo_root: Path, submodules: tuple[str, ...] = IMMUTABLE_SUBMODULES
) -> None:
    repo_root = repo_root.resolve()
    run_git(repo_root, "submodule", "sync", "--", *submodules)
    run_git(
        repo_root,
        "submodule",
        "update",
        "--init",
        "--recursive",
        "--",
        *submodules,
    )

    for submodule in submodules:
        tree_entry = run_git(repo_root, "ls-tree", "HEAD", "--", submodule)
        fields = tree_entry.split()
        if len(fields) < 3:
            raise RuntimeError(f"Could not resolve the {submodule} gitlink at HEAD.")
        expected = fields[2]
        actual = run_git(repo_root / submodule, "rev-parse", "HEAD")
        if actual != expected:
            raise RuntimeError(
                f"{submodule} is at {actual}, expected parent gitlink {expected}."
            )


def stage_runtime_assets(
    repo_root: Path, runtime_dir: Path, github_env: Path | None = None
) -> Path:
    repo_root = repo_root.resolve()
    runtime_dir = runtime_dir.resolve()
    skill_source = repo_root / ".agents" / "skills" / "update-skia"
    scripts_source = repo_root / ".github" / "scripts"

    required_sources = [skill_source / path for path in REQUIRED_SKILL_ASSETS]
    workflow_sources = [scripts_source / path for path in REQUIRED_WORKFLOW_ASSETS]
    missing = [
        path for path in (*required_sources, *workflow_sources) if not path.is_file()
    ]
    if missing:
        raise FileNotFoundError(
            "Missing required Skia sync runtime assets:\n- "
            + "\n- ".join(str(path) for path in missing)
        )

    runtime_dir.mkdir(parents=True, exist_ok=True)
    staged_skill = runtime_dir / "update-skia"
    if staged_skill.exists():
        shutil.rmtree(staged_skill)
    shutil.copytree(skill_source, staged_skill)
    for source in workflow_sources:
        shutil.copy2(source, runtime_dir / source.name)

    if github_env:
        with github_env.open("a", encoding="utf-8", newline="\n") as env:
            env.write(f"SKIA_SYNC_RUNTIME_DIR={runtime_dir}\n")
            env.write(f"SKIA_SYNC_SKILL_DIR={staged_skill}\n")
            env.write(
                f"SKIA_SYNC_VERSION_HELPER={staged_skill / 'scripts' / 'update_versions.py'}\n"
            )
            env.write(
                "SKIA_SYNC_SUBMODULE_HELPER="
                f"{runtime_dir / 'skia_sync_stage_runtime.py'}\n"
            )

    return staged_skill


def main() -> int:
    parser = argparse.ArgumentParser(
        description="Manage immutable Skia sync state outside the product checkout."
    )
    parser.add_argument("--repo-root", type=Path, required=True)
    parser.add_argument("--runtime-dir", type=Path)
    parser.add_argument("--github-env", type=Path)
    parser.add_argument("--align-submodules", action="store_true")
    args = parser.parse_args()

    if args.align_submodules:
        align_immutable_submodules(args.repo_root)
        print(
            "Aligned immutable parent submodules: "
            + ", ".join(IMMUTABLE_SUBMODULES)
        )
        return 0
    if args.runtime_dir is None:
        parser.error("--runtime-dir is required when staging runtime assets.")

    staged_skill = stage_runtime_assets(
        args.repo_root, args.runtime_dir, args.github_env
    )
    print(f"Staged immutable update-skia skill at {staged_skill}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
