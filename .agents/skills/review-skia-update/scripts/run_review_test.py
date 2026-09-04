import unittest
from unittest.mock import patch
import importlib.util
import json
from pathlib import Path
import tempfile
import jsonschema

from run_review import (
    extract_skia_milestone_from_cgmanifest,
    extract_skia_upstream_commit_from_cgmanifest,
    extract_skia_upstream_ref_from_cgmanifest,
    recorded_commit_belongs_to_upstream,
)

_PERSIST_PATH = Path(__file__).with_name("persist-skia-review.py")
_PERSIST_SPEC = importlib.util.spec_from_file_location(
    "persist_skia_review", _PERSIST_PATH
)
PERSIST = importlib.util.module_from_spec(_PERSIST_SPEC)
assert _PERSIST_SPEC.loader is not None
_PERSIST_SPEC.loader.exec_module(PERSIST)


class RunReviewTests(unittest.TestCase):
    def test_extracts_exact_skia_registration(self) -> None:
        manifest = {
            "registrations": [
                {"component": {"other": {"name": "other", "version": "1"}}},
                {
                    "component": {
                        "other": {
                            "name": "skia",
                            "version": "chrome/m152",
                        }
                    },
                    "chrome_milestone": 152,
                    "upstream_merge_commit": "abc123",
                },
            ]
        }

        self.assertEqual(
            "chrome/m152", extract_skia_milestone_from_cgmanifest(manifest)
        )
        self.assertEqual(
            "abc123", extract_skia_upstream_commit_from_cgmanifest(manifest)
        )
        self.assertEqual(
            "chrome/m152", extract_skia_upstream_ref_from_cgmanifest(manifest)
        )

    def test_returns_none_when_skia_registration_is_missing(self) -> None:
        manifest = {"registrations": []}

        self.assertIsNone(extract_skia_milestone_from_cgmanifest(manifest))
        self.assertIsNone(extract_skia_upstream_commit_from_cgmanifest(manifest))
        self.assertIsNone(extract_skia_upstream_ref_from_cgmanifest(manifest))

    def test_extracts_explicit_main_upstream_ref(self) -> None:
        manifest = {
            "registrations": [
                {
                    "component": {
                        "other": {
                            "name": "skia",
                            "version": "chrome/m152",
                        }
                    },
                    "chrome_milestone": 152,
                    "upstream_ref": "main",
                    "upstream_merge_commit": "abc123",
                },
            ]
        }

        self.assertEqual(
            "main", extract_skia_upstream_ref_from_cgmanifest(manifest)
        )

    @patch("run_review.subprocess.run")
    def test_accepts_commit_from_upstream_history(self, run) -> None:
        run.return_value.returncode = 0

        self.assertTrue(
            recorded_commit_belongs_to_upstream(
                "/repo",
                "target-sha",
                "upstream/chrome/m152",
            )
        )

    @patch("run_review.subprocess.run")
    def test_rejects_fork_head_as_upstream_commit(self, run) -> None:
        run.return_value.returncode = 1

        self.assertFalse(
            recorded_commit_belongs_to_upstream(
                "/repo",
                "fork-head",
                "upstream/chrome/m152",
            )
        )

    def test_review_output_uses_stable_skia_key(self) -> None:
        self.assertEqual(
            Path("output/ai/repos/github-52292286/ai-review"),
            PERSIST.output_directory(
                {
                    "skiaRepositoryKey": "github-52292286",
                    "legacySkiaRepositoryKeys": ["mono-skia"],
                }
            ),
        )

    def test_review_reader_falls_back_to_legacy_skia_key(self) -> None:
        identity = {
            "skiaRepositoryKey": "github-52292286",
            "legacySkiaRepositoryKeys": ["mono-skia"],
        }
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            legacy = root / "mono-skia" / "ai-review" / "354.json"
            legacy.parent.mkdir(parents=True)
            legacy.write_text("{}", encoding="utf-8")

            self.assertEqual(
                legacy,
                PERSIST.find_existing_review(identity, "354", root),
            )

    def test_schema_accepts_current_and_destination_skia_repositories(self) -> None:
        schema_path = (
            Path(__file__).resolve().parent.parent
            / "references"
            / "skia-review-schema.json"
        )
        schema = json.loads(schema_path.read_text(encoding="utf-8"))
        repo_schema = schema["properties"]["meta"]["properties"]["repo"]
        jsonschema.validate("mono/skia", repo_schema)
        jsonschema.validate("dotnet/skia", repo_schema)
        with self.assertRaises(jsonschema.ValidationError):
            jsonschema.validate("google/skia-extra", repo_schema)


if __name__ == "__main__":
    unittest.main()
