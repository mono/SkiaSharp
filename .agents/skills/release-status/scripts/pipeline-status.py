#!/usr/bin/env python3
"""Report the exact SkiaSharp Build, Tests, and BAR release handoff."""

from __future__ import annotations

import argparse
import json
from pathlib import Path
import re
import shutil
import subprocess
import sys
import tempfile


ORG = "https://dev.azure.com/dnceng"
PROJECT = "internal"
BUILD_URL = (
    "https://dev.azure.com/dnceng/internal/_build/results?buildId={build_id}"
)
BUILD_PIPELINE_SOURCE = r"\dotnet\skiasharp\skiasharp-package"
SIGNED_FEED_MARKER = "/_packaging/skiasharp/"
SUCCESS_RESULTS = {"succeeded", "partiallySucceeded"}
EXACT_RELEASE_BRANCH_RE = re.compile(
    r"^release/\d+\.\d+\.\d+(?:\.\d+)?"
    r"(?:-(?:preview|rc)\.[1-9]\d*)?$"
)
MAINTENANCE_BRANCH_RE = re.compile(r"^release/\d+\.\d+\.x$")

MIGRATION_REQUIREMENTS = (
    {
        "id": "combined-build",
        "path": "scripts/azure-pipelines-package.yml",
        "pattern": r"buildPipelineType:\s*['\"]?build['\"]?",
        "detail": (
            "backport the combined dnceng Build role used by pipeline 1642"
        ),
    },
    {
        "id": "connected-tests",
        "path": "scripts/azure-pipelines-tests.yml",
        "pattern": (
            r"source:\s*['\"]?"
            r"\\dotnet\\skiasharp\\skiasharp-package['\"]?"
        ),
        "detail": (
            "backport the folder-qualified Build resource consumed by "
            "Tests pipeline 1630"
        ),
    },
    {
        "id": "exact-artifact-selection",
        "path": "scripts/azure-templates-steps-download-artifacts.yml",
        "pattern": (
            r"Mutable latestFromBranch artifact selection is not supported"
        ),
        "forbiddenPattern": (
            r"\$versionType\s*=\s*['\"]latestFromBranch['\"]"
        ),
        "detail": (
            "backport the fail-closed exact Build artifact selector and "
            "remove mutable latestFromBranch assignment"
        ),
    },
    {
        "id": "exact-release-versioning",
        "path": "scripts/infra/native/shared/set-build-variables.ps1",
        "pattern": (
            r"Set-BuildVariable\s+DOTNET_FINAL_VERSION_KIND\s+"
            r"\$finalVersionKind"
        ),
        "detail": (
            "backport exact stable version selection for internal release/* "
            "builds"
        ),
    },
    {
        "id": "unique-transport-bar-assets",
        "path": "scripts/azure-templates-stages-signing.yml",
        "pattern": (
            r"\.Name\.Contains\("
            r"['\"]\.0\.0\.0-branch\.['\"].*"
            r"Copy-Item\s+\$transportPackages\.FullName\s+\$nonShipping"
        ),
        "detail": (
            "backport branch-only NonShipping transport staging so BAR "
            "registers one version per transport package ID"
        ),
    },
    {
        "id": "branch-first-transport-download",
        "path": "scripts/infra/shared/download.cake",
        "pattern": (
            r"else if \(!string\.IsNullOrEmpty\(GIT_BRANCH_NAME\)\).*"
            r"else if \(!string\.IsNullOrEmpty\(GIT_SHA\)\)"
        ),
        "detail": (
            "backport branch-before-commit transport package lookup so "
            "release builds consume the identity promoted to BAR"
        ),
    },
    *(
        {
            "id": f"{package}-transport-metadata",
            "path": f"scripts/infra/package/nuget/_{package}.nuspec",
            "pattern": (
                r"<copyright>.+?</copyright>.*"
                r"<license\s+type=\"expression\">MIT</license>.*"
                r"<projectUrl>.+?</projectUrl>"
            ),
            "detail": (
                f"backport Microsoft package metadata required by Arcade "
                f"NuGet validation for _{package}"
            ),
        }
        for package in ("NativeAssets", "NuGets", "Dependencies")
    ),
)

PIPELINES = (
    {
        "key": "build",
        "name": "skiasharp-package",
        "id": 1642,
        "role": "combined native/managed build, signing, and BAR registration",
    },
    {
        "key": "tests",
        "name": "skiasharp-tests",
        "id": 1630,
        "role": "connected device and unit tests",
    },
)


class StatusError(RuntimeError):
    """Release status could not be determined safely."""


def normalize_release_branch(value: str) -> str:
    branch = value
    for prefix in ("refs/remotes/origin/", "refs/heads/", "origin/"):
        if branch.startswith(prefix):
            branch = branch[len(prefix):]
            break
    if MAINTENANCE_BRANCH_RE.fullmatch(branch):
        raise StatusError(
            f"{branch} is an integration/maintenance branch; use "
            "release-branch to cut an exact release/X.Y.Z[-preview.N|-rc.N] "
            "branch before running release-status"
        )
    if not EXACT_RELEASE_BRANCH_RE.fullmatch(branch):
        raise StatusError(
            "release status requires an exact "
            "release/X.Y.Z[-preview.N|-rc.N] or "
            "release/X.Y.Z.F[-preview.N|-rc.N] branch, or a commit SHA"
        )
    return branch


def parse_default_channel_ids(value: str) -> list[int]:
    return [int(item) for item in re.findall(r"\d+", value)]


def migration_requirement_satisfied(
    content: str | None,
    requirement: dict,
) -> bool:
    if content is None or not re.search(
        requirement["pattern"],
        content,
        re.MULTILINE | re.DOTALL,
    ):
        return False
    forbidden = requirement.get("forbiddenPattern")
    return forbidden is None or re.search(
        forbidden,
        content,
        re.MULTILINE | re.DOTALL,
    ) is None


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
    except FileNotFoundError as error:
        raise StatusError(f"{args[0]} was not found on PATH") from error
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
                        "sourceVersion:sourceVersion,queueTime:queueTime}"
                    ),
                    "--top",
                    "100",
                    "-o",
                    "json",
                ]
            )
            or []
        )

    def run_detail(self, pipeline_id: int, run_id: int) -> dict:
        return (
            self.json(
                [
                    "devops",
                    "invoke",
                    "--area",
                    "pipelines",
                    "--resource",
                    "runs",
                    "--route-parameters",
                    f"project={PROJECT}",
                    f"pipelineId={pipeline_id}",
                    f"runId={run_id}",
                    "--org",
                    ORG,
                    "--api-version",
                    "7.1",
                    "-o",
                    "json",
                ],
                timeout=60,
            )
            or {}
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

    def release_config(self, build_id: int) -> dict:
        with tempfile.TemporaryDirectory() as temporary:
            root = Path(temporary)
            run(
                [
                    self.az_path,
                    "pipelines",
                    "runs",
                    "artifact",
                    "download",
                    "--run-id",
                    str(build_id),
                    "--artifact-name",
                    "ReleaseConfigs",
                    "--path",
                    str(root),
                    "--org",
                    ORG,
                    "--project",
                    PROJECT,
                ],
                timeout=120,
            )
            files = list(root.rglob("ReleaseConfigs.txt"))
            if len(files) != 1:
                raise StatusError(
                    f"Build run {build_id} has {len(files)} ReleaseConfigs.txt "
                    "files; expected exactly one"
                )
            lines = files[0].read_text(encoding="utf-8").splitlines()
        if len(lines) < 3 or not lines[0].strip().isdigit():
            raise StatusError(
                f"Build run {build_id} has an invalid ReleaseConfigs artifact"
            )
        return {
            "barBuildId": int(lines[0].strip()),
            "defaultChannelIds": parse_default_channel_ids(lines[1]),
            "stable": lines[2].strip().lower() == "true",
        }


class Darc:
    def __init__(self) -> None:
        self.darc_path = shutil.which("darc")
        if not self.darc_path:
            raise StatusError("Darc CLI 'darc' was not found on PATH")

    def get_build(self, bar_build_id: int) -> dict:
        result = run(
            [
                self.darc_path,
                "get-build",
                "--id",
                str(bar_build_id),
                "--extended",
                "--output-format",
                "json",
            ],
            timeout=120,
        )
        try:
            data = json.loads(result.stdout)
        except json.JSONDecodeError as error:
            raise StatusError(
                f"Darc returned invalid JSON for BAR build {bar_build_id}"
            ) from error
        records = data if isinstance(data, list) else [data]
        records = [record for record in records if record]
        if len(records) != 1:
            raise StatusError(
                f"BAR build {bar_build_id} resolved to {len(records)} records"
            )
        return records[0]


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

        branch = normalize_release_branch(value)
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

    def release_prerequisites(self, commit: str) -> dict:
        cache: dict[str, str | None] = {}
        missing = []
        for requirement in MIGRATION_REQUIREMENTS:
            path = requirement["path"]
            if path not in cache:
                result = run(
                    ["git", "show", f"{commit}:{path}"],
                    cwd=self.root,
                    check=False,
                )
                cache[path] = (
                    result.stdout
                    if result.returncode == 0
                    else None
                )
            content = cache[path]
            if not migration_requirement_satisfied(
                content,
                requirement,
            ):
                missing.append(
                    {
                        "id": requirement["id"],
                        "path": path,
                        "detail": requirement["detail"],
                    }
                )
        return {
            "state": "ready" if not missing else "missing",
            "missing": missing,
        }

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
    selected_run: dict | None,
    warnings: list[str],
) -> dict:
    if selected_run is None:
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
            "url": None,
            "jobs": None,
        }
    state = run_state(selected_run)
    jobs = None
    if state in ("running", "warning", "failed", "canceled"):
        try:
            jobs = job_summary(ado.timeline(int(selected_run["id"])))
        except StatusError as error:
            warnings.append(
                f"Could not read {pipeline['name']} job details: {error}"
            )
    return {
        "name": pipeline["name"],
        "pipelineId": pipeline["id"],
        "role": pipeline["role"],
        "state": state,
        "runId": int(selected_run["id"]),
        "buildNumber": selected_run.get("buildNumber"),
        "sourceBranch": selected_run.get("sourceBranch"),
        "sourceVersion": selected_run.get("sourceVersion"),
        "result": selected_run.get("result"),
        "url": BUILD_URL.format(build_id=selected_run["id"]),
        "jobs": jobs,
    }


def pipeline_resource(detail: dict) -> dict:
    return (
        (detail.get("resources") or {})
        .get("pipelines", {})
        .get("SkiaSharp", {})
    )


def is_connected_test(detail: dict, build_run: dict) -> bool:
    resource = pipeline_resource(detail)
    pipeline = resource.get("pipeline") or {}
    return (
        int(pipeline.get("id") or 0) == int(build_run["id"])
        and pipeline.get("name") == PIPELINES[0]["name"]
        and pipeline.get("folder") == r"\dotnet\skiasharp"
        and resource.get("version") == build_run.get("buildNumber")
    )


def select_connected_test(
    ado: AzureDevOps,
    runs: list[dict],
    build_run: dict,
) -> dict | None:
    connected = []
    for candidate in sort_runs(runs):
        detail = ado.run_detail(PIPELINES[1]["id"], int(candidate["id"]))
        if is_connected_test(detail, build_run):
            connected.append(candidate)
    return connected[0] if connected else None


def package_versions_from_bar(record: dict, inputs: dict) -> tuple[dict, dict]:
    assets = {}
    for package_id in ("SkiaSharp", "HarfBuzzSharp"):
        matches = [
            asset
            for asset in record.get("assets") or []
            if asset.get("name") == package_id
            and not asset.get("nonShipping", False)
        ]
        if len(matches) != 1:
            raise StatusError(
                f"BAR build {record.get('id')} has {len(matches)} shipping "
                f"{package_id} assets; expected exactly one"
            )
        asset = matches[0]
        assets[package_id] = {
            "version": str(asset.get("version") or ""),
            "locations": asset.get("locations") or [],
        }

    expected = {
        "SkiaSharp": inputs["skiaSharp"],
        "HarfBuzzSharp": inputs["harfBuzzSharp"],
    }
    stable = inputs["previewLabel"] == "stable"
    suffix = None
    for package_id, base_version in expected.items():
        version = assets[package_id]["version"]
        if stable:
            if version != base_version:
                raise StatusError(
                    f"stable BAR asset {package_id} must be {base_version}, "
                    f"got {version}"
                )
            continue
        prefix = f"{base_version}-{inputs['previewLabel']}."
        if not version.startswith(prefix):
            raise StatusError(
                f"BAR asset {package_id} {version} does not start with {prefix}"
            )
        current_suffix = version[len(base_version) + 1 :]
        if suffix is None:
            suffix = current_suffix
        elif current_suffix != suffix:
            raise StatusError(
                "SkiaSharp and HarfBuzzSharp BAR versions do not share "
                "the same release suffix"
            )

    versions = {
        "test": {
            package_id: asset["version"]
            for package_id, asset in assets.items()
        },
        "public": {
            package_id: asset["version"]
            for package_id, asset in assets.items()
        },
    }
    return versions, assets


def nonshipping_asset_versions(record: dict) -> dict[str, list[str]]:
    grouped: dict[str, list[str]] = {}
    for asset in record.get("assets") or []:
        if not asset.get("nonShipping", False):
            continue
        name = str(asset.get("name") or "")
        version = str(asset.get("version") or "")
        grouped.setdefault(name.casefold(), []).append(version)
    return grouped


def validate_unique_nonshipping_assets(record: dict) -> None:
    grouped = nonshipping_asset_versions(record)
    duplicates = {
        name: versions
        for name, versions in grouped.items()
        if len(versions) > 1
    }
    if duplicates:
        detail = "; ".join(
            f"{name}={versions}"
            for name, versions in sorted(duplicates.items())
        )
        raise StatusError(
            "BAR contains ambiguous duplicate NonShipping transport asset "
            f"IDs: {detail}"
        )


def bar_output(
    record: dict,
    config: dict,
    *,
    branch: str,
    commit: str,
    build_run: dict,
    inputs: dict,
) -> tuple[dict, dict]:
    expected_branch = f"refs/heads/{branch}"
    checks = {
        "id": (int(record.get("id") or 0), config["barBuildId"]),
        "commit": (record.get("commit"), commit),
        "azureDevOpsProject": (record.get("azureDevOpsProject"), PROJECT),
        "azureDevOpsAccount": (
            record.get("azureDevOpsAccount"),
            "dnceng",
        ),
        "azureDevOpsBuildDefinitionId": (
            int(record.get("azureDevOpsBuildDefinitionId") or 0),
            PIPELINES[0]["id"],
        ),
        "azureDevOpsBuildId": (
            int(record.get("azureDevOpsBuildId") or 0),
            int(build_run["id"]),
        ),
        "azureDevOpsBranch": (
            record.get("azureDevOpsBranch"),
            expected_branch,
        ),
        "stable": (
            bool(record.get("stable")),
            inputs["previewLabel"] == "stable",
        ),
        "releaseConfigStable": (
            bool(config.get("stable")),
            inputs["previewLabel"] == "stable",
        ),
    }
    mismatches = [
        f"{name}={actual!r}, expected {expected!r}"
        for name, (actual, expected) in checks.items()
        if actual != expected
    ]
    if mismatches:
        raise StatusError(
            f"BAR build {config['barBuildId']} does not match the selected "
            f"Build run: {'; '.join(mismatches)}"
        )

    validate_unique_nonshipping_assets(record)
    versions, assets = package_versions_from_bar(record, inputs)
    channels = sorted(
        {
            str(channel.get("name"))
            for channel in record.get("channels") or []
            if channel.get("name")
        }
    )
    default_channel_ids = config.get("defaultChannelIds") or []
    routed_assets = {
        package_id: any(
            SIGNED_FEED_MARKER in str(location).lower()
            for location in asset["locations"]
        )
        for package_id, asset in assets.items()
    }
    if not default_channel_ids:
        state = "missing-default-channels"
    elif not all(routed_assets.values()):
        state = "missing-feed-routing"
    else:
        state = "ready"
    return (
        {
            "id": config["barBuildId"],
            "state": state,
            "commit": record.get("commit"),
            "buildRunId": int(record["azureDevOpsBuildId"]),
            "buildDefinitionId": int(
                record["azureDevOpsBuildDefinitionId"]
            ),
            "buildNumber": record.get("azureDevOpsBuildNumber"),
            "branch": record.get("azureDevOpsBranch"),
            "stable": bool(record.get("stable")),
            "channels": channels,
            "defaultChannelIds": default_channel_ids,
            "assets": assets,
            "routedAssets": routed_assets,
            "nonShippingAssets": nonshipping_asset_versions(record),
        },
        versions,
    )


def build_report(
    target: str,
    *,
    ado: AzureDevOps,
    repo: GitRepository,
    darc: Darc,
) -> dict:
    branch, commit = repo.resolve_target(target)
    warnings: list[str] = []
    migration = repo.release_prerequisites(commit)
    if migration["state"] != "ready":
        warnings.append(
            "The target commit does not contain the minimum Arcade release "
            "backport required for Build 1642 -> Tests 1630 -> BAR"
        )
        return {
            "schemaVersion": 5,
            "input": target,
            "branch": branch,
            "commit": commit,
            "state": "blocked",
            "nextAction": "backport-arcade-release",
            "migration": migration,
            "buildRun": run_output(
                ado,
                PIPELINES[0],
                None,
                warnings,
            ),
            "testsRun": run_output(
                ado,
                PIPELINES[1],
                None,
                warnings,
            ),
            "barBuild": None,
            "packageVersions": None,
            "warnings": warnings,
        }
    build_runs = [
        item
        for item in ado.list_runs(PIPELINES[0]["id"], branch)
        if item.get("sourceVersion") == commit
    ]
    build_run = sort_runs(build_runs)[0] if build_runs else None
    build_state = run_state(build_run)

    tests_run = None
    bar = None
    versions = None
    bar_error = None
    if build_run and is_successful(build_run):
        try:
            config = ado.release_config(int(build_run["id"]))
            record = darc.get_build(config["barBuildId"])
            bar, versions = bar_output(
                record,
                config,
                branch=branch,
                commit=commit,
                build_run=build_run,
                inputs=repo.release_inputs(commit),
            )
        except StatusError as error:
            bar_error = str(error)
            warnings.append(bar_error)
        tests_candidates = [
            item
            for item in ado.list_runs(PIPELINES[1]["id"], branch)
            if item.get("sourceVersion") == commit
        ]
        tests_run = select_connected_test(ado, tests_candidates, build_run)

    tests_state = run_state(tests_run)
    if build_run is None:
        state, next_action = "waiting", "wait-for-build"
    elif build_state == "running":
        state, next_action = "running", "wait-for-build"
    elif build_state in ("failed", "canceled", "unknown"):
        state, next_action = "blocked", "retry-build"
        try:
            failed_config = ado.release_config(int(build_run["id"]))
            if not failed_config.get("defaultChannelIds"):
                next_action = "configure-default-channels"
                warnings.append(
                    "The failed Build registered a BAR but resolved no "
                    "default channels for this release branch"
                )
        except StatusError:
            pass
    elif bar_error:
        state, next_action = "blocked", "retry-bar-check"
    elif tests_run is None:
        state, next_action = "waiting", "wait-for-tests-trigger"
    elif tests_state == "running":
        state, next_action = "running", "wait-for-tests"
    elif tests_state in ("failed", "canceled", "unknown"):
        state, next_action = "blocked", "retry-tests"
    elif bar["state"] == "missing-default-channels":
        state, next_action = "blocked", "configure-default-channels"
    elif bar["state"] == "missing-feed-routing":
        state, next_action = "blocked", "configure-feed-routing"
    else:
        state, next_action = "ready", "start-release-testing"

    if build_run and build_state == "warning":
        warnings.append("The selected skiasharp-package run partially succeeded")
    if tests_run and tests_state == "warning":
        warnings.append("The connected skiasharp-tests run partially succeeded")

    return {
        "schemaVersion": 5,
        "input": target,
        "branch": branch,
        "commit": commit,
        "state": state,
        "nextAction": next_action,
        "migration": migration,
        "buildRun": run_output(
            ado,
            PIPELINES[0],
            build_run,
            warnings,
        ),
        "testsRun": run_output(
            ado,
            PIPELINES[1],
            tests_run,
            warnings,
        ),
        "barBuild": bar,
        "packageVersions": versions,
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
                    darc=Darc(),
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
