#!/usr/bin/env python3

import importlib.util
import json
from pathlib import Path
import shlex
import sys
from types import SimpleNamespace
import unittest
from unittest import mock


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
FINISH_PLAN = {
    "receipt": {
        "skiaSharpVersion": SKIA_VERSION,
        "harfBuzzSharpVersion": HARFBUZZ_VERSION,
        "sourceCommit": "a" * 40,
        "packages": [{"id": "SkiaSharp"}, {"id": "HarfBuzzSharp"}],
    },
    "release": {"branch": "release/4.152.0-preview.1"},
    "warnings": ["example"],
}


class ReleaseTestPlanTests(unittest.TestCase):
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

    @mock.patch.object(planner.common, "run_checked")
    def test_receipt_report_uses_finish_plan(self, run_checked):
        def write_plan(args, *, cwd, timeout):
            output = Path(args[args.index("--output") + 1])
            output.write_text(json.dumps(FINISH_PLAN), encoding="utf-8")
            return SimpleNamespace(stdout="")

        run_checked.side_effect = write_plan
        result = planner.receipt_report(Path("/repo"), SKIA_VERSION)

        self.assertEqual(result, FINISH_PLAN)
        args = run_checked.call_args.args[0]
        self.assertEqual(args[1], str(planner.RELEASE_CLI))
        self.assertEqual(args[2:4], ["finish", "plan"])
        self.assertIn(SKIA_VERSION, args)
        self.assertEqual(run_checked.call_args.kwargs["timeout"], 600)

    def test_release_summary_is_flat(self):
        summary = planner.release_summary(FINISH_PLAN)

        self.assertEqual(summary["branch"], "release/4.152.0-preview.1")
        self.assertEqual(summary["commit"], "a" * 40)
        self.assertEqual(summary["state"], "public")
        self.assertEqual(summary["publicPackages"]["SkiaSharp"], SKIA_VERSION)
        self.assertEqual(summary["verifiedPackageCount"], 2)
        self.assertEqual(summary["warnings"], ["example"])
        self.assertNotIn("managedRun", summary)
        self.assertNotIn("testsRun", summary)

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
