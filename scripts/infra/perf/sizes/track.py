#!/usr/bin/env python3
"""Collect SkiaSharp/HarfBuzzSharp NuGet package + native binary sizes.

This script builds/updates a JSON "history" document that records:

  * the top-level ``.nupkg`` size of every package produced by the build, and
  * for packages that ship native binaries, the size of each individual native
    payload (per os/arch), with symbol files (``.pdb``) excluded.

Two kinds of data points ("columns") are collected:

  * ``nightly`` -- the latest ``-nightly.*`` build from the official SkiaSharp
    Early Access Preview feed (https://aka.ms/skiasharp-eap/index.json). Every
    package is measured at its family's headline nightly version (SkiaSharp and
    HarfBuzzSharp version independently), keyed by the observation date. The
    newest ``--max-nightly`` days are kept.
  * ``released`` -- reference versions resolved from NuGet.org. For every package
    four roles are resolved from that package's *own* version line
    (SkiaSharp and HarfBuzzSharp version independently):
        prev-major  -> newest stable of the previous major line
        prev-stable -> second-newest stable in the current major
        curr-stable -> newest stable
        latest      -> newest overall (including preview / rc)
    Released package sizes are immutable, so they are measured once and cached.

Only the Python standard library is used so the script runs on a clean runner.

The history document is persisted between runs on the ``aw-data`` branch; see
``.github/workflows/track-artifact-sizes.yml``.
"""

from __future__ import annotations

import argparse
import datetime as dt
import json
import os
import shutil
import sys
import tempfile
import time
import urllib.error
import urllib.request
import zipfile

sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))  # perf/
from _common import http_get_json, latest_nightly, released_roles, semver_key  # noqa: E402

# --------------------------------------------------------------------------- #
# Constants
# --------------------------------------------------------------------------- #

SCHEMA_VERSION = 1

# The official SkiaSharp Early Access Preview (EAP) feed. This aka.ms link
# redirects to a standard NuGet v3 feed on Azure DevOps Artifacts (anonymous)
# and hosts the daily ``-nightly.*`` builds we sample.
EAP_INDEX_URL = "https://aka.ms/skiasharp-eap/index.json"

# NuGet.org flat container, used for the released reference versions.
NUGET_FLATCONTAINER = "https://api.nuget.org/v3-flatcontainer"

ROLES = ("prev-major", "prev-stable", "curr-stable", "latest")

USER_AGENT = "skiasharp-artifact-size-tracker/1.0 (+https://github.com/mono/SkiaSharp)"

# Native binary file extensions (managed .dll are filtered out separately).
_NATIVE_EXTS = (".so", ".dylib", ".dll", ".a")


# --------------------------------------------------------------------------- #
# Small HTTP helpers (stdlib only)
# --------------------------------------------------------------------------- #

def _log(msg: str) -> None:
    print(msg, flush=True)


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

    Returns ``{"nupkg": <compressed file size>, "natives": {<path>: <size>},
    "files": {<path>: <uncompressed size>}}``. ``natives`` is the subset the summary
    keeps; ``files`` is EVERY entry, kept only for the raw snapshot (callers that build
    the summary drop it).
    """
    file_size = os.path.getsize(path)
    natives: dict[str, int] = {}
    files: dict[str, int] = {}
    try:
        with zipfile.ZipFile(path) as zf:
            for zi in zf.infolist():
                if zi.is_dir():
                    continue
                files[zi.filename] = zi.file_size
                if _is_native_entry(zi.filename):
                    natives[zi.filename] = zi.file_size
    except zipfile.BadZipFile:
        _log(f"  ! not a valid zip, skipping: {path}")
        return {"nupkg": file_size, "natives": {}, "files": {}}
    return {"nupkg": file_size, "natives": natives, "files": files}


# --------------------------------------------------------------------------- #
# NuGet feed helpers (flat container)
# --------------------------------------------------------------------------- #

def resolve_eap_feed() -> dict:
    """Resolve the EAP feed's service URLs from its v3 index.

    Returns ``{"flat": <PackageBaseAddress>, "search": <SearchQueryService>}``.
    """
    index = http_get_json(EAP_INDEX_URL)
    resources = index.get("resources", [])
    flat = _find_resource(resources, "PackageBaseAddress/")
    search = _find_resource(resources, "SearchQueryService")
    if not flat:
        raise RuntimeError("EAP feed index has no PackageBaseAddress resource.")
    return {"flat": flat, "search": search}


def _find_resource(resources: list[dict], type_prefix: str) -> str | None:
    for res in resources:
        if str(res.get("@type", "")).startswith(type_prefix):
            return res["@id"]
    return None


def get_versions(flat_base: str, package_id: str) -> list[str]:
    """Return the versions of a package from a flat-container base URL.

    The returned list is not guaranteed to be sorted (Azure DevOps feeds are
    not), so callers must sort with ``_semver_key`` when order matters.
    """
    url = f"{flat_base.rstrip('/')}/{package_id.lower()}/index.json"
    try:
        return list(http_get_json(url).get("versions", []))
    except RuntimeError:
        return []


def download_nupkg(flat_base: str, package_id: str, version: str, work_dir: str) -> str | None:
    """Download a single ``.nupkg`` from a flat-container feed to ``work_dir``."""
    lower = package_id.lower()
    url = f"{flat_base.rstrip('/')}/{lower}/{version}/{lower}.{version}.nupkg"
    dest = os.path.join(work_dir, f"{lower}.{version}.nupkg")
    try:
        http_download(url, dest)
    except RuntimeError:
        return None
    return dest


def enumerate_feed_packages(search_base: str) -> list[str]:
    """List SkiaSharp/HarfBuzzSharp package ids published to the feed.

    Internal ``_``-prefixed packages (``_NuGets``, ``_NativeAssets*`` ...) are
    excluded. Returns an empty list if the search service is unavailable.
    """
    if not search_base:
        return []
    url = f"{search_base.rstrip('/')}/?q=&prerelease=true&take=1000&semVerLevel=2.0.0"
    try:
        data = http_get_json(url)
    except RuntimeError:
        return []
    ids = []
    for item in data.get("data", []):
        pid = item.get("id", "")
        if pid.startswith("_"):
            continue
        if pid.startswith("SkiaSharp") or pid.startswith("HarfBuzzSharp"):
            ids.append(pid)
    return sorted(set(ids))


# --------------------------------------------------------------------------- #
# Nightly collection (EAP feed)
# --------------------------------------------------------------------------- #

def _nightly_family(package_id: str) -> str:
    """The version-line a package belongs to."""
    return "harfbuzz" if package_id.startswith("HarfBuzzSharp") else "skia"


def collect_nightly(feed: dict, work_dir: str) -> tuple[dict[str, dict], str | None]:
    """Measure every package at its family's headline nightly version.

    All SkiaSharp-family packages share one nightly version and all
    HarfBuzzSharp-family packages share another. Packages that do not publish
    the headline nightly (e.g. deprecated ones) are skipped, keeping the
    snapshot coherent. Returns ``(packages, skia_headline_version)``.
    """
    flat = feed["flat"]
    package_ids = enumerate_feed_packages(feed["search"])
    if not package_ids:
        raise RuntimeError("Could not enumerate any packages from the EAP feed.")

    headlines = {
        "skia": latest_nightly(get_versions(flat, "SkiaSharp")),
        "harfbuzz": latest_nightly(get_versions(flat, "HarfBuzzSharp")),
    }
    _log(f"  headline nightly: SkiaSharp={headlines['skia']} "
         f"HarfBuzzSharp={headlines['harfbuzz']}")

    packages: dict[str, dict] = {}
    for pkg_id in package_ids:
        target = headlines[_nightly_family(pkg_id)]
        if not target:
            continue
        versions = get_versions(flat, pkg_id)
        if target not in versions:
            continue  # package does not ship this nightly (deprecated / not built)
        dest = download_nupkg(flat, pkg_id, target, work_dir)
        if dest is None:
            _log(f"    ! could not download {pkg_id} {target}; skipping")
            continue
        measurement = measure_nupkg(dest)
        measurement["version"] = target
        os.remove(dest)
        packages[pkg_id] = measurement

    _log(f"  measured {len(packages)} packages "
         f"({sum(1 for p in packages.values() if p['natives'])} native)")
    return packages, headlines["skia"]


# --------------------------------------------------------------------------- #
# Released reference helpers (NuGet.org)
# --------------------------------------------------------------------------- #

def get_nuget_versions(package_id: str) -> list[str]:
    return get_versions(NUGET_FLATCONTAINER, package_id)


def resolve_roles(versions: list[str]) -> dict[str, str | None]:
    """Resolve the four reference roles from a package's version list.

    ``latest`` is the newest version overall (incl. prereleases); the released stable
    roles come from the shared ``released_roles``. Absent roles are ``None``.
    """
    roles: dict[str, str | None] = {r: None for r in ROLES}
    if not versions:
        return roles
    roles["latest"] = sorted(versions, key=semver_key)[-1]
    roles.update(released_roles(versions))
    return roles


def measure_released_version(package_id: str, version: str, work_dir: str) -> dict | None:
    """Download a single released nupkg from NuGet.org and measure it."""
    dest = download_nupkg(NUGET_FLATCONTAINER, package_id, version, work_dir)
    if dest is None:
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
    p.add_argument("--date", default=None,
                   help="Observation date for the nightly entry (YYYY-MM-DD; default: today UTC).")
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
    p.add_argument("--raw", default=None,
                   help="Also write today's nightly measurement, WITH a full per-file size "
                        "breakdown per package, to this path (the workflow archives it under "
                        "sizes/raw/<date>.json.gz). The summary keeps only nupkg + natives.")
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
            _log("Collecting nightly from the EAP feed...")
            today = args.date or dt.datetime.now(dt.timezone.utc).strftime("%Y-%m-%d")
            feed = resolve_eap_feed()
            packages, headline = collect_nightly(feed, work_dir)
            if packages:
                # Raw snapshot (optional): full per-file breakdown, archived per day.
                if args.raw:
                    raw_pkgs = {pid: {"version": m["version"], "nupkg": m["nupkg"],
                                      "files": m.get("files", {})}
                                for pid, m in packages.items()}
                    with open(args.raw, "w", encoding="utf-8") as fh:
                        json.dump({"date": today, "version": headline, "packages": raw_pkgs}, fh)
                    _log(f"  wrote raw size snapshot -> {args.raw} "
                         f"({sum(len(p['files']) for p in raw_pkgs.values())} files)")
                # Summary keeps only nupkg + natives (drop the full file list).
                summary_pkgs = {pid: {"version": m["version"], "nupkg": m["nupkg"],
                                      "natives": m.get("natives", {})}
                                for pid, m in packages.items()}
                upsert_nightly(history, {
                    "date": today,
                    "version": headline,
                    "packages": summary_pkgs,
                }, args.max_nightly)
            else:
                _log("  no nightly packages measured; leaving history unchanged")

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

        # Keep the summary lean: released measurements gain a 'files' map from
        # measure_nupkg, but the summary only needs nupkg + natives (raw lives elsewhere).
        for m in history.get("releasedCache", {}).values():
            if isinstance(m, dict):
                m.pop("files", None)

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
