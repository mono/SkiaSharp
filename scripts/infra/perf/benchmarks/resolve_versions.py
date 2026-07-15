#!/usr/bin/env python3
"""Resolve the SkiaSharp versions to benchmark and print them as JSON.

Output (stdout, single line), consumed by track-benchmarks.yml:

    {"nightly": "...", "curr-stable": "...", "prev-stable": "...", "prev-major": "..."}

* nightly     -> newest `-nightly.*` on the SkiaSharp EAP feed.
* curr/prev-* -> released stables picked from nuget.org's version *list* (read over
                 plain HTTP; the packages are restored from the build feeds, so
                 nuget.org never appears in a nuget.config).

Version/HTTP primitives are shared via ``_common``. Logs go to stderr. Takes no args.
"""

from __future__ import annotations

import json
import pathlib
import sys

sys.path.insert(0, str(pathlib.Path(__file__).resolve().parents[1]))  # perf/
from _common import http_get_json, latest_nightly, log, released_roles  # noqa: E402

EAP_INDEX_URL = "https://aka.ms/skiasharp-eap/index.json"
NUGET_FLATCONTAINER = "https://api.nuget.org/v3-flatcontainer"
PACKAGE = "SkiaSharp"


def _eap_versions() -> list[str]:
    index = http_get_json(EAP_INDEX_URL)
    base = next((r["@id"] for r in index.get("resources", [])
                 if str(r.get("@type", "")).startswith("PackageBaseAddress/3.0.0")), None)
    if not base:
        raise RuntimeError("No PackageBaseAddress in the EAP service index")
    base = base if base.endswith("/") else base + "/"
    return http_get_json(f"{base}{PACKAGE.lower()}/index.json").get("versions", [])


def _nuget_versions() -> list[str]:
    return http_get_json(f"{NUGET_FLATCONTAINER}/{PACKAGE.lower()}/index.json").get("versions", [])


def main() -> int:
    nightly = latest_nightly(_eap_versions())
    if not nightly:
        raise RuntimeError("No -nightly.* versions on the EAP feed")
    result = {"nightly": nightly}
    result.update(released_roles(_nuget_versions()))
    log(f"  resolved: {result}")
    print(json.dumps(result))
    return 0


if __name__ == "__main__":
    sys.exit(main())
