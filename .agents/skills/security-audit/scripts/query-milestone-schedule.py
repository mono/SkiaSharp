#!/usr/bin/env python3
"""
Query the Chromium milestone release schedule and produce a release heads-up for
SkiaSharp's Skia bumps.

This is NOT a security check by itself. It is an early-warning tool: Skia milestones
track Chrome milestones, so the Chromium schedule tells us *when* a given milestone
branches and goes stable. If a milestone we still need to bump to is about to go
stable (or has already), we should make sure the Skia bump is ready so security
fixes land in time.

Data source:
  https://chromiumdash.appspot.com/fetch_milestone_schedule?mstone=<N|current|next|previous>
  - Returns one milestone per request as {"mstones": [ {...} ]}.
  - `mstone` accepts a milestone number, or the keywords 'current', 'next', 'previous'.
  - No authentication, no documented rate limit (be polite: small delay between calls).

Each milestone object includes (dates are ISO-8601, midnight UTC):
  - mstone           : milestone number (e.g. 150)
  - branch_point     : when the release branch is cut from trunk
  - stable_date      : when the milestone reaches the stable channel
  - late_stable_date : end of the stable window / next refresh boundary
  - feature_freeze, earliest_beta, final_beta, etc.

Prerequisites:
  - Python 3.8+ (uses urllib, no external dependencies)

Usage:
  # Heads-up relative to the milestone currently pinned in scripts/VERSIONS.txt
  python3 query-milestone-schedule.py --output output/ai/milestone-schedule-cache.json

  # Look further ahead / behind
  python3 query-milestone-schedule.py --behind 1 --ahead 6 \
    --output output/ai/milestone-schedule-cache.json

  # Override the "current" milestone instead of reading VERSIONS.txt
  python3 query-milestone-schedule.py --current 150

  # Flag anything going stable within N days as urgent (default 14)
  python3 query-milestone-schedule.py --window 21 --verbose

Output:
  JSON written to --output (if given) and a human-readable summary to stdout.
  The JSON has:
    - meta: { generated_at, current_milestone, current_milestone_source, window_days }
    - milestones[]: one entry per fetched milestone with parsed dates + day deltas
    - headsup[]: prioritized alerts (e.g. milestone going stable within the window
                 that we have not bumped to yet)
"""

import argparse
import json
import os
import re
import sys
import time
import urllib.request
import urllib.error
from datetime import datetime, timezone

SCHEDULE_URL = "https://chromiumdash.appspot.com/fetch_milestone_schedule?mstone={mstone}"

# Dates we care about for a release heads-up, in chronological order.
KEY_DATES = ["branch_point", "stable_date", "late_stable_date"]


def log(msg, verbose=True):
    if verbose:
        print(msg, file=sys.stderr)


def find_repo_root(start):
    """Walk up from `start` looking for the SkiaSharp repo root (has scripts/VERSIONS.txt)."""
    cur = os.path.abspath(start)
    while True:
        if os.path.isfile(os.path.join(cur, "scripts", "VERSIONS.txt")):
            return cur
        parent = os.path.dirname(cur)
        if parent == cur:
            return None
        cur = parent


def read_current_milestone(repo_root):
    """Parse the pinned Skia milestone from scripts/VERSIONS.txt.

    Looks for a line like: `libSkiaSharp            milestone   150`
    Falls back to `skia ... release m150`.
    """
    if not repo_root:
        return None
    versions = os.path.join(repo_root, "scripts", "VERSIONS.txt")
    if not os.path.isfile(versions):
        return None
    milestone = None
    skia_release = None
    with open(versions, "r", encoding="utf-8") as f:
        for line in f:
            if line.lstrip().startswith("#"):
                continue
            m = re.match(r"\s*libSkiaSharp\s+milestone\s+(\d+)", line)
            if m:
                milestone = int(m.group(1))
            m = re.match(r"\s*skia\s+\S+\s+m(\d+)", line)
            if m:
                skia_release = int(m.group(1))
    return milestone if milestone is not None else skia_release


def fetch_milestone(mstone, retries=3, delay=0.4):
    url = SCHEDULE_URL.format(mstone=mstone)
    last_err = None
    for attempt in range(retries):
        try:
            req = urllib.request.Request(url, headers={"User-Agent": "skiasharp-security-audit/1.0"})
            with urllib.request.urlopen(req, timeout=30) as resp:
                data = json.loads(resp.read().decode("utf-8"))
            stones = data.get("mstones") or []
            if not stones:
                return None
            return stones[0]
        except (urllib.error.URLError, urllib.error.HTTPError, ValueError) as e:
            last_err = e
            time.sleep(delay * (attempt + 1))
    log(f"  ! failed to fetch mstone={mstone}: {last_err}")
    return None


def parse_date(value):
    if not value:
        return None
    try:
        dt = datetime.fromisoformat(value.replace("Z", "+00:00"))
        if dt.tzinfo is None:
            dt = dt.replace(tzinfo=timezone.utc)
        return dt
    except ValueError:
        return None


def days_until(dt, now):
    if dt is None:
        return None
    return (dt.date() - now.date()).days


def build_milestone_entry(raw, now):
    entry = {"milestone": raw.get("mstone"), "dates": {}}
    for key in KEY_DATES + ["feature_freeze", "earliest_beta", "final_beta"]:
        dt = parse_date(raw.get(key))
        if dt is None:
            continue
        entry["dates"][key] = {
            "date": dt.date().isoformat(),
            "days_from_now": days_until(dt, now),
        }
    return entry


def main():
    ap = argparse.ArgumentParser(description="Chromium milestone release heads-up for Skia bumps.")
    ap.add_argument("--current", type=int, default=None,
                    help="Override the current Skia milestone (default: read scripts/VERSIONS.txt).")
    ap.add_argument("--behind", type=int, default=1,
                    help="How many milestones before current to include (default: 1).")
    ap.add_argument("--ahead", type=int, default=4,
                    help="How many milestones after current to include (default: 4).")
    ap.add_argument("--window", type=int, default=14,
                    help="Flag milestones reaching stable within this many days as urgent (default: 14).")
    ap.add_argument("--output", default=None, help="Write the JSON result to this path.")
    ap.add_argument("--repo-root", default=None, help="Repo root (default: auto-detect from cwd).")
    ap.add_argument("--verbose", action="store_true", help="Print extra progress detail.")
    args = ap.parse_args()

    now = datetime.now(timezone.utc)

    repo_root = args.repo_root or find_repo_root(os.getcwd()) or find_repo_root(
        os.path.dirname(os.path.abspath(__file__)))

    current = args.current
    source = "--current argument"
    if current is None:
        current = read_current_milestone(repo_root)
        source = "scripts/VERSIONS.txt"
    if current is None:
        # Last resort: ask the API what the current Chrome milestone is.
        raw = fetch_milestone("current")
        if raw:
            current = raw.get("mstone")
            source = "chromiumdash 'current'"
    if current is None:
        print("ERROR: could not determine the current Skia milestone. Pass --current N.",
              file=sys.stderr)
        return 2

    lo = current - max(0, args.behind)
    hi = current + max(0, args.ahead)
    log(f"Current Skia milestone: {current} (from {source})", args.verbose)
    log(f"Fetching milestones {lo}..{hi} from chromiumdash...", args.verbose)

    milestones = []
    for m in range(lo, hi + 1):
        raw = fetch_milestone(m)
        time.sleep(0.3)
        if not raw:
            continue
        entry = build_milestone_entry(raw, now)
        entry["is_current"] = (m == current)
        entry["status"] = "bumped" if m <= current else "pending"
        milestones.append(entry)
        if args.verbose:
            sd = entry["dates"].get("stable_date", {})
            log(f"  m{m}: stable {sd.get('date', '?')} ({sd.get('days_from_now', '?')}d)")

    headsup = build_headsup(milestones, current, args.window, now)

    result = {
        "meta": {
            "generated_at": now.isoformat(),
            "current_milestone": current,
            "current_milestone_source": source,
            "window_days": args.window,
            "data_source": "https://chromiumdash.appspot.com/fetch_milestone_schedule",
        },
        "milestones": milestones,
        "headsup": headsup,
    }

    if args.output:
        os.makedirs(os.path.dirname(os.path.abspath(args.output)), exist_ok=True)
        with open(args.output, "w", encoding="utf-8") as f:
            json.dump(result, f, indent=2)
        log(f"Wrote {args.output}", True)

    print_summary(result)
    return 0


def build_headsup(milestones, current, window, now):
    """Produce prioritized alerts about pending bumps relative to the schedule."""
    alerts = []
    for entry in milestones:
        m = entry["milestone"]
        stable = entry["dates"].get("stable_date")
        branch = entry["dates"].get("branch_point")

        if entry["status"] == "pending":
            # We have NOT bumped to this milestone yet.
            if stable is not None:
                d = stable["days_from_now"]
                if d is not None and d < 0:
                    alerts.append({
                        "level": "critical",
                        "milestone": m,
                        "message": (f"m{m} reached stable {abs(d)} day(s) ago but SkiaSharp is "
                                    f"still on m{current}. Security fixes in m{m} are shipping "
                                    f"to users without a SkiaSharp bump."),
                    })
                elif d is not None and d <= window:
                    alerts.append({
                        "level": "urgent",
                        "milestone": m,
                        "message": (f"m{m} goes stable in {d} day(s) ({stable['date']}). "
                                    f"Make sure the Skia bump from m{current} is ready."),
                    })
                elif branch is not None and branch["days_from_now"] is not None \
                        and branch["days_from_now"] <= window:
                    bd = branch["days_from_now"]
                    when = "branched" if bd < 0 else f"branches in {bd} day(s)"
                    alerts.append({
                        "level": "watch",
                        "milestone": m,
                        "message": (f"m{m} {when} ({branch['date']}); stable {stable['date']}. "
                                    f"Start preparing the bump."),
                    })
        else:
            # Already on/ahead of this milestone; note its support window winding down.
            late = entry["dates"].get("late_stable_date")
            if entry["is_current"] and late is not None and late["days_from_now"] is not None \
                    and 0 <= late["days_from_now"] <= window:
                alerts.append({
                    "level": "info",
                    "milestone": m,
                    "message": (f"Current milestone m{m} stable window ends ~{late['date']} "
                                f"({late['days_from_now']}d). Next milestone takes over soon."),
                })

    order = {"critical": 0, "urgent": 1, "watch": 2, "info": 3}
    alerts.sort(key=lambda a: (order.get(a["level"], 9), a["milestone"]))
    return alerts


LEVEL_ICON = {"critical": "🔴", "urgent": "🟠", "watch": "🟡", "info": "🔵"}


def print_summary(result):
    meta = result["meta"]
    print()
    print(f"Chromium release schedule — heads-up for Skia bumps")
    print(f"  Current SkiaSharp milestone: m{meta['current_milestone']} "
          f"(from {meta['current_milestone_source']})")
    print(f"  Urgency window: {meta['window_days']} days\n")

    print("  Milestone   Branch point   Stable date    Status    In")
    print("  " + "-" * 58)
    for entry in result["milestones"]:
        m = entry["milestone"]
        b = entry["dates"].get("branch_point", {})
        s = entry["dates"].get("stable_date", {})
        d = s.get("days_from_now")
        din = f"{d:+d}d" if isinstance(d, int) else "  ?"
        marker = "*" if entry.get("is_current") else " "
        print(f"  {marker}m{m:<8}  {b.get('date','?'):<13}  {s.get('date','?'):<13}  "
              f"{entry['status']:<8}  {din:>5}")

    print()
    headsup = result["headsup"]
    if not headsup:
        print("  ✅ No release heads-up: no pending milestones inside the urgency window.")
    else:
        print("  Heads-up:")
        for a in headsup:
            icon = LEVEL_ICON.get(a["level"], "•")
            print(f"   {icon} [{a['level'].upper()}] {a['message']}")
    print()


if __name__ == "__main__":
    sys.exit(main())
