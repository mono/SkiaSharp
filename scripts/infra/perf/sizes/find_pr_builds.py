#!/usr/bin/env python3
"""Find PR package builds whose artifact sizes have not been reported yet.

Emits a GitHub Actions matrix: [{"pr": 4913, "build": "1577884", "packagedAt": "..."}]

This replaced an `on: check_run` subscription that ran away to 40,000 runs; see the header of
.github/workflows/track-artifact-sizes.yml for why it must never come back.
"""
from __future__ import annotations

import argparse
import datetime
import json
import os
import re
import sys

sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))  # perf/
from _common import http_get_json, log, parse_iso_utc  # noqa: E402
from pr_comment import STAMP_FORMAT, read_stamp  # noqa: E402

ORG, PROJECT, DEFINITION = "dnceng-public", "public", 345
PACKAGE_STAGE = "Package NuGets"  # finishes ~1h30m before the build, so reports land sooner
AZDO = f"https://dev.azure.com/{ORG}/{PROJECT}/_apis"



def newest_build_per_pr(hours: int, fetch=http_get_json) -> dict[int, dict]:
    """One query covering every open PR, so this does not scale with how many are open.

    Older builds are superseded and deleted by Azure DevOps, so only the newest can matter.
    """
    since = (datetime.datetime.now(datetime.timezone.utc)
             - datetime.timedelta(hours=hours)).strftime("%Y-%m-%dT%H:%M:%SZ")
    data = fetch(f"{AZDO}/build/builds?definitions={DEFINITION}&minTime={since}&$top=100"
                 "&queryOrder=finishTimeDescending&api-version=7.1")
    newest: dict[int, dict] = {}
    for build in data.get("value") or []:
        match = re.search(r"refs/pull/(\d+)/merge", build.get("sourceBranch") or "")
        if match:
            pr = int(match.group(1))
            if pr not in newest or build["id"] > newest[pr]["id"]:
                newest[pr] = build
    return newest


def packaged_at(build_id, fetch=http_get_json) -> str | None:
    """When this build's packages were produced, or None if they are not ready."""
    try:
        data = fetch(f"{AZDO}/build/builds/{build_id}/timeline?api-version=7.1")
    except Exception as err:  # noqa: BLE001 - one bad timeline must not stop the sweep
        log(f"  ! build {build_id}: {err}")
        return None
    for record in data.get("records") or []:
        if record.get("type") == "Stage" and record.get("name") == PACKAGE_STAGE:
            if record.get("state") == "completed" and record.get("result") == "succeeded":
                return record.get("finishTime")
    return None


def select(repo: str, hours: int, *, builds=None, stage=None, reported=None) -> list[dict]:
    """Builds worth reporting, oldest first so a backlog drains in order.

    The comparison is ordered rather than an equality test, so a re-run reports itself simply
    by packaging later and a stale out-of-order result is ignored. No cap: the worst 2h window
    measured over 250 hours held 7 PRs, against a matrix limit of 256.
    """
    builds = builds or (lambda: newest_build_per_pr(hours))
    stage = stage or packaged_at
    reported = reported or (lambda pr: read_stamp(repo, pr))

    ready = []
    for pr, build in builds().items():
        # Deliberately not gated on the build's overall result: a build can fail in a later
        # stage having already packaged successfully, and those packages are still worth
        # measuring. `stage()` returns a time only when packaging itself succeeded.
        when = parse_iso_utc(stage(build["id"]))
        if when:
            ready.append((when, pr, build))
    ready.sort(key=lambda item: item[0])

    selected = []
    for when, pr, build in ready:
        try:
            already = reported(pr)
        except Exception as err:  # noqa: BLE001 - unknown, so leave it for the next sweep
            log(f"  ! PR #{pr}: could not read its report, skipping: {err}")
            continue
        if already and when <= already:
            log(f"  - PR #{pr}: build {build['id']} already reported")
            continue
        stamp = when.strftime(STAMP_FORMAT)
        log(f"  + PR #{pr}: build {build['id']} packaged {stamp}")
        selected.append({"pr": pr, "build": str(build["id"]), "packagedAt": stamp})
    return selected


def main(argv: list[str]) -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--repo", required=True, help="owner/name")
    parser.add_argument("--hours", type=int, default=2, help="how far back to look")
    parser.add_argument("--output", help="write the matrix JSON here instead of stdout")
    args = parser.parse_args(argv)

    entries = select(args.repo, args.hours)
    payload = json.dumps(entries)
    if args.output:
        with open(args.output, "w", encoding="utf-8") as fh:
            fh.write(payload)
    else:
        print(payload)
    log(f"{len(entries)} build(s) to measure")
    return 0


if __name__ == "__main__":
    raise SystemExit(main(sys.argv[1:]))
