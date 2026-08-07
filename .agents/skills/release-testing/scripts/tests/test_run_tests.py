#!/usr/bin/env python3

import contextlib
import importlib.util
import io
from pathlib import Path
import subprocess
import sys
import tempfile
from types import SimpleNamespace
import unittest
from unittest import mock


SCRIPT_PATH = Path(__file__).resolve().parent.parent / "run-tests.py"
SPEC = importlib.util.spec_from_file_location("run_tests", SCRIPT_PATH)
runner = importlib.util.module_from_spec(SPEC)
sys.modules[SPEC.name] = runner
SPEC.loader.exec_module(runner)


class ReleaseTestRunnerTests(unittest.TestCase):
    def test_exact_test_filter_does_not_overlap_linux_console(self):
        args = runner.test_args(
            "ConsoleTests",
            skia="4.152.0-preview.1.1",
            harfbuzz="14.2.1-preview.1.1",
        )
        filter_value = args[args.index("--filter-class") + 1]
        self.assertEqual(
            filter_value,
            "SkiaSharp.Tests.Integration.ConsoleTests",
        )

    def test_android_image_selection_requires_exact_version(self):
        packages = [
            {
                "path": (
                    "system-images;android-37.1;"
                    "google_apis_playstore_ps16k;arm64-v8a"
                ),
                "version": "8",
            },
            {
                "path": (
                    "system-images;android-37.2-beta1;"
                    "google_apis_ps16k;arm64-v8a"
                ),
                "version": "1",
            },
            {
                "path": (
                    "system-images;android-37.2-beta2;"
                    "google_apis_playstore_ps16k;arm64-v8a"
                ),
                "version": "2",
            },
        ]
        self.assertEqual(
            runner.select_android_image(
                packages,
                selector="37.2",
                architecture="arm64-v8a",
            ),
            (
                "system-images;android-37.2-beta2;"
                "google_apis_playstore_ps16k;arm64-v8a",
                "37.2",
            ),
        )
        self.assertEqual(
            runner.select_android_image(
                packages,
                selector="37.1",
                architecture="arm64-v8a",
            )[1],
            "37.1",
        )
        with self.assertRaisesRegex(
            runner.TestRunError,
            "Android 37 is not installed",
        ):
            runner.select_android_image(
                packages,
                selector="37",
                architecture="arm64-v8a",
            )

    def test_ios_runtime_check_uses_installed_simulators(self):
        simulators = [
            {
                "runtime": {"name": f"iOS {version}"},
            }
            for version in ("15.0", "26.5")
        ]
        with mock.patch.object(
            runner,
            "apple_simulators",
            return_value=simulators,
        ):
            self.assertEqual(
                runner.installed_ios_versions(Path.cwd()),
                {
                    "15.0",
                    "26.5",
                },
            )

    def test_parser_supports_versioned_mobile_commands_and_device(self):
        parser = runner.create_parser()
        android = parser.parse_args(
            [
                "android-37.1",
                "--skiasharp",
                "s",
                "--harfbuzzsharp",
                "h",
                "--device",
                "pixel_9",
                "--device-id",
                "emulator-5554",
            ]
        )
        ios = parser.parse_args(
            [
                "ios-26.3",
                "--skiasharp",
                "s",
                "--harfbuzzsharp",
                "h",
            ]
        )
        self.assertEqual(android.command, "android-37.1")
        self.assertEqual(android.skia, "s")
        self.assertEqual(android.harfbuzz, "h")
        self.assertEqual(android.device, "pixel_9")
        self.assertEqual(android.device_id, "emulator-5554")
        self.assertEqual(ios.command, "ios-26.3")
        self.assertEqual(
            runner.mobile_command(android.command),
            ("android", "37.1"),
        )
        self.assertEqual(
            runner.mobile_command(ios.command),
            ("ios", "26.3"),
        )

    def test_unversioned_mobile_commands_are_not_supported(self):
        self.assertIsNone(runner.mobile_command("android"))
        self.assertIsNone(runner.mobile_command("ios"))

    def test_appium_versions_are_exact(self):
        drivers = {
            "uiautomator2": {
                "installed": True,
                "version": "8.2.2",
            }
        }
        runner.validate_appium_driver(
            "3.6.0",
            drivers,
            "uiautomator2",
        )
        with self.assertRaisesRegex(
            runner.TestRunError,
            "Appium 3.6.0 is required",
        ):
            runner.validate_appium_driver(
                "3.5.0",
                drivers,
                "uiautomator2",
            )
        with self.assertRaisesRegex(
            runner.TestRunError,
            "uiautomator2 8.2.2 is required",
        ):
            runner.validate_appium_driver(
                "3.6.0",
                {
                    "uiautomator2": {
                        "installed": True,
                        "version": "8.1.0",
                    }
                },
                "uiautomator2",
            )

    def test_android_environment_is_discovered_with_pinned_tool(self):
        with tempfile.TemporaryDirectory() as directory:
            sdk = Path(directory) / "android"
            jdk = Path(directory) / "java"
            sdk.mkdir()
            jdk.mkdir()
            results = [
                subprocess.CompletedProcess([], 0, f"{sdk}\n", ""),
                subprocess.CompletedProcess([], 0, f"{jdk}\n", ""),
            ]
            environ = {}
            with mock.patch.object(
                runner,
                "run",
                side_effect=results,
            ) as command:
                resolved = runner.configure_android_environment(
                    Path.cwd(),
                    environ,
                )
        self.assertEqual(resolved["ANDROID_HOME"], str(sdk))
        self.assertEqual(resolved["JAVA_HOME"], str(jdk))
        self.assertEqual(environ, resolved)
        self.assertEqual(command.call_count, 2)

    def test_android_environment_is_refreshed_each_run(self):
        with tempfile.TemporaryDirectory() as directory:
            sdk = Path(directory) / "android"
            jdk = Path(directory) / "java"
            old_sdk = Path(directory) / "old-android"
            old_jdk = Path(directory) / "old-java"
            sdk.mkdir()
            jdk.mkdir()
            old_sdk.mkdir()
            old_jdk.mkdir()
            environ = {
                "ANDROID_HOME": str(old_sdk),
                "JAVA_HOME": str(old_jdk),
            }
            results = [
                subprocess.CompletedProcess([], 0, f"{sdk}\n", ""),
                subprocess.CompletedProcess([], 0, f"{jdk}\n", ""),
            ]
            with mock.patch.object(
                runner,
                "run",
                side_effect=results,
            ) as command:
                resolved = runner.configure_android_environment(
                    Path.cwd(),
                    environ,
                )
        self.assertEqual(command.call_count, 2)
        self.assertEqual(resolved["ANDROID_HOME"], str(sdk))
        self.assertEqual(resolved["JAVA_HOME"], str(jdk))
        self.assertEqual(environ["ANDROID_HOME"], str(sdk))

    def test_missing_executable_has_clear_error(self):
        with (
            mock.patch.object(runner.shutil, "which", return_value=None),
            mock.patch.object(
                runner.subprocess,
                "Popen",
                side_effect=FileNotFoundError,
            ),
            self.assertRaisesRegex(
                runner.TestRunError,
                "missing-tool was not found on PATH",
            ),
        ):
            runner.run(["missing-tool"], cwd=Path.cwd())

    def test_silent_command_reports_heartbeat_and_result(self):
        class SlowProcess:
            returncode = 0

            def __init__(self):
                self.calls = 0

            def communicate(self, timeout):
                self.calls += 1
                if self.calls == 1:
                    raise subprocess.TimeoutExpired("slow-tool", timeout)
                return "captured output", ""

        output = io.StringIO()
        with (
            mock.patch.object(runner.subprocess, "Popen", return_value=SlowProcess()),
            mock.patch.object(runner.time, "monotonic", side_effect=[0, 5, 6]),
            contextlib.redirect_stdout(output),
        ):
            result = runner.run(
                ["slow-tool"],
                cwd=Path.cwd(),
                capture=True,
            )

        self.assertEqual(result.stdout, "captured output")
        self.assertIn("command started: slow-tool", output.getvalue())
        self.assertIn(
            "command still running after 5s: slow-tool",
            output.getvalue(),
        )
        self.assertIn(
            "command finished after 6s (exit 0): slow-tool",
            output.getvalue(),
        )

    def test_item_reports_start_and_pass(self):
        parser = mock.Mock()
        parser.parse_args.return_value = SimpleNamespace(
            command="smoke",
            skia="s",
            harfbuzz="h",
            device=None,
            device_id=None,
        )
        output = io.StringIO()
        with (
            mock.patch.object(runner, "create_parser", return_value=parser),
            mock.patch.object(runner, "repo_root", return_value=Path.cwd()),
            mock.patch.object(runner, "run_test"),
            mock.patch.object(runner.time, "monotonic", side_effect=[0, 7]),
            contextlib.redirect_stdout(output),
        ):
            result = runner.main()

        self.assertEqual(result, 0)
        self.assertIn("item started: smoke", output.getvalue())
        self.assertIn("item passed after 7s: smoke", output.getvalue())

    def test_windows_command_shims_run_through_cmd(self):
        def which(command):
            return {
                "appium": "C:\\Program Files\\nodejs\\appium.cmd",
                "cmd.exe": "C:\\Windows\\System32\\cmd.exe",
            }.get(command)

        with (
            mock.patch.object(runner.sys, "platform", "win32"),
            mock.patch.object(runner.shutil, "which", side_effect=which),
        ):
            resolved = runner.resolve_command(["appium", "--version"])
        self.assertEqual(
            resolved[:4],
            [
                "C:\\Windows\\System32\\cmd.exe",
                "/d",
                "/s",
                "/c",
            ],
        )
        self.assertIn("appium.cmd", resolved[4])

    def test_json_parser_ignores_command_noise(self):
        self.assertEqual(
            runner.parse_json_output(
                "WARN [Appium] not JSON\nstatus\n{\"ready\": true}\n"
            ),
            {"ready": True},
        )

    def test_scripts_are_ascii_only(self):
        SCRIPT_PATH.read_text(encoding="ascii")
        Path(__file__).read_text(encoding="ascii")

    def test_runner_has_no_provisioning_commands(self):
        source = SCRIPT_PATH.read_text(encoding="ascii")
        for command in (
            '"install"',
            '"restore"',
            '"update"',
            '"downloadPlatform"',
        ):
            self.assertNotIn(command, source)


if __name__ == "__main__":
    unittest.main()
