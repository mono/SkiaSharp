import re
from pathlib import Path
import unittest


ROOT = Path(__file__).resolve().parents[4]
PREPARE = ROOT / ".github" / "workflows" / "release-prepare.yml"
FINISH = ROOT / ".github" / "workflows" / "release-finish.yml"


class ReleaseWorkflowTests(unittest.TestCase):
    def text(self, path: Path) -> str:
        return path.read_text(encoding="utf-8")

    def test_workflows_are_manual_only(self):
        for path in (PREPARE, FINISH):
            text = self.text(path)
            self.assertIn("workflow_dispatch:", text)
            self.assertNotRegex(text, r"(?m)^\s+pull_request(?:_target)?:")
            self.assertNotRegex(text, r"(?m)^\s+push:")

    def test_every_action_is_pinned_to_a_sha(self):
        for path in (PREPARE, FINISH):
            for action in re.findall(r"(?m)^\s*uses:\s*(\S+)", self.text(path)):
                self.assertRegex(action, r"@[0-9a-f]{40}$", action)

    def test_prepare_has_plan_approval_and_exact_tooling(self):
        text = self.text(PREPARE)
        self.assertIn("environment: release-branching", text)
        self.assertIn("--name release-branching", text)
        self.assertIn("--integration-target", text)
        self.assertIn("ref: ${{ needs.plan.outputs.tooling_sha }}", text)
        self.assertIn("git merge-base --is-ancestor \"$TOOLING_SHA\"", text)
        self.assertIn("secrets.SKIASHARP_AUTOBUMP_TOKEN", text)

    def test_finish_has_separate_tag_and_publish_approvals(self):
        text = self.text(FINISH)
        self.assertIn("environment: release-tag", text)
        self.assertIn("--name release-tag", text)
        self.assertIn("environment: release-publish", text)
        self.assertIn("--name release-publish", text)
        self.assertIn("--publication \"$RUNNER_TEMP/release-finish/publication-plan.json\"", text)
        self.assertIn("ref: ${{ needs.plan.outputs.tooling_sha }}", text)
        self.assertIn("ref: ${{ needs.plan-publication.outputs.tooling_sha }}", text)
        self.assertNotIn("always() &&", text)
        self.assertGreaterEqual(text.count("!cancelled() &&"), 2)

    def test_default_branch_guard_precedes_writes(self):
        for path in (PREPARE, FINISH):
            text = self.text(path)
            guard = text.index("Require the default branch workflow")
            secret = text.index("secrets.SKIASHARP_AUTOBUMP_TOKEN")
            self.assertLess(guard, secret)

    def test_protected_environment_check_precedes_each_secret_use(self):
        prepare = self.text(PREPARE)
        self.assertLess(
            prepare.index("--name release-branching"),
            prepare.index("secrets.SKIASHARP_AUTOBUMP_TOKEN"),
        )

        finish = self.text(FINISH)
        tag_job, publish_job = finish.split("  publish:", maxsplit=1)
        self.assertLess(
            tag_job.index("--name release-tag"),
            tag_job.index("secrets.SKIASHARP_AUTOBUMP_TOKEN"),
        )
        self.assertLess(
            publish_job.index("--name release-publish"),
            publish_job.index("secrets.SKIASHARP_AUTOBUMP_TOKEN"),
        )


if __name__ == "__main__":
    unittest.main()
