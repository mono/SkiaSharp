#!/usr/bin/env python3
"""Resolve the SkiaSharp versions to benchmark and print them as JSON.

Thin CLI wrapper around ``_common.resolve_roles()`` — the real feed/role logic lives in
``_common`` so other tracking can reuse it. Output (stdout, single line), consumed by
track-benchmarks.yml:

    {"nightly": "...", "curr-stable": "...", "prev-stable": "...", "prev-major": "..."}

* nightly     -> newest `-nightly.*` on the SkiaSharp EAP feed.
* curr/prev-* -> released stables from nuget.org's version list (read over plain HTTP;
                 the packages are restored from the build feeds, so nuget.org never
                 appears in a nuget.config).

Logs go to stderr. Takes no args.
"""

from __future__ import annotations

import json
import pathlib
import sys

sys.path.insert(0, str(pathlib.Path(__file__).resolve().parents[1]))  # perf/
from _common import log, resolve_roles  # noqa: E402


def main() -> int:
    roles = resolve_roles("SkiaSharp")
    log(f"  resolved: {roles}")
    print(json.dumps(roles))
    return 0


if __name__ == "__main__":
    sys.exit(main())
