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
        "format": 5,
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
                "prs": [4294, 4300, 3788],
                "attributions": [
                    {"display": "@ramezgerges", "kind": "human", "first_time": True},
                    {"display": "GitHub Copilot", "kind": "ai"},
                    {
                        "display": "@returning-contributor",
                        "kind": "human",
                        "first_time": False,
                    },
                    {"display": "Claude", "kind": "ai"},
                    {"display": "@dependabot[bot]", "kind": "automation"},
                ],
            }
        ],
        "prs": {
            "4294": {
                "title": "Improve rendering",
                "url": "https://github.com/mono/SkiaSharp/pull/4294",
                "author": "ramezgerges",
                "community": True,
                "first_time_contributor": True,
                "attributions": [
                    {"display": "@ramezgerges", "kind": "human", "first_time": True},
                    {"display": "GitHub Copilot", "kind": "ai"},
                ],
            },
            "3788": {
                "title": "Update generated assets",
                "url": "https://github.com/mono/SkiaSharp/pull/3788",
                "author": "dependabot[bot]",
                "community": False,
                "first_time_contributor": True,
                "attributions": [
                    {"display": "@dependabot[bot]", "kind": "automation"},
                ],
            },
            "4300": {
                "title": "Fix image decoding",
                "url": "https://github.com/mono/SkiaSharp/pull/4300",
                "author": "returning-contributor",
                "community": True,
                "first_time_contributor": False,
                "attributions": [
                    {
                        "display": "@returning-contributor",
                        "kind": "human",
                        "first_time": False,
                    },
                    {"display": "Claude", "kind": "ai"},
                ],
            },
        },
        "contributors": [
            {"login": "ramezgerges", "url": "https://github.com/ramezgerges", "prs": [4294]},
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
    def test_renders_the_short_reviewed_body_without_a_pr_list(self):
        text = render_summary.render_github_release_summary(
            _sample_data(), _sample_prose(), "v4.151.0-preview.1"
        )
        self.assertIn("**Preview 1**", text)
        self.assertIn("SkiaSharp 4.151.0 previews the Skia m151 engine update.", text)
        self.assertIn("It brings the current upstream renderer into the 4.151 line.", text)
        self.assertNotIn("## What's Changed", text)
        self.assertNotIn("Improve rendering", text)

    def test_credits_all_humans_and_repeats_first_timers_in_the_subset(self):
        text = render_summary.render_github_release_summary(
            _sample_data(), _sample_prose(), "v4.151.0-preview.1"
        )
        self.assertIn(
            "\U0001F465 Contributors: @ramezgerges, @returning-contributor.",
            text,
        )
        self.assertIn(
            "\U0001F389 First-time contributors: @ramezgerges.",
            text,
        )
        self.assertEqual(text.count("@ramezgerges"), 2)

    def test_credits_maintainers_but_rejects_unsafe_handle_facts(self):
        data = _sample_data()
        data["shipments"][0]["attributions"][0] = {
            "display": "@mattleibow",
            "kind": "human",
            "first_time": False,
        }
        text = render_summary.render_github_release_summary(data, _sample_prose(), "v4.151.0-preview.1")
        self.assertIn("\U0001F465 Contributors: @mattleibow, @returning-contributor.", text)
        data["shipments"][0]["attributions"][0] = {
            "display": "@a](evil)",
            "kind": "human",
            "first_time": False,
        }
        with self.assertRaisesRegex(ValueError, "unsafe attribution"):
            render_summary.render_github_release_summary(
                data, _sample_prose(), "v4.151.0-preview.1"
            )
        data["shipments"][0]["attributions"][0] = {
            "display": "Person\n\n## Injected",
            "kind": "human",
            "first_time": False,
        }
        with self.assertRaisesRegex(ValueError, "unsafe attribution"):
            render_summary.render_github_release_summary(
                data, _sample_prose(), "v4.151.0-preview.1"
            )

    def test_credits_automation_and_ai_separately_from_humans(self):
        text = render_summary.render_github_release_summary(
            _sample_data(), _sample_prose(), "v4.151.0-preview.1"
        )
        self.assertIn(
            "\U0001F916 Automation and AI assistance: GitHub Copilot, Claude, @dependabot[bot].",
            text,
        )
        contributors = next(
            line for line in text.splitlines() if line.startswith("\U0001F465")
        )
        self.assertNotIn("Copilot", contributors)
        self.assertNotIn("Claude", contributors)
        self.assertNotIn("dependabot", contributors)

    def test_uses_shipment_attributions_when_page_pr_facts_do_not_cover_the_delta(self):
        data = _sample_data(prs={})
        data["shipments"][0]["attributions"] = [
            {"display": "@mattleibow", "kind": "human", "first_time": False},
            {"display": "GitHub Copilot", "kind": "ai"},
        ]
        text = render_summary.render_github_release_summary(
            data, _sample_prose(), "v4.151.0-preview.1"
        )
        self.assertIn("\U0001F465 Contributors: @mattleibow.", text)
        self.assertIn("\U0001F916 Automation and AI assistance: GitHub Copilot.", text)

    def test_renders_exactly_one_release_links_marker(self):
        text = render_summary.render_github_release_summary(
            _sample_data(), _sample_prose(), "v4.151.0-preview.1"
        )
        self.assertEqual(text.count(safety.RELEASE_LINKS_MARKER), 1)

    def test_omits_body_section_when_summary_has_no_body(self):
        prose = _sample_prose()
        prose["release_summaries"]["v4.151.0-preview.1"] = {
            "headline": "A focused preview release."
        }
        text = render_summary.render_github_release_summary(
            _sample_data(), prose, "v4.151.0-preview.1"
        )
        self.assertIn("A focused preview release.", text)

    def test_omits_empty_contributor_lines(self):
        data = _sample_data()
        data["shipments"][0]["attributions"] = []
        text = render_summary.render_github_release_summary(
            data, _sample_prose(), "v4.151.0-preview.1"
        )
        self.assertNotIn("Contributors:", text)
        self.assertNotIn("First-time contributors:", text)
        self.assertNotIn("Automation and AI assistance:", text)

    def test_raises_key_error_for_a_tag_with_no_shipment(self):
        with self.assertRaises(KeyError):
            render_summary.render_github_release_summary(
                _sample_data(), _sample_prose(), "v9.9.9"
            )

    def test_raises_value_error_for_unsafe_prose(self):
        prose = _sample_prose()
        prose["release_summaries"]["v4.151.0-preview.1"]["headline"] = (
            "Fixes CVE-2024-99999 in the bundled library."
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
