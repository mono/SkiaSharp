#!/usr/bin/env python3
"""Resolve one BAR build and download its signed SkiaSharp NuGet assets."""

from __future__ import annotations

import argparse
import hashlib
import json
import os
from pathlib import Path
import re
import shutil
import subprocess
import sys
import xml.etree.ElementTree as ET
import zipfile


BAR_URI = "https://maestro.dot.net"
DEFAULT_REPOSITORY = "https://github.com/mono/SkiaSharp"
PACKAGE_NAME_PATTERN = r"^(?:SkiaSharp|HarfBuzzSharp)(?:\..*)?$"
REQUIRED_PACKAGE_IDS = {"SkiaSharp", "HarfBuzzSharp"}
SECRET_OPTIONS = {"--azdev-pat", "-p", "--password"}


class DownloadError(RuntimeError):
    """The requested package drop could not be proven or downloaded safely."""


def normalized_repository(value: str) -> str:
    normalized = value.strip().rstrip("/").lower()
    return normalized[:-4] if normalized.endswith(".git") else normalized


def redact(args: list[str]) -> list[str]:
    result = list(args)
    for index, value in enumerate(result[:-1]):
        if value in SECRET_OPTIONS:
            result[index + 1] = "***"
    return result


def run(args: list[str], *, timeout: int) -> subprocess.CompletedProcess[str]:
    try:
        result = subprocess.run(
            args,
            capture_output=True,
            text=True,
            timeout=timeout,
        )
    except FileNotFoundError as error:
        raise DownloadError(
            f"{args[0]} was not found; run eng/common/darc-init.ps1 or "
            "eng/common/darc-init.sh first"
        ) from error
    except subprocess.TimeoutExpired as error:
        raise DownloadError(
            f"command timed out after {timeout}s: "
            f"{subprocess.list2cmdline(redact(args))}"
        ) from error
    if result.returncode != 0:
        detail = result.stderr.strip() or result.stdout.strip() or "no output"
        raise DownloadError(
            f"command failed ({result.returncode}): "
            f"{subprocess.list2cmdline(redact(args))}\n{detail}"
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
    raise DownloadError("Darc returned no valid JSON")


def auth_arguments(args) -> tuple[list[str], dict]:
    command = ["--bar-uri", args.bar_uri]
    evidence = {
        "barUri": args.bar_uri,
        "barAuthentication": "default",
        "azureDevOpsAuthentication": "default",
    }
    if args.ci:
        command.append("--ci")
    for option, env_name, evidence_name in (
        ("-p", args.bar_password_env, "barAuthentication"),
        ("--azdev-pat", args.azdev_pat_env, "azureDevOpsAuthentication"),
    ):
        if not env_name:
            continue
        value = os.environ.get(env_name)
        if not value:
            raise DownloadError(
                f"environment variable {env_name} is required but empty"
            )
        command.extend([option, value])
        evidence[evidence_name] = f"environment:{env_name}"
    return command, evidence


def darc_json(
    darc: str,
    command: list[str],
    auth: list[str],
    *,
    timeout: int = 120,
):
    result = run(
        [
            darc,
            *command,
            *auth,
            "--output-format",
            "json",
        ],
        timeout=timeout,
    )
    return parse_json_output(result.stdout)


def build_matches(
    build: dict,
    *,
    repository: str,
    commit: str,
    channel: str | None,
) -> bool:
    channels = build.get("channels") or []
    return (
        normalized_repository(str(build.get("repository") or ""))
        == normalized_repository(repository)
        and str(build.get("commit") or "").lower() == commit.lower()
        and (
            channel is None
            or any(
                str(item).casefold() == channel.casefold()
                for item in channels
            )
        )
    )


def select_build(builds, *, repository: str, commit: str, channel: str | None):
    if not isinstance(builds, list):
        raise DownloadError("Darc build query did not return a JSON array")
    matches = {
        int(build["id"]): build
        for build in builds
        if isinstance(build, dict)
        and build.get("id") is not None
        and build_matches(
            build,
            repository=repository,
            commit=commit,
            channel=channel,
        )
    }
    if len(matches) != 1:
        raise DownloadError(
            "expected exactly one BAR build matching repository, commit, and "
            f"channel; found {len(matches)}"
        )
    build = next(iter(matches.values()))
    if not re.fullmatch(r"[0-9a-fA-F]{40}", str(build.get("commit") or "")):
        raise DownloadError("BAR build does not contain a full commit SHA")
    return build


def resolve_build(args, auth: list[str]) -> dict:
    if args.build_id is not None:
        builds = darc_json(
            args.darc,
            ["get-build", "--id", str(args.build_id)],
            auth,
        )
        channel = args.expected_channel
    else:
        builds = darc_json(
            args.darc,
            [
                "get-latest-build",
                "--repo",
                args.repository,
                "--channel",
                args.channel,
            ],
            auth,
        )
        channel = args.channel
    build = select_build(
        builds,
        repository=args.repository,
        commit=args.expected_commit,
        channel=channel,
    )
    if args.build_id is not None and int(build["id"]) != args.build_id:
        raise DownloadError(
            f"Darc returned BAR build {build['id']}, expected {args.build_id}"
        )
    if args.expected_branch:
        branch = str(build.get("branch") or "")
        if branch.casefold() != args.expected_branch.casefold():
            raise DownloadError(
                f"BAR build branch {branch!r} does not match "
                f"{args.expected_branch!r}"
            )
    return build


def prepare_output(output: Path) -> None:
    if output.exists() and any(output.iterdir()):
        raise DownloadError(f"output directory is not empty: {output}")
    output.mkdir(parents=True, exist_ok=True)


def gather_drop(args, auth: list[str], build: dict, output: Path) -> list[str]:
    command = [
        args.darc,
        "gather-drop",
        "--id",
        str(build["id"]),
        "--output-dir",
        str(output),
        "--asset-filter",
        PACKAGE_NAME_PATTERN,
        "--no-workarounds",
        "--include-released",
        "--overwrite",
        *auth,
    ]
    run(command, timeout=args.download_timeout)
    return redact(command)


def package_identity(path: Path) -> tuple[str, str]:
    try:
        with zipfile.ZipFile(path) as archive:
            nuspecs = [
                name
                for name in archive.namelist()
                if name.lower().endswith(".nuspec") and "/" not in name
            ]
            if len(nuspecs) != 1:
                raise DownloadError(
                    f"{path.name} contains {len(nuspecs)} root nuspec files"
                )
            root = ET.fromstring(archive.read(nuspecs[0]))
    except (OSError, zipfile.BadZipFile, ET.ParseError) as error:
        raise DownloadError(f"invalid NuGet package {path}: {error}") from error
    metadata = next(
        (element for element in root if element.tag.rsplit("}", 1)[-1] == "metadata"),
        None,
    )
    if metadata is None:
        raise DownloadError(f"{path.name} has no nuspec metadata")
    values = {
        element.tag.rsplit("}", 1)[-1]: (element.text or "").strip()
        for element in metadata
    }
    package_id = values.get("id", "")
    version = values.get("version", "")
    if not package_id or not version:
        raise DownloadError(f"{path.name} has no package id or version")
    return package_id, version


def inspect_packages(
    output: Path,
    expected: set[tuple[str, str]],
) -> list[dict]:
    package_root = output / "shipping" / "packages"
    paths = sorted(package_root.glob("*.nupkg"))
    if not paths:
        raise DownloadError("Darc downloaded no shipping NuGet packages")
    packages = []
    identities = set()
    for path in paths:
        package_id, version = package_identity(path)
        if not re.fullmatch(r"(?:SkiaSharp|HarfBuzzSharp)(?:\..*)?", package_id):
            raise DownloadError(f"unexpected package in Darc drop: {package_id}")
        identity = (package_id, version)
        if identity in identities:
            raise DownloadError(
                f"duplicate package identity in Darc drop: {package_id} {version}"
            )
        identities.add(identity)
        packages.append(
            {
                "id": package_id,
                "version": version,
                "file": str(path.relative_to(output)),
                "size": path.stat().st_size,
                "sha512": hashlib.sha512(path.read_bytes()).hexdigest(),
            }
        )
    missing_ids = REQUIRED_PACKAGE_IDS - {item[0] for item in identities}
    if missing_ids:
        raise DownloadError(
            "Darc drop is missing required package IDs: "
            + ", ".join(sorted(missing_ids))
        )
    missing = expected - identities
    if missing:
        raise DownloadError(
            "Darc drop is missing expected packages: "
            + ", ".join(f"{name} {version}" for name, version in sorted(missing))
        )
    return packages


def parse_expected(values: list[str]) -> set[tuple[str, str]]:
    expected = set()
    for value in values:
        package_id, separator, version = value.partition("=")
        if not separator or not package_id or not version:
            raise DownloadError(
                f"invalid --expected-package {value!r}; use ID=VERSION"
            )
        expected.add((package_id, version))
    missing_ids = REQUIRED_PACKAGE_IDS - {item[0] for item in expected}
    if missing_ids:
        raise DownloadError(
            "--expected-package must pin exact versions for: "
            + ", ".join(sorted(missing_ids))
        )
    return expected


def verify_signatures(dotnet: str, output: Path, packages: list[dict]) -> list[str]:
    command = [
        dotnet,
        "nuget",
        "verify",
        "--all",
        "--verbosity",
        "minimal",
        *[str(output / package["file"]) for package in packages],
    ]
    run(command, timeout=600)
    return command


def create_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(description=__doc__)
    selector = parser.add_mutually_exclusive_group(required=True)
    selector.add_argument("--build-id", type=int)
    selector.add_argument("--channel")
    parser.add_argument("--expected-commit", required=True)
    parser.add_argument("--expected-channel")
    parser.add_argument("--expected-branch")
    parser.add_argument("--expected-package", action="append", required=True)
    parser.add_argument("--repository", default=DEFAULT_REPOSITORY)
    parser.add_argument("--output-dir", type=Path, required=True)
    parser.add_argument("--evidence", type=Path)
    parser.add_argument("--darc", default=shutil.which("darc") or "darc")
    parser.add_argument("--dotnet", default=shutil.which("dotnet") or "dotnet")
    parser.add_argument("--bar-uri", default=BAR_URI)
    parser.add_argument("--bar-password-env")
    parser.add_argument("--azdev-pat-env")
    parser.add_argument("--ci", action="store_true")
    parser.add_argument("--download-timeout", type=int, default=1800)
    return parser


def execute(args) -> dict:
    if not re.fullmatch(r"[0-9a-fA-F]{40}", args.expected_commit):
        raise DownloadError("--expected-commit must be a full 40-character SHA")
    if args.channel and args.expected_channel:
        raise DownloadError(
            "--expected-channel is only valid with --build-id; "
            "--channel already pins it"
        )
    expected = parse_expected(args.expected_package)
    output = args.output_dir.resolve()
    prepare_output(output)
    auth, auth_evidence = auth_arguments(args)
    build = resolve_build(args, auth)
    gather_command = gather_drop(args, auth, build, output)
    packages = inspect_packages(output, expected)
    verification_command = verify_signatures(args.dotnet, output, packages)
    manifest = output / "manifest.json"
    if not manifest.is_file():
        raise DownloadError("Darc did not emit manifest.json")
    evidence_path = (
        args.evidence.resolve()
        if args.evidence
        else output / "darc-provenance.json"
    )
    report = {
        "schemaVersion": 1,
        "selector": {
            "buildId": args.build_id,
            "channel": args.channel,
            "expectedCommit": args.expected_commit.lower(),
            "expectedChannel": args.expected_channel,
            "expectedBranch": args.expected_branch,
            "repository": args.repository,
        },
        "resolvedBuild": build,
        "authentication": auth_evidence,
        "download": {
            "command": gather_command,
            "manifest": str(manifest),
            "manifestSha512": hashlib.sha512(manifest.read_bytes()).hexdigest(),
            "packageSource": str(output / "shipping" / "packages"),
        },
        "packages": packages,
        "signatureVerification": {
            "command": verification_command,
            "verified": True,
        },
    }
    evidence_path.parent.mkdir(parents=True, exist_ok=True)
    evidence_path.write_text(json.dumps(report, indent=2) + "\n", encoding="ascii")
    return report


def main() -> int:
    parser = create_parser()
    args = parser.parse_args()
    try:
        report = execute(args)
    except DownloadError as error:
        print(f"ERROR: {error}", file=sys.stderr)
        return 1
    print(json.dumps(report, indent=2))
    return 0


if __name__ == "__main__":
    sys.exit(main())
