"""Exercise the actual Git bundle handoff without network access or credentials."""

import importlib.util
import json
import os
from pathlib import Path
import subprocess
import tempfile
import unittest
from unittest.mock import patch


SCRIPT = Path(__file__).resolve().parents[1] / "skia-sync-publish-handoff.py"
SPEC = importlib.util.spec_from_file_location("handoff", SCRIPT)
HANDOFF = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(HANDOFF)
BRANCH = "skia-sync/m156"


def git(repo, *args):
    return subprocess.check_output(
        ["git", "-C", str(repo), *args], text=True, stderr=subprocess.PIPE
    ).strip()


class SyncPublishHandoffTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name)
        self.source = self.root / "source"
        self.source.mkdir()
        self.reports = self.root / "reports"
        self.reports.mkdir()
        self.package = self.root / "handoff"
        bases = []
        for name, repo in (
            ("parent", self.source),
            ("skia", self.source / "externals/skia"),
        ):
            repo.mkdir(parents=True, exist_ok=True)
            git(repo, "init", "--initial-branch=main")
            git(repo, "config", "user.name", "Handoff Test")
            git(repo, "config", "user.email", "test@example.invalid")
            (repo / "content.txt").write_text("base", encoding="utf-8")
            git(repo, "add", "content.txt")
            git(repo, "commit", "-m", "Base")
            bases.append(git(repo, "rev-parse", "HEAD"))
            origin = self.root / f"{name}.git"
            git(self.root, "clone", "--bare", str(repo), str(origin))
            git(repo, "checkout", "-b", "upstream")
            (repo / "upstream.txt").write_text("upstream", encoding="utf-8")
            git(repo, "add", "upstream.txt")
            git(repo, "commit", "-m", "Upstream")
            git(repo, "checkout", "-b", BRANCH, "main")
            (repo / "content.txt").write_text(name, encoding="utf-8")
            git(repo, "commit", "-am", "Fork change")
            git(repo, "merge", "--no-ff", "upstream", "-m", "Merge upstream")

        self.environment = dict(
            GITHUB_WORKSPACE=str(self.source),
            SKIA_SYNC_ARTIFACT_DIR=str(self.reports),
            SKIA_SYNC_CURRENT="155",
            SKIA_SYNC_TARGET="156",
            SKIA_SYNC_UPSTREAM_REF="chrome/m156",
            SKIA_SYNC_IS_RELEASE="false",
            SKIA_SYNC_BASE_BRANCH="main",
            SKIA_SYNC_SKIA_BASE_BRANCH="main",
            SKIA_SYNC_HEAD_BRANCH=BRANCH,
            SKIA_SYNC_PARENT_BASE_SHA=bases[0],
            SKIA_SYNC_SKIA_BASE_SHA=bases[1],
            SKIA_SYNC_BASE_UPSTREAM_SHA=bases[1],
            SKIA_SYNC_TARGET_UPSTREAM_SHA=bases[1],
        )
        for name in HANDOFF.REPORTS:
            content = "0\n" if name.endswith("-exit-code.txt") else f"Report: {name}"
            (self.reports / name).write_text(content, encoding="utf-8")

    def run_handoff(self, mode, **environment):
        with patch.dict(os.environ, self.environment | environment), patch(
            "sys.argv", [str(SCRIPT), mode, str(self.package)]
        ):
            HANDOFF.main()

    def test_round_trip_preserves_every_commit_sha_and_both_merge_parents(self):
        self.run_handoff("pack", GH_TOKEN="not-a-real-credential")
        restored = self.root / "restored"
        for name, repo in (
            ("parent", restored),
            ("skia", restored / "externals/skia"),
        ):
            repo.parent.mkdir(parents=True, exist_ok=True)
            git(self.root, "clone", str(self.root / f"{name}.git"), str(repo))
        env_file = self.root / "github-env"
        self.run_handoff(
            "restore",
            GITHUB_WORKSPACE=str(restored),
            GITHUB_ENV=str(env_file),
            EXPECTED_HEAD_BRANCH=BRANCH,
            EXPECTED_BASE_BRANCH="main",
            EXPECTED_SKIA_BASE_BRANCH="main",
        )
        for source, destination in (
            (self.source, restored),
            (self.source / "externals/skia", restored / "externals/skia"),
        ):
            self.assertEqual(
                git(source, "rev-list", "--parents", BRANCH),
                git(destination, "rev-list", "--parents", BRANCH),
            )
            self.assertEqual(3, len(git(destination, "rev-list", "--parents", "-1", BRANCH).split()))
        metadata = json.loads((self.package / "environment.json").read_text())
        self.assertEqual(set(HANDOFF.ENV_KEYS), set(metadata))
        self.assertNotIn("GH_TOKEN", env_file.read_text())
        for name in HANDOFF.REPORTS:
            self.assertEqual(
                (self.reports / name).read_bytes(),
                (self.package / "reports" / name).read_bytes(),
            )

    def test_restore_rejects_an_unexpected_branch_before_importing_commits(self):
        self.run_handoff("pack")
        with self.assertRaisesRegex(ValueError, "HEAD_BRANCH"):
            self.run_handoff("restore", EXPECTED_HEAD_BRANCH="main")

    def test_pack_rejects_environment_file_injection(self):
        with self.assertRaisesRegex(ValueError, "invalid value"):
            self.run_handoff("pack", SKIA_SYNC_TARGET="156\nGH_TOKEN=injected")

    def test_pack_rejects_symlinked_reports(self):
        report = self.reports / HANDOFF.REPORTS[0]
        report.unlink()
        report.symlink_to(self.reports / HANDOFF.REPORTS[1])
        with self.assertRaisesRegex(ValueError, "symlink"):
            self.run_handoff("pack")

    def test_pack_rejects_failed_validation(self):
        (self.reports / "test-exit-code.txt").write_text("1\n")
        with self.assertRaisesRegex(ValueError, "validation did not pass"):
            self.run_handoff("pack")


if __name__ == "__main__":
    unittest.main()
