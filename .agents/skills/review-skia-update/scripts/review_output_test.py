"""Validation and persistence of complete advisory review reports."""

import contextlib
import importlib.util
import io
import json
from pathlib import Path
import subprocess
import sys
import tempfile
import unittest
from unittest import mock


def module(name, filename):
    spec = importlib.util.spec_from_file_location(name, Path(__file__).with_name(filename))
    loaded = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(loaded)
    return loaded


VALIDATE = module("review_validate", "validate-skia-review.py")
PERSIST = module("review_persist", "persist-skia-review.py")


def report():
    section = {"status": "PASS", "summary": "No changes", "recommendations": ["Inspect"],
               "unchanged": 0, "added": [], "changed": []}
    return {
        "meta": {"schemaVersion": "1.0", "skiaPrNumber": 402, "skiasharpPrNumber": 5131,
                 "repo": "mono/skia", "upstreamBranch": "chrome/m151",
                 "oldUpstreamBranch": "chrome/m150", "analyzedAt": "2026-10-04T00:00:00Z",
                 "shas": {"prHead": "a" * 40, "base": "b" * 40, "upstream": "c" * 40}},
        "summary": "This is a complete review summary with enough context to validate the update.",
        "recommendations": ["Inspect results"], "riskAssessment": "LOW",
        "generatedFiles": {"status": "PASS", "checked": ["binding/SkiaSharp/Generated"],
                           "summary": "Matches", "recommendations": ["Inspect"]},
        "upstreamIntegrity": dict(section), "interopIntegrity": dict(section),
        "depsAudit": dict(section),
        "companionPr": dict(section, prNumber=5131, headSha="d" * 40, baseSha="e" * 40),
    }


class ReviewOutputTests(unittest.TestCase):
    def validate(self, path):
        with mock.patch.object(sys, "argv", ["validate", str(path)]), contextlib.redirect_stdout(io.StringIO()) as output:
            with self.assertRaises(SystemExit) as exit_status:
                VALIDATE.main()
        return exit_status.exception.code, output.getvalue()

    def test_real_schema_rejects_malformed_nested_values(self):
        with tempfile.TemporaryDirectory() as directory:
            path = Path(directory) / "402.json"
            path.write_text(json.dumps(report()))
            self.assertEqual(0, self.validate(path)[0])
            invalid = report()
            invalid["upstreamIntegrity"]["added"] = [{"path": "x", "summary": "y", "relatedFiles": [17]}]
            path.write_text(json.dumps(invalid))
            code, output = self.validate(path)
            self.assertEqual(1, code)
            self.assertIn("Schema: upstreamIntegrity.added.0.relatedFiles.0", output)

    def test_companion_only_changes_require_medium_risk(self):
        with tempfile.TemporaryDirectory() as directory:
            path = Path(directory) / "402.json"
            value = report()
            value["companionPr"]["status"] = "REVIEW_REQUIRED"
            value["companionPr"]["added"] = [{"path": "binding/SkiaSharp/New.cs", "summary": "New wrapper"}]
            path.write_text(json.dumps(value))
            self.assertEqual(1, self.validate(path)[0])
            value["riskAssessment"] = "MEDIUM"
            path.write_text(json.dumps(value))
            self.assertEqual(0, self.validate(path)[0])

    def test_upstream_main_ref_still_uses_cgmanifest_milestone(self):
        with tempfile.TemporaryDirectory() as directory:
            path = Path(directory) / "402.json"
            value = report()
            value["meta"]["upstreamBranch"] = "main"
            path.write_text(json.dumps(value))
            self.assertEqual(0, self.validate(path)[0])

    def test_persist_uses_custom_directory_and_propagates_renderer_failure(self):
        with tempfile.TemporaryDirectory() as directory:
            source = Path(directory) / "402.json"
            source.write_text(json.dumps(report()))
            destination = Path(directory) / "artifact"
            with mock.patch.object(sys, "argv", ["persist", str(source), "--output-dir", str(destination)]), \
                 mock.patch.object(PERSIST.subprocess, "run") as run:
                run.side_effect = [subprocess.CompletedProcess([], 0),
                                   subprocess.CalledProcessError(1, ["render"])]
                with self.assertRaises(subprocess.CalledProcessError):
                    PERSIST.main()
                self.assertTrue((destination / "402.json").exists())
                self.assertEqual(True, run.call_args.kwargs["check"])

    def test_persist_produces_valid_json_and_html(self):
        with tempfile.TemporaryDirectory() as directory:
            source = Path(directory) / "402.json"
            source.write_text(json.dumps(report()))
            destination = Path(directory) / "artifact"
            with mock.patch.object(sys, "argv", ["persist", str(source), "--output-dir", str(destination)]):
                PERSIST.main()
            self.assertEqual(report(), json.loads((destination / "402.json").read_text()))
            self.assertIn("Skia Review", (destination / "402.html").read_text())


if __name__ == "__main__":
    unittest.main()
