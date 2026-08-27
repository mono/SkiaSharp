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
        "input": {"integrationTarget": "main", "requestedVersion": None},
        "release": {
            "version": "3.119.0-preview.1",
            "numeric": "3.119.0",
            "label": "preview.1",
            "releaseType": "preview",
            "releaseBranch": "release/3.119.0-preview.1",
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
        rendered = summary.summarize_plan(plan)
        self.assertEqual(rendered["schemaVersion"], 1)
        self.assertEqual(rendered["operation"], "prepare")
        self.assertEqual(rendered["toolingSha"], "a" * 40)
        self.assertEqual(rendered["releaseIdentity"], "3.119.0-preview.1")
        self.assertEqual(rendered["releaseBranch"], "release/3.119.0-preview.1")
        self.assertEqual(rendered["releaseVersion"], "3.119.0-preview.1")
        self.assertEqual(rendered["digest"], plan["digest"])
        self.assertEqual(rendered["warnings"], ["maintenance branch will be created"])

    def test_identity_never_carries_a_build_revision(self):
        plan = make_prepare_plan()
        rendered = summary.summarize_plan(plan)
        # Prepare release versions are never suffixed with a CI build number.
        self.assertEqual(rendered["releaseIdentity"], rendered["releaseVersion"])

    def test_validates_against_schema(self):
        plan = make_prepare_plan()
        rendered = summary.summarize_plan(plan)
        common.validate_against_schema(rendered, summary.SUMMARY_SCHEMA)


class SummarizeFinishTests(unittest.TestCase):
    def test_derives_flat_fields(self):
        plan = make_finish_plan()
        rendered = summary.summarize_plan(plan)
        self.assertEqual(rendered["operation"], "finish")
        self.assertEqual(rendered["toolingSha"], "a" * 40)
        # The identity strips the CI build revision that the exact requested
        # public version carries.
        self.assertEqual(rendered["releaseIdentity"], "3.119.0-preview.1")
        self.assertEqual(rendered["releaseVersion"], "3.119.0-preview.1.42")
        self.assertEqual(rendered["releaseBranch"], "release/3.119.0-preview.1")
        self.assertEqual(rendered["digest"], plan["digest"])

    def test_release_branch_comes_from_the_verified_receipt(self):
        # The receipt's sourceBranch is the branch actually embedded in and
        # verified against the published package -- it must be reported even
        # if it differs from a naive f"release/{raw}" reconstruction.
        plan = make_finish_plan()
        plan["receipt"]["sourceBranch"] = "release/3.119.x"
        plan = common.with_digest({k: v for k, v in plan.items() if k != "digest"})
        rendered = summary.summarize_plan(plan)
        self.assertEqual(rendered["releaseBranch"], "release/3.119.x")

    def test_validates_against_schema(self):
        plan = make_finish_plan()
        rendered = summary.summarize_plan(plan)
        common.validate_against_schema(rendered, summary.SUMMARY_SCHEMA)


class SummarizeErrorTests(unittest.TestCase):
    def test_rejects_unknown_operation(self):
        plan = make_prepare_plan(operation="something-else")
        with self.assertRaisesRegex(PlanError, "unknown operation"):
            summary.summarize_plan(plan)

    def test_rejects_missing_operation(self):
        plan = make_prepare_plan()
        del plan["operation"]
        with self.assertRaises(PlanError):
            summary.summarize_plan(plan)


if __name__ == "__main__":
    unittest.main()
