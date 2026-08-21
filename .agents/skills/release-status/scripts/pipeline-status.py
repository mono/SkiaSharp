#!/usr/bin/env python3
"""Report the latest SkiaSharp release pipeline chain for a branch or commit."""

from __future__ import annotations

import argparse
import json
from pathlib import Path
import re
import shutil
import subprocess
import sys
import urllib.error
import urllib.parse
import urllib.request


ORG = "https://devdiv.visualstudio.com"
PROJECT = "DevDiv"
PREVIEW_FEED = "https://aka.ms/skiasharp-eap/index.json"
BUILD_URL = (
    "https://devdiv.visualstudio.com/DevDiv/_build/results?buildId={build_id}"
)
SUCCESS_RESULTS = {"succeeded", "partiallySucceeded"}

PIPELINES = (
    {
        "key": "native",
        "name": "SkiaSharp-Native",
        "id": 26493,
        "role": "native binaries",
    },
    {
        "key": "managed",
        "name": "SkiaSharp",
        "id": 10789,
        "role": "managed build, signing, and packages",
    },
    {
        "key": "tests",
        "name": "SkiaSharp-Tests",
        "id": 15756,
        "role": "device and unit tests",
    },
)


class StatusError(RuntimeError):
    """Release status could not be determined safely."""


def run(
    args: list[str],
    *,
    cwd: Path | None = None,
    timeout: int = 30,
    check: bool = True,
) -> subprocess.CompletedProcess[str]:
    try:
        result = subprocess.run(
            args,
            cwd=cwd,
            capture_output=True,
            text=True,
            timeout=timeout,
        )
    except subprocess.TimeoutExpired as error:
        raise StatusError(
            f"command timed out after {timeout}s: {' '.join(args)}"
        ) from error
    if check and result.returncode != 0:
        detail = result.stderr.strip() or result.stdout.strip() or "no output"
        raise StatusError(
            f"command failed ({result.returncode}): {' '.join(args)}\n{detail}"
        )
    return result


class AzureDevOps:
    def __init__(self) -> None:
        self.az_path = shutil.which("az")
        if not self.az_path:
            raise StatusError("Azure CLI 'az' was not found on PATH")

    def json(self, args: list[str], *, timeout: int = 30):
        result = run([self.az_path, *args], timeout=timeout)
        if not result.stdout.strip():
            return None
        try:
            return json.loads(result.stdout)
        except json.JSONDecodeError as error:
            raise StatusError(
                f"Azure CLI returned invalid JSON for: {' '.join(args)}"
            ) from error

    def list_runs(self, pipeline_id: int, branch: str) -> list[dict]:
        return (
            self.json(
                [
                    "pipelines",
                    "runs",
                    "list",
                    "--pipeline-ids",
                    str(pipeline_id),
                    "--branch",
                    branch,
                    "--org",
                    ORG,
                    "--project",
                    PROJECT,
                    "--query",
                    (
                        "[].{id:id,status:status,result:result,"
                        "buildNumber:buildNumber,sourceBranch:sourceBranch,"
                        "sourceVersion:sourceVersion,queueTime:queueTime,"
                        "triggerInfo:triggerInfo}"
                    ),
                    "--top",
                    "100",
                    "-o",
                    "json",
                ]
            )
            or []
        )

    def timeline(self, build_id: int) -> list[dict]:
        data = self.json(
            [
                "devops",
                "invoke",
                "--area",
                "build",
                "--resource",
                "timeline",
                "--route-parameters",
                f"project={PROJECT}",
                f"buildId={build_id}",
                "--org",
                ORG,
                "--api-version",
                "7.0",
                "-o",
                "json",
            ],
            timeout=60,
        )
        return (data or {}).get("records", [])


class GitRepository:
    def __init__(self, root: Path) -> None:
        self.root = root

    @classmethod
    def discover(cls) -> GitRepository:
        result = run(["git", "rev-parse", "--show-toplevel"])
        return cls(Path(result.stdout.strip()))

    def git(self, *args: str) -> subprocess.CompletedProcess[str]:
        return run(["git", *args], cwd=self.root)

    def resolve_target(self, value: str) -> tuple[str, str]:
        self.git("fetch", "origin", "--prune")
        if re.fullmatch(r"[0-9a-fA-F]{7,40}", value):
            commit = self.git(
                "rev-parse",
                f"{value}^{{commit}}",
            ).stdout.strip()
            branches = self.git(
                "for-each-ref",
                "--contains",
                commit,
                "--format=%(refname:strip=3)",
                "refs/remotes/origin/release/",
            ).stdout.splitlines()
            branches = sorted(
                {
                    branch
                    for branch in branches
                    if branch and not branch.endswith(".x")
                }
            )
            exact = [
                branch
                for branch in branches
                if self.git(
                    "rev-parse",
                    f"refs/remotes/origin/{branch}",
                ).stdout.strip()
                == commit
            ]
            selected = exact or branches
            if len(selected) != 1:
                raise StatusError(
                    f"commit {commit} maps to ambiguous release branches: "
                    f"{selected}"
                )
            return selected[0], commit

        branch = value
        for prefix in ("refs/remotes/origin/", "refs/heads/", "origin/"):
            if branch.startswith(prefix):
                branch = branch[len(prefix):]
                break
        if not re.fullmatch(r"release/\S+", branch):
            raise StatusError(
                "release status requires release/{version} or a commit SHA"
            )
        ref = f"refs/remotes/origin/{branch}"
        exists = run(
            ["git", "show-ref", "--verify", "--quiet", ref],
            cwd=self.root,
            check=False,
        )
        if exists.returncode != 0:
            raise StatusError(f"origin/{branch} does not exist")
        return branch, self.git(
            "rev-parse",
            f"{ref}^{{commit}}",
        ).stdout.strip()

    def release_inputs(self, commit: str) -> dict:
        versions = self.git(
            "show",
            f"{commit}:scripts/VERSIONS.txt",
        ).stdout
        variables = self.git(
            "show",
            f"{commit}:scripts/azure-templates-variables.yml",
        ).stdout
        skia = re.search(
            r"^SkiaSharp\s+nuget\s+(\S+)\s*$",
            versions,
            re.MULTILINE,
        )
        harfbuzz = re.search(
            r"^HarfBuzzSharp\s+nuget\s+(\S+)\s*$",
            versions,
            re.MULTILINE,
        )
        label = re.search(
            r"^\s*PREVIEW_LABEL:\s*['\"]?([^'\"\r\n]+)",
            variables,
            re.MULTILINE,
        )
        if not skia or not harfbuzz or not label:
            raise StatusError(
                f"could not parse release versions from {commit}"
            )
        return {
            "skiaSharp": skia.group(1),
            "harfBuzzSharp": harfbuzz.group(1),
            "previewLabel": label.group(1).strip(),
        }


class NuGetFeed:
    def __init__(self, service_index: str = PREVIEW_FEED) -> None:
        self.service_index = service_index
        self._package_base: str | None = None

    def read_json(self, url: str):
        request = urllib.request.Request(
            url,
            headers={"User-Agent": "SkiaSharp-release-status"},
        )
        try:
            with urllib.request.urlopen(request, timeout=30) as response:
                return json.load(response)
        except (
            urllib.error.HTTPError,
            urllib.error.URLError,
            TimeoutError,
            json.JSONDecodeError,
        ) as error:
            raise StatusError(f"NuGet feed request failed: {url}: {error}") from error

    def package_base(self) -> str:
        if self._package_base is None:
            index = self.read_json(self.service_index)
            resource = next(
                (
                    item["@id"]
                    for item in index.get("resources", [])
                    if str(item.get("@type", "")).startswith(
                        "PackageBaseAddress/"
                    )
                ),
                None,
            )
            if not resource:
                raise StatusError(
                    "NuGet feed has no PackageBaseAddress resource"
                )
            self._package_base = resource.rstrip("/") + "/"
        return self._package_base

    def contains(self, package_id: str, version: str) -> bool:
        url = urllib.parse.urljoin(
            self.package_base(),
            f"{package_id.lower()}/index.json",
        )
        data = self.read_json(url)
        return version.lower() in {
            str(item).lower() for item in data.get("versions", [])
        }


def sort_runs(runs: list[dict]) -> list[dict]:
    return sorted(
        runs,
        key=lambda item: (item.get("queueTime") or "", int(item["id"])),
        reverse=True,
    )


def run_state(run: dict | None) -> str:
    if run is None:
        return "not-triggered"
    if run.get("status") != "completed":
        return "running"
    result = run.get("result") or "unknown"
    if result == "succeeded":
        return "succeeded"
    if result == "partiallySucceeded":
        return "warning"
    if result in ("failed", "canceled"):
        return result
    return "unknown"


def is_successful(run: dict) -> bool:
    return (
        run.get("status") == "completed"
        and run.get("result") in SUCCESS_RESULTS
    )


def upstream_id(run: dict) -> int | None:
    value = (run.get("triggerInfo") or {}).get("pipelineId")
    return int(value) if value is not None else None


def job_summary(records: list[dict]) -> dict:
    summary = {
        "completed": [],
        "failed": [],
        "running": [],
        "pending": [],
    }
    for job in records:
        if job.get("type") != "Job":
            continue
        name = job.get("name") or "Unknown"
        state = job.get("state") or ""
        result = job.get("result") or ""
        if state == "completed":
            group = (
                "failed"
                if result in ("failed", "canceled")
                else "completed"
            )
        elif state == "inProgress":
            group = "running"
        else:
            group = "pending"
        summary[group].append(name)
    return summary


def run_output(
    ado: AzureDevOps,
    pipeline: dict,
    run: dict | None,
    warnings: list[str],
) -> dict:
    if run is None:
        return {
            "name": pipeline["name"],
            "pipelineId": pipeline["id"],
            "role": pipeline["role"],
            "state": "not-triggered",
            "runId": None,
            "buildNumber": None,
            "sourceBranch": None,
            "sourceVersion": None,
            "result": None,
            "triggeredByRunId": None,
            "url": None,
            "jobs": None,
        }
    state = run_state(run)
    jobs = None
    if state in ("running", "warning", "failed", "canceled"):
        try:
            jobs = job_summary(ado.timeline(int(run["id"])))
        except StatusError as error:
            warnings.append(
                f"Could not read {pipeline['name']} job details: {error}"
            )
    return {
        "name": pipeline["name"],
        "pipelineId": pipeline["id"],
        "role": pipeline["role"],
        "state": state,
        "runId": int(run["id"]),
        "buildNumber": run.get("buildNumber"),
        "sourceBranch": run.get("sourceBranch"),
        "sourceVersion": run.get("sourceVersion"),
        "result": run.get("result"),
        "triggeredByRunId": upstream_id(run),
        "url": BUILD_URL.format(build_id=run["id"]),
        "jobs": jobs,
    }


def package_versions(build_number: str, inputs: dict) -> dict:
    test_skia = build_number.split("+", 1)[0]
    expected = f"{inputs['skiaSharp']}-{inputs['previewLabel']}."
    if not test_skia.startswith(expected):
        raise StatusError(
            f"buildNumber {build_number} does not match {inputs}"
        )
    suffix = test_skia[len(inputs["skiaSharp"]) + 1 :]
    test_harfbuzz = f"{inputs['harfBuzzSharp']}-{suffix}"
    stable = inputs["previewLabel"] == "stable"
    return {
        "test": {
            "SkiaSharp": test_skia,
            "HarfBuzzSharp": test_harfbuzz,
        },
        "public": {
            "SkiaSharp": inputs["skiaSharp"] if stable else test_skia,
            "HarfBuzzSharp": (
                inputs["harfBuzzSharp"] if stable else test_harfbuzz
            ),
        },
    }


def check_feed(
    feed: NuGetFeed,
    versions: dict,
    warnings: list[str],
) -> dict:
    packages = {}
    try:
        for package_id, version in versions["test"].items():
            packages[package_id] = {
                "version": version,
                "available": feed.contains(package_id, version),
            }
        return {
            "source": PREVIEW_FEED,
            "state": (
                "ready"
                if all(item["available"] for item in packages.values())
                else "missing"
            ),
            "packages": packages,
        }
    except StatusError as error:
        warnings.append(str(error))
        return {
            "source": PREVIEW_FEED,
            "state": "error",
            "packages": packages,
        }


def latest_successful(runs: list[dict]) -> dict | None:
    return next((run for run in sort_runs(runs) if is_successful(run)), None)


def build_report(
    target: str,
    *,
    ado: AzureDevOps,
    repo: GitRepository,
    feed: NuGetFeed,
) -> dict:
    branch, commit = repo.resolve_target(target)
    warnings: list[str] = []
    runs = {
        pipeline["key"]: [
            run
            for run in ado.list_runs(pipeline["id"], branch)
            if run.get("sourceVersion") == commit
        ]
        for pipeline in PIPELINES
    }

    native = sort_runs(runs["native"])
    latest_native = native[0] if native else None
    triggered_managed = [
        run
        for run in sort_runs(runs["managed"])
        if latest_native
        and upstream_id(run) == int(latest_native["id"])
    ]
    managed_children = [
        run
        for run in triggered_managed
        if run.get("buildNumber") == latest_native.get("buildNumber")
    ]
    if len(managed_children) != len(triggered_managed):
        warnings.append(
            "Ignored SkiaSharp children whose buildNumber did not match "
            "the latest native run"
        )
    selected_managed = latest_successful(managed_children)
    displayed_managed = (
        selected_managed
        or (managed_children[0] if managed_children else None)
    )
    triggered_tests = [
        run
        for run in sort_runs(runs["tests"])
        if selected_managed
        and upstream_id(run) == int(selected_managed["id"])
    ]
    tests_children = [
        run
        for run in triggered_tests
        if run.get("buildNumber") == selected_managed.get("buildNumber")
    ]
    if len(tests_children) != len(triggered_tests):
        warnings.append(
            "Ignored SkiaSharp-Tests children whose buildNumber did not "
            "match the selected SkiaSharp run"
        )
    selected_tests = tests_children[0] if tests_children else None

    newer_managed = []
    if selected_managed:
        selected_index = managed_children.index(selected_managed)
        newer_managed = managed_children[:selected_index]
        if any(run_state(run) == "running" for run in newer_managed):
            warnings.append(
                "A newer SkiaSharp child is still running; wait before "
                "selecting the release build"
            )
        if any(
            run_state(run) in ("failed", "canceled")
            for run in newer_managed
        ):
            warnings.append(
                "A newer SkiaSharp child failed; the latest successful "
                f"run remains {selected_managed['id']}"
            )

    versions = None
    feed_result = None
    if selected_managed:
        try:
            versions = package_versions(
                selected_managed["buildNumber"],
                repo.release_inputs(commit),
            )
            feed_result = check_feed(feed, versions, warnings)
        except StatusError as error:
            warnings.append(str(error))

    native_state = run_state(latest_native)
    managed_state = run_state(displayed_managed)
    tests_state = run_state(selected_tests)
    if latest_native is None:
        state, next_action = "waiting", "wait-for-native"
    elif native_state in ("failed", "canceled", "unknown"):
        state, next_action = "blocked", "retry-native"
    elif native_state == "running":
        state, next_action = "running", "wait-for-native"
    elif selected_managed is None:
        if displayed_managed is None:
            state, next_action = "waiting", "wait-for-managed-trigger"
        elif managed_state == "running":
            state, next_action = "running", "wait-for-managed"
        else:
            state, next_action = "blocked", "retry-managed"
    elif any(run_state(run) == "running" for run in newer_managed):
        state, next_action = "running", "wait-for-managed"
    elif selected_tests is None:
        state, next_action = "waiting", "wait-for-tests-trigger"
    elif tests_state == "running":
        state, next_action = "running", "wait-for-tests"
    elif tests_state in ("failed", "canceled", "unknown"):
        state, next_action = "blocked", "retry-tests"
    elif feed_result is None or feed_result["state"] == "error":
        state, next_action = "waiting", "retry-package-check"
    elif feed_result["state"] != "ready":
        state, next_action = "waiting", "wait-for-packages"
    else:
        state, next_action = "ready", "start-release-testing"

    if latest_native and native_state == "warning":
        warnings.append(
            "The latest SkiaSharp-Native run partially succeeded"
        )
    if selected_managed and run_state(selected_managed) == "warning":
        warnings.append(
            "The selected SkiaSharp run partially succeeded"
        )
    if selected_tests and tests_state == "warning":
        warnings.append(
            "The selected SkiaSharp-Tests run partially succeeded"
        )

    native_output = run_output(
        ado,
        PIPELINES[0],
        latest_native,
        warnings,
    )
    managed_output = run_output(
        ado,
        PIPELINES[1],
        displayed_managed,
        warnings,
    )
    tests_output = run_output(
        ado,
        PIPELINES[2],
        selected_tests,
        warnings,
    )

    return {
        "schemaVersion": 3,
        "input": target,
        "branch": branch,
        "commit": commit,
        "state": state,
        "nextAction": next_action,
        "nativeRun": native_output,
        "managedRun": managed_output,
        "testsRun": tests_output,
        "packageVersions": versions,
        "packageFeed": feed_result,
        "warnings": warnings,
    }


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("release_branch_or_commit")
    args = parser.parse_args()
    try:
        print(
            json.dumps(
                build_report(
                    args.release_branch_or_commit,
                    ado=AzureDevOps(),
                    repo=GitRepository.discover(),
                    feed=NuGetFeed(),
                ),
                indent=2,
            )
        )
    except StatusError as error:
        print(f"ERROR: {error}", file=sys.stderr)
        return 1
    return 0


if __name__ == "__main__":
    sys.exit(main())
