"""Keep strict iOS Appium profiles on the reviewed CI image."""

from pathlib import Path
import unittest

import yaml


SCRIPTS = Path(__file__).resolve().parents[3] / "scripts"
CONFIG = yaml.safe_load((SCRIPTS / "azure-templates-stages-samples.yml").read_text())
PARAMETERS = {parameter["name"]: parameter for parameter in CONFIG["parameters"]}
JOBS = CONFIG["stages"][0]["jobs"]


class IosAppiumPolicyTests(unittest.TestCase):
    def test_dedicated_pool_uses_reviewed_tahoe_image(self):
        self.assertEqual(
            {"pool": {
                "name": "AcesShared",
                "demands": "ImageOverride -equals ACES_VM_SharedPool_Tahoe",
                "os": "macos",
            }},
            PARAMETERS["buildAgentIosAppium"]["default"],
        )

    def test_only_ios_appium_jobs_use_dedicated_pool(self):
        mac = next(job["parameters"] for job in JOBS
                   if job.get("parameters", {}).get("platform") == "macos")
        self.assertEqual("${{ parameters.buildAgentMac }}", mac["buildAgent"])
        build = next(job["parameters"] for job in JOBS
                     if job.get("parameters", {}).get("phase") == "build")
        runtime = next(job["${{ each ios in parameters.iosSimulators }}"][0]["parameters"]
                       for job in JOBS if "${{ each ios in parameters.iosSimulators }}" in job)
        for job in (build, runtime):
            self.assertEqual("${{ parameters.buildAgentIosAppium }}", job["buildAgent"])
            self.assertEqual("ios", job["platform"])
        self.assertNotIn("variables", build)
        self.assertEqual("26.6", runtime["variables"]["XCODE_VERSION"])

    def test_exact_runtime_builds_are_installed_without_latest(self):
        profiles = PARAMETERS["iosSimulators"]["default"]
        self.assertEqual(
            [("18.6", "22G86"), ("26.5", "23F77")],
            [(profile["version"], profile["buildVersion"]) for profile in profiles],
        )
        for profile in profiles:
            self.assertEqual("iPhone 16 Pro", profile["model"])
            self.assertIs(True, profile["installRuntime"])
        runtime = next(job["${{ each ios in parameters.iosSimulators }}"][0]["parameters"]
                       for job in JOBS if "${{ each ios in parameters.iosSimulators }}" in job)
        install = runtime["preBuildSteps"][0]["${{ if eq(ios.installRuntime, true) }}"][0]
        self.assertEqual(
            "./scripts/infra/managed/install-ios-runtime.ps1 "
            "-Version '${{ ios.version }}' -BuildVersion '${{ ios.buildVersion }}'",
            install["pwsh"],
        )


if __name__ == "__main__":
    unittest.main()
