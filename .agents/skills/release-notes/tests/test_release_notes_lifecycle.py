import importlib.util
import json
import sys
import unittest
from copy import deepcopy
from pathlib import Path
from unittest.mock import patch


ROOT = Path(__file__).resolve().parents[4]


def load_module(name, relative):
    spec = importlib.util.spec_from_file_location(name, ROOT / relative)
    module = importlib.util.module_from_spec(spec)
    sys.modules[name] = module
    spec.loader.exec_module(module)
    return module


DATA = load_module(
    "release_notes_data",
    "scripts/infra/docs/release-notes-data.py",
)
RENDER = load_module(
    "release_notes_render",
    "scripts/infra/docs/release-notes-render.py",
)
FIXTURE = json.loads(
    (Path(__file__).parent / "fixtures/shipment-tags.json").read_text()
)


def pr(number, author="contributor", community=True):
    return {
        "number": number,
        "url": f"https://github.com/mono/SkiaSharp/pull/{number}",
        "title": f"Change {number}",
        "author": {"login": author},
        "community": community,
        "category": "product",
    }


def shipment(tag, prs, channel="preview", previous="v4.150.0"):
    return {
        "tag": tag,
        "core_version": tag[1:].split("-")[0],
        "public_version": tag[1:],
        "channel": channel,
        "label": "Stable" if channel == "stable" else "Preview 1",
        "previous_tag": previous,
        "target_sha": "a" * 40,
        "date": "2026-08-01",
        "changelog_url": f"https://example.test/{tag}",
        "prs": prs,
    }


def teaser(prs, **extra):
    value = {
        "subtitle": "Adds useful package updates.",
        "categories": [
            {
                "heading": "What's New",
                "bullets": [{"text": "Adds a useful API", "prs": prs}],
            }
        ],
    }
    value.update(extra)
    return value


def page_data(shipments):
    numbers = {n for item in shipments for n in item["prs"]}
    return {
        "format": 4,
        "version": "4.151.0",
        "status": "preview",
        "banner": {},
        "contributors": [],
        "previews": [],
        "shipments": shipments,
        "prs": {
            str(n): {
                "url": f"https://github.com/mono/SkiaSharp/pull/{n}",
                "title": f"Change {n}",
                "author": "contributor",
                "community": True,
                "tag": "product",
            }
            for n in numbers
        },
    }


def cumulative_prose(release_teasers):
    return {
        "theme": "Useful release",
        "highlights_headline": "Useful package updates now ship.",
        "breaking": [],
        "categories": [],
        "contributor_summaries": {},
        "release_teasers": release_teasers,
    }


class ShipmentCollectionTests(unittest.TestCase):
    def collect(self, version, base):
        tags = FIXTURE["tags"]
        deltas = FIXTURE["deltas"]

        def fake_run(args, check=True):
            if args[:4] == ["git", "tag", "-l", "v*"]:
                return "\n".join(tags)
            if args[:2] == ["git", "rev-parse"]:
                return args[2].split("^")[0].encode().hex()[:40].ljust(40, "0")
            if args[:3] == ["git", "log", "-1"]:
                return "2026-08-01"
            raise AssertionError(args)

        def fake_delta(previous, tag):
            return [pr(n) for n in deltas.get(f"{previous}..{tag}", [])]

        with patch.object(DATA, "run", side_effect=fake_run), patch.object(
            DATA, "get_prs_from_diff", side_effect=fake_delta
        ), patch.object(DATA, "load_teaser_reviews", return_value={}):
            return DATA.collect_shipments(version, base)[0]

    def test_preview_rc_stable_and_multiple_builds_are_not_deduped(self):
        items = self.collect("4.151.0", "4.150.0")
        self.assertEqual(
            [s["tag"] for s in items],
            [
                "v4.151.0-preview.1.1",
                "v4.151.0-preview.1.2",
                "v4.151.0-rc.1.1",
                "v4.151.0",
            ],
        )
        self.assertEqual(items[1]["previous_tag"], items[0]["tag"])
        self.assertEqual(items[0]["previous_tag"], "v4.150.1")
        self.assertEqual(items[1]["prs"], [102])
        self.assertEqual(items[2]["channel"], "rc")
        self.assertEqual(items[3]["channel"], "stable")

    def test_internal_title_pattern_overrides_product_path(self):
        DATA._PATH_TAGS_CONFIG = None

        category = DATA._pr_category(
            {"binding/SkiaSharp/SkiaSharp.csproj"},
            "[infra] Add containerized test legs",
        )

        self.assertEqual(category, "internal")

        deterministic = DATA._pr_category(
            {"binding/SkiaSharp/SkiaApi.generated.cs"},
            "Make binding generation deterministic across platforms",
        )
        self.assertEqual(deterministic, "internal")

    def test_hotfix_preview_and_stable_use_exact_predecessors(self):
        items = self.collect("4.151.1", "4.151.0")
        self.assertEqual(
            [s["tag"] for s in items],
            ["v4.151.1-preview.1.1", "v4.151.1"],
        )
        self.assertEqual(items[0]["previous_tag"], "v4.151.0")
        self.assertEqual(items[1]["previous_tag"], items[0]["tag"])
        self.assertEqual(items[1]["prs"], [105])


class TeaserValidationAndRenderingTests(unittest.TestCase):
    def test_preview_uses_exact_teaser_website_summary(self):
        item = shipment("v4.151.0-preview.1.1", [101])
        data = page_data([item])
        data["previews"] = [
            {
                "key": item["tag"],
                "label": "Preview 1",
                "date": "August 1, 2026",
                "changelog_url": item["changelog_url"],
                "prs": [101],
            }
        ]
        prose = {
            "theme": "Preview",
            "highlights_headline": "A useful preview ships.",
            "breaking": [],
            "categories": [],
            "contributor_summaries": {},
            "preview_summaries": {item["tag"]: "Legacy text must not win."},
            "release_teasers": {
                item["tag"]: teaser(
                    [101], website_summary="The exact shipment summary wins."
                )
            },
        }
        self.assertEqual(RENDER.validate(data, prose), [])
        text = RENDER.render(data, prose)
        self.assertIn("The exact shipment summary wins.", text)
        self.assertNotIn("Legacy text must not win.", text)

    def test_rc_teaser_is_structured_escaped_and_credited(self):
        item = shipment("v4.151.0-rc.1.1", [103], channel="rc")
        data = page_data([item])
        prose = {
            "release_teasers": {
                item["tag"]: teaser(
                    [103],
                    subtitle="Tests [RC] *safely*.",
                )
            }
        }
        text = RENDER.render_release_teaser(data, prose, item["tag"])
        self.assertIn(r"Tests \[RC\] \*safely\*.", text)
        self.assertIn("## ✨ What's New", text)
        self.assertIn("[@contributor]", text)
        self.assertIn("Thanks to our contributors:", text)

    def test_stable_empty_delta_is_deterministic_without_ai(self):
        item = shipment(
            "v4.151.0", [], channel="stable", previous="v4.151.0-rc.1.1"
        )
        data = page_data([item])
        prose = {
            "theme": "Stable release",
            "highlights_headline": "The stable package now ships.",
            "breaking": [],
            "categories": [],
            "contributor_summaries": {},
            "release_teasers": {},
        }
        self.assertEqual(RENDER.validate(data, prose), [])
        text = RENDER.render_release_teaser(data, prose, item["tag"])
        self.assertEqual(
            text,
            RENDER.EMPTY_STABLE_SUBTITLE
            + "\n\n"
            + RENDER.RELEASE_LINKS_MARKER
            + "\n",
        )

    def test_teaser_prs_must_be_subset_of_exact_delta(self):
        item = shipment("v4.151.0-preview.1.1", [101])
        errors = DATA.validate_release_teaser(item, teaser([999]))
        self.assertTrue(any("outside its exact delta" in e for e in errors))

    def test_security_details_are_rejected(self):
        item = shipment("v4.151.0-preview.1.1", [101])
        value = teaser([101])
        value["categories"][0]["bullets"][0]["text"] = "Fixes CVE-2026-1234"
        errors = DATA.validate_release_teaser(item, value)
        self.assertTrue(any("security/vulnerability" in e for e in errors))

    def test_reviewed_teaser_selection_and_wording_are_enforced(self):
        item = shipment("v4.151.0-preview.2.1", [101, 102, 103])
        item["teaser_review"] = {
            "subtitle": "Reviewed subtitle.",
            "website_summary": "Reviewed `SKColor` summary.",
            "selected_prs": [101, 102],
            "required_phrases": ["SKColor"],
            "forbidden_phrases": ["managed color conversion"],
            "pr_required_phrases": {
                "101": ["specific behavior"],
            },
        }
        value = teaser(
            [101, 103],
            subtitle="Unreviewed subtitle.",
            website_summary="Managed color conversions.",
        )

        errors = DATA.validate_release_teaser(item, value)

        self.assertTrue(any("subtitle must match" in error for error in errors))
        self.assertTrue(any("website_summary must match" in error for error in errors))
        self.assertTrue(any("omits reviewed PRs: #102" in error for error in errors))
        self.assertTrue(any("excluded by review: #103" in error for error in errors))
        self.assertTrue(any("must mention reviewed phrase 'SKColor'" in error
                            for error in errors))
        self.assertTrue(any("misattributed phrase" in error for error in errors))
        self.assertTrue(any("PR #101 must mention" in error for error in errors))

    def test_internal_prs_are_rejected_from_cumulative_categories(self):
        item = shipment("v4.151.0-preview.1.1", [101])
        data = page_data([item])
        data["prs"]["101"]["tag"] = "internal"
        prose = cumulative_prose({item["tag"]: teaser([101])})
        prose["theme"] = "Consumer release"
        prose["highlights_headline"] = "Consumer improvements ship."
        prose["categories"] = [{
            "heading": "Highlights",
            "bullets": [{
                "lead": "CI mechanics",
                "detail": "Adds a containerized test leg.",
                "prs": [101],
            }],
        }]

        errors = RENDER.validate(data, prose)

        self.assertTrue(any("must not reference internal PRs: #101" in error
                            for error in errors))

        data["version"] = "2.80.0"
        legacy_errors = RENDER.validate(data, prose)
        self.assertFalse(any("must not reference internal PRs: #101" in error
                             for error in legacy_errors))

    def test_page_review_enforces_count_exclusions_and_safe_credit(self):
        item = shipment("v4.152.0-preview.1.1", [101, 102])
        data = page_data([item])
        data["contributors"] = [{"login": "builder", "prs": [102]}]
        data["cumulative_review"] = {
            "required_phrases": ["Four rounds"],
            "forbidden_phrases": ["Three rounds"],
            "excluded_prs": [101],
            "contributor_summaries": {
                "builder": "Release infrastructure maintenance",
            },
        }
        prose = cumulative_prose({item["tag"]: teaser([101, 102])})
        prose["theme"] = "Three rounds"
        prose["categories"] = [{
            "heading": "Engine",
            "bullets": [{
                "lead": "Engine updates",
                "detail": "Upstream updates.",
                "prs": [101],
            }],
        }]
        prose["contributor_summaries"] = {
            "builder": "MSVC environment initialization",
        }

        errors = RENDER.validate(data, prose)

        self.assertTrue(any("must mention reviewed phrase 'Four rounds'" in error
                            for error in errors))
        self.assertTrue(any("must not use contradicted phrase 'Three rounds'"
                            in error for error in errors))
        self.assertTrue(any("reviewer-excluded PRs: #101" in error
                            for error in errors))
        self.assertTrue(any("must match reviewed text" in error
                            for error in errors))

    def test_committed_page_reviews_cover_known_regressions(self):
        DATA._PAGE_REVIEWS_CONFIG = None
        reviews = DATA.load_page_reviews()

        self.assertEqual(reviews["4.150.2"]["required_phrases"], ["Four rounds"])
        self.assertEqual(reviews["4.150.2"]["forbidden_phrases"], ["Three rounds"])
        self.assertEqual(reviews["4.152.0"]["excluded_prs"], [4612])
        self.assertIn("binding generation",
                      reviews["4.152.0"]["forbidden_phrases"])
        self.assertIn("MSVC", reviews["4.152.0"]["forbidden_phrases"])
        self.assertEqual(
            reviews["4.152.0"]["contributor_summaries"]["mmitche"],
            "Release infrastructure maintenance",
        )

    def test_published_preview_one_review_excludes_valid_delta_pr_3788(self):
        DATA._TEASER_REVIEWS_CONFIG = None
        reviews = DATA.load_teaser_reviews()

        selected = reviews["v4.151.0-preview.1.1"]["selected_prs"]

        self.assertEqual(selected, [4294])
        self.assertNotIn(3788, selected)

    def test_published_rc_review_excludes_test_and_generic_sync_work(self):
        DATA._TEASER_REVIEWS_CONFIG = None
        review = DATA.load_teaser_reviews()["v4.151.0-rc.1.1"]

        self.assertEqual(
            review["selected_prs"],
            [3997, 4370, 4385, 4428, 4459, 4487],
        )
        self.assertNotIn(4488, review["selected_prs"])
        self.assertNotIn(4443, review["selected_prs"])
        self.assertNotIn(4489, review["selected_prs"])
        self.assertEqual(
            review["pr_required_phrases"]["4487"],
            ["exception-handling mismatch"],
        )

    def test_duplicate_exact_tags_are_rejected(self):
        item = shipment("v4.151.0-preview.1.1", [101])
        errors = DATA.validate_shipments(page_data([item, deepcopy(item)]))
        self.assertEqual(
            errors,
            ["duplicate exact shipment tags: v4.151.0-preview.1.1"],
        )

    def test_website_security_prose_is_not_mapped_to_dependency_teaser(self):
        item = shipment("v4.151.0-preview.1.1", [101])
        data = page_data([item])
        prose = {
            "categories": [
                {
                    "heading": "Security",
                    "bullets": [
                        {
                            "lead": "Dependencies refreshed",
                            "detail": "Security fixes.",
                            "prs": [101],
                        }
                    ],
                }
            ],
            "release_teasers": {item["tag"]: teaser([101])},
        }
        text = RENDER.render_release_teaser(data, prose, item["tag"])
        self.assertNotIn("Dependency Updates", text)
        self.assertNotIn("Security fixes", text)


class PreservationLifecycleTests(unittest.TestCase):
    def test_cumulative_change_preserves_reviewed_teasers(self):
        tag = "v4.151.0-preview.1.1"
        item = shipment(tag, [101])
        old_data = page_data([item])
        new_data = deepcopy(old_data)
        new_data["tallies"] = {"product": 2}
        reviewed = teaser([101], website_summary="Reviewed summary.")
        prose = {
            "theme": "Old",
            "release_teasers": {tag: reviewed},
        }
        task = DATA.build_polish_task(
            "documentation/docfx/releases/4.151.0.md",
            old_data,
            new_data,
            prose,
        )
        self.assertTrue(task["cumulative"])
        self.assertEqual(task["release_teasers"], [])
        self.assertEqual(prose["release_teasers"][tag], reviewed)

        path = ROOT / "output/release-notes-preservation-test.prose.json"
        try:
            DATA.preserve_teasers_for_cumulative_rewrite(path, prose)
            persisted = json.loads(path.read_text())
            self.assertEqual(persisted, {"release_teasers": {tag: reviewed}})
        finally:
            if path.exists():
                path.unlink()

    def test_manifest_lists_only_missing_invalid_and_facts_changed(self):
        tags = [
            "v4.151.0-preview.1.1",
            "v4.151.0-preview.1.2",
            "v4.151.0-rc.1.1",
            "v4.151.0-preview.2.1",
        ]
        old_items = [shipment(tag, [100 + i]) for i, tag in enumerate(tags)]
        new_items = deepcopy(old_items)
        new_items[2]["target_sha"] = "b" * 40
        prose = cumulative_prose({
            tags[0]: teaser([100]),
            tags[1]: teaser([999]),
            tags[2]: teaser([102]),
        })
        task = DATA.build_polish_task(
            "page.md",
            page_data(old_items),
            page_data(new_items),
            prose,
        )
        self.assertFalse(task["cumulative"])
        self.assertEqual(
            task["release_teasers"],
            [
                {"tag": tags[1], "reason": "invalid"},
                {"tag": tags[2], "reason": "facts-changed"},
                {"tag": tags[3], "reason": "missing"},
            ],
        )

    def test_cumulative_prose_only_edit_does_not_change_teaser_render(self):
        item = shipment("v4.151.0-preview.1.1", [101])
        data = page_data([item])
        before = cumulative_prose({item["tag"]: teaser([101])})
        before["theme"] = "Before"
        after = deepcopy(before)
        after["theme"] = "After"
        after["categories"] = [{"heading": "Security", "bullets": []}]
        self.assertEqual(
            RENDER.render_release_teaser(data, before, item["tag"]),
            RENDER.render_release_teaser(data, after, item["tag"]),
        )

    def test_polish_manifest_keeps_work_axes_separate(self):
        path = ROOT / "output/release-notes-manifest-test.json"
        tasks = [
            {
                "page": "documentation/docfx/releases/4.151.0.md",
                "cumulative": False,
                "release_teasers": [
                    {
                        "tag": "v4.151.0-preview.1.1",
                        "reason": "missing",
                    }
                ],
            }
        ]
        try:
            DATA.write_polish_list(tasks, path)
            self.assertEqual(
                json.loads(path.read_text()),
                {"format": 1, "tasks": tasks},
            )
        finally:
            if path.exists():
                path.unlink()

    def test_empty_stable_shipment_needs_no_ai_teaser_task(self):
        item = shipment(
            "v4.151.0", [], channel="stable", previous="v4.151.0-rc.1.1"
        )
        data = page_data([item])
        task = DATA.build_polish_task(
            "page.md",
            data,
            deepcopy(data),
            cumulative_prose({}),
        )
        self.assertIsNone(task)

    def test_partial_preserved_file_requeues_cumulative_work(self):
        item = shipment("v4.151.0-preview.1.1", [101])
        data = page_data([item])
        reviewed = teaser([101])
        task = DATA.build_polish_task(
            "page.md",
            data,
            deepcopy(data),
            {"release_teasers": {item["tag"]: reviewed}},
        )
        self.assertTrue(task["cumulative"])
        self.assertEqual(task["release_teasers"], [])

    def test_force_reauthors_cumulative_only(self):
        item = shipment("v4.151.0-preview.1.1", [101])
        data = page_data([item])
        task = DATA.build_polish_task(
            "page.md",
            data,
            deepcopy(data),
            cumulative_prose({item["tag"]: teaser([101])}),
            force=True,
        )
        self.assertTrue(task["cumulative"])
        self.assertEqual(task["release_teasers"], [])

    def test_malformed_teaser_reports_validation_errors(self):
        item = shipment("v4.151.0-preview.1.1", [101])
        value = teaser([101])
        value["categories"][0]["bullets"][0]["prs"] = [101, 101, "bad"]
        errors = DATA.validate_release_teaser(item, value)
        self.assertTrue(any("positive integers" in error for error in errors))


if __name__ == "__main__":
    unittest.main()
