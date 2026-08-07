#!/usr/bin/env python3

from pathlib import Path
import unittest


PIPELINE = (
    Path(__file__).resolve().parents[5]
    / "scripts"
    / "azure-pipelines-release-coordinator.yml"
)


class ReleaseCoordinatorTests(unittest.TestCase):
    def test_pipeline_exposes_start_and_complete_buttons(self):
        text = PIPELINE.read_text(encoding="utf-8")
        self.assertIn("- name: operation", text)
        self.assertIn("      - start", text)
        self.assertIn("      - complete", text)
        self.assertIn("- name: target", text)
        self.assertIn("- name: skipReleaseTesting", text)

    def test_pipeline_has_all_irreversible_approvals(self):
        text = PIPELINE.read_text(encoding="utf-8")
        self.assertEqual(text.count("ManualValidation@1"), 5)
        for stage in (
            "ApproveStart",
            "ApprovePackages",
            "ApproveFinalization",
            "ApproveMilestones",
        ):
            self.assertIn(f"- stage: {stage}", text)

    def test_copilot_teaser_is_tool_free_and_bounded(self):
        text = PIPELINE.read_text(encoding="utf-8")
        self.assertIn("@github/copilot@1.0.77", text)
        self.assertIn("--available-tools=''", text)
        self.assertIn("--disable-builtin-mcps", text)
        self.assertIn("--no-custom-instructions", text)
        self.assertIn("--max-ai-credits 3", text)
        self.assertNotIn("--yolo", text)

    def test_pipeline_uses_separate_copilot_and_release_tokens(self):
        text = PIPELINE.read_text(encoding="utf-8")
        teaser_start = text.index(
            "displayName: Generate isolated customer teaser"
        )
        teaser_end = text.index(
            "              - bash: |",
            teaser_start,
        )
        teaser_step = text[teaser_start:teaser_end]
        self.assertIn("COPILOT_GITHUB_TOKEN", teaser_step)
        self.assertNotRegex(teaser_step, r"(?m)^\s+GH_TOKEN:")
        self.assertIn(
            "GH_TOKEN: $(github--pat--xamarinreleasemanager)",
            text,
        )

    def test_milestone_stage_composes_audit_and_sync_paths(self):
        text = PIPELINE.read_text(encoding="utf-8")
        self.assertIn('audit_command = shlex.split(final["milestonesCommand"])', text)
        self.assertIn(
            ".agents/skills/release-milestones/scripts/sync-milestones.py",
            text,
        )
        self.assertIn('"audit": audit,', text)
        self.assertIn('"sync": sync,', text)
        self.assertIn('"syncDryRunCommand": shlex.join(sync_command),', text)
        self.assertIn("current_sync = json.loads(", text)
        self.assertIn("current_sync[field] != approved_sync[field]", text)

    def test_plans_are_visible_without_using_artifacts_for_review(self):
        text = PIPELINE.read_text(encoding="utf-8")
        self.assertEqual(text.count("##vso[task.uploadsummary]"), 4)
        self.assertIn("##vso[task.uploadfile]", text)
        self.assertIn("plan.json", text)
        self.assertIn("push-plan.json", text)
        self.assertIn("finalize-plan.json", text)
        self.assertIn("milestones-plan.json", text)
        self.assertIn("PublishPipelineArtifact@1", text)
        self.assertIn("DownloadPipelineArtifact@2", text)


if __name__ == "__main__":
    unittest.main()
