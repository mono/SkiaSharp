#!/usr/bin/env python3
"""Regression tests for Copilot setup profile isolation."""

from __future__ import annotations

from pathlib import Path
import re
import unittest


REPO_ROOT = Path(__file__).resolve().parents[3]
CODING_WORKFLOW = REPO_ROOT / ".github" / "workflows" / "copilot-setup-steps.yml"
REVIEW_WORKFLOW = REPO_ROOT / ".github" / "workflows" / "copilot-code-review.yml"
CLOUD_SETUP = REPO_ROOT / "scripts" / "infra" / "cloud" / "setup.sh"


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

        self.assertIn(
            "bash scripts/infra/cloud/setup.sh coding-agent",
            workflow,
        )
        coding_profile = profile_body(setup, "coding-agent")
        self.assertIn("restore-dotnet-tools.sh", coding_profile)
        self.assertIn("download-prebuilt-natives.sh", coding_profile)

    def test_code_review_avoids_prebuilt_native_download(self):
        workflow = REVIEW_WORKFLOW.read_text(encoding="utf-8")
        setup = CLOUD_SETUP.read_text(encoding="utf-8")

        self.assertIn(
            "bash scripts/infra/cloud/setup.sh code-review",
            workflow,
        )
        review_profile = profile_body(setup, "code-review")
        self.assertIn("restore-dotnet-tools.sh", review_profile)
        self.assertNotIn("download-prebuilt-natives.sh", review_profile)


if __name__ == "__main__":
    unittest.main()
