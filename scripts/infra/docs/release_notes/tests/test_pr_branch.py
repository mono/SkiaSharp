from __future__ import annotations

import os
import subprocess
import tempfile
import unittest
from pathlib import Path

_ROOT = Path(__file__).resolve().parents[5]
_SCRIPT = _ROOT / ".agents/skills/release-notes/scripts/prepare-pr-branch.sh"


class PrBranchTests(unittest.TestCase):
    def test_branch_preparation_preserves_sources_but_commits_only_docs_on_main(self):
        for source in ("main", "feature/docs"):
            with self.subTest(source=source), tempfile.TemporaryDirectory() as tmp:
                root = Path(tmp)

                def git(*args):
                    return subprocess.check_output(["git", *args], cwd=root, text=True, stderr=subprocess.PIPE).strip()

                git("init", "-b", "main")
                git("config", "user.name", "Docs test")
                git("config", "user.email", "docs-test@example.invalid")
                git("config", "commit.gpgsign", "false")
                git("config", "core.hooksPath", ".git/hooks")
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
                env = dict(os.environ, SOURCE_BRANCH=source, GITHUB_RUN_ID="123", GITHUB_ENV=str(env_path))
                subprocess.run(["bash", str(_SCRIPT)], cwd=root, env=env, check=True, capture_output=True)

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
            env = dict(os.environ, SOURCE_BRANCH="feature/docs", GITHUB_RUN_ID="../main", GITHUB_ENV=str(env_path))
            result = subprocess.run(["bash", str(_SCRIPT)], cwd=tmp, env=env, text=True, capture_output=True)
            self.assertNotEqual(result.returncode, 0)
            self.assertIn("GITHUB_RUN_ID must be numeric", result.stderr)
            self.assertFalse(env_path.exists())
