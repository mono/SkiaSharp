#!/usr/bin/env python3
"""Refresh the data behind the "Moving faster with AI" dashboard.

The dashboard on the ``/ai/`` site page renders from a cached JSON file,
``documentation/site/ai/dashboard-data.json``. This script refreshes that cache
so the page can be regenerated at build time (or on demand) without any live,
client-side API calls.

Design goals (see documentation/site/ai/index.html):

  * **Cache in the build.** The committed JSON is the source of truth the page
    ships with. This script updates it in place; a build then copies it verbatim.

  * **Prefer git history over the GitHub API.** The milestone bump PR numbers and
    their merge dates come from ``git log`` of the checked-out repository, not the
    ``gh`` / GitHub search API — no token, no rate limit, always available in CI.
    (The one thing git cannot give for a squash-merged PR is the *opened* date;
    those are immutable historical facts kept in the committed snapshot.)

  * **Degrade gracefully, per source.** Each section (adoption / cadence / cost)
    is refreshed independently. If a source is unreachable, that section keeps its
    last-known values and its existing ``asOf`` date; the page shows "as of <date>".
    A failure never produces a broken file or a non-zero exit that breaks the build.

Sources, mirroring the conventions already used in generate-release-notes.py:

  * NuGet   — https://azuresearch-usnc.nuget.org/query (totals + per-version counts)
  * Chrome  — https://chromiumdash.appspot.com/fetch_milestone_schedule (branch/stable)
  * Git     — ``git log`` for milestone bump squash commits (PR number + merge date)

Usage::

    python3 scripts/infra/docs/generate-ai-dashboard.py            # refresh in place
    python3 scripts/infra/docs/generate-ai-dashboard.py --output X # write elsewhere
    python3 scripts/infra/docs/generate-ai-dashboard.py --check    # don't write, report
"""

import argparse
import datetime
import gzip
import json
import re
import subprocess
import sys
import urllib.request
from pathlib import Path

# ── Paths & constants ────────────────────────────────────────────────────────

DEFAULT_OUTPUT = Path("documentation/site/ai/dashboard-data.json")

# NuGet search service (fast; gives totals + per-version downloads). The ...ussc
# host is a mirror used if the primary is slow. Both send Access-Control-* so the
# same endpoints also work if we ever move to a client-side fetch.
NUGET_HOSTS = [
    "https://azuresearch-usnc.nuget.org/query",
    "https://azuresearch-ussc.nuget.org/query",
]

# Same Chromium Dashboard endpoint that generate-release-notes.py already uses.
CHROME_SCHEDULE_URL = (
    "https://chromiumdash.appspot.com/fetch_milestone_schedule?mstone={}")

# NuGet registration index — used only for per-version *published* dates (the
# search service above does not expose them). Powers the "merged -> shipped on
# NuGet" metric in the cadence panel.
NUGET_REGISTRATION_URL = (
    "https://api.nuget.org/v3/registration5-gz-semver2/{}/index.json")

# How many of the most recent milestone bumps to show in the cadence timeline.
# 6 reaches back to m132/m133, the first AI-assisted catch-up bumps.
CADENCE_COUNT = 6

# SkiaSharp versions are <major>.<milestone>.<patch>; this is the current major.
SKIASHARP_MAJOR = 4

# How many recent release lines (<major>.<minor>) to show in the adoption panel.
ADOPTION_LINE_COUNT = 5

USER_AGENT = "SkiaSharp-ai-dashboard"


def log(*args):
    """Progress goes to STDERR so STDOUT can stay clean (matches sibling script)."""
    print(*args, file=sys.stderr)


# ── Small helpers ────────────────────────────────────────────────────────────

def today_iso():
    # type: () -> str
    return datetime.datetime.now(datetime.timezone.utc).date().isoformat()


def http_get_json(url, timeout=12):
    # type: (str, int) -> dict
    req = urllib.request.Request(
        url, headers={"User-Agent": USER_AGENT, "Accept-Encoding": "gzip"})
    with urllib.request.urlopen(req, timeout=timeout) as resp:
        status = getattr(resp, "status", 200)
        if status != 200:
            raise RuntimeError("HTTP {}".format(status))
        raw = resp.read()
        # Some NuGet registration blobs are gzip-compressed regardless of the
        # Accept-Encoding negotiation; decompress by content-encoding or magic.
        if resp.headers.get("Content-Encoding") == "gzip" or raw[:2] == b"\x1f\x8b":
            raw = gzip.decompress(raw)
        return json.loads(raw.decode("utf-8"))


def git(args):
    # type: (list[str]) -> str
    """Run a git command and return stdout (raises on failure)."""
    result = subprocess.run(
        ["git"] + args, capture_output=True, text=True, check=True)
    return result.stdout.strip()


def version_key(version):
    # type: (str) -> tuple
    """Sort key that orders releases correctly and puts a stable release ABOVE
    its own prereleases (4.148.0 > 4.148.0-rc.1 > 4.148.0-preview.1)."""
    core, _, pre = version.partition("-")
    nums = []
    for part in core.split("."):
        m = re.match(r"\d+", part)
        nums.append(int(m.group()) if m else 0)
    # A missing prerelease tag sorts last (i.e. the stable release is greatest).
    return (tuple(nums), 0 if not pre else -1, pre)


def utc_date_from_git_iso(iso):
    # type: (str) -> str
    """``2026-07-01T01:31:01+02:00`` -> ``2026-06-30`` (UTC calendar date).

    Git commit dates carry a timezone; converting to UTC before taking the date
    keeps them consistent with the UTC dates the GitHub API reports, so a merge
    just after midnight local time is not counted as a day late.
    """
    dt = datetime.datetime.fromisoformat(iso)
    if dt.tzinfo is not None:
        dt = dt.astimezone(datetime.timezone.utc)
    return dt.date().isoformat()


# ── Panel 1: adoption (NuGet) ────────────────────────────────────────────────

def fetch_nuget_package(package_id):
    # type: (str) -> dict
    """Return the NuGet search record for a package id, trying mirror hosts."""
    query = "?q=packageid:{}&prerelease=true&semVerLevel=2.0.0".format(package_id)
    last_error = None
    for host in NUGET_HOSTS:
        try:
            payload = http_get_json(host + query)
        except Exception as e:  # noqa: BLE001 - any failure -> try the mirror
            last_error = e
            continue
        data = payload.get("data") or []
        for record in data:
            if record.get("id", "").lower() == package_id.lower():
                return record
        raise RuntimeError("package '{}' not found in results".format(package_id))
    raise RuntimeError(
        "all NuGet hosts failed for '{}': {}".format(package_id, last_error))


def build_adoption(existing):
    # type: (dict) -> dict
    """Refresh the adoption panel from NuGet; fall back to ``existing`` on error."""
    try:
        primary = fetch_nuget_package("SkiaSharp")
        versions = primary.get("versions") or []
        # Current GA = highest version with no prerelease suffix.
        ga = None
        for v in versions:
            ver = v.get("version", "")
            if "-" in ver:
                continue
            if ga is None or version_key(ver) > version_key(ga["version"]):
                ga = {"version": ver, "downloads": int(v.get("downloads", 0))}
        adoption = {
            "asOf": today_iso(),
            "sourceUrl": "https://www.nuget.org/packages/SkiaSharp",
            "primary": {
                "id": "SkiaSharp",
                "total": int(primary.get("totalDownloads", 0)),
                "currentGa": ga or {"version": "", "downloads": 0},
                "lines": aggregate_version_lines(versions, ADOPTION_LINE_COUNT),
            },
        }
        log("adoption: refreshed ({:,} total downloads)".format(
            adoption["primary"]["total"]))
        return adoption
    except Exception as e:  # noqa: BLE001 - keep last-known adoption block
        log("adoption: refresh failed, keeping snapshot ({})".format(e))
        return existing.get("adoption", {})


def aggregate_version_lines(versions, count):
    # type: (list[dict], int) -> list[dict]
    """Collapse per-version download counts into the newest ``count`` release
    lines (``<major>.<minor>``), each split into cumulative stable vs prerelease
    downloads, e.g. ``{"line": "4.148", "stableVersion": "4.148.0",
    "stable": 74059, "previews": 773}``. Newest line first.
    """
    lines = {}  # (major, minor) -> aggregate
    for v in versions:
        ver = v.get("version", "")
        parts = ver.split(".")
        if len(parts) < 2:
            continue
        try:
            major = int(re.match(r"\d+", parts[0]).group())
            minor = int(re.match(r"\d+", parts[1]).group())
        except (AttributeError, ValueError):
            continue
        downloads = int(v.get("downloads", 0))
        line = lines.setdefault(
            (major, minor),
            {"line": "{}.{}".format(major, minor), "stable": 0,
             "previews": 0, "stableVersion": None})
        if "-" in ver:
            line["previews"] += downloads
        else:
            line["stable"] += downloads
            # Track the highest stable version string for the line.
            if (line["stableVersion"] is None
                    or version_key(ver) > version_key(line["stableVersion"])):
                line["stableVersion"] = ver
    ordered = sorted(lines, reverse=True)[:count]
    return [lines[k] for k in ordered]


# ── Panel 2: cadence (git history + Chromium Dashboard) ──────────────────────

_MILESTONE_RE = re.compile(r"milestone\s+m?(\d+)", re.IGNORECASE)
_PR_RE = re.compile(r"\(#(\d+)\)")


def discover_milestone_bumps(count):
    # type: (int) -> list[dict]
    """Find recent milestone bump PRs from git history (no GitHub API).

    Scans the first-parent history of HEAD for squash-merge commits whose subject
    names a Skia milestone and carries a ``(#PR)`` suffix, e.g.
    ``[skia-sync] Update Skia to milestone m151 (4.151.0) (#4294)``. Returns the
    most recent ``count`` distinct milestones, oldest first, each as
    ``{milestone, prNumber, prMerged}``.
    """
    # %cI = committer date, strict ISO with timezone; %s = subject.
    out = git(["log", "--first-parent", "HEAD",
               "--grep=milestone", "-i", "--pretty=format:%cI\t%s"])
    by_milestone = {}  # milestone -> earliest bump commit
    for line in out.splitlines():
        if "\t" not in line:
            continue
        iso, subject = line.split("\t", 1)
        pr_match = _PR_RE.search(subject)
        ms_match = _MILESTONE_RE.search(subject)
        if not pr_match or not ms_match:
            continue
        milestone = int(ms_match.group(1))
        merged = utc_date_from_git_iso(iso)
        entry = {"milestone": milestone,
                 "prNumber": int(pr_match.group(1)),
                 "prMerged": merged}
        prev = by_milestone.get(milestone)
        # Keep the earliest merge for a milestone (the original bump, not a re-merge).
        if prev is None or merged < prev["prMerged"]:
            by_milestone[milestone] = entry
    ordered = sorted(by_milestone.values(), key=lambda e: e["prMerged"])
    return ordered[-count:]


def fetch_chrome_branch_and_stable(milestone):
    # type: (int) -> dict
    """Return ``{branchPoint, stableDate}`` (UTC dates) for a Chrome milestone."""
    payload = http_get_json(CHROME_SCHEDULE_URL.format(milestone))
    mstones = payload.get("mstones") or []
    if not mstones:
        raise RuntimeError("no schedule data for m{}".format(milestone))
    ms = mstones[0]
    branch = ms.get("branch_point")
    stable = ms.get("stable_date")
    if not branch:
        raise RuntimeError("m{} missing branch_point".format(milestone))
    return {
        "branchPoint": branch[:10],
        "stableDate": stable[:10] if stable else "",
    }


def fetch_nuget_first_releases():
    # type: () -> dict
    """Return ``{milestone: 'YYYY-MM-DD'}`` — the date SkiaSharp first shipped a
    package for each ``<major>.<milestone>.0`` line (earliest published version,
    preview / rc / GA). Read from the NuGet registration index (published dates).
    """
    index = http_get_json(NUGET_REGISTRATION_URL.format("skiasharp"), timeout=25)
    published = {}  # version -> published date

    def walk(items):
        for it in items:
            entry = it.get("catalogEntry")
            if entry:
                ver, pub = entry.get("version"), entry.get("published")
                if ver and pub:
                    published[ver] = pub[:10]
            elif "items" in it:
                walk(it["items"])

    for page in index.get("items", []):
        if "items" in page:
            walk(page["items"])
        else:  # paged registration: the leaf blob lives behind its @id
            walk(http_get_json(page["@id"], timeout=25).get("items", []))

    earliest = {}  # milestone -> earliest published date
    for ver, pub in published.items():
        parts = ver.split(".")
        if len(parts) < 3 or parts[0] != str(SKIASHARP_MAJOR):
            continue
        m = re.match(r"\d+", parts[1])
        if not m:
            continue
        milestone = int(m.group())
        # A published date of "1900-01-01" marks an unlisted/deleted version.
        if pub.startswith("1900"):
            continue
        if milestone not in earliest or pub < earliest[milestone]:
            earliest[milestone] = pub
    return earliest


def build_cadence(existing):
    # type: (dict) -> dict
    """Refresh the cadence panel: PR#/merge from git, branch/stable from Chrome,
    and the first-NuGet-release date from the NuGet registration index.

    The immutable ``prOpened`` date (which a squash merge does not preserve in git
    history) is carried over from the committed snapshot by PR number. Any source
    that fails leaves the affected value on its last-known state.
    """
    prev_cadence = existing.get("cadence", {})
    prev_by_pr = {m.get("prNumber"): m
                  for m in prev_cadence.get("milestones", [])}
    prev_by_ms = {m.get("milestone"): m
                  for m in prev_cadence.get("milestones", [])}

    try:
        bumps = discover_milestone_bumps(CADENCE_COUNT)
    except Exception as e:  # noqa: BLE001 - can't read git -> keep snapshot
        log("cadence: git history unavailable, keeping snapshot ({})".format(e))
        return prev_cadence

    if not bumps:
        log("cadence: no milestone bumps found in git, keeping snapshot")
        return prev_cadence

    # First-NuGet-release dates (best-effort; falls back per milestone below).
    try:
        first_releases = fetch_nuget_first_releases()
    except Exception as e:  # noqa: BLE001
        log("cadence: NuGet release dates unavailable, keeping snapshot ({})".format(e))
        first_releases = {}

    milestones = []
    for bump in bumps:
        prev = prev_by_pr.get(bump["prNumber"]) or prev_by_ms.get(bump["milestone"]) or {}
        entry = {
            "milestone": bump["milestone"],
            "branchPoint": prev.get("branchPoint", ""),
            "prNumber": bump["prNumber"],
            # opened is immutable and not in git history — carry from snapshot.
            "prOpened": prev.get("prOpened", ""),
            "prMerged": bump["prMerged"],
            "stableDate": prev.get("stableDate", ""),
            # first ship on NuGet — refresh from NuGet, else carry snapshot value.
            "firstPreview": first_releases.get(
                bump["milestone"], prev.get("firstPreview")),
            "note": prev.get("note", ""),
        }
        try:
            chrome = fetch_chrome_branch_and_stable(bump["milestone"])
            entry["branchPoint"] = chrome["branchPoint"]
            if chrome["stableDate"]:
                entry["stableDate"] = chrome["stableDate"]
        except Exception as e:  # noqa: BLE001 - keep snapshot dates for this one
            log("  · m{} Chrome schedule failed, keeping snapshot dates: {}".format(
                bump["milestone"], e))
        milestones.append(entry)

    log("cadence: refreshed {} milestone(s): {}".format(
        len(milestones), ", ".join("m{}".format(m["milestone"]) for m in milestones)))
    return {
        "asOf": today_iso(),
        "scheduleUrl": prev_cadence.get(
            "scheduleUrl", "https://chromiumdash.appspot.com/schedule"),
        "prsUrl": prev_cadence.get(
            "prsUrl",
            "https://github.com/mono/SkiaSharp/pulls?q=is%3Apr+milestone+in%3Atitle"),
        "caption": prev_cadence.get(
            "caption",
            "AI opens, tests, and lands the sync PR; humans review the API."),
        "milestones": milestones,
    }


# ── Panel 3: automation cost ─────────────────────────────────────────────────

def build_cost(existing):
    # type: (dict) -> dict
    """Carry over the curated cost figures.

    Per-run "Effective tokens" / "Turns" live inside each gh-aw run's *log
    archive*, which is not readable without downloading and unzipping the logs
    (and the right permissions). Rather than hard-code fragile log scraping into
    the site build, these representative figures are curated in the committed
    snapshot and shown with an "as of" date. Refresh them by re-running the
    workflows and reading ``gh run view <id> --log | grep -E 'Effective tokens|Turns'``.
    """
    return existing.get("cost", {})


# ── Main ─────────────────────────────────────────────────────────────────────

def load_existing(path):
    # type: (Path) -> dict
    if path.exists():
        try:
            return json.loads(path.read_text(encoding="utf-8"))
        except Exception as e:  # noqa: BLE001
            log("warning: could not parse existing {} ({}); starting fresh".format(
                path, e))
    return {}


def main(argv=None):
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument(
        "--output", type=Path, default=DEFAULT_OUTPUT,
        help="path to the dashboard JSON to refresh (default: {})".format(
            DEFAULT_OUTPUT))
    parser.add_argument(
        "--base", type=Path, default=None,
        help="existing JSON to read last-known values from "
             "(default: same as --output)")
    parser.add_argument(
        "--check", action="store_true",
        help="compute and print, but do not write the output file")
    args = parser.parse_args(argv)

    base_path = args.base or args.output
    existing = load_existing(base_path)

    result = {
        "generatedAt": today_iso(),
        "adoption": build_adoption(existing),
        "cadence": build_cadence(existing),
        "cost": build_cost(existing),
        "footerUrl": existing.get(
            "footerUrl",
            "https://github.com/mono/SkiaSharp/tree/main/.github/workflows"),
    }

    text = json.dumps(result, indent=2, ensure_ascii=False) + "\n"
    if args.check:
        log("--check: not writing. Result:")
        print(text)
        return 0

    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(text, encoding="utf-8")
    log("wrote {}".format(args.output))
    return 0


if __name__ == "__main__":
    sys.exit(main())
