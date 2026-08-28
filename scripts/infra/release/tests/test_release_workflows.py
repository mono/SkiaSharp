import re
from pathlib import Path
import unittest


ROOT = Path(__file__).resolve().parents[4]
PREPARE = ROOT / ".github" / "workflows" / "release-prepare.yml"
FINISH = ROOT / ".github" / "workflows" / "release-finish.yml"
TOOLING_TESTS = ROOT / ".github" / "workflows" / "release-tooling-tests.yml"


class ReleaseWorkflowTests(unittest.TestCase):
    def text(self, path: Path) -> str:
        return path.read_text(encoding="utf-8")

    def job(self, path: Path, name: str) -> str:
        match = re.search(
            rf"(?ms)^  {re.escape(name)}:\n(.*?)(?=^  [a-z0-9-]+:\n|\Z)",
            self.text(path),
        )
        self.assertIsNotNone(match, name)
        return match.group(1)

    def test_workflows_are_manual_only(self):
        for path in (PREPARE, FINISH):
            text = self.text(path)
            self.assertIn("workflow_dispatch:", text)
            self.assertNotRegex(text, r"(?m)^\s+pull_request(?:_target)?:")
            self.assertNotRegex(text, r"(?m)^\s+push:")

    def test_every_action_is_pinned_to_a_sha(self):
        for path in (PREPARE, FINISH, TOOLING_TESTS):
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
        self.assertIn(
            "--publication \"$RUNNER_TEMP/release-finish/publication/publication-plan.json\"",
            text,
        )
        self.assertIn(
            "--plan \"$RUNNER_TEMP/release-finish/original/plan.json\"",
            text,
        )
        self.assertIn("ref: ${{ needs.plan.outputs.tooling_sha }}", text)
        self.assertIn("ref: ${{ needs.plan-publication.outputs.tooling_sha }}", text)
        self.assertNotIn("always() &&", text)
        self.assertGreaterEqual(text.count("!cancelled() &&"), 2)

    def test_finish_read_jobs_can_discover_drafts_without_the_cross_repo_pat(self):
        for name in ("plan", "plan-publication"):
            job = self.job(FINISH, name)
            self.assertIn("contents: write", job)
            self.assertIn("GH_TOKEN: ${{ github.token }}", job)
            self.assertNotIn("secrets.SKIASHARP_AUTOBUMP_TOKEN", job)

    def test_default_branch_guard_precedes_writes(self):
        for path in (PREPARE, FINISH):
            text = self.text(path)
            guard = text.index("Require the default branch workflow")
            secret = text.index("secrets.SKIASHARP_AUTOBUMP_TOKEN")
            self.assertLess(guard, secret)

    def test_protected_environment_check_precedes_irreversible_secret_use(self):
        prepare = self.job(PREPARE, "apply")
        self.assertLess(
            prepare.index("--name release-branching"),
            prepare.index("secrets.SKIASHARP_AUTOBUMP_TOKEN"),
        )

        tag_job = self.job(FINISH, "create-draft")
        publish_job = self.job(FINISH, "publish")
        self.assertLess(
            tag_job.index("--name release-tag"),
            tag_job.index("secrets.SKIASHARP_AUTOBUMP_TOKEN"),
        )
        self.assertLess(
            publish_job.index("--name release-publish"),
            publish_job.index("secrets.SKIASHARP_AUTOBUMP_TOKEN"),
        )

    def test_closeout_pat_exemption_is_bounded_and_serialized(self):
        closeout = self.job(FINISH, "closeout")
        self.assertNotIn("environment:", closeout)
        self.assertIn("needs.publish.result", closeout)
        self.assertIn("group: release-${{ needs.plan.outputs.release_identity }}", closeout)
        self.assertIn("secrets.SKIASHARP_AUTOBUMP_TOKEN", closeout)
        docs = (ROOT / "documentation" / "dev" / "releasing.md").read_text(encoding="utf-8")
        self.assertIn("Closeout also", docs)
        self.assertIn("without another approval", docs)

    def test_release_tooling_tests_are_a_pr_gate(self):
        text = self.text(TOOLING_TESTS)
        self.assertIn("pull_request:", text)
        self.assertIn("scripts/infra/release/**", text)
        self.assertIn("scripts/infra/docs/release_notes/**", text)
        self.assertIn(".agents/skills/release-testing/**", text)
        self.assertEqual(text.count("python3 -m unittest discover"), 3)
        self.assertIn("scripts/infra/caching/repo-deps.py validate", text)


if __name__ == "__main__":
    unittest.main()
