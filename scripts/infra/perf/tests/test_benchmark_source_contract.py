#!/usr/bin/env python3

import re
import unittest
from pathlib import Path


REPO_ROOT = Path(__file__).resolve().parents[4]
SHARED_TRANSPORT_FEED = (
    "https://pkgs.dev.azure.com/dnceng/public/"
    "_packaging/dotnet-libraries-transport/nuget/v3/index.json"
)
PRODUCT_TRANSPORT_FEED = (
    "https://pkgs.dev.azure.com/dnceng/public/"
    "_packaging/skiasharp-transport/nuget/v3/index.json"
)


class BenchmarkSourceContractTests(unittest.TestCase):
    def test_shared_download_uses_channel_transport_feed(self):
        shared = (REPO_ROOT / "scripts/infra/shared/shared.cake").read_text()
        match = re.search(
            r'CI_ARTIFACTS_FEED_URL\s*=\s*Argument\s*'
            r'\(\s*"previewFeed"\s*,\s*"([^"]+)"\s*\)',
            shared,
        )

        self.assertIsNotNone(match, "The shared previewFeed default was not found.")
        self.assertEqual(SHARED_TRANSPORT_FEED, match.group(1))
        self.assertNotEqual(PRODUCT_TRANSPORT_FEED, match.group(1))

    def test_download_query_and_extraction_share_one_feed(self):
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

    def test_benchmark_fast_path_uses_shared_download_contract(self):
        workflow = (REPO_ROOT / ".github/workflows/track-benchmarks.yml").read_text()
        match = re.search(
            r"- name: Download prebuilt natives \(fast path\)(.*?)"
            r"(?=\n\s+- name:)",
            workflow,
            re.DOTALL,
        )

        self.assertIsNotNone(match, "The benchmark source fast path was not found.")
        self.assertIn("dotnet cake --target=externals-download", match.group(1))
        self.assertNotIn("--previewFeed", match.group(1))


if __name__ == "__main__":
    unittest.main()
