#!/usr/bin/env python3
"""Find PR package builds that still need an artifact-size report.

This is the intake for the PR mode of ``track-artifact-sizes``.

Why it is shaped like this
--------------------------
The workflow used to subscribe to ``check_run: [completed]``. GitHub offers no server-side filter for that event (no app, name, or
path filter), so every completed check run on the default branch started a runner and filtering happened only after the job began.
Because ``persist-aw-data`` runs on ``workflow_run`` of this workflow and then publishes its own check run, the pair formed a
self-sustaining loop — both workflows reached 40,000 runs, and a sample of 25 consecutive runs showed 24 doing no work at all. A
job-level ``if:`` cannot fix that: a *skipped* job still publishes a check run.

So discovery is a poll instead. The important part is what it polls. Azure DevOps already maintains the queue we need, so one
anonymous request returns every recent PR build — O(1) in the number of open pull requests, not O(open PRs):

    builds?definitions=345&minTime=<now-2h>&queryOrder=finishTimeDescending

The sweep runs hourly and looks back 2 hours, so an entire sweep can be missed without losing a report. Whether a build is actually
reported is decided by a durable stamp in the PR comment rather than by the clock, so a duplicate sweep is a no-op and a delayed one
still catches up.

There is no per-sweep cap. Over 250 hours of real traffic the worst 2-hour window held 7 distinct pull requests, against a GitHub
matrix limit of 256, so capping would only add a silent-deferral path that never earns its keep.

Only the newest build per PR is considered: older builds are superseded, and Azure DevOps deletes them, so they neither need nor
deserve a report.
"""

from __future__ import annotations

import argparse
import datetime
import json
import os
import re
import sys

sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))  # perf/
from _common import http_get_json, log  # noqa: E402

DEFAULT_ORG = "dnceng-public"
DEFAULT_PROJECT = "public"
# The public SkiaSharp pipeline; its `Package NuGets` stage publishes the `nuget` artifact.
DEFAULT_DEFINITION = 345
# The stage that produces the packages. It finishes well before the build does (measured
# ~1h30m earlier on a real build), so watching it reports sooner than waiting for the build.
PACKAGE_STAGE = "Package NuGets"

GITHUB_API = "https://api.github.com"
COMMENT_MARKER = "<!-- skiasharp-pr-artifact-sizes"
_BUILD_STAMP_RE = re.compile(r"<!--\s*build=([0-9]+(?:\.[0-9]+)?)\s*-->")
_PR_BRANCH_RE = re.compile(r"refs/pull/(\d+)/merge", re.IGNORECASE)


class GitHubReadError(RuntimeError):
    """A PR's existing size comment could not be read.

    Callers must treat this as *unknown*, never as *unreported*: measuring downloads roughly a gigabyte, so guessing
    "not yet measured" on a throttled read turns a transient API failure into a repeated multi-gigabyte download.
    """


# --------------------------------------------------------------------------- #
# Azure DevOps — the queue
# --------------------------------------------------------------------------- #

def _api(org: str, project: str) -> str:
    return f"https://dev.azure.com/{org}/{project}/_apis"


def recent_pr_builds(
    *,
    org: str = DEFAULT_ORG,
    project: str = DEFAULT_PROJECT,
    definition: int = DEFAULT_DEFINITION,
    hours: int = 24,
    top: int = 100,
    fetch=None,
) -> dict[int, dict]:
    """Return the NEWEST build per PR from one query.

    One request covers every open PR, so this does not grow with the number of open pull requests. Superseded builds are
    ignored because only the newest can still be relevant.
    """
    fetch = fetch or http_get_json
    since = (datetime.datetime.now(datetime.timezone.utc) - datetime.timedelta(hours=hours)).strftime("%Y-%m-%dT%H:%M:%SZ")
    url = f"{_api(org, project)}/build/builds?definitions={definition}&minTime={since}&$top={top}&queryOrder=finishTimeDescending&api-version=7.1"
    data = fetch(url)

    newest: dict[int, dict] = {}
    for build in data.get("value") or []:
        match = _PR_BRANCH_RE.search(build.get("sourceBranch") or "")
        if not match:
            continue  # a branch or main build, not a PR
        pr = int(match.group(1))
        current = newest.get(pr)
        if current is None or int(build["id"]) > int(current["id"]):
            newest[pr] = build
    return newest


def package_stage_status(build_id: str, *, org: str = DEFAULT_ORG, project: str = DEFAULT_PROJECT, fetch=None) -> tuple[bool, int]:
    """Return ``(package stage succeeded, attempt number)`` for a build.

    The attempt matters: Azure DevOps REUSES the build id when a failed stage is re-run, and ``buildNumberRevision`` is a daily
    revision counter, not a rerun counter — so neither can distinguish a re-run. The timeline's per-stage ``attempt`` is the only
    field that does, and without it a corrected measurement after a re-run would be silently suppressed by the stamp.
    """
    fetch = fetch or http_get_json
    try:
        data = fetch(f"{_api(org, project)}/build/builds/{build_id}/timeline?api-version=7.1")
    except Exception as err:  # noqa: BLE001 - one unreadable timeline must not stop the sweep
        log(f"  ! build {build_id}: could not read timeline: {err}")
        return False, 1
    for record in data.get("records") or []:
        if record.get("type") == "Stage" and record.get("name") == PACKAGE_STAGE:
            attempt = record.get("attempt")
            attempt = attempt if isinstance(attempt, int) and attempt > 0 else 1
            ok = record.get("state") == "completed" and record.get("result") == "succeeded"
            return ok, attempt
    return False, 1


# --------------------------------------------------------------------------- #
# GitHub — the stamp
# --------------------------------------------------------------------------- #

def github_get(path: str, *, token: str | None = None, fetch=None):
    fetch = fetch or http_get_json
    headers = {"Accept": "application/vnd.github+json"}
    token = token if token is not None else os.environ.get("GH_TOKEN", "")
    if token:
        headers["Authorization"] = f"Bearer {token}"
    return fetch(f"{GITHUB_API}{path}", headers=headers)


def github_paginate(path: str, *, per_page: int = 100, max_pages: int = 50, **kwargs):
    """Yield every item across pages.

    The per-issue comments endpoint ignores ``sort`` and ``direction`` — it always returns oldest-first — so the report can sit on
    the LAST page once a PR passes ``per_page`` comments. Reading only the first page would miss it and re-measure forever.
    """
    sep = "&" if "?" in path else "?"
    for page in range(1, max_pages + 1):
        batch = github_get(f"{path}{sep}per_page={per_page}&page={page}", **kwargs)
        if not isinstance(batch, list):
            raise GitHubReadError(f"unexpected response shape for {path} page {page}")
        yield from batch
        if len(batch) < per_page:
            return
    raise GitHubReadError(f"{path} exceeded {max_pages} pages")


def reported_build(repo: str, pr_number: int, **kwargs) -> str | None:
    """Return the build identity already reported on a PR, or None.

    FIRST marker comment wins, matching every writer in the workflow (all of which use ``find()``). If reader and writers disagreed
    about which comment is authoritative — two markers are reachable when a manual dispatch races the sweep — the writers would keep
    stamping one comment while this read returned the other, and the PR would be re-measured on every sweep forever.
    """
    try:
        for comment in github_paginate(f"/repos/{repo}/issues/{pr_number}/comments", **kwargs):
            body = comment.get("body") or ""
            if COMMENT_MARKER in body:
                match = _BUILD_STAMP_RE.search(body)
                return match.group(1) if match else None
        return None
    except GitHubReadError:
        raise
    except Exception as err:  # noqa: BLE001
        raise GitHubReadError(f"PR #{pr_number}: {err}") from err


# --------------------------------------------------------------------------- #
# Selection
# --------------------------------------------------------------------------- #

def select(
    repo: str,
    *,
    hours: int = 2,
    only_pr: int | None = None,
    ignore_reported: bool = False,
    read_reported=None,
    azdo_fetch=None,
    timeline_fetch=None,
    **kwargs,
) -> list[dict]:
    """Return every build needing a report, oldest first.

    There is deliberately no cap. Measured over 250 hours of real traffic, the worst 2-hour window contained 7 distinct pull
    requests — 37x under GitHub's 256-job matrix limit — so a cap would only add a way to silently defer work without ever being
    needed. Oldest-first keeps a backlog draining FIFO.
    """
    read_reported = read_reported or (lambda pr: reported_build(repo, pr))
    builds = recent_pr_builds(hours=hours, fetch=azdo_fetch, **kwargs)
    if only_pr is not None:
        builds = {pr: b for pr, b in builds.items() if pr == only_pr}
    log(f"{len(builds)} pull request(s) with a build in the last {hours}h")

    ready: list[tuple[str, int, dict, int]] = []
    for pr, build in builds.items():
        if build.get("status") == "completed" and build.get("result") != "succeeded":
            log(f"  - PR #{pr}: build {build['id']} did not succeed")
            continue
        # The timeline is the only source of the rerun attempt, and it also lets an in-progress build report as soon as packaging
        # is done rather than waiting for the whole build (measured ~1h30m earlier on a real build).
        packaged, attempt = package_stage_status(build["id"], fetch=timeline_fetch)
        if not packaged:
            log(f"  - PR #{pr}: build {build['id']} has not packaged yet")
            continue
        ready.append((build.get("finishTime") or build.get("queueTime") or "", pr, build, attempt))

    ready.sort(key=lambda item: item[0])

    selected: list[dict] = []
    errors = 0
    for _, pr, build, attempt in ready:
        identity = f"{build['id']}.{attempt}"
        try:
            # An explicit request re-reports on demand, so the stamp is not consulted.
            already = None if ignore_reported else read_reported(pr)
        except GitHubReadError as err:
            log(f"  ! PR #{pr}: skipping, could not read its report: {err}")
            errors += 1
            continue
        if already == identity:
            log(f"  - PR #{pr}: build {identity} already reported")
            continue
        log(f"  + PR #{pr}: build {identity}")
        selected.append({"pr": pr, "build": str(build["id"]), "identity": identity})

    if errors and errors >= max(3, len(ready) // 2):
        log(f"::warning::{errors} of {len(ready)} PR lookups failed; GitHub may be throttling or unavailable")
    return selected


def parse_args(argv: list[str]) -> argparse.Namespace:
    p = argparse.ArgumentParser(description=__doc__,
                                formatter_class=argparse.RawDescriptionHelpFormatter)
    p.add_argument("--repo", required=True, help="owner/name of the GitHub repository.")
    p.add_argument("--hours", type=int, default=2,
                   help="How far back to look for builds (default: 2).")
    p.add_argument("--org", default=DEFAULT_ORG, help=f"AzDO org (default: {DEFAULT_ORG}).")
    p.add_argument("--project", default=DEFAULT_PROJECT,
                   help=f"AzDO project (default: {DEFAULT_PROJECT}).")
    p.add_argument("--definition", type=int, default=DEFAULT_DEFINITION,
                   help=f"AzDO pipeline definition id (default: {DEFAULT_DEFINITION}).")
    p.add_argument("--only-pr", type=int, default=None,
                   help="Restrict to a single PR number (used by `/track sizes`).")
    p.add_argument("--ignore-reported", action="store_true",
                   help="Report even if the stamp already matches (explicit re-request).")
    p.add_argument("--output", default=None, help="Write the matrix JSON here.")
    return p.parse_args(argv)


def main(argv: list[str]) -> int:
    args = parse_args(argv)
    entries = select(
        args.repo,
        hours=args.hours,
        only_pr=args.only_pr,
        ignore_reported=args.ignore_reported,
        org=args.org,
        project=args.project,
        definition=args.definition,
    )
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
