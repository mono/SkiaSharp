from contextlib import ExitStack
import json
from pathlib import Path
import subprocess
import tempfile
import unittest
from unittest.mock import call, patch

from run_review import (
    ensure_commit_available,
    extract_skia_milestone_from_cgmanifest,
    extract_skia_upstream_commit_from_cgmanifest,
    extract_skia_upstream_ref_from_cgmanifest,
    main,
    recorded_commit_belongs_to_upstream,
    resolve_skia_base,
)


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

    @patch("run_review.run_git")
    @patch("run_review.subprocess.run")
    def test_fetches_exact_commit_missing_from_shallow_branch_fetch(self, run, git) -> None:
        run.return_value.returncode = 1

        ensure_commit_available("/repo/skia", "origin", "base-sha")

        self.assertEqual(
            [
                call(["fetch", "--no-tags", "origin", "base-sha"], cwd="/repo/skia"),
                call(["cat-file", "-e", "base-sha^{commit}"], cwd="/repo/skia"),
            ],
            git.call_args_list,
        )

    @patch("run_review.run_git")
    @patch("run_review.subprocess.run")
    def test_does_not_fetch_available_commit(self, run, git) -> None:
        run.return_value.returncode = 0

        ensure_commit_available("/repo/skia", "origin", "base-sha")

        git.assert_not_called()

    @patch("run_review.run_git", side_effect=RuntimeError("fetch failed"))
    @patch("run_review.subprocess.run")
    def test_missing_commit_fetch_failure_is_not_ignored(self, run, git) -> None:
        run.return_value.returncode = 1

        with self.assertRaisesRegex(RuntimeError, "fetch failed"):
            ensure_commit_available("/repo/skia", "origin", "base-sha")

    @patch("run_review.run_git")
    def test_resolves_native_base_from_matching_companion_commit(self, git) -> None:
        git.return_value.stdout = "recorded-native-pin\n"

        self.assertEqual(
            "recorded-native-pin", resolve_skia_base("/repo", "companion-base")
        )
        git.assert_called_once_with(
            ["rev-parse", "--verify", "companion-base:externals/skia"], cwd="/repo"
        )

    def run_review_with_source_result(self, source_error=None):
        native_pin = "a" * 40
        native_head = "b" * 40
        native_pr = {
            "title": "Merge upstream chrome/m157",
            "headRefName": "skia-sync/m157",
            "headRefOid": native_head,
            "baseRefName": "skiasharp",
            "baseRefOid": "0" * 40,
            "state": "OPEN",
        }
        companion_pr = {
            **native_pr,
            "baseRefName": "main",
            "baseRefOid": "c" * 40,
            "headRefOid": "d" * 40,
        }

        def manifest(milestone):
            return {"registrations": [{
                "component": {"other": {"name": "skia", "version": f"chrome/m{milestone}"}},
                "upstream_merge_commit": str(milestone),
            }]}

        with tempfile.TemporaryDirectory() as directory, ExitStack() as stack:
            skia_root = Path(directory) / "externals/skia"
            (skia_root / ".git").mkdir(parents=True)
            (skia_root / "third_party/externals/harfbuzz").mkdir(parents=True)
            output_dir = Path(directory) / "review"

            def run(args, **kwargs):
                stdout = ""
                if args == ["git", "rev-parse", "--show-toplevel"]:
                    stdout = directory
                elif args == ["git", "rev-parse", "HEAD"]:
                    stdout = native_head
                elif args[:3] == ["gh", "pr", "view"]:
                    stdout = json.dumps(native_pr if "mono/skia" in args else companion_pr)
                return subprocess.CompletedProcess(args, 0, stdout, "")

            stack.enter_context(patch("run_review.subprocess.run", side_effect=run))
            stack.enter_context(patch("run_review.ensure_remote"))
            git = stack.enter_context(patch("run_review.run_git"))
            git.return_value.stdout = native_pin + "\n"
            stack.enter_context(patch("run_review.load_json_at_git_ref", side_effect=[
                manifest(156), manifest(157),
            ]))
            stack.enter_context(patch("run_review.recorded_commit_belongs_to_upstream", return_value=True))
            stack.enter_context(patch("run_review.check_generated_files.run_check", return_value={
                "status": "PASS", "checked": ["binding/SkiaSharp/Generated"], "mismatches": [],
            }))
            source = stack.enter_context(patch("run_review.check_source.run_check", return_value={
                "upstreamIntegrity": {"status": "PASS", "unchanged": 1},
                "interopIntegrity": {"status": "PASS", "unchanged": 1},
            }, side_effect=source_error))
            stack.enter_context(patch("run_review.check_deps.run_check", return_value={"status": "PASS"}))
            stack.enter_context(patch("run_review.check_companion.run_check", return_value={"status": "PASS"}))
            stack.enter_context(patch("sys.argv", [
                "run_review.py", "--skia-pr", "416", "--skiasharp-pr", "5542",
                "--milestone", "157", "--output-dir", str(output_dir),
            ]))

            exit_code = main()
            source.assert_called_once()
            self.assertEqual(native_pin, source.call_args.kwargs["base_sha"])
            raw_results = json.loads((output_dir / "raw-results.json").read_text())
            self.assertEqual(native_pin, raw_results["meta"]["shas"]["base"])
            return exit_code, raw_results

    def test_completed_review_uses_paired_native_base_and_exits_successfully(self) -> None:
        exit_code, results = self.run_review_with_source_result()

        self.assertEqual(0, exit_code)
        self.assertEqual("PASS", results["upstreamIntegrity"]["status"])

    def test_failed_check_saves_partial_results_but_exits_unsuccessfully(self) -> None:
        exit_code, results = self.run_review_with_source_result(
            RuntimeError("native base unavailable")
        )

        self.assertEqual(1, exit_code)
        self.assertEqual("ERROR", results["upstreamIntegrity"]["status"])
        self.assertEqual("PASS", results["generatedFiles"]["status"])
        self.assertEqual("PASS", results["depsAudit"]["status"])


if __name__ == "__main__":
    unittest.main()
