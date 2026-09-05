from __future__ import annotations

import sys
import unittest
from pathlib import Path
from unittest import mock

# scripts/infra/docs/release_notes/tests/ -> parents[2] == scripts/infra/docs
_DOCS_DIR = Path(__file__).resolve().parents[2]
if str(_DOCS_DIR) not in sys.path:
    sys.path.insert(0, str(_DOCS_DIR))

from release_notes import common, github
from repository_identity import IdentityError


def _load_release_notes_data_module():
    import importlib.util

    gen_path = _DOCS_DIR / "release-notes-data.py"
    spec = importlib.util.spec_from_file_location("_rn_data_format_check", str(gen_path))
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    return module


def _load_release_notes_renderer_module():
    import importlib.util

    renderer_path = _DOCS_DIR / "release-notes-render.py"
    spec = importlib.util.spec_from_file_location(
        "_rn_renderer_identity_check", str(renderer_path)
    )
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    return module


class ParseTagTests(unittest.TestCase):
    def test_stable_tag(self):
        parsed = common.parse_tag("v4.151.0")
        self.assertIsNotNone(parsed)
        self.assertEqual(parsed.core, "4.151.0")
        self.assertEqual(parsed.core_tuple, (4, 151, 0, 0))
        self.assertIsNone(parsed.channel)
        self.assertEqual(parsed.channel_name, "stable")
        self.assertIsNone(parsed.hotfix)
        self.assertEqual(parsed.label, "Stable")
        self.assertEqual(parsed.public_version, "4.151.0")

    def test_hotfix_tag(self):
        parsed = common.parse_tag("v3.119.0.1")
        self.assertIsNotNone(parsed)
        self.assertEqual(parsed.core, "3.119.0.1")
        self.assertEqual(parsed.core_tuple, (3, 119, 0, 1))
        self.assertIsNone(parsed.channel)
        self.assertEqual(parsed.hotfix, 1)
        self.assertEqual(parsed.label, "Hotfix")

    def test_preview_tag_with_build_revision(self):
        # The real historical/current tag shape: "-preview.<milestone>.<build>".
        parsed = common.parse_tag("v4.150.0-preview.1.1")
        self.assertIsNotNone(parsed)
        self.assertEqual(parsed.core, "4.150.0")
        self.assertEqual(parsed.channel, "preview")
        self.assertEqual(parsed.milestone, 1)
        self.assertEqual(parsed.build, (1,))
        self.assertEqual(parsed.label, "Preview 1 (Build 1)")
        self.assertEqual(parsed.public_version, "4.150.0-preview.1.1")

    def test_parses_two_part_arcade_build_revision(self):
        parsed = common.parse_tag("v4.152.0-rc.1.26426.14")

        self.assertEqual(parsed.build, (26426, 14))
        self.assertEqual(parsed.public_version, "4.152.0-rc.1.26426.14")

    def test_preview_tag_without_build_revision(self):
        # The release-automation-v2 tag shape: "-preview.<milestone>" only.
        parsed = common.parse_tag("v3.119.0-preview.1")
        self.assertIsNotNone(parsed)
        self.assertEqual(parsed.core, "3.119.0")
        self.assertEqual(parsed.channel, "preview")
        self.assertEqual(parsed.milestone, 1)
        self.assertIsNone(parsed.build)
        self.assertEqual(parsed.label, "Preview 1")

    def test_rc_tag(self):
        parsed = common.parse_tag("v4.150.0-rc.1.1")
        self.assertIsNotNone(parsed)
        self.assertEqual(parsed.channel, "rc")
        self.assertEqual(parsed.channel_name, "rc")
        self.assertEqual(parsed.milestone, 1)
        self.assertEqual(parsed.label, "Release Candidate 1 (Build 1)")

    def test_rejects_decorative_legacy_labels(self):
        # These exist in the repository's real tag history but must never be
        # treated as an exact shipment: they cannot be confidently classified
        # as stable, hotfix, preview, or rc.
        for tag in ("v1.49.2.1-beta", "v4.150.0-gpu1", "v4.148.0-alpha", "v4.148.0-preview.2.x"):
            with self.subTest(tag=tag):
                self.assertIsNone(common.parse_tag(tag))

    def test_rejects_non_tag_strings(self):
        for value in ("not-a-tag", "4.151.0", "vX.Y.Z", ""):
            with self.subTest(value=value):
                self.assertIsNone(common.parse_tag(value))

    def test_sort_key_orders_preview_before_rc_before_stable(self):
        preview = common.parse_tag("v4.150.0-preview.1.1")
        rc = common.parse_tag("v4.150.0-rc.1.1")
        stable = common.parse_tag("v4.150.0")
        self.assertLess(preview.sort_key, rc.sort_key)
        self.assertLess(rc.sort_key, stable.sort_key)

    def test_sort_key_orders_by_milestone_then_build(self):
        first = common.parse_tag("v4.150.0-preview.1.1")
        second = common.parse_tag("v4.150.0-preview.1.2")
        third = common.parse_tag("v4.150.0-preview.2.1")
        self.assertLess(first.sort_key, second.sort_key)
        self.assertLess(second.sort_key, third.sort_key)

    def test_sort_key_orders_by_core_version_first(self):
        older = common.parse_tag("v4.150.1")
        newer_preview = common.parse_tag("v4.151.0-preview.1")
        self.assertLess(older.sort_key, newer_preview.sort_key)


class CoreTupleTests(unittest.TestCase):
    def test_pads_missing_segments_with_zero(self):
        self.assertEqual(common.core_tuple("4.151.0"), (4, 151, 0, 0))

    def test_keeps_four_segments(self):
        self.assertEqual(common.core_tuple("3.119.0.1"), (3, 119, 0, 1))


class NotesSidecarTests(unittest.TestCase):
    def setUp(self):
        self.module = _load_release_notes_data_module()

    @staticmethod
    def _write_page_and_notes(releases, version, text):
        (releases / "{}.md".format(version)).write_text("# {}".format(version))
        notes = releases / "_sources" / "{}.notes.md".format(version)
        notes.parent.mkdir(exist_ok=True)
        notes.write_text(text)

    def test_cumulative_page_carries_notes_after_its_stable_base(self):
        import tempfile

        with tempfile.TemporaryDirectory() as tmp_dir:
            releases = Path(tmp_dir)
            self._write_page_and_notes(releases, "4.151.2", "Already shipped")
            self._write_page_and_notes(releases, "4.153.0", "Behavior changed")
            self._write_page_and_notes(releases, "4.154.0", "Current note")

            notes = self.module.load_notes_sidecars(
                "4.154.0", "4.151.2", "4.154.0", releases
            )

            self.assertEqual(
                [note["path"] for note in notes],
                [
                    "_sources/4.153.0.notes.md",
                    "_sources/4.154.0.notes.md",
                ],
            )

    def test_stable_baseline_stops_carrying_prior_notes(self):
        import tempfile

        with tempfile.TemporaryDirectory() as tmp_dir:
            releases = Path(tmp_dir)
            self._write_page_and_notes(releases, "4.153.0", "Already shipped")
            self._write_page_and_notes(releases, "4.154.0", "Current note")

            notes = self.module.load_notes_sidecars(
                "4.154.0", "4.153.0", "4.154.0", releases
            )

            self.assertEqual(
                [note["path"] for note in notes],
                ["_sources/4.154.0.notes.md"],
            )

    def test_orphan_notes_do_not_leak_into_a_cumulative_page(self):
        import tempfile

        with tempfile.TemporaryDirectory() as tmp_dir:
            releases = Path(tmp_dir)
            sources = releases / "_sources"
            sources.mkdir()
            (sources / "4.153.0.notes.md").write_text("Orphan note")
            self._write_page_and_notes(releases, "4.154.0", "Current note")

            notes = self.module.load_notes_sidecars(
                "4.154.0", "4.151.2", "4.154.0", releases
            )

            self.assertEqual(
                [note["path"] for note in notes],
                ["_sources/4.154.0.notes.md"],
            )

    def test_unreleased_head_uses_only_its_own_notes(self):
        import tempfile

        with tempfile.TemporaryDirectory() as tmp_dir:
            releases = Path(tmp_dir)
            self._write_page_and_notes(releases, "4.153.0", "Prior line")
            (releases / "4.154.0-unreleased.md").write_text("# Unreleased")
            notes = releases / "_sources" / "4.154.0-unreleased.notes.md"
            notes.write_text("Head delta")

            loaded = self.module.load_notes_sidecars(
                "4.154.0-unreleased", "4.153.0", "4.154.0", releases
            )

            self.assertEqual(
                [note["path"] for note in loaded],
                ["_sources/4.154.0-unreleased.notes.md"],
            )

    def test_build_data_json_emits_each_cumulative_notes_candidate(self):
        notes = [
            {"path": "_sources/4.153.0.notes.md", "sha256": "sha256:153"},
            {"path": "_sources/4.154.0.notes.md", "sha256": "sha256:154"},
        ]

        data = self.module.build_data_json(
            [],
            {
                "version": "4.154.0",
                "status": "unreleased",
                "companions": {"notes": notes},
            },
        )

        self.assertEqual(
            data["breaking_candidates"],
            [
                {
                    "source": "notes-sidecar",
                    "path": "_sources/4.153.0.notes.md",
                    "sha256": "sha256:153",
                    "prs": [],
                },
                {
                    "source": "notes-sidecar",
                    "path": "_sources/4.154.0.notes.md",
                    "sha256": "sha256:154",
                    "prs": [],
                },
            ],
        )

    def test_stable_named_sidecar_matches_an_unreleased_page(self):
        import tempfile

        with tempfile.TemporaryDirectory() as tmp_dir:
            releases = Path(tmp_dir)
            (releases / "4.154.0-unreleased.md").write_text("# Unreleased")

            self.assertTrue(
                self.module._notes_sidecar_has_page("4.154.0", releases)
            )


class ReleaseGithubTests(unittest.TestCase):
    def test_owns_the_managed_markers(self):
        self.assertEqual(github.SUMMARY_START_MARKER, "<!-- SKIASHARP:RELEASE-SUMMARY:START -->")
        self.assertEqual(github.SUMMARY_END_MARKER, "<!-- SKIASHARP:RELEASE-SUMMARY:END -->")
        self.assertEqual(
            github.GENERATED_START_MARKER, "<!-- SKIASHARP:GITHUB-GENERATED-NOTES:START -->"
        )
        self.assertEqual(
            github.GENERATED_END_MARKER, "<!-- SKIASHARP:GITHUB-GENERATED-NOTES:END -->"
        )


class DataFormatSyncTests(unittest.TestCase):
    """release-notes-data.py's format bump and this package's expectation of it
    must never drift apart -- see common.DATA_FORMAT's docstring."""

    def test_matches_release_notes_data_format_version(self):
        module = _load_release_notes_data_module()
        self.assertEqual(module._DATA_JSON_FORMAT_VERSION, common.DATA_FORMAT)


class WebsiteContentUnchangedTests(unittest.TestCase):
    """_website_content_unchanged() ignores "format" and "shipments" -- a
    format bump (3 -> 4) or a shipments-only change must never, by itself,
    make it report "changed": that drives whether prose is discarded and the
    page is added to files-to-polish, and neither should happen just because
    a new/altered exact shipment tag appeared with no other fact moving."""

    def setUp(self):
        self.module = _load_release_notes_data_module()

    def _write(self, tmp_dir, data):
        import json as _json

        path = Path(tmp_dir) / "4.150.0.data.json"
        path.write_text(_json.dumps(data))
        return path

    def test_a_bare_format_bump_with_no_other_changes_is_unchanged(self):
        import tempfile

        old = {"format": 3, "version": "4.150.0", "prs": {}}
        new = {"format": 4, "version": "4.150.0", "prs": {}, "shipments": []}
        with tempfile.TemporaryDirectory() as tmp_dir:
            path = self._write(tmp_dir, old)
            self.assertTrue(self.module._website_content_unchanged(path, new))

    def test_newly_appearing_or_shifting_shipments_alone_is_unchanged(self):
        import tempfile

        old = {"format": 3, "version": "4.150.0", "prs": {}}
        new = {
            "format": 4,
            "version": "4.150.0",
            "prs": {},
            "shipments": [{"tag": "v4.150.0", "target_sha": "a" * 40}],
        }
        with tempfile.TemporaryDirectory() as tmp_dir:
            path = self._write(tmp_dir, old)
            self.assertTrue(self.module._website_content_unchanged(path, new))

    def test_a_genuine_content_change_is_still_detected(self):
        import tempfile

        old = {"format": 3, "version": "4.150.0", "prs": {}}
        new = {"format": 4, "version": "4.150.0", "prs": {"1": {"title": "New PR"}}, "shipments": []}
        with tempfile.TemporaryDirectory() as tmp_dir:
            path = self._write(tmp_dir, old)
            self.assertFalse(self.module._website_content_unchanged(path, new))

    def test_a_missing_data_json_is_changed(self):
        module = self.module
        self.assertFalse(
            module._website_content_unchanged(Path("/nonexistent/4.150.0.data.json"), {})
        )


class DataJsonUnchangedIsStrictTests(unittest.TestCase):
    """_data_json_unchanged() is the genuine no-op check: unlike
    _website_content_unchanged(), it must NOT ignore "format"/"shipments" --
    otherwise a newly published preview/RC tag (the only fact that moved)
    would never be written to data.json at all, and no GitHub summary could
    ever converge for it (the bug this split fixes)."""

    def setUp(self):
        self.module = _load_release_notes_data_module()

    def _write(self, tmp_dir, data):
        import json as _json

        path = Path(tmp_dir) / "4.150.0.data.json"
        path.write_text(_json.dumps(data))
        return path

    def test_true_only_when_byte_for_byte_identical(self):
        import tempfile

        old = {"format": 4, "version": "4.150.0", "prs": {}, "shipments": []}
        with tempfile.TemporaryDirectory() as tmp_dir:
            path = self._write(tmp_dir, old)
            self.assertTrue(self.module._data_json_unchanged(path, dict(old)))

    def test_a_bare_format_bump_is_NOT_unchanged(self):
        import tempfile

        old = {"format": 3, "version": "4.150.0", "prs": {}}
        new = {"format": 4, "version": "4.150.0", "prs": {}, "shipments": []}
        with tempfile.TemporaryDirectory() as tmp_dir:
            path = self._write(tmp_dir, old)
            self.assertFalse(self.module._data_json_unchanged(path, new))

    def test_a_new_shipment_tag_is_NOT_unchanged(self):
        import tempfile

        old = {
            "format": 4,
            "version": "4.150.0",
            "prs": {},
            "shipments": [{"tag": "v4.150.0-preview.1", "target_sha": "a" * 40}],
        }
        new = {
            "format": 4,
            "version": "4.150.0",
            "prs": {},
            "shipments": [
                {"tag": "v4.150.0-preview.1", "target_sha": "a" * 40},
                {"tag": "v4.150.0-preview.2", "target_sha": "b" * 40},
            ],
        }
        with tempfile.TemporaryDirectory() as tmp_dir:
            path = self._write(tmp_dir, old)
            self.assertFalse(self.module._data_json_unchanged(path, new))

    def test_a_removed_or_altered_shipment_is_NOT_unchanged(self):
        import tempfile

        old = {
            "format": 4,
            "version": "4.150.0",
            "prs": {},
            "shipments": [
                {"tag": "v4.150.0-preview.1", "target_sha": "a" * 40},
                {"tag": "v4.150.0-preview.2", "target_sha": "b" * 40},
            ],
        }
        removed = {
            "format": 4,
            "version": "4.150.0",
            "prs": {},
            "shipments": [{"tag": "v4.150.0-preview.1", "target_sha": "a" * 40}],
        }
        altered = {
            "format": 4,
            "version": "4.150.0",
            "prs": {},
            "shipments": [
                {"tag": "v4.150.0-preview.1", "target_sha": "a" * 40},
                {"tag": "v4.150.0-preview.2", "target_sha": "c" * 40},
            ],
        }
        with tempfile.TemporaryDirectory() as tmp_dir:
            path = self._write(tmp_dir, old)
            self.assertFalse(self.module._data_json_unchanged(path, removed))
        with tempfile.TemporaryDirectory() as tmp_dir:
            path = self._write(tmp_dir, old)
            self.assertFalse(self.module._data_json_unchanged(path, altered))

    def test_a_missing_data_json_is_changed(self):
        module = self.module
        self.assertFalse(
            module._data_json_unchanged(Path("/nonexistent/4.150.0.data.json"), {})
        )


class PreserveHistoricalGitHubUrlsTests(unittest.TestCase):
    def setUp(self):
        self.module = _load_release_notes_data_module()
        self.module.configure_repository_identity(
            repository="dotnet/SkiaSharp"
        )

    def test_preserves_existing_urls_when_only_the_owner_changes(self):
        existing = {
            "banner": {
                "github_release_url": (
                    "https://github.com/mono/SkiaSharp/releases/tag/v4.151.0"
                )
            },
            "prs": {
                "42": {"url": "https://github.com/mono/SkiaSharp/pull/42"}
            },
            "shipments": [
                {
                    "tag": "v4.151.0",
                    "changelog_url": (
                        "https://github.com/mono/SkiaSharp/compare/"
                        "v4.151.0-rc.1...v4.151.0"
                    ),
                }
            ],
        }
        generated = {
            "banner": {
                "github_release_url": (
                    "https://github.com/dotnet/SkiaSharp/releases/tag/v4.151.0"
                )
            },
            "prs": {
                "42": {"url": "https://github.com/dotnet/SkiaSharp/pull/42"}
            },
            "shipments": [
                {
                    "tag": "v4.151.0",
                    "changelog_url": (
                        "https://github.com/dotnet/SkiaSharp/compare/"
                        "v4.151.0-rc.1...v4.151.0"
                    ),
                }
            ],
        }

        actual = self.module.preserve_historical_github_urls(existing, generated)

        self.assertEqual(actual, existing)

    def test_matches_preview_and_shipment_records_by_stable_identity(self):
        existing = {
            "shipments": [
                {
                    "tag": "v4.151.0",
                    "changelog_url": (
                        "https://github.com/mono/SkiaSharp/compare/"
                        "v4.151.0-rc.1...v4.151.0"
                    ),
                }
            ],
            "previews": [
                {
                    "key": "preview.1",
                    "release_url": (
                        "https://github.com/mono/SkiaSharp/releases/tag/"
                        "v4.151.0-preview.1"
                    ),
                }
            ],
        }
        generated = {
            "shipments": [
                {
                    "tag": "v4.151.0",
                    "changelog_url": (
                        "https://github.com/dotnet/SkiaSharp/compare/"
                        "v4.151.0-rc.1...v4.151.0"
                    ),
                },
                {
                    "tag": "v4.151.1",
                    "changelog_url": (
                        "https://github.com/dotnet/SkiaSharp/compare/"
                        "v4.151.0...v4.151.1"
                    ),
                },
            ],
            "previews": [
                {
                    "key": "rc.1",
                    "release_url": (
                        "https://github.com/dotnet/SkiaSharp/releases/tag/"
                        "v4.151.0-rc.1"
                    ),
                },
                {
                    "key": "preview.1",
                    "release_url": (
                        "https://github.com/dotnet/SkiaSharp/releases/tag/"
                        "v4.151.0-preview.1"
                    ),
                },
            ],
        }

        actual = self.module.preserve_historical_github_urls(existing, generated)

        self.assertIn("mono/SkiaSharp", actual["shipments"][0]["changelog_url"])
        self.assertIn("dotnet/SkiaSharp", actual["shipments"][1]["changelog_url"])
        self.assertIn("dotnet/SkiaSharp", actual["previews"][0]["release_url"])
        self.assertIn("mono/SkiaSharp", actual["previews"][1]["release_url"])

    def test_does_not_preserve_a_changed_endpoint_or_different_repository(self):
        preserve = self.module.preserve_historical_github_urls

        self.assertEqual(
            preserve(
                "https://github.com/mono/SkiaSharp/pull/41",
                "https://github.com/dotnet/SkiaSharp/pull/42",
            ),
            "https://github.com/dotnet/SkiaSharp/pull/42",
        )
        self.assertEqual(
            preserve(
                "https://github.com/mono/Other/releases/tag/v1.0.0",
                "https://github.com/dotnet/Other/releases/tag/v1.0.0",
            ),
            "https://github.com/dotnet/Other/releases/tag/v1.0.0",
        )
        self.assertEqual(
            preserve(
                "https://github.com/attacker/SkiaSharp/pull/42",
                "https://github.com/dotnet/SkiaSharp/pull/42",
            ),
            "https://github.com/dotnet/SkiaSharp/pull/42",
        )


class RepositoryIdentityIntegrationTests(unittest.TestCase):
    def setUp(self):
        self.module = _load_release_notes_data_module()

    @staticmethod
    def _write_identity_fixture(root, *, repository, skia, site):
        import json

        (root / ".gitmodules").write_text(
            '[submodule "externals/skia"]\n'
            "  path = externals/skia\n"
            "  url = https://github.com/{}.git\n"
            '[submodule "docs"]\n'
            "  path = docs\n"
            "  url = https://github.com/dotnet/SkiaSharp-API-docs\n".format(skia),
            encoding="utf-8",
        )
        config = root / "repository-identity.json"
        config.write_text(
            json.dumps(
                {
                    "canonicalRepositoryId": 52293126,
                    "offlineRepository": repository,
                    "upstreamSkiaRepository": "google/skia",
                    "publicSiteBaseUrl": site,
                    "skiaRepositoryKey": "github-52292286",
                    "legacySkiaRepositoryKeys": ["mono-skia"],
                }
            ),
            encoding="utf-8",
        )
        return config

    def test_current_mono_identity_comes_from_the_shared_foundation(self):
        import tempfile

        with tempfile.TemporaryDirectory() as tmp_dir:
            root = Path(tmp_dir)
            config = self._write_identity_fixture(
                root,
                repository="mono/SkiaSharp",
                skia="mono/skia",
                site="https://mono.github.io/SkiaSharp",
            )
            identity = self.module.configure_repository_identity(
                root=root,
                environ={},
                config_path=config,
            )

        self.assertEqual(self.module.REPO, "mono/SkiaSharp")
        self.assertEqual(
            self.module.SKIA_REMOTE_URL, "https://github.com/mono/skia.git"
        )
        self.assertEqual(
            identity["publicSiteBaseUrl"], "https://mono.github.io/SkiaSharp"
        )
        self.assertEqual(
            common.PUBLIC_SITE_BASE_URL, common.IDENTITY["publicSiteBaseUrl"]
        )

    def test_simulated_transfer_flips_future_repository_skia_and_release_links(self):
        import tempfile

        with tempfile.TemporaryDirectory() as tmp_dir:
            root = Path(tmp_dir)
            config = self._write_identity_fixture(
                root,
                repository="dotnet/SkiaSharp",
                skia="dotnet/skia",
                site="https://skiasharp.example.test",
            )

            identity = self.module.configure_repository_identity(
                root=root,
                environ={},
                config_path=config,
            )
            shipments = self.module._release_shipments.collect_shipments(
                "9.9.9",
                ["v9.8.0", "v9.9.9"],
                tag_date=lambda tag: "2026-09-05",
                target_sha=lambda tag: "a" * 40,
                prs_between=lambda previous, tag: [],
                repository=self.module.REPO,
            )
            data = self.module.build_data_json(
                [],
                {
                    "version": "9.9.9",
                    "status": "stable",
                    "shipments": shipments,
                },
            )

        self.assertEqual(identity["publicSiteBaseUrl"], "https://skiasharp.example.test")
        self.assertEqual(self.module.REPO, "dotnet/SkiaSharp")
        self.assertEqual(
            self.module.SKIA_REMOTE_URL, "https://github.com/dotnet/skia.git"
        )
        self.assertEqual(self.module._release_common.REPO, "dotnet/SkiaSharp")
        self.assertEqual(
            data["banner"]["github_release_url"],
            "https://github.com/dotnet/SkiaSharp/releases/tag/v9.9.9",
        )
        self.assertEqual(
            data["shipments"][0]["changelog_url"],
            "https://github.com/dotnet/SkiaSharp/compare/v9.8.0...v9.9.9",
        )

    def test_transferred_skia_and_historical_mono_skia_are_both_parsed(self):
        self.module.SKIA_PR_PATTERNS = self.module._skia_pr_patterns("dotnet/skia")
        bodies = (
            "Related skia PR: https://github.com/dotnet/skia/pull/42",
            "Companion PR: https://github.com/mono/skia/pull/41",
            "Backport mono/skia#40",
        )

        self.assertEqual(
            [
                next(
                    int(match.group(1))
                    for pattern in self.module.SKIA_PR_PATTERNS
                    if (match := pattern.search(body))
                )
                for body in bodies
            ],
            [42, 41, 40],
        )

    def test_destination_context_drives_renderer_fallback_links(self):
        with mock.patch.dict(
            "os.environ", {"GITHUB_REPOSITORY": "dotnet/SkiaSharp"}
        ):
            renderer = _load_release_notes_renderer_module()

        self.assertEqual(
            renderer.pr_links([42], {"prs": {}}),
            "[#42](https://github.com/dotnet/SkiaSharp/pull/42)",
        )
        self.assertEqual(
            renderer.pr_links(
                [41],
                {
                    "prs": {
                        "41": {
                            "url": "https://github.com/mono/SkiaSharp/pull/41"
                        }
                    }
                },
            ),
            "[#41](https://github.com/mono/SkiaSharp/pull/41)",
        )

    def test_destination_context_drives_future_preview_compare_links(self):
        self.module.configure_repository_identity(
            repository="dotnet/SkiaSharp"
        )

        def fake_run(command, check=True):
            if command[:4] == ["git", "tag", "-l", "v*"]:
                return "v4.150.0\nv4.151.0-preview.1.1\n"
            if command[:3] == ["git", "log", "-1"]:
                return "2026-09-05"
            return ""

        with mock.patch.object(self.module, "run", side_effect=fake_run):
            [preview] = self.module.collect_preview_milestones(
                "4.151.0", "4.150.0"
            )

        self.assertEqual(
            preview["compare_url"],
            "https://github.com/dotnet/SkiaSharp/compare/"
            "v4.150.0...v4.151.0-preview.1.1",
        )

    def test_malformed_runtime_repository_fails_loudly(self):
        with mock.patch.dict("os.environ", {"GITHUB_REPOSITORY": "dotnet"}):
            with self.assertRaises(IdentityError):
                _load_release_notes_data_module()

    def test_malformed_transfer_config_fails_before_state_changes(self):
        import json
        import tempfile

        repository_before = self.module.REPO
        with tempfile.TemporaryDirectory() as tmp_dir:
            root = Path(tmp_dir)
            config = self._write_identity_fixture(
                root,
                repository="dotnet/SkiaSharp",
                skia="dotnet/skia",
                site="https://skiasharp.example.test",
            )
            value = json.loads(config.read_text(encoding="utf-8"))
            value["publicSiteBaseUrl"] = "not-a-url"
            config.write_text(json.dumps(value), encoding="utf-8")

            with self.assertRaises(IdentityError):
                self.module.configure_repository_identity(
                    root=root,
                    environ={},
                    config_path=config,
                )

        self.assertEqual(self.module.REPO, repository_before)


class ClassifyDataWriteTests(unittest.TestCase):
    """_classify_data_write() is the pure decision table _write_page uses.
    This directly proves the fix: a shipment-only change (website content
    unchanged, but the data.json dict is not fully unchanged) is ALWAYS
    written, regardless of --force, while never discarding prose and never
    being added to files-to-polish."""

    def setUp(self):
        self.module = _load_release_notes_data_module()

    def test_fully_unchanged_without_force_skips_entirely(self):
        self.assertIsNone(
            self.module._classify_data_write(
                fully_unchanged=True, website_content_unchanged=True, force=False
            )
        )

    def test_fully_unchanged_with_force_writes_but_preserves_prose(self):
        # Preserves the exact pre-shipments --force behavior: still returned
        # for polish (matching historical force semantics), but prose is
        # untouched since nothing about the page's content changed.
        action = self.module._classify_data_write(
            fully_unchanged=True, website_content_unchanged=True, force=True
        )
        self.assertEqual(action, {"delete_prose": False, "add_to_polish": True})

    def test_website_content_change_always_deletes_prose_and_polishes(self):
        for force in (False, True):
            with self.subTest(force=force):
                action = self.module._classify_data_write(
                    fully_unchanged=False,
                    website_content_unchanged=False,
                    force=force,
                )
                self.assertEqual(action, {"delete_prose": True, "add_to_polish": True})

    def test_shipment_only_change_is_written_regardless_of_force(self):
        # THE regression case: website content is unchanged (no PR/roster/
        # preview/link fact moved) but the data.json dict is not fully
        # unchanged (a shipment appeared/changed, or format bumped). This
        # must never be skipped -- previously it silently was, so a newly
        # published preview/RC tag's shipment record never reached
        # data.json and no GitHub summary could ever converge for it.
        for force in (False, True):
            with self.subTest(force=force):
                action = self.module._classify_data_write(
                    fully_unchanged=False,
                    website_content_unchanged=True,
                    force=force,
                )
                self.assertIsNotNone(action, "a shipment-only change must always be written")
                self.assertEqual(action, {"delete_prose": False, "add_to_polish": False})


class WritePageShipmentOnlyChangeRegressionTests(unittest.TestCase):
    """End-to-end (file-system-level, no real git) regression tests
    reproducing the exact bug scenario: a page whose committed data.json
    already exists is regenerated with a NEW/altered shipments list but
    otherwise-identical website content. The write must happen (so a
    subsequent `git diff` / Prepare patch is non-empty -- "has_changes"),
    the committed prose must be preserved, and the page must not appear in
    files-to-polish.

    This exercises the same three functions _write_page itself calls
    (_data_json_unchanged, _website_content_unchanged, _classify_data_write)
    against real temp files, composing them exactly as _write_page does,
    without needing a real git repository/branch checkout.
    """

    def setUp(self):
        self.module = _load_release_notes_data_module()

    def _simulate_write_page(self, data_path, prose_path, new_data, *, force=False):
        """Mirrors _write_page's tail exactly, for a single already-existing
        page, given the freshly-computed ``new_data`` dict."""
        module = self.module
        fully_unchanged = module._data_json_unchanged(data_path, new_data)
        website_content_unchanged = module._website_content_unchanged(data_path, new_data)
        action = module._classify_data_write(fully_unchanged, website_content_unchanged, force)
        if action is None:
            return None
        data_path.write_text(__import__("json").dumps(new_data))
        if action["delete_prose"] and prose_path.exists():
            prose_path.unlink()
        return "polish" if action["add_to_polish"] else None

    def test_a_new_preview_tag_is_written_prose_preserved_no_polish(self):
        import json as _json
        import tempfile

        old_data = {
            "format": 3,
            "version": "4.150.0",
            "prs": {"1": {"title": "Existing PR"}},
        }
        new_data = {
            "format": 4,
            "version": "4.150.0",
            "prs": {"1": {"title": "Existing PR"}},
            "shipments": [{"tag": "v4.150.0-preview.1", "target_sha": "a" * 40}],
        }
        with tempfile.TemporaryDirectory() as tmp_dir:
            data_path = Path(tmp_dir) / "4.150.0.data.json"
            prose_path = Path(tmp_dir) / "4.150.0.prose.json"
            data_path.write_text(_json.dumps(old_data))
            prose_path.write_text(_json.dumps({"theme": "Reviewed prose"}))

            result = self._simulate_write_page(data_path, prose_path, new_data)

            # "has_changes": the file on disk actually changed.
            self.assertEqual(_json.loads(data_path.read_text()), new_data)
            self.assertNotEqual(_json.loads(data_path.read_text()), old_data)
            # Prose preserved.
            self.assertTrue(prose_path.exists())
            self.assertEqual(
                _json.loads(prose_path.read_text()), {"theme": "Reviewed prose"}
            )
            # Not added to files-to-polish.
            self.assertIsNone(result)

    def test_a_removed_shipment_tag_is_also_written_and_not_polished(self):
        import json as _json
        import tempfile

        old_data = {
            "format": 4,
            "version": "4.150.0",
            "prs": {},
            "shipments": [
                {"tag": "v4.150.0-preview.1", "target_sha": "a" * 40},
                {"tag": "v4.150.0-preview.2", "target_sha": "b" * 40},
            ],
        }
        new_data = {
            "format": 4,
            "version": "4.150.0",
            "prs": {},
            "shipments": [{"tag": "v4.150.0-preview.1", "target_sha": "a" * 40}],
        }
        with tempfile.TemporaryDirectory() as tmp_dir:
            data_path = Path(tmp_dir) / "4.150.0.data.json"
            prose_path = Path(tmp_dir) / "4.150.0.prose.json"
            data_path.write_text(_json.dumps(old_data))
            prose_path.write_text(_json.dumps({"theme": "Reviewed prose"}))

            result = self._simulate_write_page(data_path, prose_path, new_data)

            self.assertEqual(_json.loads(data_path.read_text()), new_data)
            self.assertTrue(prose_path.exists())
            self.assertIsNone(result)

    def test_a_website_pr_fact_change_still_deletes_prose_and_polishes(self):
        import json as _json
        import tempfile

        old_data = {
            "format": 4,
            "version": "4.150.0",
            "prs": {"1": {"title": "Old title"}},
            "shipments": [{"tag": "v4.150.0-preview.1", "target_sha": "a" * 40}],
        }
        new_data = {
            "format": 4,
            "version": "4.150.0",
            "prs": {"1": {"title": "Old title"}, "2": {"title": "New PR merged"}},
            "shipments": [{"tag": "v4.150.0-preview.1", "target_sha": "a" * 40}],
        }
        with tempfile.TemporaryDirectory() as tmp_dir:
            data_path = Path(tmp_dir) / "4.150.0.data.json"
            prose_path = Path(tmp_dir) / "4.150.0.prose.json"
            data_path.write_text(_json.dumps(old_data))
            prose_path.write_text(_json.dumps({"theme": "Stale prose"}))

            result = self._simulate_write_page(data_path, prose_path, new_data)

            self.assertEqual(_json.loads(data_path.read_text()), new_data)
            # Prose discarded -- this page needs fresh prose.
            self.assertFalse(prose_path.exists())
            self.assertEqual(result, "polish")

    def test_truly_unchanged_page_is_skipped_without_force(self):
        import json as _json
        import tempfile

        data = {
            "format": 4,
            "version": "4.150.0",
            "prs": {},
            "shipments": [{"tag": "v4.150.0-preview.1", "target_sha": "a" * 40}],
        }
        with tempfile.TemporaryDirectory() as tmp_dir:
            data_path = Path(tmp_dir) / "4.150.0.data.json"
            prose_path = Path(tmp_dir) / "4.150.0.prose.json"
            data_path.write_text(_json.dumps(data))
            prose_path.write_text(_json.dumps({"theme": "Reviewed prose"}))
            before = data_path.stat().st_mtime_ns

            module = self.module
            fully_unchanged = module._data_json_unchanged(data_path, dict(data))
            website_content_unchanged = module._website_content_unchanged(data_path, dict(data))
            action = module._classify_data_write(
                fully_unchanged, website_content_unchanged, force=False
            )

            self.assertIsNone(action)
            # Confirm _write_page would never even reach the write step: the
            # file's mtime is untouched (no write happened).
            self.assertEqual(data_path.stat().st_mtime_ns, before)
            self.assertTrue(prose_path.exists())


if __name__ == "__main__":
    unittest.main()
