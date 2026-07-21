# Chromium Release Heads-Up (main vs Beta)

[Chromium Dash](https://chromiumdash.appspot.com/) exposes two JSON endpoints we combine to
answer one question: **are we keeping up with what's coming, and how much time do we have?**
The same data also drift-checks the release-notes **support** paths (see
[Support-Tier Drift](#support-tier-drift-release-notes-support-block) below).

1. **Channel releases** — which milestone *and exact Skia commit* is live in each channel
   (Extended stable, Stable, Beta, Dev, Canary).
2. **Milestone schedule** — when each milestone branches from trunk and reaches stable.

## The Model (why this is simple)

- SkiaSharp's `main` is the **front line** and tracks the Chrome **Beta** milestone.
- As a milestone graduates **Beta → Stable → Extended stable**, a `release/<major>.<M>.x` branch
  is cut from a `main` that was *already* on milestone M. So on the **milestone axis**, the
  stable / extended-stable release lines are inherently covered — their branch *name* states the
  milestone (`release/4.148.x` = m148), and they inherited that Skia from main.
- Therefore **"where we are" = main's milestone**, and the single signal that matters is:

  > **is `main_milestone >= beta_channel_milestone` ?**

  If `main` falls behind Beta, the Skia bump to the new milestone is overdue. The schedule then
  tells us how much lead time remains before that gap reaches the **stable** channel.

This is why the default output is main-centric and does **not** walk previous release branches.

> ℹ️ This is scheduling context, **not** a security check on its own. Pair it with the Chrome
> Releases blog (CVE disclosure) and the Skia CVE resolution process to decide urgency.

### Branch ↔ milestone mapping (verified)

| Branch | Pinned milestone | NuGet |
|--------|------------------|-------|
| `main` | 150 (current Beta) | 4.150.x |
| `release/4.148.x` | 148 (Extended) | 4.148.0 |
| `release/3.119.x` | 119 | 3.119.5 |
| `release/2.88.x` | 88 | 2.88.x |

`<major>` is the SkiaSharp epoch (currently `4`); the **minor is the milestone**. Milestones are
globally unique across majors, so `release/*.<M>.x` is unambiguous. A `release/<major>.<M>.x`
branch is only cut partway through the cycle — until then, `main` is the head for that milestone.

## What This Tool Tracks (and What It Doesn't)

This tool answers **milestone coverage**: is `main` keeping up with the front line? It needs
only `main`'s milestone and the Beta channel milestone — nothing else.

It does **not** try to verify whether a shipped `release/*.x` line carries the latest Skia
*within* its milestone (e.g. a cherry-picked CVE fix in `150.0.7871.x`). That within-milestone
backport question belongs to the [Skia CVE resolution](skia-cve-resolution.md) process, which
does proper merge-base ancestry against the upstream fix commit. A naive SHA comparison can't
answer it anyway: our submodule pins the **mono/skia fork**, not upstream google/skia, so the
branch SHA and the channel's `hashes.skia` are not directly comparable.

## Data Access

### Channel releases (milestone + Skia commit per channel)

```
https://chromiumdash.appspot.com/fetch_releases?channel=<Channel>&platform=<Platform>&num=1
```

- **Channels:** `Extended`, `Stable`, `Beta`, `Dev`, `Canary`.
- **Platform:** use `Windows` — it carries **all** channels (Extended stable & Canary are absent on
  Linux). `Mac` also has Extended stable.
- **Returns:** a JSON array (newest first); element `[0]` is the current release.

| Field | Meaning |
|-------|---------|
| `channel` | `Extended` / `Stable` / `Beta` / `Dev` / `Canary` |
| `milestone` | Milestone live in that channel |
| `version` | Full Chrome version (e.g. `150.0.7871.24`) |
| `hashes.skia` | **Exact upstream Skia commit** in that channel |
| `time` | Release timestamp (epoch **milliseconds**) |

### Milestone schedule (dates)

```
https://chromiumdash.appspot.com/fetch_milestone_schedule?mstone=<N|current|next|previous>
```

- Returns `{"mstones": [ { ... } ]}` — **one milestone per request** (no working range param).
- Fields: `mstone`, `branch_point`, `stable_date`, `late_stable_date`, `feature_freeze`,
  `earliest_beta`, `final_beta` (ISO-8601, midnight UTC).

No authentication, no documented rate limit (be polite: ~0.3s between calls).

## Script Usage

```bash
# Lean heads-up to stdout (main vs Beta + upcoming schedule)
python3 .agents/skills/security-audit/scripts/query-milestone-schedule.py

# + structured JSON for the audit report
python3 .agents/skills/security-audit/scripts/query-milestone-schedule.py \
  --output output/ai/milestone-schedule-cache.json

# Print JSON instead of a table
python3 .agents/skills/security-audit/scripts/query-milestone-schedule.py --json
```

The script reads `main`'s milestone + major from `scripts/VERSIONS.txt`
(`libSkiaSharp milestone NNN` and `SkiaSharp nuget <major>.NNN.x`), fetches the channels
(`platform=Windows`, which carries all five) and the upcoming schedule, and emits the
main-vs-Beta heads-up. Progress is logged to stderr, so stdout/`--output` stay clean.

### Flags

| Flag | Default | Purpose |
|------|---------|---------|
| `--ahead N` | `4` | How many milestones past main to include in the schedule |
| `--window N` | `14` | Days-ahead threshold for schedule `watch` alerts |
| `--json` | off | Print JSON to stdout instead of a table |
| `--output PATH` | — | Write the structured JSON (how the audit consumes it) |

## Output Structure

```jsonc
{
  "meta": {
    "main_milestone": 150,
    "main_milestone_source": "scripts/VERSIONS.txt",
    "major": 4,
    "beta_milestone": 150,
    "status": "current",          // "current" (main >= Beta), "behind" (main < Beta), or "unknown" (Beta lookup failed)
    "platform": "Windows",
    "window_days": 14
  },
  "channels": [
    { "channel": "Beta", "milestone": 150, "version": "150.0.7871.24",
      "skia_hash": "9f330f170430...", "date": "2026-06-..." }
  ],
  "upcoming": [
    { "milestone": 151, "is_main": false, "channels": ["Dev", "Canary"],
      "dates": { "branch_point": { "date": "2026-06-29", "days_from_now": 12 },
                 "stable_date":  { "date": "2026-07-28", "days_from_now": 41 } } }
  ],
  "headsup": [
    { "level": "ok", "milestone": 150, "message": "main (m150) is at or ahead of Beta..." }
  ],
  "support": {
    "configured": true,
    "stable_lines": ["4.148"],
    "preview_lines": ["4.150"],
    "chrome": { "extended": 148, "stable": 149, "beta": 150 },
    "status": "ok",                 // "ok" | "warn" | "drift" | "unknown" | "absent"
    "alerts": [
      { "level": "ok", "message": "support.stable is on Extended-stable m148 while preview m150..." }
    ]
  }
}
```

### Heads-up alert levels

| Level | Icon | Trigger |
|-------|------|---------|
| `critical` | 🔴 | `main < Beta` **and** a milestone newer than main already ships on a stable-class channel (Stable/Extended) — the bump is overdue and reaching non-preview users. |
| `urgent` | 🟠 | `main < Beta` — the front line is behind; bump main to the Beta milestone. |
| `unknown` | ❓ | The Beta channel milestone could not be read (Chromium Dash unavailable) — the signal could not be evaluated. **Do not treat as OK.** |
| `watch` | 🟡 | A milestone past main **branches within the window** — start preparing. |
| `ok` | 🟢 | `main >= Beta` — front line current. |
| `info` | 🔵 | Other context. |

> The `critical` trigger uses the **live** Stable/Extended channel milestones from `fetch_releases`
> (authoritative), falling back to the scheduled stable date only when channel data is missing.

## Support-Tier Drift (release-notes `support` block)

The same run also drift-checks the release-notes **support paths** in
`scripts/infra/docs/versions.json` against the live channels. That block is two
hand-maintained lists of `major.minor` lines SkiaSharp actually ships — `stable` (the
supported stable line) and `preview` (the in-flight preview line) — and drives the
website's TOC/index grouping (see the release-notes spec §3.5). SkiaSharp ships NuGet
packages, not a multi-tier channel product, so this is **two lists, not a mirror of
Chrome's five channels**.

This is **detection only**: the fix is always a manual edit of `versions.json` (we don't
ship every Chrome milestone, so the block must not be auto-derived). With `E ≤ S ≤ B` =
Chrome Extended/Stable/Beta milestones and `stable*`/`preview*` = our newest stable/preview
milestone:

| Condition | Verdict |
|-----------|---------|
| `stable* = S` | 🟢 ok — current stable |
| `stable* = E` (E<S) **and** `preview* ≥ S` | 🟢 ok — promotion gap (preview about to ship) |
| `stable* = E` (E<S) **and** `preview* < S` | 🔴 drift — stuck on extended, nothing promoting |
| `stable* < E`, between `E` and `S`, or any stable entry off `{E,S}` | 🔴 drift — out of date / off-channel |
| `stable* > S` | 🟡 warn — ahead of Chrome stable, verify |
| `preview* = B` or `preview* > B` | 🟢 ok — tracks beta / ahead in Dev/Canary |
| `S < preview* < B` | 🟡 warn — trails beta, update soon |
| `preview* ≤ S` | 🔴 drift — not a real preview |
| `preview` empty | 🟡 warn — no preview documented |

The verdict lands in the `support` object of the JSON (`status` = `ok`/`warn`/`drift`/
`unknown`/`absent`) and prints under **"Support tiers (versions.json)"**. A `drift` verdict
is an audit finding: the website is mis-stating what is supported — fix it by editing the
`support` lists to the milestones we actually released.

## How the Audit Uses This

Run in **Step 3** (right after the Chrome Releases blog query):

- **Where we are vs what's coming** — `meta.status` + the `upcoming` table answer it directly.
- **Escalate bumps** — if `status == "behind"` (or a `watch` milestone) also carries HIGH/CRITICAL
  Skia CVEs from the Chrome Releases / NVD pass, raise it in `nextSteps` with the stable date as
  the deadline.
- **Within-milestone backports** — for whether a shipped `release/*.x` line is missing a
  cherry-picked Skia security fix, use the [Skia CVE resolution](skia-cve-resolution.md) process
  (merge-base ancestry), not this tool.

The output is advisory context; it need not be embedded in the structured report schema, but its
`critical`/`urgent` findings should inform the prose summary and `nextSteps`.
