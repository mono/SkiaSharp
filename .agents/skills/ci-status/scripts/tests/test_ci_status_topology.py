#!/usr/bin/env python3

import importlib.util
from pathlib import Path
import sys
import unittest


SCRIPT_PATH = Path(__file__).resolve().parent.parent / "ci-status.py"
SPEC = importlib.util.spec_from_file_location("ci_status", SCRIPT_PATH)
ci_status = importlib.util.module_from_spec(SPEC)
sys.modules[SPEC.name] = ci_status
SPEC.loader.exec_module(ci_status)


class CiStatusTopologyTests(unittest.TestCase):
    def test_public_pipeline_is_unchanged(self):
        self.assertEqual(
            ci_status.PUBLIC_PIPELINES,
            [
                {
                    "name": "mono-SkiaSharp",
                    "id": 345,
                    "org": "https://dev.azure.com/dnceng-public",
                    "project": "public",
                }
            ],
        )

    def test_internal_pipeline_is_combined_build_then_tests(self):
        self.assertEqual(
            ci_status.INTERNAL_PIPELINES,
            [
                {
                    "name": "skiasharp-package",
                    "id": 1642,
                    "org": "https://dev.azure.com/dnceng",
                    "project": "internal",
                },
                {
                    "name": "skiasharp-tests",
                    "id": 1630,
                    "org": "https://dev.azure.com/dnceng",
                    "project": "internal",
                },
            ],
        )

if __name__ == "__main__":
    unittest.main()
