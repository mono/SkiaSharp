#!/usr/bin/env python3

import json
import os
import subprocess
import sys
import tempfile
import unittest
from pathlib import Path


SKILL = Path(__file__).resolve().parents[1]
SCRIPTS = SKILL / "scripts"
FIXTURES = SKILL / "evals" / "fixtures"


class TsaIntegrationTests(unittest.TestCase):
    def run_script(self, name, *args, expected=0, env=None):
        result = subprocess.run(
            [sys.executable, str(SCRIPTS / name), *map(str, args)],
            capture_output=True,
            text=True,
            env=env,
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
            self.assertEqual("https://dev.azure.com/dnceng", tsa["organization"])
            self.assertEqual("internal", tsa["project"])
            self.assertFalse(tsa["emptyResult"])
            self.assertEqual("matched", tsa["items"][0]["correlation"]["status"])
            self.assertEqual("unmatched", tsa["items"][1]["correlation"]["status"])
            self.assertEqual(
                "binding/SkiaSharp/SKPathBuilder.cs",
                tsa["items"][0]["impactedFile"],
            )
            self.assertEqual("High", tsa["items"][0]["evidence"]["risk"])
            self.assertIn(
                "Microsoft.VSTS.TCM.ReproSteps",
                tsa["items"][0]["rawFields"],
            )

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
                "organization": "https://dev.azure.com/dnceng",
                "project": "internal",
                "codebaseTag": "TSA-skiasharp.skiasharp_main",
                "portalSearchUrl": "https://example.invalid/TSA-skiasharp.skiasharp_main",
                "cacheFile": "tsa.json",
                "emptyResult": False,
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

    def test_empty_dnceng_query_is_explicit_success(self):
        with tempfile.TemporaryDirectory() as temp_dir:
            temp = Path(temp_dir)
            empty = temp / "empty.json"
            empty.write_text("[]", encoding="utf-8")
            cache = temp / "tsa.json"
            result = self.run_script(
                "query-tsa-work-items.py",
                "--input-json", empty,
                "--output", cache,
            )
            data = json.loads(cache.read_text(encoding="utf-8"))
            self.assertEqual("success", data["queryStatus"])
            self.assertTrue(data["emptyResult"])
            self.assertEqual(0, data["summary"]["total"])
            self.assertIn("success", result.stdout)

    def test_cli_zero_row_response_is_explicit_success(self):
        with tempfile.TemporaryDirectory() as temp_dir:
            temp = Path(temp_dir)
            fake_bin = temp / "bin"
            fake_bin.mkdir()
            fake_az = fake_bin / "az"
            fake_az.write_text("#!/bin/sh\nexit 0\n", encoding="utf-8")
            fake_az.chmod(0o755)
            cache = temp / "tsa.json"
            env = os.environ.copy()
            env["PATH"] = f"{fake_bin}{os.pathsep}{env['PATH']}"

            self.run_script(
                "query-tsa-work-items.py",
                "--output", cache,
                env=env,
            )
            data = json.loads(cache.read_text(encoding="utf-8"))
            self.assertEqual("success", data["queryStatus"])
            self.assertTrue(data["emptyResult"])

    def test_cli_nonempty_response_hydrates_complete_record(self):
        with tempfile.TemporaryDirectory() as temp_dir:
            temp = Path(temp_dir)
            fake_bin = temp / "bin"
            fake_bin.mkdir()
            fake_az = fake_bin / "az"
            fake_az.write_text(
                """#!/usr/bin/env python3
import json
import sys

if sys.argv[1:3] == ["boards", "query"]:
    print(json.dumps([{"id": 2001, "fields": {"System.Id": 2001}}]))
elif sys.argv[1:3] == ["devops", "invoke"]:
    print(json.dumps({"value": [{
        "id": 2001,
        "fields": {
            "System.Id": 2001,
            "System.Title": "[binskim:Error]: BA2008 (in libSkiaSharp.dll)",
            "System.State": "To Do",
            "System.WorkItemType": "Bug",
            "System.Tags": "TSA; TSA-Security; TSA-BinSkim-BA2008; TSA-skiasharp.skiasharp_main",
            "System.AreaPath": "internal\\\\Dotnet-Core-Engineering",
            "System.IterationPath": "internal",
            "System.Description": "Complete hydrated evidence"
        }
    }]}))
else:
    sys.exit(2)
""",
                encoding="utf-8",
            )
            fake_az.chmod(0o755)
            cache = temp / "tsa.json"
            env = os.environ.copy()
            env["PATH"] = f"{fake_bin}{os.pathsep}{env['PATH']}"

            self.run_script(
                "query-tsa-work-items.py",
                "--output", cache,
                env=env,
            )
            data = json.loads(cache.read_text(encoding="utf-8"))
            self.assertEqual("success", data["queryStatus"])
            self.assertEqual(1, data["summary"]["active"])
            self.assertEqual("active", data["items"][0]["activity"])
            self.assertEqual(
                "Complete hydrated evidence",
                data["items"][0]["rawFields"]["System.Description"],
            )

    def test_same_rule_in_different_files_is_not_collapsed(self):
        with tempfile.TemporaryDirectory() as temp_dir:
            temp = Path(temp_dir)
            rows = json.loads(
                (FIXTURES / "tsa-query-raw.json").read_text(encoding="utf-8")
            )
            duplicate_rule = json.loads(json.dumps(rows[0]))
            duplicate_rule["id"] = 1003
            duplicate_rule["fields"]["System.Id"] = 1003
            duplicate_rule["fields"]["System.Title"] = (
                "[roslynanalyzers:Warning]: CA2265 "
                "(in binding/SkiaSharp/SKRegion.cs)"
            )
            duplicate_rule["fields"]["Microsoft.DevDiv.FileImpacted"] = (
                "binding/SkiaSharp/SKRegion.cs"
            )
            input_path = temp / "rows.json"
            input_path.write_text(json.dumps(rows + [duplicate_rule]), encoding="utf-8")
            cache = temp / "tsa.json"

            self.run_script(
                "query-tsa-work-items.py",
                "--input-json", input_path,
                "--output", cache,
            )
            data = json.loads(cache.read_text(encoding="utf-8"))
            ca2265_groups = [
                group for group in data["groups"]
                if group["ruleIds"] == ["CA2265"]
            ]
            self.assertEqual(2, len(ca2265_groups))
            self.assertEqual(
                {
                    "binding/SkiaSharp/SKPathBuilder.cs",
                    "binding/SkiaSharp/SKRegion.cs",
                },
                {group["occurrence"] for group in ca2265_groups},
            )

    def test_failed_refresh_replaces_stale_success_evidence(self):
        with tempfile.TemporaryDirectory() as temp_dir:
            temp = Path(temp_dir)
            report = json.loads(
                (FIXTURES / "security-audit-report.json").read_text(encoding="utf-8")
            )
            report["tsaWorkItems"] = {
                "queryStatus": "success",
                "items": [{"id": 999}],
                "summary": {"total": 1},
            }
            report_path = temp / "report.json"
            report_path.write_text(json.dumps(report), encoding="utf-8")
            error_cache = temp / "error.json"
            error_cache.write_text(
                json.dumps({
                    "queryStatus": "error",
                    "queriedAt": "2026-09-02T01:00:01+00:00",
                    "organization": "https://dev.azure.com/dnceng",
                    "project": "internal",
                    "codebaseTag": "TSA-skiasharp.skiasharp_main",
                    "wiql": "SELECT [System.Id] FROM WorkItems",
                    "portalSearchUrl": (
                        "https://almsearch.dev.azure.com/dnceng/internal/_search"
                        "?type=workitem&text=TSA-skiasharp.skiasharp_main"
                    ),
                    "cacheFile": str(error_cache),
                    "emptyResult": False,
                    "error": "authentication failed",
                    "summary": {
                        "total": 0,
                        "active": 0,
                        "historical": 0,
                        "byState": {},
                        "byCategory": {},
                        "byTool": {},
                        "correlated": 0,
                        "unmatched": 0,
                    },
                    "groups": [],
                    "items": [],
                }),
                encoding="utf-8",
            )

            self.run_script(
                "correlate-tsa-work-items.py",
                "--report", report_path,
                "--tsa-cache", error_cache,
                expected=1,
            )
            refreshed = json.loads(report_path.read_text(encoding="utf-8"))
            self.assertEqual("error", refreshed["tsaWorkItems"]["queryStatus"])
            self.assertEqual([], refreshed["tsaWorkItems"]["items"])

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

    def test_correlation_uses_hydrated_evidence(self):
        with tempfile.TemporaryDirectory() as temp_dir:
            temp = Path(temp_dir)
            report = temp / "report.json"
            report.write_text(
                (FIXTURES / "security-audit-report.json").read_text(encoding="utf-8"),
                encoding="utf-8",
            )
            cache = temp / "tsa.json"
            self.run_script(
                "query-tsa-work-items.py",
                "--input-json", FIXTURES / "tsa-query-raw.json",
                "--output", cache,
            )
            data = json.loads(cache.read_text(encoding="utf-8"))
            item = data["items"][0]
            item["title"] = "Analyzer warning"
            item["tags"] = [
                tag for tag in item["tags"]
                if tag != "libpng"
            ]
            item["rawFields"]["System.Tags"] = "; ".join(item["tags"])
            item["evidence"]["description"] = "This finding affects libpng."
            item["rawFields"]["System.Description"] = item["evidence"]["description"]
            cache.write_text(json.dumps(data), encoding="utf-8")

            self.run_script(
                "correlate-tsa-work-items.py",
                "--report", report,
                "--tsa-cache", cache,
            )
            correlated = json.loads(report.read_text(encoding="utf-8"))
            self.assertEqual(
                "matched",
                correlated["tsaWorkItems"]["items"][0]["correlation"]["status"],
            )

    def test_operational_sources_cannot_be_duplicated_as_findings(self):
        with tempfile.TemporaryDirectory() as temp_dir:
            report = json.loads(
                (FIXTURES / "security-audit-report.json").read_text(encoding="utf-8")
            )
            report["findings"].append({
                "dependency": "tsa-compliance",
                "status": "needs_attention",
                "cves": [],
                "action": "Resolve active TSA work items.",
            })
            report["summary"]["needsAttention"] = 1
            path = Path(temp_dir) / "duplicate-report.json"
            path.write_text(json.dumps(report), encoding="utf-8")

            result = self.run_script("validate-security-audit.py", path, expected=1)
            self.assertIn("not present in versionVerification", result.stdout)
            self.assertIn("dedicated sections and nextSteps", result.stdout)

    def test_viewer_keeps_cg_out_of_dependency_overview(self):
        viewer = (SCRIPTS / "viewer.html").read_text(encoding="utf-8")
        self.assertNotIn("source: 'CG (Build Pipeline)'", viewer)
        self.assertIn('id="cg-section"', viewer)


if __name__ == "__main__":
    unittest.main()
