#!/usr/bin/env python3
"""Resolve an exact SkiaSharp BAR build and its per-build NuGet feed."""

from __future__ import annotations
from dataclasses import dataclass
import re
import release_test_common as common

MAESTRO_URI = "https://maestro.dot.net"


@dataclass(frozen=True)
class BarBuild:
    id: int
    build_number: str | None
    azdo_build_id: int | None
    build_link: str | None
    branch: str
    commit: str
    package_feed: str


def query_assets(version: str, *, bar_id: int | None, max_age: int) -> list[dict]:
    args = ["darc", "get-asset", "--name", "SkiaSharp", "--version", version, "--max-age", str(max_age), "--bar-uri", MAESTRO_URI, "--output-format", "json"]
    if bar_id is not None:
        args.extend(["--build", str(bar_id)])

    value = common.parse_json_output(common.run_checked(args, timeout=180).stdout)
    if not isinstance(value, list):
        raise common.ReleaseTestError("Darc returned invalid asset JSON")
    return value


def resolve_build(version: str, *, bar_id: int | None = None, max_age: int = 30) -> BarBuild:
    candidates = {}
    for asset in query_assets(version, bar_id=bar_id, max_age=max_age):
        build = asset.get("build") or {}
        build_id = build.get("id")
        if asset.get("name") == "SkiaSharp" and asset.get("version") == version and isinstance(build_id, int):
            candidates[build_id] = asset

    if not candidates:
        scope = f" in BAR build {bar_id}" if bar_id is not None else ""
        raise common.ReleaseTestError(f"SkiaSharp {version} was not found in Maestro{scope}")
    if len(candidates) != 1:
        build_ids = ", ".join(str(value) for value in sorted(candidates))
        raise common.ReleaseTestError(f"multiple BAR builds contain SkiaSharp {version}: {build_ids}; select one with --bar-id")

    asset = next(iter(candidates.values()))
    build = asset["build"]
    commit = str(build.get("commit") or "")
    branch = str(build.get("branch") or "")
    if not branch or not re.fullmatch(r"[0-9a-f]{40}", commit):
        raise common.ReleaseTestError(f"BAR build {build['id']} has incomplete source metadata")
    if build.get("released") is True:
        raise common.ReleaseTestError(f"BAR build {build['id']} is already released")

    feeds = [location for location in asset.get("locations") or [] if re.search(r"/_packaging/[^/]+/nuget/v3/index\.json$", location)]
    if not feeds:
        raise common.ReleaseTestError(f"BAR build {build['id']} has no NuGet feed locations")
    if len(feeds) > 1:
        raise common.ReleaseTestError(f"BAR build {build['id']} has multiple NuGet feed locations")

    return BarBuild(
        id=build["id"],
        build_number=build.get("buildNumber"),
        azdo_build_id=build.get("azdoBuildId"),
        build_link=build.get("buildLink"),
        branch=branch.removeprefix("refs/heads/"),
        commit=commit,
        package_feed=feeds[0],
    )
