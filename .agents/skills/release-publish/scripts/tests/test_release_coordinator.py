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
        self.assertEqual(text.count("ManualValidation@1"), 6)
        for stage in (
            "ApproveStart",
            "ApprovePackages",
            "ApproveDraft",
            "ApprovePublication",
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

    def test_downstream_package_approval_remains_human_owned(self):
        text = PIPELINE.read_text(encoding="utf-8")
        self.assertIn(
            "A human must separately review versions and",
            text,
        )
        self.assertIn(
            "Wait for human-approved package push",
            text,
        )
        self.assertIn(
            "Queue, wait for protected approval, and verify NuGet.org",
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
        self.assertEqual(text.count("##vso[task.uploadsummary]"), 5)
        self.assertIn("##vso[task.uploadfile]", text)
        self.assertIn("plan.json", text)
        self.assertIn("push-plan.json", text)
        self.assertIn("draft-plan.json", text)
        self.assertIn("publication-plan.json", text)
        self.assertIn("milestones-plan.json", text)
        self.assertIn("PublishPipelineArtifact@1", text)
        self.assertIn("DownloadPipelineArtifact@2", text)

    def test_draft_precedes_teaser_and_publication(self):
        text = PIPELINE.read_text(encoding="utf-8")
        create = text.index("- stage: CreateDraft")
        teaser = text.index("- stage: PrepareTeaser")
        plan = text.index("- stage: PlanPublication")
        publish = text.index("- stage: PublishRelease")
        self.assertLess(create, teaser)
        self.assertLess(teaser, plan)
        self.assertLess(plan, publish)
        self.assertIn('context["draftAuditCommand"]', text)
        self.assertIn('draft["publishAuditCommand"]', text)


if __name__ == "__main__":
    unittest.main()
