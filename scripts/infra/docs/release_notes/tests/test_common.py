from __future__ import annotations

import sys
import unittest
from pathlib import Path

# scripts/infra/docs/release_notes/tests/ -> parents[2] == scripts/infra/docs
_DOCS_DIR = Path(__file__).resolve().parents[2]
if str(_DOCS_DIR) not in sys.path:
    sys.path.insert(0, str(_DOCS_DIR))

from release_notes import common


def _load_release_notes_data_module():
    import importlib.util

    gen_path = _DOCS_DIR / "release-notes-data.py"
    spec = importlib.util.spec_from_file_location("_rn_data_format_check", str(gen_path))
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
        self.assertEqual(parsed.build, 1)
        self.assertEqual(parsed.label, "Preview 1 (Build 1)")
        self.assertEqual(parsed.public_version, "4.150.0-preview.1.1")

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


class ImportReleaseGithubTests(unittest.TestCase):
    def test_resolves_the_same_managed_markers(self):
        gh = common.import_release_github()
        self.assertEqual(gh.SUMMARY_START_MARKER, "<!-- SKIASHARP:RELEASE-SUMMARY:START -->")
        self.assertEqual(gh.SUMMARY_END_MARKER, "<!-- SKIASHARP:RELEASE-SUMMARY:END -->")
        self.assertEqual(
            gh.GENERATED_START_MARKER, "<!-- SKIASHARP:GITHUB-GENERATED-NOTES:START -->"
        )
        self.assertEqual(
            gh.GENERATED_END_MARKER, "<!-- SKIASHARP:GITHUB-GENERATED-NOTES:END -->"
        )

    def test_is_idempotent_and_cached_by_sys_modules(self):
        first = common.import_release_github()
        second = common.import_release_github()
        self.assertIs(first, second)


class DataFormatSyncTests(unittest.TestCase):
    """release-notes-data.py's format bump and this package's expectation of it
    must never drift apart -- see common.DATA_FORMAT's docstring."""

    def test_matches_release_notes_data_format_version(self):
        module = _load_release_notes_data_module()
        self.assertEqual(module._DATA_JSON_FORMAT_VERSION, common.DATA_FORMAT)


class DataJsonUnchangedIgnoresFormatAndShipmentsTests(unittest.TestCase):
    """A format bump (3 -> 4) or a shipments-only change must never, by
    itself, make _data_json_unchanged() report "changed" -- that would
    discard every historical page's reviewed prose the next time Prepare
    runs, which is exactly the mass rewrite this feature must not cause."""

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
            self.assertTrue(self.module._data_json_unchanged(path, new))

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
            self.assertTrue(self.module._data_json_unchanged(path, new))

    def test_a_genuine_content_change_is_still_detected(self):
        import tempfile

        old = {"format": 3, "version": "4.150.0", "prs": {}}
        new = {"format": 4, "version": "4.150.0", "prs": {"1": {"title": "New PR"}}, "shipments": []}
        with tempfile.TemporaryDirectory() as tmp_dir:
            path = self._write(tmp_dir, old)
            self.assertFalse(self.module._data_json_unchanged(path, new))

    def test_a_missing_data_json_is_changed(self):
        module = self.module
        self.assertFalse(
            module._data_json_unchanged(Path("/nonexistent/4.150.0.data.json"), {})
        )


if __name__ == "__main__":
    unittest.main()
