from __future__ import annotations

import sys
import unittest
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent.parent))

import release_github as gh
from release_common import CommandResult, ReleaseToolError


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


class _FakeRunner:
    """Returns one canned :class:`CommandResult` and records the argv used."""

    def __init__(self, *, returncode: int = 0, stdout: str = "", stderr: str = ""):
        self.returncode = returncode
        self.stdout = stdout
        self.stderr = stderr
        self.calls: list[list[str]] = []

    def run(self, args, *, cwd, check=True, timeout=120, input=None):
        args_list = list(args)
        self.calls.append(args_list)
        return CommandResult(
            args=tuple(args_list), returncode=self.returncode, stdout=self.stdout, stderr=self.stderr
        )


class RefShaTests(unittest.TestCase):
    """Item 1 regression: ``ref_sha`` must use GitHub's singular "get a
    reference" endpoint, not the plural "list matching references"
    endpoint. The plural endpoint does a *string* prefix match on ref
    names -- unrelated to the git tree hierarchy -- so once a release's
    stable/RC tag or branch is created alongside its still-existing
    preview sibling (e.g. ``release/6.0.x`` next to
    ``release/6.0.x-preview``), a query for the shorter name can return a
    JSON array instead of a single object and crash a caller that treats
    the payload as a dict.
    """

    def test_exact_match_uses_singular_endpoint_and_returns_sha(self):
        sha_value = "a" * 40
        runner = _FakeRunner(stdout=f'{{"ref": "refs/heads/release/6.0.x", "object": {{"sha": "{sha_value}"}}}}')
        client = gh.GhCliGitHubClient(repository="mono/skia", runner=runner)
        sha = client.ref_sha(repository="mono/skia", ref="refs/heads/release/6.0.x")
        self.assertEqual(sha, sha_value)
        self.assertEqual(len(runner.calls), 1)
        argv = runner.calls[0]
        self.assertIn("repos/mono/skia/git/ref/heads/release/6.0.x", argv)
        # Never the plural, prefix-matching "list" endpoint.
        self.assertNotIn("repos/mono/skia/git/refs/heads/release/6.0.x", argv)

    def test_missing_ref_returns_none_on_404(self):
        runner = _FakeRunner(returncode=1, stdout="", stderr="HTTP 404: Not Found")
        client = gh.GhCliGitHubClient(repository="mono/skia", runner=runner)
        sha = client.ref_sha(repository="mono/skia", ref="refs/heads/release/6.0.x")
        self.assertIsNone(sha)

    def test_prefix_list_payload_is_exact_matched_defensively(self):
        # Reproduces the real failure mode: querying for the shorter
        # "release/6.0.x" ref while a longer sibling branch
        # "release/6.0.x-preview" also exists. Even if a response ever
        # comes back as an array (the plural endpoint's behavior), the
        # client must not crash and must not return the wrong sha --
        # it exact-matches by full ref name.
        payload = (
            '[{"ref": "refs/heads/release/6.0.x", "object": {"sha": "%s"}},'
            ' {"ref": "refs/heads/release/6.0.x-preview", "object": {"sha": "%s"}}]'
        ) % ("a" * 40, "b" * 40)
        runner = _FakeRunner(stdout=payload)
        client = gh.GhCliGitHubClient(repository="mono/skia", runner=runner)
        sha = client.ref_sha(repository="mono/skia", ref="refs/heads/release/6.0.x")
        self.assertEqual(sha, "a" * 40)

    def test_prefix_list_payload_without_an_exact_match_returns_none(self):
        payload = '[{"ref": "refs/heads/release/6.0.x-preview", "object": {"sha": "%s"}}]' % ("b" * 40)
        runner = _FakeRunner(stdout=payload)
        client = gh.GhCliGitHubClient(repository="mono/skia", runner=runner)
        sha = client.ref_sha(repository="mono/skia", ref="refs/heads/release/6.0.x")
        self.assertIsNone(sha)


class TargetCommitishConflictTests(unittest.TestCase):
    """Item 3: ``target_commitish_conflicts`` is the single rule shared by
    :func:`check_release_conflict` and the ``finish plan-publication`` /
    ``finish publish`` bindings -- drafts stay strict, published releases
    are authoritative through the separately verified tag."""

    def test_matching_target_never_conflicts(self):
        release = gh.ReleaseInfo(
            tag_name="v3.119.0", name="Version 3.119.0", is_draft=True, is_prerelease=False,
            target_commitish="a" * 40, body="", url="https://example.invalid",
        )
        self.assertFalse(gh.target_commitish_conflicts(release, "a" * 40))

    def test_draft_with_branch_name_target_conflicts(self):
        release = gh.ReleaseInfo(
            tag_name="v3.119.0", name="Version 3.119.0", is_draft=True, is_prerelease=False,
            target_commitish="main", body="", url="https://example.invalid",
        )
        self.assertTrue(gh.target_commitish_conflicts(release, "a" * 40))

    def test_draft_with_a_different_exact_sha_conflicts(self):
        release = gh.ReleaseInfo(
            tag_name="v3.119.0", name="Version 3.119.0", is_draft=True, is_prerelease=False,
            target_commitish="b" * 40, body="", url="https://example.invalid",
        )
        self.assertTrue(gh.target_commitish_conflicts(release, "a" * 40))

    def test_published_release_with_legacy_branch_name_target_is_tolerated(self):
        release = gh.ReleaseInfo(
            tag_name="v4.151.1", name="Version 4.151.1", is_draft=False, is_prerelease=False,
            target_commitish="main", body="notes", url="https://example.invalid",
        )
        self.assertFalse(
            gh.target_commitish_conflicts(release, "279f93f4ffa7f9fe4e9c0bc298bedc3c9e439764")
        )

    def test_published_release_with_a_different_exact_sha_still_conflicts(self):
        release = gh.ReleaseInfo(
            tag_name="v3.119.0", name="Version 3.119.0", is_draft=False, is_prerelease=False,
            target_commitish="b" * 40, body="", url="https://example.invalid",
        )
        self.assertTrue(gh.target_commitish_conflicts(release, "a" * 40))


if __name__ == "__main__":
    unittest.main()
