# Chromium Milestone Schedule & Channel Tracking (Release Heads-Up)

The [Chromium Dash](https://chromiumdash.appspot.com/) project exposes two JSON endpoints we use
together:

1. **Milestone schedule** — when each milestone branches from trunk and reaches stable.
2. **Channel releases** — which milestone *and exact Skia commit* is live in each channel
   (Extended stable, Stable, Beta, Dev, Canary).

Because **Skia milestones track Chrome milestones** (our submodule pins `chrome/mNNN`), these tell
us *when* the next Skia milestone we need goes stable, and *which Skia commit* each channel sits
on right now. That makes them an early-warning + tracking signal: SkiaSharp maintains support for
**Extended stable, Stable, and Beta concurrently**, so we need to know the milestone and Skia
commit behind each of those channels at any time.

> ℹ️ This is **not** a security check on its own. It is scheduling + channel context. Pair it with
> the Chrome Releases blog (CVE disclosure) and the Skia CVE resolution process to decide urgency.

## Why This Matters for SkiaSharp

1. **Concurrent channel support:** We ship/maintain branches for Extended stable, Stable, and Beta
   at the same time. Each channel pins a *different* milestone and a *different* Skia commit. We
   must track all three so each branch carries the right Skia.
2. **Bump timing:** A bump that lands *after* a channel advances means a window where known CVEs
   fixed upstream are unpatched in that SkiaSharp branch. The schedule gives the deadline.
3. **Exact commit verification:** The channel endpoint returns `hashes.skia` — the precise upstream
   Skia commit in that channel — so we can verify a SkiaSharp branch points at the right merge base.
4. **Planning:** Branch points and stable dates are fixed weeks ahead; we can start a bump as soon
   as a milestone branches instead of reacting to a CVE.

## Data Access

### 1. Milestone schedule (dates)

- **Endpoint:** `https://chromiumdash.appspot.com/fetch_milestone_schedule?mstone=<value>`
- **`mstone` accepts:** a milestone number (e.g. `150`), or the keywords `current`, `next`,
  `previous`. Invalid values return a pydantic validation error.
- **Returns:** `{"mstones": [ { ... } ]}` — **one milestone per request**. There is no working
  bulk/range parameter (`num_milestones` and `mstone=all` do **not** return ranges), so the script
  iterates one number at a time.

#### Milestone object fields (dates are ISO-8601, midnight UTC)

| Field | Meaning |
|-------|---------|
| `mstone` | Milestone number (e.g. `150`) |
| `branch_point` | When the release branch is cut from trunk — earliest a stable Skia `chrome/mNNN` branch exists |
| `stable_date` | When the milestone reaches the **stable** channel (the deadline that matters most) |
| `late_stable_date` | End of the stable window / first refresh boundary |
| `feature_freeze`, `earliest_beta`, `final_beta` | Earlier cycle dates (context) |
| `owners`, `ldaps` | Google release TPMs (not relevant to us) |

### 2. Channel releases (milestone + Skia commit per channel)

- **Endpoint:** `https://chromiumdash.appspot.com/fetch_releases?channel=<Channel>&platform=<Platform>&num=1`
- **Channels:** `Extended` (extended stable), `Stable`, `Beta`, `Dev`, `Canary`.
- **Platform:** use `Windows` — it carries **all** channels (Extended stable and Canary are absent
  on Linux). `Mac` also has Extended stable.
- **Returns:** a JSON array of releases (newest first). The first element is the current release.

#### Release object fields (the ones we use)

| Field | Meaning |
|-------|---------|
| `channel` | `Extended` / `Stable` / `Beta` / `Dev` / `Canary` |
| `milestone` | Milestone number live in that channel |
| `version` | Full Chrome version string (e.g. `150.0.7871.24`) |
| `hashes.skia` | **Exact upstream Skia commit** in that channel — verify the SkiaSharp branch against this |
| `hashes.chromium` | Chromium commit (context) |
| `time` | Release timestamp (epoch **milliseconds**) |

Typical mapping (channels advance one milestone at a time): Extended < Stable < Beta < Dev ≈ Canary.

## Script Usage

```bash
# Heads-up + channel tracking relative to the milestone pinned in scripts/VERSIONS.txt
python3 .agents/skills/security-audit/scripts/query-milestone-schedule.py \
  --output output/ai/milestone-schedule-cache.json

# Track a different set of channels, widen the urgency window
python3 .agents/skills/security-audit/scripts/query-milestone-schedule.py \
  --track Extended,Stable,Beta --window 21 --verbose \
  --output output/ai/milestone-schedule-cache.json

# Schedule only (skip the channel lookup)
python3 .agents/skills/security-audit/scripts/query-milestone-schedule.py --no-channels

# Override the "current" milestone (skip reading VERSIONS.txt)
python3 .agents/skills/security-audit/scripts/query-milestone-schedule.py --current 150
```

The script determines the **current** Skia milestone from `scripts/VERSIONS.txt`
(`libSkiaSharp  milestone  NNN`, falling back to `skia  release  mNNN`), fetches each channel's live
release **and** a window of milestones around the current/channel milestones, computes day-deltas
against today, and emits prioritized heads-up alerts.

### Key flags

| Flag | Default | Purpose |
|------|---------|---------|
| `--platform` | `Windows` | Platform for channel releases (Windows has all channels) |
| `--channels` | `Extended,Stable,Beta,Dev,Canary` | Which channels to fetch |
| `--track` | `Extended,Stable,Beta` | Channels SkiaSharp supports concurrently (drives alerts) |
| `--window` | `14` | Days-ahead threshold for urgent/watch alerts |
| `--behind` / `--ahead` | `1` / `4` | Schedule window around the current milestone |
| `--no-channels` | off | Schedule-only mode |

## Output Structure

```jsonc
{
  "meta": {
    "generated_at": "2026-06-17T...",
    "current_milestone": 150,
    "current_milestone_source": "scripts/VERSIONS.txt",
    "window_days": 14,
    "platform": "Windows",
    "tracked_channels": ["Extended", "Stable", "Beta"]
  },
  "channels": [
    { "channel": "Extended", "milestone": 148, "version": "148.0.7778.271",
      "skia_hash": "3a90f6662a2c...", "date": "2026-06-...", "tracked": true },
    { "channel": "Beta", "milestone": 150, "version": "150.0.7871.24",
      "skia_hash": "9f330f170430...", "date": "2026-06-...", "tracked": true }
  ],
  "milestones": [
    {
      "milestone": 151,
      "status": "pending",          // "bumped" (<= current) or "pending" (> current)
      "is_current": false,
      "channels": ["Dev", "Canary"],
      "skia_hash": "fc2311fe7338...",
      "dates": {
        "branch_point": { "date": "2026-06-29", "days_from_now": 12 },
        "stable_date":  { "date": "2026-07-28", "days_from_now": 41 }
      }
    }
  ],
  "headsup": [
    { "level": "urgent", "milestone": 150, "channel": "Beta", "message": "..." }
  ]
}
```

### Heads-up alert levels

| Level | Icon | Trigger |
|-------|------|---------|
| `critical` | 🔴 | A **pending** milestone (we haven't bumped to it) is **already stable**. CVEs fixed upstream are live for users with no SkiaSharp bump. |
| `urgent` | 🟠 | A pending milestone goes stable **within the window** (default 14 days), **or** a tracked channel (Extended/Stable/Beta) has advanced past the pinned milestone and its branch needs a bump. |
| `watch` | 🟡 | A pending milestone **branches within the window**. Good time to start the bump. |
| `info` | 🔵 | A tracked channel matches/should pin a milestone, or the current milestone's stable window is ending. |

## How the Audit Uses This

Run this in **Step 1.6** of the audit (right after the Chrome Releases blog query). Use it to:

- **Track concurrent channels** — confirm each SkiaSharp branch (Extended/Stable/Beta) points at
  the Skia commit (`hashes.skia`) its channel currently ships.
- **Prioritize Skia bump recommendations** — if a milestone with HIGH/CRITICAL CVEs (from the
  Chrome Releases / NVD passes) is also `critical`/`urgent` on the schedule, escalate it in
  `nextSteps`.
- **Add timing to the report** — when recommending a Skia bump, cite the target milestone's
  stable date so the reader knows the deadline.
- **Catch silent gaps** — a `critical` heads-up means we are already behind a stable milestone,
  and an `urgent` channel alert means a tracked channel branch is behind, even if no GitHub issue
  has been filed yet.

The script output is advisory context; it does not need to be embedded in the structured report
schema, but its `critical`/`urgent` findings should inform the prose summary and `nextSteps`.
