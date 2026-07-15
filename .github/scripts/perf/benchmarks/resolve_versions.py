#!/usr/bin/env python3
"""Resolve the SkiaSharp versions to benchmark and print them as JSON.

Output (stdout, single line), consumed by track-benchmarks.yml:

    {"nightly": "...", "curr-stable": "...", "prev-stable": "...", "prev-major": "..."}

* nightly     -> newest `-nightly.*` on the SkiaSharp EAP feed.
* curr/prev-* -> released stables picked from nuget.org's version *list* (read over
                 plain HTTP; the packages are restored from the build feeds, so
                 nuget.org never appears in a nuget.config).

Logs go to stderr. Takes no arguments.
"""

from __future__ import annotations

import json
import re
import sys
import time
import urllib.error
import urllib.request

EAP_INDEX_URL = "https://aka.ms/skiasharp-eap/index.json"
NUGET_FLATCONTAINER = "https://api.nuget.org/v3-flatcontainer"
PACKAGE = "SkiaSharp"
USER_AGENT = "skiasharp-benchmark-version-resolver/1.0 (+https://github.com/mono/SkiaSharp)"
NIGHTLY_RE = re.compile(r"^(\d+)\.(\d+)\.(\d+)-nightly\.(\d+)$")


def _log(msg: str) -> None:
    print(msg, file=sys.stderr, flush=True)


def _get_json(url: str, retries: int = 4, timeout: int = 60) -> dict:
    last: Exception | None = None
    for attempt in range(1, retries + 1):
        try:
            req = urllib.request.Request(url, headers={"User-Agent": USER_AGENT})
            with urllib.request.urlopen(req, timeout=timeout) as resp:
                return json.loads(resp.read())
        except (urllib.error.URLError, TimeoutError, ConnectionError, json.JSONDecodeError) as err:
            last = err
            time.sleep(min(30, 2 ** attempt))
    raise RuntimeError(f"GET failed after {retries} attempts: {url}\n  {last}")


def _semver_key(version: str) -> tuple:
    return tuple(int(p) for p in re.findall(r"\d+", version.split("-", 1)[0]) or [0])


def _latest_nightly() -> str:
    index = _get_json(EAP_INDEX_URL)
    base = next((r["@id"] for r in index.get("resources", [])
                 if str(r.get("@type", "")).startswith("PackageBaseAddress/3.0.0")), None)
    if not base:
        raise RuntimeError("No PackageBaseAddress in the EAP service index")
    base = base if base.endswith("/") else base + "/"
    versions = _get_json(f"{base}{PACKAGE.lower()}/index.json").get("versions", [])
    nightlies = [(tuple(int(x) for x in m.groups()), v)
                 for v in versions if (m := NIGHTLY_RE.match(v))]
    if not nightlies:
        raise RuntimeError("No -nightly.* versions on the EAP feed")
    return max(nightlies)[1]


def _released_roles() -> dict[str, str]:
    versions = _get_json(f"{NUGET_FLATCONTAINER}/{PACKAGE.lower()}/index.json").get("versions", [])
    stables = sorted((v for v in versions if "-" not in v), key=_semver_key)
    roles: dict[str, str] = {}
    if stables:
        curr = stables[-1]
        roles["curr-stable"] = curr
        major = _semver_key(curr)[0]
        same_major = [v for v in stables if _semver_key(v)[0] == major]
        if len(same_major) >= 2:
            roles["prev-stable"] = same_major[-2]
        lower_major = [v for v in stables if _semver_key(v)[0] < major]
        if lower_major:
            roles["prev-major"] = lower_major[-1]
    return roles


def main() -> int:
    result = {"nightly": _latest_nightly()}
    result.update(_released_roles())
    _log(f"  resolved: {result}")
    print(json.dumps(result))
    return 0


if __name__ == "__main__":
    sys.exit(main())
