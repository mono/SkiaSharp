#!/usr/bin/env python3
"""
Query the Chromium milestone release schedule and produce a release heads-up for
SkiaSharp's Skia bumps.

This is NOT a security check by itself. It is an early-warning tool: Skia milestones
track Chrome milestones, so the Chromium schedule tells us *when* a given milestone
branches and goes stable. If a milestone we still need to bump to is about to go
stable (or has already), we should make sure the Skia bump is ready so security
fixes land in time.

Two data sources are combined:

1. Milestone *schedule* (dates per milestone):
   https://chromiumdash.appspot.com/fetch_milestone_schedule?mstone=<N|current|next|previous>
   - Returns one milestone per request as {"mstones": [ {...} ]}.
   - `mstone` accepts a milestone number, or the keywords 'current', 'next', 'previous'.

2. Channel *releases* (which milestone + Skia commit is live in each channel):
   https://chromiumdash.appspot.com/fetch_releases?channel=<Channel>&platform=<Platform>&num=1
   - Channels: Extended (extended stable), Stable, Beta, Dev, Canary.
   - Each release carries hashes.skia (the exact upstream Skia commit), milestone, version, time.
   - This is what lets us track Extended/Stable/Beta concurrently: each channel pins a different
     milestone and a different Skia commit, and SkiaSharp will maintain a branch per channel.

Each milestone schedule object includes (dates are ISO-8601, midnight UTC):
  - mstone           : milestone number (e.g. 150)
  - branch_point     : when the release branch is cut from trunk
  - stable_date      : when the milestone reaches the stable channel
  - late_stable_date : end of the stable window / next refresh boundary
  - feature_freeze, earliest_beta, final_beta, etc.

No authentication, no documented rate limit (be polite: small delay between calls).

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
    - meta: { generated_at, current_milestone, current_milestone_source, window_days,
              platform, tracked_channels }
    - channels[]: per-channel live release (channel, milestone, version, skia_hash, date, tracked)
    - milestones[]: one entry per fetched milestone with parsed dates, day deltas, and the
                    channel(s) currently sitting on that milestone
    - headsup[]: prioritized alerts (schedule + tracked-channel coverage)
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
RELEASES_URL = ("https://chromiumdash.appspot.com/fetch_releases"
                "?channel={channel}&platform={platform}&num=1")

# Chromium release channels, oldest milestone to newest.
ALL_CHANNELS = ["Extended", "Stable", "Beta", "Dev", "Canary"]
# Channels SkiaSharp maintains support for concurrently.
DEFAULT_TRACKED_CHANNELS = ["Extended", "Stable", "Beta"]
# Extended stable + Canary only exist on Windows/Mac; Windows has every channel.
DEFAULT_PLATFORM = "Windows"

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


def fetch_channel_release(channel, platform, retries=3, delay=0.4):
    """Fetch the current release for a channel/platform from fetch_releases.

    Returns a dict with channel, milestone, version, skia_hash, date (ISO), and the
    full hashes map, or None if the channel/platform has no release.
    """
    url = RELEASES_URL.format(channel=channel, platform=platform)
    last_err = None
    for attempt in range(retries):
        try:
            req = urllib.request.Request(url, headers={"User-Agent": "skiasharp-security-audit/1.0"})
            with urllib.request.urlopen(req, timeout=30) as resp:
                data = json.loads(resp.read().decode("utf-8"))
            if not isinstance(data, list) or not data:
                return None
            rel = data[0]
            hashes = rel.get("hashes") or {}
            ts = rel.get("time")
            date = None
            if isinstance(ts, (int, float)):
                # `time` is epoch milliseconds.
                date = datetime.fromtimestamp(ts / 1000.0, tz=timezone.utc).date().isoformat()
            return {
                "channel": channel,
                "platform": platform,
                "milestone": rel.get("milestone"),
                "version": rel.get("version"),
                "skia_hash": hashes.get("skia"),
                "chromium_hash": hashes.get("chromium"),
                "date": date,
            }
        except (urllib.error.URLError, urllib.error.HTTPError, ValueError) as e:
            last_err = e
            time.sleep(delay * (attempt + 1))
    log(f"  ! failed to fetch channel={channel} platform={platform}: {last_err}")
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
    ap.add_argument("--platform", default=DEFAULT_PLATFORM,
                    help="Platform for channel releases (default: Windows — has all channels).")
    ap.add_argument("--channels", default=",".join(ALL_CHANNELS),
                    help="Comma-separated channels to fetch (default: Extended,Stable,Beta,Dev,Canary).")
    ap.add_argument("--track", default=",".join(DEFAULT_TRACKED_CHANNELS),
                    help="Channels SkiaSharp supports concurrently (default: Extended,Stable,Beta).")
    ap.add_argument("--no-channels", action="store_true",
                    help="Skip the channel-release lookup (schedule-only mode).")
    ap.add_argument("--output", default=None, help="Write the JSON result to this path.")
    ap.add_argument("--repo-root", default=None, help="Repo root (default: auto-detect from cwd).")
    ap.add_argument("--verbose", action="store_true", help="Print extra progress detail.")
    args = ap.parse_args()

    channel_names = [c.strip() for c in args.channels.split(",") if c.strip()]
    tracked = [c.strip() for c in args.track.split(",") if c.strip()]

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

    log(f"Current Skia milestone: {current} (from {source})", args.verbose)

    # Fetch live channel releases first (Extended/Stable/Beta/Dev/Canary).
    channels = []
    if not args.no_channels:
        log(f"Fetching channel releases ({args.platform}): {', '.join(channel_names)}", args.verbose)
        for ch in channel_names:
            rel = fetch_channel_release(ch, args.platform)
            time.sleep(0.3)
            if not rel:
                continue
            rel["tracked"] = ch in tracked
            channels.append(rel)
            if args.verbose:
                log(f"  {ch}: m{rel.get('milestone')} {rel.get('version')} "
                    f"skia={(rel.get('skia_hash') or '?')[:12]}")

    # Schedule range: current ± behind/ahead, widened to cover every channel milestone.
    channel_mstones = [c["milestone"] for c in channels if isinstance(c.get("milestone"), int)]
    lo = min([current - max(0, args.behind)] + channel_mstones)
    hi = max([current + max(0, args.ahead)] + channel_mstones)
    log(f"Fetching milestone schedule {lo}..{hi} from chromiumdash...", args.verbose)

    milestones = []
    for m in range(lo, hi + 1):
        raw = fetch_milestone(m)
        time.sleep(0.3)
        if not raw:
            continue
        entry = build_milestone_entry(raw, now)
        entry["is_current"] = (m == current)
        entry["status"] = "bumped" if m <= current else "pending"
        # Attach the channel(s) currently sitting on this milestone.
        entry["channels"] = [c["channel"] for c in channels if c.get("milestone") == m]
        skia = next((c["skia_hash"] for c in channels if c.get("milestone") == m and c.get("skia_hash")), None)
        if skia:
            entry["skia_hash"] = skia
        milestones.append(entry)
        if args.verbose:
            sd = entry["dates"].get("stable_date", {})
            log(f"  m{m}: stable {sd.get('date', '?')} ({sd.get('days_from_now', '?')}d)")

    headsup = build_headsup(milestones, current, args.window, now)
    headsup += build_channel_headsup(channels, tracked, current)
    headsup.sort(key=lambda a: (LEVEL_ORDER.get(a["level"], 9), a.get("milestone") or 0))

    result = {
        "meta": {
            "generated_at": now.isoformat(),
            "current_milestone": current,
            "current_milestone_source": source,
            "window_days": args.window,
            "platform": args.platform,
            "tracked_channels": tracked,
            "data_sources": [
                "https://chromiumdash.appspot.com/fetch_milestone_schedule",
                "https://chromiumdash.appspot.com/fetch_releases",
            ],
        },
        "channels": channels,
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


def build_channel_headsup(channels, tracked, current):
    """Alerts about the channels SkiaSharp tracks concurrently (Extended/Stable/Beta).

    Each tracked channel pins a milestone + Skia commit. If a tracked channel has advanced
    past the milestone SkiaSharp is pinned to, that channel's branch needs a bump.
    """
    alerts = []
    for ch in channels:
        if not ch.get("tracked"):
            continue
        m = ch.get("milestone")
        if not isinstance(m, int):
            continue
        name = ch["channel"]
        skia = (ch.get("skia_hash") or "?")[:12]
        ver = ch.get("version") or "?"
        if m > current:
            alerts.append({
                "level": "urgent",
                "milestone": m,
                "channel": name,
                "message": (f"Tracked channel {name} is on m{m} ({ver}, skia {skia}) but SkiaSharp "
                            f"is pinned to m{current}. The {name} branch needs a Skia bump to m{m}."),
            })
        elif m == current:
            alerts.append({
                "level": "info",
                "milestone": m,
                "channel": name,
                "message": (f"Tracked channel {name} is on m{m} ({ver}, skia {skia}) — matches the "
                            f"pinned milestone."),
            })
        else:
            alerts.append({
                "level": "info",
                "milestone": m,
                "channel": name,
                "message": (f"Tracked channel {name} is on m{m} ({ver}, skia {skia}); SkiaSharp's "
                            f"{name} branch should pin m{m}."),
            })
    return alerts


LEVEL_ORDER = {"critical": 0, "urgent": 1, "watch": 2, "info": 3}
LEVEL_ICON = {"critical": "🔴", "urgent": "🟠", "watch": "🟡", "info": "🔵"}


def print_summary(result):
    meta = result["meta"]
    print()
    print(f"Chromium release schedule — heads-up for Skia bumps")
    print(f"  Current SkiaSharp milestone: m{meta['current_milestone']} "
          f"(from {meta['current_milestone_source']})")
    print(f"  Urgency window: {meta['window_days']} days")
    if meta.get("tracked_channels"):
        print(f"  Tracked channels ({meta.get('platform','?')}): "
              f"{', '.join(meta['tracked_channels'])}")
    print()

    channels = result.get("channels") or []
    if channels:
        print("  Channel     Milestone   Version             Skia commit   Track")
        print("  " + "-" * 64)
        for c in channels:
            track = "✓" if c.get("tracked") else " "
            skia = (c.get("skia_hash") or "?")[:12]
            print(f"  {c.get('channel',''):<10}  m{str(c.get('milestone','?')):<8}  "
                  f"{c.get('version','?'):<18}  {skia:<12}  {track}")
        print()

    print("  Milestone   Branch point   Stable date    Status    In      Channels")
    print("  " + "-" * 72)
    for entry in result["milestones"]:
        m = entry["milestone"]
        b = entry["dates"].get("branch_point", {})
        s = entry["dates"].get("stable_date", {})
        d = s.get("days_from_now")
        din = f"{d:+d}d" if isinstance(d, int) else "  ?"
        marker = "*" if entry.get("is_current") else " "
        chans = ",".join(entry.get("channels") or [])
        print(f"  {marker}m{m:<8}  {b.get('date','?'):<13}  {s.get('date','?'):<13}  "
              f"{entry['status']:<8}  {din:>5}   {chans}")

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
