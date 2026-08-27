#!/usr/bin/env python3

import importlib.util
from pathlib import Path
import subprocess
import sys
import tempfile
from types import SimpleNamespace
import unittest
from unittest import mock


SCRIPT_PATH = Path(__file__).resolve().parent.parent / "pipeline-status.py"
SPEC = importlib.util.spec_from_file_location("pipeline_status", SCRIPT_PATH)
status = importlib.util.module_from_spec(SPEC)
sys.modules[SPEC.name] = status
SPEC.loader.exec_module(status)


COMMIT = "a" * 40
BRANCH = "release/4.152.0-preview.1"
BUILD_NUMBER = "4.152.0-preview.1.26421.1"
BUILD_RUN_ID = 202
TESTS_RUN_ID = 302
BAR_BUILD_ID = 400


def make_run(
    run_id,
    *,
    status_value="completed",
    result="succeeded",
):
    return {
        "id": run_id,
        "status": status_value,
        "result": result,
        "buildNumber": BUILD_NUMBER,
        "sourceBranch": f"refs/heads/{BRANCH}",
        "sourceVersion": COMMIT,
        "queueTime": f"2026-08-06T00:00:{run_id % 60:02d}Z",
    }


def make_test_detail(build_run_id=BUILD_RUN_ID, build_number=BUILD_NUMBER):
    return {
        "resources": {
            "pipelines": {
                "SkiaSharp": {
                    "pipeline": {
                        "folder": r"\dotnet\skiasharp",
                        "id": build_run_id,
                        "name": "skiasharp-package",
                    },
                    "version": build_number,
                }
            }
        }
    }


def full_path_test_detail(
    build_run_id=BUILD_RUN_ID,
    build_number=BUILD_NUMBER,
):
    detail = make_test_detail(build_run_id, build_number)
    pipeline = detail["resources"]["pipelines"]["SkiaSharp"]["pipeline"]
    pipeline["name"] = r"\dotnet\skiasharp\skiasharp-package"
    return detail


def bar_record(
    *,
    stable=False,
    channels=None,
    locations=None,
    skia_version="4.152.0-preview.1.26421.1",
    harfbuzz_version="14.2.1-preview.1.26421.1",
    extra_assets=None,
):
    if channels is None:
        channels = [{"name": ".NET Libraries"}]
    if locations is None:
        locations = [
            "https://pkgs.dev.azure.com/dnceng/public/"
            "_packaging/dotnet-libraries/nuget/v3/index.json"
        ]
    if extra_assets is None:
        transport_locations = [
            "https://pkgs.dev.azure.com/dnceng/public/"
            "_packaging/dotnet-libraries-transport/nuget/v3/index.json"
        ]
        extra_assets = [
            {
                "name": "_NativeAssets",
                "version": "0.0.0-branch.release-4.152.0-preview.1.1",
                "nonShipping": True,
                "locations": transport_locations,
            },
            {
                "name": "_NuGets",
                "version": "0.0.0-branch.release-4.152.0-preview.1.1",
                "nonShipping": True,
                "locations": transport_locations,
            },
        ]
    return {
        "id": BAR_BUILD_ID,
        "commit": COMMIT,
        "azureDevOpsAccount": "dnceng",
        "azureDevOpsProject": "internal",
        "azureDevOpsBuildDefinitionId": 1642,
        "azureDevOpsBuildId": BUILD_RUN_ID,
        "azureDevOpsBuildNumber": BUILD_NUMBER,
        "azureDevOpsBranch": f"refs/heads/{BRANCH}",
        "stable": stable,
        "channels": channels,
        "assets": [
            {
                "name": "SkiaSharp",
                "version": skia_version,
                "nonShipping": False,
                "locations": locations,
            },
            {
                "name": "HarfBuzzSharp",
                "version": harfbuzz_version,
                "nonShipping": False,
                "locations": locations,
            },
            *extra_assets,
        ],
    }


class FakeAdo:
    def __init__(self, runs, details=None, timelines=None):
        self.runs = runs
        self.details = details or {}
        self.timelines = timelines or {}

    def list_runs(self, pipeline_id, branch):
        return self.runs.get(pipeline_id, [])

    def run_detail(self, pipeline_id, run_id):
        return self.details.get(run_id, {})

    def timeline(self, build_id):
        return self.timelines.get(
            build_id,
            [
                {
                    "type": "Stage",
                    "name": "api_scan",
                    "state": "completed",
                    "result": "succeeded",
                }
            ],
        )

    def release_config(self, build_id):
        return {
            "barBuildId": BAR_BUILD_ID,
            "defaultChannelIds": [1648],
            "stable": False,
        }


class FakeDarc:
    def __init__(self, record=None):
        self.record = record or bar_record()

    def get_build(self, bar_build_id):
        return self.record


class FakeFeeds:
    def __init__(self, available=True, overrides=None, error_on=None):
        self.available = available
        self.overrides = overrides or {}
        self.error_on = error_on
        self.requests = []

    def has_version(self, index_url, package_id, version):
        self.requests.append((index_url, package_id, version))
        if self.error_on == (index_url, package_id):
            raise status.StatusError("feed query failed")
        if not self.available:
            return False
        if (index_url, package_id) in self.overrides:
            return self.overrides[(index_url, package_id)]
        expected = (
            status.TRANSPORT_FEED_INDEX
            if package_id.startswith("_")
            else status.PRODUCT_FEED_INDEX
        )
        return index_url == expected


class FakeRepo:
    def resolve_target(self, value):
        return BRANCH, COMMIT

    def release_inputs(self, commit):
        return {
            "skiaSharp": "4.152.0",
            "harfBuzzSharp": "14.2.1",
            "previewLabel": "preview.1",
        }

    def release_prerequisites(self, commit):
        return {"state": "ready", "missing": []}


def complete_chain():
    return {
        1642: [make_run(BUILD_RUN_ID)],
        1630: [make_run(TESTS_RUN_ID)],
    }


def complete_ado():
    return FakeAdo(
        complete_chain(),
        details={TESTS_RUN_ID: make_test_detail()},
    )


def git(cwd, *args):
    return subprocess.run(
        ["git", *args],
        cwd=cwd,
        check=True,
        capture_output=True,
        text=True,
    ).stdout.strip()


class PipelineStatusTests(unittest.TestCase):
    def test_pipeline_contract_uses_dnceng_topology(self):
        self.assertEqual(status.ORG, "https://dev.azure.com/dnceng")
        self.assertEqual(status.PROJECT, "internal")
        self.assertEqual(
            [(item["name"], item["id"]) for item in status.PIPELINES],
            [("skiasharp-package", 1642), ("skiasharp-tests", 1630)],
        )
        self.assertEqual(
            status.BUILD_PIPELINE_SOURCE,
            r"\dotnet\skiasharp\skiasharp-package",
        )
        self.assertEqual(status.PRODUCT_CHANNEL_ID, 1648)
        self.assertEqual(
            status.PRODUCT_FEED_MARKER,
            "/_packaging/dotnet-libraries/",
        )

    def test_release_config_default_channels_are_explicit_ids(self):
        self.assertEqual(
            status.parse_default_channel_ids("[1648][3882]"),
            [1648, 3882],
        )
        self.assertEqual(status.parse_default_channel_ids("[]"), [])

    def test_feed_verifier_rejects_non_nuget_path_components(self):
        verifier = status.FeedVerifier()
        verifier.flat_bases[status.TRANSPORT_FEED_INDEX] = (
            "https://example.test/flat"
        )
        for version in (
            "../_nugets/0.0.0-branch.main.105",
            "1/2/3",
            r"1\2\3",
            "1%2f2",
        ):
            with (
                self.subTest(version=version),
                self.assertRaisesRegex(
                    status.StatusError,
                    "invalid NuGet version",
                ),
            ):
                verifier.has_version(
                    status.TRANSPORT_FEED_INDEX,
                    "_NuGets",
                    version,
                )

    def test_feed_verifier_wraps_non_json_service_index(self):
        response = mock.MagicMock()
        response.__enter__.return_value = response
        decode_error = status.json.JSONDecodeError("invalid", "<html>", 0)
        with (
            mock.patch.object(
                status.urllib.request,
                "urlopen",
                return_value=response,
            ),
            mock.patch.object(
                status.json,
                "load",
                side_effect=decode_error,
            ),
            self.assertRaisesRegex(
                status.StatusError,
                "failed to read approved package feed",
            ),
        ):
            status.FeedVerifier().flat_base(status.PRODUCT_FEED_INDEX)

    def test_release_requirement_matching_semantics(self):
        requirement = {
            "pattern": r"required",
            "forbiddenPattern": r"forbidden",
        }
        self.assertTrue(
            status.release_requirement_satisfied(
                "required",
                requirement,
            )
        )
        self.assertFalse(
            status.release_requirement_satisfied(
                "required forbidden",
                requirement,
            )
        )
        self.assertFalse(
            status.release_requirement_satisfied(
                "unrelated",
                requirement,
            )
        )
        absent = {"absent": True}
        self.assertTrue(status.release_requirement_satisfied(None, absent))
        self.assertFalse(
            status.release_requirement_satisfied("present", absent)
        )

    def test_historical_branch_forms_have_explicit_release_roles(self):
        for branch in (
            "release/9.9.4",
            "release/9.9.4-rc.1",
            "release/9.9.4-preview.1",
            "release/9.9.4.1",
        ):
            with self.subTest(branch=branch):
                self.assertEqual(
                    status.normalize_release_branch(branch),
                    branch,
                )
        for branch in ("release/9.9.x",):
            with self.subTest(branch=branch):
                with self.assertRaisesRegex(
                    status.StatusError,
                    "integration/maintenance branch",
                ):
                    status.normalize_release_branch(branch)

    def test_azure_cli_uses_resolved_cmd_launcher(self):
        az_path = r"C:\Program Files\Microsoft SDKs\Azure\CLI2\wbin\az.CMD"
        with mock.patch.object(
            status.shutil,
            "which",
            return_value=az_path,
        ), mock.patch.object(
            status.subprocess,
            "run",
            return_value=SimpleNamespace(
                stdout="[]\n",
                stderr="",
                returncode=0,
            ),
        ) as run:
            result = status.AzureDevOps().json(["pipelines", "runs", "list"])
        self.assertEqual(run.call_args.args[0][0], az_path)
        self.assertFalse(run.call_args.kwargs.get("shell", False))
        self.assertEqual(result, [])

    def test_branch_shortcut_and_sha_resolve_same_commit(self):
        with tempfile.TemporaryDirectory() as temporary:
            root = Path(temporary)
            remote = root / "remote.git"
            seed = root / "seed"
            work = root / "work"
            seed.mkdir()
            git(
                root,
                "init",
                "--bare",
                "--quiet",
                "--initial-branch=main",
                str(remote),
            )
            git(seed, "init", "--quiet", "-b", "main")
            git(seed, "config", "user.name", "Test User")
            git(seed, "config", "user.email", "test@example.com")
            scripts = seed / "scripts"
            scripts.mkdir()
            (scripts / "VERSIONS.txt").write_text(
                "SkiaSharp nuget 4.152.0\n"
                "HarfBuzzSharp nuget 14.2.1\n",
                encoding="ascii",
            )
            (scripts / "azure-templates-variables.yml").write_text(
                "variables:\n  PREVIEW_LABEL: 'preview.1'\n",
                encoding="ascii",
            )
            (scripts / "azure-pipelines-package.yml").write_text(
                "buildPipelineType: 'build'\n",
                encoding="ascii",
            )
            (scripts / "azure-pipelines-tests.yml").write_text(
                r"source: '\dotnet\skiasharp\skiasharp-package'"
                "\n",
                encoding="ascii",
            )
            (
                scripts / "azure-templates-steps-download-artifacts.yml"
            ).write_text(
                "Mutable latestFromBranch artifact selection is not "
                "supported\n",
                encoding="ascii",
            )
            package = scripts / "infra" / "package"
            package.mkdir(parents=True)
            (package / "nuget.cake").write_text(
                'if (PREVIEW_LABEL.StartsWith ("pr."))\n'
                'versions.Add ("pr", version);\n'
                "else\n"
                'versions.Add ("branch", version);\n'
                'Task ("nuget-assemble-arcade-assets")\n'
                "var transportPackages = GetNuGetPackages "
                "(OUTPUT_SPECIAL_NUGETS_PATH, \"transport\");\n"
                "foreach (var package in transportPackages)\n"
                "CopyFileToDirectory (package, nonShipping);\n"
                'if (productNames.Contains ($"{packageBaseName}.symbols.nupkg")) '
                "{ continue; }\n"
                "var packagePdbRoot = MakeAbsolute "
                "(OUTPUT_PDB_ARTIFACTS_PATH.Combine (packageBaseName));\n"
                'if (entryPath.StartsWith ("ref/")) { continue; }\n'
                "var targetPath = packagePdbRoot"
                ".CombineWithFilePath (entryPath).Collapse ();\n"
                "var relative = packagePdbRoot.GetRelativePath (targetPath);\n"
                'relative.Segments.Any (segment => segment == "..");\n'
                'throw new Exception ("PDB package path escapes");\n'
                "if (pdbCount == 0) "
                'OUTPUT_PDB_ARTIFACTS_PATH.CombineWithFilePath (".empty");\n',
                encoding="ascii",
            )
            tests = package / "tests"
            tests.mkdir()
            (tests / "AssembleArcadeAssets.Tests.ps1").write_text(
                "'../escape.pdb'\n"
                "Invoke-Assembly -ExpectFailure\n"
                "$global:LASTEXITCODE = 0\n"
                "throw 'An escaping PDB path wrote outside'\n",
                encoding="ascii",
            )
            (seed / "build.cake").write_text(
                'Task ("nuget").IsDependentOn '
                '("nuget-assemble-arcade-assets");\n'
                'Task ("nuget-assemble-arcade-assets");\n',
                encoding="ascii",
            )
            (scripts / "azure-templates-stages-prepare.yml").write_text(
                "SetBuildVariables.Tests.ps1\n"
                "PrepareApiScanInputs.Tests.ps1\n"
                "repo-deps.py validate\n",
                encoding="ascii",
            )
            (scripts / "azure-templates-stages-package.yml").write_text(
                "target: nuget\n"
                "postBuildSteps:\n"
                "  - pwsh: AssembleArcadeAssets.Tests.ps1\n"
                "publishArtifacts:\n"
                "name: nuget\n"
                "name: nuget_special\n"
                "name: arcade_shipping\n"
                "name: arcade_nonshipping\n"
                "name: PdbArtifacts\n"
                "isProduction: false\n",
                encoding="ascii",
            )
            (scripts / "azure-templates-stages-signing.yml").write_text(
                "artifactName: arcade_shipping_signed\n"
                "artifactName: arcade_shipping\n"
                "stage: publish_assets\n"
                "artifactName: arcade_shipping_signed\n"
                "artifactName: arcade_nonshipping\n"
                "dependsOn: generate_arcade_manifest\n"
                "validateDependsOn:\n"
                "  - publish_assets\n",
                encoding="ascii",
            )
            infra_shared = scripts / "infra" / "shared"
            infra_shared.mkdir(parents=True)
            (infra_shared / "download.cake").write_text(
                'if (PREVIEW_LABEL.StartsWith ("pr."))\n'
                "else if (!string.IsNullOrEmpty(GIT_BRANCH_NAME))\n"
                "else version += \"branch.main\";\n",
                encoding="ascii",
            )
            shared = scripts / "infra" / "native" / "shared"
            shared.mkdir(parents=True)
            (shared / "set-build-variables.ps1").write_text(
                "Set-BuildVariable DOTNET_FINAL_VERSION_KIND "
                "$finalVersionKind\n",
                encoding="ascii",
            )
            infra_shared = scripts / "infra" / "shared"
            (infra_shared / "shared.cake").write_text(
                'DirectoryPath ROOT_OUTPUT_PATH = MakeAbsolute(Directory('
                'Argument("outputPath", "output")));\n'
                'DirectoryPath OUTPUT_NUGETS_PATH = '
                'ROOT_OUTPUT_PATH.Combine("nugets");\n'
                'DirectoryPath OUTPUT_SPECIAL_NUGETS_PATH = '
                'ROOT_OUTPUT_PATH.Combine("nugets-special");\n'
                'DirectoryPath OUTPUT_ARCADE_ASSETS_PATH = '
                'ROOT_OUTPUT_PATH.Combine("arcade-assets");\n'
                'DirectoryPath OUTPUT_PDB_ARTIFACTS_PATH = '
                'ROOT_OUTPUT_PATH.Combine("pdbs");\n',
                encoding="ascii",
            )
            nuget = package / "nuget"
            nuget.mkdir()
            metadata = (
                "<copyright>Microsoft Corporation</copyright>"
                '<license type="expression">MIT</license>'
                "<projectUrl>https://example.test</projectUrl>\n"
            )
            for package in ("NativeAssets", "NuGets", "Dependencies"):
                (nuget / f"_{package}.nuspec").write_text(
                    metadata,
                    encoding="ascii",
                )
            git(seed, "add", "scripts", "build.cake")
            git(seed, "commit", "-m", "Release")
            git(seed, "branch", BRANCH)
            git(seed, "remote", "add", "origin", str(remote))
            git(seed, "push", "origin", "main", BRANCH)
            git(root, "clone", "--quiet", str(remote), str(work))

            repo = status.GitRepository(work)
            branch, branch_sha = repo.resolve_target(BRANCH)
            sha_branch, sha = repo.resolve_target(branch_sha)

            self.assertEqual(branch, BRANCH)
            self.assertEqual(sha_branch, BRANCH)
            self.assertEqual(sha, branch_sha)
            self.assertEqual(
                repo.release_inputs(sha),
                {
                    "skiaSharp": "4.152.0",
                    "harfBuzzSharp": "14.2.1",
                    "previewLabel": "preview.1",
                },
            )
            self.assertEqual(
                repo.release_prerequisites(sha),
                {"state": "ready", "missing": []},
            )

    def test_complete_chain_is_ready_with_exact_bar_assets(self):
        report = status.build_report(
            BRANCH,
            ado=complete_ado(),
            repo=FakeRepo(),
            darc=FakeDarc(),
        )
        self.assertEqual(report["state"], "ready")
        self.assertEqual(report["nextAction"], "start-release-testing")
        self.assertEqual(report["buildRun"]["runId"], BUILD_RUN_ID)
        self.assertEqual(report["testsRun"]["runId"], TESTS_RUN_ID)
        self.assertEqual(report["barBuild"]["id"], BAR_BUILD_ID)
        self.assertEqual(report["barBuild"]["state"], "ready")
        self.assertEqual(
            report["packageVersions"]["test"]["SkiaSharp"],
            "4.152.0-preview.1.26421.1",
        )

    def test_newest_exact_build_failure_requires_retry(self):
        runs = complete_chain()
        runs[1642].append(make_run(203, result="failed"))
        report = status.build_report(
            COMMIT,
            ado=FakeAdo(runs),
            repo=FakeRepo(),
            darc=FakeDarc(),
        )
        self.assertEqual(report["state"], "blocked")
        self.assertEqual(report["nextAction"], "retry-build")
        self.assertEqual(report["buildRun"]["runId"], 203)
        self.assertIsNone(report["testsRun"]["runId"])

    def test_failed_build_without_mapping_is_actionable(self):
        runs = complete_chain()
        runs[1642][0]["result"] = "failed"
        ado = FakeAdo(runs)
        ado.release_config = lambda build_id: {
            "barBuildId": BAR_BUILD_ID,
            "defaultChannelIds": [],
            "stable": False,
        }
        report = status.build_report(
            COMMIT,
            ado=ado,
            repo=FakeRepo(),
            darc=FakeDarc(),
        )
        self.assertEqual(
            report["nextAction"],
            "configure-default-channels",
        )

    def test_waits_for_connected_tests(self):
        report = status.build_report(
            COMMIT,
            ado=FakeAdo(
                complete_chain(),
                details={TESTS_RUN_ID: make_test_detail(999)},
            ),
            repo=FakeRepo(),
            darc=FakeDarc(),
        )
        self.assertEqual(report["nextAction"], "wait-for-tests-trigger")
        self.assertIsNone(report["testsRun"]["runId"])

    def test_connected_tests_accept_live_full_resource_name(self):
        report = status.build_report(
            COMMIT,
            ado=FakeAdo(
                complete_chain(),
                details={TESTS_RUN_ID: full_path_test_detail()},
            ),
            repo=FakeRepo(),
            darc=FakeDarc(),
        )
        self.assertEqual(report["testsRun"]["runId"], TESTS_RUN_ID)
        self.assertEqual(report["nextAction"], "start-release-testing")

    def test_failed_connected_tests_require_retry(self):
        runs = complete_chain()
        runs[1630][0]["result"] = "failed"
        report = status.build_report(
            COMMIT,
            ado=FakeAdo(
                runs,
                details={TESTS_RUN_ID: make_test_detail()},
            ),
            repo=FakeRepo(),
            darc=FakeDarc(),
        )
        self.assertEqual(report["state"], "blocked")
        self.assertEqual(report["nextAction"], "retry-tests")

    def test_missing_api_scan_requires_explicit_release_build(self):
        report = status.build_report(
            COMMIT,
            ado=FakeAdo(
                complete_chain(),
                details={TESTS_RUN_ID: make_test_detail()},
                timelines={BUILD_RUN_ID: []},
            ),
            repo=FakeRepo(),
            darc=FakeDarc(),
        )
        self.assertEqual(report["state"], "blocked")
        self.assertEqual(report["nextAction"], "run-api-scan")
        self.assertEqual(report["apiScan"]["state"], "missing")

    def test_failed_api_scan_blocks_release(self):
        runs = complete_chain()
        runs[1642][0]["result"] = "failed"
        report = status.build_report(
            COMMIT,
            ado=FakeAdo(
                runs,
                details={TESTS_RUN_ID: make_test_detail()},
                timelines={
                    BUILD_RUN_ID: [
                        {
                            "type": "Stage",
                            "name": "api_scan",
                            "state": "completed",
                            "result": "failed",
                        }
                    ]
                },
            ),
            repo=FakeRepo(),
            darc=FakeDarc(),
        )
        self.assertEqual(report["nextAction"], "retry-api-scan")
        self.assertEqual(report["apiScan"]["state"], "failed")

    def test_api_scan_timeline_error_is_not_reported_as_bar_error(self):
        class TimelineErrorAdo(FakeAdo):
            def timeline(self, build_id):
                raise status.StatusError("timeline unavailable")

        report = status.build_report(
            COMMIT,
            ado=TimelineErrorAdo(
                complete_chain(),
                details={TESTS_RUN_ID: make_test_detail()},
            ),
            repo=FakeRepo(),
            darc=FakeDarc(),
        )
        self.assertEqual(report["nextAction"], "retry-api-scan-check")
        self.assertIsNotNone(report["barBuild"])
        self.assertIn("timeline unavailable", report["warnings"][0])

    def test_bar_waits_for_default_channel_asset_locations(self):
        ado = complete_ado()
        ado.release_config = lambda build_id: {
            "barBuildId": BAR_BUILD_ID,
            "defaultChannelIds": [],
            "stable": False,
        }
        report = status.build_report(
            COMMIT,
            ado=ado,
            repo=FakeRepo(),
            darc=FakeDarc(bar_record(channels=[], locations=[])),
        )
        self.assertEqual(report["state"], "blocked")
        self.assertEqual(
            report["nextAction"],
            "configure-default-channels",
        )
        self.assertEqual(report["barBuild"]["channels"], [])

    def test_general_testing_channel_is_not_release_ready(self):
        ado = complete_ado()
        ado.release_config = lambda build_id: {
            "barBuildId": BAR_BUILD_ID,
            "defaultChannelIds": [3882],
            "stable": False,
        }
        report = status.build_report(
            COMMIT,
            ado=ado,
            repo=FakeRepo(),
            darc=FakeDarc(
                bar_record(
                    channels=[{"name": ".NET Libraries Internal"}],
                )
            ),
        )
        self.assertEqual(
            report["nextAction"],
            "configure-default-channels",
        )

    def test_wrong_or_missing_signed_feed_route_is_blocked(self):
        for locations in (
            [],
            [
                "https://pkgs.dev.azure.com/dnceng/public/"
                "_packaging/dotnet-libraries-transport/nuget/v3/index.json"
            ],
        ):
            with self.subTest(locations=locations):
                report = status.build_report(
                    COMMIT,
                    ado=complete_ado(),
                    repo=FakeRepo(),
                    darc=FakeDarc(bar_record(locations=locations)),
                )
                self.assertEqual(report["state"], "blocked")
                self.assertEqual(
                    report["nextAction"],
                    "configure-feed-routing",
                )

    def test_missing_bar_locations_probe_exact_approved_feed_versions(self):
        feeds = FakeFeeds()
        record = bar_record(locations=[])
        for asset in record["assets"]:
            asset["locations"] = []
        report = status.build_report(
            COMMIT,
            ado=complete_ado(),
            repo=FakeRepo(),
            darc=FakeDarc(record),
            feeds=feeds,
        )
        self.assertEqual(report["nextAction"], "start-release-testing")
        self.assertEqual(
            feeds.requests,
            [
                (
                    status.PRODUCT_FEED_INDEX,
                    "SkiaSharp",
                    "4.152.0-preview.1.26421.1",
                ),
                (
                    status.TRANSPORT_FEED_INDEX,
                    "SkiaSharp",
                    "4.152.0-preview.1.26421.1",
                ),
                (
                    status.PRODUCT_FEED_INDEX,
                    "HarfBuzzSharp",
                    "14.2.1-preview.1.26421.1",
                ),
                (
                    status.TRANSPORT_FEED_INDEX,
                    "HarfBuzzSharp",
                    "14.2.1-preview.1.26421.1",
                ),
                (
                    status.TRANSPORT_FEED_INDEX,
                    "_NativeAssets",
                    "0.0.0-branch.release-4.152.0-preview.1.1",
                ),
                (
                    status.PRODUCT_FEED_INDEX,
                    "_NativeAssets",
                    "0.0.0-branch.release-4.152.0-preview.1.1",
                ),
                (
                    status.TRANSPORT_FEED_INDEX,
                    "_NuGets",
                    "0.0.0-branch.release-4.152.0-preview.1.1",
                ),
                (
                    status.PRODUCT_FEED_INDEX,
                    "_NuGets",
                    "0.0.0-branch.release-4.152.0-preview.1.1",
                ),
            ],
        )

    def test_missing_bar_locations_fail_when_exact_versions_are_absent(self):
        report = status.build_report(
            COMMIT,
            ado=complete_ado(),
            repo=FakeRepo(),
            darc=FakeDarc(bar_record(locations=[])),
            feeds=FakeFeeds(available=False),
        )
        self.assertEqual(report["nextAction"], "configure-feed-routing")

    def test_null_locations_reject_cross_feed_duplicates(self):
        record = bar_record(locations=[])
        for asset in record["assets"]:
            asset["locations"] = []
        report = status.build_report(
            COMMIT,
            ado=complete_ado(),
            repo=FakeRepo(),
            darc=FakeDarc(record),
            feeds=FakeFeeds(
                overrides={
                    (status.TRANSPORT_FEED_INDEX, "SkiaSharp"): True,
                }
            ),
        )
        self.assertEqual(report["nextAction"], "configure-feed-routing")

    def test_null_locations_reject_wrong_feed_only(self):
        cases = (
            (
                "SkiaSharp",
                status.PRODUCT_FEED_INDEX,
                status.TRANSPORT_FEED_INDEX,
            ),
            (
                "_NuGets",
                status.TRANSPORT_FEED_INDEX,
                status.PRODUCT_FEED_INDEX,
            ),
        )
        for package_id, expected, wrong in cases:
            with self.subTest(package_id=package_id):
                record = bar_record(locations=[])
                for asset in record["assets"]:
                    asset["locations"] = []
                report = status.build_report(
                    COMMIT,
                    ado=complete_ado(),
                    repo=FakeRepo(),
                    darc=FakeDarc(record),
                    feeds=FakeFeeds(
                        overrides={
                            (expected, package_id): False,
                            (wrong, package_id): True,
                        }
                    ),
                )
                self.assertEqual(
                    report["nextAction"],
                    "configure-feed-routing",
                )

    def test_bar_locations_reject_correct_and_opposite_feeds(self):
        for package_id, opposite in (
            ("SkiaSharp", status.TRANSPORT_FEED_INDEX),
            ("_NuGets", status.PRODUCT_FEED_INDEX),
        ):
            with self.subTest(package_id=package_id):
                record = bar_record()
                asset = next(
                    item
                    for item in record["assets"]
                    if item["name"] == package_id
                )
                asset["locations"].append(opposite)
                report = status.build_report(
                    COMMIT,
                    ado=complete_ado(),
                    repo=FakeRepo(),
                    darc=FakeDarc(record),
                )
                self.assertEqual(
                    report["nextAction"],
                    "configure-feed-routing",
                )

    def test_bar_locations_allow_neutral_source_artifact(self):
        record = bar_record()
        for asset in record["assets"]:
            asset["locations"].append(
                "https://dev.azure.com/dnceng/internal/_apis/build/"
                "builds/3058119/artifacts"
            )
        report = status.build_report(
            COMMIT,
            ado=complete_ado(),
            repo=FakeRepo(),
            darc=FakeDarc(record),
        )
        self.assertEqual(report["nextAction"], "start-release-testing")

    def test_opposite_feed_query_errors_fail_closed(self):
        record = bar_record(locations=[])
        for asset in record["assets"]:
            asset["locations"] = []
        report = status.build_report(
            COMMIT,
            ado=complete_ado(),
            repo=FakeRepo(),
            darc=FakeDarc(record),
            feeds=FakeFeeds(
                error_on=(status.TRANSPORT_FEED_INDEX, "SkiaSharp")
            ),
        )
        self.assertEqual(report["nextAction"], "retry-bar-check")
        self.assertIn("feed query failed", report["warnings"][0])

    def test_stable_bar_uses_exact_package_versions(self):
        repo = FakeRepo()
        repo.release_inputs = lambda commit: {
            "skiaSharp": "4.152.0",
            "harfBuzzSharp": "14.2.1",
            "previewLabel": "stable",
        }
        ado = complete_ado()
        ado.release_config = lambda build_id: {
            "barBuildId": BAR_BUILD_ID,
            "defaultChannelIds": [1648],
            "stable": True,
        }
        record = bar_record(
            stable=True,
            skia_version="4.152.0",
            harfbuzz_version="14.2.1",
        )
        report = status.build_report(
            COMMIT,
            ado=ado,
            repo=repo,
            darc=FakeDarc(record),
        )
        self.assertEqual(
            report["packageVersions"],
            {
                "test": {
                    "SkiaSharp": "4.152.0",
                    "HarfBuzzSharp": "14.2.1",
                },
                "public": {
                    "SkiaSharp": "4.152.0",
                    "HarfBuzzSharp": "14.2.1",
                },
            },
        )

    def test_bar_build_must_match_exact_azure_build(self):
        record = bar_record()
        record["azureDevOpsBuildId"] = 999
        report = status.build_report(
            COMMIT,
            ado=complete_ado(),
            repo=FakeRepo(),
            darc=FakeDarc(record),
        )
        self.assertEqual(report["state"], "blocked")
        self.assertEqual(report["nextAction"], "retry-bar-check")
        self.assertIn("azureDevOpsBuildId=999", report["warnings"][0])

    def test_duplicate_nonshipping_transport_ids_are_blocked(self):
        record = bar_record(
            extra_assets=[
                {
                    "name": "_NuGets",
                    "version": "0.0.0-branch.release-9.9.4.1",
                    "nonShipping": True,
                    "locations": ["transport"],
                },
                {
                    "name": "_NuGets",
                    "version": "0.0.0-commit.abc123.1",
                    "nonShipping": True,
                    "locations": ["transport"],
                },
            ]
        )
        report = status.build_report(
            COMMIT,
            ado=complete_ado(),
            repo=FakeRepo(),
            darc=FakeDarc(record),
        )
        self.assertEqual(report["state"], "blocked")
        self.assertEqual(report["nextAction"], "retry-bar-check")
        self.assertIn(
            "duplicate NonShipping transport asset IDs",
            report["warnings"][0],
        )

    def test_missing_release_tooling_fails_closed(self):
        repo = FakeRepo()
        repo.release_prerequisites = lambda commit: {
            "state": "missing",
            "missing": [
                {
                    "id": "combined-build",
                    "path": "scripts/azure-pipelines-package.yml",
                    "detail": "backport combined Build",
                }
            ],
        }
        report = status.build_report(
            COMMIT,
            ado=FakeAdo({}),
            repo=repo,
            darc=FakeDarc(),
        )
        self.assertEqual(report["state"], "blocked")
        self.assertEqual(
            report["nextAction"],
            "update-release-tooling",
        )
        self.assertEqual(
            report["prerequisites"]["missing"][0]["id"],
            "combined-build",
        )

    def test_scripts_are_ascii_only(self):
        SCRIPT_PATH.read_text(encoding="ascii")
        Path(__file__).read_text(encoding="ascii")


if __name__ == "__main__":
    unittest.main()
