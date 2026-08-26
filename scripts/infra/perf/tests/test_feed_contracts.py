#!/usr/bin/env python3

import re
import sys
import unittest
import urllib.error
import importlib.util
from pathlib import Path
from unittest import mock


REPO_ROOT = Path(__file__).resolve().parents[4]
PERF_ROOT = REPO_ROOT / "scripts" / "infra" / "perf"
sys.path.insert(0, str(PERF_ROOT))

import _common as common  # noqa: E402

SIZES_TRACK_PATH = PERF_ROOT / "sizes" / "track.py"
SIZES_SPEC = importlib.util.spec_from_file_location(
    "size_tracker_feed_contracts",
    SIZES_TRACK_PATH,
)
sizes = importlib.util.module_from_spec(SIZES_SPEC)
SIZES_SPEC.loader.exec_module(sizes)


PRODUCT_FEED = (
    "https://pkgs.dev.azure.com/dnceng/public/"
    "_packaging/dotnet-libraries/nuget/v3/index.json"
)
TRANSPORT_FEED = (
    "https://pkgs.dev.azure.com/dnceng/public/"
    "_packaging/dotnet-libraries-transport/nuget/v3/index.json"
)


class FeedContractTests(unittest.TestCase):
    def test_primary_product_and_transport_routes_are_dotnet_libraries(self):
        self.assertEqual(common.SIGNED_BUILDS_INDEX_URL, PRODUCT_FEED)
        shared = (REPO_ROOT / "scripts/infra/shared/shared.cake").read_text()
        match = re.search(
            r'CI_ARTIFACTS_FEED_URL\s*=\s*Argument\s*'
            r'\(\s*"previewFeed"\s*,\s*"([^"]+)"\s*\)',
            shared,
        )
        self.assertIsNotNone(match)
        self.assertEqual(TRANSPORT_FEED, match.group(1))

    def test_download_query_and_extraction_share_transport_feed(self):
        download = (REPO_ROOT / "scripts/infra/shared/download.cake").read_text()
        self.assertRegex(download, r"SourceUrl\s*=\s*CI_ARTIFACTS_FEED_URL")
        self.assertRegex(
            download,
            r"new\s+NuGetDiff\s*\(\s*CI_ARTIFACTS_FEED_URL\s*\)",
        )

    def test_identity_free_download_selects_main_native_family(self):
        download = (REPO_ROOT / "scripts/infra/shared/download.cake").read_text()
        externals = (
            REPO_ROOT / "scripts/infra/managed/externals-download.cake"
        ).read_text()
        self.assertRegex(download, r'else\s+version\s*\+=\s*"branch\.main"')
        self.assertRegex(download, r'version\s*\+=\s*"\.\*"')
        self.assertIn('DownloadPackageAsync("_nativeassets"', externals)

    def test_benchmark_fast_path_uses_shared_transport_contract(self):
        workflow = (
            REPO_ROOT / ".github/workflows/track-benchmarks.yml"
        ).read_text()
        match = re.search(
            r"- name: Download prebuilt natives \(fast path\)(.*?)"
            r"(?=\n\s+- name:)",
            workflow,
            re.DOTALL,
        )
        self.assertIsNotNone(match)
        self.assertIn("dotnet cake --target=externals-download", match.group(1))
        self.assertNotIn("--previewFeed", match.group(1))

    def test_primary_nightly_prevents_legacy_fallback(self):
        with (
            mock.patch.object(
                common,
                "signed_build_versions",
                return_value=["4.152.0-nightly.2"],
            ),
            mock.patch.object(
                common,
                "versions_from_index",
            ) as legacy,
            mock.patch.object(common, "nuget_versions", return_value=[]),
        ):
            roles = common.resolve_roles()
        self.assertEqual(roles["nightly"], "4.152.0-nightly.2")
        legacy.assert_not_called()

    def test_missing_primary_package_does_not_retry_404(self):
        error = urllib.error.HTTPError(
            PRODUCT_FEED,
            404,
            "Not Found",
            {},
            None,
        )
        with (
            mock.patch.object(
                common.urllib.request,
                "urlopen",
                side_effect=error,
            ) as request,
            mock.patch.object(common.time, "sleep") as sleep,
        ):
            with self.assertRaisesRegex(RuntimeError, "404"):
                common.http_get(PRODUCT_FEED)
        request.assert_called_once()
        sleep.assert_not_called()

    def test_legacy_fallback_is_nightly_only_and_primary_first(self):
        with (
            mock.patch.object(
                common,
                "signed_build_versions",
                return_value=[],
            ),
            mock.patch.object(
                common,
                "versions_from_index",
                return_value=["4.152.0-nightly.1"],
            ) as legacy,
            mock.patch.object(
                common,
                "nuget_versions",
                return_value=["4.151.0"],
            ),
        ):
            roles = common.resolve_roles()
        legacy.assert_called_once_with(
            common.LEGACY_NIGHTLY_INDEX_URL,
            "SkiaSharp",
        )
        self.assertEqual(roles["nightly"], "4.152.0-nightly.1")
        self.assertEqual(roles["latest"], "4.151.0")

    def test_size_tracker_probes_primary_before_legacy_nightly(self):
        source = (
            REPO_ROOT / "scripts/infra/perf/sizes/track.py"
        ).read_text()
        primary_probe = source.index("has_complete_nightly_families(feed)")
        legacy_switch = source.index(
            "resolve_signed_build_feed(LEGACY_NIGHTLY_INDEX_URL)"
        )
        collection = source.index("collect_nightly(feed, work_dir)")
        self.assertLess(primary_probe, legacy_switch)
        self.assertLess(legacy_switch, collection)

    def test_size_tracker_requires_both_nightly_families(self):
        feed = {"flat": "https://example.test/flat"}

        def versions(_, package):
            return (
                ["4.152.0-nightly.1"]
                if package == "SkiaSharp"
                else []
            )

        with mock.patch.object(sizes, "feed_versions", side_effect=versions):
            self.assertFalse(sizes.has_complete_nightly_families(feed))
        with mock.patch.object(
            sizes,
            "feed_versions",
            return_value=["4.152.0-nightly.1"],
        ):
            self.assertTrue(sizes.has_complete_nightly_families(feed))

    def test_legacy_product_feed_is_confined_to_tracking_fallback(self):
        allowed = {
            "scripts/infra/perf/_common.py",
            "scripts/infra/perf/tests/test_feed_contracts.py",
            "benchmarks/SkiaSharp.Benchmarks.Tracking/nuget.config",
        }
        legacy = "_packaging/skiasharp/nuget/v3/index.json"
        matches = set()
        for root in (
            REPO_ROOT / ".agents" / "skills",
            REPO_ROOT / "documentation",
            REPO_ROOT / "scripts",
            REPO_ROOT / "tests",
            REPO_ROOT / "benchmarks",
        ):
            for path in root.rglob("*"):
                if path.is_file():
                    try:
                        source = path.read_text(encoding="utf-8")
                    except UnicodeDecodeError:
                        continue
                    if legacy in source:
                        matches.add(path.relative_to(REPO_ROOT).as_posix())
        self.assertEqual(matches, allowed)


if __name__ == "__main__":
    unittest.main()
