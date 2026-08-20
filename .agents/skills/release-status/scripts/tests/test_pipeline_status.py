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
):
    if channels is None:
        channels = [{"name": "SkiaSharp"}]
    if locations is None:
        locations = ["https://example.test/skiasharp"]
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
            "defaultChannels": "[529]",
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

    def test_registered_bar_requires_explicit_channel_promotion(self):
        report = status.build_report(
            COMMIT,
            ado=complete_ado(),
            repo=FakeRepo(),
            darc=FakeDarc(bar_record(channels=[], locations=[])),
        )
        self.assertEqual(report["state"], "waiting")
        self.assertEqual(report["nextAction"], "promote-bar")
        self.assertEqual(
            report["barBuild"]["promotionCommand"],
            "darc add-build-to-channel --id 400 --channel SkiaSharp",
        )

    def test_channel_without_locations_waits_for_bar_assets(self):
        report = status.build_report(
            COMMIT,
            ado=complete_ado(),
            repo=FakeRepo(),
            darc=FakeDarc(bar_record(locations=[])),
        )
        self.assertEqual(report["nextAction"], "wait-for-bar-assets")

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
            "defaultChannels": "[529]",
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

    def test_scripts_are_ascii_only(self):
        SCRIPT_PATH.read_text(encoding="ascii")
        Path(__file__).read_text(encoding="ascii")


if __name__ == "__main__":
    unittest.main()
