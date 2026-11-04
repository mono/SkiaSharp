#!/usr/bin/env python3
"""Regression tests for the auto-Skia-sync dispatch and host bootstrap."""

from __future__ import annotations

from pathlib import Path
import re
import unittest

import yaml


WORKFLOW = (
    Path(__file__).resolve().parents[2] / "workflows" / "auto-skia-sync.md"
)
COMPILED_WORKFLOW = WORKFLOW.with_suffix(".lock.yml")


class AutoSkiaSyncWorkflowTests(unittest.TestCase):
    def test_config_archive_surrounds_artifact_transport_and_runtime_restoration(self):
        compiled = yaml.safe_load(COMPILED_WORKFLOW.read_text(encoding="utf-8"))
        steps = compiled["jobs"]["agent"]["steps"]
        names = [step.get("name") for step in steps]
        extract = names.index("Extract trusted agent configuration")
        self.assertGreater(extract, names.index("Download activation artifact"))
        self.assertLess(extract, names.index("Restore agent config folders from base branch"))
        self.assertLess(extract, names.index("Prepare Skia checkout"))
        self.assertLess(extract, names.index("Install native build dependencies"))
        activation = compiled["jobs"]["activation"]["steps"]
        names = [step.get("name") for step in activation]
        archive = names.index("Archive trusted agent configuration")
        self.assertGreater(archive, names.index("Save agent config folders for base branch restoration"))
        self.assertLess(archive, names.index("Upload activation artifact"))
        checkout = next(step for step in activation if step.get("name") == "Checkout .github and .agents folders")
        self.assertNotIn("ref", checkout["with"])
        snapshot = next(step for step in activation if step.get("name") == "Save agent config folders for base branch restoration")
        self.assertEqual(".agents .github", snapshot["env"]["GH_AW_AGENT_FOLDERS"])
        self.assertEqual("AGENTS.md", snapshot["env"]["GH_AW_AGENT_FILES"])

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
                    3,
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

    def test_publication_preserves_history_and_isolates_the_write_token(self):
        lines = WORKFLOW.read_text(encoding="utf-8").splitlines()
        workflow = yaml.safe_load("\n".join(lines[1:lines.index("---", 1)]))
        publisher = workflow["jobs"]["publish-sync"]
        steps = publisher["steps"]
        self.assertEqual("${{ secrets.SKIASHARP_AUTOBUMP_TOKEN }}", steps[-1]["env"]["GH_TOKEN"])
        self.assertEqual("${{ github.sha }}", steps[0]["with"]["ref"])
        self.assertFalse(steps[0]["with"]["persist-credentials"])
        for key in ("env", "steps", "pre-agent-steps", "post-steps"):
            self.assertNotIn("secrets.", str(workflow.get(key, {})))
        self.assertNotIn("create-pull-request", workflow["safe-outputs"])
        self.assertNotIn("jobs", workflow["safe-outputs"])
        compiled = yaml.safe_load(COMPILED_WORKFLOW.read_text(encoding="utf-8"))
        self.assertNotIn("SKIASHARP_AUTOBUMP_TOKEN", str(compiled["jobs"]["agent"]))
        self.assertIn("publish-sync", compiled["jobs"])

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
