from __future__ import annotations

import sys
import unittest
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent.parent))

import release_model as model
from release_common import PlanError


class ParseReleaseVersionTests(unittest.TestCase):
    def test_stable(self):
        version = model.parse_release_version("3.119.0")
        self.assertEqual(version.numeric, "3.119.0")
        self.assertIsNone(version.channel)
        self.assertTrue(version.stable)
        self.assertFalse(version.is_hotfix)
        self.assertEqual(version.label, "stable")
        self.assertEqual(version.release_type, "stable")
        self.assertEqual(version.line, "3.119")
        self.assertEqual(version.integration_branch, "release/3.119.x")
        self.assertEqual(version.release_branch, "release/3.119.0")
        self.assertEqual(version.tag, "v3.119.0")
        self.assertEqual(version.title, "Version 3.119.0")

    def test_preview(self):
        version = model.parse_release_version("3.119.0-preview.1")
        self.assertEqual(version.channel, "preview")
        self.assertEqual(version.iteration, 1)
        self.assertFalse(version.stable)
        self.assertEqual(version.label, "preview.1")
        self.assertEqual(version.release_type, "preview")
        self.assertEqual(version.title, "Version 3.119.0 (Preview 1)")

    def test_rc(self):
        version = model.parse_release_version("3.119.0-rc.2")
        self.assertEqual(version.channel, "rc")
        self.assertEqual(version.label, "rc.2")
        self.assertEqual(version.release_type, "rc")
        self.assertEqual(version.title, "Version 3.119.0 (RC 2)")

    def test_hotfix_stable(self):
        version = model.parse_release_version("3.119.0.1")
        self.assertTrue(version.is_hotfix)
        self.assertTrue(version.stable)
        self.assertEqual(version.release_type, "hotfix stable")
        self.assertEqual(version.parts, (3, 119, 0, 1))

    def test_hotfix_preview(self):
        version = model.parse_release_version("3.119.0.1-preview.1")
        self.assertTrue(version.is_hotfix)
        self.assertEqual(version.release_type, "hotfix preview")

    def test_rejects_invalid_grammar(self):
        for bad in ["3.119", "v3.119.0", "3.119.0-beta.1", "3.119.0.1.2", "3.119.0-preview"]:
            with self.assertRaises(PlanError, msg=bad):
                model.parse_release_version(bad)

    def test_rejects_zero_iteration(self):
        with self.assertRaises(PlanError):
            model.parse_release_version("3.119.0-preview.0")

    def test_sort_key_orders_channels_before_stable(self):
        preview = model.parse_release_version("3.119.0-preview.1")
        rc = model.parse_release_version("3.119.0-rc.1")
        stable = model.parse_release_version("3.119.0")
        self.assertLess(preview.sort_key, rc.sort_key)
        self.assertLess(rc.sort_key, stable.sort_key)


class ReleaseBranchAndTagTests(unittest.TestCase):
    def test_parse_release_branch(self):
        version = model.parse_release_branch("release/3.119.0-preview.1")
        self.assertEqual(version.raw, "3.119.0-preview.1")

    def test_parse_release_branch_rejects_missing_prefix(self):
        with self.assertRaises(PlanError):
            model.parse_release_branch("3.119.0-preview.1")

    def test_parse_release_tag(self):
        version = model.parse_release_tag("v3.119.0-rc.2")
        self.assertEqual(version.raw, "3.119.0-rc.2")

    def test_parse_release_tag_rejects_missing_prefix(self):
        with self.assertRaises(PlanError):
            model.parse_release_tag("3.119.0")


class IntegrationBranchTests(unittest.TestCase):
    def test_accepts_main(self):
        self.assertEqual(model.normalize_integration_branch("main"), "main")

    def test_accepts_maintenance_branch(self):
        self.assertEqual(
            model.normalize_integration_branch("release/3.119.x"), "release/3.119.x"
        )

    def test_strips_known_prefixes(self):
        self.assertEqual(
            model.normalize_integration_branch("refs/remotes/origin/main"), "main"
        )
        self.assertEqual(
            model.normalize_integration_branch("origin/release/3.119.x"), "release/3.119.x"
        )

    def test_rejects_exact_release_branch(self):
        with self.assertRaises(PlanError):
            model.normalize_integration_branch("release/3.119.0-preview.1")

    def test_rejects_pr_ref(self):
        with self.assertRaises(PlanError):
            model.normalize_integration_branch("refs/pull/123/head")


class HarfBuzzIncrementTests(unittest.TestCase):
    def test_three_part_gains_a_revision(self):
        self.assertEqual(model.increment_harfbuzz("1.8.8"), "1.8.8.1")

    def test_four_part_increments_last(self):
        self.assertEqual(model.increment_harfbuzz("1.8.8.1"), "1.8.8.2")
        self.assertEqual(model.increment_harfbuzz("14.2.1.200"), "14.2.1.201")

    def test_rejects_non_numeric(self):
        with self.assertRaises(PlanError):
            model.increment_harfbuzz("1.8.x")


class CalculateNextVersionsTests(unittest.TestCase):
    def test_bumps_patch_and_harfbuzz(self):
        skia, harfbuzz = model.calculate_next_versions("3.119.0", "1.8.8.1")
        self.assertEqual(skia, "3.119.1")
        self.assertEqual(harfbuzz, "1.8.8.2")

    def test_rejects_hotfix_numeric(self):
        with self.assertRaises(PlanError):
            model.calculate_next_versions("3.119.0.1", "1.8.8.1")


class PublicVersionCompositionTests(unittest.TestCase):
    def test_stable_requires_bare_equality(self):
        version = model.parse_release_version("3.119.0")
        base, build = version.validate_public_version("3.119.0")
        self.assertEqual(base, "3.119.0")
        self.assertIsNone(build)

    def test_stable_rejects_suffixed_version(self):
        version = model.parse_release_version("3.119.0")
        with self.assertRaises(PlanError):
            version.validate_public_version("3.119.0-preview.1.12345.1")

    def test_preview_accepts_bare_build_number(self):
        version = model.parse_release_version("3.119.0-preview.1")
        base, build = version.validate_public_version("3.119.0-preview.1.42")
        self.assertEqual(base, "3.119.0")
        self.assertEqual(build, "42")

    def test_preview_accepts_five_digit_date_prefixed_build(self):
        version = model.parse_release_version("3.119.0-preview.1")
        base, build = version.validate_public_version("3.119.0-preview.1.12345.7")
        self.assertEqual(build, "12345.7")

    def test_preview_accepts_eight_digit_date_prefixed_build(self):
        version = model.parse_release_version("3.119.0-preview.1")
        base, build = version.validate_public_version("3.119.0-preview.1.20250131.3")
        self.assertEqual(build, "20250131.3")

    def test_rc_rejects_wrong_base(self):
        version = model.parse_release_version("3.119.0-rc.1")
        with self.assertRaises(PlanError):
            version.validate_public_version("3.119.0-rc.2.5")

    def test_rejects_malformed_build_revision(self):
        version = model.parse_release_version("3.119.0-preview.1")
        for bad in ["3.119.0-preview.1.abc", "3.119.0-preview.1.", "3.119.0-preview.1.1.2"]:
            with self.assertRaises(PlanError, msg=bad):
                version.validate_public_version(bad)

    def test_four_part_hotfix_stable_composition(self):
        version = model.parse_release_version("3.119.0.1")
        base, build = version.validate_public_version("3.119.0.1")
        self.assertEqual(base, "3.119.0.1")
        self.assertIsNone(build)

    def test_four_part_hotfix_preview_composition(self):
        version = model.parse_release_version("3.119.0.1-preview.1")
        base, build = version.validate_public_version("3.119.0.1-preview.1.9")
        self.assertEqual(base, "3.119.0.1")
        self.assertEqual(build, "9")

    def test_compose_public_version_matches_validate(self):
        composed = model.compose_public_version("3.119.0", "preview.1", "42")
        version = model.parse_release_version("3.119.0-preview.1")
        base, build = version.validate_public_version(composed)
        self.assertEqual(base, "3.119.0")
        self.assertEqual(build, "42")

    def test_compose_public_version_rejects_stable_label(self):
        with self.assertRaises(PlanError):
            model.compose_public_version("3.119.0", "stable", "1")


class BuildRevisionGrammarTests(unittest.TestCase):
    def test_accepts_bare_number(self):
        self.assertIsNotNone(model.BUILD_REVISION_RE.fullmatch("5"))

    def test_accepts_five_digit_prefixed(self):
        self.assertIsNotNone(model.BUILD_REVISION_RE.fullmatch("12345.7"))

    def test_accepts_eight_digit_prefixed(self):
        self.assertIsNotNone(model.BUILD_REVISION_RE.fullmatch("20250131.3"))

    def test_rejects_wrong_digit_counts(self):
        for bad in ["1234.5", "123456.7", "1234567.8", "abc.1", "1.2.3"]:
            self.assertIsNone(model.BUILD_REVISION_RE.fullmatch(bad), bad)


if __name__ == "__main__":
    unittest.main()
