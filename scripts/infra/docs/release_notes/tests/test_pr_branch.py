from __future__ import annotations

import os
import subprocess
import tempfile
import unittest
from pathlib import Path
from unittest import mock

_ROOT = Path(__file__).resolve().parents[5]
_SCRIPT = _ROOT / ".agents/skills/release-notes/scripts/prepare-pr-branch.sh"


def _git_environment(**extra):
    env = {key: value for key, value in os.environ.items() if not key.startswith("GIT_")}
    env.update(GIT_CONFIG_NOSYSTEM="1", GIT_CONFIG_GLOBAL=os.devnull, GIT_TERMINAL_PROMPT="0")
    env.update(extra)
    return env


class PrBranchTests(unittest.TestCase):
    def test_branch_preparation_preserves_sources_but_commits_only_docs_on_main(self):
        for source in ("main", "feature/docs"):
            with self.subTest(source=source), tempfile.TemporaryDirectory() as tmp:
                root = Path(tmp)
                env = _git_environment()

                def git(*args):
                    return subprocess.check_output(["git", *args], cwd=root, env=env,
                                                   text=True, stderr=subprocess.PIPE).strip()

                git("init", "--template=", "-b", "main")
                git("config", "user.name", "Docs test")
                git("config", "user.email", "docs-test@example.invalid")
                git("config", "commit.gpgsign", "false")
                hooks = root / "empty-hooks"
                hooks.mkdir()
                git("config", "core.hooksPath", str(hooks))
                docs = root / "documentation/docfx/releases"
                docs.mkdir(parents=True)
                (docs / "page.md").write_text("Existing page\n")
                tool = root / "tool.py"
                tool.write_text("existing tool\n")
                git("add", ".")
                git("commit", "-m", "baseline")
                baseline = git("rev-parse", "HEAD")
                git("update-ref", "refs/remotes/origin/main", baseline)

                if source != "main":
                    git("checkout", "-b", source)
                    tool.write_text("feature tool\n")
                    git("add", "tool.py")
                    git("commit", "-m", "feature implementation")

                (docs / "page.md").write_text("Prepared and polished page\n")
                git("add", "documentation")
                env_path = root / "github-env"
                script_env = dict(env, SOURCE_BRANCH=source, GITHUB_RUN_ID="123", GITHUB_ENV=str(env_path))
                subprocess.run(["bash", str(_SCRIPT)], cwd=root, env=script_env, check=True, capture_output=True)

                expected_branch = "bot/release-notes" if source == "main" else "bot/release-notes-test-123"
                self.assertEqual(git("branch", "--show-current"), expected_branch)
                self.assertEqual(git("rev-parse", "HEAD"), baseline)
                self.assertEqual(tool.read_text(), "existing tool\n" if source == "main" else "feature tool\n")
                self.assertEqual((docs / "page.md").read_text(), "Prepared and polished page\n")
                self.assertEqual(env_path.read_text(), "DOCS_PR_BRANCH=" + expected_branch + "\n")

                git("add", "--", "documentation/docfx/releases")
                git("commit", "-m", "generated documentation")
                self.assertEqual(git("rev-parse", "HEAD^"), baseline)
                self.assertEqual(git("diff", "--name-only", baseline, "HEAD"),
                                 "documentation/docfx/releases/page.md")

    def test_invalid_run_id_fails_before_git_changes(self):
        with tempfile.TemporaryDirectory() as tmp:
            env_path = Path(tmp) / "github-env"
            env = _git_environment(SOURCE_BRANCH="feature/docs", GITHUB_RUN_ID="../main", GITHUB_ENV=str(env_path))
            result = subprocess.run(["bash", str(_SCRIPT)], cwd=tmp, env=env, text=True, capture_output=True)
            self.assertNotEqual(result.returncode, 0)
            self.assertIn("GITHUB_RUN_ID must be numeric", result.stderr)
            self.assertFalse(env_path.exists())

    def test_git_environment_drops_inherited_repository_and_config_overrides(self):
        with mock.patch.dict(os.environ, {
            "GIT_DIR": "/unrelated/repository",
            "GIT_WORK_TREE": "/unrelated/worktree",
            "GIT_CONFIG_COUNT": "1",
            "GIT_CONFIG_KEY_0": "core.hooksPath",
            "GIT_CONFIG_VALUE_0": "/unrelated/hooks",
            "GIT_TEMPLATE_DIR": "/unrelated/templates",
        }):
            env = _git_environment()
        self.assertEqual({key for key in env if key.startswith("GIT_")},
                         {"GIT_CONFIG_NOSYSTEM", "GIT_CONFIG_GLOBAL", "GIT_TERMINAL_PROMPT"})
        self.assertEqual(env["GIT_CONFIG_GLOBAL"], os.devnull)
