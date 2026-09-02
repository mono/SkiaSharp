#!/usr/bin/env python3
"""Find the newest successful package build for each open PR.

This is the *targeted intake* for the PR mode of ``track-artifact-sizes``.

Historically the workflow subscribed to ``check_run: [completed]`` so it could react the
moment the Azure DevOps ``Package NuGets`` job finished. GitHub offers no server-side
filter for ``check_run`` (no app, name, or path filters), so that subscription started a
runner for **every** completed check run on the default branch — including the check runs
our own Actions workflows publish. Because ``persist-aw-data`` runs on
``workflow_run: [completed]`` of ``track-artifact-sizes`` and then publishes its own check
run, the two workflows formed a self-sustaining 1:1 loop that produced tens of thousands of
runs. See the PR that introduced this file for the measurements.

Polling Azure DevOps directly is bounded (one cheap REST call per open PR, on a cron) and
targeted (it asks for exactly the pipeline and result we care about), so it replaces the
event subscription without giving up the behaviour.

Output is a GitHub Actions matrix payload::

    [{"pr": 4912, "build": "1576555"}]

Only builds that are ``completed`` + ``succeeded`` on ``refs/pull/<n>/merge`` are returned,
and any PR whose size comment already records that exact build id is dropped so a rerun
never re-downloads ~1 GB of packages for a result that is already published.
"""

from __future__ import annotations

import argparse
import json
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))  # perf/
from _common import http_get_json, log  # noqa: E402

DEFAULT_ORG = "dnceng-public"
DEFAULT_PROJECT = "public"
# The public SkiaSharp pipeline; its `Package NuGets` job publishes the `nuget` artifact
# that measure_pr.py consumes.
DEFAULT_DEFINITION = 345


def latest_successful_build(
    pr_number: int,
    *,
    org: str = DEFAULT_ORG,
    project: str = DEFAULT_PROJECT,
    definition: int = DEFAULT_DEFINITION,
    fetch=None,
    errors: list | None = None,
) -> str | None:
    """Return the newest completed+succeeded build id for a PR, or None."""
    fetch = fetch or http_get_json
    url = (
        f"https://dev.azure.com/{org}/{project}/_apis/build/builds"
        f"?definitions={definition}"
        f"&branchName=refs/pull/{pr_number}/merge"
        "&statusFilter=completed&resultFilter=succeeded"
        "&queryOrder=finishTimeDescending&$top=1&api-version=7.1"
    )
    try:
        data = fetch(url)
    except Exception as err:  # noqa: BLE001 - a single unreachable PR must not fail the sweep
        log(f"  ! PR #{pr_number}: could not query builds: {err}")
        if errors is not None:
            errors.append(pr_number)
        return None
    builds = data.get("value") or []
    if not builds:
        return None
    return str(builds[0]["id"])


def build_matrix(
    pr_numbers: list[int],
    measured: dict[int, str] | None = None,
    **kwargs,
) -> list[dict]:
    """Return the matrix entries for PRs with a new, not-yet-measured build."""
    measured = measured or {}
    entries: list[dict] = []
    errors: list[int] = []
    for pr_number in pr_numbers:
        build_id = latest_successful_build(pr_number, errors=errors, **kwargs)
        if not build_id:
            if pr_number not in errors:
                log(f"  - PR #{pr_number}: no successful package build")
            continue
        if measured.get(pr_number) == build_id:
            log(f"  - PR #{pr_number}: build {build_id} already measured")
            continue
        log(f"  + PR #{pr_number}: build {build_id}")
        entries.append({"pr": pr_number, "build": build_id})
    # A widespread query failure (throttling, an AzDO outage) otherwise looks exactly like
    # "nobody has a build" and would silently stop PR size reports with no signal anywhere.
    if errors and len(errors) >= max(3, len(pr_numbers) // 2):
        log(f"::warning::{len(errors)} of {len(pr_numbers)} PR build queries failed; "
            "Azure DevOps may be throttling or unavailable")
    return entries


def parse_measured(pairs: list[str]) -> dict[int, str]:
    """Parse ``--measured 4912=1576555`` pairs into a mapping."""
    measured: dict[int, str] = {}
    for pair in pairs:
        pr_text, _, build_id = pair.partition("=")
        if not build_id:
            raise ValueError(f"--measured expects <pr>=<build>, got {pair!r}")
        measured[int(pr_text)] = build_id
    return measured


def parse_args(argv: list[str]) -> argparse.Namespace:
    p = argparse.ArgumentParser(description=__doc__,
                                formatter_class=argparse.RawDescriptionHelpFormatter)
    p.add_argument("--pr", type=int, action="append", default=[],
                   help="Open PR number to check (repeatable).")
    p.add_argument("--measured", action="append", default=[],
                   help="Already-measured <pr>=<build> pair (repeatable); such PRs are skipped.")
    p.add_argument("--org", default=DEFAULT_ORG, help=f"AzDO org (default: {DEFAULT_ORG}).")
    p.add_argument("--project", default=DEFAULT_PROJECT,
                   help=f"AzDO project (default: {DEFAULT_PROJECT}).")
    p.add_argument("--definition", type=int, default=DEFAULT_DEFINITION,
                   help=f"AzDO pipeline definition id (default: {DEFAULT_DEFINITION}).")
    p.add_argument("--limit", type=int, default=10,
                   help="Maximum matrix entries to emit (default: 10).")
    p.add_argument("--output", default=None,
                   help="Write the matrix JSON here (default: stdout).")
    return p.parse_args(argv)


def main(argv: list[str]) -> int:
    args = parse_args(argv)
    entries = build_matrix(
        args.pr,
        parse_measured(args.measured),
        org=args.org,
        project=args.project,
        definition=args.definition,
    )
    if args.limit >= 0:
        entries = entries[: args.limit]
    payload = json.dumps(entries)
    if args.output:
        with open(args.output, "w", encoding="utf-8") as fh:
            fh.write(payload)
    else:
        print(payload)
    log(f"{len(entries)} PR build(s) to measure")
    return 0


if __name__ == "__main__":
    raise SystemExit(main(sys.argv[1:]))
