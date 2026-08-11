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
BUILD = "4.152.0-preview.1.2+4.152.0-preview.1"


def make_run(
    run_id,
    *,
    status_value="completed",
    result="succeeded",
    upstream=None,
):
    return {
        "id": run_id,
        "status": status_value,
        "result": result,
        "buildNumber": BUILD,
        "sourceBranch": f"refs/heads/{BRANCH}",
        "sourceVersion": COMMIT,
        "queueTime": f"2026-08-06T00:00:{run_id % 60:02d}Z",
        "triggerInfo": (
            {"pipelineId": str(upstream)}
            if upstream is not None
            else {}
        ),
    }


class FakeAdo:
    def __init__(self, runs, timelines=None):
        self.runs = runs
        self.timelines = timelines or {}

    def list_runs(self, pipeline_id, branch):
        return self.runs.get(pipeline_id, [])

    def timeline(self, build_id):
        return self.timelines.get(build_id, [])


class FakeRepo:
    def resolve_target(self, value):
        return BRANCH, COMMIT

    def release_inputs(self, commit):
        return {
            "skiaSharp": "4.152.0",
            "harfBuzzSharp": "14.2.1",
            "previewLabel": "preview.1",
        }


class FakeFeed:
    def __init__(self, available=True):
        self.available = available

    def contains(self, package_id, version):
        return self.available


def complete_chain():
    return {
        26493: [make_run(102)],
        10789: [make_run(202, upstream=102)],
        15756: [make_run(302, upstream=202)],
    }


def git(cwd, *args):
    return subprocess.run(
        ["git", *args],
        cwd=cwd,
        check=True,
        capture_output=True,
        text=True,
    ).stdout.strip()


class PipelineStatusTests(unittest.TestCase):
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

    def test_complete_chain_is_ready(self):
        report = status.build_report(
            BRANCH,
            ado=FakeAdo(complete_chain()),
            repo=FakeRepo(),
            feed=FakeFeed(),
        )
        self.assertEqual(report["state"], "ready")
        self.assertEqual(report["nextAction"], "start-release-testing")
        self.assertEqual(
            [
                report["nativeRun"]["runId"],
                report["managedRun"]["runId"],
                report["testsRun"]["runId"],
            ],
            [102, 202, 302],
        )
        self.assertEqual(report["packageFeed"]["state"], "ready")

    def test_latest_native_failure_requires_retry(self):
        runs = complete_chain()
        runs[26493].append(make_run(103, result="failed"))
        report = status.build_report(
            COMMIT,
            ado=FakeAdo(runs),
            repo=FakeRepo(),
            feed=FakeFeed(),
        )
        self.assertEqual(report["state"], "blocked")
        self.assertEqual(report["nextAction"], "retry-native")
        self.assertEqual(report["nativeRun"]["runId"], 103)
        self.assertIsNone(report["managedRun"]["runId"])

    def test_waits_for_managed(self):
        report = status.build_report(
            COMMIT,
            ado=FakeAdo({26493: [make_run(102)], 10789: [], 15756: []}),
            repo=FakeRepo(),
            feed=FakeFeed(),
        )
        self.assertEqual(report["nextAction"], "wait-for-managed-trigger")

    def test_waits_for_tests_even_when_packages_exist(self):
        runs = complete_chain()
        runs[15756][0]["status"] = "inProgress"
        runs[15756][0]["result"] = None
        report = status.build_report(
            COMMIT,
            ado=FakeAdo(runs),
            repo=FakeRepo(),
            feed=FakeFeed(),
        )
        self.assertEqual(report["state"], "running")
        self.assertEqual(report["nextAction"], "wait-for-tests")
        self.assertEqual(report["packageFeed"]["state"], "ready")

    def test_waits_for_newer_managed_child(self):
        runs = complete_chain()
        runs[10789].append(
            make_run(
                203,
                status_value="inProgress",
                result=None,
                upstream=102,
            )
        )
        report = status.build_report(
            COMMIT,
            ado=FakeAdo(runs),
            repo=FakeRepo(),
            feed=FakeFeed(),
        )
        self.assertEqual(report["state"], "running")
        self.assertEqual(report["nextAction"], "wait-for-managed")

    def test_failed_tests_require_retry(self):
        runs = complete_chain()
        runs[15756][0]["result"] = "failed"
        report = status.build_report(
            COMMIT,
            ado=FakeAdo(runs),
            repo=FakeRepo(),
            feed=FakeFeed(),
        )
        self.assertEqual(report["state"], "blocked")
        self.assertEqual(report["nextAction"], "retry-tests")

    def test_missing_packages_waits(self):
        report = status.build_report(
            COMMIT,
            ado=FakeAdo(complete_chain()),
            repo=FakeRepo(),
            feed=FakeFeed(available=False),
        )
        self.assertEqual(report["state"], "waiting")
        self.assertEqual(report["nextAction"], "wait-for-packages")

    def test_stable_versions_have_internal_and_public_forms(self):
        runs = complete_chain()
        for values in runs.values():
            values[0]["buildNumber"] = "4.152.0-stable.3+4.152.0"
        repo = FakeRepo()
        repo.release_inputs = lambda commit: {
            "skiaSharp": "4.152.0",
            "harfBuzzSharp": "14.2.1",
            "previewLabel": "stable",
        }
        report = status.build_report(
            COMMIT,
            ado=FakeAdo(runs),
            repo=repo,
            feed=FakeFeed(),
        )
        self.assertEqual(
            report["packageVersions"]["test"]["SkiaSharp"],
            "4.152.0-stable.3",
        )
        self.assertEqual(
            report["packageVersions"]["public"]["SkiaSharp"],
            "4.152.0",
        )

    def test_scripts_are_ascii_only(self):
        SCRIPT_PATH.read_text(encoding="ascii")
        Path(__file__).read_text(encoding="ascii")


if __name__ == "__main__":
    unittest.main()
