#!/usr/bin/env python3

"""Stage Skia sync process assets outside the mutable product checkout."""

import argparse
import shutil
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


def stage_runtime_assets(
    repo_root: Path, runtime_dir: Path, github_env: Path | None = None
) -> Path:
    repo_root = repo_root.resolve()
    runtime_dir = runtime_dir.resolve()
    skill_source = repo_root / ".agents" / "skills" / "update-skia"
    push_helper = repo_root / ".github" / "scripts" / "skia-sync-push-prs.sh"

    required_sources = [skill_source / path for path in REQUIRED_SKILL_ASSETS]
    missing = [path for path in (*required_sources, push_helper) if not path.is_file()]
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
    shutil.copy2(push_helper, runtime_dir / push_helper.name)

    if github_env:
        with github_env.open("a", encoding="utf-8", newline="\n") as env:
            env.write(f"SKIA_SYNC_RUNTIME_DIR={runtime_dir}\n")
            env.write(f"SKIA_SYNC_SKILL_DIR={staged_skill}\n")
            env.write(
                f"SKIA_SYNC_VERSION_HELPER={staged_skill / 'scripts' / 'update_versions.py'}\n"
            )

    return staged_skill


def main() -> int:
    parser = argparse.ArgumentParser(
        description="Stage immutable Skia sync assets outside the product checkout."
    )
    parser.add_argument("--repo-root", type=Path, required=True)
    parser.add_argument("--runtime-dir", type=Path, required=True)
    parser.add_argument("--github-env", type=Path)
    args = parser.parse_args()

    staged_skill = stage_runtime_assets(
        args.repo_root, args.runtime_dir, args.github_env
    )
    print(f"Staged immutable update-skia skill at {staged_skill}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
