#!/usr/bin/env python3
"""Measure the NuGet packages a PR build produced, for the PR size-diff comment.

Given an Azure DevOps build id, this downloads that build's ``nuget`` pipeline artifact
(the full set of ``.nupkg`` files the ``Package NuGets`` job published), unpacks it, and
measures every package the same way the nightly tracker does — the top-level ``.nupkg``
size plus EVERY file inside it (uncompressed), with the native-binary subset flagged.

The output JSON is consumed by ``render_pr_md.py`` to render a size-diff against the
latest nightly baseline on the ``aw-data`` branch.

Nothing is rebuilt here: the packages come straight from the artifact the AzDO pipeline
already produced, so this runs entirely on a plain GitHub Actions runner (stdlib only).

Output schema::

    {
      "buildId": "1517880",
      "prNumber": 4487,                # or null if it could not be resolved
      "org": "dnceng-public",
      "project": "public",
      "artifact": "nuget",
      "packages": {
        "<packageId>": {
          "version": "<full version>",
          "nupkg": <compressed .nupkg size>,
          "files":   {<path>: <uncompressed size>},   # EVERY entry
          "natives": {<path>: <uncompressed size>}     # native-binary subset
        }
      }
    }

See ``.github/workflows/track-artifact-sizes-pr.yml``.
"""

from __future__ import annotations

import argparse
import glob
import json
import os
import re
import sys
import tempfile
import xml.etree.ElementTree as ET
import zipfile

sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))  # perf/
from _common import http_get_json, log  # noqa: E402

# Reuse the exact measurement + native-detection the nightly tracker uses so the
# PR numbers are apples-to-apples with the persisted baseline.
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))  # sizes/
from track import http_download, measure_nupkg  # noqa: E402

DEFAULT_ORG = "dnceng-public"
DEFAULT_PROJECT = "public"
DEFAULT_ARTIFACT = "nuget"

_PR_BRANCH_RE = re.compile(r"refs/pull/(\d+)/merge", re.IGNORECASE)


def _api_base(org: str, project: str) -> str:
    return f"https://dev.azure.com/{org}/{project}/_apis"


def resolve_artifact_url(org: str, project: str, build_id: str, artifact: str) -> str:
    """Return the (anonymous) download URL for a named pipeline artifact."""
    url = (f"{_api_base(org, project)}/build/builds/{build_id}/artifacts"
           f"?artifactName={artifact}&api-version=7.1")
    data = http_get_json(url)
    # When artifactName is given the object is returned directly; be lenient and
    # also handle a {"value": [...]} list in case the API shape changes.
    if "resource" not in data and isinstance(data.get("value"), list):
        data = next((a for a in data["value"] if a.get("name") == artifact), {})
    download_url = (data.get("resource") or {}).get("downloadUrl")
    if not download_url:
        raise RuntimeError(f"Build {build_id} has no downloadable '{artifact}' artifact.")
    return download_url


def resolve_pr_number(org: str, project: str, build_id: str) -> int | None:
    """Resolve the PR number from the build's source branch (refs/pull/NNNN/merge)."""
    try:
        build = http_get_json(
            f"{_api_base(org, project)}/build/builds/{build_id}?api-version=7.1")
    except RuntimeError as err:
        log(f"  ! could not fetch build {build_id}: {err}")
        return None
    source_branch = build.get("sourceBranch", "") or ""
    m = _PR_BRANCH_RE.search(source_branch)
    if m:
        return int(m.group(1))
    log(f"  ! build source branch is not a PR ref: {source_branch!r}")
    return None


def download_and_extract(org: str, project: str, build_id: str, artifact: str,
                         work_dir: str) -> str:
    """Download the artifact zip and extract it; return the folder of ``.nupkg`` files."""
    download_url = resolve_artifact_url(org, project, build_id, artifact)
    zip_path = os.path.join(work_dir, f"{artifact}.zip")
    log(f"  downloading '{artifact}' artifact of build {build_id} ...")
    http_download(download_url, zip_path)
    log(f"  downloaded {os.path.getsize(zip_path) / (1024 * 1024):.0f} MB; extracting ...")
    extract_dir = os.path.join(work_dir, "extracted")
    with zipfile.ZipFile(zip_path) as zf:
        zf.extractall(extract_dir)
    os.remove(zip_path)
    return extract_dir


def read_nuspec_identity(nupkg_path: str) -> tuple[str | None, str | None]:
    """Read the authoritative package id + version from the ``.nuspec`` inside a nupkg."""
    try:
        with zipfile.ZipFile(nupkg_path) as zf:
            nuspec = next((n for n in zf.namelist()
                           if n.endswith(".nuspec") and "/" not in n), None)
            if not nuspec:
                return None, None
            root = ET.fromstring(zf.read(nuspec))
    except (zipfile.BadZipFile, ET.ParseError, KeyError) as err:
        log(f"  ! could not read nuspec from {os.path.basename(nupkg_path)}: {err}")
        return None, None
    # Namespace-agnostic lookups ({*} matches any namespace).
    meta = root.find("{*}metadata")
    if meta is None:
        return None, None
    pid = meta.findtext("{*}id")
    version = meta.findtext("{*}version")
    return (pid or None), (version or None)


def measure_directory(nupkg_dir: str) -> dict[str, dict]:
    """Measure every ``.nupkg`` under ``nupkg_dir``, keyed by package id.

    A build can contain both a stable and a prerelease build of the same id (main
    builds). PR builds only ever produce prereleases, so if a duplicate id appears we
    keep the prerelease variant to stay representative of a real PR.
    """
    packages: dict[str, dict] = {}
    nupkgs = sorted(glob.glob(os.path.join(nupkg_dir, "**", "*.nupkg"), recursive=True))
    if not nupkgs:
        raise RuntimeError(f"No .nupkg files found under {nupkg_dir}")
    for path in nupkgs:
        pid, version = read_nuspec_identity(path)
        if not pid:
            log(f"  ! skipping (no id): {os.path.basename(path)}")
            continue
        measurement = measure_nupkg(path)
        measurement["version"] = version
        existing = packages.get(pid)
        if existing is not None:
            keep_new = _is_prerelease(version) and not _is_prerelease(existing.get("version"))
            log(f"  ! duplicate package id {pid} "
                f"({existing.get('version')} vs {version}); "
                f"keeping {version if keep_new else existing.get('version')}")
            if not keep_new:
                continue
        packages[pid] = measurement
    log(f"  measured {len(packages)} packages "
        f"({sum(1 for p in packages.values() if p['natives'])} native)")
    return packages


def _is_prerelease(version: str | None) -> bool:
    return bool(version) and "-" in version


def parse_args(argv: list[str]) -> argparse.Namespace:
    p = argparse.ArgumentParser(description=__doc__,
                                formatter_class=argparse.RawDescriptionHelpFormatter)
    p.add_argument("--build-id", default=None,
                   help="Azure DevOps build id to download the nuget artifact from.")
    p.add_argument("--nupkg-dir", default=None,
                   help="Measure an already-extracted folder of .nupkg files instead of "
                        "downloading (for local testing).")
    p.add_argument("--org", default=DEFAULT_ORG, help=f"AzDO org (default: {DEFAULT_ORG}).")
    p.add_argument("--project", default=DEFAULT_PROJECT,
                   help=f"AzDO project (default: {DEFAULT_PROJECT}).")
    p.add_argument("--artifact", default=DEFAULT_ARTIFACT,
                   help=f"Pipeline artifact name (default: {DEFAULT_ARTIFACT}).")
    p.add_argument("--pr-number", type=int, default=None,
                   help="Override the PR number (default: resolve from the build).")
    p.add_argument("--output", default="pr-sizes.json", help="Where to write the measurement JSON.")
    p.add_argument("--work-dir", default=None, help="Temp dir for the download (default: temp).")
    return p.parse_args(argv)


def main(argv: list[str]) -> int:
    args = parse_args(argv)
    if not args.build_id and not args.nupkg_dir:
        print("error: one of --build-id or --nupkg-dir is required", file=sys.stderr)
        return 2

    owns_work_dir = args.work_dir is None
    work_dir = args.work_dir or tempfile.mkdtemp(prefix="pr-sizes-")
    os.makedirs(work_dir, exist_ok=True)

    try:
        if args.nupkg_dir:
            nupkg_dir = args.nupkg_dir
        else:
            nupkg_dir = download_and_extract(
                args.org, args.project, args.build_id, args.artifact, work_dir)

        packages = measure_directory(nupkg_dir)

        pr_number = args.pr_number
        if pr_number is None and args.build_id:
            pr_number = resolve_pr_number(args.org, args.project, args.build_id)

        result = {
            "buildId": args.build_id,
            "prNumber": pr_number,
            "org": args.org,
            "project": args.project,
            "artifact": args.artifact,
            "packages": packages,
        }
        with open(args.output, "w", encoding="utf-8") as fh:
            json.dump(result, fh, indent=2, sort_keys=True)
        log(f"  wrote PR measurement -> {args.output} "
            f"(build {args.build_id}, PR {pr_number})")
    finally:
        if owns_work_dir:
            import shutil
            shutil.rmtree(work_dir, ignore_errors=True)

    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
