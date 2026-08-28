from __future__ import annotations

import sys
import unittest
from pathlib import Path

_DOCS_DIR = Path(__file__).resolve().parents[2]
if str(_DOCS_DIR) not in sys.path:
    sys.path.insert(0, str(_DOCS_DIR))

from release_notes import render_summary, safety


def _sample_data(**overrides):
    data = {
        "format": 4,
        "version": "4.151.0",
        "shipments": [
            {
                "tag": "v4.151.0-preview.1",
                "core_version": "4.151.0",
                "public_version": "4.151.0-preview.1",
                "channel": "preview",
                "label": "Preview 1",
                "previous_tag": "v4.150.2",
                "target_sha": "a" * 40,
                "date": "2026-01-01",
                "changelog_url": (
                    "https://github.com/mono/SkiaSharp/compare/v4.150.2...v4.151.0-preview.1"
                ),
                "prs": [4294, 3788],
            }
        ],
        "contributors": [
            {"login": "ramezgerges", "url": "https://github.com/ramezgerges", "prs": [4294]},
            {"login": "someone-else", "url": "https://github.com/someone-else", "prs": [9999]},
        ],
    }
    data.update(overrides)
    return data


def _sample_prose(**overrides):
    prose = {
        "release_summaries": {
            "v4.151.0-preview.1": {
                "headline": "SkiaSharp 4.151.0 previews the Skia m151 engine update.",
                "body": "It brings the current upstream renderer into the 4.151 line.",
            }
        }
    }
    prose.update(overrides)
    return prose


class RenderGithubReleaseSummaryTests(unittest.TestCase):
    def test_renders_label_headline_and_body(self):
        text = render_summary.render_github_release_summary(
            _sample_data(), _sample_prose(), "v4.151.0-preview.1"
        )
        self.assertIn("**Preview 1**", text)
        self.assertIn("SkiaSharp 4.151.0 previews the Skia m151 engine update.", text)
        self.assertIn("It brings the current upstream renderer into the 4.151 line.", text)

    def test_renders_exactly_one_release_links_marker(self):
        text = render_summary.render_github_release_summary(
            _sample_data(), _sample_prose(), "v4.151.0-preview.1"
        )
        self.assertEqual(text.count(safety.RELEASE_LINKS_MARKER), 1)

    def test_credits_only_contributors_whose_prs_intersect_this_shipment(self):
        text = render_summary.render_github_release_summary(
            _sample_data(), _sample_prose(), "v4.151.0-preview.1"
        )
        self.assertIn("Thanks to our contributors: @ramezgerges", text)
        self.assertNotIn("@someone-else", text)

    def test_omits_contributor_line_when_nobody_qualifies(self):
        data = _sample_data(contributors=[])
        text = render_summary.render_github_release_summary(
            data, _sample_prose(), "v4.151.0-preview.1"
        )
        self.assertNotIn("Thanks to our contributors", text)

    def test_never_credits_a_login_that_fails_safety_validation(self):
        data = _sample_data(contributors=[
            {"login": "a](evil)", "url": "x", "prs": [4294]},
        ])
        text = render_summary.render_github_release_summary(
            data, _sample_prose(), "v4.151.0-preview.1"
        )
        self.assertNotIn("Thanks to our contributors", text)
        self.assertNotIn("evil", text)

    def test_omits_body_section_when_summary_has_no_body(self):
        prose = _sample_prose()
        prose["release_summaries"]["v4.151.0-preview.1"] = {
            "headline": "A focused preview release."
        }
        text = render_summary.render_github_release_summary(
            _sample_data(), prose, "v4.151.0-preview.1"
        )
        self.assertIn("A focused preview release.", text)

    def test_raises_key_error_for_a_tag_with_no_shipment(self):
        with self.assertRaises(KeyError):
            render_summary.render_github_release_summary(
                _sample_data(), _sample_prose(), "v9.9.9"
            )

    def test_raises_key_error_for_a_shipment_with_no_summary_yet(self):
        data = _sample_data()
        data["shipments"].append({
            "tag": "v4.151.0",
            "core_version": "4.151.0",
            "public_version": "4.151.0",
            "channel": "stable",
            "label": "Stable",
            "previous_tag": "v4.151.0-preview.1",
            "target_sha": "b" * 40,
            "date": "2026-02-01",
            "changelog_url": "https://github.com/mono/SkiaSharp/compare/v4.151.0-preview.1...v4.151.0",
            "prs": [],
        })
        with self.assertRaises(KeyError):
            render_summary.render_github_release_summary(data, _sample_prose(), "v4.151.0")

    def test_raises_value_error_for_unsafe_prose(self):
        prose = _sample_prose()
        prose["release_summaries"]["v4.151.0-preview.1"]["headline"] = (
            "Fixes CVE-2024-99999 in the bundled library."
        )
        with self.assertRaises(ValueError):
            render_summary.render_github_release_summary(
                _sample_data(), prose, "v4.151.0-preview.1"
            )

    def test_raises_value_error_when_prose_smuggles_a_managed_marker(self):
        gh = safety._gh
        prose = _sample_prose()
        prose["release_summaries"]["v4.151.0-preview.1"]["body"] = (
            "Safe start. {} sneaky.".format(gh.SUMMARY_END_MARKER)
        )
        with self.assertRaises(ValueError):
            render_summary.render_github_release_summary(
                _sample_data(), prose, "v4.151.0-preview.1"
            )

    def test_is_deterministic(self):
        first = render_summary.render_github_release_summary(
            _sample_data(), _sample_prose(), "v4.151.0-preview.1"
        )
        second = render_summary.render_github_release_summary(
            _sample_data(), _sample_prose(), "v4.151.0-preview.1"
        )
        self.assertEqual(first, second)


if __name__ == "__main__":
    unittest.main()
