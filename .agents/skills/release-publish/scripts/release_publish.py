#!/usr/bin/env python3
"""Shared deterministic helpers for SkiaSharp release publication."""

from __future__ import annotations

from dataclasses import dataclass
import json
from pathlib import Path
import re
import shlex
import shutil
import subprocess
import sys
import urllib.error
import urllib.parse
import urllib.request


STATUS_SCRIPT = (
    Path(__file__).resolve().parents[2]
    / "release-status"
    / "scripts"
    / "pipeline-status.py"
)
RELEASE_BRANCH_RE = re.compile(
    r"^release/(?P<numeric>\d+\.\d+\.\d+(?:\.\d+)?)"
    r"(?:-(?P<channel>preview|rc)\.(?P<iteration>\d+))?$"
)

# The two dnceng/internal pipeline definitions that form the immutable chain
# a BAR build must have originated from.
BUILD_DEFINITION_ID = 1642
TESTS_DEFINITION_ID = 1630

# The exact Shipping and NonShipping packages every BAR build must register.
BAR_ASSET_PACKAGES = ("SkiaSharp", "HarfBuzzSharp")
TRANSPORT_ASSET_PACKAGES = ("_NativeAssets", "_NuGets")
PRODUCT_CHANNEL_ID = 1648
PRODUCT_FEED_MARKER = "/_packaging/dotnet-libraries/"
TRANSPORT_FEED_MARKER = "/_packaging/dotnet-libraries-transport/"

class PublishError(RuntimeError):
    """The release could not be audited or advanced safely."""


def display(args: list[str]) -> str:
    return (
        subprocess.list2cmdline(args)
        if sys.platform == "win32"
        else shlex.join(args)
    )


def shell_command(args: list[str]) -> str:
    if sys.platform != "win32":
        return shlex.join(args)

    def quote(argument: str) -> str:
        if re.fullmatch(r"[A-Za-z0-9_./:\\=+-]+", argument):
            return argument
        return "'" + argument.replace("'", "''") + "'"

    formatted = " ".join(quote(argument) for argument in args)
    return f"& {formatted}" if formatted.startswith("'") else formatted


def resolve_command(args: list[str]) -> list[str]:
    executable = shutil.which(args[0])
    if not executable:
        return args
    resolved = [executable, *args[1:]]
    if sys.platform == "win32" and Path(executable).suffix.lower() in {
        ".bat",
        ".cmd",
    }:
        return [
            shutil.which("cmd.exe") or "cmd.exe",
            "/d",
            "/s",
            "/c",
            subprocess.list2cmdline(resolved),
        ]
    return resolved


def run(
    args: list[str],
    *,
    cwd: Path | None = None,
    timeout: int = 120,
    check: bool = True,
) -> subprocess.CompletedProcess[str]:
    args = resolve_command(args)
    try:
        result = subprocess.run(
            args,
            cwd=cwd,
            capture_output=True,
            text=True,
            timeout=timeout,
        )
    except FileNotFoundError as error:
        raise PublishError(f"{args[0]} was not found on PATH") from error
    except subprocess.TimeoutExpired as error:
        raise PublishError(
            f"command timed out after {timeout}s: {display(args)}"
        ) from error
    if check and result.returncode != 0:
        detail = result.stderr.strip() or result.stdout.strip() or "no output"
        raise PublishError(
            f"command failed ({result.returncode}): {display(args)}\n{detail}"
        )
    return result


def parse_json_output(text: str):
    decoder = json.JSONDecoder()
    for index, character in enumerate(text):
        if character not in "[{":
            continue
        try:
            value, _ = decoder.raw_decode(text[index:])
            return value
        except json.JSONDecodeError:
            pass
    raise PublishError("command returned no valid JSON")


def run_json(
    args: list[str],
    *,
    cwd: Path | None = None,
    timeout: int = 120,
):
    return parse_json_output(run(args, cwd=cwd, timeout=timeout).stdout)


@dataclass(frozen=True)
class ReleaseVersion:
    branch: str
    raw: str
    numeric: str
    parts: tuple[int, ...]
    channel: str | None
    iteration: int | None

    @classmethod
    def parse(cls, value: str) -> ReleaseVersion:
        match = RELEASE_BRANCH_RE.fullmatch(value)
        if not match:
            raise PublishError(
                "release must be an exact release/X.Y.Z[-preview.N|-rc.N] "
                "or release/X.Y.Z.F[...] branch"
            )
        iteration = (
            int(match.group("iteration"))
            if match.group("iteration")
            else None
        )
        if iteration == 0:
            raise PublishError("preview/rc iteration zero is not publishable")
        numeric = match.group("numeric")
        raw = numeric
        if match.group("channel"):
            raw += f"-{match.group('channel')}.{iteration}"
        return cls(
            branch=value,
            raw=raw,
            numeric=numeric,
            parts=tuple(int(part) for part in numeric.split(".")),
            channel=match.group("channel"),
            iteration=iteration,
        )

    @property
    def stable(self) -> bool:
        return self.channel is None

    @property
    def release_type(self) -> str:
        prefix = "hotfix " if len(self.parts) == 4 else ""
        return prefix + (self.channel or "stable")

    @property
    def title(self) -> str:
        if self.channel == "preview":
            return f"Version {self.numeric} (Preview {self.iteration})"
        if self.channel == "rc":
            return f"Version {self.numeric} (RC {self.iteration})"
        return f"Version {self.numeric}"

    def validate_public_version(self, version: str) -> None:
        if self.stable:
            if version != self.numeric:
                raise PublishError(
                    f"stable public version must be {self.numeric}, got {version}"
                )
            return
        match = re.fullmatch(
            rf"{re.escape(self.raw)}\."
            rf"(?:(?:\d{{5}}|\d{{8}})\.)?\d+",
            version,
        )
        if not match or version.endswith(".0"):
            raise PublishError(
                f"public version {version} does not match {self.raw}.BUILD"
            )


class GitRepository:
    def __init__(self, root: Path) -> None:
        self.root = root

    @classmethod
    def discover(cls) -> GitRepository:
        result = run(["git", "rev-parse", "--show-toplevel"])
        return cls(Path(result.stdout.strip()))

    def git(
        self,
        *args: str,
        timeout: int = 120,
        check: bool = True,
    ) -> subprocess.CompletedProcess[str]:
        return run(
            ["git", *args],
            cwd=self.root,
            timeout=timeout,
            check=check,
        )

    def resolve_release_sha(
        self,
        branch: str,
        expected_sha: str,
    ) -> str:
        ref = f"refs/remotes/origin/{branch}"
        self.git(
            "fetch",
            "origin",
            f"refs/heads/{branch}:{ref}",
        )
        sha = self.git("rev-parse", f"{ref}^{{commit}}").stdout.strip()
        if sha != expected_sha:
            raise PublishError(
                f"{branch} advanced after testing: expected "
                f"{expected_sha}, found {sha}"
            )
        return sha

    def remote_tags(self) -> dict[str, str]:
        lines = self.git(
            "ls-remote",
            "--tags",
            "origin",
            "refs/tags/v*",
        ).stdout.splitlines()
        direct: dict[str, str] = {}
        peeled: dict[str, str] = {}
        for line in lines:
            if not line.strip():
                continue
            sha, ref = line.split(maxsplit=1)
            name = ref.removeprefix("refs/tags/")
            if name.endswith("^{}"):
                peeled[name[:-3]] = sha
            else:
                direct[name] = sha
        return {name: peeled.get(name, sha) for name, sha in direct.items()}

    def push_tag(self, tag: str, sha: str) -> None:
        self.git("push", "origin", f"{sha}:refs/tags/{tag}")


def status_report(root: Path, release_branch: str) -> dict:
    return run_json(
        [sys.executable, str(STATUS_SCRIPT), release_branch],
        cwd=root,
        timeout=240,
    )


def validate_status_handoff(
    status: dict,
    release: ReleaseVersion,
    *,
    expected_sha: str,
    expected_build_run: int,
    expected_tests_run: int,
    expected_bar_build: int,
) -> dict:
    if status.get("branch") != release.branch:
        raise PublishError(
            f"release-status resolved {status.get('branch')}, "
            f"expected {release.branch}"
        )
    if status.get("commit") != expected_sha:
        raise PublishError(
            f"release-status resolved {status.get('commit')}, "
            f"expected tested commit {expected_sha}"
        )
    if status.get("nextAction") != "start-release-testing":
        raise PublishError(
            f"release-status is not ready: {status.get('nextAction')}"
        )
    prerequisites = status.get("prerequisites") or {}
    if prerequisites.get("state") != "ready":
        raise PublishError(
            "release-status did not verify the target branch's release "
            "tooling prerequisites"
        )
    build = status.get("buildRun") or {}
    tests = status.get("testsRun") or {}
    bar = status.get("barBuild") or {}
    if build.get("runId") != expected_build_run:
        raise PublishError(
            f"Build run changed: expected {expected_build_run}, "
            f"found {build.get('runId')}"
        )
    if build.get("pipelineId") != BUILD_DEFINITION_ID:
        raise PublishError(
            f"Build run did not originate from pipeline {BUILD_DEFINITION_ID}"
        )
    if tests.get("runId") != expected_tests_run:
        raise PublishError(
            f"tests run changed: expected {expected_tests_run}, "
            f"found {tests.get('runId')}"
        )
    if tests.get("pipelineId") != TESTS_DEFINITION_ID:
        raise PublishError(
            f"tests run did not originate from pipeline {TESTS_DEFINITION_ID}"
        )
    if build.get("sourceVersion") != expected_sha:
        raise PublishError(
            "selected Build run does not use the tested source commit"
        )
    if tests.get("sourceVersion") != expected_sha:
        raise PublishError(
            "selected Tests run does not use the tested source commit"
        )
    expected_branch = f"refs/heads/{release.branch}"
    if build.get("sourceBranch") != expected_branch:
        raise PublishError(
            "selected Build run does not use the release branch"
        )
    if bar.get("id") != expected_bar_build:
        raise PublishError(
            f"BAR build changed: expected {expected_bar_build}, "
            f"found {bar.get('id')}"
        )
    if bar.get("commit") != expected_sha:
        raise PublishError(
            "BAR build does not match the tested source commit"
        )
    if bar.get("buildRunId") != expected_build_run:
        raise PublishError(
            "BAR build is not linked to the pinned Build run"
        )
    if bar.get("buildDefinitionId") != BUILD_DEFINITION_ID:
        raise PublishError(
            "BAR build did not originate from the "
            f"skiasharp-package pipeline ({BUILD_DEFINITION_ID})"
        )
    if bar.get("state") != "ready":
        raise PublishError(
            f"BAR build {bar.get('id')} is not ready: {bar.get('state')}"
        )
    if PRODUCT_CHANNEL_ID not in (bar.get("defaultChannelIds") or []):
        raise PublishError(
            f"BAR build {bar.get('id')} is not mapped to .NET Libraries "
            f"channel {PRODUCT_CHANNEL_ID}"
        )
    if bar.get("branch") != expected_branch:
        raise PublishError("BAR build does not use the release branch")
    if bar.get("buildNumber") != build.get("buildNumber"):
        raise PublishError(
            "BAR build number does not match the pinned Build run"
        )
    duplicate_transport = {
        name: versions
        for name, versions in (
            bar.get("nonShippingAssets") or {}
        ).items()
        if len(versions) > 1
    }
    if duplicate_transport:
        raise PublishError(
            "BAR build contains ambiguous duplicate NonShipping transport "
            f"asset IDs: {duplicate_transport}"
        )
    versions = status.get("packageVersions")
    if not versions or not versions.get("test") or not versions.get("public"):
        raise PublishError("release-status returned no package versions")
    if versions["test"] != versions["public"]:
        raise PublishError(
            "test and public package versions must be the same BAR assets"
        )
    assets = bar.get("assets") or {}
    for package_id in BAR_ASSET_PACKAGES:
        asset = assets.get(package_id) or {}
        expected_version = versions["public"].get(package_id)
        if not asset or not asset.get("version"):
            raise PublishError(
                f"BAR build {bar.get('id')} is missing the {package_id} "
                "asset"
            )
        if asset.get("version") != expected_version:
            raise PublishError(
                f"BAR asset version for {package_id} is "
                f"{asset.get('version')}, expected {expected_version}"
            )
        locations = asset.get("locations") or []
        if locations:
            has_product = any(
                PRODUCT_FEED_MARKER in str(location).lower()
                for location in locations
            )
            has_transport = any(
                TRANSPORT_FEED_MARKER in str(location).lower()
                for location in locations
            )
            if not has_product or has_transport:
                raise PublishError(
                    f"BAR build {bar.get('id')} does not route {package_id} "
                    "exclusively to dotnet-libraries"
                )
        if not locations and not (
            (bar.get("routedAssets") or {}).get(package_id)
        ):
            raise PublishError(
                f"BAR build {bar.get('id')} has no BAR location or exact "
                f"dotnet-libraries package proof for {package_id}"
            )
    transport_assets = bar.get("transportAssets") or {}
    routed_transport = bar.get("routedTransportAssets") or {}
    for package_id in TRANSPORT_ASSET_PACKAGES:
        asset = transport_assets.get(package_id) or {}
        if not asset.get("version"):
            raise PublishError(
                f"BAR build {bar.get('id')} is missing the {package_id} "
                "NonShipping asset"
            )
        locations = asset.get("locations") or []
        if locations:
            has_transport = any(
                TRANSPORT_FEED_MARKER in str(location).lower()
                for location in locations
            )
            has_product = any(
                PRODUCT_FEED_MARKER in str(location).lower()
                for location in locations
            )
            if not has_transport or has_product:
                raise PublishError(
                    f"BAR build {bar.get('id')} does not route {package_id} "
                    "exclusively to dotnet-libraries-transport"
                )
        if not locations and not routed_transport.get(package_id):
            raise PublishError(
                f"BAR build {bar.get('id')} has no BAR location or exact "
                f"dotnet-libraries-transport package proof for {package_id}"
            )
    public_skia = versions["public"].get("SkiaSharp") or ""
    release.validate_public_version(public_skia)
    return {
        "build": build,
        "tests": tests,
        "bar": bar,
        "versions": versions,
    }


class NuGet:
    def package_exists(self, package_id: str, version: str) -> bool:
        lower_id = package_id.lower()
        lower_version = version.lower()
        url = (
            "https://api.nuget.org/v3-flatcontainer/"
            f"{urllib.parse.quote(lower_id)}/{urllib.parse.quote(lower_version)}/"
            f"{urllib.parse.quote(lower_id)}.nuspec"
        )
        request = urllib.request.Request(
            url,
            method="HEAD",
            headers={"User-Agent": "SkiaSharp-release-publish"},
        )
        try:
            with urllib.request.urlopen(request, timeout=30) as response:
                return response.status == 200
        except urllib.error.HTTPError as error:
            if error.code == 404:
                return False
            raise PublishError(
                f"NuGet.org request failed for {package_id} {version}: {error}"
            ) from error
        except (urllib.error.URLError, TimeoutError) as error:
            raise PublishError(
                f"NuGet.org request failed for {package_id} {version}: {error}"
            ) from error

    def check(self, versions: dict) -> dict:
        packages = {
            package_id: {
                "version": version,
                "available": self.package_exists(package_id, version),
            }
            for package_id, version in versions["public"].items()
        }
        available = [
            item["available"] for item in packages.values()
        ]
        state = (
            "ready"
            if all(available)
            else "missing"
            if not any(available)
            else "partial"
        )
        return {
            "state": state,
            "packages": packages,
        }


def operation(
    operation_id: str,
    status: str,
    detail: str,
    *,
    url: str | None = None,
) -> dict:
    item = {
        "id": operation_id,
        "status": status,
        "detail": detail,
    }
    if url:
        item["url"] = url
    return item
