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

RUNNER_PATH = Path(__file__).resolve().parent.parent / "run-tests.py"
RUNNER_SPEC = importlib.util.spec_from_file_location(
    "release_test_runner",
    RUNNER_PATH,
)
runner = importlib.util.module_from_spec(RUNNER_SPEC)
sys.modules[RUNNER_SPEC.name] = runner
RUNNER_SPEC.loader.exec_module(runner)


STATUS = {
    "packageVersions": {
        "test": {
            "SkiaSharp": "4.152.0-preview.1.1",
            "HarfBuzzSharp": "14.2.1-preview.1.1",
        }
    }
}
APPLE_TARGETS = {
    "xcodeVersion": "27.0",
    "minimum": {"version": "18.2", "device": "iPhone 16"},
    "maximum": {"version": "26.5", "device": "iPhone 17"},
    "availableVersions": ["18.2", "26.5", "27.0"],
    "developerDirectory": "/Applications/Xcode.app/Contents/Developer",
}


def simulator(version: str, device: str) -> dict:
    return {
        "isAvailable": True,
        "deviceType": {
            "name": device,
            "productFamily": "iPhone",
        },
        "runtime": {
            "name": f"iOS {version}",
            "version": version,
            "isAvailable": True,
        },
    }


class ReleaseTestPlanTests(unittest.TestCase):
    def test_xcode_26_selects_ios_15_and_26(self):
        targets = planner.select_apple_targets(
            "26.6",
            "/Applications/Xcode-26.6.0.app",
            [
                simulator("15.8", "iPhone 13 Pro"),
                simulator("15.0", "iPhone 13"),
                simulator("26.0", "iPhone 16"),
                simulator("26.5", "iPhone 17"),
            ],
        )

        self.assertEqual(targets["minimum"]["version"], "15.0")
        self.assertEqual(targets["minimum"]["device"], "iPhone 13")
        self.assertEqual(targets["maximum"]["version"], "26.5")
        self.assertEqual(targets["maximum"]["device"], "iPhone 17")

    def test_xcode_27_selects_ios_18_and_26(self):
        targets = planner.select_apple_targets(
            "27.0",
            "/Applications/Xcode.app",
            [
                simulator("16.2", "iPhone 14"),
                simulator("18.2", "iPhone 16"),
                simulator("18.5", "iPhone 16 Pro"),
                simulator("26.5", "iPhone 17"),
                simulator("27.0", "iPhone 17 Pro"),
            ],
        )

        self.assertEqual(targets["minimum"]["version"], "18.2")
        self.assertEqual(targets["minimum"]["device"], "iPhone 16")
        self.assertEqual(targets["maximum"]["version"], "26.5")
        self.assertEqual(targets["maximum"]["device"], "iPhone 17")

    def test_full_macos_matrix(self):
        matrix, missing = planner.build_matrix(
            STATUS,
            "macOS",
            apple_targets=APPLE_TARGETS,
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
                "ios-18.2",
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

    def test_matrix_commands_use_one_python_runner(self):
        matrix, _ = planner.build_matrix(
            STATUS,
            "macOS",
            apple_targets=APPLE_TARGETS,
        )
        for item in matrix:
            self.assertNotIn("selectedByDefault", item)
            command = item["command"]
            self.assertIn("run-tests.py", command)
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
        self.assertIn("android-26", android["command"])
        android_max = next(
            item for item in matrix if item["id"] == "android-37.1"
        )
        self.assertIn("android-37.1", android_max["command"])
        ios_min = next(item for item in matrix if item["id"] == "ios-18.2")
        self.assertIn("ios-18.2 --device 'iPhone 16'", ios_min["command"])
        ios = next(item for item in matrix if item["id"] == "ios-26.5")
        self.assertIn("ios-26.5 --device 'iPhone 17'", ios["command"])

    def test_plan_contract_has_no_global_setup_commands(self):
        self.assertNotIn(
            "globalSetupCommands",
            SCRIPT_PATH.read_text(encoding="ascii"),
        )

    def test_every_planned_command_round_trips_through_runner_parser(self):
        matrix, _ = planner.build_matrix(
            STATUS,
            "macOS",
            apple_targets=APPLE_TARGETS,
        )
        for item in matrix:
            argv = shlex.split(item["command"])
            script_index = next(
                index
                for index, value in enumerate(argv)
                if value.endswith("run-tests.py")
            )
            parsed = runner.create_parser().parse_args(
                argv[script_index + 1 :]
            )
            self.assertEqual(parsed.command, item["id"])
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
            ["C:\\Program Files\\Python\\python.exe", "run-tests.py"],
            platform_name="win32",
        )
        self.assertTrue(result.startswith("& 'C:\\Program Files"))

    def test_json_parser_ignores_command_noise(self):
        self.assertEqual(
            planner.parse_json_output(
                "WARN [status] not JSON\n{\"ready\": true}\n"
            ),
            {"ready": True},
        )

    def test_scripts_are_ascii_only(self):
        SCRIPT_PATH.read_text(encoding="ascii")
        Path(__file__).read_text(encoding="ascii")


if __name__ == "__main__":
    unittest.main()
