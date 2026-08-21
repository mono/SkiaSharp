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
It also drift-checks the release-notes "support" paths (scripts/infra/docs/versions.json)
against the live channels and reports OK/WARN/DRIFT — detection only; the fix is a manual
edit of that file (spec §3.5).

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
STABLE_LIKE = {"Extended", "Stable"}  # channels that ship to non-preview users
PLATFORM = "Windows"             # the only platform that carries all five channels

KEY_DATES = ["branch_point", "stable_date", "late_stable_date"]
LEVEL_ORDER = {"critical": 0, "error": 0, "urgent": 1, "unknown": 1, "watch": 2, "warn": 2, "ok": 3, "info": 4}
LEVEL_ICON = {"critical": "🔴", "error": "🔴", "urgent": "🟠", "unknown": "❓", "watch": "🟡", "warn": "🟡", "ok": "🟢", "info": "🔵"}


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
    if not isinstance(data, dict):
        return None
    stones = data.get("mstones")
    if isinstance(stones, list) and stones and isinstance(stones[0], dict):
        return stones[0]
    return None


def fetch_channel(channel, platform):
    data = http_json(RELEASES_URL.format(channel=channel, platform=platform))
    if not isinstance(data, list) or not data or not isinstance(data[0], dict):
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


# --- Support tiers (versions.json) ---
# The release-notes TOC/index groups release lines by a manually-maintained
# "support" block in scripts/infra/docs/versions.json (two lists: stable +
# preview). We DON'T write that file here — we only read it and report drift
# against the live Chrome channels, so a maintainer can fix it by hand.

SUPPORT_VERSIONS_REL = os.path.join("scripts", "infra", "docs", "versions.json")


def line_milestone(line):
    """'4.148' -> 148 (the SkiaSharp minor IS the Chrome/Skia milestone)."""
    try:
        return int(str(line).split(".")[1])
    except (IndexError, ValueError):
        return None


def load_support_block(repo_root):
    """Read the support paths from versions.json: {"stable": [...], "preview": [...]}.

    Returns None when the file or block is absent (the grouping then degrades to
    the legacy flat layout and there is nothing to drift-check).
    """
    if not repo_root:
        return None
    path = os.path.join(repo_root, SUPPORT_VERSIONS_REL)
    if not os.path.isfile(path):
        return None
    try:
        with open(path, "r", encoding="utf-8") as f:
            data = json.load(f)
    except (ValueError, OSError):
        return None
    block = data.get("support")
    if not isinstance(block, dict):
        return None

    def as_list(value):
        if value is None:
            return []
        if isinstance(value, str):
            return [value]
        if isinstance(value, list):
            return [str(v) for v in value]
        return []

    return {"stable": as_list(block.get("stable")),
            "preview": as_list(block.get("preview"))}


# --- date math ---

def parse_date(value):
    if not isinstance(value, str) or not value:
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

def build_headsup(main_ms, beta_ms, stable_like_ms, upcoming, window, beta_stable_days):
    alerts = []

    # Primary signal: is the front line keeping up with Beta?
    if beta_ms is None:
        alerts.append({"level": "unknown", "milestone": main_ms,
                       "message": ("Could not read the Beta channel milestone (Chromium Dash "
                                   "unavailable or returned no data). The main-vs-Beta signal "
                                   "could not be evaluated — re-run before relying on this.")})
    elif main_ms < beta_ms:
        behind = beta_ms - main_ms
        # Critical when a milestone newer than main is ALREADY on a stable-class channel
        # (live channel data is authoritative; the schedule date is a fallback hint).
        live_stable = stable_like_ms is not None and stable_like_ms > main_ms
        if live_stable or (beta_stable_days is not None and beta_stable_days < 0):
            where = (f"m{stable_like_ms} already ships on a stable channel" if live_stable
                     else f"m{beta_ms} is already stable")
            alerts.append({"level": "critical", "milestone": beta_ms,
                           "message": (f"main is on m{main_ms} but Beta is on m{beta_ms} "
                                       f"({behind} ahead) and {where}. The Skia bump on main "
                                       f"is overdue.")})
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


def build_support_alerts(support, chrome_ms):
    """Compare versions.json support paths to the live Chrome channels (spec §3.5).

    ``chrome_ms`` maps channel name -> milestone (from fetch_releases). Implements
    the support-tier decision table:

      * stable line must sit on Chrome **Stable** ``S``, or on **Extended-stable**
        ``E`` during the promotion gap (when a preview already reaches ``S``);
        every stable entry must be ``S`` or ``E`` — nothing older or off-channel.
      * preview line must cover Chrome **Beta** ``B`` (newer, in Dev/Canary, is OK;
        behind ``B`` warns; at/below ``S`` is not a real preview).

    Detection only — the fix is always a manual edit of versions.json. Returns
    ``(alerts, status)`` where status is ``ok`` | ``warn`` | ``drift`` | ``unknown``.
    """
    E = chrome_ms.get("Extended")
    S = chrome_ms.get("Stable")
    B = chrome_ms.get("Beta")
    alerts = []

    if S is None or B is None:
        alerts.append({"level": "unknown", "message": (
            "Could not read the Chrome Stable/Beta milestone — support-tier drift "
            "not evaluated. Re-run before relying on this.")})
        return alerts, "unknown"

    e_label = "m%d" % E if E is not None else "m?"
    stable_ms = sorted({m for m in (line_milestone(l) for l in support["stable"]) if m is not None})
    preview_ms = sorted({m for m in (line_milestone(l) for l in support["preview"]) if m is not None})
    allowed = {m for m in (E, S) if m is not None}
    floor = E if E is not None else S

    # --- stable line ---
    if not stable_ms:
        alerts.append({"level": "error", "message": (
            "support.stable is empty — it must list the current Chrome Stable "
            "milestone m%d (or Extended-stable %s during a promotion gap)." % (S, e_label))})
    else:
        top = max(stable_ms)
        ptop = max(preview_ms) if preview_ms else None
        # Additional (older) supported lines must still be on a stable-class channel.
        for m in stable_ms:
            if m != top and m not in allowed:
                alerts.append({"level": "error", "message": (
                    "support.stable also lists m%d, which is neither the Chrome Stable "
                    "(m%d) nor Extended-stable (%s) milestone — supported lines must be "
                    "stable or extended-stable." % (m, S, e_label))})
        # Headline: where the newest supported stable sits vs the channels.
        if top == S:
            pass  # current stable — ideal
        elif E is not None and top == E and E < S:
            if ptop is not None and ptop >= S:
                alerts.append({"level": "ok", "message": (
                    "support.stable is on Extended-stable m%d while preview m%d "
                    "(>= Chrome Stable m%d) is about to promote — the normal "
                    "promotion gap." % (top, ptop, S))})
            else:
                alerts.append({"level": "error", "message": (
                    "support.stable is on Extended-stable m%d but no preview reaches "
                    "Chrome Stable m%d — promote a stable or add a preview at m%d." % (top, S, S))})
        elif top > S:
            alerts.append({"level": "warn", "message": (
                "support.stable newest line is m%d, ahead of Chrome Stable m%d — "
                "unusual; verify the support paths." % (top, S))})
        elif top < floor:
            alerts.append({"level": "error", "message": (
                "support.stable newest line is m%d, behind even Chrome Extended-stable "
                "%s — SkiaSharp stable is out of date." % (top, e_label))})
        else:  # floor < top < S — not a stable-class channel
            alerts.append({"level": "error", "message": (
                "support.stable newest line is m%d, between Chrome Extended-stable %s "
                "and Stable m%d and not a supported channel — use the Stable (m%d) or "
                "Extended-stable milestone." % (top, e_label, S, S))})

    # --- preview line ---
    if not preview_ms:
        alerts.append({"level": "warn", "message": (
            "support.preview is empty — no in-flight preview line is documented "
            "(Chrome Beta is m%d)." % B)})
    else:
        ptop = max(preview_ms)
        for m in [m for m in preview_ms if m <= S]:
            alerts.append({"level": "error", "message": (
                "support.preview lists m%d, which is not newer than Chrome Stable "
                "m%d — a preview line must be ahead of stable." % (m, S))})
        if ptop > B:
            alerts.append({"level": "ok", "message": (
                "support.preview m%d is ahead of Chrome Beta m%d (Dev/Canary) — "
                "fine, previewing ahead." % (ptop, B))})
        elif ptop > S and ptop < B:
            alerts.append({"level": "warn", "message": (
                "support.preview newest line is m%d, behind Chrome Beta m%d — update "
                "the preview line soon." % (ptop, B))})

    if not alerts:
        alerts.append({"level": "ok", "message": (
            "support.stable/preview match the Chrome channels (Extended %s, Stable "
            "m%d, Beta m%d)." % (e_label, S, B))})
    status = ("drift" if any(a["level"] == "error" for a in alerts)
              else "warn" if any(a["level"] == "warn" for a in alerts) else "ok")
    alerts.sort(key=lambda a: LEVEL_ORDER.get(a["level"], 9))
    return alerts, status


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

    # Highest milestone already live on a non-preview (Stable/Extended) channel.
    stable_like_ms = max(
        (c["milestone"] for c in channels
         if c.get("channel") in STABLE_LIKE and isinstance(c.get("milestone"), int)),
        default=None)

    if beta_ms is None:
        status = "unknown"
    elif main_ms < beta_ms:
        status = "behind"
    else:
        status = "current"
    headsup = build_headsup(main_ms, beta_ms, stable_like_ms, upcoming, args.window, beta_stable_days)

    # Support-tier drift: compare the manually-maintained versions.json support
    # paths to the live channels (read-only; the fix is a manual edit, spec §3.5).
    chrome_ms = {c["channel"]: c.get("milestone") for c in channels}
    support_block = load_support_block(repo_root)
    if support_block is None:
        support_result = {
            "configured": False,
            "status": "absent",
            "alerts": [{"level": "info", "message": (
                "No 'support' block in %s — support-tier drift not checked." % SUPPORT_VERSIONS_REL)}],
        }
    else:
        support_alerts, support_status = build_support_alerts(support_block, chrome_ms)
        support_result = {
            "configured": True,
            "stable_lines": support_block["stable"],
            "preview_lines": support_block["preview"],
            "chrome": {
                "extended": chrome_ms.get("Extended"),
                "stable": chrome_ms.get("Stable"),
                "beta": chrome_ms.get("Beta"),
            },
            "status": support_status,
            "alerts": support_alerts,
        }

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
        "support": support_result,
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
    icon = {"current": "🟢", "behind": "🟠", "unknown": "❓"}.get(meta["status"], "🟠")
    print()
    print("Chromium release heads-up (main vs Beta)")
    print(f"  Where we are: main = m{meta['main_milestone']} "
          f"(major {meta.get('major','?')}, from {meta['main_milestone_source']})")
    print(f"  Beta channel: m{meta.get('beta_milestone') or '?'}   {icon} {meta['status'].upper()}")
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

    support = result.get("support")
    if support:
        status = support.get("status", "absent")
        icon = {"ok": "🟢", "warn": "🟡", "drift": "🔴", "unknown": "❓", "absent": "🔵"}.get(status, "•")
        print(f"  Support tiers (versions.json):  {icon} {status.upper()}")
        if support.get("configured"):
            ch = support.get("chrome", {})
            print(f"   stable={support.get('stable_lines')}  preview={support.get('preview_lines')}"
                  f"   (Chrome Extended m{ch.get('extended')} / Stable m{ch.get('stable')} / Beta m{ch.get('beta')})")
        for a in support.get("alerts", []):
            print(f"   {LEVEL_ICON.get(a['level'], '•')} [{a['level'].upper()}] {a['message']}")
        print()


if __name__ == "__main__":
    sys.exit(main())
