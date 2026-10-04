import unittest
from unittest.mock import patch
import tempfile
from pathlib import Path

from run_review import (
    extract_skia_milestone_from_cgmanifest,
    extract_skia_upstream_commit_from_cgmanifest,
    extract_skia_upstream_ref_from_cgmanifest,
    recorded_commit_belongs_to_upstream,
    required_skia_pr,
    resolve_prs,
    run_isolated,
    validate_metadata,
    incomplete_results,
)


class RunReviewTests(unittest.TestCase):
    def test_parent_link_requires_one_canonical_url(self) -> None:
        body = "**Required skia PR**\n\n[Native](https://github.com/mono/skia/pull/402)\n\n**Areas Affected**"
        self.assertEqual(402, required_skia_pr(body))
        for bad in ("No link", "**Required skia PR**\nNone.",
                    body.replace("/402)", "/402/files)"),
                    body.replace("/402)", "/402) and https://github.com/mono/skia/pull/403")):
            with self.subTest(bad=bad), self.assertRaises(ValueError):
                required_skia_pr(bad)

    @patch("run_review.fetch_pr")
    def test_parent_only_fetches_companion_before_linked_native(self, fetch) -> None:
        parent = {"body": "**Required skia PR**\nhttps://github.com/mono/skia/pull/402",
                  "headRefOid": "a" * 40, "baseRefOid": "b" * 40, "baseRefName": "main"}
        native = {"headRefOid": "c" * 40, "baseRefOid": "d" * 40, "baseRefName": "skiasharp"}
        fetch.side_effect = [parent, native]
        pair = resolve_prs(5131)
        self.assertEqual(402, pair["nativeNumber"])
        self.assertEqual([("mono/SkiaSharp", 5131), ("mono/skia", 402)],
                         [call.args for call in fetch.call_args_list])
        fetch.reset_mock(side_effect=True)
        fetch.side_effect = [dict(parent, body=""), native]
        self.assertEqual(402, resolve_prs(5131, 402)["nativeNumber"])

    def test_requires_frozen_exact_heads_and_bases(self) -> None:
        with self.assertRaisesRegex(ValueError, "headRefOid"):
            validate_metadata({"parentNumber": 1, "nativeNumber": 2,
                               "parent": {"headRefOid": "bad"},
                               "native": {}})

    def test_partial_mechanics_fail_but_a_binding_mismatch_is_reviewable(self) -> None:
        source = {"upstreamIntegrity": {"status": "PASS"},
                  "interopIntegrity": {"status": "REVIEW_REQUIRED"}}
        self.assertEqual([], incomplete_results({"status": "FAIL"}, source,
                                                {"status": "PASS"}, {"status": "PASS"}))
        errors = incomplete_results({"status": "ERROR"}, source,
                                    {"status": "PASS"}, None)
        self.assertEqual(2, len(errors))
        self.assertIn("Generated Files", errors[0])
        self.assertIn("Companion PR", errors[1])

    @patch("run_review.subprocess.run")
    def test_isolated_worker_receives_only_ro_skill_metadata_and_rw_results(self, run) -> None:
        with tempfile.TemporaryDirectory() as directory:
            metadata = {"parentNumber": 5131, "nativeNumber": 402}
            run_isolated(metadata, str(Path(directory) / "results"), None)
            command = run.call_args.args[0]
            self.assertEqual("docker", command[0])
            mounts = [command[i + 1] for i, token in enumerate(command[:-1]) if token == "--mount"]
            self.assertEqual(3, len(mounts))
            self.assertIn("target=/skill,readonly", mounts[0])
            self.assertIn("target=/input/pr.json,readonly", mounts[1])
            self.assertIn("target=/export", mounts[2])
            self.assertNotIn("--env", command)
            self.assertNotIn("-e", command)
            self.assertNotIn("--cap-drop=ALL", command)
            self.assertIn("--security-opt=no-new-privileges", command)
            self.assertNotIn("GH_TOKEN", " ".join(command))
            self.assertFalse(list(Path(directory).glob(".skia-review-metadata-*")))
            self.assertTrue(run.call_args.kwargs["check"])

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


if __name__ == "__main__":
    unittest.main()
