#!/usr/bin/env python3

import importlib.util
from pathlib import Path
import sys
import unittest
from unittest import mock


SCRIPT_PATH = (
    Path(__file__).resolve().parent.parent / "detect-release-publish.py"
)
sys.path.insert(0, str(SCRIPT_PATH.parent))
SPEC = importlib.util.spec_from_file_location(
    "detect_release_publish",
    SCRIPT_PATH,
)
detector = importlib.util.module_from_spec(SPEC)
sys.modules[SPEC.name] = detector
SPEC.loader.exec_module(detector)


class DetectReleasePublishTests(unittest.TestCase):
    def test_detector_emits_pinned_audit_command(self):
        status = {
            "branch": "release/4.152.0-preview.1",
            "commit": "a" * 40,
            "nextAction": "start-release-testing",
            "managedRun": {
                "runId": 10,
                "buildNumber": "4.152.0-preview.1.1+branch",
            },
            "testsRun": {"runId": 20},
            "packageVersions": {
                "test": {
                    "SkiaSharp": "4.152.0-preview.1.1",
                    "HarfBuzzSharp": "1.0.0-preview.1.1",
                },
                "public": {
                    "SkiaSharp": "4.152.0-preview.1.1",
                    "HarfBuzzSharp": "1.0.0-preview.1.1",
                },
            },
            "warnings": [],
        }
        with mock.patch.object(
            detector.publish,
            "status_report",
            return_value=status,
        ):
            result = detector.detect(
                Path.cwd(),
                "release/4.152.0-preview.1",
            )
        self.assertEqual(result["sourceSha"], "a" * 40)
        self.assertEqual(result["managedRunId"], 10)
        self.assertEqual(result["testsRunId"], 20)
        self.assertIn("--expect-managed-run 10", result["pushAuditCommand"])
        self.assertIn("--expect-tests-run 20", result["pushAuditCommand"])
        self.assertTrue(result["pushAuditCommand"].endswith("--dry-run"))
        self.assertIn(
            "finalize-release.py",
            result["finalizeAuditCommand"],
        )

    def test_detector_requires_ready_status(self):
        status = {
            "branch": "release/4.152.0",
            "nextAction": "wait-for-tests",
        }
        with (
            mock.patch.object(
                detector.publish,
                "status_report",
                return_value=status,
            ),
            self.assertRaisesRegex(
                detector.DetectionError,
                "not ready",
            ),
        ):
            detector.detect(Path.cwd(), "release/4.152.0")

    def test_detector_accepts_tested_source_sha(self):
        source_sha = "a" * 40
        status = {
            "branch": "release/4.152.0",
            "commit": source_sha,
            "nextAction": "start-release-testing",
            "managedRun": {
                "runId": 10,
                "buildNumber": "4.152.0-stable.1+4.152.0",
            },
            "testsRun": {"runId": 20},
            "packageVersions": {
                "test": {
                    "SkiaSharp": "4.152.0-stable.1",
                    "HarfBuzzSharp": "1.0.0-stable.1",
                },
                "public": {
                    "SkiaSharp": "4.152.0",
                    "HarfBuzzSharp": "1.0.0",
                },
            },
        }
        with mock.patch.object(
            detector.publish,
            "status_report",
            return_value=status,
        ):
            result = detector.detect(Path.cwd(), source_sha)
        self.assertEqual(result["input"], source_sha)
        self.assertEqual(result["releaseBranch"], "release/4.152.0")

    def test_scripts_are_ascii_only(self):
        SCRIPT_PATH.read_text(encoding="ascii")
        Path(__file__).read_text(encoding="ascii")


if __name__ == "__main__":
    unittest.main()
