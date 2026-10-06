#!/usr/bin/env python3
"""Regression tests for the auto-Skia-sync dispatch and host bootstrap."""

from __future__ import annotations

from pathlib import Path
import re
import unittest


WORKFLOW = (
    Path(__file__).resolve().parents[2] / "workflows" / "auto-skia-sync.md"
)
COMPILED_WORKFLOW = WORKFLOW.with_suffix(".lock.yml")


class AutoSkiaSyncWorkflowTests(unittest.TestCase):
    def test_dispatch_inputs_keep_target_before_optional_branch(self):
        for path, expected in (
            (WORKFLOW, ["target", "target_branch"]),
            (COMPILED_WORKFLOW, ["aw_context", "target", "target_branch"]),
        ):
            with self.subTest(workflow=path.name):
                workflow = path.read_text(encoding="utf-8")
                dispatch = re.search(
                    r"(?m)^  workflow_dispatch:\n    inputs:\n"
                    r"((?:[ \t]+[^\n]*\n)+)",
                    workflow,
                )
                self.assertIsNotNone(dispatch)
                inputs = re.findall(r"(?m)^      (\w+):", dispatch.group(1))
                self.assertEqual(expected, inputs)
                self.assertEqual(sorted(inputs), inputs)

    def test_target_branch_reaches_both_detection_steps_and_concurrency(self):
        for path in (WORKFLOW, COMPILED_WORKFLOW):
            with self.subTest(workflow=path.name):
                workflow = path.read_text(encoding="utf-8")
                self.assertEqual(
                    2,
                    len(re.findall(
                        r"(?m)^\s+SYNC_BASE_BRANCH: "
                        r"\$\{\{ github\.event\.inputs\.target_branch \}\}$",
                        workflow,
                    )),
                )
                self.assertIn(
                    "group: skia-upstream-sync-${{ "
                    "github.event.inputs.target_branch || 'auto' }}",
                    workflow,
                )
                self.assertNotIn("github.event.inputs.base_branch", workflow)

    def test_create_pull_request_allows_resolved_sync_base_branch(self):
        workflow = WORKFLOW.read_text(encoding="utf-8")
        compiled_workflow = COMPILED_WORKFLOW.read_text(encoding="utf-8")

        self.assertIn(
            'allowed-base-branches: "${{ github.event.inputs.target_branch || '
            "'main,release/*' }}\"",
            workflow,
        )
        self.assertEqual(
            2,
            compiled_workflow.count("allowed_base_branches"),
        )
        self.assertEqual(
            2,
            compiled_workflow.count(
                "${{ github.event.inputs.target_branch || 'main,release/*' }}"
            ),
        )

    def test_android_workload_uses_repository_sdk_bootstrap(self):
        workflow = WORKFLOW.read_text(encoding="utf-8")

        self.assertIn(
            "./eng/common/dotnet.sh workload install android --skip-sign-check",
            workflow,
        )
        self.assertNotRegex(
            workflow,
            re.compile(
                r"(?m)^\s*dotnet workload install android --skip-sign-check\s*$"
            ),
        )


if __name__ == "__main__":
    unittest.main()
