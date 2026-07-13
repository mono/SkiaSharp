#!/usr/bin/env python3
"""Collect SkiaSharp/HarfBuzzSharp NuGet package + native binary sizes.

This script builds/updates a JSON "history" document that records:

  * the top-level ``.nupkg`` size of every package produced by the build, and
  * for packages that ship native binaries, the size of each individual native
    payload (per os/arch), with symbol files (``.pdb``) excluded.

Two kinds of data points ("columns") are collected:

  * ``nightly`` -- the latest *succeeded* ``main`` build of the Azure DevOps
    ``xamarin/public`` pipeline (definitionId 4). Packages are read from the
    ``nuget_preview`` build artifact. The newest ``--max-nightly`` days are kept.
  * ``released`` -- reference versions resolved from NuGet.org. For every package
    four roles are resolved from that package's *own* version line
    (SkiaSharp and HarfBuzzSharp version independently):
        prev-major  -> newest stable of the previous major line
        prev-stable -> second-newest stable in the current major
        curr-stable -> newest stable
        latest      -> newest overall (including preview / rc)
    Released package sizes are immutable, so they are measured once and cached.

Only the Python standard library is used so the script runs on a clean runner.

The history document is persisted between runs via the GitHub Actions cache; see
``.github/workflows/track-artifact-sizes.yml``.
"""

from __future__ import annotations

import argparse
import datetime as dt
import json
import os
import re
import shutil
import sys
import tempfile
import time
import urllib.error
import urllib.request
import zipfile

# --------------------------------------------------------------------------- #
# Constants
# --------------------------------------------------------------------------- #

SCHEMA_VERSION = 1

AZDO_ORG = "xamarin"
AZDO_PROJECT = "public"
AZDO_PIPELINE_ID = 4
AZDO_API = f"https://dev.azure.com/{AZDO_ORG}/{AZDO_PROJECT}/_apis"

NUGET_FLATCONTAINER = "https://api.nuget.org/v3-flatcontainer"

# The build artifact that contains the preview-versioned packages produced by
# every main build.
NIGHTLY_ARTIFACT = "nuget_preview"

ROLES = ("prev-major", "prev-stable", "curr-stable", "latest")

USER_AGENT = "skiasharp-artifact-size-tracker/1.0 (+https://github.com/mono/SkiaSharp)"

# Native binary file extensions (managed .dll are filtered out separately).
_NATIVE_EXTS = (".so", ".dylib", ".dll", ".a")


# --------------------------------------------------------------------------- #
# Small HTTP helpers (stdlib only)
# --------------------------------------------------------------------------- #

def _log(msg: str) -> None:
    print(msg, flush=True)


def http_get(url: str, *, retries: int = 4, timeout: int = 120) -> bytes:
    """GET a URL returning the raw bytes, with simple exponential backoff."""
    last_err: Exception | None = None
    for attempt in range(1, retries + 1):
        try:
            req = urllib.request.Request(url, headers={"User-Agent": USER_AGENT})
            with urllib.request.urlopen(req, timeout=timeout) as resp:
                return resp.read()
        except (urllib.error.URLError, TimeoutError, ConnectionError) as err:
            last_err = err
            wait = min(30, 2 ** attempt)
            _log(f"  ! request failed ({err}); retry {attempt}/{retries} in {wait}s")
            time.sleep(wait)
    raise RuntimeError(f"GET failed after {retries} attempts: {url}\n  {last_err}")


def http_get_json(url: str, **kwargs) -> dict:
    return json.loads(http_get(url, **kwargs).decode("utf-8"))


def http_download(url: str, dest: str, *, retries: int = 4, timeout: int = 600) -> None:
    """Download a URL to ``dest`` fully (streamed), with retries."""
    last_err: Exception | None = None
    for attempt in range(1, retries + 1):
        try:
            req = urllib.request.Request(url, headers={"User-Agent": USER_AGENT})
            with urllib.request.urlopen(req, timeout=timeout) as resp, open(dest, "wb") as fh:
                shutil.copyfileobj(resp, fh, length=1024 * 1024)
            return
        except (urllib.error.URLError, TimeoutError, ConnectionError) as err:
            last_err = err
            wait = min(30, 2 ** attempt)
            _log(f"  ! download failed ({err}); retry {attempt}/{retries} in {wait}s")
            time.sleep(wait)
            if os.path.exists(dest):
                os.remove(dest)
    raise RuntimeError(f"Download failed after {retries} attempts: {url}\n  {last_err}")


# --------------------------------------------------------------------------- #
# NuGet package measurement
# --------------------------------------------------------------------------- #

_NUPKG_RE = re.compile(r"^(?P<id>.+?)\.(?P<ver>\d+\.\d+.*?)\.nupkg$", re.IGNORECASE)


def split_nupkg_name(filename: str) -> tuple[str, str] | None:
    """Split ``SkiaSharp.NativeAssets.Win32.4.151.0-preview.1.1.nupkg`` into
    ``("SkiaSharp.NativeAssets.Win32", "4.151.0-preview.1.1")``.

    Symbol packages are ignored (returns ``None``).
    """
    low = filename.lower()
    if low.endswith(".snupkg") or ".symbols." in low:
        return None
    m = _NUPKG_RE.match(filename)
    if not m:
        return None
    return m.group("id"), m.group("ver")


def _is_native_entry(name: str) -> bool:
    """Return True if a zip entry is an actual native binary payload.

    Handles the layouts SkiaSharp ships:
      * ``runtimes/<rid>/native/<lib>.so|.dylib|.dll``
      * ``runtimes/<rid>/native/<lib>.framework/<lib>``      (Apple mach-o binary)
      * ``runtimes/<rid>/native/<lib>.framework.zip``        (MacCatalyst)
      * ``buildTransitive/.../libSkiaSharp.a/.../libSkiaSharp.a`` (WebAssembly)

    Symbol files and framework metadata are excluded.
    """
    if name.endswith("/"):
        return False
    low = name.lower()
    base = name.rsplit("/", 1)[-1]

    # Never count symbols or framework metadata.
    if low.endswith((".pdb", ".dwarf", ".plist")):
        return False
    if "/_codesignature/" in low or base == "Info.plist":
        return False

    # Compressed Apple frameworks are shipped as a single payload archive.
    if low.endswith((".framework.zip", ".xcframework.zip")):
        return True

    # Mach-o binary inside an (uncompressed) *.framework directory: the file
    # name matches the framework name and has no extension.
    if ".framework/" in low:
        parts = name.split("/")
        if len(parts) >= 2 and parts[-2].endswith(".framework"):
            framework_stem = parts[-2][: -len(".framework")]
            if base == framework_stem:
                return True
        return False

    ext = os.path.splitext(base)[1].lower()
    if ext not in _NATIVE_EXTS:
        return False

    # Managed assemblies are ``.dll`` files under ``lib/``; native binaries only
    # ever live in a ``.../native/...`` runtime folder, so gate ``.dll`` on that.
    if ext == ".dll":
        return "/native/" in low

    return True


def measure_nupkg(path: str) -> dict:
    """Measure a single ``.nupkg``.

    Returns ``{"nupkg": <compressed file size>, "natives": {<path>: <size>, ...}}``
    where ``natives`` is empty for managed-only packages.
    """
    file_size = os.path.getsize(path)
    natives: dict[str, int] = {}
    try:
        with zipfile.ZipFile(path) as zf:
            for zi in zf.infolist():
                if _is_native_entry(zi.filename):
                    natives[zi.filename] = zi.file_size
    except zipfile.BadZipFile:
        _log(f"  ! not a valid zip, skipping: {path}")
        return {"nupkg": file_size, "natives": {}}
    return {"nupkg": file_size, "natives": natives}


def measure_nupkg_dir(directory: str) -> dict:
    """Measure every ``.nupkg`` found under ``directory`` (recursively).

    Returns ``{packageId: {"version": v, **measurement}}``. When the same
    package id appears more than once the first occurrence wins.
    """
    packages: dict[str, dict] = {}
    for root, _dirs, files in os.walk(directory):
        for fn in sorted(files):
            if not fn.lower().endswith(".nupkg"):
                continue
            split = split_nupkg_name(fn)
            if not split:
                continue
            pkg_id, version = split
            if pkg_id in packages:
                continue
            measurement = measure_nupkg(os.path.join(root, fn))
            measurement["version"] = version
            packages[pkg_id] = measurement
    return packages


# --------------------------------------------------------------------------- #
# Azure DevOps (nightly) helpers
# --------------------------------------------------------------------------- #

def resolve_nightly_build(build_id: int | None) -> dict:
    """Return metadata for the nightly build to sample.

    When ``build_id`` is given it is used directly; otherwise the latest
    succeeded ``main`` build of the pipeline is chosen.
    """
    if build_id is not None:
        data = http_get_json(f"{AZDO_API}/build/builds/{build_id}?api-version=7.1")
        return _build_meta(data)

    url = (
        f"{AZDO_API}/build/builds?api-version=7.1&definitions={AZDO_PIPELINE_ID}"
        "&$top=25&queryOrder=finishTimeDescending"
        "&reasonFilter=individualCI,batchedCI,schedule,manual"
        "&statusFilter=completed&resultFilter=succeeded"
    )
    data = http_get_json(url)
    for build in data.get("value", []):
        if build.get("sourceBranch") == "refs/heads/main":
            return _build_meta(build)
    raise RuntimeError("No succeeded main build found for pipeline 4.")


def _build_meta(build: dict) -> dict:
    version = _clean_build_version(build.get("buildNumber", ""))
    finish = build.get("finishTime") or build.get("queueTime") or ""
    date = finish[:10] if finish else dt.datetime.now(dt.timezone.utc).strftime("%Y-%m-%d")
    return {
        "buildId": int(build["id"]),
        "version": version,
        "date": date,
        "buildNumber": build.get("buildNumber", ""),
    }


def _clean_build_version(build_number: str) -> str:
    """``4.151.0-preview.0.38+main`` -> ``4.151.0-preview.0.38``."""
    return build_number.split("+", 1)[0].strip()


def get_artifact_download_url(build_id: int, artifact_name: str) -> str:
    data = http_get_json(f"{AZDO_API}/build/builds/{build_id}/artifacts?api-version=7.1")
    for artifact in data.get("value", []):
        if artifact.get("name") == artifact_name:
            url = artifact.get("resource", {}).get("downloadUrl")
            if not url:
                raise RuntimeError(f"Artifact '{artifact_name}' has no downloadUrl.")
            return url
    available = ", ".join(a.get("name", "?") for a in data.get("value", []))
    raise RuntimeError(
        f"Artifact '{artifact_name}' not found on build {build_id}. Available: {available}"
    )


def measure_nightly(build_meta: dict, work_dir: str) -> dict[str, dict]:
    """Download + extract the nightly artifact and measure every package."""
    build_id = build_meta["buildId"]
    _log(f"  resolving artifact '{NIGHTLY_ARTIFACT}' on build {build_id} "
         f"({build_meta['buildNumber']})")
    url = get_artifact_download_url(build_id, NIGHTLY_ARTIFACT)

    zip_path = os.path.join(work_dir, f"{NIGHTLY_ARTIFACT}.zip")
    extract_dir = os.path.join(work_dir, NIGHTLY_ARTIFACT)
    _log(f"  downloading artifact (this is ~0.5 GB)...")
    http_download(url, zip_path)
    _log(f"  extracting {os.path.getsize(zip_path) / 1e6:.0f} MB archive...")
    with zipfile.ZipFile(zip_path) as zf:
        zf.extractall(extract_dir)
    os.remove(zip_path)

    packages = measure_nupkg_dir(extract_dir)
    shutil.rmtree(extract_dir, ignore_errors=True)
    _log(f"  measured {len(packages)} packages "
         f"({sum(1 for p in packages.values() if p['natives'])} native)")
    return packages


# --------------------------------------------------------------------------- #
# NuGet.org (released reference) helpers
# --------------------------------------------------------------------------- #

def get_nuget_versions(package_id: str) -> list[str]:
    url = f"{NUGET_FLATCONTAINER}/{package_id.lower()}/index.json"
    try:
        return list(http_get_json(url).get("versions", []))
    except RuntimeError:
        return []


_SEMVER_RE = re.compile(r"^(\d+)\.(\d+)\.(\d+)(?:\.(\d+))?(?:-(.+))?$")


def _semver_key(version: str) -> tuple:
    """Return a sortable key for a NuGet SemVer2 version string.

    Release versions sort after their pre-releases (per SemVer precedence).
    """
    m = _SEMVER_RE.match(version)
    if not m:
        return ((0, 0, 0, 0), (1,))
    major, minor, patch, rev, pre = m.groups()
    core = (int(major), int(minor), int(patch), int(rev or 0))
    if pre is None:
        # A release sorts after all of its pre-releases: flag 1 > flag 0.
        return (core, (1,))
    ids: list[tuple] = []
    for part in pre.split("."):
        if part.isdigit():
            ids.append((0, int(part)))  # numeric identifiers compare numerically
        else:
            ids.append((1, part))       # ... and lower than alphanumeric ones
    # flag 0 marks a pre-release; the nested tuple only ever compares within
    # pre-releases, so its (int, int|str) items never clash across positions.
    return (core, (0, tuple(ids)))


def _is_stable(version: str) -> bool:
    return "-" not in version


def resolve_roles(versions: list[str]) -> dict[str, str | None]:
    """Resolve the four reference roles from a package's version list."""
    roles: dict[str, str | None] = {r: None for r in ROLES}
    if not versions:
        return roles

    ordered = sorted(versions, key=_semver_key)
    roles["latest"] = ordered[-1]

    stables = [v for v in ordered if _is_stable(v)]
    if not stables:
        return roles

    curr = stables[-1]
    roles["curr-stable"] = curr
    curr_major = _semver_key(curr)[0][0]

    same_major = [v for v in stables if _semver_key(v)[0][0] == curr_major]
    if len(same_major) >= 2:
        roles["prev-stable"] = same_major[-2]

    lower_major = [v for v in stables if _semver_key(v)[0][0] < curr_major]
    if lower_major:
        roles["prev-major"] = lower_major[-1]

    return roles


def measure_released_version(package_id: str, version: str, work_dir: str) -> dict | None:
    """Download a single released nupkg from NuGet.org and measure it."""
    lower = package_id.lower()
    url = f"{NUGET_FLATCONTAINER}/{lower}/{version}/{lower}.{version}.nupkg"
    dest = os.path.join(work_dir, f"{lower}.{version}.nupkg")
    try:
        http_download(url, dest)
    except RuntimeError:
        _log(f"    ! could not download {package_id} {version}; skipping")
        return None
    measurement = measure_nupkg(dest)
    measurement["version"] = version
    os.remove(dest)
    return measurement


def collect_released(
    package_ids: list[str],
    released_cache: dict,
    work_dir: str,
    *,
    force: bool,
) -> dict[str, dict[str, str | None]]:
    """Resolve roles for every package and ensure the cache has their sizes.

    Returns ``{role: {packageId: version}}`` describing the resolution used for
    this run. ``released_cache`` is mutated in place, keyed by ``id@version``.
    """
    resolved: dict[str, dict[str, str | None]] = {r: {} for r in ROLES}
    for pkg_id in sorted(package_ids):
        versions = get_nuget_versions(pkg_id)
        if not versions:
            continue
        roles = resolve_roles(versions)
        wanted = {v for v in roles.values() if v}
        for version in sorted(wanted):
            cache_key = f"{pkg_id}@{version}"
            if force or cache_key not in released_cache:
                measurement = measure_released_version(pkg_id, version, work_dir)
                if measurement is None:
                    continue
                released_cache[cache_key] = measurement
        for role, version in roles.items():
            if version and f"{pkg_id}@{version}" in released_cache:
                resolved[role][pkg_id] = version
    return resolved


# --------------------------------------------------------------------------- #
# History document
# --------------------------------------------------------------------------- #

def load_history(path: str) -> dict:
    if os.path.exists(path):
        try:
            with open(path, "r", encoding="utf-8") as fh:
                data = json.load(fh)
            if isinstance(data, dict) and data.get("schema") == SCHEMA_VERSION:
                data.setdefault("nightly", [])
                data.setdefault("releasedCache", {})
                data.setdefault("roles", {})
                return data
            _log(f"  history schema mismatch (found {data.get('schema')}); starting fresh")
        except (json.JSONDecodeError, OSError) as err:
            _log(f"  could not read history ({err}); starting fresh")
    return {"schema": SCHEMA_VERSION, "nightly": [], "releasedCache": {}, "roles": {}}


def save_history(path: str, history: dict) -> None:
    history["schema"] = SCHEMA_VERSION
    history["updatedUtc"] = dt.datetime.now(dt.timezone.utc).strftime("%Y-%m-%dT%H:%M:%SZ")
    tmp = f"{path}.tmp"
    with open(tmp, "w", encoding="utf-8") as fh:
        json.dump(history, fh, indent=2, sort_keys=True)
    os.replace(tmp, path)
    _log(f"  wrote history -> {path} ({os.path.getsize(path) / 1024:.0f} KB)")


def upsert_nightly(history: dict, entry: dict, max_nightly: int) -> None:
    nightly = [n for n in history.get("nightly", []) if n.get("date") != entry["date"]]
    nightly.append(entry)
    nightly.sort(key=lambda n: n["date"])
    history["nightly"] = nightly[-max_nightly:]


# --------------------------------------------------------------------------- #
# Main
# --------------------------------------------------------------------------- #

def parse_args(argv: list[str]) -> argparse.Namespace:
    p = argparse.ArgumentParser(description=__doc__,
                                formatter_class=argparse.RawDescriptionHelpFormatter)
    p.add_argument("--history", default="artifact-sizes.json",
                   help="Path to the JSON history document (read + written).")
    p.add_argument("--build-id", type=int, default=None,
                   help="Sample a specific AzDO build id instead of the latest main build.")
    p.add_argument("--max-nightly", type=int, default=10,
                   help="Number of nightly days to retain (default: 10).")
    p.add_argument("--nightly-only", action="store_true",
                   help="Only collect the nightly data point.")
    p.add_argument("--released-only", action="store_true",
                   help="Only (re)collect released reference versions.")
    p.add_argument("--force", action="store_true",
                   help="Re-measure even when data is already cached.")
    p.add_argument("--work-dir", default=None,
                   help="Directory for temporary downloads (default: a temp dir).")
    return p.parse_args(argv)


def main(argv: list[str]) -> int:
    args = parse_args(argv)
    history = load_history(args.history)

    owns_work_dir = args.work_dir is None
    work_dir = args.work_dir or tempfile.mkdtemp(prefix="artifact-sizes-")
    os.makedirs(work_dir, exist_ok=True)

    try:
        # ---- Nightly -----------------------------------------------------
        if not args.released_only:
            _log("Collecting nightly build...")
            build_meta = resolve_nightly_build(args.build_id)
            existing = next((n for n in history.get("nightly", [])
                             if n.get("date") == build_meta["date"]), None)
            if existing and existing.get("buildId") == build_meta["buildId"] and not args.force:
                _log(f"  already have build {build_meta['buildId']} "
                     f"for {build_meta['date']}; skipping download")
            else:
                packages = measure_nightly(build_meta, work_dir)
                if packages:
                    upsert_nightly(history, {
                        "date": build_meta["date"],
                        "buildId": build_meta["buildId"],
                        "version": build_meta["version"],
                        "packages": packages,
                    }, args.max_nightly)

        # ---- Released reference versions ---------------------------------
        if not args.nightly_only:
            _log("Collecting released reference versions...")
            package_ids = _known_package_ids(history)
            if not package_ids:
                _log("  no known package ids yet; run nightly collection first")
            else:
                resolved = collect_released(
                    package_ids, history["releasedCache"], work_dir, force=args.force)
                history["roles"] = resolved
                total = len(history["releasedCache"])
                _log(f"  released cache now holds {total} package/version entries")

        save_history(args.history, history)
    finally:
        if owns_work_dir:
            shutil.rmtree(work_dir, ignore_errors=True)

    return 0


def _known_package_ids(history: dict) -> list[str]:
    """Package universe = every id ever seen in a nightly entry."""
    ids: set[str] = set()
    for entry in history.get("nightly", []):
        ids.update(entry.get("packages", {}).keys())
    return sorted(ids)


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
