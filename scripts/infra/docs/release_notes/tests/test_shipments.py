from __future__ import annotations

import sys
import unittest
from pathlib import Path
from unittest import mock

_DOCS_DIR = Path(__file__).resolve().parents[2]
if str(_DOCS_DIR) not in sys.path:
    sys.path.insert(0, str(_DOCS_DIR))

from release_notes import common, shipments


# A realistic slice of this repository's actual tag history spanning a
# rolled-up preview line (4.150.x), its rc, its stable, its patch, and the
# start of the next line's preview -- covering all four shipment channels
# (preview, rc, stable, hotfix-style patch) in one fixture.
SAMPLE_TAGS = [
    "v4.150.0-preview.1.1",
    "v4.150.0-preview.2.1",
    "v4.150.0-rc.1.1",
    "v4.150.0",
    "v4.150.1",
    "v4.150.2",
    "v4.151.0-preview.1.1",
    # Decorative/legacy tags that must never become shipments.
    "v4.150.0-gpu1",
    "v1.49.2.1-beta",
]


def _fake_prs_between(counts):
    """Build a prs_between callable from {(from_tag, to_tag): [pr_numbers]}."""

    def _prs_between(from_tag, to_tag):
        numbers = counts.get((from_tag, to_tag), [])
        return [{"number": n} for n in numbers]

    return _prs_between


class CollectShipmentsTests(unittest.TestCase):
    def setUp(self):
        self.dates = {
            "v4.150.0-preview.1.1": "2026-01-01",
            "v4.150.0-preview.2.1": "2026-01-08",
            "v4.150.0-rc.1.1": "2026-01-15",
            "v4.150.0": "2026-01-22",
            "v4.150.1": "2026-01-29",
            "v4.150.2": "2026-02-05",
            "v4.151.0-preview.1.1": "2026-02-12",
        }
        self.shas = {tag: "{:040x}".format(index) for index, tag in enumerate(self.dates, 1)}
        self.prs = _fake_prs_between({
            (None, "v4.150.0-preview.1.1"): [1, 2],
            ("v4.150.0-preview.1.1", "v4.150.0-preview.2.1"): [3],
            ("v4.150.0-preview.2.1", "v4.150.0-rc.1.1"): [],
            ("v4.150.0-rc.1.1", "v4.150.0"): [4],
            ("v4.150.0", "v4.150.1"): [5],
            ("v4.150.1", "v4.150.2"): [6, 7],
            ("v4.150.2", "v4.151.0-preview.1.1"): [8],
        })

    def _collect(self, page_version):
        return shipments.collect_shipments(
            page_version,
            SAMPLE_TAGS,
            tag_date=lambda tag: self.dates.get(tag, ""),
            target_sha=lambda tag: self.shas.get(tag, "0" * 40),
            prs_between=self.prs,
        )

    def test_selects_only_tags_matching_the_page_core(self):
        result = self._collect("4.150.0")
        tags = [item["tag"] for item in result]
        self.assertEqual(
            tags,
            [
                "v4.150.0-preview.1.1",
                "v4.150.0-preview.2.1",
                "v4.150.0-rc.1.1",
                "v4.150.0",
            ],
        )

    def test_excludes_patch_releases_of_a_different_core(self):
        # 4.150.1 and 4.150.2 are their own pages (their own core), not part
        # of the 4.150.0 page's shipments.
        result = self._collect("4.150.0")
        self.assertNotIn("v4.150.1", [item["tag"] for item in result])

    def test_stable_channel_and_label(self):
        result = self._collect("4.150.0")
        stable = next(item for item in result if item["tag"] == "v4.150.0")
        self.assertEqual(stable["channel"], "stable")
        self.assertEqual(stable["label"], "Stable")
        self.assertEqual(stable["public_version"], "4.150.0")
        self.assertEqual(stable["core_version"], "4.150.0")

    def test_preview_channel_and_label(self):
        result = self._collect("4.150.0")
        preview = next(item for item in result if item["tag"] == "v4.150.0-preview.1.1")
        self.assertEqual(preview["channel"], "preview")
        self.assertEqual(preview["label"], "Preview 1 (Build 1)")

    def test_rc_channel_and_label(self):
        result = self._collect("4.150.0")
        rc = next(item for item in result if item["tag"] == "v4.150.0-rc.1.1")
        self.assertEqual(rc["channel"], "rc")
        self.assertEqual(rc["label"], "Release Candidate 1 (Build 1)")

    def test_hotfix_style_patch_release_is_its_own_page(self):
        result = self._collect("4.150.1")
        self.assertEqual(len(result), 1)
        self.assertEqual(result[0]["channel"], "stable")
        self.assertEqual(result[0]["previous_tag"], "v4.150.0")

    def test_previous_tag_is_the_global_predecessor_not_page_scoped(self):
        result = self._collect("4.151.0")
        [only] = result
        # 4.151.0-preview.1's predecessor is 4.150.2 -- the last GLOBAL tag,
        # even though it belongs to a different core version/page.
        self.assertEqual(only["previous_tag"], "v4.150.2")

    def test_earliest_shipment_on_a_page_has_no_previous_tag_when_it_is_first_ever(self):
        result = shipments.collect_shipments(
            "1.0.0",
            ["v1.0.0"],
            tag_date=lambda tag: "2020-01-01",
            target_sha=lambda tag: "a" * 40,
            prs_between=lambda a, b: [],
        )
        [only] = result
        self.assertIsNone(only["previous_tag"])
        self.assertIsNone(only["changelog_url"])

    def test_changelog_url_chains_between_shipments(self):
        result = self._collect("4.150.0")
        rc = next(item for item in result if item["tag"] == "v4.150.0-rc.1.1")
        self.assertEqual(
            rc["changelog_url"],
            "https://github.com/{}/compare/"
            "v4.150.0-preview.2.1...v4.150.0-rc.1.1".format(common.REPO),
        )

    def test_destination_repository_is_used_for_future_exact_shipments(self):
        result = shipments.collect_shipments(
            "4.151.0",
            SAMPLE_TAGS,
            tag_date=lambda tag: self.dates.get(tag, ""),
            target_sha=lambda tag: self.shas.get(tag, "0" * 40),
            prs_between=self.prs,
            repository="dotnet/SkiaSharp",
        )

        [preview] = result
        self.assertEqual(
            preview["changelog_url"],
            "https://github.com/dotnet/SkiaSharp/compare/"
            "v4.150.2...v4.151.0-preview.1.1",
        )

    def test_prs_are_the_exact_delta_since_the_previous_tag(self):
        result = self._collect("4.150.0")
        stable = next(item for item in result if item["tag"] == "v4.150.0")
        self.assertEqual(stable["prs"], [4])
        preview1 = next(item for item in result if item["tag"] == "v4.150.0-preview.1.1")
        self.assertEqual(preview1["prs"], [1, 2])

    def test_decorative_legacy_tags_are_never_shipments(self):
        result = self._collect("4.150.0")
        self.assertNotIn("v4.150.0-gpu1", [item["tag"] for item in result])

    def test_ignores_tags_outside_the_exact_release_grammar_entirely(self):
        result = shipments.collect_shipments(
            "1.49.2",
            ["v1.49.2.1-beta"],
            tag_date=lambda tag: "",
            target_sha=lambda tag: "a" * 40,
            prs_between=lambda a, b: [],
        )
        self.assertEqual(result, [])

    def test_no_shipments_for_a_page_version_with_no_matching_tag(self):
        result = self._collect("9.9.9")
        self.assertEqual(result, [])

    def test_deduplicates_pr_numbers_and_ignores_missing_numbers(self):
        result = shipments.collect_shipments(
            "1.0.0",
            ["v1.0.0"],
            tag_date=lambda tag: "",
            target_sha=lambda tag: "a" * 40,
            prs_between=lambda a, b: [{"number": 5}, {"number": 5}, {}],
        )
        [only] = result
        self.assertEqual(only["prs"], [5])


def _valid_shipment(**overrides):
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
        "prs": [1, 2],
    }
    shipment.update(overrides)
    return shipment


class ValidateShipmentTests(unittest.TestCase):
    def test_a_well_formed_shipment_has_no_errors(self):
        self.assertEqual(shipments.validate_shipment(_valid_shipment()), [])

    def test_rejects_a_non_object(self):
        self.assertEqual(shipments.validate_shipment("not-a-dict"), ["shipment must be a JSON object"])

    def test_rejects_missing_required_fields(self):
        shipment = _valid_shipment()
        del shipment["target_sha"]
        errors = shipments.validate_shipment(shipment)
        self.assertTrue(any("target_sha" in error for error in errors))

    def test_rejects_a_tag_outside_the_exact_grammar(self):
        errors = shipments.validate_shipment(_valid_shipment(tag="v4.151.0-gpu1"))
        self.assertTrue(any("not an exact release tag" in error for error in errors))

    def test_rejects_a_public_version_mismatched_with_the_tag(self):
        errors = shipments.validate_shipment(_valid_shipment(public_version="9.9.9"))
        self.assertTrue(any("public_version" in error for error in errors))

    def test_rejects_core_channel_and_label_mismatched_with_the_tag(self):
        errors = shipments.validate_shipment(
            _valid_shipment(
                core_version="9.9.9",
                channel="stable",
                label="Wrong",
            )
        )
        self.assertTrue(any("core_version" in error for error in errors))
        self.assertTrue(any("channel" in error for error in errors))
        self.assertTrue(any("label" in error for error in errors))

    def test_rejects_an_unknown_channel(self):
        errors = shipments.validate_shipment(_valid_shipment(channel="beta"))
        self.assertTrue(any("channel" in error for error in errors))

    def test_rejects_a_malformed_target_sha(self):
        errors = shipments.validate_shipment(_valid_shipment(target_sha="not-a-sha"))
        self.assertTrue(any("target_sha" in error for error in errors))

    def test_rejects_a_non_integer_pr_list(self):
        errors = shipments.validate_shipment(_valid_shipment(prs=["4128"]))
        self.assertTrue(any("prs" in error for error in errors))

    def test_rejects_a_changelog_url_outside_the_repository(self):
        errors = shipments.validate_shipment(
            _valid_shipment(changelog_url="https://evil.example/compare/a...b")
        )
        self.assertTrue(any("changelog_url" in error for error in errors))

    def test_accepts_preserved_historical_owner_after_transfer(self):
        shipment = _valid_shipment(
            changelog_url=(
                "https://github.com/mono/SkiaSharp/compare/"
                "v4.150.2...v4.151.0-preview.1"
            )
        )
        with mock.patch.object(common, "REPO", "dotnet/SkiaSharp"):
            self.assertEqual([], shipments.validate_shipment(shipment))

    def test_accepts_current_repository_case_insensitively(self):
        shipment = _valid_shipment(
            changelog_url=(
                "https://github.com/dotnet/skiasharp/compare/"
                "v4.150.2...v4.151.0-preview.1"
            )
        )
        with mock.patch.object(common, "REPO", "dotnet/skiasharp"):
            self.assertEqual([], shipments.validate_shipment(shipment))

    def test_rejects_an_unrelated_owner_with_the_same_repository_name(self):
        errors = shipments.validate_shipment(
            _valid_shipment(
                changelog_url=(
                    "https://github.com/attacker/SkiaSharp/compare/"
                    "v4.150.2...v4.151.0-preview.1"
                )
            )
        )
        self.assertTrue(any("changelog_url" in error for error in errors))

    def test_rejects_a_compare_url_for_a_different_repository_name(self):
        errors = shipments.validate_shipment(
            _valid_shipment(
                changelog_url=(
                    "https://github.com/dotnet/NotSkiaSharp/compare/"
                    "v4.150.2...v4.151.0-preview.1"
                )
            )
        )
        self.assertTrue(any("changelog_url" in error for error in errors))

    def test_rejects_a_compare_url_with_a_malformed_owner(self):
        errors = shipments.validate_shipment(
            _valid_shipment(
                changelog_url=(
                    "https://github.com/bad owner/SkiaSharp/compare/"
                    "v4.150.2...v4.151.0-preview.1"
                )
            )
        )
        self.assertTrue(any("changelog_url" in error for error in errors))

    def test_rejects_a_changelog_url_for_different_endpoints(self):
        errors = shipments.validate_shipment(
            _valid_shipment(
                changelog_url=(
                    "https://github.com/mono/SkiaSharp/compare/"
                    "v4.150.2...v4.151.0-preview.2"
                )
            )
        )
        self.assertTrue(any("changelog_url" in error for error in errors))

    def test_rejects_inconsistent_previous_tag_and_changelog_url(self):
        errors = shipments.validate_shipment(_valid_shipment(previous_tag=None))
        self.assertTrue(any("previous_tag" in error and "changelog_url" in error for error in errors))

    def test_accepts_the_first_ever_shipment_with_no_previous_tag(self):
        errors = shipments.validate_shipment(
            _valid_shipment(previous_tag=None, changelog_url=None)
        )
        self.assertEqual(errors, [])


class ValidateShipmentsTests(unittest.TestCase):
    def test_rejects_a_non_list(self):
        self.assertEqual(shipments.validate_shipments({}), ["shipments must be an array"])

    def test_rejects_duplicate_tags(self):
        errors = shipments.validate_shipments([_valid_shipment(), _valid_shipment()])
        self.assertTrue(any("duplicate shipment tag" in error for error in errors))

    def test_accepts_distinct_shipments(self):
        errors = shipments.validate_shipments([
            _valid_shipment(),
            _valid_shipment(
                tag="v4.151.0",
                public_version="4.151.0",
                channel="stable",
                label="Stable",
                changelog_url=(
                    "https://github.com/mono/SkiaSharp/compare/"
                    "v4.150.2...v4.151.0"
                ),
            ),
        ])
        self.assertEqual(errors, [])


if __name__ == "__main__":
    unittest.main()
