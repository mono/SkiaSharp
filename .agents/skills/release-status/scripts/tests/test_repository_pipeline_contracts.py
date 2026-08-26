#!/usr/bin/env python3

import importlib.util
import json
from pathlib import Path
import sys
import unittest


ROOT = Path(__file__).resolve().parents[5]
SCRIPT_PATH = Path(__file__).resolve().parent.parent / "pipeline-status.py"
SPEC = importlib.util.spec_from_file_location(
    "pipeline_status_contracts",
    SCRIPT_PATH,
)
status = importlib.util.module_from_spec(SPEC)
sys.modules[SPEC.name] = status
SPEC.loader.exec_module(status)


class RepositoryPipelineContractTests(unittest.TestCase):
    def test_release_branches_use_arcade_servicing_pool_providers(self):
        contracts = {
            "scripts/azure-pipelines-complete.yml": (
                "/eng/common/templates/variables/pool-providers.yml@self",
                "$(DncEngPublicBuildPool)",
            ),
            "scripts/azure-pipelines-package.yml": (
                "/eng/common/templates-official/variables/"
                "pool-providers.yml@self",
                "$(DncEngInternalBuildPool)",
            ),
        }
        for relative_path, (provider, pool) in contracts.items():
            with self.subTest(path=relative_path):
                source = (ROOT / relative_path).read_text(encoding="utf-8")
                self.assertIn(provider, source)
                self.assertGreaterEqual(source.count(pool), 5)
                self.assertNotIn("NetCore1ESPool-Internal", source)

        release_script = (
            ROOT
            / ".agents"
            / "skills"
            / "release-branch"
            / "scripts"
            / "create-release-branches.py"
        ).read_text(encoding="utf-8")
        self.assertNotIn("DncEngPublicBuildPool", release_script)
        self.assertNotIn("DncEngInternalBuildPool", release_script)
        self.assertNotIn("NetCore-Svc-", release_script)

    def test_repository_feed_routing_uses_dotnet_libraries(self):
        product_paths = (
            ".agents/skills/release-testing/references/setup.md",
            "tests/SkiaSharp.Tests.Integration/nuget.config",
        )
        transport_paths = (
            "documentation/dev/building-samples.md",
            "scripts/infra/shared/shared.cake",
        )
        for relative_path in product_paths:
            with self.subTest(path=relative_path):
                source = (ROOT / relative_path).read_text(encoding="utf-8")
                comparable = source.replace('"\n    "', "")
                self.assertIn("/_packaging/dotnet-libraries/", comparable)
                self.assertNotIn("/_packaging/skiasharp/", source)
                self.assertNotIn("aka.ms/skiasharp-eap", source)
        for relative_path in (
            "benchmarks/SkiaSharp.Benchmarks.Tracking/nuget.config",
            "scripts/infra/perf/_common.py",
        ):
            with self.subTest(path=relative_path):
                source = (ROOT / relative_path).read_text(encoding="utf-8")
                comparable = source.replace('"\n    "', "")
                self.assertIn("/_packaging/dotnet-libraries/", comparable)
                self.assertNotIn("aka.ms/skiasharp-eap", source)
        for relative_path in transport_paths:
            with self.subTest(path=relative_path):
                source = (ROOT / relative_path).read_text(encoding="utf-8")
                self.assertIn(
                    "/_packaging/dotnet-libraries-transport/",
                    source,
                )
                self.assertNotIn("/_packaging/skiasharp-transport/", source)

        mirror = (
            ROOT / "scripts" / "infra" / "package" / "manage-nuget-feed.ps1"
        ).read_text(encoding="utf-8")
        self.assertIn(
            '[ValidateSet("dotnet-libraries", '
            '"dotnet-libraries-transport")]',
            mirror,
        )
        self.assertIn("DestFeed   = 'dotnet-libraries'", mirror)
        self.assertIn("DestFeed   = 'dotnet-libraries-transport'", mirror)
        workflow = (
            ROOT / ".github" / "workflows" / "manage-nuget-feed.yml"
        ).read_text(encoding="utf-8")
        self.assertIn(
            "feed: [dotnet-libraries, dotnet-libraries-transport]",
            workflow,
        )
        self.assertNotIn("skiasharp-ci", workflow.lower())

    def test_tests_pipeline_uses_folder_qualified_build_resource(self):
        source = (
            ROOT / "scripts" / "azure-pipelines-tests.yml"
        ).read_text(encoding="utf-8")
        self.assertIn(
            r"source: '\dotnet\skiasharp\skiasharp-package'",
            source,
        )
        self.assertIn("trigger: true", source)

    def test_artifact_download_rejects_mutable_latest_selector(self):
        source = (
            ROOT
            / "scripts"
            / "azure-templates-steps-download-artifacts.yml"
        ).read_text(encoding="utf-8")
        self.assertIn(
            "Mutable latestFromBranch artifact selection is not supported",
            source,
        )
        self.assertNotIn("$versionType = 'latestFromBranch'", source)

    def test_tsa_tracks_work_in_devdiv(self):
        data = json.loads(
            (
                ROOT / "scripts" / "infra" / "security" / "tsaoptions-v2.json"
            ).read_text(encoding="utf-8")
        )
        self.assertEqual(
            data["instanceUrl"],
            "https://devdiv.visualstudio.com/",
        )
        self.assertEqual(data["projectName"], "DevDiv")
        self.assertEqual(
            data["areaPath"],
            r"DevDiv\.NET MAUI\SkiaSharp",
        )
        self.assertEqual(data["iterationPath"], "DevDiv")

    def test_migration_requirement_set_cannot_silently_shrink(self):
        self.assertEqual(
            {requirement["id"] for requirement in status.MIGRATION_REQUIREMENTS},
            {
                "combined-build",
                "connected-tests",
                "exact-artifact-selection",
                "exact-release-versioning",
                "package-output-root",
                "cake-arcade-assets",
                "single-transport-family",
                "real-pdb-artifacts",
                "pdb-escape-contract-test",
                "expected-failure-exit-reset",
                "top-level-arcade-assembly",
                "prepare-tool-free",
                "package-cake-behavior-test",
                "public-arcade-artifacts",
                "internal-arcade-publishing",
                "no-powershell-asset-assembler",
                "transport-download-family",
                "NativeAssets-transport-metadata",
                "NuGets-transport-metadata",
                "Dependencies-transport-metadata",
            },
        )
        serialized = json.dumps(status.MIGRATION_REQUIREMENTS).lower()
        self.assertNotIn("apple-symbol", serialized)
        self.assertNotIn("dsym", serialized)

    def test_current_tree_satisfies_minimum_release_backport(self):
        report = status.GitRepository(ROOT).release_prerequisites("HEAD")
        self.assertEqual(report, {"state": "ready", "missing": []})


if __name__ == "__main__":
    unittest.main()
