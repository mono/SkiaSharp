#!/usr/bin/env python3
"""Regression tests for Copilot setup profile isolation."""

from __future__ import annotations

from pathlib import Path
import unittest


REPO_ROOT = Path(__file__).resolve().parents[3]
CODING_WORKFLOW = REPO_ROOT / ".github" / "workflows" / "copilot-setup-steps.yml"
REVIEW_WORKFLOW = REPO_ROOT / ".github" / "workflows" / "copilot-code-review.yml"
AUTOMATION_WORKFLOW = (
    REPO_ROOT / ".github" / "workflows" / "automation-tooling-tests.yml"
)
CLOUD_SETUP = REPO_ROOT / "scripts" / "infra" / "cloud" / "setup.sh"
RESTORE_TOOLS = CLOUD_SETUP.with_name("restore-dotnet-tools.sh")
PREBUILT_NATIVES = CLOUD_SETUP.with_name("download-prebuilt-natives.sh")


class CopilotSetupWorkflowTests(unittest.TestCase):
    def test_coding_agent_bootstraps_managed_workspace(self):
        workflow = CODING_WORKFLOW.read_text(encoding="utf-8")
        setup = CLOUD_SETUP.read_text(encoding="utf-8")
        prebuilt_natives = PREBUILT_NATIVES.read_text(encoding="utf-8")

        self.assertRegex(workflow, r"(?m)^  copilot-setup-steps:$")
        self.assertIn(
            "bash scripts/infra/cloud/setup.sh --restore-tools --download-prebuilt-natives",
            workflow,
        )
        self.assertIn('if [[ "$restore_tools" == true ]]', setup)
        self.assertIn('if [[ "$download_prebuilt_natives" == true ]]', setup)
        self.assertIn(
            "dotnet cake --target=externals-download",
            prebuilt_natives,
        )

    def test_code_review_avoids_prebuilt_native_download(self):
        workflow = REVIEW_WORKFLOW.read_text(encoding="utf-8")
        restore_tools = RESTORE_TOOLS.read_text(encoding="utf-8")

        self.assertRegex(workflow, r"(?m)^  copilot-setup-steps:$")
        self.assertIn(
            "bash scripts/infra/cloud/setup.sh --restore-tools",
            workflow,
        )
        self.assertNotIn("--download-prebuilt-natives", workflow)
        self.assertNotIn("--install-android-workload", workflow)
        self.assertNotIn("externals-download", restore_tools)

    def test_optional_bootstraps_are_explicit(self):
        coding_workflow = CODING_WORKFLOW.read_text(encoding="utf-8")
        review_workflow = REVIEW_WORKFLOW.read_text(encoding="utf-8")

        self.assertIn("--download-prebuilt-natives", coding_workflow)
        self.assertNotIn("--download-prebuilt-natives", review_workflow)
        self.assertNotIn("--install-android-workload", coding_workflow)
        self.assertNotIn("--install-android-workload", review_workflow)

    def test_automation_tests_follow_shared_setup_changes(self):
        workflow = AUTOMATION_WORKFLOW.read_text(encoding="utf-8")

        self.assertEqual(
            2,
            workflow.count('      - "scripts/infra/cloud/**"'),
        )
        self.assertEqual(
            2,
            workflow.count('      - "scripts/infra/tests/setup-software-gpu.sh"'),
        )


if __name__ == "__main__":
    unittest.main()
