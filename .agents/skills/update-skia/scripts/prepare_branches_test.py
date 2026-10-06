import os
import subprocess
import tempfile
import unittest
from pathlib import Path
from unittest.mock import patch

from prepare_branches import prepare_branches


def run_git(repo: Path, *args: str) -> str:
    result = subprocess.run(
        ["git", "-C", str(repo), *args],
        check=True,
        capture_output=True,
        text=True,
    )
    return result.stdout.rstrip("\r\n")


def configure_git(repo: Path) -> None:
    run_git(repo, "config", "user.email", "fixture@example.test")
    run_git(repo, "config", "user.name", "fixture")


def create_history(root: Path, name: str) -> tuple[Path, str, str]:
    repo = root / f"{name}-origin"
    run_git(root, "init", "--quiet", str(repo))
    configure_git(repo)

    payload = repo / "payload.txt"
    payload.write_text("release\n", encoding="utf-8")
    run_git(repo, "add", payload.name)
    run_git(repo, "commit", "--quiet", "-m", "release state")
    release_sha = run_git(repo, "rev-parse", "HEAD")
    run_git(repo, "branch", "release")

    payload.write_text("main\n", encoding="utf-8")
    run_git(repo, "commit", "--quiet", "-am", "main state")
    main_sha = run_git(repo, "rev-parse", "HEAD")
    run_git(repo, "branch", "-M", "main")
    return repo, release_sha, main_sha


class PrepareBranchesTests(unittest.TestCase):
    def test_release_branch_realigns_all_submodules_before_branching_skia(
        self,
    ) -> None:
        with tempfile.TemporaryDirectory() as temporary_directory:
            root = Path(temporary_directory)
            docs, release_docs_sha, main_docs_sha = create_history(root, "docs")
            skia, release_skia_sha, main_skia_sha = create_history(root, "skia")
            parent = root / "parent-origin"

            run_git(root, "init", "--quiet", str(parent))
            configure_git(parent)
            for source, destination in (
                (docs, "docs"),
                (skia, "externals/skia"),
            ):
                run_git(
                    parent,
                    "-c",
                    "protocol.file.allow=always",
                    "submodule",
                    "add",
                    str(source),
                    destination,
                )

            run_git(parent / "docs", "checkout", "--quiet", release_docs_sha)
            run_git(
                parent / "externals" / "skia",
                "checkout",
                "--quiet",
                release_skia_sha,
            )
            run_git(parent, "add", ".gitmodules", "docs", "externals/skia")
            run_git(parent, "commit", "--quiet", "-m", "release pointers")
            release_parent_sha = run_git(parent, "rev-parse", "HEAD")
            run_git(parent, "branch", "release")

            run_git(parent / "docs", "checkout", "--quiet", main_docs_sha)
            run_git(
                parent / "externals" / "skia",
                "checkout",
                "--quiet",
                main_skia_sha,
            )
            run_git(parent, "add", "docs", "externals/skia")
            run_git(parent, "commit", "--quiet", "-m", "main pointers")
            run_git(parent, "branch", "-M", "main")

            workspace = root / "workspace"
            subprocess.run(
                [
                    "git",
                    "-c",
                    "protocol.file.allow=always",
                    "clone",
                    "--quiet",
                    "--recursive",
                    str(parent),
                    str(workspace),
                ],
                check=True,
                capture_output=True,
                text=True,
            )

            workspace_skia = workspace / "externals" / "skia"
            run_git(workspace_skia, "checkout", "--quiet", release_skia_sha)
            self.assertIn("externals/skia", run_git(workspace, "status", "--short"))

            head_branch = "skia-sync/release-test"
            with patch.dict(os.environ, {"GIT_ALLOW_PROTOCOL": "file"}):
                prepare_branches(
                    workspace,
                    "release",
                    release_parent_sha,
                    "release",
                    release_skia_sha,
                    head_branch,
                )

            self.assertEqual(head_branch, run_git(workspace, "branch", "--show-current"))
            self.assertEqual(release_parent_sha, run_git(workspace, "rev-parse", "HEAD"))
            self.assertEqual(
                release_docs_sha,
                run_git(workspace / "docs", "rev-parse", "HEAD"),
            )
            self.assertEqual(
                head_branch,
                run_git(workspace_skia, "branch", "--show-current"),
            )
            self.assertEqual(
                release_skia_sha,
                run_git(workspace_skia, "rev-parse", "HEAD"),
            )
            self.assertEqual("", run_git(workspace, "status", "--short"))
            self.assertFalse(
                any(
                    line and line[0] in "-+U"
                    for line in run_git(
                        workspace, "submodule", "status", "--recursive"
                    ).splitlines()
                )
            )


if __name__ == "__main__":
    unittest.main()
