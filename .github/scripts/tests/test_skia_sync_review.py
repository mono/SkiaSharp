"""Configuration checks for the single advisory Skia review workflow."""

from pathlib import Path
import unittest

import yaml


ROOT = Path(__file__).resolve().parents[3]
WORKFLOWS = ROOT / ".github/workflows"


class SkiaReviewWorkflowTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.authored = (WORKFLOWS / "skia-sync-review.md").read_text()
        cls.lock = (WORKFLOWS / "skia-sync-review.lock.yml").read_text()
        cls.config = yaml.safe_load(cls.authored.split("---", 2)[1])
        cls.events = cls.config.get("on", cls.config.get(True))

    def test_single_adapter_with_exact_parent_comment_and_manual_entry(self):
        self.assertFalse((WORKFLOWS / "skia-sync-review.yml").exists())
        self.assertEqual("skia-sync-review", self.events["slash_command"]["name"])
        self.assertEqual(["pull_request_comment"], self.events["slash_command"]["events"])
        self.assertIn("github.repository == 'mono/SkiaSharp'", self.config["if"])
        self.assertIn("github.event.comment.body == '/skia-sync-review'", self.config["if"])
        self.assertIn("github.event.issue.pull_request", self.config["if"])
        self.assertEqual("number", self.events["workflow_dispatch"]["inputs"]["skiasharp_pr"]["type"])
        self.assertEqual(True, self.events["workflow_dispatch"]["inputs"]["staged"]["default"])
        self.assertIn("skiasharp_pr is required", self.lock)
        self.assertIn("GH_AW_REQUIRED_ROLES: \"admin,maintainer,write\"", self.lock)

    def test_trusted_checkout_isolation_and_bounded_safe_output(self):
        self.assertEqual("gpt-6-sol", self.config["model"])
        self.assertEqual(False, self.config["checkout"])
        self.assertEqual([".agents/skills/review-skia-update"], self.config["skills"])
        self.assertEqual({"contents": "read", "pull-requests": "read", "issues": "read"}, self.config["permissions"])
        self.assertNotIn("Checkout PR branch", self.lock)
        agent = yaml.safe_load(self.lock)["jobs"]["agent"]
        names = [step.get("name", "") for step in agent["steps"]]
        self.assertFalse(any(
            step.get("uses", "").startswith("actions/checkout@")
            for step in agent["steps"]
        ))
        self.assertLess(
            names.index("Restore inline skills from activation artifact"),
            names.index("Run mechanical review in isolated container"),
        )
        self.assertEqual(".github/skills/review-skia-update",
                         self.config["env"]["SKIA_REVIEW_SKILL_DIR"])
        self.assertIn('python3 "$SKIA_REVIEW_SKILL_DIR/scripts/run_review.py"',
                      self.authored)
        self.assertIn("--isolated", self.authored)
        self.assertIn("--skiasharp-pr \"$SKIASHARP_PR\"", self.authored)
        self.assertEqual("${{ github.event.issue.number || inputs.skiasharp_pr }}",
                         self.config["safe-outputs"]["add-comment"]["target"])
        self.assertEqual(True, self.config["safe-outputs"]["add-comment"]["hide-older-comments"])
        self.assertIn("github.ref != 'refs/heads/main'", self.config["safe-outputs"]["staged"])
        self.assertNotIn("create-pull-request:", self.authored)
        self.assertNotIn("publish-skia-review:", self.authored)

    def test_normal_artifacts_and_registry(self):
        persist = (WORKFLOWS / "persist-aw-data.yml").read_text()
        registry = (ROOT / ".agents/skills/ci-status/scripts/ci-status.py").read_text()
        self.assertIn('"Review - Skia Sync") echo "ai-review"', persist)
        self.assertIn("name: agent", persist)
        self.assertNotIn("skia-review-evidence-", persist)
        self.assertIn('"workflow": "skia-sync-review.lock.yml"', registry)
        self.assertIn("/tmp/gh-aw/agent/ai-review", self.authored)


if __name__ == "__main__":
    unittest.main()
