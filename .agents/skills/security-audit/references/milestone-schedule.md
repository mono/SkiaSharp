# Chromium Milestone Schedule (Release Heads-Up)

The [Chromium Dash schedule](https://chromiumdash.appspot.com/schedule) publishes the release
calendar for every Chrome/Chromium milestone — when each milestone branches from trunk and when
it reaches the stable channel.

Because **Skia milestones track Chrome milestones** (our submodule pins `chrome/mNNN`), this
calendar tells us *when* the next Skia milestone we need to bump to goes stable. That makes it an
early-warning signal: if a milestone we still owe a bump for is about to go stable — or already
has — security fixes are reaching users before SkiaSharp ships them.

> ℹ️ This is **not** a security check on its own. It is scheduling context. Pair it with the
> Chrome Releases blog (CVE disclosure) and the Skia CVE resolution process to decide urgency.

## Why This Matters for SkiaSharp

1. **Bump timing:** A bump that lands *after* the milestone goes stable means a window where
   known CVEs fixed upstream are unpatched in SkiaSharp. The schedule tells us the deadline.
2. **Planning:** Branch points and stable dates are fixed weeks ahead. We can start a bump as
   soon as a milestone branches instead of reacting to a CVE.
3. **Release coordination:** If a SkiaSharp release is planned for a given week, this surfaces
   which Skia milestone should be in it.

## Data Access

- **Endpoint:** `https://chromiumdash.appspot.com/fetch_milestone_schedule?mstone=<value>`
- **`mstone` accepts:** a milestone number (e.g. `150`), or the keywords `current`, `next`,
  `previous`. Invalid values return a pydantic validation error.
- **Returns:** `{"mstones": [ { ... } ]}` — **one milestone per request**. There is no documented
  bulk/range parameter (`num_milestones` and `mstone=all` do not return ranges), so the script
  iterates one number at a time.
- **No authentication; no documented rate limit** (be polite: ~0.3s between calls).

### Milestone object fields (dates are ISO-8601, midnight UTC)

| Field | Meaning |
|-------|---------|
| `mstone` | Milestone number (e.g. `150`) |
| `branch_point` | When the release branch is cut from trunk — earliest a stable Skia `chrome/mNNN` branch exists |
| `stable_date` | When the milestone reaches the **stable** channel (the deadline that matters most) |
| `late_stable_date` | End of the stable window / first refresh boundary |
| `feature_freeze`, `earliest_beta`, `final_beta` | Earlier cycle dates (context) |
| `owners`, `ldaps` | Google release TPMs (not relevant to us) |

## Script Usage

```bash
# Heads-up relative to the milestone pinned in scripts/VERSIONS.txt
python3 .agents/skills/security-audit/scripts/query-milestone-schedule.py \
  --output output/ai/milestone-schedule-cache.json

# Look further ahead / behind, widen the urgency window to 3 weeks
python3 .agents/skills/security-audit/scripts/query-milestone-schedule.py \
  --behind 1 --ahead 6 --window 21 --verbose \
  --output output/ai/milestone-schedule-cache.json

# Override the "current" milestone (skip reading VERSIONS.txt)
python3 .agents/skills/security-audit/scripts/query-milestone-schedule.py --current 150
```

The script determines the **current** Skia milestone from `scripts/VERSIONS.txt`
(`libSkiaSharp  milestone  NNN`, falling back to `skia  release  mNNN`), fetches a window of
milestones around it, computes day-deltas against today, and emits prioritized heads-up alerts.

## Output Structure

```jsonc
{
  "meta": {
    "generated_at": "2026-06-16T...",
    "current_milestone": 150,
    "current_milestone_source": "scripts/VERSIONS.txt",
    "window_days": 14
  },
  "milestones": [
    {
      "milestone": 151,
      "status": "pending",          // "bumped" (<= current) or "pending" (> current)
      "is_current": false,
      "dates": {
        "branch_point": { "date": "2026-06-29", "days_from_now": 13 },
        "stable_date":  { "date": "2026-07-28", "days_from_now": 42 }
      }
    }
  ],
  "headsup": [
    { "level": "watch", "milestone": 151, "message": "..." }
  ]
}
```

### Heads-up alert levels

| Level | Icon | Trigger |
|-------|------|---------|
| `critical` | 🔴 | A **pending** milestone (we haven't bumped to it) is **already stable**. CVEs fixed upstream are live for users with no SkiaSharp bump. |
| `urgent` | 🟠 | A pending milestone goes stable **within the window** (default 14 days). The bump should be ready. |
| `watch` | 🟡 | A pending milestone **branches within the window**. Good time to start the bump. |
| `info` | 🔵 | The current milestone's stable window is ending; the next milestone takes over soon. |

## How the Audit Uses This

Run this in **Step 1.6** of the audit (right after the Chrome Releases blog query). Use it to:

- **Prioritize Skia bump recommendations** — if a milestone with HIGH/CRITICAL CVEs (from the
  Chrome Releases / NVD passes) is also `critical`/`urgent` on the schedule, escalate it in
  `nextSteps`.
- **Add timing to the report** — when recommending a Skia bump, cite the target milestone's
  stable date so the reader knows the deadline.
- **Catch silent gaps** — a `critical` heads-up means we are already behind a stable milestone
  even if no GitHub issue has been filed yet.

The script output is advisory context; it does not need to be embedded in the structured report
schema, but its `critical`/`urgent` findings should inform the prose summary and `nextSteps`.
