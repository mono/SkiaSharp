#!/usr/bin/env python3
"""Resolve the newest SkiaSharp ``-nightly.*`` version from the EAP feed and pin it.

Why this exists
---------------
The tracking benchmark project floats its SkiaSharp version as ``*-*`` for local
development. In CI that is unsafe: the three OS matrix legs would each restore
independently and could pick *different* nightlies (a new one may publish mid-run),
which would make the cross-OS comparison meaningless. Passing ``-p:...`` to
``dotnet run`` does not help either, because BenchmarkDotNet rebuilds the project
in its own toolchain and would re-resolve ``*-*``.

So CI resolves the version **once** (``--github-output``) and every leg writes that
exact version to ``version.props`` (``--pin <version> --write-props ...``), which the
csproj imports — guaranteeing all legs, and BenchmarkDotNet's rebuild, use the same
pinned nightly.

Only the Python standard library is used.
"""

from __future__ import annotations

import argparse
import json
import re
import sys
import time
import urllib.error
import urllib.request

EAP_INDEX_URL = "https://aka.ms/skiasharp-eap/index.json"
# NuGet.org flat container — used ONLY to read the released version *list* over HTTP
# (like track-artifact-sizes.py). The packages themselves are restored from the
# allowed build feeds; nuget.org is never placed in a committed nuget.config.
NUGET_FLATCONTAINER = "https://api.nuget.org/v3-flatcontainer"
USER_AGENT = "skiasharp-benchmark-version-resolver/1.0 (+https://github.com/mono/SkiaSharp)"

NIGHTLY_RE = re.compile(r"^(\d+)\.(\d+)\.(\d+)-nightly\.(\d+)$")

# The released reference roles (subset of track-artifact-sizes.py's roles). "latest"
# is covered by the nightly for a perf tracker, so we keep the three released stables.
RELEASED_ROLES = ("curr-stable", "prev-stable", "prev-major")


def _log(msg: str) -> None:
    print(msg, file=sys.stderr, flush=True)


def http_get_json(url: str, *, retries: int = 4, timeout: int = 60) -> dict:
    last_err: Exception | None = None
    for attempt in range(1, retries + 1):
        try:
            req = urllib.request.Request(url, headers={"User-Agent": USER_AGENT})
            with urllib.request.urlopen(req, timeout=timeout) as resp:
                return json.loads(resp.read())
        except (urllib.error.URLError, TimeoutError, ConnectionError, json.JSONDecodeError) as err:
            last_err = err
            wait = min(30, 2 ** attempt)
            _log(f"  ! request failed ({err}); retry {attempt}/{retries} in {wait}s")
            time.sleep(wait)
    raise RuntimeError(f"GET failed after {retries} attempts: {url}\n  {last_err}")


def package_base_address(index_url: str) -> str:
    """Resolve the flat-container (PackageBaseAddress) base from a v3 service index."""
    index = http_get_json(index_url)
    for resource in index.get("resources", []):
        if str(resource.get("@type", "")).startswith("PackageBaseAddress/3.0.0"):
            base = resource["@id"]
            return base if base.endswith("/") else base + "/"
    raise RuntimeError(f"No PackageBaseAddress/3.0.0 resource in service index: {index_url}")


def resolve_latest_nightly(package: str, index_url: str) -> str:
    base = package_base_address(index_url)
    versions_url = f"{base}{package.lower()}/index.json"
    versions = http_get_json(versions_url).get("versions", [])

    nightlies = []
    for v in versions:
        m = NIGHTLY_RE.match(v)
        if m:
            nightlies.append((tuple(int(x) for x in m.groups()), v))
    if not nightlies:
        raise RuntimeError(f"No '-nightly.*' versions found for {package} at {versions_url}")

    nightlies.sort()
    latest = nightlies[-1][1]
    _log(f"  resolved {package} nightly: {latest} (from {len(versions)} version(s))")
    return latest


# --------------------------------------------------------------------------- #
# Released reference roles (version list read from nuget.org over HTTP)
# --------------------------------------------------------------------------- #

def get_nuget_versions(package: str) -> list[str]:
    url = f"{NUGET_FLATCONTAINER}/{package.lower()}/index.json"
    return http_get_json(url).get("versions", [])


def _is_stable(version: str) -> bool:
    return "-" not in version


def _semver_key(version: str):
    core = version.split("-", 1)[0]
    parts = [int(p) for p in re.findall(r"\d+", core)] or [0]
    return tuple(parts)


def resolve_released_roles(package: str) -> dict[str, str | None]:
    """Pick curr-stable / prev-stable / prev-major from nuget.org, like the size tracker."""
    versions = get_nuget_versions(package)
    stables = sorted((v for v in versions if _is_stable(v)), key=_semver_key)
    roles: dict[str, str | None] = {r: None for r in RELEASED_ROLES}
    if not stables:
        return roles

    curr = stables[-1]
    roles["curr-stable"] = curr
    curr_major = _semver_key(curr)[0]

    same_major = [v for v in stables if _semver_key(v)[0] == curr_major]
    if len(same_major) >= 2:
        roles["prev-stable"] = same_major[-2]

    lower_major = [v for v in stables if _semver_key(v)[0] < curr_major]
    if lower_major:
        roles["prev-major"] = lower_major[-1]

    _log(f"  resolved {package} released roles: {roles}")
    return roles


def resolve_all(package: str, index_url: str) -> dict[str, str]:
    """The full role->version map benchmarked per OS: nightly + released stables."""
    result: dict[str, str] = {"nightly": resolve_latest_nightly(package, index_url)}
    for role, ver in resolve_released_roles(package).items():
        if ver:
            result[role] = ver
    return result


def write_props(path: str, version: str) -> None:
    # Bracketed exact-version constraint so a newer nightly publishing mid-run
    # cannot roll the pin up (a bare version is a `>=` minimum, not exact).
    # SkiaSharpResolvedVersion is the CORE version (System.Version-parseable, no
    # prerelease suffix) used by the csproj to pick API-level #if defines.
    core = version.split("-", 1)[0]
    content = (
        "<Project>\n"
        "  <!-- Generated by resolve-skiasharp-version.py; do not commit. Pins one\n"
        "       nightly across all CI legs so the cross-OS comparison is valid, and\n"
        "       sets the core version used to select API-level #if defines. -->\n"
        "  <PropertyGroup>\n"
        f"    <SkiaSharpTrackingVersion>[{version}]</SkiaSharpTrackingVersion>\n"
        f"    <SkiaSharpResolvedVersion>{core}</SkiaSharpResolvedVersion>\n"
        "  </PropertyGroup>\n"
        "</Project>\n"
    )
    with open(path, "w", encoding="utf-8") as fh:
        fh.write(content)
    _log(f"  wrote {path} (SkiaSharpTrackingVersion={version})")


def parse_args(argv: list[str]) -> argparse.Namespace:
    p = argparse.ArgumentParser(description=__doc__,
                                formatter_class=argparse.RawDescriptionHelpFormatter)
    p.add_argument("--package", default="SkiaSharp", help="Package id to resolve.")
    p.add_argument("--index-url", default=EAP_INDEX_URL, help="v3 feed service index URL.")
    p.add_argument("--pin", default=None,
                   help="Use this exact version instead of resolving from the feed.")
    p.add_argument("--write-props", default=None,
                   help="Write an MSBuild props file setting SkiaSharpTrackingVersion here.")
    p.add_argument("--github-output", action="store_true",
                   help="Append outputs to the $GITHUB_OUTPUT file.")
    p.add_argument("--roles", action="store_true",
                   help="Resolve the full role->version map (nightly + released stables) "
                        "and print it as JSON. With --github-output, emit one output per role "
                        "(role name with '-' replaced by '_').")
    return p.parse_args(argv)


def _emit_github_output(pairs: dict[str, str]) -> None:
    import os
    target = os.environ.get("GITHUB_OUTPUT")
    if not target:
        return
    with open(target, "a", encoding="utf-8") as fh:
        for key, value in pairs.items():
            fh.write(f"{key}={value}\n")


def main(argv: list[str]) -> int:
    args = parse_args(argv)

    if args.roles:
        roles = resolve_all(args.package, args.index_url)
        if args.github_output:
            _emit_github_output({r.replace("-", "_"): v for r, v in roles.items()})
        print(json.dumps(roles))
        return 0

    version = args.pin or resolve_latest_nightly(args.package, args.index_url)

    if args.write_props:
        write_props(args.write_props, version)

    if args.github_output:
        _emit_github_output({"version": version})

    # The resolved version on stdout (stderr carries the logs).
    print(version)
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
