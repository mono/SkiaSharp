import json
import sys
import tempfile
import unittest
from copy import deepcopy
from pathlib import Path
from unittest.mock import patch


ROOT = Path(__file__).resolve().parents[5]
PACKAGE_PARENT = ROOT / "scripts/infra/docs"
if str(PACKAGE_PARENT) not in sys.path:
    sys.path.insert(0, str(PACKAGE_PARENT))

from release_notes import common as COMMON
from release_notes import generate as GENERATE
from release_notes import model as MODEL
from release_notes import render as RENDER
from release_notes import sources as SOURCES
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


def release_summary(prs, **extra):
    value = {
        "summary": "Adds useful package updates.",
        "prs": prs,
    }
    value.update(extra)
    return value


def page_data(shipments):
    numbers = {n for item in shipments for n in item["prs"]}
    return {
        "format": 5,
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


def cumulative_prose(release_summaries):
    return {
        "theme": "Useful release",
        "highlights_headline": "Useful package updates now ship.",
        "breaking": [],
        "categories": [],
        "contributor_summaries": {},
        "release_summaries": release_summaries,
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

        with patch.object(COMMON, "run", side_effect=fake_run), patch.object(
            SOURCES, "get_prs_from_diff", side_effect=fake_delta
        ):
            return MODEL.collect_shipments(version, base)[0]

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

    def test_titles_do_not_override_product_paths(self):
        category = SOURCES.pr_category(
            {"binding/SkiaSharp/SkiaSharp.csproj"},
            "[infra] Add containerized test legs",
        )
        self.assertEqual(category, "product")

        deterministic = SOURCES.pr_category(
            {"binding/SkiaSharp/SkiaApi.generated.cs"},
            "Make binding generation deterministic across platforms",
        )
        self.assertEqual(deterministic, "product")

    def test_new_repository_paths_fail_classification(self):
        with self.assertRaisesRegex(ValueError, "Unclassified"):
            SOURCES.pr_category({"new-shipping-area/Feature.cs"}, "Add feature")

    def test_legacy_paths_are_explicitly_classified(self):
        self.assertEqual(
            SOURCES.pr_category({"nuget/SkiaSharp.nuspec"}),
            "mixed",
        )
        self.assertEqual(SOURCES.pr_category({"VERSIONS.txt"}), "mixed")
        self.assertEqual(
            SOURCES.pr_category({"scripts/VERSIONS.txt"}),
            "mixed",
        )
        for path in (
            "cake/shared.cake",
            "design/README.md",
            "wiki/Versioning.md",
            "mono.snk",
        ):
            self.assertEqual(SOURCES.pr_category({path}), "internal")
        for path in (
            "binding/HarfBuzzSharp/HarfBuzzSharp.slnx",
            "source/SkiaSharpSource.slnf",
        ):
            self.assertEqual(SOURCES.pr_category({path}), "internal")
        for path in (
            "binding/IncludeNativeAssets.HarfBuzzSharp.targets",
            "source/SkiaSharp.Build.targets",
        ):
            self.assertEqual(SOURCES.pr_category({path}), "mixed")

        self.assertEqual(
            SOURCES.pr_category({
                "binding/IncludeNativeAssets.HarfBuzzSharp.targets",
                "source/SkiaSharp.Build.targets",
                "scripts/azure-templates-stages-test.yml",
                "tests/Tests/BaseTest.cs",
            }),
            "mixed",
        )
        self.assertEqual(
            SOURCES.pr_category({
                "binding/HarfBuzzSharp/HarfBuzzSharp.slnx",
                "native/windows/libHarfBuzzSharp/libHarfBuzzSharp.slnx",
                "native/windows/build.cake",
                "documentation/dev/building.md",
            }),
            "mixed",
        )

    def test_superseded_line_is_immediate_harfbuzz_predecessor(self):
        co_releases = {
            "3.119.4": "8.3.1.5",
            "4.147.0": "8.3.1.6",
            "4.148.0": "14.2.0",
        }

        self.assertEqual(
            GENERATE._previous_co_release_version(
                "3.119.4",
                ["4.147.0"],
                co_releases,
            ),
            "8.3.1.6",
        )
        self.assertEqual(
            GENERATE._previous_co_release_version(
                "3.119.4",
                [],
                co_releases,
            ),
            "8.3.1.5",
        )

    def test_harfbuzz_update_title_joins_path_scoped_prs(self):
        prs = [
            {"number": 101, "title": "Add shaping API", "body": ""},
            {"number": 102, "title": "Update HarfBuzz to 14.2.0", "body": ""},
            {"number": 103, "title": "Update Skia", "body": ""},
        ]

        self.assertEqual(
            GENERATE._harfbuzz_pr_numbers(prs, [prs[0]]),
            [101, 102],
        )

    def test_placeholder_tag_is_not_an_exact_release(self):
        self.assertIsNone(MODEL.parse_tag("v3.0.0-preview.2.x"))
        self.assertIsNotNone(MODEL.parse_tag("v3.0.0-preview.2.1"))

    def test_harfbuzz_summary_facts_include_product_work_only(self):
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
            {
                "number": 103,
                "title": "Adjust HarfBuzz build targets",
                "url": "https://github.com/mono/SkiaSharp/pull/103",
                "author": {"login": "maintainer"},
                "category": "mixed",
                "files": ["binding/IncludeNativeAssets.HarfBuzzSharp.targets"],
            },
        ]
        metadata = {
            "version": "4.152.0",
            "status": "preview",
            "shipments": [],
            "harfbuzz": {"version": "14.2.1", "prs": [101, 102, 103]},
        }

        data = MODEL.build_data_json(prs, metadata)

        self.assertEqual(data["harfbuzz"]["prs"], [101])

    def test_product_pr_keeps_generic_commit_facts(self):
        prs = [{
            "number": 4379,
            "title": "[skia-sync] Merge upstream chrome/m150 bug fixes",
            "url": "https://github.com/mono/SkiaSharp/pull/4379",
            "author": {"login": "mattleibow"},
            "category": "product",
            "commit": "a" * 40,
            "body": "Fix Use-After-Free in SubRunAllocator.",
            "skiaPr": 281,
        }]

        data = MODEL.build_data_json(
            prs,
            {
                "version": "4.150.1",
                "status": "preview",
                "from": "v4.150.0",
                "to": "v4.150.1",
                "shipments": [],
            },
        )

        fact = data["prs"]["4379"]
        self.assertEqual(fact["tag"], "product")
        self.assertEqual(fact["commit"], "a" * 40)
        self.assertEqual(fact["body"], "Fix Use-After-Free in SubRunAllocator.")
        self.assertEqual(fact["companion_pr"]["number"], 281)
        self.assertEqual(
            data["range"],
            {
                "from": "v4.150.0",
                "to": "v4.150.1",
                "base_version": None,
            },
        )

    def test_internal_pr_omits_body_and_contributor_credit(self):
        prs = [{
            "number": 101,
            "title": "Update workflow",
            "url": "https://github.com/mono/SkiaSharp/pull/101",
            "author": {"login": "contributor"},
            "category": "internal",
            "commit": "b" * 40,
            "body": "Internal implementation detail.",
        }]

        data = MODEL.build_data_json(
            prs,
            {"version": "4.150.1", "status": "preview", "shipments": []},
        )

        self.assertNotIn("body", data["prs"]["101"])
        self.assertEqual(data["contributors"], [])

    def test_commit_body_cannot_forge_a_second_pr_record(self):
        commit = "a" * 40
        forged_separator = (
            "Real body text.\n---COMMIT-END-7f3b---\n"
            + ("b" * 40)
            + "\n1096616+mattleibow@users.noreply.github.com\n"
            "Matthew Leibowitz\n"
            "[skia-sync] Merge upstream changes (#4379)\n"
            "Injected instructions."
        )
        log = "\0".join([
            commit,
            "contributor@example.com",
            "Contributor",
            "Real product fix (#9999)",
            forged_separator,
            "",
        ])

        with patch.object(COMMON, "run", return_value=log), patch.object(
            SOURCES, "_files_by_commit", return_value={commit: {"binding/Fix.cs"}}
        ):
            prs = SOURCES.get_prs_from_diff("before", "after")

        self.assertEqual([pr["number"] for pr in prs], [9999])
        self.assertEqual(prs[0]["body"], forged_separator)

    def test_hotfix_preview_and_stable_use_exact_predecessors(self):
        items = self.collect("4.151.1", "4.151.0")
        self.assertEqual(
            [s["tag"] for s in items],
            ["v4.151.1-preview.1.1", "v4.151.1"],
        )
        self.assertEqual(items[0]["previous_tag"], "v4.151.0")
        self.assertEqual(items[1]["previous_tag"], items[0]["tag"])
        self.assertEqual(items[1]["prs"], [105])


class ReleaseSummaryValidationAndRenderingTests(unittest.TestCase):
    def test_api_diff_links_require_generated_landing_pages(self):
        data = page_data([])
        data["api_links"] = [{
            "label": "HarfBuzzSharp API diff",
            "href": "harfbuzzsharp/8.3.1.6/index.md",
        }]
        prose = cumulative_prose({})

        with tempfile.TemporaryDirectory() as tmp:
            releases = Path(tmp)
            with patch.object(RENDER, "RELEASES_DIR", releases):
                errors = RENDER.validate(data, prose)
                self.assertTrue(any(
                    "data.api_links target does not exist" in error
                    for error in errors
                ))

                target = releases / data["api_links"][0]["href"]
                target.parent.mkdir(parents=True)
                target.write_text("# API diff: 8.3.1.6\n")
                self.assertFalse(any(
                    "data.api_links target does not exist" in error
                    for error in RENDER.validate(data, prose)
                ))

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

    def test_harfbuzz_summary_renders_deterministic_pr_credit(self):
        data = page_data([])
        data["prs"]["101"] = {
            "url": "https://github.com/mono/SkiaSharp/pull/101",
            "title": "Update HarfBuzz",
            "author": "contributor",
            "community": True,
            "tag": "product",
        }
        data["harfbuzz"] = {
            "version": "14.2.0",
            "previous_version": "8.3.1.6",
            "prs": [101],
        }
        prose = cumulative_prose({})
        prose["harfbuzz_summary"] = (
            "Updates the bundled HarfBuzz from 8.3.1.6 to 14.2.0."
        )

        rendered = RENDER.render(data, prose)

        self.assertIn(
            "HarfBuzz from 8.3.1.6 to 14.2.0. — ❤️ "
            "[@contributor](https://github.com/contributor) "
            "([#101](https://github.com/mono/SkiaSharp/pull/101))",
            rendered,
        )

    def test_harfbuzz_summary_rejects_shortened_version(self):
        data = page_data([])
        data["harfbuzz"] = {
            "version": "14.2.0",
            "previous_version": "8.3.1.5",
            "prs": [],
        }
        prose = cumulative_prose({})
        prose["harfbuzz_summary"] = (
            "Updates the bundled HarfBuzz from 8.3.1 to 14.2.0."
        )

        errors = RENDER.validate(data, prose)
        self.assertTrue(any(
            "version values absent from data.harfbuzz: 8.3.1" in error
            for error in errors
        ))

        prose["harfbuzz_summary"] = (
            "Updates the bundled HarfBuzz from 8.3.1.5 to 14.2.0."
        )
        self.assertEqual(RENDER.validate(data, prose), [])

    def test_unchanged_harfbuzz_omits_empty_narrative(self):
        data = page_data([])
        data["harfbuzz"] = {
            "version": "14.2.1",
            "previous_version": "14.2.1",
            "prs": [],
        }
        prose = cumulative_prose({})

        self.assertNotIn("## HarfBuzzSharp", RENDER.render(data, prose))

    def test_legacy_harfbuzz_no_change_sentence_is_preserved(self):
        data = page_data([])
        data["format"] = 3
        data["harfbuzz"] = {
            "version": "14.2.1",
            "prs": [],
        }
        prose = cumulative_prose({})
        prose["preview_summaries"] = {}
        prose["harfbuzz_summary"] = (
            "Legacy prose that the reviewed page did not render."
        )

        rendered = RENDER.render(data, prose)
        self.assertIn(
            RENDER.LEGACY_NO_HARFBUZZ_CHANGES,
            rendered,
        )
        self.assertNotIn(prose["harfbuzz_summary"], rendered)

    def test_preview_uses_release_summary(self):
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
            "release_summaries": {
                item["tag"]: release_summary(
                    [101], summary="Adds the exact shipment feature."
                )
            },
        }
        self.assertEqual(RENDER.validate(data, prose), [])
        text = RENDER.render(data, prose)
        self.assertIn("Adds the exact shipment feature.", text)
        self.assertIn("#101", text)

    def test_rc_github_summary_is_escaped_counted_and_credited(self):
        item = shipment("v4.151.0-rc.1.1", [103], channel="rc")
        data = page_data([item])
        prose = cumulative_prose({
            item["tag"]: release_summary(
                [103],
                summary="Tests [RC] *safely*.",
            )
        })
        text = RENDER.render_github_release_summary(data, prose, item["tag"])
        self.assertIn(r"Tests \[RC\] \*safely\*.", text)
        self.assertIn("[@contributor]", text)
        self.assertIn("1 pull request · 1 consumer-facing", text)

    def test_stable_summary_uses_cumulative_highlights(self):
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
            "release_summaries": {},
        }
        self.assertEqual(RENDER.validate(data, prose), [])
        text = RENDER.render_github_release_summary(data, prose, item["tag"])
        self.assertIn("The stable package now ships.", text)
        self.assertIn("0 pull requests · 0 consumer-facing", text)

    def test_release_summary_prs_must_be_subset_of_exact_scope(self):
        item = shipment("v4.151.0-preview.1.1", [101])
        data = page_data([item])
        prose = cumulative_prose({
            item["tag"]: release_summary([999]),
        })
        errors = RENDER.validate(data, prose)
        self.assertTrue(any("outside its exact scope" in error for error in errors))

    def test_release_summary_must_ground_exact_breaking_prs(self):
        item = shipment("v4.151.0-rc.1.1", [101, 102], channel="rc")
        data = page_data([item])
        prose = cumulative_prose({
            item["tag"]: release_summary([102]),
        })
        prose["breaking"] = [{
            "title": "Breaking API",
            "body": "Migrate to the replacement API.",
            "prs": [101],
        }]

        errors = RENDER.validate(data, prose)
        self.assertTrue(any(
            "missing: #101" in error
            for error in errors
        ))

        prose["release_summaries"][item["tag"]]["prs"].append(101)
        self.assertEqual(RENDER.validate(data, prose), [])

    def test_security_category_requires_explicit_security_evidence(self):
        data = page_data([
            shipment("v4.151.0-preview.1.1", [101, 102]),
        ])
        data["prs"]["101"]["body"] = (
            "Updates libexpat with upstream security hardening."
        )
        data["prs"]["102"]["body"] = "Updates libpng to the latest release."
        prose = cumulative_prose({
            "v4.151.0-preview.1.1": release_summary([101, 102]),
        })
        prose["categories"] = [{
            "heading": "Security",
            "bullets": [{
                "lead": "Native dependencies updated",
                "detail": "Updates bundled parsers and codecs.",
                "prs": [101, 102],
            }],
        }]

        errors = RENDER.validate(data, prose)
        self.assertTrue(any(
            "without explicit security evidence: #102" in error
            for error in errors
        ))

        prose["categories"][0]["bullets"][0]["prs"] = [101]
        self.assertEqual(RENDER.validate(data, prose), [])

    def test_security_evidence_accepts_standard_explicit_wording(self):
        for text in (
            "Addresses CVE.",
            "Addresses CVE-2026-1234.",
            "Publishes an advisory.",
            "Includes upstream security fixes.",
            "Includes security updates and advisories.",
            "Fixes two vulnerabilities.",
            "Applies security patches.",
        ):
            self.assertTrue(COMMON.has_security_evidence("", text), text)
        self.assertFalse(COMMON.has_security_evidence(
            "Update libpng",
            "Updates to the latest upstream release.",
        ))

    def test_legacy_security_prose_does_not_require_new_evidence_field(self):
        data = page_data([
            shipment("v4.151.0-preview.1.1", [101]),
        ])
        data["format"] = 3
        prose = cumulative_prose({})
        prose["release_summaries"] = {}
        prose["preview_summaries"] = {
            "v4.151.0-preview.1.1": "Updates a bundled dependency.",
        }
        prose["categories"] = [{
            "heading": "Security",
            "bullets": [{
                "lead": "Bundled dependency updated",
                "detail": "Updates the native dependency.",
                "prs": [101],
            }],
        }]

        self.assertEqual(RENDER.validate(data, prose), [])

    def test_exact_build_scope_replaces_rollup_milestone_scope(self):
        item = shipment("v4.151.0-preview.1.2", [102])
        data = page_data([item])
        data["prs"]["101"] = {
            "url": "https://github.com/mono/SkiaSharp/pull/101",
            "title": "Earlier build",
            "author": "contributor",
            "community": True,
            "tag": "product",
        }
        data["previews"] = [{
            "key": item["tag"],
            "label": "Preview 1",
            "prs": [101, 102],
        }]
        prose = cumulative_prose({
            item["tag"]: release_summary([101]),
        })

        errors = RENDER.validate(data, prose)

        self.assertTrue(any("outside its exact scope" in error for error in errors))

    def test_semantic_category_choices_are_not_script_policy(self):
        item = shipment("v4.151.0-preview.1.1", [101])
        data = page_data([item])
        data["prs"]["101"]["tag"] = "internal"
        prose = cumulative_prose({
            item["tag"]: release_summary([101]),
        })
        prose["theme"] = "Consumer release"
        prose["highlights_headline"] = "Consumer improvements ship."
        prose["categories"] = [{
            "heading": "Platform",
            "bullets": [{
                "lead": "CI mechanics",
                "detail": "Adds a containerized test leg.",
                "prs": [101],
            }],
        }]

        errors = RENDER.validate(data, prose)

        self.assertFalse(any("internal" in error.casefold() for error in errors))

    def test_sync_round_wording_is_not_counted_by_renderer(self):
        item = shipment("v4.150.2", [101, 102], channel="stable")
        data = page_data([item])
        data["version"] = "4.150.2"
        data["prs"]["101"]["title"] = "[skia-sync] Merge upstream bug fixes"
        data["prs"]["102"]["title"] = "[skia-sync] Update with Ganesh fixes"
        prose = cumulative_prose({})
        prose["theme"] = "Three rounds of Skia fixes"
        prose["categories"] = [{
            "heading": "Engine",
            "bullets": [{
                "lead": "Engine updates",
                "detail": "Upstream updates.",
                "prs": [101, 102],
            }],
        }]

        errors = RENDER.validate(data, prose)

        self.assertFalse(any("round" in error.casefold() for error in errors))

    def test_duplicate_exact_tags_are_rejected(self):
        item = shipment("v4.151.0-preview.1.1", [101])
        errors = COMMON.validate_shipments(page_data([item, deepcopy(item)]))
        self.assertEqual(
            errors,
            ["duplicate exact shipment tags: v4.151.0-preview.1.1"],
        )

class PreservationLifecycleTests(unittest.TestCase):
    def test_changed_facts_delete_full_prose_and_queue_page(self):
        with tempfile.TemporaryDirectory() as directory:
            page = Path(directory) / "4.150.0.md"
            data_path = COMMON.data_json_path(page)
            context_path = COMMON.context_markdown_path(page)
            prose_path = COMMON.prose_json_path(page)
            data_path.parent.mkdir(parents=True)
            data_path.write_text(json.dumps({"version": "old"}) + "\n")
            prose_path.write_text(json.dumps(cumulative_prose({})) + "\n")

            result = MODEL.write_page_outputs(
                page, {"version": "4.150.0"}, 3
            )

            self.assertEqual(result, str(context_path))
            self.assertTrue(context_path.exists())
            self.assertFalse(prose_path.exists())
            self.assertEqual(json.loads(data_path.read_text()),
                             {"version": "4.150.0"})

    def test_unchanged_facts_keep_prose_and_do_not_queue_page(self):
        with tempfile.TemporaryDirectory() as directory:
            page = Path(directory) / "4.150.0.md"
            data_path = COMMON.data_json_path(page)
            context_path = COMMON.context_markdown_path(page)
            prose_path = COMMON.prose_json_path(page)
            data = {"version": "4.150.0"}
            data_path.parent.mkdir(parents=True)
            data_path.write_text(json.dumps(data, indent=2) + "\n")
            context_path.write_text(MODEL.render_agent_context(data, page))
            prose_path.write_text(json.dumps(cumulative_prose({})) + "\n")

            result = MODEL.write_page_outputs(page, data, 3)

            self.assertIsNone(result)
            self.assertTrue(prose_path.exists())

    def test_force_deletes_full_prose_and_queues_unchanged_page(self):
        with tempfile.TemporaryDirectory() as directory:
            page = Path(directory) / "4.150.0.md"
            data_path = COMMON.data_json_path(page)
            context_path = COMMON.context_markdown_path(page)
            prose_path = COMMON.prose_json_path(page)
            data = {"version": "4.150.0"}
            data_path.parent.mkdir(parents=True)
            data_path.write_text(json.dumps(data, indent=2) + "\n")
            prose_path.write_text(json.dumps(cumulative_prose({})) + "\n")

            result = MODEL.write_page_outputs(page, data, 3, force=True)

            self.assertEqual(result, str(context_path))
            self.assertFalse(prose_path.exists())

    def test_missing_prose_requeues_existing_atomic_context(self):
        with tempfile.TemporaryDirectory() as directory:
            page = Path(directory) / "4.150.0.md"
            data = {"version": "4.150.0"}
            data_path = COMMON.data_json_path(page)
            context_path = COMMON.context_markdown_path(page)
            data_path.parent.mkdir(parents=True)
            data_path.write_text(json.dumps(data, indent=2) + "\n")
            context_path.write_text(MODEL.render_agent_context(data, page))

            result = MODEL.write_page_outputs(page, data, 3)

            self.assertEqual(result, str(context_path))
            self.assertFalse(data_path.with_name(data_path.name + ".tmp").exists())
            self.assertFalse(
                context_path.with_name(context_path.name + ".tmp").exists()
            )

    def test_cumulative_prose_only_edit_does_not_change_prerelease_summary(self):
        item = shipment("v4.151.0-preview.1.1", [101])
        data = page_data([item])
        before = cumulative_prose({
            item["tag"]: release_summary([101]),
        })
        before["theme"] = "Before"
        after = deepcopy(before)
        after["theme"] = "After"
        after["categories"] = [{"heading": "Security", "bullets": []}]
        self.assertEqual(
            RENDER.render_github_release_summary(data, before, item["tag"]),
            RENDER.render_github_release_summary(data, after, item["tag"]),
        )

    def test_polish_list_contains_one_context_path_per_line(self):
        path = ROOT / "output/release-notes-polish-list-test.txt"
        pages = [
            "documentation/docfx/releases/_sources/4.150.0.context.md",
            "documentation/docfx/releases/_sources/4.150.1.context.md",
        ]
        try:
            COMMON.write_polish_list(pages, path)
            self.assertEqual(path.read_text().splitlines(), pages)
        finally:
            if path.exists():
                path.unlink()

    def test_empty_polish_list_is_an_empty_file(self):
        path = ROOT / "output/release-notes-polish-list-test.txt"
        try:
            COMMON.write_polish_list([], path)
            self.assertEqual(path.read_text(), "")
        finally:
            if path.exists():
                path.unlink()

    def test_malformed_release_summary_reports_validation_errors(self):
        item = shipment("v4.151.0-preview.1.1", [101])
        data = page_data([item])
        prose = cumulative_prose({
            item["tag"]: {
                "summary": "Adds useful package updates.",
                "prs": [101, 101, "bad"],
            }
        })
        errors = RENDER.validate(data, prose)
        self.assertTrue(any("positive integers" in error for error in errors))


class ScopedPruningTests(unittest.TestCase):
    def test_scoped_render_preserves_out_of_range_unreleased_pages(self):
        with tempfile.TemporaryDirectory() as directory:
            releases = Path(directory)
            pages = [
                releases / "4.150.2-unreleased.md",
                releases / "4.151.0-unreleased.md",
            ]
            for page in pages:
                page.write_text("page")
                COMMON.data_json_path(page).parent.mkdir(parents=True, exist_ok=True)
                COMMON.data_json_path(page).write_text("{}")
                COMMON.context_markdown_path(page).write_text("context")
                COMMON.prose_json_path(page).write_text("{}")

            with patch.object(RENDER, "RELEASES_DIR", releases):
                pruned = RENDER._prune_stale_unreleased(
                    {"4.152.0"},
                    min_core=COMMON.core_tuple("4.150.0"),
                    max_core=COMMON.core_tuple("4.150.0"),
                )
                self.assertEqual(pruned, 0)
                self.assertTrue(all(page.exists() for page in pages))

                pruned = RENDER._prune_stale_unreleased(
                    {"4.152.0"},
                    min_core=COMMON.core_tuple("4.150.2"),
                    max_core=COMMON.core_tuple("4.150.2"),
                )
                self.assertEqual(pruned, 1)
                self.assertFalse(pages[0].exists())
                self.assertFalse(COMMON.context_markdown_path(pages[0]).exists())
                self.assertTrue(pages[1].exists())


class ApiDiffLifecycleTests(unittest.TestCase):
    def test_scoped_rebuild_clears_shared_line_once(self):
        source = (ROOT / "scripts/infra/docs/api-diff.cake").read_text()
        tracker = source.index("var clearedLineDirs = new HashSet<string>")
        package_loop = source.index("foreach (var id in packageIds)")
        clear_guard = source.index("clearedLineDirs.Add (lineDir.FullPath)")
        clear_call = source.index(
            "ClearGeneratedApiDiffsIn (lineDir.FullPath)", clear_guard
        )

        self.assertLess(tracker, package_loop)
        self.assertLess(package_loop, clear_guard)
        self.assertLess(clear_guard, clear_call)

    def test_scoped_harfbuzz_uses_co_release_versions(self):
        source = (ROOT / "scripts/infra/docs/api-diff.cake").read_text()
        self.assertIn(
            ".OrderBy (id => IsHarfBuzzFamily (id) ? 1 : 0)",
            source,
        )
        self.assertIn("FamilyCoreInRange (", source)
        self.assertIn(
            "CoreInRange (kvp.Key, minVersion, maxVersion)",
            source,
        )
        self.assertIn(
            "string.Equals (kvp.Value, core",
            source,
        )
        self.assertIn(
            "feedSkiaHarfBuzzLines.Add (apiDiffVersion)",
            source,
        )
        self.assertIn(
            "!feedSkiaHarfBuzzLines.Contains (inflightSkia)",
            source,
        )
        self.assertIn(
            "dir, name, null, existingApiDiffFiles",
            source,
        )
        self.assertIn(
            "IsCoReleasedHarfBuzzLine (l.key, skiaHarfBuzzDeps)",
            source,
        )
        self.assertIn(
            "skiaHarfBuzzDeps.Values.Any",
            source,
        )
        self.assertIn(
            "missingCoReleaseApiDiff",
            source,
        )
        self.assertIn(
            "&& !missingCoReleaseApiDiff",
            source,
        )

    def test_api_diff_markdown_is_normalized_before_commit(self):
        source = (ROOT / "scripts/infra/docs/api-diff.cake").read_text()
        self.assertIn(
            "var isNewIndex = !existingApiDiffFiles.Contains (indexPath.FullPath)",
            source,
        )
        self.assertIn(
            "if (isNewIndex)",
            source,
        )
        self.assertIn(
            "NormalizeGeneratedMarkdown (indexPath)",
            source,
        )
        self.assertIn(
            "NormalizeGeneratedMarkdown (apiDiffPath)",
            source,
        )
        self.assertIn(
            "var existingApiDiffFiles = DirectoryExists (RELEASES_PATH)",
            source,
        )
        self.assertIn(
            "var isNewApiDiff = !existingApiDiffFiles.Contains (apiDiffPath.FullPath)",
            source,
        )
        self.assertIn(
            "if (isNewApiDiff)",
            source,
        )
        self.assertNotIn(
            "NormalizeGeneratedMarkdown (file)",
            source,
        )
        self.assertIn(
            "text.TrimEnd ('\\r', '\\n') + Environment.NewLine",
            source,
        )


if __name__ == "__main__":
    unittest.main()
