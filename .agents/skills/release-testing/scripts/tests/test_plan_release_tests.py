#!/usr/bin/env python3

import importlib.util
import io
import json
from pathlib import Path
import shlex
import sys
import unittest
from unittest import mock
import zipfile


SCRIPTS = Path(__file__).resolve().parent.parent
sys.path.insert(0, str(SCRIPTS))
SCRIPT_PATH = SCRIPTS / "plan-release-tests.py"
SKILL_PATH = SCRIPTS.parent / "SKILL.md"
ROOT = SCRIPTS.parents[3]
INTEGRATION_PROJECT = ROOT / "tests/SkiaSharp.Tests.Integration/SkiaSharp.Tests.Integration.csproj"
PLATFORM_TEST_BASE = ROOT / "tests/SkiaSharp.Tests.Integration/Tests/PlatformTestBase.cs"
LINUX_TESTS = ROOT / "tests/SkiaSharp.Tests.Integration/Tests/LinuxConsoleTests.cs"
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
    "run-android-tests.py": load_runner("run_android_tests", "run-android-tests.py"),
    "run-ios-tests.py": load_runner("run_ios_tests", "run-ios-tests.py"),
}


SKIA_VERSION = "4.150.3"
HARFBUZZ_VERSION = "14.2.1.3"
RELEASE_BRANCH = "release/4.150.3"
HISTORICAL_SKIA_VERSION = "4.151.1"
HISTORICAL_HARFBUZZ_VERSION = "14.2.1.1"
BAR_ID = 329644
BAR_COMMIT = "f8a404ff3375872d042404a29189ceb8e3001a41"
BAR_FEED = "https://pkgs.dev.azure.com/dnceng/public/_packaging/" "darc-pub-dotnet-SkiaSharp-f8a404ff/nuget/v3/index.json"
BAR_FLAT_CONTAINER = (
    "https://pkgs.dev.azure.com/dnceng/9ee6d478-d288-47f7-aacc-" "f6e6d082ae6d/_packaging/8bf0dc6d-5564-4f8e-8ff2-" "8167b63c6306/nuget/v3/flat2/"
)
BAR_GUID_FEED = (
    "https://pkgs.dev.azure.com/dnceng/9ee6d478-d288-47f7-aacc-" "f6e6d082ae6d/_packaging/8bf0dc6d-5564-4f8e-8ff2-" "8167b63c6306/nuget/v3/index.json"
)
BAR_SOURCE = {
    "barBuildId": BAR_ID,
    "buildNumber": "4.150.3+20260828.7",
    "azdoBuildId": 3060533,
    "buildLink": "https://dev.azure.com/dnceng/internal/_build/results?buildId=3060533",
    "branch": RELEASE_BRANCH,
    "commit": BAR_COMMIT,
    "packageFeed": BAR_FEED,
}
BAR_BUILD = planner.darc.BarBuild(
    id=BAR_ID,
    build_number=BAR_SOURCE["buildNumber"],
    azdo_build_id=BAR_SOURCE["azdoBuildId"],
    build_link=BAR_SOURCE["buildLink"],
    branch=RELEASE_BRANCH,
    commit=BAR_COMMIT,
    package_feed=BAR_FEED,
)
DARC_ASSET = {
    "name": "SkiaSharp",
    "version": SKIA_VERSION,
    "build": {
        "id": BAR_ID,
        "branch": f"refs/heads/{RELEASE_BRANCH}",
        "commit": BAR_COMMIT,
        "buildNumber": BAR_SOURCE["buildNumber"],
        "azdoBuildId": BAR_SOURCE["azdoBuildId"],
        "buildLink": BAR_SOURCE["buildLink"],
        "released": False,
    },
    "locations": ["https://dev.azure.com/dnceng/internal/_apis/build/builds/3060533/artifacts", BAR_FEED],
}
CI_PLAN = {
    "receipt": {
        **BAR_SOURCE,
        "flatContainer": BAR_FLAT_CONTAINER,
        "resolvedPackageSource": BAR_GUID_FEED,
        "skiaSharpVersion": SKIA_VERSION,
        "harfBuzzSharpVersion": HARFBUZZ_VERSION,
        "sourceCommit": BAR_COMMIT,
        "packages": [{"id": "SkiaSharp"}, {"id": "SkiaSharp.HarfBuzz"}, {"id": "HarfBuzzSharp"}],
    },
    "release": {"branch": RELEASE_BRANCH},
}


def package_identity(package_id, version, *, branch=RELEASE_BRANCH, commit=BAR_COMMIT, harfbuzz_versions=()):
    return planner.nuget.PackageIdentity(id=package_id, version=version, branch=branch, commit=commit, harfbuzz_versions=tuple(harfbuzz_versions))


class ReleaseTestPlanTests(unittest.TestCase):
    def receipt_report(self, version=SKIA_VERSION):
        with mock.patch.object(planner.darc, "resolve_build", return_value=BAR_BUILD), mock.patch.object(
            planner.nuget, "resolve_flat_container", return_value=BAR_FLAT_CONTAINER
        ):
            return planner.receipt_report(version, bar_id=BAR_ID)

    @mock.patch.object(planner.darc.common, "run_checked")
    def test_query_assets_uses_maestro(self, run_checked):
        run_checked.return_value.stdout = json.dumps([DARC_ASSET])

        result = planner.darc.query_assets(SKIA_VERSION, bar_id=BAR_ID, max_age=30)

        self.assertEqual(result, [DARC_ASSET])
        command = run_checked.call_args.args[0]
        self.assertIn("https://maestro.dot.net", command)
        self.assertEqual(command[-2:], ["--build", str(BAR_ID)])

    @mock.patch.object(planner.darc, "query_assets")
    def test_resolve_build_returns_unique_feed(self, darc_assets):
        darc_assets.return_value = [DARC_ASSET]

        self.assertEqual(planner.darc.resolve_build(SKIA_VERSION), BAR_BUILD)

    @mock.patch.object(planner.darc, "query_assets")
    def test_resolve_build_requires_id_when_ambiguous(self, darc_assets):
        other = {**DARC_ASSET, "build": {**DARC_ASSET["build"], "id": BAR_ID + 1}}
        darc_assets.return_value = [DARC_ASSET, other]

        with self.assertRaisesRegex(planner.PlanError, "select one with --bar-id"):
            planner.darc.resolve_build(SKIA_VERSION)

    @mock.patch.object(planner.darc, "query_assets")
    def test_resolve_build_rejects_released_build(self, darc_assets):
        darc_assets.return_value = [{**DARC_ASSET, "build": {**DARC_ASSET["build"], "released": True}}]

        with self.assertRaisesRegex(planner.PlanError, "already released"):
            planner.darc.resolve_build(SKIA_VERSION)

    @mock.patch.object(planner.nuget.urllib.request, "urlopen")
    def test_resolve_flat_container_uses_guid_resource(self, urlopen):
        response = mock.MagicMock()
        response.__enter__.return_value.read.return_value = json.dumps(
            {"resources": [{"@id": BAR_FLAT_CONTAINER, "@type": "PackageBaseAddress/3.0.0"}]}
        ).encode()
        urlopen.return_value = response

        self.assertEqual(planner.nuget.resolve_flat_container(BAR_FEED), BAR_FLAT_CONTAINER)
        self.assertEqual(planner.nuget.service_index_from_flat_container(BAR_FLAT_CONTAINER), BAR_GUID_FEED)

    @mock.patch.object(planner.nuget.urllib.request, "urlopen")
    def test_read_package_uses_nuspec_source_and_dependencies(self, urlopen):
        nuspec = f"""<?xml version="1.0"?>
<package xmlns="http://schemas.microsoft.com/packaging/2013/05/nuspec.xsd">
  <metadata>
    <id>SkiaSharp.HarfBuzz</id>
    <version>{SKIA_VERSION}</version>
    <repository branch="{RELEASE_BRANCH}" commit="{'a' * 40}" />
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

        result = planner.nuget.read_package("SkiaSharp.HarfBuzz", SKIA_VERSION, BAR_FLAT_CONTAINER)

        self.assertEqual(result.commit, "a" * 40)
        self.assertEqual(result.harfbuzz_versions, (HARFBUZZ_VERSION,))
        request = urlopen.call_args.args[0]
        self.assertEqual(request.full_url, f"{BAR_FLAT_CONTAINER}skiasharp.harfbuzz/4.150.3/" "skiasharp.harfbuzz.4.150.3.nupkg")

    def test_full_macos_matrix(self):
        matrix, missing = planner.build_matrix(SKIA_VERSION, HARFBUZZ_VERSION, BAR_GUID_FEED, "macOS")
        self.assertEqual(
            [item["id"] for item in matrix], ["smoke", "console", "linux", "blazor", "android-26", "android-37.1", "maccatalyst", "ios-18.6", "ios-26.5"]
        )
        self.assertEqual(missing, ["MAUI Windows requires a Windows host"])

    def test_linux_matrix_reports_apple_and_windows_gaps(self):
        matrix, missing = planner.build_matrix(SKIA_VERSION, HARFBUZZ_VERSION, BAR_GUID_FEED, "Linux")
        self.assertEqual([item["id"] for item in matrix], ["smoke", "console", "linux", "blazor", "android-26", "android-37.1"])
        self.assertEqual(missing, ["iOS and Mac Catalyst require a macOS host", "MAUI Windows requires a Windows host"])

    def test_matrix_commands_use_platform_runners(self):
        matrix, _ = planner.build_matrix(SKIA_VERSION, HARFBUZZ_VERSION, BAR_GUID_FEED, "macOS")
        for item in matrix:
            self.assertNotIn("selectedByDefault", item)
            command = item["command"]
            self.assertRegex(command, r"run-(?:host|android|ios)-tests\.py")
            self.assertIn(f"--skiasharp {SKIA_VERSION}", command)
            self.assertIn(f"--harfbuzzsharp {HARFBUZZ_VERSION}", command)
            self.assertIn(f"--package-source {BAR_GUID_FEED}", command)
        android = next(item for item in matrix if item["id"] == "android-26")
        self.assertIn("run-android-tests.py 26", android["command"])
        android_max = next(item for item in matrix if item["id"] == "android-37.1")
        self.assertIn("run-android-tests.py 37.1", android_max["command"])
        ios_min = next(item for item in matrix if item["id"] == "ios-18.6")
        self.assertIn("run-ios-tests.py 18.6", ios_min["command"])
        ios = next(item for item in matrix if item["id"] == "ios-26.5")
        self.assertIn("run-ios-tests.py 26.5", ios["command"])

    def test_plan_contract_has_no_global_setup_commands(self):
        self.assertNotIn("globalSetupCommands", SCRIPT_PATH.read_text(encoding="ascii"))

    def test_plan_reports_ci_verification_and_restore_sources(self):
        self.assertEqual(
            planner.package_sources(CI_PLAN["receipt"]),
            {
                "barLocation": BAR_FEED,
                "ciVerification": BAR_GUID_FEED,
                "resolvedFlatContainer": BAR_FLAT_CONTAINER,
                "runnerRestore": [BAR_GUID_FEED, planner.common.DOTNET_PUBLIC_SOURCE],
            },
        )

    def test_integration_harness_uses_resolved_bar_feed(self):
        project = INTEGRATION_PROJECT.read_text(encoding="utf-8")
        platform = PLATFORM_TEST_BASE.read_text(encoding="utf-8")
        linux = LINUX_TESTS.read_text(encoding="utf-8")

        self.assertIn("<PackageSource", project)
        self.assertIn("<RestoreSources", project)
        self.assertIn('key="SkiaSharp BAR"', platform)
        self.assertIn("WriteNuGetConfig(projectDir)", linux)

    def test_skill_preserves_the_operational_workflow(self):
        skill = SKILL_PATH.read_text(encoding="ascii")
        for heading in (
            "## Boundaries",
            "## Test matrix",
            "## Runner ownership",
            "### 1. Resolve and verify the BAR package family",
            "### 2. Approve the exact matrix",
            "### 3. Prepare once",
            "### 4. Run every approved item",
            "### 5. Repair and retry",
            "### 6. Report and decide",
        ):
            self.assertIn(heading, skill)
        for item_id in (
            "smoke",
            "console",
            "linux",
            "blazor",
            f"android-{planner.common.ANDROID_MIN_VERSION}",
            f"android-{planner.common.ANDROID_MAX_VERSION}",
            "maccatalyst",
            f"ios-{planner.common.IOS_MIN_VERSION}",
            f"ios-{planner.common.IOS_MAX_VERSION}",
            "windows",
        ):
            self.assertIn(f"`{item_id}`", skill)

    def test_every_planned_command_round_trips_through_runner_parser(self):
        matrix, _ = planner.build_matrix(SKIA_VERSION, HARFBUZZ_VERSION, BAR_GUID_FEED, "macOS")
        for item in matrix:
            argv = shlex.split(item["command"])
            script_index = next(index for index, value in enumerate(argv) if Path(value).name in RUNNERS)
            script_name = Path(argv[script_index]).name
            parsed = RUNNERS[script_name].create_parser().parse_args(argv[script_index + 1 :])
            parsed_id = (
                f"android-{parsed.version}"
                if script_name == "run-android-tests.py"
                else f"ios-{parsed.version}" if script_name == "run-ios-tests.py" else parsed.command
            )
            self.assertEqual(parsed_id, item["id"])
            self.assertEqual(parsed.skia, SKIA_VERSION)
            self.assertEqual(parsed.harfbuzz, HARFBUZZ_VERSION)
            self.assertEqual(parsed.package_source, BAR_GUID_FEED)

    @mock.patch.object(planner.nuget, "read_package")
    def test_receipt_report_uses_ci_anchor_packages(self, read_package):
        def package(package_id, version, _flat_container):
            dependencies = (HARFBUZZ_VERSION,) if package_id == "SkiaSharp.HarfBuzz" else ()
            return package_identity(package_id, version, harfbuzz_versions=dependencies)

        read_package.side_effect = package
        result = self.receipt_report()

        self.assertEqual(result["receipt"]["sourceCommit"], BAR_COMMIT)
        self.assertEqual(result["receipt"]["harfBuzzSharpVersion"], HARFBUZZ_VERSION)
        self.assertEqual([item["id"] for item in result["receipt"]["packages"]], ["SkiaSharp", "SkiaSharp.HarfBuzz", "HarfBuzzSharp"])
        self.assertEqual(
            read_package.call_args_list,
            [
                mock.call("SkiaSharp", SKIA_VERSION, BAR_FLAT_CONTAINER),
                mock.call("SkiaSharp.HarfBuzz", SKIA_VERSION, BAR_FLAT_CONTAINER),
                mock.call("HarfBuzzSharp", HARFBUZZ_VERSION, BAR_FLAT_CONTAINER),
            ],
        )

    @mock.patch.object(planner.nuget, "read_package")
    def test_receipt_report_requires_exact_harfbuzz_dependency(self, read_package):
        read_package.side_effect = [
            package_identity("SkiaSharp", SKIA_VERSION),
            package_identity("SkiaSharp.HarfBuzz", SKIA_VERSION, harfbuzz_versions=(f"[{HARFBUZZ_VERSION}]",)),
        ]

        with self.assertRaisesRegex(planner.PlanError, "does not pin one concrete HarfBuzzSharp dependency"):
            self.receipt_report()

    @mock.patch.object(planner.nuget, "read_package")
    def test_receipt_report_rejects_mismatched_anchor_branch(self, read_package):
        def package(package_id, version, _flat_container):
            return package_identity(
                package_id,
                version,
                branch="release/4.151.0" if package_id == "HarfBuzzSharp" else RELEASE_BRANCH,
                harfbuzz_versions=(HARFBUZZ_VERSION,) if package_id == "SkiaSharp.HarfBuzz" else (),
            )

        read_package.side_effect = package

        with self.assertRaisesRegex(planner.PlanError, "source metadata does not match"):
            self.receipt_report()

    @mock.patch.object(planner.nuget, "read_package")
    def test_receipt_report_rejects_reused_anchor_from_another_commit(self, read_package):
        def package(package_id, version, _flat_container):
            if package_id == "HarfBuzzSharp":
                return package_identity(package_id, version, branch="release/4.150.1", commit="c3e4f4c20e1f23ab74d31a8838a5bd6dc55365f2")
            return package_identity(
                package_id,
                version,
                branch="release/4.151.1",
                commit="279f93f4ffa7f9fe4e9c0bc298bedc3c9e439764",
                harfbuzz_versions=(HISTORICAL_HARFBUZZ_VERSION,) if package_id == "SkiaSharp.HarfBuzz" else (),
            )

        read_package.side_effect = package

        with self.assertRaisesRegex(planner.PlanError, "source metadata does not match"):
            self.receipt_report(HISTORICAL_SKIA_VERSION)

    @mock.patch.object(planner.nuget, "read_package")
    def test_receipt_report_rejects_feed_from_another_bar(self, read_package):
        def package(package_id, version, _flat_container):
            return package_identity(package_id, version, commit="b" * 40, harfbuzz_versions=(HARFBUZZ_VERSION,) if package_id == "SkiaSharp.HarfBuzz" else ())

        read_package.side_effect = package

        with self.assertRaisesRegex(planner.PlanError, "BAR build and package source metadata do not match"):
            self.receipt_report()

    def test_release_summary_reports_ci_package_identity(self):
        self.assertEqual(
            planner.release_summary(CI_PLAN),
            {
                "branch": RELEASE_BRANCH,
                "commit": BAR_COMMIT,
                "barBuildId": BAR_ID,
                "buildNumber": BAR_SOURCE["buildNumber"],
                "azdoBuildId": BAR_SOURCE["azdoBuildId"],
                "buildLink": BAR_SOURCE["buildLink"],
                "ciPackages": {"SkiaSharp": SKIA_VERSION, "HarfBuzzSharp": HARFBUZZ_VERSION},
                "verifiedPackageCount": 3,
            },
        )

    def test_windows_command_quotes_powershell_metacharacters(self):
        result = planner.format_command(["tool", "system-images;android-36;google_apis;arm64-v8a"], platform_name="win32")
        self.assertIn("'system-images;android-36;google_apis;arm64-v8a'", result)

    def test_windows_command_uses_call_operator_for_quoted_executable(self):
        result = planner.format_command(["C:\\Program Files\\Python\\python.exe", "run-host-tests.py"], platform_name="win32")
        self.assertTrue(result.startswith("& 'C:\\Program Files"))

    def test_scripts_are_ascii_only(self):
        SCRIPT_PATH.read_text(encoding="ascii")
        Path(planner.darc.__file__).read_text(encoding="ascii")
        Path(planner.nuget.__file__).read_text(encoding="ascii")
        Path(__file__).read_text(encoding="ascii")


if __name__ == "__main__":
    unittest.main()
