#!/usr/bin/env python3
"""Network-free contracts for Skia sync resolution and merge message parsing."""

from __future__ import annotations

import importlib.util
import json
from pathlib import Path
import subprocess
import unittest


ROOT = Path(__file__).resolve().parents[3]


def load(name: str, filename: str):
    spec = importlib.util.spec_from_file_location(name, ROOT / ".github/scripts" / filename)
    assert spec and spec.loader
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    return module


PAIR = load("resolve_skia_sync_pair", "resolve-skia-sync-pair.py")
STATE = load("skia_sync_review_state", "skia-sync-review-state.py")


def pr(repository, number, *, head="skia-sync/m147", base="main", body="", state="open", merged=False):
    return {
        "number": number,
        "title": "[skia-sync] Update Skia",
        "body": body,
        "html_url": f"https://github.com/{repository}/pull/{number}",
        "state": state,
        "draft": False,
        "user": {"login": "octocat"},
        "merged_at": "2026-09-18T00:00:00Z" if merged else None,
        "merge_commit_sha": "a" * 40 if merged else None,
        "head": {"repo": {"full_name": repository}, "ref": head, "sha": "b" * 40},
        "base": {"repo": {"full_name": repository}, "ref": base, "sha": "c" * 40},
    }


def pair(*, parent_base="main", native_state="open", native_merged=False, parent_state="open", parent_merged=False):
    native_base = "skiasharp" if parent_base == "main" else parent_base
    parent = pr("mono/SkiaSharp", 5114, base=parent_base, body="https://github.com/mono/skia/pull/367", state=parent_state, merged=parent_merged)
    native = pr("mono/skia", 367, base=native_base, body="https://github.com/mono/SkiaSharp/pull/5114", state=native_state, merged=native_merged)
    return parent, native


class PairResolverTests(unittest.TestCase):
    def test_open_milestone_pair_is_reviewable(self):
        contract = PAIR.resolve_pair(*pair(), "review")
        self.assertEqual(147, contract["milestone"])
        self.assertEqual("skiasharp", contract["native"]["base_branch"])

    def test_release_base_maps_to_same_native_base(self):
        contract = PAIR.resolve_pair(*pair(parent_base="release/3.119.x"), "merge")
        self.assertEqual("release/3.119.x", contract["native"]["base_branch"])

    def test_release_sync_extracts_milestone_from_parent_text(self):
        parent, native = pair(parent_base="release/3.119.x")
        parent["head"]["ref"] = native["head"]["ref"] = "skia-sync/release-3.119.x"
        parent["title"] = "[skia-sync] Merge upstream chrome/m147 bug fixes"
        self.assertEqual(147, PAIR.resolve_pair(parent, native, "merge")["milestone"])

    def test_merged_native_and_parent_resumptions_are_valid(self):
        self.assertTrue(PAIR.resolve_pair(*pair(native_state="closed", native_merged=True), "merge")["native"]["merged"])
        self.assertTrue(PAIR.resolve_pair(*pair(native_state="closed", native_merged=True, parent_state="closed", parent_merged=True), "merge")["parent"]["merged"])

    def test_review_rejects_closed_native(self):
        with self.assertRaises(PAIR.PairValidationError):
            PAIR.resolve_pair(*pair(native_state="closed", native_merged=True), "review")

    def test_rejects_ambiguous_or_nonreciprocal_links(self):
        parent, native = pair()
        parent["body"] += "\nhttps://github.com/mono/skia/pull/369"
        with self.assertRaises(PAIR.PairValidationError):
            PAIR.resolve_pair(parent, native, "merge")

    def test_rejects_malformed_path_suffix_links(self):
        parent, native = pair()
        parent["body"] = "https://github.com/mono/skia/pull/367/files"
        with self.assertRaises(PAIR.PairValidationError):
            PAIR.resolve_pair(parent, native, "merge")
        parent, native = pair()
        native["body"] = "https://github.com/mono/SkiaSharp/pull/5117"
        with self.assertRaises(PAIR.PairValidationError):
            PAIR.resolve_pair(parent, native, "merge")

    def test_rejects_unshared_main_tip_and_wrong_base(self):
        parent, native = pair()
        parent["head"]["ref"] = native["head"]["ref"] = "skia-sync/main"
        with self.assertRaises(PAIR.PairValidationError):
            PAIR.resolve_pair(parent, native, "merge")

    def test_rejects_noncanonical_shas_titles_and_milestone_disagreement(self):
        parent, native = pair()
        native["head"]["sha"] = "A" * 40
        with self.assertRaises(PAIR.PairValidationError):
            PAIR.resolve_pair(parent, native, "merge")
        parent, native = pair()
        native["title"] = "Update Skia"
        with self.assertRaises(PAIR.PairValidationError):
            PAIR.resolve_pair(parent, native, "merge")
        parent, native = pair()
        parent["title"] = "[skia-sync] chrome/m148"
        with self.assertRaises(PAIR.PairValidationError):
            PAIR.resolve_pair(parent, native, "merge")
        parent, native = pair()
        native["base"]["ref"] = "main"
        with self.assertRaises(PAIR.PairValidationError):
            PAIR.resolve_pair(parent, native, "merge")


class ReviewEvidenceStateTests(unittest.TestCase):
    def test_selects_only_expected_actor_and_frozen_pair(self):
        marker = (
            "<!-- skia-sync-review:v1 native-pr=367 native-head=" + "b" * 40 +
            " parent-pr=5114 parent-head=" + "c" * 40 + " run=9 artifact=101 -->"
        )
        latest = marker.replace("run=9 artifact=101", "run=10 artifact=102")
        result = STATE.find_review_marker(
            [{"user": {"login": "other"}, "body": marker},
             {"user": {"login": "github-actions[bot]"}, "body": marker},
             {"user": {"login": "github-actions[bot]"}, "body": latest}],
            native_pr=367, native_head="b" * 40, parent_pr=5114, parent_head="c" * 40,
        )
        self.assertEqual("10", result["run"])
        self.assertEqual("102", result["artifact"])

    def test_repin_requires_only_the_verified_two_file_child(self):
        kwargs = dict(
            parent_head="d" * 40, marker_parent_head="c" * 40, parents=["c" * 40],
            changed_paths=["externals/skia", "cgmanifest.json"], gitlink="a" * 40,
            component_repository_url="https://github.com/mono/skia.git",
            component_commit_hash="a" * 40, native_merge_sha="a" * 40,
        )
        self.assertTrue(STATE.repin_is_safe(**kwargs))
        kwargs["changed_paths"].append("README.md")
        self.assertFalse(STATE.repin_is_safe(**kwargs))

    def test_state_cli_rejects_untrusted_marker_and_invalid_repin(self):
        fixture = ROOT / ".github/scripts/tests/.review-state-fixture.json"
        try:
            fixture.write_text(json.dumps([{"user": {"login": "other"}, "body": "none"}]))
            result = subprocess.run(
                ["python3", str(ROOT / ".github/scripts/skia-sync-review-state.py"),
                 "--comments", str(fixture), "--native-pr", "367", "--native-head", "b" * 40,
                 "--parent-pr", "5114", "--parent-head", "c" * 40],
                capture_output=True, text=True,
            )
            self.assertNotEqual(0, result.returncode)
        finally:
            fixture.unlink(missing_ok=True)

    def test_requires_successful_main_review_run_and_evidence_artifact(self):
        run = {
            "name": "Review - Skia Sync", "path": ".github/workflows/skia-sync-review.yml",
            "status": "completed", "conclusion": "success", "head_branch": "main",
            "head_repository": {"full_name": "mono/SkiaSharp"},
        }
        artifacts = {
            "artifacts": [
                {"id": 101, "name": "skia-review-evidence", "expired": False}
            ]
        }
        self.assertTrue(STATE.provenance_is_trusted(run, artifacts, "101"))
        self.assertFalse(STATE.provenance_is_trusted(run, artifacts, "102"))
        artifacts["artifacts"][0]["expired"] = True
        self.assertFalse(STATE.provenance_is_trusted(run, artifacts, "101"))


if __name__ == "__main__":
    unittest.main()
