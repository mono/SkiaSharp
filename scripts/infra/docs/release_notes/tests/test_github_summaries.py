#!/usr/bin/env python3

from dataclasses import replace
import importlib.util
import json
from pathlib import Path
import sys
import unittest


SCRIPT = Path(__file__).resolve().parent.parent / "update_github_summaries.py"
SPEC = importlib.util.spec_from_file_location("release_summary_updater", SCRIPT)
updater = importlib.util.module_from_spec(SPEC)
sys.modules[SPEC.name] = updater
SPEC.loader.exec_module(updater)
REPO_ROOT = Path(__file__).resolve().parents[5]


def prose_path(version):
    return "documentation/docfx/releases/_sources/{}.prose.json".format(version)


def data_path(version):
    return "documentation/docfx/releases/_sources/{}.data.json".format(version)


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


def prose(summaries=None, **extra):
    return {
        "release_summaries": summaries or {},
        "highlights_headline": "Updates the SkiaSharp release line.",
        "highlights_body": None,
        **extra,
    }


def data(*shipments):
    return {"format": 5, "shipments": list(shipments), "prs": {}}


def marked_body(summary="", generated="line one\nline two\n"):
    return (
        "manual header\n"
        + updater.SUMMARY_START_MARKER
        + "\n"
        + summary
        + ("\n" if summary else "")
        + updater.SUMMARY_END_MARKER
        + "\nmanual between regions\n"
        + updater.GENERATED_START_MARKER
        + "\n"
        + generated
        + updater.GENERATED_END_MARKER
        + "\nmanual footer\n"
    )


def fake_renderer(data_value, prose_value, tag):
    release = prose_value.get("release_summaries", {}).get(tag)
    text = (
        release["summary"]
        if release
        else prose_value["highlights_headline"]
    )
    return text + "\n\n" + updater.RELEASE_LINKS_MARKER + "\n"


class FakeRepository:
    def __init__(self):
        self.current_paths = []
        self.current = {}

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

    def patch_release(self, release_id, body):
        self.patch_calls.append((release_id, body))
        for tag, values in self.snapshots.items():
            current = values[
                min(self.get_counts.get(tag, 1) - 1, len(values) - 1)
            ]
            if current.release_id == release_id:
                values.append(
                    replace(current, body=body, etag='"updated"')
                )
                return


def snapshot(tag, body, *, release_id=1, etag='"body-1"'):
    return updater.ReleaseSnapshot(
        release_id=release_id,
        tag=tag,
        body=body,
        etag=etag,
        url="https://github.com/mono/SkiaSharp/releases/tag/" + tag,
    )


def candidate(tag, *, channel="preview", prs=None):
    release_shipment = shipment(tag, channel=channel, prs=prs)
    summaries = {
        tag: {
            "summary": "Updates this exact release.",
            "prs": [] if prs == [] else [1],
        }
    }
    data_value = data(release_shipment)
    if channel == "stable":
        data_value["range"] = {"base_version": "4.150.0"}
    return updater.Candidate(
        tag=tag,
        prose_path=prose_path(release_shipment["core_version"]),
        data_path=data_path(release_shipment["core_version"]),
        prose=prose(summaries),
        data=data_value,
        shipment=release_shipment,
    )


class CandidateSelectionTests(unittest.TestCase):
    def test_current_selection_converges_prerelease_and_stable(self):
        repo = FakeRepository()
        path = prose_path("4.151.0")
        preview = "v4.151.0-preview.1.1"
        stable = "v4.151.0"
        repo.current_paths = [path]
        repo.current[path] = prose({
            preview: {"summary": "Adds the preview.", "prs": [1]},
            stable: {"summary": "Ships the stable release.", "prs": []},
        })
        repo.current[data_path("4.151.0")] = data(
            shipment(preview),
            shipment(stable, channel="stable", prs=[]),
        )

        selected = updater.select_current_candidates(repo)

        self.assertEqual([item.tag for item in selected], [stable, preview])

    def test_exact_tag_requires_matching_shipment_facts(self):
        repo = FakeRepository()
        path = prose_path("4.151.0")
        tag = "v4.151.0-preview.1.1"
        repo.current_paths = [path]
        repo.current[path] = prose({
            tag: {"summary": "Adds the preview.", "prs": [1]},
        })
        repo.current[data_path("4.151.0")] = data()
        with self.assertRaisesRegex(updater.UpdateError, "no exact shipment facts"):
            updater.select_current_candidates(repo, tag=tag)

    def test_unsupported_data_format_requires_regeneration(self):
        with self.assertRaisesRegex(
            updater.UpdateError,
            "unsupported release data format 4; expected 5",
        ):
            updater._shipment_map(
                {"format": 4, "shipments": []},
                data_path("4.151.0"),
            )

    def test_current_selection_skips_unsupported_cached_pages(self):
        repo = FakeRepository()
        old_path = prose_path("3.119.0")
        new_path = prose_path("4.151.0")
        old_tag = "v3.119.0-preview.1.1"
        new_tag = "v4.151.0-preview.1.1"
        repo.current_paths = [old_path, new_path]
        repo.current[old_path] = prose({
            old_tag: {"summary": "Old cached preview.", "prs": [1]},
        })
        repo.current[data_path("3.119.0")] = {
            "format": 4,
            "shipments": [shipment(old_tag)],
        }
        repo.current[new_path] = prose({
            new_tag: {"summary": "Adds the current preview.", "prs": [1]},
        })
        repo.current[data_path("4.151.0")] = data(shipment(new_tag))

        selected = updater.select_current_candidates(repo)

        self.assertEqual([item.tag for item in selected], [new_tag])

    def test_exact_request_rejects_unsupported_cached_page(self):
        repo = FakeRepository()
        path = prose_path("3.119.0")
        tag = "v3.119.0-preview.1.1"
        repo.current_paths = [path]
        repo.current[path] = prose({
            tag: {"summary": "Old cached preview.", "prs": [1]},
        })
        repo.current[data_path("3.119.0")] = {
            "format": 4,
            "shipments": [shipment(tag)],
        }

        with self.assertRaisesRegex(
            updater.UpdateError,
            "Force-regenerate this version",
        ):
            updater.select_current_candidates(repo, tag=tag)

class BodyModelTests(unittest.TestCase):
    def test_renderer_uses_summary_links_stats_and_contributors(self):
        tag = "v4.151.0-preview.1.1"
        facts = shipment(tag, prs=[1, 2])
        data_value = data(facts)
        data_value["prs"] = {
            "1": {
                "url": "https://github.com/mono/SkiaSharp/pull/1",
                "author": "contributor",
                "community": True,
                "title": "Add feature",
                "tag": "product",
            },
            "2": {
                "url": "https://github.com/mono/SkiaSharp/pull/2",
                "author": "maintainer",
                "community": False,
                "title": "Internal change",
                "tag": "internal",
            },
        }
        prose_value = prose({
            tag: {"summary": "Adds a useful feature.", "prs": [1]},
        })
        item = updater.Candidate(
            tag=tag,
            prose_path=prose_path("4.151.0"),
            data_path=data_path("4.151.0"),
            prose=prose_value,
            data=data_value,
            shipment=facts,
        )

        rendered = updater.render_managed_summary(item)

        self.assertIn("Adds a useful feature.", rendered)
        self.assertIn("2 pull requests · 1 consumer-facing", rendered)
        self.assertIn("@contributor", rendered)
        self.assertIn("packages/SkiaSharp/4.151.0-preview.1.1", rendered)
        self.assertNotIn(updater.RELEASE_LINKS_MARKER, rendered)

    def test_stable_renderer_uses_release_summary(self):
        item = candidate("v4.151.0", channel="stable", prs=[])
        rendered = updater.render_managed_summary(item)
        self.assertIn("Updates this exact release.", rendered)
        self.assertIn("0 pull requests · 0 consumer-facing", rendered)
        self.assertIn("compare/v4.150.0...v4.151.0", rendered)

    def test_replacement_preserves_generated_and_manual_content(self):
        generated = "## What's Changed\r\n* exact bytes  \r\n"
        body = marked_body("old summary", generated)
        generated_region = body[
            body.index(updater.GENERATED_START_MARKER):
            body.index(updater.GENERATED_END_MARKER)
            + len(updater.GENERATED_END_MARKER)
        ]

        updated = updater.replace_managed_summary(body, "new\n\nsummary")

        self.assertIn("manual header\n", updated)
        self.assertNotIn("old summary", updated)
        self.assertIn("new\n\nsummary", updated)
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
            updater.replace_managed_summary(
                "## What's Changed\n* old release\n",
                "reviewed summary",
            )
        )

    def test_partial_or_reordered_markers_are_rejected(self):
        with self.assertRaisesRegex(updater.UpdateError, "incomplete"):
            updater.replace_managed_summary(
                updater.SUMMARY_START_MARKER + "\nlegacy",
                "reviewed summary",
            )
        with self.assertRaisesRegex(updater.UpdateError, "out of order"):
            updater.replace_managed_summary(
                updater.GENERATED_START_MARKER
                + updater.SUMMARY_START_MARKER
                + updater.SUMMARY_END_MARKER
                + updater.GENERATED_END_MARKER,
                "reviewed summary",
            )


class UpdateTests(unittest.TestCase):
    def test_batch_updates_after_preflight(self):
        first = candidate("v4.151.0-preview.1.1")
        second = candidate("v4.151.0-rc.1.1")
        github = FakeGitHub({
            first.tag: snapshot(first.tag, marked_body(), release_id=11, etag='"a"'),
            second.tag: snapshot(second.tag, marked_body(), release_id=12, etag='"b"'),
        })

        result = updater.update_releases(
            [first, second], github, renderer=fake_renderer
        )

        self.assertEqual(
            [call[0] for call in github.patch_calls],
            [11, 12],
        )
        self.assertEqual(
            [entry.status for entry in result.entries],
            ["updated", "updated"],
        )

    def test_missing_and_unmarked_releases_are_skipped(self):
        missing = candidate("v4.151.0-preview.1.1")
        legacy = candidate("v4.151.0-rc.1.1")
        github = FakeGitHub({
            legacy.tag: snapshot(legacy.tag, "legacy body", release_id=12),
        })

        result = updater.update_releases(
            [missing, legacy], github, renderer=fake_renderer
        )

        self.assertEqual(github.patch_calls, [])
        self.assertEqual(
            [entry.status for entry in result.entries],
            ["skipped", "skipped"],
        )

    def test_idempotent_rerun_sends_no_patch(self):
        item = candidate("v4.151.0-preview.1.1")
        rendered = updater.render_managed_summary(item, fake_renderer)
        current = updater.replace_managed_summary(marked_body(), rendered)
        github = FakeGitHub({item.tag: snapshot(item.tag, current)})

        result = updater.update_releases(
            [item], github, renderer=fake_renderer
        )

        self.assertEqual(github.patch_calls, [])
        self.assertEqual(result.entries[0].status, "unchanged")

    def test_stale_body_race_blocks_entire_batch(self):
        first = candidate("v4.151.0-preview.1.1")
        second = candidate("v4.151.0-rc.1.1")
        first_initial = snapshot(first.tag, marked_body(), release_id=11, etag='"a"')
        second_initial = snapshot(second.tag, marked_body(), release_id=12, etag='"b"')
        second_changed = replace(
            second_initial,
            body=second_initial.body + "concurrent edit\n",
            etag='"b2"',
        )
        github = FakeGitHub({
            first.tag: [first_initial, first_initial],
            second.tag: [second_initial, second_changed],
        })

        with self.assertRaisesRegex(updater.UpdateError, "no PATCH was sent"):
            updater.update_releases(
                [first, second], github, renderer=fake_renderer
            )
        self.assertEqual(github.patch_calls, [])

    def test_post_patch_body_is_verified(self):
        item = candidate("v4.151.0-preview.1.1")

        class NonPersistingGitHub(FakeGitHub):
            def patch_release(self, release_id, body):
                self.patch_calls.append((release_id, body))

        github = NonPersistingGitHub({
            item.tag: snapshot(item.tag, marked_body()),
        })

        with self.assertRaisesRegex(
            updater.UpdateError,
            "did not match the requested managed summary",
        ):
            updater.update_releases(
                [item],
                github,
                renderer=fake_renderer,
            )


class GitHubClientTests(unittest.TestCase):
    def test_patch_sends_authenticated_body(self):
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
        client.patch_release(42, "new body")

        req, timeout = requests[0]
        self.assertEqual(req.method, "PATCH")
        self.assertIsNone(req.get_header("If-match"))
        self.assertEqual(
            req.get_header("Authorization"),
            "Bearer test-token",
        )
        self.assertEqual(json.loads(req.data), {"body": "new body"})
        self.assertEqual(timeout, 30)


class WorkflowTests(unittest.TestCase):
    def test_workflow_is_deterministic_and_least_privilege(self):
        workflow = (
            REPO_ROOT / ".github/workflows/update-github-release-summaries.yml"
        ).read_text(encoding="utf-8")
        self.assertIn("branches: [main]", workflow)
        self.assertIn(
            "documentation/docfx/releases/_sources/*.prose.json", workflow
        )
        self.assertIn(
            "scripts/infra/docs/release_notes/render.py", workflow
        )
        self.assertIn(
            "scripts/infra/docs/release_notes/update_github_summaries.py",
            workflow,
        )
        self.assertIn("types: [published]", workflow)
        self.assertIn("contents: write", workflow)
        self.assertNotIn("pull-requests:", workflow)
        self.assertIn("cancel-in-progress: false", workflow)
        self.assertIn("persist-credentials: false", workflow)
        self.assertIn("release_notes/update_github_summaries.py", workflow)
        self.assertNotIn("BEFORE_SHA", workflow)
        self.assertNotIn("DISPATCH_TAG", workflow)
        self.assertNotIn("copilot", workflow.lower())
        self.assertNotIn("gh ", workflow)


if __name__ == "__main__":
    unittest.main()
