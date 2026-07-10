#!/usr/bin/env python3
"""Gather the network-sourced index data into _sources/index.json.

    build-index.py   ->  documentation/docfx/releases/_sources/index.json

The release-notes pipeline is three scripts, split by what they PRODUCE:

    build-data.py   ->  _sources/<version>.data.json          (per-page facts)
    build-index.py  ->  _sources/index.json                   (index data, THIS)
    render-notes.py ->  <version>.md + TOC.yml + index.md     (all Markdown)

TOC.yml and index.md are AGGREGATES that render-notes.py builds from the committed
JSON, entirely offline. The one thing that aggregate needs but cannot get without
a network — the live Chrome release schedule for the two milestones in flight (the
release-cadence timeline) — is fetched HERE, in the network-capable Prepare phase,
and committed as _sources/index.json so the render stays offline and re-runnable.

This script does the two things that need a network: it enumerates the remote
release branches to record which ``-unreleased`` pages are still live heads (the
``live_unreleased`` set that render-notes.py ``--all`` later uses to prune stale
ones — the pruning itself is done there, offline), and it fetches the Chrome
schedule. It writes NO Markdown.

It reuses build-data.py's shared low-level helpers so there is one source of truth
for git/version parsing.
"""

from __future__ import annotations

import importlib.util
import json
import re
import sys
import urllib.request
from pathlib import Path

# ── reuse build-data.py's shared low-level helpers (one source of truth) ─────
_GEN = Path(__file__).with_name("build-data.py")
_spec = importlib.util.spec_from_file_location("_rn_build_data", str(_GEN))
_gen = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(_gen)

log = _gen.log
RELEASES_DIR = _gen.RELEASES_DIR
CHROME_SCHEDULE_URL = _gen.CHROME_SCHEDULE_URL
get_upcoming_version = _gen.get_upcoming_version
get_version_from_remote_branch = _gen.get_version_from_remote_branch
list_remote_release_branches = _gen.list_remote_release_branches
cadence_milestones = _gen.cadence_milestones


def live_unreleased_versions():
    # type: () -> set[str]
    """The version cores whose ``{version}-unreleased.md`` page is still a live head.

    An unreleased page models "what may ship next" for an in-flight LINE, and a
    line is in-flight only while it is the version of an active HEAD — either
    ``main`` (the upcoming version) or a servicing ``release/X.Y.x`` branch.
    Determining that needs the remote branch list, so it is computed HERE, in the
    network-capable Prepare phase, and recorded in index.json. render-notes.py
    (offline) then deletes any ``-unreleased`` page whose version is not in this
    set — e.g. once the head advances 4.148 → 4.150 and no ``release/4.148.x``
    servicing branch exists, ``4.148.0-unreleased.md`` is stale and removed; if
    that servicing branch DOES exist the page is kept, because 4.148 is still a
    live (serviced) head. This script writes NO Markdown and deletes nothing.

    An empty set (no release branches enumerable) means "unknown" — render-notes.py
    treats that as "prune nothing" to avoid clobbering on a degenerate checkout.
    """
    all_branches = list_remote_release_branches()
    if not all_branches:
        return set()

    live = set()  # type: set[str]
    main_version = get_upcoming_version()
    if main_version:
        live.add(main_version)
    for b in all_branches:
        if not b.endswith(".x"):
            continue
        m = re.match(r"release/(\d+)\.(\d+)\.x$", b)
        if not m:
            continue
        svc_version = (get_version_from_remote_branch(b)
                       or "{}.{}.0".format(m.group(1), m.group(2)))
        live.add(svc_version)
    return live


def fetch_chrome_schedule(milestone, timeout=8):
    # type: (int, int) -> dict
    """Return real Chrome phase dates for ``milestone`` from Chromium Dash.

    Returns ``{"beta", "early_stable", "stable_cut", "stable"}`` -> ISO date
    string. The schedule is required, so any problem (offline, timeout,
    HTTP/JSON error, missing milestone or missing phase date) raises
    ``RuntimeError`` and fails generation loudly rather than emitting a
    placeholder — a retry once connectivity is back recreates the page.
    """
    url = CHROME_SCHEDULE_URL.format(milestone)
    try:
        req = urllib.request.Request(url, headers={"User-Agent": "SkiaSharp-docs"})
        with urllib.request.urlopen(req, timeout=timeout) as resp:
            status = getattr(resp, "status", 200)
            if status != 200:
                raise RuntimeError("HTTP {}".format(status))
            payload = json.loads(resp.read().decode("utf-8"))
    except Exception as e:
        raise RuntimeError(
            "Could not fetch Chrome schedule for m{} from {}: {}".format(
                milestone, url, e))
    mstones = payload.get("mstones") or []
    if not mstones:
        raise RuntimeError(
            "Chrome schedule for m{} returned no milestone data".format(milestone))
    ms = mstones[0]
    fields = {
        "beta": "earliest_beta",
        "early_stable": "early_stable",
        "stable_cut": "stable_cut",
        "stable": "stable_date",
    }
    schedule = {}
    for phase, key in fields.items():
        value = ms.get(key)
        if not value:
            raise RuntimeError(
                "Chrome schedule for m{} is missing '{}'".format(milestone, key))
        schedule[phase] = value
    return schedule


def _index_json_path():
    # type: () -> Path
    """The committed index data: ``releases/_sources/index.json``."""
    return RELEASES_DIR / "_sources" / "index.json"


def build_index_json():
    # type: () -> dict
    """The network-sourced index data render-notes.py needs, offline.

    Two things that need a network and that the offline render cannot recompute:

      * ``chrome_schedule`` — the live Chrome schedule for the two milestones in
        flight, for the release-cadence timeline in index.md.
      * ``live_unreleased`` — the version cores whose ``-unreleased`` page is still
        a live head (from the remote branch list). render-notes.py deletes any
        ``-unreleased`` page not in this set; build-index writes NO Markdown and
        deletes nothing itself.

    Timestamp-free so an identical run yields an identical file and there is no git
    churn (mirrors data.json, spec §4.6).
    """
    _, cur_ms, next_ms = cadence_milestones()
    return {
        "chrome_schedule": {
            str(cur_ms): fetch_chrome_schedule(cur_ms),
            str(next_ms): fetch_chrome_schedule(next_ms),
        },
        "live_unreleased": sorted(live_unreleased_versions()),
    }


def main():
    if not RELEASES_DIR.is_dir():
        log("Error: {} does not exist".format(RELEASES_DIR), file=sys.stderr)
        sys.exit(1)
    data = build_index_json()
    path = _index_json_path()
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(data, indent=2) + "\n")
    log("Wrote {}".format(path))


if __name__ == "__main__":
    main()
