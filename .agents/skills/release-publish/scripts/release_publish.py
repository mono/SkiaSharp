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
import tempfile
import urllib.error
import urllib.parse
import urllib.request


STATUS_SCRIPT = (
    Path(__file__).resolve().parents[2]
    / "release-status"
    / "scripts"
    / "pipeline-status.py"
)
AZURE_ORG = "https://devdiv.visualstudio.com"
AZURE_PROJECT = "DevDiv"
PUBLISH_PIPELINE_ID = 25298
AZURE_WEB_ORG = "devdiv"
RELEASE_BRANCH_RE = re.compile(
    r"^release/(?P<numeric>\d+\.\d+\.\d+(?:\.\d+)?)"
    r"(?:-(?P<channel>preview|rc)\.(?P<iteration>\d+))?$"
)
class PublishError(RuntimeError):
    """The release could not be audited or advanced safely."""


def azure_publish_run_url(run_id: int) -> str:
    return (
        f"https://dev.azure.com/{AZURE_WEB_ORG}/{AZURE_PROJECT}/"
        f"_build/results?buildId={run_id}&view=results"
    )


class AzurePublish:
    def __init__(self) -> None:
        self.az_path = shutil.which("az")
        if not self.az_path:
            raise PublishError("Azure CLI 'az' was not found on PATH")

    def json(self, args: list[str], *, timeout: int = 120):
        return run_json(
            [self.az_path, *args],
            timeout=timeout,
        )

    @staticmethod
    def request_body(
        managed_build_number: str,
        *,
        stable: bool,
        preview: bool,
    ) -> dict:
        return {
            "previewRun": preview,
            "resources": {
                "pipelines": {
                    "SkiaSharp": {
                        "version": managed_build_number,
                    }
                }
            },
            "templateParameters": {
                "selectedResource": "SkiaSharp",
                "pushPackages": True,
                "pushStable": stable,
            },
        }

    def invoke_run(self, body: dict) -> dict:
        path: Path | None = None
        try:
            with tempfile.NamedTemporaryFile(
                mode="w",
                encoding="utf-8",
                suffix=".json",
                delete=False,
            ) as stream:
                json.dump(body, stream)
                path = Path(stream.name)
            return self.json(
                [
                    "devops",
                    "invoke",
                    "--org",
                    AZURE_ORG,
                    "--area",
                    "pipelines",
                    "--resource",
                    "runs",
                    "--route-parameters",
                    f"project={AZURE_PROJECT}",
                    f"pipelineId={PUBLISH_PIPELINE_ID}",
                    "--http-method",
                    "POST",
                    "--in-file",
                    str(path),
                    "--api-version",
                    "7.1",
                    "-o",
                    "json",
                ],
                timeout=240,
            )
        finally:
            if path:
                path.unlink(missing_ok=True)

    def preview(
        self,
        managed_build_number: str,
        *,
        stable: bool,
    ) -> bool:
        response = self.invoke_run(
            self.request_body(
                managed_build_number,
                stable=stable,
                preview=True,
            )
        )
        return response.get("id") == -1 and bool(response.get("finalYaml"))

    def queue(
        self,
        managed_build_number: str,
        *,
        stable: bool,
    ) -> dict:
        return self.invoke_run(
            self.request_body(
                managed_build_number,
                stable=stable,
                preview=False,
            )
        )

    def run_detail(self, run_id: int) -> dict:
        return self.json(
            [
                "devops",
                "invoke",
                "--org",
                AZURE_ORG,
                "--area",
                "pipelines",
                "--resource",
                "runs",
                "--route-parameters",
                f"project={AZURE_PROJECT}",
                f"pipelineId={PUBLISH_PIPELINE_ID}",
                f"runId={run_id}",
                "--api-version",
                "7.1",
                "-o",
                "json",
            ]
        )

    def matching_runs(
        self,
        managed_run_id: int,
        build_number: str,
        *,
        stable: bool,
    ) -> list[dict]:
        expected_name = f"SkiaSharp {build_number}"
        builds = self.json(
            [
                "pipelines",
                "runs",
                "list",
                "--pipeline-ids",
                str(PUBLISH_PIPELINE_ID),
                "--org",
                AZURE_ORG,
                "--project",
                AZURE_PROJECT,
                "--top",
                "100",
                "-o",
                "json",
            ]
        )
        candidates = [
            build
            for build in builds
            if build.get("buildNumber") == expected_name
            or build.get("status") != "completed"
        ]
        matched = []
        for build in candidates:
            detail = self.run_detail(int(build["id"]))
            resource = (
                (detail.get("resources") or {})
                .get("pipelines", {})
                .get("SkiaSharp", {})
            )
            pipeline = resource.get("pipeline") or {}
            parameters = detail.get("templateParameters") or {}
            # Pipeline resources expose the selected run ID in pipeline.id.
            if int(pipeline.get("id") or 0) != managed_run_id:
                continue
            if resource.get("version") != build_number:
                continue
            if parameters.get("selectedResource") != "SkiaSharp":
                continue
            if str(parameters.get("pushPackages")).lower() != "true":
                continue
            if (
                str(parameters.get("pushStable")).lower()
                != str(stable).lower()
            ):
                continue
            matched.append(
                {
                    "runId": int(build["id"]),
                    "name": build.get("buildNumber"),
                    "status": build.get("status"),
                    "result": build.get("result"),
                    "queueTime": build.get("queueTime"),
                    "url": azure_publish_run_url(int(build["id"])),
                }
            )
        return sorted(
            matched,
            key=lambda item: item["runId"],
            reverse=True,
        )


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
            rf"{re.escape(self.raw)}\.(\d+)",
            version,
        )
        if not match or int(match.group(1)) == 0:
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
    expected_managed_run: int,
    expected_tests_run: int,
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
    managed = status.get("managedRun") or {}
    tests = status.get("testsRun") or {}
    if managed.get("runId") != expected_managed_run:
        raise PublishError(
            f"managed run changed: expected {expected_managed_run}, "
            f"found {managed.get('runId')}"
        )
    if tests.get("runId") != expected_tests_run:
        raise PublishError(
            f"tests run changed: expected {expected_tests_run}, "
            f"found {tests.get('runId')}"
        )
    if managed.get("sourceVersion") != expected_sha:
        raise PublishError(
            "selected managed run does not use the tested source commit"
        )
    versions = status.get("packageVersions")
    if not versions or not versions.get("test") or not versions.get("public"):
        raise PublishError("release-status returned no package versions")
    public_skia = versions["public"].get("SkiaSharp") or ""
    release.validate_public_version(public_skia)
    return {
        "managed": managed,
        "tests": tests,
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
