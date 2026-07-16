#!/usr/bin/env python3
"""Select the historical ``-nightly.*`` versions to BACKFILL, dated by publish date.

The daily tracker records one ``nightly`` point per day going forward. To seed the
dashboard's trend with history, this picks the newest ``--count`` nightlies — **one per
calendar day** (the newest nightly published that day) — and stamps each with its real
publish date from the EAP feed's registration index. Output (stdout, single line) is
consumed as a GitHub Actions matrix by ``backfill-benchmarks.yml``:

    [{"version": "4.151.0-nightly.55", "date": "2026-07-15"}, ...]   # oldest -> newest

All feed access reuses ``_common`` (no duplicated feed/SemVer logic); logs go to stderr.
"""

from __future__ import annotations

import argparse
import json
import pathlib
import sys

sys.path.insert(0, str(pathlib.Path(__file__).resolve().parents[1]))  # perf/
from _common import eap_versions, log, published_dates, semver_key  # noqa: E402


def select(count: int, package: str = "SkiaSharp") -> list[dict[str, str]]:
    """Newest ``count`` nightlies, one per calendar day, with their publish dates."""
    dates = published_dates(package)
    nightlies = [v for v in eap_versions(package) if "-nightly." in v.lower() and v in dates]

    # Collapse to one version per calendar day: the newest nightly published that day.
    by_day: dict[str, str] = {}
    for v in sorted(nightlies, key=semver_key):  # ascending -> last write per day wins
        by_day[dates[v][:10]] = v

    chosen_days = sorted(by_day)[-count:] if count > 0 else sorted(by_day)
    return [{"version": by_day[d], "date": d} for d in chosen_days]


def main(argv: list[str]) -> int:
    p = argparse.ArgumentParser(description=__doc__,
                                formatter_class=argparse.RawDescriptionHelpFormatter)
    p.add_argument("--count", type=int, default=19,
                   help="How many recent nightlies (one per calendar day) to backfill. Default 19.")
    p.add_argument("--package", default="SkiaSharp", help="Package to resolve (default SkiaSharp).")
    args = p.parse_args(argv)

    points = select(args.count, args.package)
    if not points:
        log("  no nightly versions with publish dates resolved")
        return 1
    log(f"  selected {len(points)} nightly point(s): {points[0]['date']} .. {points[-1]['date']}")
    print(json.dumps(points))
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
