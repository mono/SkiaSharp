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
import re
import sys

sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))  # perf/
from _common import http_get_json, log  # noqa: E402

DEFAULT_ORG = "dnceng-public"
DEFAULT_PROJECT = "public"
# The public SkiaSharp pipeline; its `Package NuGets` job publishes the `nuget` artifact
# that measure_pr.py consumes.
DEFAULT_DEFINITION = 345

GITHUB_API = "https://api.github.com"
# The stable marker on the size-report comment; the build stamp follows it.
COMMENT_MARKER = "<!-- skiasharp-pr-artifact-sizes"
_BUILD_STAMP_RE = re.compile(r"<!--\s*build=(\d+)\s*-->")


class GitHubReadError(RuntimeError):
    """A PR's existing size comment could not be read.

    The caller must treat this as *unknown*, never as *unmeasured*: measuring downloads
    roughly a gigabyte, so guessing "not yet measured" on a throttled or failing read is
    how a transient API problem turns into a repeated multi-gigabyte download.
    """


def github_get(path: str, *, token: str | None = None, fetch=None):
    """GET one GitHub API path, authenticated when a token is available."""
    fetch = fetch or http_get_json
    headers = {"Accept": "application/vnd.github+json"}
    token = token if token is not None else os.environ.get("GH_TOKEN", "")
    if token:
        headers["Authorization"] = f"Bearer {token}"
    return fetch(f"{GITHUB_API}{path}", headers=headers)


def github_paginate(path: str, *, per_page: int = 100, max_pages: int = 50, **kwargs):
    """Yield every item across pages.

    The per-issue comments endpoint ignores `sort` and `direction` — it always returns
    oldest-first — so the newest comment is on the LAST page. Reading only the first page
    would miss the size report on any PR with more than `per_page` comments.
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


def measured_build(repo: str, pr_number: int, **kwargs) -> str | None:
    """Return the build id already reported on a PR, or None if there is no report.

    FIRST marker comment wins, matching every writer in the workflow (all of which use
    `page.data.find(...)`). If reader and writers ever disagreed about which comment is
    authoritative — two markers can exist after a concurrent manual dispatch, or because
    a human quoted the marker — the writers would keep stamping one comment while this
    read returned the other, and the PR would be re-selected and re-downloaded forever.

    Raises GitHubReadError when the comments cannot be read.
    """
    try:
        for comment in github_paginate(f"/repos/{repo}/issues/{pr_number}/comments",
                                       **kwargs):
            body = comment.get("body") or ""
            if COMMENT_MARKER in body:
                match = _BUILD_STAMP_RE.search(body)
                return match.group(1) if match else None
        return None
    except GitHubReadError:
        raise
    except Exception as err:  # noqa: BLE001
        raise GitHubReadError(f"PR #{pr_number}: {err}") from err


def open_pull_requests(repo: str, **kwargs) -> list[int]:
    """Return the open PR numbers for a repository.

    Ascending `created` order makes paging append-only, so a PR opened mid-sweep cannot
    shift the window and hide an entry.
    """
    return [pr["number"]
            for pr in github_paginate(
                f"/repos/{repo}/pulls?state=open&sort=created&direction=asc", **kwargs)]


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
    *,
    read_measured=None,
    **kwargs,
) -> list[dict]:
    """Return the matrix entries for PRs with a new, not-yet-measured build.

    `read_measured(pr)` looks the stamp up live; a GitHubReadError from it SKIPS that PR
    for this sweep rather than measuring it, because an unreadable stamp is unknown, not
    absent. The next sweep retries.
    """
    measured = measured or {}
    entries: list[dict] = []
    errors: list[int] = []
    for pr_number in pr_numbers:
        build_id = latest_successful_build(pr_number, errors=errors, **kwargs)
        if not build_id:
            if pr_number not in errors:
                log(f"  - PR #{pr_number}: no successful package build")
            continue
        if read_measured is not None:
            try:
                already = read_measured(pr_number)
            except GitHubReadError as err:
                log(f"  ! PR #{pr_number}: skipping, could not read its report: {err}")
                errors.append(pr_number)
                continue
        else:
            already = measured.get(pr_number)
        if already == build_id:
            log(f"  - PR #{pr_number}: build {build_id} already measured")
            continue
        log(f"  + PR #{pr_number}: build {build_id}")
        entries.append({"pr": pr_number, "build": build_id})
    # A widespread query failure (throttling, an AzDO outage) otherwise looks exactly like
    # "nobody has a build" and would silently stop PR size reports with no signal anywhere.
    if errors and len(errors) >= max(3, len(pr_numbers) // 2):
        log(f"::warning::{len(errors)} of {len(pr_numbers)} PR lookups failed; "
            "Azure DevOps or GitHub may be throttling or unavailable")
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
                   help="Open PR number to check (repeatable). Omit with --repo to sweep.")
    p.add_argument("--repo", default=None,
                   help="owner/name; discover open PRs and read their reported build ids.")
    p.add_argument("--measured", action="append", default=[],
                   help="Already-measured <pr>=<build> pair (repeatable). Ignored when "
                        "--repo is given, because the stamp is then read live.")
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
    pr_numbers = list(args.pr)
    read_measured = None
    if args.repo:
        # Discovery and stamp reads both fail closed: an unreadable PR list aborts the
        # sweep rather than silently reporting "nothing to do".
        if not pr_numbers:
            pr_numbers = open_pull_requests(args.repo)
            log(f"{len(pr_numbers)} open pull request(s)")
        read_measured = lambda pr: measured_build(args.repo, pr)  # noqa: E731
    entries = build_matrix(
        pr_numbers,
        parse_measured(args.measured),
        read_measured=read_measured,
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
