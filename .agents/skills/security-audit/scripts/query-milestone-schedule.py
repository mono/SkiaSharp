#!/usr/bin/env python3
"""
Chromium release heads-up for SkiaSharp's Skia bumps.

Model (see references/milestone-schedule.md for the full rationale):
  - `main` is the SkiaSharp front line and tracks the Chrome **Beta** milestone.
  - As milestones graduate Beta -> Stable -> Extended stable, a `release/<major>.<M>.x`
    branch is cut from a `main` that was already on milestone M, so on the *milestone*
    axis those release lines are inherently covered.
  - Therefore "where we are" = main's milestone, and the one signal that matters is:
        is  main_milestone >= beta_channel_milestone ?
    If main falls behind Beta, the bump to the new milestone is overdue. The schedule
    tells us how much lead time remains before that gap reaches the stable channel.

Data sources (no auth, no documented rate limit):
  1. https://chromiumdash.appspot.com/fetch_milestone_schedule?mstone=<N|current|next|previous>
       -> branch_point / stable_date / ... for one milestone per request.
  2. https://chromiumdash.appspot.com/fetch_releases?channel=<C>&platform=Windows&num=1
       -> the live milestone + exact Skia commit (hashes.skia) per channel.

Default output is main-centric and lean. The full 5-channel list is included as context.

Usage:
  python3 query-milestone-schedule.py                       # lean heads-up to stdout
  python3 query-milestone-schedule.py --output out.json     # + structured JSON
  python3 query-milestone-schedule.py --json                # print JSON instead of a table

Prerequisites: Python 3.8+ (stdlib only).
"""

import argparse
import json
import os
import re
import sys
import time
import urllib.error
import urllib.request
from datetime import datetime, timezone

SCHEDULE_URL = "https://chromiumdash.appspot.com/fetch_milestone_schedule?mstone={mstone}"
RELEASES_URL = ("https://chromiumdash.appspot.com/fetch_releases"
                "?channel={channel}&platform={platform}&num=1")

# Channels oldest -> newest milestone. Windows carries all of them.
ALL_CHANNELS = ["Extended", "Stable", "Beta", "Dev", "Canary"]
FRONT_CHANNEL = "Beta"            # the channel `main` is expected to keep up with
PLATFORM = "Windows"             # the only platform that carries all five channels

KEY_DATES = ["branch_point", "stable_date", "late_stable_date"]
LEVEL_ORDER = {"critical": 0, "urgent": 1, "watch": 2, "ok": 3, "info": 4}
LEVEL_ICON = {"critical": "🔴", "urgent": "🟠", "watch": "🟡", "ok": "🟢", "info": "🔵"}


def log(msg):
    print(msg, file=sys.stderr)


# --- HTTP ---

def http_json(url, retries=3, delay=0.4):
    last = None
    for attempt in range(retries):
        try:
            req = urllib.request.Request(url, headers={"User-Agent": "skiasharp-security-audit/1.0"})
            with urllib.request.urlopen(req, timeout=30) as resp:
                return json.loads(resp.read().decode("utf-8"))
        except (urllib.error.URLError, urllib.error.HTTPError, ValueError) as e:
            last = e
            time.sleep(delay * (attempt + 1))
    log(f"  ! request failed: {url} ({last})")
    return None


def fetch_schedule(mstone):
    data = http_json(SCHEDULE_URL.format(mstone=mstone))
    stones = (data or {}).get("mstones") or []
    return stones[0] if stones else None


def fetch_channel(channel, platform):
    data = http_json(RELEASES_URL.format(channel=channel, platform=platform))
    if not isinstance(data, list) or not data:
        return None
    rel = data[0]
    hashes = rel.get("hashes") or {}
    ts = rel.get("time")
    date = None
    if isinstance(ts, (int, float)):
        date = datetime.fromtimestamp(ts / 1000.0, tz=timezone.utc).date().isoformat()
    return {
        "channel": channel,
        "platform": platform,
        "milestone": rel.get("milestone"),
        "version": rel.get("version"),
        "skia_hash": hashes.get("skia"),
        "date": date,
    }


# --- Local repo / VERSIONS.txt ---

def find_repo_root(start):
    cur = os.path.abspath(start)
    while True:
        if os.path.isfile(os.path.join(cur, "scripts", "VERSIONS.txt")):
            return cur
        parent = os.path.dirname(cur)
        if parent == cur:
            return None
        cur = parent


def parse_versions(text):
    """Return (milestone, major) from a VERSIONS.txt body."""
    milestone = major = None
    for line in text.splitlines():
        if line.lstrip().startswith("#"):
            continue
        m = re.match(r"\s*libSkiaSharp\s+milestone\s+(\d+)", line)
        if m:
            milestone = int(m.group(1))
        m = re.match(r"\s*SkiaSharp\s+nuget\s+(\d+)\.\d+", line)
        if m:
            major = int(m.group(1))
    return milestone, major


def read_main_versions(repo_root):
    if not repo_root:
        return None, None
    path = os.path.join(repo_root, "scripts", "VERSIONS.txt")
    if not os.path.isfile(path):
        return None, None
    with open(path, "r", encoding="utf-8") as f:
        return parse_versions(f.read())


# --- date math ---

def parse_date(value):
    if not value:
        return None
    try:
        dt = datetime.fromisoformat(value.replace("Z", "+00:00"))
        return dt if dt.tzinfo else dt.replace(tzinfo=timezone.utc)
    except ValueError:
        return None


def days_until(dt, now):
    return None if dt is None else (dt.date() - now.date()).days


def milestone_dates(raw, now):
    dates = {}
    for key in KEY_DATES + ["feature_freeze", "earliest_beta", "final_beta"]:
        dt = parse_date(raw.get(key))
        if dt is not None:
            dates[key] = {"date": dt.date().isoformat(), "days_from_now": days_until(dt, now)}
    return dates


# --- core ---

def build_headsup(main_ms, beta_ms, upcoming, window, beta_stable_days):
    alerts = []

    # Primary signal: is the front line keeping up with Beta?
    if beta_ms is None:
        pass
    elif main_ms < beta_ms:
        behind = beta_ms - main_ms
        if beta_stable_days is not None and beta_stable_days < 0:
            alerts.append({"level": "critical", "milestone": beta_ms,
                           "message": (f"main is on m{main_ms} but Beta has moved to m{beta_ms} "
                                       f"({behind} ahead) and m{beta_ms} is ALREADY stable. The "
                                       f"Skia bump on main is overdue.")})
        else:
            in_days = f" (stable in {beta_stable_days}d)" if beta_stable_days is not None else ""
            alerts.append({"level": "urgent", "milestone": beta_ms,
                           "message": (f"main is on m{main_ms} but Beta is on m{beta_ms} "
                                       f"({behind} ahead){in_days}. Bump main to m{beta_ms}.")})
    else:
        alerts.append({"level": "ok", "milestone": main_ms,
                       "message": f"main (m{main_ms}) is at or ahead of Beta (m{beta_ms}). Front line current."})

    # Schedule watch for milestones beyond main that haven't branched/stabilised yet.
    for entry in upcoming:
        if entry["milestone"] <= main_ms:
            continue
        branch = entry["dates"].get("branch_point")
        stable = entry["dates"].get("stable_date")
        if branch and branch["days_from_now"] is not None and 0 <= branch["days_from_now"] <= window:
            alerts.append({"level": "watch", "milestone": entry["milestone"],
                           "message": (f"m{entry['milestone']} branches in {branch['days_from_now']}d "
                                       f"({branch['date']}); stable {stable['date'] if stable else '?'}.")})

    alerts.sort(key=lambda a: (LEVEL_ORDER.get(a["level"], 9), a["milestone"]))
    return alerts


def main():
    ap = argparse.ArgumentParser(description="Chromium release heads-up for SkiaSharp Skia bumps (main vs Beta).")
    ap.add_argument("--ahead", type=int, default=4,
                    help="How many milestones past main to include in the schedule (default: 4).")
    ap.add_argument("--window", type=int, default=14,
                    help="Days-ahead threshold for schedule 'watch' alerts (default: 14).")
    ap.add_argument("--json", action="store_true", help="Print the JSON result to stdout instead of a table.")
    ap.add_argument("--output", default=None, help="Write the JSON result to this path.")
    args = ap.parse_args()

    now = datetime.now(timezone.utc)
    repo_root = find_repo_root(os.getcwd()) or \
        find_repo_root(os.path.dirname(os.path.abspath(__file__)))

    main_ms, major = read_main_versions(repo_root)
    if main_ms is None:
        print("ERROR: could not determine main's milestone from scripts/VERSIONS.txt.", file=sys.stderr)
        return 2

    # Channels (for the main-vs-Beta signal + context).
    channels, beta_ms = [], None
    log(f"Fetching channels ({PLATFORM})...")
    for name in ALL_CHANNELS:
        rel = fetch_channel(name, PLATFORM)
        time.sleep(0.3)
        if rel:
            channels.append(rel)
            if name == FRONT_CHANNEL:
                beta_ms = rel.get("milestone")
            log(f"  {name}: m{rel.get('milestone')} {rel.get('version')} "
                f"skia={(rel.get('skia_hash') or '?')[:12]}")

    # Upcoming schedule: main .. max(main+ahead, beta).
    hi = max(main_ms + max(0, args.ahead), beta_ms or main_ms)
    log(f"Fetching schedule m{main_ms}..m{hi}...")
    upcoming = []
    for m in range(main_ms, hi + 1):
        raw = fetch_schedule(m)
        time.sleep(0.3)
        if not raw:
            continue
        entry = {"milestone": m, "dates": milestone_dates(raw, now),
                 "is_main": m == main_ms,
                 "channels": [c["channel"] for c in channels if c.get("milestone") == m]}
        upcoming.append(entry)

    beta_stable_days = None
    for e in upcoming:
        if e["milestone"] == beta_ms:
            sd = e["dates"].get("stable_date")
            beta_stable_days = sd["days_from_now"] if sd else None

    status = "behind" if (beta_ms is not None and main_ms < beta_ms) else "current"
    headsup = build_headsup(main_ms, beta_ms, upcoming, args.window, beta_stable_days)

    result = {
        "meta": {
            "generated_at": now.isoformat(),
            "main_milestone": main_ms,
            "main_milestone_source": "scripts/VERSIONS.txt",
            "major": major,
            "beta_milestone": beta_ms,
            "status": status,
            "platform": PLATFORM,
            "window_days": args.window,
            "data_sources": [
                "https://chromiumdash.appspot.com/fetch_milestone_schedule",
                "https://chromiumdash.appspot.com/fetch_releases",
            ],
        },
        "channels": channels,
        "upcoming": upcoming,
        "headsup": headsup,
    }

    if args.output:
        os.makedirs(os.path.dirname(os.path.abspath(args.output)), exist_ok=True)
        with open(args.output, "w", encoding="utf-8") as f:
            json.dump(result, f, indent=2)
        log(f"Wrote {args.output}")

    if args.json:
        print(json.dumps(result, indent=2))
    else:
        print_summary(result)
    return 0


def print_summary(result):
    meta = result["meta"]
    icon = "🟢" if meta["status"] == "current" else "🟠"
    print()
    print("Chromium release heads-up (main vs Beta)")
    print(f"  Where we are: main = m{meta['main_milestone']} "
          f"(major {meta.get('major','?')}, from {meta['main_milestone_source']})")
    print(f"  Beta channel: m{meta.get('beta_milestone','?')}   {icon} {meta['status'].upper()}")
    print(f"  Urgency window: {meta['window_days']}d ({meta.get('platform','?')})")
    print()

    channels = result.get("channels") or []
    if channels:
        print("  Channel     Milestone   Version             Skia commit")
        print("  " + "-" * 58)
        for c in channels:
            front = " <- main tracks this" if c["channel"] == FRONT_CHANNEL else ""
            print(f"  {c['channel']:<10}  m{str(c.get('milestone','?')):<8}  "
                  f"{c.get('version','?'):<18}  {(c.get('skia_hash') or '?')[:12]}{front}")
        print()

    print("  Upcoming    Branch point   Stable date    Stable in   Channels")
    print("  " + "-" * 66)
    for e in result["upcoming"]:
        b = e["dates"].get("branch_point", {})
        s = e["dates"].get("stable_date", {})
        d = s.get("days_from_now")
        din = f"{d:+d}d" if isinstance(d, int) else "?"
        marker = "*" if e.get("is_main") else " "
        print(f"  {marker}m{e['milestone']:<7}  {b.get('date','?'):<13}  {s.get('date','?'):<13}  "
              f"{din:>8}   {','.join(e.get('channels') or [])}")
    print()

    print("  Heads-up:")
    for a in result["headsup"]:
        print(f"   {LEVEL_ICON.get(a['level'], '•')} [{a['level'].upper()}] {a['message']}")
    print()


if __name__ == "__main__":
    sys.exit(main())
