#!/usr/bin/env python3

import importlib.util
from pathlib import Path
import sys
import unittest


SCRIPT_PATH = Path(__file__).resolve().parent.parent / "query-cg-alerts.py"
SPEC = importlib.util.spec_from_file_location("query_cg_alerts", SCRIPT_PATH)
query = importlib.util.module_from_spec(SPEC)
sys.modules[SPEC.name] = query
SPEC.loader.exec_module(query)


class ComponentGovernancePipelineTests(unittest.TestCase):
    def test_combined_build_pipeline_is_the_only_shipped_scan_source(self):
        self.assertEqual(query.ORG, "https://dev.azure.com/dnceng")
        self.assertEqual(query.PROJECT, "internal")
        self.assertEqual(
            query.PIPELINES,
            {
                "build": {
                    "id": 1642,
                    "name": "skiasharp-package",
                }
            },
        )

    def test_viewer_links_to_dnceng_internal(self):
        viewer = (SCRIPT_PATH.parent / "viewer.html").read_text(
            encoding="utf-8"
        )
        self.assertIn(
            "https://dev.azure.com/dnceng/internal/_build?definitionId=",
            viewer,
        )
        self.assertNotIn("devdiv", viewer.lower())

if __name__ == "__main__":
    unittest.main()
