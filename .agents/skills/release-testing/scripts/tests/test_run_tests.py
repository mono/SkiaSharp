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


SCRIPTS = Path(__file__).resolve().parent.parent
sys.path.insert(0, str(SCRIPTS))


def load(name: str, filename: str):
    path = SCRIPTS / filename
    spec = importlib.util.spec_from_file_location(name, path)
    module = importlib.util.module_from_spec(spec)
    sys.modules[spec.name] = module
    spec.loader.exec_module(module)
    return module


common = load("release_test_common", "release_test_common.py")
host = load("run_host_tests", "run-host-tests.py")
android = load("run_android_tests", "run-android-tests.py")
apple = load("run_apple_tests", "run-apple-tests.py")


class ReleaseTestRunnerTests(unittest.TestCase):
    def test_exact_test_filter_does_not_overlap_linux_console(self):
        args = common.test_args(
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
            android.select_android_image(
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
            android.select_android_image(
                packages,
                selector="37.1",
                architecture="arm64-v8a",
            )[1],
            "37.1",
        )
        with self.assertRaisesRegex(
            common.ReleaseTestError,
            "Android 37 is not installed",
        ):
            android.select_android_image(
                packages,
                selector="37",
                architecture="arm64-v8a",
            )

    def test_ios_runtime_and_device_selection(self):
        simulators = [
            {
                "isAvailable": True,
                "runtime": {"name": "iOS 18.6", "version": "18.6"},
                "deviceType": {
                    "name": name,
                    "productFamily": "iPhone",
                },
            }
            for name in ("iPhone 16", "iPhone 16 Pro")
        ]
        with mock.patch.object(
            apple,
            "apple_simulators",
            return_value=simulators,
        ):
            self.assertEqual(
                apple.installed_ios_versions(Path.cwd()),
                {"18.6"},
            )
        self.assertEqual(
            apple.resolve_ios_device_type(simulators, "18.6", None),
            "iPhone 16",
        )
        with self.assertRaisesRegex(
            common.ReleaseTestError,
            "does not support device type iPhone 13",
        ):
            apple.resolve_ios_device_type(
                simulators,
                "18.6",
                "iPhone 13",
            )

    def test_ios_run_creates_and_deletes_fresh_simulator_on_failure(self):
        simulators = [
            {
                "isAvailable": True,
                "runtime": {"name": "iOS 18.6", "version": "18.6"},
                "deviceType": {
                    "name": "iPhone 16",
                    "productFamily": "iPhone",
                },
            }
        ]
        args = SimpleNamespace(device=None)
        with (
            mock.patch.object(apple.sys, "platform", "darwin"),
            mock.patch.object(apple.common, "require_workload"),
            mock.patch.object(apple.common, "require_appium_driver"),
            mock.patch.object(
                apple,
                "apple_simulators",
                return_value=simulators,
            ),
            mock.patch.object(
                apple.common,
                "run_json",
                return_value={"udid": "SIM-123"},
            ) as create,
            mock.patch.object(apple.common, "run_streaming") as command,
            mock.patch.object(
                apple.common,
                "run_test",
                side_effect=common.ReleaseTestError("test failed"),
            ),
            self.assertRaisesRegex(common.ReleaseTestError, "test failed"),
        ):
            apple.run_ios(Path.cwd(), args, "18.6")

        create_args = create.call_args.args[0]
        self.assertEqual(create_args[5:7], ["simulator", "create"])
        self.assertTrue(
            create_args[7].startswith("SkiaSharp Release iOS 18.6 ")
        )
        commands = [call.args[0] for call in command.call_args_list]
        self.assertTrue(
            any(
                values[5:8] == ["simulator", "boot", "SIM-123"]
                for values in commands
            )
        )
        self.assertTrue(
            any(
                values[5:8] == ["simulator", "delete", "SIM-123"]
                for values in commands
            )
        )

    def test_split_parsers_accept_their_platform_options(self):
        android_args = android.create_parser().parse_args(
            [
                "37.1",
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
        apple_args = apple.create_parser().parse_args(
            [
                "ios-26.5",
                "--skiasharp",
                "s",
                "--harfbuzzsharp",
                "h",
            ]
        )
        host_args = host.create_parser().parse_args(
            [
                "linux",
                "--skiasharp",
                "s",
                "--harfbuzzsharp",
                "h",
            ]
        )
        self.assertEqual(android_args.version, "37.1")
        self.assertEqual(android_args.device, "pixel_9")
        self.assertEqual(android_args.device_id, "emulator-5554")
        self.assertEqual(apple_args.command, "ios-26.5")
        self.assertEqual(host_args.command, "linux")

    def test_appium_versions_are_exact(self):
        drivers = {
            "uiautomator2": {
                "installed": True,
                "version": "8.2.2",
            }
        }
        common.validate_appium_driver(
            "3.6.0",
            drivers,
            "uiautomator2",
        )
        with self.assertRaisesRegex(
            common.ReleaseTestError,
            "Appium 3.6.0 is required",
        ):
            common.validate_appium_driver(
                "3.5.0",
                drivers,
                "uiautomator2",
            )
        with self.assertRaisesRegex(
            common.ReleaseTestError,
            "uiautomator2 8.2.2 is required",
        ):
            common.validate_appium_driver(
                "3.6.0",
                {
                    "uiautomator2": {
                        "installed": True,
                        "version": "8.1.0",
                    }
                },
                "uiautomator2",
            )

    def test_android_environment_is_refreshed_each_run(self):
        with tempfile.TemporaryDirectory() as directory:
            sdk = Path(directory) / "android"
            jdk = Path(directory) / "java"
            old_sdk = Path(directory) / "old-android"
            old_jdk = Path(directory) / "old-java"
            for path in (sdk, jdk, old_sdk, old_jdk):
                path.mkdir()
            environ = {
                "ANDROID_HOME": str(old_sdk),
                "JAVA_HOME": str(old_jdk),
            }
            results = [
                subprocess.CompletedProcess([], 0, f"{sdk}\n", ""),
                subprocess.CompletedProcess([], 0, f"{jdk}\n", ""),
            ]
            with mock.patch.object(
                android.common,
                "run_streaming",
                side_effect=results,
            ) as command:
                resolved = android.configure_android_environment(
                    Path.cwd(),
                    environ,
                )
        self.assertEqual(command.call_count, 2)
        self.assertEqual(resolved["ANDROID_HOME"], str(sdk))
        self.assertEqual(resolved["JAVA_HOME"], str(jdk))
        self.assertEqual(environ, resolved)

    def test_missing_executable_has_clear_error(self):
        with (
            mock.patch.object(common.shutil, "which", return_value=None),
            mock.patch.object(
                common.subprocess,
                "Popen",
                side_effect=FileNotFoundError,
            ),
            self.assertRaisesRegex(
                common.ReleaseTestError,
                "missing-tool was not found on PATH",
            ),
        ):
            common.run_streaming(["missing-tool"], cwd=Path.cwd())

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
            mock.patch.object(
                common.subprocess,
                "Popen",
                return_value=SlowProcess(),
            ),
            mock.patch.object(common.time, "monotonic", side_effect=[0, 5, 6]),
            contextlib.redirect_stdout(output),
        ):
            result = common.run_streaming(
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
        args = SimpleNamespace(command="smoke", skia="s", harfbuzz="h")
        output = io.StringIO()
        with (
            mock.patch.object(
                common,
                "repository_root",
                return_value=Path.cwd(),
            ),
            mock.patch.object(common.time, "monotonic", side_effect=[0, 7]),
            contextlib.redirect_stdout(output),
        ):
            result = common.execute_item(args, mock.Mock())

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
            mock.patch.object(common.sys, "platform", "win32"),
            mock.patch.object(common.shutil, "which", side_effect=which),
        ):
            resolved = common.resolve_command(["appium", "--version"])
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

    def test_scripts_are_ascii_only(self):
        for filename in (
            "release_test_common.py",
            "run-host-tests.py",
            "run-android-tests.py",
            "run-apple-tests.py",
        ):
            (SCRIPTS / filename).read_text(encoding="ascii")
        Path(__file__).read_text(encoding="ascii")

    def test_runners_have_no_provisioning_commands(self):
        for filename in (
            "run-host-tests.py",
            "run-android-tests.py",
            "run-apple-tests.py",
        ):
            source = (SCRIPTS / filename).read_text(encoding="ascii")
            for command in (
                '"install"',
                '"restore"',
                '"update"',
                '"downloadPlatform"',
            ):
                self.assertNotIn(command, source)


if __name__ == "__main__":
    unittest.main()
