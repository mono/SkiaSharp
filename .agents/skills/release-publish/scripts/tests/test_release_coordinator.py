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
        self.assertIn("- name: overrideReleaseTesting", text)
        self.assertIn("    default: false", text)

    def test_pipeline_has_all_irreversible_approvals(self):
        text = PIPELINE.read_text(encoding="utf-8")
        self.assertEqual(text.count("ManualValidation@1"), 7)
        for stage in (
            "ApproveStart",
            "ApprovePackages",
            "ApproveDraft",
            "ApprovePublication",
            "ApproveAssignments",
            "ApproveMilestones",
        ):
            self.assertIn(f"- stage: {stage}", text)

    def test_copilot_teaser_is_tool_free_and_bounded(self):
        text = PIPELINE.read_text(encoding="utf-8")
        self.assertIn("@github/copilot@1.0.78-1", text)
        self.assertIn("--available-tools=''", text)
        self.assertIn("--disable-builtin-mcps", text)
        self.assertIn("--no-custom-instructions", text)
        self.assertIn("--max-ai-credits 3", text)
        self.assertNotIn("--allow-all-tools", text)
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
        self.assertIn('or plan.get("resumeCommand")', text)
        self.assertIn('command.append("--wait")', text)
        self.assertIn(
            'result["nextAction"] != "start-release-draft"',
            text,
        )

    def test_milestones_reconcile_before_advancement(self):
        text = PIPELINE.read_text(encoding="utf-8")
        reconcile = text.index("- stage: PlanAssignments")
        apply_reconcile = text.index("- stage: ReconcileAssignments")
        advance = text.index("- stage: PlanMilestones")
        apply_advance = text.index("- stage: AdvanceMilestones")
        self.assertLess(reconcile, apply_reconcile)
        self.assertLess(apply_reconcile, advance)
        self.assertLess(advance, apply_advance)
        self.assertIn(
            'command = shlex.split(final["milestonesCommand"])',
            text,
        )
        self.assertIn(
            "advance-release-milestones.py",
            text,
        )
        self.assertIn('"reconcile": reconcile,', text)
        self.assertIn('"advance": advance,', text)
        self.assertIn(
            '"advanceDryRunCommand": shlex.join(command)',
            text,
        )
        self.assertIn('"confirm-reconcile-assignments"', text)
        self.assertIn('"confirm-advance-milestones"', text)
        self.assertIn("current[field] != approved[field]", text)
        self.assertIn(
            "eq(dependencies.PlanDraft.result, 'Succeeded')",
            text,
        )
        self.assertIn(
            "eq(dependencies.PlanPublication.result, 'Succeeded')",
            text,
        )
        self.assertIn(
            "eq(dependencies.PlanAssignments.result, 'Succeeded')",
            text,
        )
        self.assertIn('plan["reconcileDryRunCommand"]', text)
        self.assertIn(
            "Release assignments changed after approval",
            text,
        )
        self.assertNotIn("audit-milestones.py", text)
        self.assertNotIn("sync-milestones.py", text)

    def test_plans_are_visible_without_using_artifacts_for_review(self):
        text = PIPELINE.read_text(encoding="utf-8")
        self.assertEqual(text.count("##vso[task.uploadsummary]"), 6)
        self.assertIn("##vso[task.uploadfile]", text)
        self.assertIn("plan.json", text)
        self.assertIn("push-plan.json", text)
        self.assertIn("draft-plan.json", text)
        self.assertIn("publication-plan.json", text)
        self.assertIn("assignments-plan.json", text)
        self.assertIn("milestones-plan.json", text)
        self.assertIn("PublishPipelineArtifact@1", text)
        self.assertIn("DownloadPipelineArtifact@2", text)

    def test_package_result_is_required_before_draft(self):
        text = PIPELINE.read_text(encoding="utf-8")
        push = text.index("- stage: PushPackages")
        draft = text.index("- stage: PlanDraft")
        self.assertLess(push, draft)
        self.assertIn("artifact: release-package-result", text[push:draft])
        self.assertIn("artifact: release-package-result", text[draft:])

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
