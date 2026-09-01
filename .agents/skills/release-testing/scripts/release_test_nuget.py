#!/usr/bin/env python3
"""Resolve and inspect packages from one BAR-specific NuGet feed."""

from __future__ import annotations
from dataclasses import dataclass
import io
import json
import re
import urllib.error
import urllib.parse
import urllib.request
import xml.etree.ElementTree as ET
import zipfile
import release_test_common as common

USER_AGENT = {"User-Agent": "SkiaSharp-release-testing"}


@dataclass(frozen=True)
class PackageIdentity:
    id: str
    version: str
    branch: str
    commit: str
    harfbuzz_versions: tuple[str, ...]


def is_concrete_version(value: str | None) -> bool:
    return bool(value) and not re.search(r"[\s\[\](),*]", value)


def resolve_flat_container(index_url: str) -> str:
    request = urllib.request.Request(index_url, headers=USER_AGENT)
    try:
        with urllib.request.urlopen(request, timeout=60) as response:
            index = json.load(response)
    except (json.JSONDecodeError, urllib.error.HTTPError, urllib.error.URLError, TimeoutError) as error:
        raise common.ReleaseTestError(f"could not read BAR package feed {index_url}: {error}") from error

    resources = [
        resource.get("@id")
        for resource in index.get("resources") or []
        if str(resource.get("@type") or "").startswith("PackageBaseAddress") and resource.get("@id")
    ]
    if len(resources) != 1:
        raise common.ReleaseTestError(f"BAR package feed has {len(resources)} flat-container resources")
    return str(resources[0]).rstrip("/") + "/"


def service_index_from_flat_container(flat_container: str) -> str:
    if not flat_container.endswith("flat2/"):
        raise common.ReleaseTestError(f"unexpected BAR flat-container URL: {flat_container}")
    return flat_container.removesuffix("flat2/") + "index.json"


def read_package(package_id: str, version: str, flat_container: str) -> PackageIdentity:
    lower_id = package_id.lower()
    lower_version = version.lower()
    package_name = f"{urllib.parse.quote(lower_id)}.{urllib.parse.quote(lower_version)}.nupkg"
    url = f"{flat_container}{urllib.parse.quote(lower_id)}/{urllib.parse.quote(lower_version)}/{package_name}"
    request = urllib.request.Request(url, headers=USER_AGENT)
    try:
        with urllib.request.urlopen(request, timeout=60) as response:
            content = response.read()
    except (urllib.error.HTTPError, urllib.error.URLError, TimeoutError) as error:
        raise common.ReleaseTestError(f"BAR feed package {package_id} {version} is unavailable: {error}") from error

    try:
        with zipfile.ZipFile(io.BytesIO(content)) as package:
            nuspecs = [name for name in package.namelist() if name.lower().endswith(".nuspec")]
            if len(nuspecs) != 1:
                raise common.ReleaseTestError(f"{package_id} {version} contains {len(nuspecs)} nuspecs")
            metadata = ET.fromstring(package.read(nuspecs[0])).find("{*}metadata")
    except (ET.ParseError, zipfile.BadZipFile) as error:
        raise common.ReleaseTestError(f"BAR feed package {package_id} {version} is malformed: {error}") from error

    if metadata is None:
        raise common.ReleaseTestError(f"{package_id} {version} has no nuspec metadata")
    actual_id = metadata.findtext("{*}id")
    actual_version = metadata.findtext("{*}version")
    repository = metadata.find("{*}repository")
    if (
        actual_id != package_id
        or actual_version != version
        or repository is None
        or not repository.get("branch")
        or not re.fullmatch(r"[0-9a-f]{40}", repository.get("commit") or "")
    ):
        raise common.ReleaseTestError(f"{package_id} {version} has inconsistent source metadata")

    dependencies = {
        dependency.get("version")
        for dependency in metadata.findall(".//{*}dependency")
        if dependency.get("id") == "HarfBuzzSharp" and dependency.get("version")
    }
    return PackageIdentity(
        id=actual_id, version=actual_version, branch=repository.get("branch"), commit=repository.get("commit"), harfbuzz_versions=tuple(sorted(dependencies))
    )
