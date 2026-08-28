from __future__ import annotations

import sys
import unittest
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent.parent))

import release_common as common
import release_summary as summary
from release_common import PlanError


def make_prepare_plan(**overrides) -> dict:
    plan = {
        "schemaVersion": 1,
        "operation": "prepare",
        "generatedAt": "2024-01-01T00:00:00Z",
        "toolingSha": "a" * 40,
        "nextAction": "apply",
        "input": {"integrationTarget": "main", "requestedVersion": None},
        "release": {
            "identity": "3.119.0-preview.1",
            "version": "3.119.0-preview.1",
            "numeric": "3.119.0",
            "label": "preview.1",
            "releaseType": "preview",
            "branch": "release/3.119.0-preview.1",
            "integrationBranch": "release/3.119.x",
            "isHotfix": False,
            "stable": False,
        },
        "base": {"ref": "refs/remotes/origin/main", "sha": "b" * 40},
        "maintenanceBranch": {"name": "release/3.119.x", "exists": False, "action": "create", "baseSha": "b" * 40},
        "skia": {"sha": "c" * 40, "releaseBranch": "release/3.119.0-preview.1", "remoteState": "missing"},
        "skiaSharpRemoteState": "missing",
        "versions": {"skiaSharp": "3.119.0", "requiresPackageBump": False},
        "operations": [],
        "stableBump": None,
        "warnings": ["maintenance branch will be created"],
    }
    plan.update(overrides)
    return common.with_digest(plan)


def make_finish_plan(**overrides) -> dict:
    plan = {
        "schemaVersion": 1,
        "operation": "finish",
        "generatedAt": "2024-01-01T00:00:00Z",
        "toolingSha": "a" * 40,
        "nextAction": "create-draft",
        "input": {"requestedVersion": "3.119.0-preview.1.42"},
        "receipt": {
            "skiaSharpVersion": "3.119.0-preview.1.42",
            "base": "3.119.0",
            "label": "preview.1",
            "buildRevision": "42",
            "sourceCommit": "d" * 40,
            "sourceBranch": "release/3.119.0-preview.1",
            "harfBuzzSharpVersion": "1.8.8.1-preview.1.42",
            "packages": [],
        },
        "release": {
            "identity": "3.119.0-preview.1",
            "version": "3.119.0-preview.1.42",
            "branch": "release/3.119.0-preview.1",
            "raw": "3.119.0-preview.1", "numeric": "3.119.0", "label": "preview.1",
            "releaseType": "preview", "stable": False, "title": "Version 3.119.0 (Preview 1)",
            "tag": "v3.119.0-preview.1",
        },
        "tag": {"name": "v3.119.0-preview.1", "targetCommit": "d" * 40, "existingSha": None, "status": "pending"},
        "previousTag": "v3.118.0",
        "draft": {"exists": False, "isPublished": False, "status": "pending"},
        "warnings": [],
    }
    plan.update(overrides)
    return common.with_digest(plan)


class SummarizePrepareTests(unittest.TestCase):
    def test_derives_flat_fields(self):
        plan = make_prepare_plan()
        rendered = summary.summarize_document(plan)
        self.assertEqual(rendered["schemaVersion"], 1)
        self.assertEqual(rendered["operation"], "prepare")
        self.assertEqual(rendered["toolingSha"], "a" * 40)
        self.assertEqual(rendered["nextAction"], "apply")
        self.assertEqual(rendered["releaseIdentity"], "3.119.0-preview.1")
        self.assertEqual(rendered["releaseBranch"], "release/3.119.0-preview.1")
        self.assertEqual(rendered["releaseVersion"], "3.119.0-preview.1")
        self.assertEqual(rendered["planDigest"], plan["planDigest"])
        self.assertEqual(rendered["warnings"], ["maintenance branch will be created"])

    def test_identity_never_carries_a_build_revision(self):
        plan = make_prepare_plan()
        rendered = summary.summarize_document(plan)
        # Prepare release versions are never suffixed with a CI build number.
        self.assertEqual(rendered["releaseIdentity"], rendered["releaseVersion"])

    def test_validates_against_schema(self):
        plan = make_prepare_plan()
        rendered = summary.summarize_document(plan)
        common.validate_against_schema(rendered, summary.SUMMARY_SCHEMA)


class SummarizeFinishTests(unittest.TestCase):
    def test_derives_flat_fields(self):
        plan = make_finish_plan()
        rendered = summary.summarize_document(plan)
        self.assertEqual(rendered["operation"], "finish")
        self.assertEqual(rendered["toolingSha"], "a" * 40)
        self.assertEqual(rendered["nextAction"], "create-draft")
        # The identity strips the CI build revision that the exact requested
        # public version carries.
        self.assertEqual(rendered["releaseIdentity"], "3.119.0-preview.1")
        self.assertEqual(rendered["releaseVersion"], "3.119.0-preview.1.42")
        self.assertEqual(rendered["releaseBranch"], "release/3.119.0-preview.1")
        self.assertEqual(rendered["planDigest"], plan["planDigest"])

    def test_release_branch_comes_from_the_verified_receipt(self):
        # release.branch is populated from the receipt's verified
        # sourceBranch when the plan was built -- it must be reported even
        # if it differs from a naive f"release/{raw}" reconstruction.
        plan = make_finish_plan()
        plan["release"]["branch"] = "release/3.119.x"
        plan = common.with_digest({k: v for k, v in plan.items() if k != "planDigest"})
        rendered = summary.summarize_document(plan)
        self.assertEqual(rendered["releaseBranch"], "release/3.119.x")

    def test_validates_against_schema(self):
        plan = make_finish_plan()
        rendered = summary.summarize_document(plan)
        common.validate_against_schema(rendered, summary.SUMMARY_SCHEMA)


class SummarizeResultDocumentTests(unittest.TestCase):
    """A command result (apply/create-draft/plan-publication/publish/closeout)
    carries the same envelope as a plan, minus ``operation``/``schemaVersion``,
    and must summarize identically."""

    def test_summarizes_a_result_envelope(self):
        plan = make_finish_plan()
        result = common.build_envelope(
            plan, next_action="publish", tag="v3.119.0-preview.1", readyToPublish=True
        )
        rendered = summary.summarize_document(result)
        self.assertNotIn("operation", rendered)
        self.assertEqual(rendered["nextAction"], "publish")
        self.assertEqual(rendered["releaseIdentity"], "3.119.0-preview.1")
        self.assertEqual(rendered["releaseVersion"], "3.119.0-preview.1.42")
        self.assertEqual(rendered["releaseBranch"], "release/3.119.0-preview.1")
        self.assertEqual(rendered["planDigest"], plan["planDigest"])

    def test_validates_against_schema(self):
        plan = make_prepare_plan()
        result = common.build_envelope(plan, next_action="done", operations=[])
        rendered = summary.summarize_document(result)
        common.validate_against_schema(rendered, summary.SUMMARY_SCHEMA)


class SummarizeErrorTests(unittest.TestCase):
    def test_rejects_document_missing_next_action(self):
        plan = make_prepare_plan()
        del plan["nextAction"]
        with self.assertRaisesRegex(PlanError, "nextAction"):
            summary.summarize_document(plan)

    def test_rejects_document_missing_release_branch(self):
        plan = make_prepare_plan()
        del plan["release"]["branch"]
        with self.assertRaisesRegex(PlanError, "release.branch"):
            summary.summarize_document(plan)

    def test_rejects_document_missing_tooling_sha(self):
        plan = make_prepare_plan()
        del plan["toolingSha"]
        with self.assertRaises(PlanError):
            summary.summarize_document(plan)

    def test_rejects_document_missing_plan_digest(self):
        plan = make_prepare_plan()
        del plan["planDigest"]
        with self.assertRaises(PlanError):
            summary.summarize_document(plan)


class RenderMarkdownPrepareTests(unittest.TestCase):
    def test_includes_header_and_summary_fields(self):
        plan = make_prepare_plan()
        text = summary.render_markdown(plan)
        self.assertTrue(text.startswith("# Release 3.119.0-preview.1\n"))
        self.assertIn("Next action", text)
        self.assertIn("apply", text)
        self.assertIn("Plan digest", text)
        self.assertIn(plan["planDigest"], text)
        self.assertIn("Tooling SHA", text)
        self.assertIn("a" * 40, text)

    def test_includes_warnings_section(self):
        plan = make_prepare_plan()
        text = summary.render_markdown(plan)
        self.assertIn("## Warnings", text)
        self.assertIn("maintenance branch will be created", text)

    def test_warnings_section_reports_none_when_empty(self):
        plan = make_prepare_plan(warnings=[])
        text = summary.render_markdown(plan)
        self.assertIn("## Warnings\n\n_none_", text)

    def test_includes_operations_table(self):
        plan = make_prepare_plan(
            operations=[
                {"id": "create-maintenance-branch", "kind": "git-ref", "status": "pending", "detail": "release/3.119.x"},
                {"id": "create-skia-ref", "kind": "github-ref", "status": "done", "detail": None},
            ]
        )
        text = summary.render_markdown(plan)
        self.assertIn("## Operations", text)
        self.assertIn("create-maintenance-branch", text)
        self.assertIn("create-skia-ref", text)
        self.assertIn("pending", text)

    def test_no_operations_section_when_absent(self):
        plan = make_prepare_plan()
        del plan["operations"]
        plan = common.with_digest({k: v for k, v in plan.items() if k != "planDigest"})
        text = summary.render_markdown(plan)
        self.assertNotIn("## Operations", text)

    def test_is_deterministic_across_calls(self):
        plan = make_prepare_plan()
        first = summary.render_markdown(plan)
        second = summary.render_markdown(plan)
        self.assertEqual(first, second)

    def test_is_deterministic_regardless_of_key_order(self):
        plan = make_prepare_plan()
        reordered = dict(reversed(list(plan.items())))
        self.assertEqual(summary.render_markdown(plan), summary.render_markdown(reordered))


class RenderMarkdownFinishTests(unittest.TestCase):
    def test_includes_receipt_packages_tag_and_draft_sections(self):
        plan = make_finish_plan()
        plan["receipt"]["packages"] = [
            {"id": "SkiaSharp", "version": "3.119.0-preview.1.42", "sourceCommit": "d" * 40, "sourceBranch": "release/3.119.0-preview.1"},
        ]
        plan = common.with_digest({k: v for k, v in plan.items() if k != "planDigest"})
        text = summary.render_markdown(plan)
        self.assertIn("## Receipt", text)
        self.assertIn("### Packages", text)
        self.assertIn("SkiaSharp", text)
        self.assertIn("## Tag", text)
        self.assertIn("v3.119.0-preview.1", text)
        self.assertIn("Previous tag: `v3.118.0`", text)
        self.assertIn("## Draft", text)

    def test_previous_tag_none_renders_explicitly(self):
        plan = make_finish_plan(previousTag=None)
        text = summary.render_markdown(plan)
        self.assertIn("Previous tag: _none_", text)


class RenderMarkdownResultDocumentTests(unittest.TestCase):
    def test_renders_operations_and_additional_fields_for_a_result(self):
        base_plan = common.with_digest(
            {
                "toolingSha": "a" * 40,
                "release": {"identity": "3.119.0-preview.1", "version": "3.119.0-preview.1.42", "branch": "release/3.119.0-preview.1"},
            }
        )
        result = common.build_envelope(
            base_plan, next_action="plan-publication", tag="v3.119.0-preview.1",
            tagStatus="done", draftStatus="done", bodySha256="abc123", alreadyExists=False, isPublished=False,
        )
        text = summary.render_markdown(result)
        self.assertIn("# Release 3.119.0-preview.1", text)
        self.assertNotIn("Operation", text)  # no "operation" key on a result document
        self.assertIn("## Tag", text)
        self.assertIn("v3.119.0-preview.1", text)
        self.assertIn("## Additional fields", text)
        self.assertIn("tagStatus", text)
        self.assertIn("bodySha256", text)

    def test_renders_closeout_results_table(self):
        base_plan = common.with_digest(
            {
                "toolingSha": "a" * 40,
                "release": {"identity": "3.119.0-preview.1", "version": "3.119.0-preview.1.42", "branch": "release/3.119.0-preview.1"},
            }
        )
        result = common.build_envelope(
            base_plan, next_action="done",
            results=[{"milestone": "3.119.0-preview.1", "status": "done", "movedTo": "3.119.0-preview.2"}],
            warnings=[],
        )
        text = summary.render_markdown(result)
        self.assertIn("## Results", text)
        self.assertIn("3.119.0-preview.1", text)
        self.assertIn("3.119.0-preview.2", text)

    def test_table_cells_escape_pipe_characters(self):
        base_plan = common.with_digest(
            {
                "toolingSha": "a" * 40,
                "release": {"identity": "3.119.0-preview.1", "version": "3.119.0-preview.1.42", "branch": "release/3.119.0-preview.1"},
            }
        )
        result = common.build_envelope(
            base_plan, next_action="done",
            operations=[{"id": "x", "status": "done", "detail": "a | b"}],
        )
        text = summary.render_markdown(result)
        self.assertIn("a \\| b", text)


if __name__ == "__main__":
    unittest.main()
