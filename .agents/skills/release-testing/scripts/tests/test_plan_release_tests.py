#!/usr/bin/env python3

import importlib.util
import io
from pathlib import Path
import shlex
import sys
import unittest
from unittest import mock
import zipfile


SCRIPTS = Path(__file__).resolve().parent.parent
sys.path.insert(0, str(SCRIPTS))
SCRIPT_PATH = SCRIPTS / "plan-release-tests.py"
SPEC = importlib.util.spec_from_file_location("plan_release_tests", SCRIPT_PATH)
planner = importlib.util.module_from_spec(SPEC)
sys.modules[SPEC.name] = planner
SPEC.loader.exec_module(planner)


def load_runner(name: str, filename: str):
    path = SCRIPTS / filename
    spec = importlib.util.spec_from_file_location(name, path)
    module = importlib.util.module_from_spec(spec)
    sys.modules[spec.name] = module
    spec.loader.exec_module(module)
    return module


RUNNERS = {
    "run-host-tests.py": load_runner("run_host_tests", "run-host-tests.py"),
    "run-android-tests.py": load_runner(
        "run_android_tests",
        "run-android-tests.py",
    ),
    "run-ios-tests.py": load_runner(
        "run_ios_tests",
        "run-ios-tests.py",
    ),
}


SKIA_VERSION = "4.152.0-preview.1.1"
HARFBUZZ_VERSION = "14.2.1-preview.1.1"
PUBLIC_PLAN = {
    "receipt": {
        "skiaSharpVersion": SKIA_VERSION,
        "harfBuzzSharpVersion": HARFBUZZ_VERSION,
        "sourceCommit": "a" * 40,
        "packages": [
            {"id": "SkiaSharp"},
            {"id": "SkiaSharp.HarfBuzz"},
            {"id": "HarfBuzzSharp"},
        ],
    },
    "release": {"branch": "release/4.152.0-preview.1"},
    "warnings": ["example"],
}


class ReleaseTestPlanTests(unittest.TestCase):
    @mock.patch.object(planner.urllib.request, "urlopen")
    def test_read_package_uses_nuspec_source_and_dependencies(self, urlopen):
        nuspec = f"""<?xml version="1.0"?>
<package xmlns="http://schemas.microsoft.com/packaging/2013/05/nuspec.xsd">
  <metadata>
    <id>SkiaSharp.HarfBuzz</id>
    <version>{SKIA_VERSION}</version>
    <repository branch="release/4.152.0-preview.1" commit="{'a' * 40}" />
    <dependencies>
      <group>
        <dependency id="HarfBuzzSharp" version="{HARFBUZZ_VERSION}" />
      </group>
    </dependencies>
  </metadata>
</package>"""
        content = io.BytesIO()
        with zipfile.ZipFile(content, "w") as package:
            package.writestr("SkiaSharp.HarfBuzz.nuspec", nuspec)
        response = mock.MagicMock()
        response.__enter__.return_value.read.return_value = content.getvalue()
        urlopen.return_value = response

        result = planner.read_package("SkiaSharp.HarfBuzz", SKIA_VERSION)

        self.assertEqual(result["commit"], "a" * 40)
        self.assertEqual(
            result["harfBuzzVersions"],
            [HARFBUZZ_VERSION],
        )

    def test_full_macos_matrix(self):
        matrix, missing = planner.build_matrix(
            SKIA_VERSION,
            HARFBUZZ_VERSION,
            "macOS",
        )
        self.assertEqual(
            [item["id"] for item in matrix],
            [
                "smoke",
                "console",
                "linux",
                "blazor",
                "android-26",
                "android-37.1",
                "maccatalyst",
                "ios-18.6",
                "ios-26.5",
            ],
        )
        self.assertEqual(
            missing,
            ["MAUI Windows requires a Windows host"],
        )

    def test_linux_matrix_reports_apple_and_windows_gaps(self):
        matrix, missing = planner.build_matrix(
            SKIA_VERSION,
            HARFBUZZ_VERSION,
            "Linux",
        )
        self.assertEqual(
            [item["id"] for item in matrix],
            [
                "smoke",
                "console",
                "linux",
                "blazor",
                "android-26",
                "android-37.1",
            ],
        )
        self.assertEqual(
            missing,
            [
                "iOS and Mac Catalyst require a macOS host",
                "MAUI Windows requires a Windows host",
            ],
        )

    def test_matrix_commands_use_platform_runners(self):
        matrix, _ = planner.build_matrix(
            SKIA_VERSION,
            HARFBUZZ_VERSION,
            "macOS",
        )
        for item in matrix:
            self.assertNotIn("selectedByDefault", item)
            command = item["command"]
            self.assertRegex(
                command,
                r"run-(?:host|android|ios)-tests\.py",
            )
            self.assertIn(
                "--skiasharp 4.152.0-preview.1.1",
                command,
            )
            self.assertIn(
                "--harfbuzzsharp 14.2.1-preview.1.1",
                command,
            )
        android = next(
            item for item in matrix if item["id"] == "android-26"
        )
        self.assertIn("run-android-tests.py 26", android["command"])
        android_max = next(
            item for item in matrix if item["id"] == "android-37.1"
        )
        self.assertIn("run-android-tests.py 37.1", android_max["command"])
        ios_min = next(item for item in matrix if item["id"] == "ios-18.6")
        self.assertIn("run-ios-tests.py 18.6", ios_min["command"])
        ios = next(item for item in matrix if item["id"] == "ios-26.5")
        self.assertIn("run-ios-tests.py 26.5", ios["command"])

    def test_plan_contract_has_no_global_setup_commands(self):
        self.assertNotIn(
            "globalSetupCommands",
            SCRIPT_PATH.read_text(encoding="ascii"),
        )

    def test_every_planned_command_round_trips_through_runner_parser(self):
        matrix, _ = planner.build_matrix(
            SKIA_VERSION,
            HARFBUZZ_VERSION,
            "macOS",
        )
        for item in matrix:
            argv = shlex.split(item["command"])
            script_index = next(
                index
                for index, value in enumerate(argv)
                if Path(value).name in RUNNERS
            )
            script_name = Path(argv[script_index]).name
            parsed = RUNNERS[script_name].create_parser().parse_args(
                argv[script_index + 1 :]
            )
            parsed_id = (
                f"android-{parsed.version}"
                if script_name == "run-android-tests.py"
                else f"ios-{parsed.version}"
                if script_name == "run-ios-tests.py"
                else parsed.command
            )
            self.assertEqual(parsed_id, item["id"])
            self.assertEqual(parsed.skia, "4.152.0-preview.1.1")
            self.assertEqual(parsed.harfbuzz, "14.2.1-preview.1.1")

    @mock.patch.object(planner, "read_package")
    def test_receipt_report_uses_public_anchor_packages(self, read_package):
        def package(package_id, version):
            dependencies = (
                [HARFBUZZ_VERSION]
                if package_id == "SkiaSharp.HarfBuzz"
                else []
            )
            return {
                "id": package_id,
                "version": version,
                "branch": "release/4.152.0-preview.1",
                "commit": "a" * 40,
                "harfBuzzVersions": dependencies,
            }

        read_package.side_effect = package
        result = planner.receipt_report(Path("/repo"), SKIA_VERSION)

        self.assertEqual(result["receipt"]["sourceCommit"], "a" * 40)
        self.assertEqual(
            result["receipt"]["harfBuzzSharpVersion"],
            HARFBUZZ_VERSION,
        )
        self.assertEqual(
            [item["id"] for item in result["receipt"]["packages"]],
            ["SkiaSharp", "SkiaSharp.HarfBuzz", "HarfBuzzSharp"],
        )
        self.assertEqual(
            read_package.call_args_list,
            [
                mock.call("SkiaSharp", SKIA_VERSION),
                mock.call("SkiaSharp.HarfBuzz", SKIA_VERSION),
                mock.call("HarfBuzzSharp", HARFBUZZ_VERSION),
            ],
        )

    @mock.patch.object(planner, "read_package")
    def test_receipt_report_rejects_mismatched_anchor_branch(
        self,
        read_package,
    ):
        def package(package_id, version):
            return {
                "id": package_id,
                "version": version,
                "branch": (
                    "release/4.151.0"
                    if package_id == "HarfBuzzSharp"
                    else "release/4.152.0-preview.1"
                ),
                "commit": "a" * 40,
                "harfBuzzVersions": (
                    [HARFBUZZ_VERSION]
                    if package_id == "SkiaSharp.HarfBuzz"
                    else []
                ),
            }

        read_package.side_effect = package

        with self.assertRaisesRegex(
            planner.PlanError,
            "source metadata does not match",
        ):
            planner.receipt_report(Path("/repo"), SKIA_VERSION)

    @mock.patch.object(planner, "read_package")
    def test_receipt_report_rejects_reused_anchor_from_another_commit(
        self,
        read_package,
    ):
        def package(package_id, version):
            return {
                "id": package_id,
                "version": version,
                "branch": "release/4.152.0-preview.1",
                "commit": (
                    "b" * 40
                    if package_id == "HarfBuzzSharp"
                    else "a" * 40
                ),
                "harfBuzzVersions": (
                    [HARFBUZZ_VERSION]
                    if package_id == "SkiaSharp.HarfBuzz"
                    else []
                ),
            }

        read_package.side_effect = package

        with self.assertRaisesRegex(
            planner.PlanError,
            "source metadata does not match",
        ):
            planner.receipt_report(Path("/repo"), SKIA_VERSION)

    def test_release_summary_reports_public_package_identity(self):
        self.assertEqual(
            planner.release_summary(PUBLIC_PLAN),
            {
                "branch": "release/4.152.0-preview.1",
                "commit": "a" * 40,
                "state": "public",
                "warnings": ["example"],
                "publicPackages": {
                    "SkiaSharp": SKIA_VERSION,
                    "HarfBuzzSharp": HARFBUZZ_VERSION,
                },
                "verifiedPackageCount": 3,
            },
        )

    def test_windows_command_quotes_powershell_metacharacters(self):
        result = planner.format_command(
            ["tool", "system-images;android-36;google_apis;arm64-v8a"],
            platform_name="win32",
        )
        self.assertIn(
            "'system-images;android-36;google_apis;arm64-v8a'",
            result,
        )

    def test_windows_command_uses_call_operator_for_quoted_executable(self):
        result = planner.format_command(
            ["C:\\Program Files\\Python\\python.exe", "run-host-tests.py"],
            platform_name="win32",
        )
        self.assertTrue(result.startswith("& 'C:\\Program Files"))

    def test_scripts_are_ascii_only(self):
        SCRIPT_PATH.read_text(encoding="ascii")
        Path(__file__).read_text(encoding="ascii")


if __name__ == "__main__":
    unittest.main()
