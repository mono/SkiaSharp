# Chromium Release Heads-Up (main vs Beta)

[Chromium Dash](https://chromiumdash.appspot.com/) exposes two JSON endpoints we combine to
answer one question: **are we keeping up with what's coming, and how much time do we have?**

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

## Two Axes (only one needs tracking by default)

| Axis | Question | What it needs |
|------|----------|---------------|
| **1. Milestone coverage** (default) | Are we keeping up with the front line? | `main` milestone + Beta channel milestone. Nothing else. |
| **2. Within-milestone backports** (`--check-backports`) | Does a shipped `release/*.x` line carry the latest Skia *for its milestone* (e.g. a cherry-picked CVE fix in `149.0.7827.x`)? | Resolve `release/<major>.<M>.x`, read its Skia submodule SHA, compare against the channel's current `hashes.skia` via the Skia CVE process. |

Axis 1 is the heads-up. Axis 2 is a deeper, opt-in audit check.

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

# Axis 2: also resolve release/<major>.<M>.x lines and their Skia SHA
python3 .agents/skills/security-audit/scripts/query-milestone-schedule.py --check-backports

# Print JSON instead of a table
python3 .agents/skills/security-audit/scripts/query-milestone-schedule.py --json
```

The script reads `main`'s milestone + major from `scripts/VERSIONS.txt`
(`libSkiaSharp milestone NNN` and `SkiaSharp nuget <major>.NNN.x`), fetches the channels and the
upcoming schedule, and emits the main-vs-Beta heads-up.

### Flags

| Flag | Default | Purpose |
|------|---------|---------|
| `--current N` | from VERSIONS.txt | Override main's milestone |
| `--platform` | `Windows` | Channel platform (Windows has all channels) |
| `--ahead N` | `4` | How many milestones past main to include in the schedule |
| `--window N` | `14` | Days-ahead threshold for schedule `watch` alerts |
| `--no-channels` | off | Schedule-only (disables the main-vs-Beta signal) |
| `--check-backports` | off | Resolve each tracked channel's `release/<major>.<M>.x` line + Skia SHA |
| `--track` | `Extended,Stable` | Channels to resolve in `--check-backports` (Beta == main) |
| `--json` | off | Print JSON to stdout instead of a table |
| `--output PATH` | — | Write the structured JSON |
| `--repo-root` / `--verbose` | auto / off | Path override, progress detail |

## Output Structure

```jsonc
{
  "meta": {
    "main_milestone": 150,
    "main_milestone_source": "scripts/VERSIONS.txt",
    "major": 4,
    "beta_milestone": 150,
    "status": "current",          // "current" if main >= Beta, else "behind"
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
  // only with --check-backports:
  "backports": [
    { "channel": "Extended", "channel_milestone": 148, "channel_skia": "3a90f6662a2c...",
      "branch": "origin/release/4.148.x", "branch_exists": true,
      "branch_milestone": 148, "branch_skia_fork": "1a155bae3ac8..." }
  ]
}
```

### Heads-up alert levels

| Level | Icon | Trigger |
|-------|------|---------|
| `critical` | 🔴 | `main < Beta` **and** the Beta milestone is already stable — the bump is overdue and shipping to stable users. |
| `urgent` | 🟠 | `main < Beta` — the front line is behind; bump main to the Beta milestone. |
| `watch` | 🟡 | A milestone past main **branches within the window** — start preparing. |
| `ok` | 🟢 | `main >= Beta` — front line current. |
| `info` | 🔵 | Other context. |

> ⚠️ `backports` reports the **mono/skia fork** submodule SHA, which is not directly comparable to
> the channel's upstream `hashes.skia`. Use it to confirm a release line exists and to feed the
> Skia CVE resolution process (merge-base check) — not as a literal SHA-equality test.

## How the Audit Uses This

Run in **Step 1.6** (right after the Chrome Releases blog query):

- **Where we are vs what's coming** — `meta.status` + the `upcoming` table answer it directly.
- **Escalate bumps** — if `status == "behind"` (or a `watch` milestone) also carries HIGH/CRITICAL
  Skia CVEs from the Chrome Releases / NVD pass, raise it in `nextSteps` with the stable date as
  the deadline.
- **Deep check (optional)** — run `--check-backports` when auditing whether a shipped
  `release/*.x` line is missing a within-milestone Skia security backport.

The output is advisory context; it need not be embedded in the structured report schema, but its
`critical`/`urgent` findings should inform the prose summary and `nextSteps`.
