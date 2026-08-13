#!/usr/bin/env python3

import json
import subprocess
import sys
import tempfile
import unittest
from pathlib import Path


SKILL = Path(__file__).resolve().parents[1]
SCRIPTS = SKILL / "scripts"
FIXTURES = SKILL / "evals" / "fixtures"


class TsaIntegrationTests(unittest.TestCase):
    def run_script(self, name, *args, expected=0):
        result = subprocess.run(
            [sys.executable, str(SCRIPTS / name), *map(str, args)],
            capture_output=True,
            text=True,
        )
        self.assertEqual(expected, result.returncode, result.stdout + result.stderr)
        return result

    def test_query_correlate_validate_and_render(self):
        with tempfile.TemporaryDirectory() as temp_dir:
            temp = Path(temp_dir)
            cache = temp / "tsa.json"
            report = temp / "report.json"
            report.write_text(
                (FIXTURES / "security-audit-report.json").read_text(encoding="utf-8"),
                encoding="utf-8",
            )

            self.run_script(
                "query-tsa-work-items.py",
                "--input-json", FIXTURES / "tsa-query-raw.json",
                "--output", cache,
            )
            self.run_script(
                "correlate-tsa-work-items.py",
                "--report", report,
                "--tsa-cache", cache,
            )

            data = json.loads(report.read_text(encoding="utf-8"))
            tsa = data["tsaWorkItems"]
            self.assertEqual(2, tsa["summary"]["total"])
            self.assertEqual(1, tsa["summary"]["active"])
            self.assertEqual(1, tsa["summary"]["historical"])
            self.assertEqual("matched", tsa["items"][0]["correlation"]["status"])
            self.assertEqual("unmatched", tsa["items"][1]["correlation"]["status"])

            self.run_script("validate-security-audit.py", report)
            html = temp / "report.html"
            markdown = temp / "report.md"
            self.run_script("render-security-audit.py", report, html)
            self.run_script("render-security-audit-md.py", report, markdown)
            self.assertIn("TSA Azure Boards", html.read_text(encoding="utf-8"))
            self.assertIn("TSA Azure Boards Work Items", markdown.read_text(encoding="utf-8"))
            self.assertIn("TSA-skiasharp.skiasharp_main", markdown.read_text(encoding="utf-8"))

    def test_error_status_fails_validation(self):
        with tempfile.TemporaryDirectory() as temp_dir:
            report = json.loads(
                (FIXTURES / "security-audit-report.json").read_text(encoding="utf-8")
            )
            report["tsaWorkItems"] = {
                "queryStatus": "error",
                "queriedAt": "2026-08-13T07:00:01+00:00",
                "organization": "https://dev.azure.com/devdiv",
                "project": "DevDiv",
                "codebaseTag": "TSA-skiasharp.skiasharp_main",
                "portalSearchUrl": "https://example.invalid/TSA-skiasharp.skiasharp_main",
                "cacheFile": "tsa.json",
                "error": "authentication failed",
                "summary": {
                    "total": 0,
                    "active": 0,
                    "historical": 0,
                    "byState": {},
                    "byCategory": {},
                    "byTool": {},
                    "correlated": 0,
                    "unmatched": 0
                },
                "groups": [],
                "items": []
            }
            path = Path(temp_dir) / "error-report.json"
            path.write_text(json.dumps(report), encoding="utf-8")
            result = self.run_script("validate-security-audit.py", path, expected=1)
            self.assertIn("queryStatus", result.stdout)

    def test_empty_query_is_an_error_not_success(self):
        with tempfile.TemporaryDirectory() as temp_dir:
            temp = Path(temp_dir)
            empty = temp / "empty.json"
            empty.write_text("[]", encoding="utf-8")
            cache = temp / "tsa.json"
            result = self.run_script(
                "query-tsa-work-items.py",
                "--input-json", empty,
                "--output", cache,
                expected=1,
            )
            data = json.loads(cache.read_text(encoding="utf-8"))
            self.assertEqual("error", data["queryStatus"])
            self.assertIn("no items", data["error"])
            self.assertIn("error", result.stdout)

    def test_renderer_escapes_mixed_case_script_terminator(self):
        with tempfile.TemporaryDirectory() as temp_dir:
            temp = Path(temp_dir)
            report = json.loads(
                (FIXTURES / "security-audit-report.json").read_text(encoding="utf-8")
            )
            cache = temp / "tsa.json"
            self.run_script(
                "query-tsa-work-items.py",
                "--input-json", FIXTURES / "tsa-query-raw.json",
                "--output", cache,
            )
            cache_data = json.loads(cache.read_text(encoding="utf-8"))
            cache_data["items"][0]["title"] = "</ScRiPt><script>alert(1)</script>"
            cache.write_text(json.dumps(cache_data), encoding="utf-8")
            report_path = temp / "report.json"
            report_path.write_text(json.dumps(report), encoding="utf-8")
            self.run_script(
                "correlate-tsa-work-items.py",
                "--report", report_path,
                "--tsa-cache", cache,
            )
            html = temp / "report.html"
            self.run_script("render-security-audit.py", report_path, html)
            content = html.read_text(encoding="utf-8")
            self.assertNotIn("</ScRiPt><script>alert(1)</script>", content)
            self.assertIn("\\u003c/ScRiPt\\u003e", content)


if __name__ == "__main__":
    unittest.main()
