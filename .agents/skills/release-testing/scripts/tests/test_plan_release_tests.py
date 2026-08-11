#!/usr/bin/env python3

import importlib.util
from pathlib import Path
import shlex
import sys
import unittest


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


STATUS = {
    "packageVersions": {
        "test": {
            "SkiaSharp": "4.152.0-preview.1.1",
            "HarfBuzzSharp": "14.2.1-preview.1.1",
        }
    }
}


class ReleaseTestPlanTests(unittest.TestCase):
    def test_full_macos_matrix(self):
        matrix, missing = planner.build_matrix(STATUS, "macOS")
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
        matrix, missing = planner.build_matrix(STATUS, "Linux")
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
        matrix, _ = planner.build_matrix(STATUS, "macOS")
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
        matrix, _ = planner.build_matrix(STATUS, "macOS")
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

    def test_status_override_is_limited_to_tests_wait(self):
        base = {
            "managedRun": {"state": "succeeded"},
            "packageFeed": {"state": "ready"},
        }
        self.assertEqual(
            planner.plan_eligibility(
                {**base, "nextAction": "wait-for-tests"},
                allow_incomplete_ci=True,
            ),
            (True, True),
        )
        with self.assertRaisesRegex(
            planner.PlanError,
            "only the tests wait",
        ):
            planner.plan_eligibility(
                {**base, "nextAction": "wait-for-managed"},
                allow_incomplete_ci=True,
            )
        with self.assertRaisesRegex(
            planner.PlanError,
            "only the tests wait",
        ):
            planner.plan_eligibility(
                {**base, "nextAction": "retry-tests"},
                allow_incomplete_ci=True,
            )

    def test_release_summary_is_flat(self):
        status = {
            "branch": "release/4.152.0-preview.1",
            "commit": "a" * 40,
            "state": "ready",
            "nextAction": "start-release-testing",
            "warnings": ["example"],
            "managedRun": {
                "state": "succeeded",
                "runId": 20,
                "buildNumber": "build",
                "sourceBranch": "refs/heads/release/x",
                "sourceVersion": "a" * 40,
                "url": "managed",
            },
            "testsRun": {"runId": 30, "url": "tests"},
            "packageVersions": {
                "test": {"SkiaSharp": "s", "HarfBuzzSharp": "h"},
                "public": {"SkiaSharp": "s", "HarfBuzzSharp": "h"},
            },
        }
        summary = planner.release_summary(
            status,
            status_override=False,
        )
        self.assertEqual(summary["managedRunId"], 20)
        self.assertEqual(summary["testsRunId"], 30)
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
