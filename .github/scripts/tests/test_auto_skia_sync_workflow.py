#!/usr/bin/env python3
"""Regression tests for the auto-Skia-sync host bootstrap."""

from __future__ import annotations

from pathlib import Path
import re
import unittest


WORKFLOW = (
    Path(__file__).resolve().parents[2] / "workflows" / "auto-skia-sync.md"
)


class AutoSkiaSyncWorkflowTests(unittest.TestCase):
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
