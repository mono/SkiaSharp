#!/usr/bin/env python3
"""Shared primitives for the perf trackers (benchmarks + sizes).

Both trackers independently resolve the same version roles from the same NuGet feeds and
both hand-rolled identical SemVer sorting + HTTP-with-retries helpers. Those live here now
so there is one implementation: the HTTP layer, SemVer sorting, the EAP/nuget feed helpers,
and role resolution (``nightly`` + ``latest`` + released baselines). Domain-specific things
(per-package size resolution, the time/byte Markdown formatters) stay in their own scripts.

Consumers add the ``perf/`` folder to ``sys.path`` and ``import _common`` (they are run
by path from CI, not as a package). Only the standard library is used.
"""

from __future__ import annotations

import json
import re
import sys
import time
import urllib.error
import urllib.request

USER_AGENT = "skiasharp-perf-tracker/1.0 (+https://github.com/mono/SkiaSharp)"

# SkiaSharp package feeds.
EAP_INDEX_URL = "https://aka.ms/skiasharp-eap/index.json"       # daily -nightly.* builds
NUGET_FLATCONTAINER = "https://api.nuget.org/v3-flatcontainer"  # released stables

_SEMVER_RE = re.compile(r"^(\d+)\.(\d+)\.(\d+)(?:\.(\d+))?(?:-(.+))?$")


def log(msg: str) -> None:
    """Log to stderr so a script whose stdout carries data (e.g. JSON) stays clean."""
    print(msg, file=sys.stderr, flush=True)


# --------------------------------------------------------------------------- #
# HTTP (stdlib only, exponential backoff)
# --------------------------------------------------------------------------- #

def http_get(url: str, *, retries: int = 4, timeout: int = 120) -> bytes:
    """GET a URL returning the raw bytes, retrying transient failures."""
    last: Exception | None = None
    for attempt in range(1, retries + 1):
        try:
            req = urllib.request.Request(url, headers={"User-Agent": USER_AGENT})
            with urllib.request.urlopen(req, timeout=timeout) as resp:
                return resp.read()
        except (urllib.error.URLError, TimeoutError, ConnectionError) as err:
            last = err
            wait = min(30, 2 ** attempt)
            log(f"  ! request failed ({err}); retry {attempt}/{retries} in {wait}s")
            time.sleep(wait)
    raise RuntimeError(f"GET failed after {retries} attempts: {url}\n  {last}")


def http_get_json(url: str, **kwargs) -> dict:
    return json.loads(http_get(url, **kwargs).decode("utf-8"))


# --------------------------------------------------------------------------- #
# SemVer
# --------------------------------------------------------------------------- #

def semver_key(version: str) -> tuple:
    """A sortable key for a NuGet SemVer2 string.

    A release sorts *after* all of its pre-releases (SemVer precedence). The key is
    ``(core, flags)`` where ``core = (major, minor, patch, rev)`` — so ``semver_key(v)[0]``
    is the core tuple and ``semver_key(v)[0][0]`` is the major.
    """
    m = _SEMVER_RE.match(version)
    if not m:
        return ((0, 0, 0, 0), (1,))
    major, minor, patch, rev, pre = m.groups()
    core = (int(major), int(minor), int(patch), int(rev or 0))
    if pre is None:
        return (core, (1,))  # release: flag 1 > flag 0 (pre-release)
    ids: list[tuple] = []
    for part in pre.split("."):
        ids.append((0, int(part)) if part.isdigit() else (1, part))
    return (core, (0, tuple(ids)))


def is_stable(version: str) -> bool:
    return "-" not in version


# --------------------------------------------------------------------------- #
# NuGet feed access (EAP nightly feed + nuget.org)
# --------------------------------------------------------------------------- #

def pick_resource(resources: list[dict], type_prefix: str) -> str | None:
    """First v3 service-index resource whose ``@type`` starts with ``type_prefix``."""
    for res in resources:
        if str(res.get("@type", "")).startswith(type_prefix):
            return res["@id"]
    return None


def feed_versions(flat_base: str, package: str) -> list[str]:
    """Versions of ``package`` from a flat-container (PackageBaseAddress) base URL.

    Order is not guaranteed (Azure DevOps feeds aren't sorted), so callers sort with
    ``semver_key`` when order matters. Returns ``[]`` if the package/feed is absent.
    """
    url = f"{flat_base.rstrip('/')}/{package.lower()}/index.json"
    try:
        return list(http_get_json(url).get("versions", []))
    except RuntimeError:
        return []


def eap_versions(package: str = "SkiaSharp") -> list[str]:
    """All versions of ``package`` on the SkiaSharp EAP feed (hosts the -nightly.* builds)."""
    resources = http_get_json(EAP_INDEX_URL).get("resources", [])
    flat = pick_resource(resources, "PackageBaseAddress/")
    if not flat:
        raise RuntimeError("No PackageBaseAddress resource in the EAP service index")
    return feed_versions(flat, package)


def nuget_versions(package: str = "SkiaSharp") -> list[str]:
    """All versions of ``package`` on nuget.org (flat container)."""
    return feed_versions(NUGET_FLATCONTAINER, package)


# --------------------------------------------------------------------------- #
# Version-role resolution
#
# Roles split into RELEASED (published to nuget.org) and UNRELEASED (never shipped):
#   released:    latest (newest overall, may be a preview/rc) + curr-stable +
#                prev-stable + prev-major
#   unreleased:  nightly (EAP/CI daily build)  and  pr (a branch source build)
# `latest` is a preview so it is less vital than the stables (which can still be patched),
# but it is a *shipped* release we must not regress. Only `pr` is ephemeral / per-branch
# (dropped from persisted history); `nightly` is unreleased but tracked as the CI trend.
# --------------------------------------------------------------------------- #

def latest_nightly(versions: list[str]) -> str | None:
    """Newest ``-nightly.*`` version (by SemVer) from a version list, or None."""
    nightlies = [v for v in versions if "-nightly." in v.lower()]
    return sorted(nightlies, key=semver_key)[-1] if nightlies else None


def stable_roles(versions: list[str]) -> dict[str, str]:
    """The *stable* released reference roles present in a version list.

    Returns only the roles that exist (a brand-new major may have no prev-stable /
    prev-major): ``curr-stable`` (newest stable), ``prev-stable`` (2nd-newest in the
    current major), ``prev-major`` (newest stable of a lower major). Prereleases are
    excluded — for all released versions incl. the newest preview/rc use ``released_roles``.
    """
    stables = sorted((v for v in versions if is_stable(v)), key=semver_key)
    roles: dict[str, str] = {}
    if not stables:
        return roles
    curr = stables[-1]
    roles["curr-stable"] = curr
    curr_major = semver_key(curr)[0][0]
    same_major = [v for v in stables if semver_key(v)[0][0] == curr_major]
    if len(same_major) >= 2:
        roles["prev-stable"] = same_major[-2]
    lower_major = [v for v in stables if semver_key(v)[0][0] < curr_major]
    if lower_major:
        roles["prev-major"] = lower_major[-1]
    return roles


def released_roles(versions: list[str]) -> dict[str, str]:
    """All *released* roles from a package's nuget.org version list.

    Everything on nuget.org counts as released: ``latest`` (the newest version overall,
    which may be a preview/rc) plus the stable baselines from ``stable_roles``
    (``curr-stable`` / ``prev-stable`` / ``prev-major``). Only ``nightly`` (EAP/CI) and the
    per-branch ``pr`` build are unreleased. Only the roles that actually exist are returned.
    """
    if not versions:
        return {}
    roles = {"latest": sorted(versions, key=semver_key)[-1]}
    roles.update(stable_roles(versions))
    return roles


def resolve_roles(package: str = "SkiaSharp") -> dict[str, str]:
    """Resolve the version roles to track for ``package``.

    ``nightly`` is the newest ``-nightly.*`` on the EAP/CI feed (unreleased); the released
    roles come from nuget.org via ``released_roles`` — ``latest`` (newest, incl. preview/rc)
    plus ``curr-stable`` / ``prev-stable`` / ``prev-major``. Only roles that exist are
    returned (a brand-new major may lack prev-*).
    """
    nightly = latest_nightly(eap_versions(package))
    if not nightly:
        raise RuntimeError("No -nightly.* versions on the EAP feed")
    roles = {"nightly": nightly}
    roles.update(released_roles(nuget_versions(package)))
    return roles
