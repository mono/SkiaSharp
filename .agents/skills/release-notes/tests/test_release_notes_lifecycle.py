import importlib.util
import json
import sys
import tempfile
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
        "author": author,
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
        ):
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

    def test_harfbuzz_summary_facts_exclude_internal_only_work(self):
        prs = [
            {
                "number": 101,
                "title": "Add HarfBuzz API",
                "url": "https://github.com/mono/SkiaSharp/pull/101",
                "author": {"login": "contributor"},
                "category": "product",
                "files": ["binding/HarfBuzzSharp/Foo.cs"],
            },
            {
                "number": 102,
                "title": "Convert all .sln solutions to .slnx format",
                "url": "https://github.com/mono/SkiaSharp/pull/102",
                "author": {"login": "maintainer"},
                "category": "internal",
                "files": ["binding/HarfBuzzSharp/HarfBuzzSharp.slnx"],
            },
        ]
        metadata = {
            "version": "4.152.0",
            "status": "preview",
            "shipments": [],
            "harfbuzz": {"version": "14.2.1", "prs": [101, 102]},
        }

        data = DATA.build_data_json(prs, metadata)

        self.assertEqual(data["harfbuzz"]["prs"], [101])

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
    def test_harfbuzz_version_change_requires_authored_summary(self):
        data = page_data([])
        data["harfbuzz"] = {
            "version": "14.2.1",
            "previous_version": "14.2.0",
            "prs": [],
        }
        prose = cumulative_prose({})

        errors = RENDER.validate(data, prose)
        self.assertTrue(any("changes HarfBuzz" in error for error in errors))

        prose["harfbuzz_summary"] = (
            "Updates the bundled HarfBuzz from 14.2.0 to 14.2.1."
        )
        self.assertFalse(any(
            "changes HarfBuzz" in error
            for error in RENDER.validate(data, prose)
        ))
        self.assertIn(prose["harfbuzz_summary"], RENDER.render(data, prose))

    def test_unchanged_harfbuzz_uses_narrow_deterministic_statement(self):
        data = page_data([])
        data["harfbuzz"] = {
            "version": "14.2.1",
            "previous_version": "14.2.1",
            "prs": [],
        }
        prose = cumulative_prose({})

        self.assertNotIn(
            "same HarfBuzz as the previous line",
            RENDER.render(data, prose),
        )
        self.assertIn(RENDER.NO_CHANGES_BODY, RENDER.render(data, prose))

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

    def test_sync_round_count_and_internal_mechanics_are_validated_generally(self):
        item = shipment("v4.150.2", [101, 102], channel="stable")
        data = page_data([item])
        data["version"] = "4.150.2"
        data["prs"]["101"]["title"] = "[skia-sync] Merge upstream bug fixes"
        data["prs"]["102"]["title"] = "[skia-sync] Update with Ganesh fixes"
        data["contributors"] = [{"login": "builder", "prs": [102]}]
        prose = cumulative_prose({item["tag"]: teaser([101, 102])})
        prose["theme"] = "Three rounds of Skia fixes"
        prose["categories"] = [{
            "heading": "Engine",
            "bullets": [{
                "lead": "Engine updates",
                "detail": "Upstream updates.",
                "prs": [101, 102],
            }],
        }]
        prose["contributor_summaries"] = {
            "builder": "MSVC environment initialization",
        }

        errors = RENDER.validate(data, prose)

        self.assertTrue(any("facts contain 2" in error for error in errors))
        self.assertTrue(any("internal work broadly" in error
                            for error in errors))

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
    def test_changed_facts_delete_full_prose_and_queue_page(self):
        with tempfile.TemporaryDirectory() as directory:
            page = Path(directory) / "4.150.0.md"
            data_path = DATA._data_json_path(page)
            prose_path = DATA._prose_json_path(page)
            data_path.parent.mkdir(parents=True)
            data_path.write_text(json.dumps({"version": "old"}) + "\n")
            prose_path.write_text(json.dumps(cumulative_prose({})) + "\n")

            result = DATA._write_page_outputs(
                page, {"version": "4.150.0"}, 3
            )

            self.assertEqual(result, str(page))
            self.assertFalse(prose_path.exists())
            self.assertEqual(json.loads(data_path.read_text()),
                             {"version": "4.150.0"})

    def test_unchanged_facts_keep_prose_and_do_not_queue_page(self):
        with tempfile.TemporaryDirectory() as directory:
            page = Path(directory) / "4.150.0.md"
            data_path = DATA._data_json_path(page)
            prose_path = DATA._prose_json_path(page)
            data = {"version": "4.150.0"}
            data_path.parent.mkdir(parents=True)
            data_path.write_text(json.dumps(data, indent=2) + "\n")
            prose_path.write_text(json.dumps(cumulative_prose({})) + "\n")

            result = DATA._write_page_outputs(page, data, 3)

            self.assertIsNone(result)
            self.assertTrue(prose_path.exists())

    def test_force_deletes_full_prose_and_queues_unchanged_page(self):
        with tempfile.TemporaryDirectory() as directory:
            page = Path(directory) / "4.150.0.md"
            data_path = DATA._data_json_path(page)
            prose_path = DATA._prose_json_path(page)
            data = {"version": "4.150.0"}
            data_path.parent.mkdir(parents=True)
            data_path.write_text(json.dumps(data, indent=2) + "\n")
            prose_path.write_text(json.dumps(cumulative_prose({})) + "\n")

            result = DATA._write_page_outputs(page, data, 3, force=True)

            self.assertEqual(result, str(page))
            self.assertFalse(prose_path.exists())

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

    def test_polish_list_contains_one_page_path_per_line(self):
        path = ROOT / "output/release-notes-polish-list-test.txt"
        pages = [
            "documentation/docfx/releases/4.150.0.md",
            "documentation/docfx/releases/4.150.1.md",
        ]
        try:
            DATA.write_polish_list(pages, path)
            self.assertEqual(path.read_text().splitlines(), pages)
        finally:
            if path.exists():
                path.unlink()

    def test_empty_polish_list_is_an_empty_file(self):
        path = ROOT / "output/release-notes-polish-list-test.txt"
        try:
            DATA.write_polish_list([], path)
            self.assertEqual(path.read_text(), "")
        finally:
            if path.exists():
                path.unlink()

    def test_malformed_teaser_reports_validation_errors(self):
        item = shipment("v4.151.0-preview.1.1", [101])
        value = teaser([101])
        value["categories"][0]["bullets"][0]["prs"] = [101, 101, "bad"]
        errors = DATA.validate_release_teaser(item, value)
        self.assertTrue(any("positive integers" in error for error in errors))


class ApiDiffLifecycleTests(unittest.TestCase):
    def test_scoped_rebuild_clears_shared_line_once(self):
        source = (ROOT / "scripts/infra/docs/api-diff.cake").read_text()
        tracker = source.index("var clearedLineDirs = new HashSet<string>")
        package_loop = source.index("foreach (var id in TRACKED_NUGETS.Keys)")
        clear_guard = source.index("clearedLineDirs.Add (lineDir.FullPath)")
        clear_call = source.index(
            "ClearGeneratedApiDiffsIn (lineDir.FullPath)", clear_guard
        )

        self.assertLess(tracker, package_loop)
        self.assertLess(package_loop, clear_guard)
        self.assertLess(clear_guard, clear_call)


if __name__ == "__main__":
    unittest.main()
