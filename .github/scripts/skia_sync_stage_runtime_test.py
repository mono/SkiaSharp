import subprocess
import shutil
import tempfile
import unittest
from pathlib import Path

from skia_sync_stage_runtime import (
    REQUIRED_SKILL_ASSETS,
    REQUIRED_WORKFLOW_ASSETS,
    align_immutable_submodules,
    stage_runtime_assets,
)


def run_git(repo: Path, *args: str) -> str:
    result = subprocess.run(
        ["git", "-C", str(repo), *args],
        check=True,
        capture_output=True,
        text=True,
    )
    return result.stdout.strip()


def configure_git(repo: Path) -> None:
    run_git(repo, "config", "user.email", "fixture@example.test")
    run_git(repo, "config", "user.name", "fixture")


class StageRuntimeAssetsTests(unittest.TestCase):
    def test_staged_skill_survives_release_checkout_replacement(self) -> None:
        with tempfile.TemporaryDirectory() as temporary_directory:
            root = Path(temporary_directory)
            repo_root = root / "product"
            skill_root = repo_root / ".agents" / "skills" / "update-skia"
            scripts_root = repo_root / ".github" / "scripts"
            for relative_path in REQUIRED_SKILL_ASSETS:
                source = skill_root / relative_path
                source.parent.mkdir(parents=True, exist_ok=True)
                source.write_text(f"main:{relative_path}\n", encoding="utf-8")
            scripts_root.mkdir(parents=True, exist_ok=True)
            for relative_path in REQUIRED_WORKFLOW_ASSETS:
                (scripts_root / relative_path).write_text(
                    f"main:{relative_path}\n", encoding="utf-8"
                )

            runtime_dir = root / "runner-temp" / "skia-sync-runtime"
            github_env = root / "github-env"
            staged_skill = stage_runtime_assets(
                repo_root, runtime_dir, github_env
            )

            shutil.rmtree(repo_root / ".agents")

            for relative_path in REQUIRED_SKILL_ASSETS:
                self.assertEqual(
                    f"main:{relative_path}\n",
                    (staged_skill / relative_path).read_text(encoding="utf-8"),
                )
            for relative_path in REQUIRED_WORKFLOW_ASSETS:
                self.assertEqual(
                    f"main:{relative_path}\n",
                    (runtime_dir / relative_path).read_text(encoding="utf-8"),
                )
            self.assertEqual(
                [
                    f"SKIA_SYNC_RUNTIME_DIR={runtime_dir.resolve()}",
                    f"SKIA_SYNC_SKILL_DIR={staged_skill}",
                    (
                        "SKIA_SYNC_VERSION_HELPER="
                        f"{staged_skill / 'scripts' / 'update_versions.py'}"
                    ),
                    (
                        "SKIA_SYNC_SUBMODULE_HELPER="
                        f"{runtime_dir.resolve() / 'skia_sync_stage_runtime.py'}"
                    ),
                ],
                github_env.read_text(encoding="utf-8").splitlines(),
            )

    def test_release_checkout_realigns_docs_to_parent_gitlink(self) -> None:
        with tempfile.TemporaryDirectory() as temporary_directory:
            root = Path(temporary_directory)
            docs_source = root / "docs-source"
            parent = root / "parent"

            run_git(root, "init", "--quiet", str(docs_source))
            configure_git(docs_source)
            docs_file = docs_source / "api.xml"
            docs_file.write_text("release\n", encoding="utf-8")
            run_git(docs_source, "add", docs_file.name)
            run_git(docs_source, "commit", "--quiet", "-m", "release docs")
            release_docs_sha = run_git(docs_source, "rev-parse", "HEAD")

            docs_file.write_text("main\n", encoding="utf-8")
            run_git(docs_source, "commit", "--quiet", "-am", "main docs")
            main_docs_sha = run_git(docs_source, "rev-parse", "HEAD")

            run_git(root, "init", "--quiet", str(parent))
            configure_git(parent)
            subprocess.run(
                [
                    "git",
                    "-C",
                    str(parent),
                    "-c",
                    "protocol.file.allow=always",
                    "submodule",
                    "add",
                    str(docs_source),
                    "docs",
                ],
                check=True,
                capture_output=True,
                text=True,
            )
            run_git(parent / "docs", "checkout", "--quiet", release_docs_sha)
            run_git(parent, "add", ".gitmodules", "docs")
            run_git(parent, "commit", "--quiet", "-m", "release pointer")
            run_git(parent, "branch", "release")

            run_git(parent / "docs", "checkout", "--quiet", main_docs_sha)
            run_git(parent, "add", "docs")
            run_git(parent, "commit", "--quiet", "-m", "main pointer")

            run_git(parent, "checkout", "--quiet", "release")
            self.assertIn("docs", run_git(parent, "status", "--short"))

            align_immutable_submodules(parent)

            self.assertEqual(
                release_docs_sha, run_git(parent / "docs", "rev-parse", "HEAD")
            )
            self.assertEqual("", run_git(parent, "status", "--short"))


if __name__ == "__main__":
    unittest.main()
