from __future__ import annotations

import json
import sys
import tempfile
import unittest
from pathlib import Path
from unittest import mock

_DOCS_DIR = Path(__file__).resolve().parents[2]
if str(_DOCS_DIR) not in sys.path:
    sys.path.insert(0, str(_DOCS_DIR))

from release_notes import common, github, update_github_summaries as updater

GH = github


def _release_info(tag, body, *, is_draft=False):
    return GH.ReleaseInfo(
        tag_name=tag,
        name="Version",
        is_draft=is_draft,
        is_prerelease=False,
        target_commitish="main",
        body=body,
        url="https://github.com/mono/SkiaSharp/releases/tag/{}".format(tag),
    )


class FakeGitHubClient:
    """An in-memory GitHubSummaryClient fake with no network access."""

    def __init__(
        self,
        bodies=None,
        *,
        race_tags=frozenset(),
        fail_write_tags=frozenset(),
        draft_tags=frozenset(),
    ):
        self.bodies = dict(bodies or {})
        self.race_tags = set(race_tags)
        self.fail_write_tags = set(fail_write_tags)
        self.draft_tags = set(draft_tags)
        self._calls: dict[str, int] = {}
        self.writes: list[tuple[str, str]] = []

    def get_release(self, tag):
        self._calls[tag] = self._calls.get(tag, 0) + 1
        if tag not in self.bodies:
            return None
        body = self.bodies[tag]
        if tag in self.race_tags and self._calls[tag] >= 2:
            body = body + "\n<!-- concurrently edited by someone else -->"
        return _release_info(tag, body, is_draft=tag in self.draft_tags)

    def publish(self, tag):
        """Simulate publication of the draft: the SAME body the draft
        held becomes the published release's body -- exactly what happens
        when the release-published event later fires."""
        self.draft_tags.discard(tag)

    def update_release_body(self, *, tag, body):
        self.writes.append((tag, body))
        if tag not in self.fail_write_tags:
            self.bodies[tag] = body


def _shipment(**overrides):
    shipment = {
        "tag": "v4.151.0-preview.1",
        "core_version": "4.151.0",
        "public_version": "4.151.0-preview.1",
        "channel": "preview",
        "label": "Preview 1",
        "previous_tag": "v4.150.2",
        "target_sha": "a" * 40,
        "date": "2026-01-01",
        "changelog_url": "https://github.com/mono/SkiaSharp/compare/v4.150.2...v4.151.0-preview.1",
        "prs": [4294],
    }
    shipment.update(overrides)
    return shipment


def _data(*, shipments=None, format_version=4, **overrides):
    data = {
        "format": format_version,
        "version": "4.151.0",
        "shipments": shipments if shipments is not None else [_shipment()],
        "contributors": [],
    }
    data.update(overrides)
    return data


def _prose(*, summaries=None):
    return {"release_summaries": summaries if summaries is not None else {
        "v4.151.0-preview.1": {"headline": "A focused preview release."}
    }}


class _RepoFixture:
    """A temporary ``documentation/docfx/releases/_sources`` tree."""

    def __init__(self, tmp_dir: str):
        self.root = Path(tmp_dir)
        self.sources = self.root / updater.SOURCES_DIR
        self.sources.mkdir(parents=True)

    def write_page(self, version, *, data=None, prose=None):
        if data is not None:
            (self.sources / "{}.data.json".format(version)).write_text(
                json.dumps(data), encoding="utf-8"
            )
        if prose is not None:
            (self.sources / "{}.prose.json".format(version)).write_text(
                json.dumps(prose), encoding="utf-8"
            )


class SelectCandidatesTests(unittest.TestCase):
    def setUp(self):
        self._tmp = tempfile.TemporaryDirectory()
        self.addCleanup(self._tmp.cleanup)
        self.fixture = _RepoFixture(self._tmp.name)
        self.repository = updater.RepositoryView(self.fixture.root)

    def test_selects_a_shipment_with_matching_facts_and_summary(self):
        self.fixture.write_page("4.151.0", data=_data(), prose=_prose())
        candidates = updater.select_candidates(self.repository)
        self.assertEqual([c.tag for c in candidates], ["v4.151.0-preview.1"])

    def test_ignores_a_shipment_with_no_reviewed_summary_yet(self):
        self.fixture.write_page("4.151.0", data=_data(), prose=None)
        candidates = updater.select_candidates(self.repository)
        self.assertEqual(candidates, [])

    def test_ignores_a_page_with_no_data_json(self):
        self.fixture.write_page("4.151.0", data=None, prose=_prose())
        candidates = updater.select_candidates(self.repository)
        self.assertEqual(candidates, [])

    def test_silently_skips_old_format_data_on_a_converge_all_run(self):
        self.fixture.write_page("4.150.0", data=_data(format_version=3), prose=_prose(
            summaries={"v4.150.0": {"headline": "x"}}
        ))
        candidates = updater.select_candidates(self.repository)
        self.assertEqual(candidates, [])

    def test_raises_a_clear_error_for_an_explicitly_requested_old_format_tag(self):
        self.fixture.write_page("4.151.0", data=_data(format_version=3), prose=_prose())
        with self.assertRaisesRegex(updater.UpdateError, "unsupported release data format"):
            updater.select_candidates(self.repository, tag="v4.151.0-preview.1")

    def test_ignores_unreleased_head_pages(self):
        self.fixture.write_page(
            "4.151.0-unreleased",
            data=_data(shipments=[]),
            prose=_prose(summaries={}),
        )
        candidates = updater.select_candidates(self.repository)
        self.assertEqual(candidates, [])

    def test_filters_to_exactly_one_requested_tag(self):
        self.fixture.write_page(
            "4.151.0",
            data=_data(shipments=[
                _shipment(tag="v4.151.0-preview.1"),
                _shipment(
                    tag="v4.151.0",
                    public_version="4.151.0",
                    channel="stable",
                    label="Stable",
                    changelog_url=(
                        "https://github.com/mono/SkiaSharp/compare/"
                        "v4.150.2...v4.151.0"
                    ),
                ),
            ]),
            prose=_prose(summaries={
                "v4.151.0-preview.1": {"headline": "Preview summary."},
                "v4.151.0": {"headline": "Stable summary."},
            }),
        )
        candidates = updater.select_candidates(self.repository, tag="v4.151.0")
        self.assertEqual([c.tag for c in candidates], ["v4.151.0"])

    def test_rejects_a_shipment_whose_core_version_does_not_match_its_own_page(self):
        self.fixture.write_page(
            "4.151.0",
            data=_data(shipments=[_shipment(core_version="9.9.9")]),
            prose=_prose(),
        )
        with self.assertRaisesRegex(updater.UpdateError, "core_version.*derived from its tag"):
            updater.select_candidates(self.repository)

    def test_rejects_duplicate_shipment_tags_within_one_data_file(self):
        self.fixture.write_page(
            "4.151.0",
            data=_data(shipments=[_shipment(), _shipment()]),
            prose=_prose(),
        )
        with self.assertRaisesRegex(updater.UpdateError, "duplicate shipment tag"):
            updater.select_candidates(self.repository)

    def test_rejects_a_falsey_non_array_shipment_value(self):
        self.fixture.write_page(
            "4.151.0",
            data={
                "format": 4,
                "version": "4.151.0",
                "shipments": {},
                "contributors": [],
            },
            prose=_prose(),
        )
        with self.assertRaisesRegex(updater.UpdateError, "shipments must be an array"):
            updater.select_candidates(self.repository, tag="v4.151.0")

    def test_rejects_a_shipment_stored_under_a_different_page_version(self):
        self.fixture.write_page("4.151.0", data=_data(), prose=_prose())
        self.fixture.write_page(
            "4.151.0b",
            data={
                "format": 4,
                "version": "4.151.0b",
                "shipments": [_shipment()],
            },
            prose=_prose(),
        )
        with self.assertRaisesRegex(updater.UpdateError, "does not match its own page version"):
            updater.select_candidates(self.repository)

    def test_rejects_malformed_json(self):
        (self.fixture.sources / "4.151.0.data.json").write_text("{not json", encoding="utf-8")
        with self.assertRaisesRegex(updater.UpdateError, "not valid JSON"):
            updater.select_candidates(self.repository)

    def test_invalid_requested_tag_is_rejected(self):
        with self.assertRaisesRegex(updater.UpdateError, "invalid exact release tag"):
            updater.select_candidates(self.repository, tag="not-a-tag")


class UpdateReleasesTests(unittest.TestCase):
    def setUp(self):
        self.initial_body = GH.build_managed_body("", "## What's Changed\n* A PR by @a\n")

    def _candidate(self, **shipment_overrides):
        shipment = _shipment(**shipment_overrides)
        return updater.Candidate(
            tag=shipment["tag"],
            prose_path=Path("prose.json"),
            data_path=Path("data.json"),
            prose=_prose(summaries={shipment["tag"]: {"headline": "A focused preview release."}}),
            data=_data(shipments=[shipment]),
            shipment=shipment,
        )

    def test_updates_a_marked_release_and_reports_updated(self):
        candidate = self._candidate()
        client = FakeGitHubClient({candidate.tag: self.initial_body})
        result = updater.update_releases([candidate], client)
        self.assertEqual([e.status for e in result.entries], ["updated"])
        self.assertEqual(len(client.writes), 1)

    def test_preserves_the_generated_notes_region_byte_for_byte(self):
        candidate = self._candidate()
        client = FakeGitHubClient({candidate.tag: self.initial_body})
        updater.update_releases([candidate], client)
        (_, written_body) = client.writes[0]
        start = written_body.index(GH.GENERATED_START_MARKER)
        end = written_body.index(GH.GENERATED_END_MARKER) + len(GH.GENERATED_END_MARKER)
        original_start = self.initial_body.index(GH.GENERATED_START_MARKER)
        original_end = (
            self.initial_body.index(GH.GENERATED_END_MARKER) + len(GH.GENERATED_END_MARKER)
        )
        self.assertEqual(written_body[start:end], self.initial_body[original_start:original_end])

    def test_replaces_only_the_managed_summary_region(self):
        candidate = self._candidate()
        client = FakeGitHubClient({candidate.tag: self.initial_body})
        updater.update_releases([candidate], client)
        (_, written_body) = client.writes[0]
        self.assertIn("A focused preview release.", written_body)
        self.assertIn(GH.SUMMARY_START_MARKER, written_body)
        self.assertIn(GH.SUMMARY_END_MARKER, written_body)

    def test_skips_a_release_that_does_not_exist(self):
        candidate = self._candidate()
        client = FakeGitHubClient({})
        result = updater.update_releases([candidate], client)
        self.assertEqual(result.entries[0].status, "skipped")
        self.assertEqual(client.writes, [])

    def test_adopts_an_unmarked_release_and_preserves_its_body(self):
        candidate = self._candidate()
        original = "Just a plain GitHub-generated release body."
        client = FakeGitHubClient({candidate.tag: original})
        result = updater.update_releases([candidate], client)
        self.assertEqual(result.entries[0].status, "updated")
        self.assertEqual(len(client.writes), 1)
        (_, written_body) = client.writes[0]
        self.assertIn(original, written_body)
        self.assertIn(GH.SUMMARY_START_MARKER, written_body)
        self.assertIn(GH.GENERATED_START_MARKER, written_body)

    def test_skips_an_unpublished_draft_without_any_patch(self):
        # Summary convergence must never edit an unpublished draft.
        candidate = self._candidate()
        client = FakeGitHubClient(
            {candidate.tag: self.initial_body}, draft_tags={candidate.tag}
        )
        result = updater.update_releases([candidate], client)
        self.assertEqual(result.entries[0].status, "skipped")
        self.assertIn("draft", result.entries[0].detail)
        self.assertEqual(client.writes, [])

    def test_skips_an_unpublished_draft_even_when_its_generated_body_is_unchanged(self):
        candidate = self._candidate()
        client = FakeGitHubClient(
            {candidate.tag: self.initial_body}, draft_tags={candidate.tag}
        )
        updater.update_releases([candidate], client)
        # The draft's body -- including its GitHub-generated notes region --
        # is byte-for-byte untouched; no PATCH was ever attempted.
        self.assertEqual(client.bodies[candidate.tag], self.initial_body)
        self.assertEqual(client.writes, [])

    def test_converges_once_the_same_release_is_later_published(self):
        # The exact scenario the fix targets: a draft is skipped on one run,
        # then publication completes (the release-published event fires, or
        # this workflow's next run observes the now-published release), and
        # the SAME candidate/client converges successfully with no
        # intervening state change other than is_draft flipping to False.
        candidate = self._candidate()
        client = FakeGitHubClient(
            {candidate.tag: self.initial_body}, draft_tags={candidate.tag}
        )
        draft_result = updater.update_releases([candidate], client)
        self.assertEqual(draft_result.entries[0].status, "skipped")
        self.assertEqual(client.writes, [])

        client.publish(candidate.tag)
        published_result = updater.update_releases([candidate], client)
        self.assertEqual(published_result.entries[0].status, "updated")
        self.assertEqual(len(client.writes), 1)
        (_, written_body) = client.writes[0]
        self.assertIn("A focused preview release.", written_body)
        self.assertIn(GH.SUMMARY_START_MARKER, written_body)

    def test_is_idempotent_a_second_run_reports_unchanged_and_writes_nothing_more(self):
        candidate = self._candidate()
        client = FakeGitHubClient({candidate.tag: self.initial_body})
        first = updater.update_releases([candidate], client)
        second = updater.update_releases([candidate], client)
        self.assertEqual(first.entries[0].status, "updated")
        self.assertEqual(second.entries[0].status, "unchanged")
        self.assertEqual(len(client.writes), 1)

    def test_running_twice_produces_a_byte_identical_body(self):
        candidate = self._candidate()
        client = FakeGitHubClient({candidate.tag: self.initial_body})
        updater.update_releases([candidate], client)
        body_after_first = client.bodies[candidate.tag]
        updater.update_releases([candidate], client)
        body_after_second = client.bodies[candidate.tag]
        self.assertEqual(body_after_first, body_after_second)

    def test_aborts_the_whole_batch_when_a_concurrent_edit_is_detected(self):
        candidate = self._candidate()
        client = FakeGitHubClient(
            {candidate.tag: self.initial_body}, race_tags={candidate.tag}
        )
        with self.assertRaisesRegex(updater.UpdateError, "changed after preflight"):
            updater.update_releases([candidate], client)
        self.assertEqual(client.writes, [])

    def test_raises_when_a_write_does_not_verify(self):
        candidate = self._candidate()
        client = FakeGitHubClient(
            {candidate.tag: self.initial_body}, fail_write_tags={candidate.tag}
        )
        with self.assertRaisesRegex(updater.UpdateError, "did not match"):
            updater.update_releases([candidate], client)
        # The write was still attempted -- verification is a post-write check.
        self.assertEqual(len(client.writes), 1)

    def test_a_batch_with_one_unsafe_candidate_aborts_before_any_write(self):
        safe = self._candidate(tag="v4.151.0-preview.1")
        unsafe_shipment = _shipment(
            tag="v4.151.0-preview.2", public_version="4.151.0-preview.2", label="Preview 2"
        )
        unsafe = updater.Candidate(
            tag=unsafe_shipment["tag"],
            prose_path=Path("prose.json"),
            data_path=Path("data.json"),
            prose=_prose(summaries={
                unsafe_shipment["tag"]: {"headline": "Fixes CVE-2024-1 in a bundled library."}
            }),
            data=_data(shipments=[unsafe_shipment]),
            shipment=unsafe_shipment,
        )
        client = FakeGitHubClient({
            safe.tag: self.initial_body,
            unsafe.tag: self.initial_body,
        })
        with self.assertRaisesRegex(updater.UpdateError, "preflight failed"):
            updater.update_releases([safe, unsafe], client)
        self.assertEqual(client.writes, [])

    def test_a_malicious_pr_title_style_marker_injection_is_rejected_before_any_write(self):
        # Simulates untrusted content (e.g. a PR title an agent paraphrased)
        # smuggling a managed-marker sentinel into authored prose.
        candidate = self._candidate()
        candidate.prose["release_summaries"][candidate.tag]["body"] = (
            "Normal text {} then more text.".format(GH.SUMMARY_END_MARKER)
        )
        client = FakeGitHubClient({candidate.tag: self.initial_body})
        with self.assertRaisesRegex(updater.UpdateError, "preflight failed"):
            updater.update_releases([candidate], client)
        self.assertEqual(client.writes, [])


class RenderManagedSummaryTests(unittest.TestCase):
    def test_expands_release_links_marker_into_deterministic_links(self):
        shipment = _shipment()
        candidate = updater.Candidate(
            tag=shipment["tag"],
            prose_path=Path("prose.json"),
            data_path=Path("data.json"),
            prose=_prose(summaries={shipment["tag"]: {"headline": "A focused preview release."}}),
            data=_data(shipments=[shipment]),
            shipment=shipment,
        )
        rendered = updater.render_managed_summary(candidate)
        self.assertIn("nuget.org/packages/SkiaSharp/4.151.0-preview.1", rendered)
        self.assertIn(
            "{}/docs/releases/4.151.0.html".format(common.PUBLIC_SITE_BASE_URL),
            rendered,
        )
        self.assertIn(
            "github.com/mono/SkiaSharp/compare/v4.150.2...v4.151.0-preview.1", rendered
        )
        self.assertNotIn(updater.safety.RELEASE_LINKS_MARKER, rendered)

    def test_uses_the_explicit_public_site_value_without_inferring_the_owner(self):
        shipment = _shipment()
        candidate = updater.Candidate(
            tag=shipment["tag"],
            prose_path=Path("prose.json"),
            data_path=Path("data.json"),
            prose=_prose(
                summaries={
                    shipment["tag"]: {"headline": "A focused preview release."}
                }
            ),
            data=_data(shipments=[shipment]),
            shipment=shipment,
        )

        rendered = updater.render_managed_summary(
            candidate,
            documentation_base_url="https://learn.microsoft.com/skiasharp/",
        )

        self.assertIn(
            "https://learn.microsoft.com/skiasharp/docs/releases/4.151.0.html",
            rendered,
        )
        self.assertIn("github.com/mono/SkiaSharp/compare/", rendered)

    def test_default_public_site_value_is_read_at_call_time(self):
        shipment = _shipment()
        candidate = updater.Candidate(
            tag=shipment["tag"],
            prose_path=Path("prose.json"),
            data_path=Path("data.json"),
            prose=_prose(
                summaries={
                    shipment["tag"]: {"headline": "A focused preview release."}
                }
            ),
            data=_data(shipments=[shipment]),
            shipment=shipment,
        )

        with mock.patch.object(
            common,
            "PUBLIC_SITE_BASE_URL",
            "https://skiasharp.example.test",
        ):
            rendered = updater.render_managed_summary(candidate)

        self.assertIn(
            "https://skiasharp.example.test/docs/releases/4.151.0.html",
            rendered,
        )

    def test_omits_the_changelog_link_for_the_first_ever_shipment(self):
        shipment = _shipment(previous_tag=None, changelog_url=None)
        candidate = updater.Candidate(
            tag=shipment["tag"],
            prose_path=Path("prose.json"),
            data_path=Path("data.json"),
            prose=_prose(summaries={shipment["tag"]: {"headline": "A focused preview release."}}),
            data=_data(shipments=[shipment]),
            shipment=shipment,
        )
        rendered = updater.render_managed_summary(candidate)
        self.assertNotIn("Full changelog", rendered)

    def test_rejects_a_changelog_url_pointing_outside_the_repository(self):
        shipment = _shipment(changelog_url="https://evil.example/compare/a...b")
        candidate = updater.Candidate(
            tag=shipment["tag"],
            prose_path=Path("prose.json"),
            data_path=Path("data.json"),
            prose=_prose(summaries={shipment["tag"]: {"headline": "A focused preview release."}}),
            data=_data(shipments=[shipment]),
            shipment=shipment,
        )
        with self.assertRaisesRegex(updater.UpdateError, "invalid changelog_url"):
            updater.render_managed_summary(candidate)

    def test_rejects_a_changelog_url_for_a_different_repository_name(self):
        shipment = _shipment(
            changelog_url=(
                "https://github.com/dotnet/NotSkiaSharp/compare/"
                "v4.150.2...v4.151.0-preview.1"
            )
        )
        candidate = updater.Candidate(
            tag=shipment["tag"],
            prose_path=Path("prose.json"),
            data_path=Path("data.json"),
            prose=_prose(
                summaries={
                    shipment["tag"]: {"headline": "A focused preview release."}
                }
            ),
            data=_data(shipments=[shipment]),
            shipment=shipment,
        )

        with self.assertRaisesRegex(updater.UpdateError, "invalid changelog_url"):
            updater.render_managed_summary(candidate)


class MainEndToEndTests(unittest.TestCase):
    def setUp(self):
        self._tmp = tempfile.TemporaryDirectory()
        self.addCleanup(self._tmp.cleanup)
        self.fixture = _RepoFixture(self._tmp.name)

    def test_converges_a_push_event_and_reports_success(self):
        destination_shipment = _shipment(
            changelog_url=(
                "https://github.com/dotnet/SkiaSharp/compare/"
                "v4.150.2...v4.151.0-preview.1"
            )
        )
        self.fixture.write_page(
            "4.151.0",
            data=_data(shipments=[destination_shipment]),
            prose=_prose(),
        )
        initial_body = GH.build_managed_body("", "## What's Changed\n")
        fake_client = FakeGitHubClient({"v4.151.0-preview.1": initial_body})
        with mock.patch.object(
            GH, "RestGitHubClient", return_value=fake_client
        ) as client_type:
            exit_code = updater.main([
                "--event", "push",
                "--repository", "dotnet/SkiaSharp",
                "--documentation-base-url", "https://skiasharp.example.test",
                "--root", str(self.fixture.root),
            ])
        self.assertEqual(exit_code, 0)
        self.assertEqual(len(fake_client.writes), 1)
        client_type.assert_called_once_with("dotnet/SkiaSharp")
        self.assertIn(
            "https://skiasharp.example.test/docs/releases/4.151.0.html",
            fake_client.writes[0][1],
        )

    def test_reports_a_nonzero_exit_and_writes_a_summary_on_failure(self):
        self.fixture.write_page("4.151.0", data=_data(format_version=3), prose=_prose())
        fake_client = FakeGitHubClient({})
        with mock.patch.object(GH, "RestGitHubClient", return_value=fake_client):
            exit_code = updater.main([
                "--event", "workflow_dispatch",
                "--tag", "v4.151.0-preview.1",
                "--root", str(self.fixture.root),
            ])
        self.assertEqual(exit_code, 1)
        self.assertEqual(fake_client.writes, [])

    def test_omitted_site_argument_uses_the_refreshed_identity_value(self):
        self.fixture.write_page("4.151.0", data=_data(), prose=_prose())
        initial_body = GH.build_managed_body("", "## What's Changed\n")
        fake_client = FakeGitHubClient(
            {"v4.151.0-preview.1": initial_body}
        )
        with (
            mock.patch.object(
                common,
                "configure_identity",
                return_value={
                    "repository": "dotnet/SkiaSharp",
                    "publicSiteBaseUrl": "https://skiasharp.example.test",
                },
            ),
            mock.patch.object(
                GH, "RestGitHubClient", return_value=fake_client
            ),
        ):
            exit_code = updater.main([
                "--event", "push",
                "--repository", "dotnet/SkiaSharp",
                "--root", str(self.fixture.root),
            ])

        self.assertEqual(exit_code, 0)
        self.assertIn(
            "https://skiasharp.example.test/docs/releases/4.151.0.html",
            fake_client.writes[0][1],
        )

    def test_a_quiet_run_with_no_eligible_summaries_still_exits_zero(self):
        fake_client = FakeGitHubClient({})
        with mock.patch.object(GH, "RestGitHubClient", return_value=fake_client):
            exit_code = updater.main([
                "--event", "push",
                "--root", str(self.fixture.root),
            ])
        self.assertEqual(exit_code, 0)


if __name__ == "__main__":
    unittest.main()
