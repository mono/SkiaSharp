from __future__ import annotations

import importlib.util
import sys
import unittest
from pathlib import Path
from unittest import mock


_DOCS_DIR = Path(__file__).resolve().parents[2]
if str(_DOCS_DIR) not in sys.path:
    sys.path.insert(0, str(_DOCS_DIR))


def _load_release_notes_data_module():
    path = _DOCS_DIR / "release-notes-data.py"
    spec = importlib.util.spec_from_file_location("_rn_rollup_tests", str(path))
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    return module


class StableDelimitedRollupTests(unittest.TestCase):
    def setUp(self):
        self.module = _load_release_notes_data_module()
        self.branches = [
            "release/4.151.2",
            "release/4.152.0",
            "release/4.153.0-preview.1",
            "release/4.154.0-preview.1",
        ]

    def _diff_range(self, branch, stable_versions):
        with (
            mock.patch.object(
                self.module,
                "list_remote_release_branches",
                return_value=self.branches,
            ),
            mock.patch.object(
                self.module,
                "_version_has_stable_tag",
                side_effect=lambda version: version in stable_versions,
            ),
            mock.patch.object(
                self.module,
                "_versions_config_lookup",
                return_value=None,
            ),
        ):
            return self.module.determine_diff_range(branch)

    def test_consecutive_preview_only_lines_remain_separate(self):
        stable = {"4.151.2"}

        cases = [
            ("release/4.152.0", "origin/release/4.151.2"),
            ("release/4.153.0-preview.1", "origin/release/4.152.0"),
            ("release/4.154.0-preview.1", "origin/release/4.153.0-preview.1"),
        ]
        for branch, expected_base in cases:
            with self.subTest(branch=branch):
                from_ref, _, _ = self._diff_range(branch, stable)
                self.assertEqual(from_ref, expected_base)

    def test_stable_153_rolls_up_152_and_153(self):
        self.branches.append("release/4.153.0")

        from_ref, _, _ = self._diff_range(
            "release/4.153.0", {"4.151.2", "4.153.0"}
        )

        self.assertEqual(from_ref, "origin/release/4.151.2")

    def test_stable_154_rolls_up_all_lines_since_last_stable(self):
        self.branches.append("release/4.154.0")

        from_ref, _, _ = self._diff_range(
            "release/4.154.0", {"4.151.2", "4.154.0"}
        )

        self.assertEqual(from_ref, "origin/release/4.151.2")

    def test_stable_152_keeps_later_preview_lines_separate(self):
        stable = {"4.151.2", "4.152.0"}

        from_153, _, _ = self._diff_range(
            "release/4.153.0-preview.1", stable
        )
        from_154, _, _ = self._diff_range(
            "release/4.154.0-preview.1", stable
        )

        self.assertEqual(from_153, "origin/release/4.152.0")
        self.assertEqual(from_154, "origin/release/4.153.0-preview.1")

    def test_stable_search_does_not_treat_exact_branch_as_stable(self):
        branches = [
            "release/4.152.0",
            "release/4.153.0-preview.1",
        ]

        with (
            mock.patch.object(
                self.module,
                "_version_has_stable_tag",
                return_value=False,
            ),
            mock.patch.object(
                self.module,
                "_versions_config_lookup",
                return_value=None,
            ),
        ):
            base = self.module.find_previous_stable_base(
                branches, 4, 154, 0
            )

        self.assertIsNone(base)

    def test_four_segment_stable_skips_preview_only_plain_version(self):
        branches = [
            "release/1.68.0",
            "release/1.68.1",
            "release/1.68.1.1",
        ]

        with (
            mock.patch.object(
                self.module,
                "_version_has_stable_tag",
                side_effect=lambda version: version in {"1.68.0", "1.68.1.1"},
            ),
            mock.patch.object(
                self.module,
                "_versions_config_lookup",
                return_value=None,
            ),
        ):
            base = self.module.find_previous_stable_base(
                branches, 1, 68, 1, 1
            )

        self.assertEqual(base, "release/1.68.0")

    def test_four_segment_stable_uses_stable_plain_version_rc_branch(self):
        branches = [
            "release/1.68.0",
            "release/1.68.1-rc.1",
            "release/1.68.1.1",
        ]

        with (
            mock.patch.object(
                self.module,
                "_version_has_stable_tag",
                side_effect=lambda version: version in {
                    "1.68.0",
                    "1.68.1",
                    "1.68.1.1",
                },
            ),
            mock.patch.object(
                self.module,
                "_versions_config_lookup",
                return_value=None,
            ),
        ):
            base = self.module.find_previous_stable_base(
                branches, 1, 68, 1, 1
            )

        self.assertEqual(base, "release/1.68.1-rc.1")


if __name__ == "__main__":
    unittest.main()
