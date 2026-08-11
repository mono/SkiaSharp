#!/usr/bin/env python3

from dataclasses import replace
import importlib.util
import json
from pathlib import Path
import sys
import unittest
from unittest import mock


SCRIPT = Path(__file__).resolve().parent.parent / "update-github-release-teasers.py"
SPEC = importlib.util.spec_from_file_location("release_teaser_updater", SCRIPT)
updater = importlib.util.module_from_spec(SPEC)
sys.modules[SPEC.name] = updater
SPEC.loader.exec_module(updater)
REPO_ROOT = Path(__file__).resolve().parents[4]


def prose_path(version):
    return (
        "documentation/docfx/releases/_sources/"
        + version
        + ".prose.json"
    )


def data_path(version):
    return (
        "documentation/docfx/releases/_sources/"
        + version
        + ".data.json"
    )


def shipment(tag, *, channel="preview", prs=None):
    return {
        "tag": tag,
        "core_version": tag[1:].split("-", 1)[0],
        "public_version": tag[1:],
        "channel": channel,
        "label": channel.title(),
        "previous_tag": "v0.0.1",
        "target_sha": "a" * 40,
        "date": "August 9, 2026",
        "changelog_url": (
            "https://github.com/mono/SkiaSharp/compare/v0.0.1...{}".format(tag)
        ),
        "prs": [1] if prs is None else prs,
    }


def prose(teasers, **extra):
    return {"release_teasers": teasers, **extra}


def data(*shipments):
    return {"format": 4, "shipments": list(shipments), "prs": {}}


def marked_body(teaser="", generated="line one\nline two\n"):
    return (
        "manual header\n"
        + updater.TEASER_START_MARKER
        + "\n"
        + teaser
        + ("\n" if teaser else "")
        + updater.TEASER_END_MARKER
        + "\nmanual between regions\n"
        + updater.GENERATED_START_MARKER
        + "\n"
        + generated
        + updater.GENERATED_END_MARKER
        + "\nmanual footer\n"
    )


def fake_renderer(data_value, prose_value, tag):
    subtitle = prose_value.get("release_teasers", {}).get(tag, {}).get(
        "subtitle", "No additional package changes.")
    return subtitle + "\n\n" + updater.RELEASE_LINKS_MARKER + "\n"


class FakeRepository:
    def __init__(self):
        self.changed = []
        self.historical = {}
        self.current_paths = []
        self.current = {}

    def changed_prose_paths(self, before, after):
        return list(self.changed)

    def json_at(self, ref, path):
        return self.historical.get((ref, path))

    def current_prose_paths(self):
        return list(self.current_paths)

    def current_json(self, path):
        return self.current.get(path)


class FakeGitHub:
    def __init__(self, snapshots):
        self.snapshots = {
            tag: list(values) if isinstance(values, list) else [values]
            for tag, values in snapshots.items()
        }
        self.get_counts = {}
        self.patch_calls = []

    def get_release(self, tag):
        values = self.snapshots.get(tag)
        if not values:
            return None
        index = self.get_counts.get(tag, 0)
        self.get_counts[tag] = index + 1
        return values[min(index, len(values) - 1)]

    def patch_release(self, release_id, body, *, expected_etag):
        self.patch_calls.append((release_id, body, expected_etag))


def snapshot(tag, body, *, release_id=1, etag='"body-1"'):
    return updater.ReleaseSnapshot(
        release_id=release_id,
        tag=tag,
        body=body,
        etag=etag,
        url="https://github.com/mono/SkiaSharp/releases/tag/" + tag,
    )


def candidate(tag, teaser=None, *, release_shipment=None):
    release_shipment = release_shipment or shipment(tag)
    prose_value = prose({
        tag: teaser or {"subtitle": "Reviewed {}".format(tag), "categories": []}
    })
    data_value = data(release_shipment)
    return updater.Candidate(
        tag=tag,
        prose_path=prose_path(release_shipment["core_version"]),
        data_path=data_path(release_shipment["core_version"]),
        prose=prose_value,
        data=data_value,
        shipment=release_shipment,
    )


class CandidateSelectionTests(unittest.TestCase):
    BEFORE = "1" * 40
    AFTER = "2" * 40

    def test_data_only_push_maps_to_owning_prose_file(self):
        changed = data_path("4.151.0")
        with mock.patch.object(updater, "_run_git", return_value=changed + "\n"):
            paths = updater.RepositoryView(REPO_ROOT).changed_prose_paths(
                self.BEFORE, self.AFTER)

        self.assertEqual(paths, [prose_path("4.151.0")])

    def test_push_selects_batch_changes_and_multiple_tags_per_line(self):
        repo = FakeRepository()
        first = prose_path("4.151.0")
        second = prose_path("4.151.1")
        repo.changed = [first, second]
        tag1 = "v4.151.0-preview.1.1"
        tag2 = "v4.151.0-rc.1.1"
        tag3 = "v4.151.1-preview.1.1"
        repo.historical.update({
            (self.BEFORE, first): prose({
                tag1: {"subtitle": "Old", "categories": []},
            }),
            (self.AFTER, first): prose({
                tag1: {"subtitle": "New", "categories": []},
                tag2: {"subtitle": "RC", "categories": []},
            }),
            (self.BEFORE, data_path("4.151.0")): data(shipment(tag1)),
            (self.AFTER, data_path("4.151.0")): data(
                shipment(tag1), shipment(tag2)),
            (self.BEFORE, second): prose({}),
            (self.AFTER, second): prose({
                tag3: {"subtitle": "Hotfix", "categories": []},
            }),
            (self.BEFORE, data_path("4.151.1")): data(),
            (self.AFTER, data_path("4.151.1")): data(shipment(tag3)),
        })

        candidates = updater.select_push_candidates(
            repo, self.BEFORE, self.AFTER)

        self.assertEqual(
            [item.tag for item in candidates],
            [tag1, tag2, tag3],
        )

    def test_push_ignores_cumulative_only_prose_change(self):
        repo = FakeRepository()
        path = prose_path("4.151.0")
        tag = "v4.151.0-preview.1.1"
        teaser = {"subtitle": "Same", "categories": []}
        facts = data(shipment(tag))
        repo.changed = [path]
        repo.historical.update({
            (self.BEFORE, path): prose({tag: teaser}, theme="Old"),
            (self.AFTER, path): prose({tag: teaser}, theme="New"),
            (self.BEFORE, data_path("4.151.0")): facts,
            (self.AFTER, data_path("4.151.0")): facts,
        })

        self.assertEqual(
            updater.select_push_candidates(repo, self.BEFORE, self.AFTER),
            [],
        )

    def test_push_selects_new_deterministic_empty_stable_shipment(self):
        repo = FakeRepository()
        path = prose_path("4.151.0")
        tag = "v4.151.0"
        stable = shipment(tag, channel="stable", prs=[])
        repo.changed = [path]
        repo.historical.update({
            (self.BEFORE, path): prose({}, theme="Preview"),
            (self.AFTER, path): prose({}, theme="Stable"),
            (self.BEFORE, data_path("4.151.0")): data(),
            (self.AFTER, data_path("4.151.0")): data(stable),
        })

        candidates = updater.select_push_candidates(
            repo, self.BEFORE, self.AFTER)

        self.assertEqual([item.tag for item in candidates], [tag])

    def test_push_skips_unreleased_source(self):
        repo = FakeRepository()
        path = prose_path("4.152.0-unreleased")
        repo.changed = [path]
        repo.historical[(self.AFTER, path)] = prose({
            "v4.152.0-preview.1.1": {"subtitle": "No", "categories": []},
        })

        self.assertEqual(
            updater.select_push_candidates(repo, self.BEFORE, self.AFTER),
            [],
        )

    def test_current_selection_converges_exact_published_tag(self):
        repo = FakeRepository()
        path = prose_path("4.151.0")
        tag = "v4.151.0-preview.1.1"
        repo.current_paths = [path]
        repo.current[path] = prose({
            tag: {"subtitle": "Reviewed", "categories": []},
        })
        repo.current[data_path("4.151.0")] = data(shipment(tag))

        selected = updater.select_current_candidates(repo, tag=tag)

        self.assertEqual([item.tag for item in selected], [tag])

    def test_current_selection_converges_empty_stable_without_ai_entry(self):
        repo = FakeRepository()
        path = prose_path("4.151.0")
        tag = "v4.151.0"
        repo.current_paths = [path]
        repo.current[path] = prose({})
        repo.current[data_path("4.151.0")] = data(
            shipment(tag, channel="stable", prs=[]))

        selected = updater.select_current_candidates(repo, tag=tag)

        self.assertEqual([item.tag for item in selected], [tag])

    def test_exact_tag_requires_matching_shipment_facts(self):
        repo = FakeRepository()
        path = prose_path("4.151.0")
        tag = "v4.151.0-preview.1.1"
        repo.current_paths = [path]
        repo.current[path] = prose({
            tag: {"subtitle": "Reviewed", "categories": []},
        })
        repo.current[data_path("4.151.0")] = data()

        with self.assertRaisesRegex(
            updater.UpdateError, "no exact shipment facts"
        ):
            updater.select_current_candidates(repo, tag=tag)

    def test_alpha_and_beta_exact_tags_are_supported(self):
        repo = FakeRepository()
        path = prose_path("4.151.0")
        alpha = "v4.151.0-alpha.1.1"
        beta = "v4.151.0-beta.2.3"
        repo.current_paths = [path]
        repo.current[path] = prose({
            alpha: {"subtitle": "Alpha", "categories": []},
            beta: {"subtitle": "Beta", "categories": []},
        })
        repo.current[data_path("4.151.0")] = data(
            shipment(alpha, channel="alpha"),
            shipment(beta, channel="beta"),
        )

        selected = updater.select_current_candidates(repo)

        self.assertEqual([item.tag for item in selected], [alpha, beta])

    def test_shipment_core_must_match_owning_prose_file(self):
        repo = FakeRepository()
        path = prose_path("4.151.0")
        tag = "v4.151.0-preview.1.1"
        wrong = shipment(tag)
        wrong["core_version"] = "4.152.0"
        repo.current_paths = [path]
        repo.current[path] = prose({
            tag: {"subtitle": "Reviewed", "categories": []},
        })
        repo.current[data_path("4.151.0")] = data(wrong)

        with self.assertRaisesRegex(updater.UpdateError, "does not match"):
            updater.select_current_candidates(repo, tag=tag)

    def test_release_event_skips_unmanaged_tag(self):
        with (
            mock.patch.object(updater, "_write_summary") as summary,
            mock.patch.object(updater, "RepositoryView") as repository,
        ):
            status = updater.main([
                "--event", "release",
                "--tag", "native-assets",
            ])

        self.assertEqual(status, 0)
        repository.assert_not_called()
        result = summary.call_args.args[0]
        self.assertEqual(result.entries[0].status, "skipped")


class BodyModelTests(unittest.TestCase):
    def test_release_notes_renderer_and_link_expansion_are_integrated(self):
        tag = "v4.151.0-preview.1.1"
        facts = shipment(tag)
        data_value = data(facts)
        data_value["prs"] = {
            "1": {
                "url": "https://github.com/mono/SkiaSharp/pull/1",
                "author": "contributor",
                "community": True,
                "title": "Add feature",
                "tag": "product",
            },
        }
        prose_value = prose({
            tag: {
                "subtitle": "A reviewed preview.",
                "website_summary": "Preview summary.",
                "categories": [{
                    "heading": "What's New",
                    "bullets": [{"text": "Added feature", "prs": [1]}],
                }],
            },
        })
        item = updater.Candidate(
            tag=tag,
            prose_path=prose_path("4.151.0"),
            data_path=data_path("4.151.0"),
            prose=prose_value,
            data=data_value,
            shipment=facts,
        )

        rendered = updater.render_managed_teaser(item)

        self.assertIn("A reviewed preview.", rendered)
        self.assertIn("What's New", rendered)
        self.assertIn("packages/SkiaSharp/4.151.0-preview.1.1", rendered)
        self.assertIn("docs/releases/4.151.0.html", rendered)
        self.assertIn("compare/v0.0.1...v4.151.0-preview.1.1", rendered)
        self.assertNotIn(updater.RELEASE_LINKS_MARKER, rendered)

    def test_replacement_preserves_generated_payload_and_manual_content(self):
        generated = "## What's Changed\r\n* exact bytes  \r\n"
        body = marked_body("old teaser", generated)
        generated_region = body[
            body.index(updater.GENERATED_START_MARKER):
            body.index(updater.GENERATED_END_MARKER)
            + len(updater.GENERATED_END_MARKER)
        ]

        updated = updater.replace_managed_teaser(body, "new\n\nteaser")

        self.assertIn("manual header\n", updated)
        self.assertIn("\nmanual between regions\n", updated)
        self.assertTrue(updated.endswith("\nmanual footer\n"))
        self.assertNotIn("old teaser", updated)
        self.assertIn("new\n\nteaser", updated)
        self.assertEqual(
            updated[
                updated.index(updater.GENERATED_START_MARKER):
                updated.index(updater.GENERATED_END_MARKER)
                + len(updater.GENERATED_END_MARKER)
            ],
            generated_region,
        )

    def test_unmarked_legacy_body_is_not_adopted(self):
        self.assertIsNone(
            updater.replace_managed_teaser(
                "## What's Changed\n* old release\n",
                "reviewed teaser",
            )
        )

    def test_partial_or_reordered_markers_are_rejected(self):
        with self.assertRaisesRegex(updater.UpdateError, "incomplete"):
            updater.replace_managed_teaser(
                updater.TEASER_START_MARKER + "\nlegacy",
                "reviewed teaser",
            )
        with self.assertRaisesRegex(updater.UpdateError, "out of order"):
            updater.replace_managed_teaser(
                updater.GENERATED_START_MARKER
                + updater.TEASER_START_MARKER
                + updater.TEASER_END_MARKER
                + updater.GENERATED_END_MARKER,
                "reviewed teaser",
            )


class UpdateTests(unittest.TestCase):
    def test_batch_updates_after_preflight_and_passes_expected_etag(self):
        first = candidate("v4.151.0-preview.1.1")
        second = candidate("v4.151.0-rc.1.1")
        github = FakeGitHub({
            first.tag: snapshot(first.tag, marked_body(), release_id=11, etag='"a"'),
            second.tag: snapshot(second.tag, marked_body(), release_id=12, etag='"b"'),
        })

        result = updater.update_releases(
            [first, second], github, renderer=fake_renderer)

        self.assertEqual(
            [(call[0], call[2]) for call in github.patch_calls],
            [(11, '"a"'), (12, '"b"')],
        )
        self.assertEqual(
            [entry.status for entry in result.entries],
            ["updated", "updated"],
        )

    def test_missing_release_and_legacy_release_are_clear_skips(self):
        missing = candidate("v4.151.0-preview.1.1")
        legacy = candidate("v4.151.0-rc.1.1")
        github = FakeGitHub({
            legacy.tag: snapshot(legacy.tag, "legacy body", release_id=12),
        })

        result = updater.update_releases(
            [missing, legacy], github, renderer=fake_renderer)

        self.assertEqual(github.patch_calls, [])
        self.assertEqual(
            [(entry.tag, entry.status) for entry in result.entries],
            [(missing.tag, "skipped"), (legacy.tag, "skipped")],
        )
        self.assertIn("does not exist", result.entries[0].detail)
        self.assertIn("legacy", result.entries[1].detail)

    def test_unmarked_legacy_empty_stable_release_is_never_rewritten(self):
        release_shipment = shipment("v4.150.0", channel="stable", prs=[])
        legacy = candidate(
            "v4.150.0",
            release_shipment=release_shipment,
        )
        github = FakeGitHub({
            legacy.tag: snapshot(
                legacy.tag,
                "## What's Changed\n\nExisting curated stable notes.\n",
                release_id=150,
            ),
        })

        result = updater.update_releases(
            [legacy], github, renderer=fake_renderer)

        self.assertEqual(github.patch_calls, [])
        self.assertEqual(result.entries[0].status, "skipped")
        self.assertIn("no managed markers", result.entries[0].detail)

    def test_idempotent_rerun_sends_no_patch(self):
        item = candidate("v4.151.0-preview.1.1")
        rendered = updater.render_managed_teaser(item, fake_renderer)
        current = updater.replace_managed_teaser(marked_body(), rendered)
        github = FakeGitHub({item.tag: snapshot(item.tag, current)})

        result = updater.update_releases(
            [item], github, renderer=fake_renderer)

        self.assertEqual(github.patch_calls, [])
        self.assertEqual(result.entries[0].status, "unchanged")

    def test_stale_body_race_rejects_entire_batch_before_patch(self):
        first = candidate("v4.151.0-preview.1.1")
        second = candidate("v4.151.0-rc.1.1")
        first_initial = snapshot(first.tag, marked_body(), release_id=11, etag='"a"')
        second_initial = snapshot(second.tag, marked_body(), release_id=12, etag='"b"')
        second_changed = replace(
            second_initial,
            body=second_initial.body + "concurrent manual edit\n",
            etag='"b2"',
        )
        github = FakeGitHub({
            first.tag: [first_initial, first_initial],
            second.tag: [second_initial, second_changed],
        })

        with self.assertRaisesRegex(
            updater.UpdateError, "no PATCH was sent"
        ):
            updater.update_releases(
                [first, second], github, renderer=fake_renderer)

        self.assertEqual(github.patch_calls, [])

    def test_malformed_later_target_blocks_all_patches(self):
        first = candidate("v4.151.0-preview.1.1")
        second = candidate("v4.151.0-rc.1.1")
        github = FakeGitHub({
            first.tag: snapshot(first.tag, marked_body(), release_id=11),
            second.tag: snapshot(
                second.tag,
                updater.TEASER_START_MARKER + "\ncorrupt",
                release_id=12,
            ),
        })

        with self.assertRaisesRegex(updater.UpdateError, "preflight failed"):
            updater.update_releases(
                [first, second], github, renderer=fake_renderer)

        self.assertEqual(github.patch_calls, [])

    def test_release_published_event_converges_reviewed_prose(self):
        repo = FakeRepository()
        path = prose_path("4.151.0")
        tag = "v4.151.0-preview.1.1"
        repo.current_paths = [path]
        repo.current[path] = prose({
            tag: {"subtitle": "Already reviewed", "categories": []},
        })
        repo.current[data_path("4.151.0")] = data(shipment(tag))
        candidates = updater.select_current_candidates(repo, tag=tag)
        github = FakeGitHub({
            tag: snapshot(tag, marked_body(), release_id=31, etag='"release"'),
        })

        result = updater.update_releases(
            candidates, github, renderer=fake_renderer)

        self.assertEqual(len(github.patch_calls), 1)
        self.assertEqual(result.entries[0].status, "updated")


class GitHubClientTests(unittest.TestCase):
    def test_patch_sends_expected_etag_without_shelling_out(self):
        requests = []

        class Response:
            headers = {"ETag": '"next"'}

            def __enter__(self):
                return self

            def __exit__(self, *args):
                return None

            def read(self):
                return b'{"id":42}'

        def open_request(req, timeout):
            requests.append((req, timeout))
            return Response()

        client = updater.GitHubClient(
            "mono/SkiaSharp",
            "test-token",
            api_url="https://example.invalid",
            opener=open_request,
        )

        client.patch_release(42, "new body", expected_etag='"expected"')

        req, timeout = requests[0]
        self.assertEqual(req.method, "PATCH")
        self.assertEqual(req.get_header("If-match"), '"expected"')
        self.assertEqual(json.loads(req.data), {"body": "new body"})
        self.assertEqual(timeout, 30)


class WorkflowTests(unittest.TestCase):
    def test_workflow_is_deterministic_and_least_privilege(self):
        workflow = (
            REPO_ROOT / ".github/workflows/update-github-release-teasers.yml"
        ).read_text(encoding="utf-8")

        self.assertIn("branches: [main]", workflow)
        self.assertIn(
            "documentation/docfx/releases/_sources/*.prose.json", workflow)
        self.assertIn("types: [published]", workflow)
        self.assertIn("workflow_dispatch:", workflow)
        self.assertIn("contents: write", workflow)
        self.assertNotIn("pull-requests:", workflow)
        self.assertNotIn("issues:", workflow)
        self.assertIn("cancel-in-progress: false", workflow)
        self.assertIn("persist-credentials: false", workflow)
        self.assertIn("update-github-release-teasers.py", workflow)
        self.assertNotIn("copilot", workflow.lower())
        self.assertNotIn("gh ", workflow)


if __name__ == "__main__":
    unittest.main()
