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


def test_detail(build_run_id=BUILD_RUN_ID, build_number=BUILD_NUMBER):
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
        channels = [{"name": "SkiaSharp"}]
    if locations is None:
        locations = [
            "https://pkgs.dev.azure.com/dnceng/public/"
            "_packaging/skiasharp/nuget/v3/index.json"
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
            *(extra_assets or []),
        ],
    }


class FakeAdo:
    def __init__(self, runs, details=None):
        self.runs = runs
        self.details = details or {}

    def list_runs(self, pipeline_id, branch):
        return self.runs.get(pipeline_id, [])

    def run_detail(self, pipeline_id, run_id):
        return self.details.get(run_id, {})

    def timeline(self, build_id):
        return []

    def release_config(self, build_id):
        return {
            "barBuildId": BAR_BUILD_ID,
            "defaultChannelIds": [529],
            "stable": False,
        }


class FakeDarc:
    def __init__(self, record=None):
        self.record = record or bar_record()

    def get_build(self, bar_build_id):
        return self.record


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
        details={TESTS_RUN_ID: test_detail()},
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

    def test_release_config_default_channels_are_explicit_ids(self):
        self.assertEqual(
            status.parse_default_channel_ids("[529][612]"),
            [529, 612],
        )
        self.assertEqual(status.parse_default_channel_ids("[]"), [])

    def test_exact_artifact_selector_rejects_mutable_assignment(self):
        requirement = next(
            item
            for item in status.MIGRATION_REQUIREMENTS
            if item["id"] == "exact-artifact-selection"
        )
        marker = (
            "Mutable latestFromBranch artifact selection is not supported"
        )
        self.assertTrue(
            status.migration_requirement_satisfied(
                marker,
                requirement,
            )
        )
        self.assertFalse(
            status.migration_requirement_satisfied(
                marker + "\n$versionType = 'latestFromBranch'\n",
                requirement,
            )
        )

    def test_historical_branch_forms_have_explicit_release_roles(self):
        for branch in (
            "release/4.150.3",
            "release/4.150.4",
            "release/4.151.2",
            "release/4.151.3",
            "release/4.152.0-rc.1",
            "release/4.150.4-preview.1",
            "release/4.151.3.1",
        ):
            with self.subTest(branch=branch):
                self.assertEqual(
                    status.normalize_release_branch(branch),
                    branch,
                )
        for branch in ("release/4.150.x", "release/4.151.x"):
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
            (scripts / "azure-templates-stages-signing.yml").write_text(
                "$_.Name.Contains('.0.0.0-branch.')\n"
                "Copy-Item $transportPackages.FullName $nonShipping\n",
                encoding="ascii",
            )
            shared = scripts / "infra" / "native" / "shared"
            shared.mkdir(parents=True)
            (shared / "set-build-variables.ps1").write_text(
                "Set-BuildVariable DOTNET_FINAL_VERSION_KIND "
                "$finalVersionKind\n",
                encoding="ascii",
            )
            nuget = scripts / "infra" / "package" / "nuget"
            nuget.mkdir(parents=True)
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
            git(seed, "add", "scripts")
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
            ado=FakeAdo(complete_chain(), details={TESTS_RUN_ID: test_detail(999)}),
            repo=FakeRepo(),
            darc=FakeDarc(),
        )
        self.assertEqual(report["nextAction"], "wait-for-tests-trigger")
        self.assertIsNone(report["testsRun"]["runId"])

    def test_failed_connected_tests_require_retry(self):
        runs = complete_chain()
        runs[1630][0]["result"] = "failed"
        report = status.build_report(
            COMMIT,
            ado=FakeAdo(runs, details={TESTS_RUN_ID: test_detail()}),
            repo=FakeRepo(),
            darc=FakeDarc(),
        )
        self.assertEqual(report["state"], "blocked")
        self.assertEqual(report["nextAction"], "retry-tests")

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

    def test_channel_names_do_not_select_release_assets(self):
        report = status.build_report(
            COMMIT,
            ado=complete_ado(),
            repo=FakeRepo(),
            darc=FakeDarc(
                bar_record(
                    channels=[{"name": "General Testing"}],
                )
            ),
        )
        self.assertEqual(report["nextAction"], "start-release-testing")

    def test_wrong_or_missing_signed_feed_route_is_blocked(self):
        for locations in (
            [],
            [
                "https://pkgs.dev.azure.com/dnceng/public/"
                "_packaging/skiasharp-transport/nuget/v3/index.json"
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
            "defaultChannelIds": [529],
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

    def test_historical_stable_and_rc_package_families(self):
        cases = (
            (
                {"skiaSharp": "4.150.3", "harfBuzzSharp": "14.2.1.3",
                 "previewLabel": "stable"},
                "4.150.3",
                "14.2.1.3",
            ),
            (
                {"skiaSharp": "4.151.2", "harfBuzzSharp": "14.2.1.102",
                 "previewLabel": "stable"},
                "4.151.2",
                "14.2.1.102",
            ),
            (
                {"skiaSharp": "4.152.0", "harfBuzzSharp": "14.2.1.200",
                 "previewLabel": "rc.1"},
                "4.152.0-rc.1.26425.1",
                "14.2.1.200-rc.1.26425.1",
            ),
        )
        for inputs, skia_version, harfbuzz_version in cases:
            with self.subTest(inputs=inputs):
                record = bar_record(
                    stable=inputs["previewLabel"] == "stable",
                    skia_version=skia_version,
                    harfbuzz_version=harfbuzz_version,
                )
                versions, _ = status.package_versions_from_bar(
                    record,
                    inputs,
                )
                self.assertEqual(
                    versions["test"],
                    {
                        "SkiaSharp": skia_version,
                        "HarfBuzzSharp": harfbuzz_version,
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
                    "version": "0.0.0-branch.release-4.152.0-rc.1.1",
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

    def test_missing_historical_migration_surface_fails_closed(self):
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
            "backport-arcade-release",
        )
        self.assertEqual(
            report["migration"]["missing"][0]["id"],
            "combined-build",
        )

    def test_scripts_are_ascii_only(self):
        SCRIPT_PATH.read_text(encoding="ascii")
        Path(__file__).read_text(encoding="ascii")


if __name__ == "__main__":
    unittest.main()
