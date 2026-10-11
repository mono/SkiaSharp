import json
from pathlib import Path
import unittest
import xml.etree.ElementTree as ET

import yaml


ROOT = Path(__file__).resolve().parents[3]


class WindowsToolchainTests(unittest.TestCase):
    def test_profiles_request_modern_compiler_and_installer_tools(self):
        required = {
            "Microsoft.VisualStudio.Component.VC.Tools.x86.x64",
            "Microsoft.VisualStudio.Component.VC.Tools.ARM64",
            "Microsoft.VisualStudio.Component.VC.Runtimes.x86.x64.Spectre",
            "Microsoft.VisualStudio.Component.VC.Runtimes.ARM64.Spectre",
            "Microsoft.VisualStudio.Component.VC.Llvm.Clang",
            "Microsoft.VisualStudio.Component.VC.CMake.Project",
            "Microsoft.VisualStudio.Component.Windows11SDK.28000",
        }
        for profile in ("source/.vsconfig", "scripts/infra/native/windows/build-tools.vsconfig"):
            with self.subTest(profile=profile):
                components = json.loads((ROOT / profile).read_text())["components"]
                self.assertEqual(len(components), len(set(components)))
                self.assertTrue(required.issubset(components))
                self.assertFalse(any("14.44" in component or "19041" in component
                                     or "v143" in component for component in components))
                self.assertNotIn("Microsoft.VisualStudio.Workload.XamarinBuildTools", components)
        build_tools = json.loads((ROOT / "scripts/infra/native/windows/build-tools.vsconfig").read_text())
        self.assertIn("Microsoft.VisualStudio.Workload.NetCrossPlatBuildTools", build_tools["components"])

    def test_native_projects_use_v145_without_raising_winui_minimum_os(self):
        projects = (
            "native/windows/libHarfBuzzSharp/libHarfBuzzSharp.vcxproj",
            "native/winui/SkiaSharp.Views.WinUI.Native/SkiaSharp.Views.WinUI.Native/"
            "SkiaSharp.Views.WinUI.Native.vcxproj",
        )
        namespace = {"msbuild": "http://schemas.microsoft.com/developer/msbuild/2003"}
        for project in projects:
            with self.subTest(project=project):
                tree = ET.parse(ROOT / project)
                toolsets = tree.findall(".//msbuild:PlatformToolset", namespace)
                self.assertTrue(toolsets)
                self.assertEqual({"v145"}, {toolset.text for toolset in toolsets})
        tree = ET.parse(ROOT / projects[1])
        self.assertEqual("10.0.17763.0",
                         tree.find(".//msbuild:WindowsTargetPlatformMinVersion", namespace).text)
        self.assertFalse((ROOT / "native/winui/global.json").exists())

    def test_angle_pin_matches_component_manifest(self):
        versions = {}
        for line in (ROOT / "scripts/VERSIONS.txt").read_text().splitlines():
            fields = line.split()
            if len(fields) == 3 and fields[0] == "ANGLE":
                versions[fields[1]] = fields[2]
        registrations = json.loads((ROOT / "cgmanifest.json").read_text())["registrations"]
        components = [registration["component"] for registration in registrations]
        angle = next(component["git"] for component in components
                     if component["type"] == "git"
                     and component["git"]["repositoryUrl"] == "https://github.com/google/angle.git")
        branch = next(component["other"] for component in components
                      if component["type"] == "other" and component["other"]["name"] == "angle")
        self.assertRegex(versions["revision"], r"^[0-9a-f]{40}$")
        self.assertEqual(versions["revision"], angle["commitHash"])
        self.assertEqual(versions["release"], branch["version"])

    def test_windows_validation_covers_both_products_and_all_architectures(self):
        workflow = yaml.safe_load((ROOT / ".github/workflows/validate-windows-components.yml").read_text())
        self.assertEqual({"build", "full-vs"}, set(workflow["jobs"]))
        for job in workflow["jobs"].values():
            self.assertEqual("windows-2025-vs2026", job["runs-on"])
            self.assertEqual(["x86", "x64", "arm64"], job["strategy"]["matrix"]["arch"])
            self.assertEqual("2026", job["env"]["VS_VERSION"])
            self.assertEqual("10.0.28000.0", job["env"]["WINDOWS_SDK_VERSION"])
            scripts = "\n".join(step.get("run", "") for step in job["steps"])
            self.assertIn("native/winui-angle/smoke_test.py", scripts)
            self.assertIn("dotnet test tests/SkiaSharp.Tests.Console.slnx", scripts)
            for obsolete in ("10.0.111", "configure_vs2026", "install-llvm", "install-ninja", "--filter"):
                self.assertNotIn(obsolete, scripts)

    def test_producing_pipelines_no_longer_request_old_tools(self):
        bootstrapper = (ROOT / "scripts/azure-templates-jobs-bootstrapper.yml").read_text()
        self.assertIn("install-components.ps1", bootstrapper)
        self.assertIn("NINJA_EXE: $(NINJA_EXE)", bootstrapper)
        self.assertNotIn("install-llvm.ps1", bootstrapper)
        self.assertNotIn("install-winsdk.ps1", bootstrapper)
        self.assertNotIn("DOTNET_VERSION_WINUI",
                         (ROOT / "scripts/azure-templates-stages-native-windows.yml").read_text())
        for pipeline in ("complete", "package", "tests"):
            contents = (ROOT / f"scripts/azure-pipelines-{pipeline}.yml").read_text()
            self.assertNotIn("windows-2022", contents)
            self.assertIn("vs2026", contents)


if __name__ == "__main__":
    unittest.main()
