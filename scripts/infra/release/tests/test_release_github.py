from __future__ import annotations

import sys
import unittest
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent.parent))

import release_github as gh
from release_common import ReleaseToolError


class TagVersionOrderingTests(unittest.TestCase):
    def test_parses_stable_preview_and_rc(self):
        stable = gh.TagVersion.parse("v3.119.0")
        preview = gh.TagVersion.parse("v3.119.0-preview.2")
        rc = gh.TagVersion.parse("v3.119.0-rc.1")
        self.assertIsNotNone(stable)
        self.assertIsNotNone(preview)
        self.assertIsNotNone(rc)
        self.assertIsNone(gh.TagVersion.parse("not-a-tag"))

    def test_channel_orders_before_stable(self):
        preview = gh.TagVersion.parse("v3.119.0-preview.1")
        rc = gh.TagVersion.parse("v3.119.0-rc.1")
        stable = gh.TagVersion.parse("v3.119.0")
        self.assertLess(preview.sort_key, rc.sort_key)
        self.assertLess(rc.sort_key, stable.sort_key)

    def test_previous_release_tag_across_channels(self):
        tags = ["v3.119.0-preview.1", "v3.119.0-preview.2", "v3.119.0-rc.1", "v3.118.0"]
        self.assertEqual(gh.previous_release_tag("v3.119.0-rc.1", tags), "v3.119.0-preview.2")
        self.assertEqual(gh.previous_release_tag("v3.119.0-preview.1", tags), "v3.118.0")

    def test_previous_release_tag_stable_looks_across_all_channels(self):
        tags = ["v3.119.0-preview.1", "v3.119.0-rc.1", "v3.119.0"]
        self.assertEqual(gh.previous_release_tag("v3.119.0", tags), "v3.119.0-rc.1")

    def test_previous_release_tag_returns_none_when_first_ever(self):
        self.assertIsNone(gh.previous_release_tag("v1.0.0-preview.1", ["v1.0.0-preview.1"]))

    def test_previous_release_tag_rejects_invalid_current_tag(self):
        with self.assertRaises(gh.GitHubError):
            gh.previous_release_tag("not-a-tag", ["v1.0.0"])


class ManagedMarkerTests(unittest.TestCase):
    def test_build_initial_body_contains_all_markers(self):
        body = gh.build_initial_body("## What's Changed\n* did a thing")
        self.assertIn(gh.SUMMARY_START_MARKER, body)
        self.assertIn(gh.SUMMARY_END_MARKER, body)
        self.assertIn(gh.GENERATED_START_MARKER, body)
        self.assertIn(gh.GENERATED_END_MARKER, body)
        self.assertIn("did a thing", body)
        self.assertTrue(gh.has_managed_markers(body))

    def test_replace_managed_summary_only_touches_summary_region(self):
        body = gh.build_initial_body("generated notes")
        updated = gh.replace_managed_summary(body, "Reviewed highlight prose.")
        self.assertIn("Reviewed highlight prose.", updated)
        self.assertIn("generated notes", updated)
        self.assertIn(gh.GENERATED_START_MARKER, updated)

    def test_replace_managed_summary_returns_none_for_legacy_body(self):
        self.assertIsNone(gh.replace_managed_summary("no markers here", "prose"))

    def test_rejects_duplicate_markers(self):
        body = gh.build_initial_body("notes") + gh.SUMMARY_START_MARKER
        with self.assertRaises(gh.GitHubError):
            gh.replace_managed_summary(body, "prose")

    def test_rejects_out_of_order_markers(self):
        body = (
            f"{gh.GENERATED_START_MARKER}\nnotes\n{gh.GENERATED_END_MARKER}\n"
            f"{gh.SUMMARY_START_MARKER}\n\n{gh.SUMMARY_END_MARKER}\n"
        )
        with self.assertRaises(gh.GitHubError):
            gh.replace_managed_summary(body, "prose")


class TagConflictTests(unittest.TestCase):
    def test_no_conflict_when_tag_absent(self):
        gh.check_tag_conflict(None, "a" * 40)  # must not raise

    def test_no_conflict_when_tag_matches(self):
        gh.check_tag_conflict("a" * 40, "a" * 40)  # must not raise

    def test_conflict_when_tag_points_elsewhere(self):
        with self.assertRaisesRegex(gh.GitHubError, "never moved"):
            gh.check_tag_conflict("a" * 40, "b" * 40)


class ReleaseConflictTests(unittest.TestCase):
    def test_no_conflict_when_release_absent(self):
        gh.check_release_conflict(
            None, expected_title="Version 3.119.0", expected_target="a" * 40, expected_prerelease=False
        )

    def test_no_conflict_when_matching(self):
        release = gh.ReleaseInfo(
            tag_name="v3.119.0", name="Version 3.119.0", is_draft=True, is_prerelease=False,
            target_commitish="a" * 40, body="", url="https://example.invalid",
        )
        gh.check_release_conflict(
            release, expected_title="Version 3.119.0", expected_target="a" * 40, expected_prerelease=False
        )

    def test_conflict_on_target_mismatch(self):
        release = gh.ReleaseInfo(
            tag_name="v3.119.0", name="Version 3.119.0", is_draft=True, is_prerelease=False,
            target_commitish="a" * 40, body="", url="https://example.invalid",
        )
        with self.assertRaises(gh.GitHubError):
            gh.check_release_conflict(
                release, expected_title="Version 3.119.0", expected_target="b" * 40, expected_prerelease=False
            )

    def test_conflict_on_prerelease_mismatch(self):
        release = gh.ReleaseInfo(
            tag_name="v3.119.0-preview.1", name="Version 3.119.0 (Preview 1)", is_draft=True,
            is_prerelease=True, target_commitish="a" * 40, body="", url="https://example.invalid",
        )
        with self.assertRaises(gh.GitHubError):
            gh.check_release_conflict(
                release, expected_title="Version 3.119.0 (Preview 1)", expected_target="a" * 40,
                expected_prerelease=False,
            )

    def test_conflict_on_title_mismatch(self):
        release = gh.ReleaseInfo(
            tag_name="v3.119.0", name="Wrong Title", is_draft=True, is_prerelease=False,
            target_commitish="a" * 40, body="", url="https://example.invalid",
        )
        with self.assertRaises(gh.GitHubError):
            gh.check_release_conflict(
                release, expected_title="Version 3.119.0", expected_target="a" * 40, expected_prerelease=False
            )

    def test_published_release_with_legacy_branch_name_target_is_tolerated(self):
        # Live regression: the real, already-published mono/SkiaSharp
        # release v4.151.1 has targetCommitish == "main" (confirmed via
        # `gh release view v4.151.1`), not the exact package source commit
        # -- GitHub's target_commitish is only a fallback used if the named
        # tag doesn't exist yet; once the tag exists (verified separately
        # via check_tag_conflict), it is authoritative and target_commitish
        # is no longer a trustworthy second opinion. This must reconcile,
        # not block.
        release = gh.ReleaseInfo(
            tag_name="v4.151.1", name="Version 4.151.1", is_draft=False, is_prerelease=False,
            target_commitish="main", body="notes", url="https://example.invalid",
        )
        gh.check_release_conflict(
            release, expected_title="Version 4.151.1",
            expected_target="279f93f4ffa7f9fe4e9c0bc298bedc3c9e439764", expected_prerelease=False,
        )

    def test_published_release_with_a_different_exact_sha_target_still_conflicts(self):
        # A real (40-hex) target_commitish that simply disagrees is a
        # genuine conflict, published or not.
        release = gh.ReleaseInfo(
            tag_name="v3.119.0", name="Version 3.119.0", is_draft=False, is_prerelease=False,
            target_commitish="b" * 40, body="", url="https://example.invalid",
        )
        with self.assertRaises(gh.GitHubError):
            gh.check_release_conflict(
                release, expected_title="Version 3.119.0", expected_target="a" * 40, expected_prerelease=False
            )

    def test_draft_with_branch_name_target_still_conflicts(self):
        # The branch-name leniency applies only to an already-published
        # release; this tool always creates a draft with an exact SHA, so
        # any disagreement on an unpublished draft remains a hard conflict.
        release = gh.ReleaseInfo(
            tag_name="v3.119.0", name="Version 3.119.0", is_draft=True, is_prerelease=False,
            target_commitish="main", body="", url="https://example.invalid",
        )
        with self.assertRaises(gh.GitHubError):
            gh.check_release_conflict(
                release, expected_title="Version 3.119.0", expected_target="a" * 40, expected_prerelease=False
            )


if __name__ == "__main__":
    unittest.main()
