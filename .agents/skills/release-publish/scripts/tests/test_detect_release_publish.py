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


def bar_build(
    *,
    bar_id=30,
    commit="a" * 40,
    build_run_id=10,
    skia_version="4.152.0-preview.1.1",
    harfbuzz_version="1.0.0-preview.1.1",
    branch="release/4.152.0-preview.1",
    build_number="4.152.0-preview.1.1+branch",
):
    return {
        "id": bar_id,
        "state": "ready",
        "commit": commit,
        "buildRunId": build_run_id,
        "buildDefinitionId": detector.publish.BUILD_DEFINITION_ID,
        "branch": f"refs/heads/{branch}",
        "buildNumber": build_number,
        "defaultChannelIds": [529],
        "channels": ["General Testing"],
        "assets": {
            "SkiaSharp": {
                "version": skia_version,
                "locations": [
                    "https://pkgs.dev.azure.com/dnceng/public/"
                    "_packaging/skiasharp/nuget/v3/index.json"
                ],
            },
            "HarfBuzzSharp": {
                "version": harfbuzz_version,
                "locations": [
                    "https://pkgs.dev.azure.com/dnceng/public/"
                    "_packaging/skiasharp/nuget/v3/index.json"
                ],
            },
        },
    }


class DetectReleasePublishTests(unittest.TestCase):
    def test_detector_emits_pinned_audit_command(self):
        status = {
            "branch": "release/4.152.0-preview.1",
            "commit": "a" * 40,
            "nextAction": "start-release-testing",
            "migration": {"state": "ready", "missing": []},
            "buildRun": {
                "runId": 10,
                "pipelineId": detector.publish.BUILD_DEFINITION_ID,
                "buildNumber": "4.152.0-preview.1.1+branch",
                "sourceBranch": "refs/heads/release/4.152.0-preview.1",
                "sourceVersion": "a" * 40,
            },
            "testsRun": {
                "runId": 20,
                "pipelineId": detector.publish.TESTS_DEFINITION_ID,
                "sourceVersion": "a" * 40,
            },
            "barBuild": bar_build(),
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
        self.assertEqual(result["buildRunId"], 10)
        self.assertEqual(result["testsRunId"], 20)
        self.assertEqual(result["barBuildId"], 30)
        self.assertEqual(result["defaultChannelIds"], [529])
        self.assertEqual(result["migration"]["state"], "ready")
        self.assertEqual(
            result["barAssets"]["SkiaSharp"]["locations"],
            [
                "https://pkgs.dev.azure.com/dnceng/public/"
                "_packaging/skiasharp/nuget/v3/index.json"
            ],
        )
        self.assertIn("--expect-build-run 10", result["pushAuditCommand"])
        self.assertIn("--expect-tests-run 20", result["pushAuditCommand"])
        self.assertIn("--expect-bar-build 30", result["pushAuditCommand"])
        self.assertTrue(result["pushAuditCommand"].endswith("--dry-run"))
        self.assertIn(
            "create-release-draft.py",
            result["draftAuditCommand"],
        )
        self.assertIn("--expect-bar-build 30", result["draftAuditCommand"])

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
            "migration": {"state": "ready", "missing": []},
            "buildRun": {
                "runId": 10,
                "pipelineId": detector.publish.BUILD_DEFINITION_ID,
                "buildNumber": "4.152.0+4.152.0",
                "sourceBranch": "refs/heads/release/4.152.0",
                "sourceVersion": source_sha,
            },
            "testsRun": {
                "runId": 20,
                "pipelineId": detector.publish.TESTS_DEFINITION_ID,
                "sourceVersion": source_sha,
            },
            "barBuild": bar_build(
                commit=source_sha,
                skia_version="4.152.0",
                harfbuzz_version="1.0.0",
                branch="release/4.152.0",
                build_number="4.152.0+4.152.0",
            ),
            "packageVersions": {
                "test": {
                    "SkiaSharp": "4.152.0",
                    "HarfBuzzSharp": "1.0.0",
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

    def test_detector_does_not_select_release_by_channel_name(self):
        status = {
            "branch": "release/4.152.0-preview.1",
            "commit": "a" * 40,
            "nextAction": "start-release-testing",
            "migration": {"state": "ready", "missing": []},
            "buildRun": {
                "runId": 10,
                "pipelineId": detector.publish.BUILD_DEFINITION_ID,
                "buildNumber": "4.152.0-preview.1.1+branch",
                "sourceBranch": "refs/heads/release/4.152.0-preview.1",
                "sourceVersion": "a" * 40,
            },
            "testsRun": {
                "runId": 20,
                "pipelineId": detector.publish.TESTS_DEFINITION_ID,
                "sourceVersion": "a" * 40,
            },
            "barBuild": bar_build(),
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
        }
        status["barBuild"]["channels"] = []
        with mock.patch.object(
            detector.publish,
            "status_report",
            return_value=status,
        ):
            result = detector.detect(
                Path.cwd(),
                "release/4.152.0-preview.1",
            )
        self.assertEqual(result["barBuildId"], 30)

    def test_detector_rejects_missing_bar_asset_locations(self):
        status = {
            "branch": "release/4.152.0-preview.1",
            "commit": "a" * 40,
            "nextAction": "start-release-testing",
            "migration": {"state": "ready", "missing": []},
            "buildRun": {
                "runId": 10,
                "pipelineId": detector.publish.BUILD_DEFINITION_ID,
                "buildNumber": "4.152.0-preview.1.1+branch",
                "sourceBranch": "refs/heads/release/4.152.0-preview.1",
                "sourceVersion": "a" * 40,
            },
            "testsRun": {
                "runId": 20,
                "pipelineId": detector.publish.TESTS_DEFINITION_ID,
                "sourceVersion": "a" * 40,
            },
            "barBuild": bar_build(),
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
        }
        status["barBuild"]["assets"]["SkiaSharp"]["locations"] = []
        with (
            mock.patch.object(
                detector.publish,
                "status_report",
                return_value=status,
            ),
            self.assertRaisesRegex(
                detector.DetectionError,
                "no recorded package locations",
            ),
        ):
            detector.detect(Path.cwd(), "release/4.152.0-preview.1")

    def test_scripts_are_ascii_only(self):
        SCRIPT_PATH.read_text(encoding="ascii")
        Path(__file__).read_text(encoding="ascii")


if __name__ == "__main__":
    unittest.main()
