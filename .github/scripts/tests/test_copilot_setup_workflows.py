#!/usr/bin/env python3
"""Regression tests for Copilot setup profile isolation."""

from __future__ import annotations

from pathlib import Path
import re
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


def profile_body(script: str, profile: str) -> str:
    match = re.search(
        rf"(?ms)^\s+{re.escape(profile)}\)\n(?P<body>.*?)^\s+;;$",
        script,
    )
    if match is None:
        raise AssertionError(f"Missing {profile} setup profile")
    return match.group("body")


class CopilotSetupWorkflowTests(unittest.TestCase):
    def test_coding_agent_bootstraps_managed_workspace(self):
        workflow = CODING_WORKFLOW.read_text(encoding="utf-8")
        setup = CLOUD_SETUP.read_text(encoding="utf-8")
        prebuilt_natives = PREBUILT_NATIVES.read_text(encoding="utf-8")

        self.assertRegex(workflow, r"(?m)^  copilot-setup-steps:$")
        self.assertIn(
            "bash scripts/infra/cloud/setup.sh coding-agent",
            workflow,
        )
        coding_profile = profile_body(setup, "coding-agent")
        self.assertIn("restore-dotnet-tools.sh", coding_profile)
        self.assertIn("download-prebuilt-natives.sh", coding_profile)
        self.assertIn(
            "dotnet cake --target=externals-download",
            prebuilt_natives,
        )

    def test_code_review_avoids_prebuilt_native_download(self):
        workflow = REVIEW_WORKFLOW.read_text(encoding="utf-8")
        setup = CLOUD_SETUP.read_text(encoding="utf-8")
        restore_tools = RESTORE_TOOLS.read_text(encoding="utf-8")

        self.assertRegex(workflow, r"(?m)^  copilot-setup-steps:$")
        self.assertIn(
            "bash scripts/infra/cloud/setup.sh code-review",
            workflow,
        )
        review_profile = profile_body(setup, "code-review")
        self.assertIn("restore-dotnet-tools.sh", review_profile)
        self.assertNotIn("download-prebuilt-natives.sh", review_profile)
        self.assertNotIn("externals-download", restore_tools)

    def test_prebuilt_natives_are_isolated_to_coding_agent(self):
        setup = CLOUD_SETUP.read_text(encoding="utf-8")
        shared_setup = setup.rsplit('case "$profile" in', 1)[0]

        self.assertNotIn("download-prebuilt-natives.sh", shared_setup)
        self.assertNotIn("externals-download", shared_setup)
        for profile in ("code-review", "skia-sync"):
            with self.subTest(profile=profile):
                body = profile_body(setup, profile)
                self.assertNotIn("download-prebuilt-natives.sh", body)
                self.assertNotIn("externals-download", body)

    def test_automation_tests_follow_shared_setup_changes(self):
        workflow = AUTOMATION_WORKFLOW.read_text(encoding="utf-8")

        self.assertEqual(
            2,
            workflow.count('      - "scripts/infra/cloud/**"'),
        )
        self.assertEqual(
            2,
            workflow.count('      - "scripts/infra/tests/start-xvfb.sh"'),
        )


if __name__ == "__main__":
    unittest.main()
