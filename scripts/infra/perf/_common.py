#!/usr/bin/env python3
"""Shared primitives for the perf trackers (benchmarks + sizes).

Both trackers independently resolve the same version roles from NuGet feeds and both
hand-rolled identical SemVer sorting + HTTP-with-retries helpers. Those live here now so
there is one implementation. Domain-specific things (per-package role resolution in the
size tracker, the time/byte formatters in each Markdown renderer) stay in their own
scripts — this module is just the common version/HTTP layer.

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

# Stable/prerelease reference roles, oldest -> newest software.
RELEASED_ROLES = ("prev-major", "prev-stable", "curr-stable")

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


def latest_nightly(versions: list[str]) -> str | None:
    """Newest ``-nightly.*`` version (by SemVer) from a version list, or None."""
    nightlies = [v for v in versions if "-nightly." in v.lower()]
    return sorted(nightlies, key=semver_key)[-1] if nightlies else None


def released_roles(versions: list[str]) -> dict[str, str]:
    """Resolve the released reference roles present in a version list.

    Returns only the roles that exist (a brand-new major may have no prev-stable /
    prev-major): ``curr-stable`` (newest stable), ``prev-stable`` (2nd-newest in the
    current major), ``prev-major`` (newest stable of a lower major).
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
