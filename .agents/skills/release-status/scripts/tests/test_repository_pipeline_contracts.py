#!/usr/bin/env python3

import json
from pathlib import Path
import unittest


ROOT = Path(__file__).resolve().parents[5]


class RepositoryPipelineContractTests(unittest.TestCase):
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

    def test_tsa_routes_to_dnceng_internal(self):
        data = json.loads(
            (
                ROOT / "scripts" / "infra" / "security" / "tsaoptions-v2.json"
            ).read_text(encoding="utf-8")
        )
        self.assertEqual(data["instanceUrl"], "https://dev.azure.com/dnceng/")
        self.assertEqual(data["projectName"], "internal")
        self.assertEqual(
            data["areaPath"],
            r"internal\Dotnet-Core-Engineering",
        )


if __name__ == "__main__":
    unittest.main()
