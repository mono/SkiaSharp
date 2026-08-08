#!/usr/bin/env python3

import importlib.util
from pathlib import Path
import sys
from types import SimpleNamespace
import unittest
from unittest import mock


PIPELINE = (
    Path(__file__).resolve().parents[5]
    / "scripts"
    / "azure-pipelines-release-coordinator.yml"
)
LOCAL_COORDINATOR = (
    Path(__file__).resolve().parents[5]
    / "scripts"
    / "release-coordinator.py"
)
SPEC = importlib.util.spec_from_file_location(
    "local_release_coordinator",
    LOCAL_COORDINATOR,
)
local = importlib.util.module_from_spec(SPEC)
sys.modules[SPEC.name] = local
SPEC.loader.exec_module(local)


class ReleaseCoordinatorTests(unittest.TestCase):
    def test_pipeline_exposes_start_and_complete_buttons(self):
        text = PIPELINE.read_text(encoding="utf-8")
        self.assertIn("- name: operation", text)
        self.assertIn("      - start", text)
        self.assertIn("      - complete", text)
        self.assertIn("- name: target", text)
        self.assertIn("- name: overrideReleaseTesting", text)
        self.assertIn("    default: false", text)
        plan_start = text[
            text.index("displayName: Build immutable start plan") :
            text.index("- stage: ApproveStart")
        ]
        self.assertIn(
            "GH_TOKEN: $(github--pat--xamarinreleasemanager)",
            plan_start,
        )

    def test_local_coordinator_exposes_four_phases_and_aliases(self):
        parser = local.create_parser()
        cases = (
            (["start"], local.start_phase),
            (["a"], local.start_phase),
            (["packages", "release/4.152.0"], local.packages_phase),
            (["b", "release/4.152.0"], local.packages_phase),
            (["release", "release/4.152.0"], local.release_phase),
            (["c", "release/4.152.0"], local.release_phase),
            (["finish", "release/4.152.0"], local.finish_phase),
            (["d", "release/4.152.0"], local.finish_phase),
        )
        for arguments, handler in cases:
            with self.subTest(arguments=arguments):
                self.assertIs(
                    parser.parse_args(arguments).handler,
                    handler,
                )

    def test_local_package_azure_mode_waits_only_for_pipeline(self):
        context = {
            "releaseBranch": "release/4.152.0-preview.1",
            "pushAuditCommand": "python3 push.py release --dry-run",
        }
        plan = {
            "nextAction": "confirm-publish-packages",
            "executionCommand": "python3 push.py release",
            "resumeCommand": None,
        }
        completed = {
            "nextAction": "start-release-draft",
            "publishRun": {"runId": 14911788},
        }
        args = SimpleNamespace(
            target=context["releaseBranch"],
            verification="azure",
            execute=True,
        )
        with (
            mock.patch.object(
                local,
                "detect_publication",
                return_value=context,
            ),
            mock.patch.object(
                local,
                "run_json",
                side_effect=[plan, completed],
            ) as run,
        ):
            result = local.packages_phase(args)

        audit_command = run.call_args_list[0].args[0]
        execution_command = run.call_args_list[1].args[0]
        self.assertIn("--dry-run", audit_command)
        self.assertEqual(
            audit_command[audit_command.index("--verification") + 1],
            "azure",
        )
        self.assertIn("--wait", execution_command)
        self.assertEqual(
            execution_command[
                execution_command.index("--verification") + 1
            ],
            "azure",
        )
        self.assertEqual(result["result"], completed)
        self.assertIn(
            "--verification azure --publish-run 14911788",
            result["nextCommand"],
        )

    def test_local_release_numeric_supports_hotfixes(self):
        self.assertEqual(
            local.release_numeric("release/4.152.0-preview.1"),
            "4.152.0",
        )
        self.assertEqual(
            local.release_numeric("release/4.152.0.1"),
            "4.152.0.1",
        )

    def test_local_release_plans_marked_generated_notes_publication(self):
        context = {
            "releaseBranch": "release/4.152.0-preview.1",
            "draftAuditCommand": "python3 draft.py release --dry-run",
        }
        draft = {
            "nextAction": "confirm-publish-release",
            "publishAuditCommand": "python3 publish.py release --dry-run",
        }
        publication = {
            "nextAction": "confirm-publish-release",
            "executionCommand": "python3 publish.py release",
        }
        args = SimpleNamespace(
            target=context["releaseBranch"],
            execute_draft=False,
            publish=False,
            verification="nuget",
            publish_run=None,
        )
        with (
            mock.patch.object(
                local,
                "detect_publication",
                return_value=context,
            ),
            mock.patch.object(
                local,
                "run_json",
                side_effect=[draft, publication],
            ) as run,
        ):
            result = local.release_phase(args)

        self.assertEqual(run.call_count, 2)
        self.assertEqual(result["draft"], draft)
        self.assertEqual(result["publication"], publication)
        self.assertIsNone(result["publicationResult"])
        self.assertIsNone(result["nextCommand"])

    def test_local_release_recovers_docs_dispatch_after_publication(self):
        context = {
            "releaseBranch": "release/4.152.0",
            "draftAuditCommand": "python3 draft.py release --dry-run",
        }
        draft = {
            "nextAction": "audit-release-publication",
            "publishAuditCommand": "python3 publish.py release --dry-run",
        }
        publication = {
            "nextAction": "dispatch-release-notes",
            "executionCommand": "python3 publish.py release",
        }
        completed = {
            "nextAction": "start-release-milestones",
        }
        args = SimpleNamespace(
            target=context["releaseBranch"],
            execute_draft=False,
            publish=True,
            verification="nuget",
            publish_run=None,
        )
        with (
            mock.patch.object(
                local,
                "detect_publication",
                return_value=context,
            ),
            mock.patch.object(
                local,
                "run_json",
                side_effect=[draft, publication, completed],
            ) as run,
        ):
            result = local.release_phase(args)

        self.assertEqual(run.call_count, 3)
        self.assertEqual(result["publicationResult"], completed)
        self.assertIn(" finish ", result["nextCommand"])

    def test_local_finish_recovers_docs_dispatch_before_milestones(self):
        context = {
            "releaseBranch": "release/4.152.0",
            "draftAuditCommand": "python3 draft.py release --dry-run",
        }
        draft = {
            "nextAction": "audit-release-publication",
            "publishAuditCommand": "python3 publish.py release --dry-run",
        }
        publication = {
            "nextAction": "dispatch-release-notes",
            "executionCommand": "python3 publish.py release",
        }
        args = SimpleNamespace(
            target=context["releaseBranch"],
            verification="nuget",
            publish_run=None,
            execute=True,
        )
        completed = {"nextAction": "start-release-milestones"}
        with (
            mock.patch.object(
                local,
                "detect_publication",
                return_value=context,
            ),
            mock.patch.object(
                local,
                "run_json",
                side_effect=[draft, publication, completed],
            ) as run,
            mock.patch.object(
                local,
                "release_numeric",
                side_effect=local.CoordinatorError("stop after dispatch"),
            ),
        ):
            with self.assertRaisesRegex(
                local.CoordinatorError,
                "stop after dispatch",
            ):
                local.finish_phase(args)

        self.assertEqual(run.call_args_list[2].args[0][1], "publish.py")

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

    def test_pipeline_has_no_coordinator_owned_teaser_generation(self):
        text = PIPELINE.read_text(encoding="utf-8")
        self.assertNotIn("CopilotGitHubToken", text)
        self.assertNotIn("copilotModel", text)
        self.assertNotIn("PrepareTeaser", text)
        self.assertNotIn("release-teaser", text)
        self.assertNotIn("teaser-file", text)
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
            "Queue and wait for protected Azure publication",
            text,
        )
        self.assertIn(
            'command.extend(["--verification", "azure"])',
            text,
        )
        self.assertIn('or plan.get("resumeCommand")', text)
        self.assertIn('command.append("--wait")', text)
        self.assertIn(
            'result["nextAction"] != "start-release-draft"',
            text,
        )
        self.assertIn(
            'publication["nuget"]["state"] != "ready"',
            text,
        )
        self.assertIn('"--publish-run"', text)

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

    def test_draft_precedes_short_publication_phase(self):
        text = PIPELINE.read_text(encoding="utf-8")
        create = text.index("- stage: CreateDraft")
        plan = text.index("- stage: PlanPublication")
        publish = text.index("- stage: PublishRelease")
        self.assertLess(create, plan)
        self.assertLess(plan, publish)
        self.assertIn('context["draftAuditCommand"]', text)
        self.assertIn('draft["publishAuditCommand"]', text)
        self.assertIn('"dispatch-release-notes"', text)


if __name__ == "__main__":
    unittest.main()
