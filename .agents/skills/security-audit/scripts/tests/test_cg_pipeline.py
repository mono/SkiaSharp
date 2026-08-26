#!/usr/bin/env python3

import importlib.util
import json
from pathlib import Path
import sys
import unittest


SCRIPT_PATH = Path(__file__).resolve().parent.parent / "query-cg-alerts.py"
ROOT = Path(__file__).resolve().parents[5]
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

    def test_tsa_upload_tracks_work_in_devdiv(self):
        path = ROOT / "scripts" / "infra" / "security" / "tsaoptions-v2.json"
        source = path.read_text(encoding="utf-8")
        data = json.loads(source)
        self.assertEqual(
            data["instanceUrl"],
            "https://devdiv.visualstudio.com/",
        )
        self.assertEqual(data["projectName"], "DevDiv")
        self.assertEqual(
            data["areaPath"],
            r"DevDiv\.NET MAUI\SkiaSharp",
        )
        self.assertEqual(data["iterationPath"], "DevDiv")

if __name__ == "__main__":
    unittest.main()
