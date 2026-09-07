#!/usr/bin/env python3
"""The PR artifact-size comment: its format, and the only code that reads or writes it.

`find_pr_builds.py` decides whether to spend a ~1.1 GB download by reading a stamp out of this
comment, and three workflow steps write it. They must agree on the marker, the stamp and how
the comment is located, so all of that lives here once. They previously did not agree, twice:
a writer emitted `build=<id>` after the reader had moved to `build=<id> packaged=<time>`, and
the writers paginated to find the comment while the reader did not — which past 100 comments
hides the stamp from the reader alone. Both failures are silent and cost a gigabyte an hour.
"""
from __future__ import annotations

import argparse
import datetime
import json
import os
import re
import sys

sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))  # perf/
from _common import http_request, log, parse_iso_utc  # noqa: E402

MARKER = "<!-- skiasharp-pr-artifact-sizes -->"
STAMP_RE = re.compile(r"<!--\s*build=(\d+)\s+packaged=(\S+?)\s*-->")
# Whole seconds, UTC implied. Azure timestamps are always UTC and their sub-second precision
# is both inconsistent and useless for ordering builds minutes apart.
STAMP_FORMAT = "%Y-%m-%dT%H:%M:%S"

GITHUB_API = "https://api.github.com"


def compose(build_id: str, packaged_at: str | None, body: str) -> str:
    """Prefix a report body with the marker and the dedupe stamp.

    Callers never write either themselves. A body without a stamp the reader can parse is
    indistinguishable from an unmeasured PR, so it would be re-measured every hour; hence the
    fallback rather than omitting the stamp when the caller has no packaging time.
    """
    stamp = packaged_at or datetime.datetime.now(datetime.timezone.utc).strftime(STAMP_FORMAT)
    return "\n".join([MARKER, f"<!-- build={build_id} packaged={stamp} -->", body.lstrip("\n")])


def _headers() -> dict:
    headers = {"Accept": "application/vnd.github+json"}
    token = os.environ.get("GH_TOKEN", "")
    if token:
        headers["Authorization"] = f"******"
    return headers


def find(repo: str, pr: int, *, request=http_request) -> dict | None:
    """The bot's existing comment on this PR, or None.

    One page: the endpoint ignores sort/direction and always returns oldest-first, so the
    bot's comment stays on page 1 until the PR passes 100. Reading and writing share this,
    so they cannot disagree about where the comment is.
    """
    raw = request(f"{GITHUB_API}/repos/{repo}/issues/{pr}/comments?per_page=100",
                  headers=_headers())
    for comment in json.loads(raw.decode("utf-8")):
        if MARKER in (comment.get("body") or ""):
            return comment
    return None


def read_stamp(repo: str, pr: int, **kwargs) -> datetime.datetime | None:
    """When the packages last reported on this PR were produced.

    Raises on a failed read: the caller must treat that as *unknown*, never as *unreported*,
    or a throttled read repeats the ~1.1 GB download every sweep.
    """
    comment = find(repo, pr, **kwargs)
    if not comment:
        return None
    match = STAMP_RE.search(comment.get("body") or "")
    return parse_iso_utc(match.group(2)) if match else None


def upsert(repo: str, pr: int, body: str, *, request=http_request) -> None:
    existing = find(repo, pr, request=request)
    payload = json.dumps({"body": body}).encode("utf-8")
    if existing:
        request(f"{GITHUB_API}/repos/{repo}/issues/comments/{existing['id']}", method="PATCH",
                data=payload, headers=_headers())
        log(f"updated the size report on PR #{pr}")
    else:
        request(f"{GITHUB_API}/repos/{repo}/issues/{pr}/comments", method="POST", data=payload,
                headers=_headers())
        log(f"created the size report on PR #{pr}")


def status_body(status: str, build_id: str, build_url: str | None, run_url: str | None) -> str:
    """Placeholder bodies for the states with no measurement to render."""
    build = f"[`{build_id}`]({build_url})" if build_url else f"`{build_id}`"
    lead = (f"Measuring the packages from build {build}…" if status == "claiming"
            else f"Could not measure the packages from build {build}.")
    link = (f"[Run]({run_url})" if status == "claiming" else f"See [the failed run]({run_url}).")
    return "\n".join(["## 📦 Artifact size report", "", lead, "", link if run_url else ""])


def resolve_pr(explicit: str | None, from_json: str | None) -> int | None:
    """The PR to comment on; a manual dispatch may name only a build, so fall back to it."""
    if explicit and explicit.strip():
        return int(explicit.strip())
    if from_json and os.path.exists(from_json):
        with open(from_json, encoding="utf-8") as fh:
            return json.load(fh).get("prNumber") or None
    return None


def main(argv: list[str]) -> int:
    parser = argparse.ArgumentParser(description="Post or update the PR size-report comment.")
    parser.add_argument("--repo", required=True, help="owner/name")
    parser.add_argument("--pr", help="PR number; may be resolved from --pr-from instead")
    parser.add_argument("--pr-from", help="JSON measurement to read prNumber from")
    parser.add_argument("--build", required=True, help="AzDO build id, for the stamp")
    parser.add_argument("--packaged-at", help=f"packaging time ({STAMP_FORMAT}); default now")
    parser.add_argument("--body-file", help="rendered report; omit to post a --status body")
    parser.add_argument("--status", choices=["claiming", "failed"],
                        help="post a short placeholder instead of a rendered report")
    parser.add_argument("--build-url")
    parser.add_argument("--run-url")
    args = parser.parse_args(argv)

    pr = resolve_pr(args.pr, args.pr_from)
    if not pr:
        log("no PR number could be resolved; nothing to comment on")
        return 0

    if args.body_file:
        with open(args.body_file, encoding="utf-8") as fh:
            body = fh.read()
    else:
        body = status_body(args.status or "claiming", args.build, args.build_url, args.run_url)

    upsert(args.repo, pr, compose(args.build, args.packaged_at, body))
    return 0


if __name__ == "__main__":
    raise SystemExit(main(sys.argv[1:]))
