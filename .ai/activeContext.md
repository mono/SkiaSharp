# Active Context

> Current focus, recent changes, immediate next steps, and working patterns.
> **Update this file frequently** - it's the "working memory" for AI assistants.

## Current Focus

**Phase**: Phase 4 - Data Cache Architecture (COMPLETE ✅)
**Status**: All infrastructure complete, sync runs hourly + on push

## Recent Changes

### 2026-02-04 (Data Cache Architecture - Session 2)
1. ✅ Implemented 2-step sync with batched engagement
2. ✅ Added `--items-only` and `--engagement-only` flags
3. ✅ Fixed Spectre.Console markup escape bug (`[1/25]` → `(1/25)`)
4. ✅ Restructured workflow: NuGet first, then GitHub
5. ✅ NuGet now syncs every 6 hours (0, 6, 12, 18 UTC)
6. ✅ Index.json now sorted by number (stable for diffs)
7. ✅ **Checkpoint-based engagement sync**:
   - Default batch size changed from 25 to 100
   - CLI returns exit code 1 when rate limit hit
   - Workflow uses while-loop with git checkpoints
   - Runs until rate limit or all items synced
8. ✅ Updated all documentation and memory banks

### 2026-02-04 (Data Cache Architecture - Session 1)
1. ✅ Branch renamed: `dashboard` → `docs-dashboard`
2. ✅ Created `docs-data-cache` orphan branch
3. ✅ Created `sync-data-cache.yml` workflow
4. ✅ Implemented `sync github` and `sync nuget` commands
5. ✅ Implemented `generate` command
6. ✅ Engagement scoring with hot issue detection
7. ✅ Dashboard UI (hot issues on Home and Issues pages)

## Architecture Summary

```
Push to docs-dashboard
         │
         ▼
sync-data-cache.yml
         │
         ├─ Step 1: NuGet (every 6 hours only) ──→ push
         │
         ├─ Step 2: GitHub items (Layer 1) ──→ push
         │
         └─ Step 3: GitHub engagement (Layer 2)
                    │
                    └─ while loop: 100 items ──→ push ──→ repeat until rate limit
```

### Workflows
| Workflow | Triggers | Purpose |
|----------|----------|---------|
| `sync-data-cache.yml` | Hourly, push to docs-dashboard | Sync APIs → cache |
| `build-dashboard.yml` | Every 6 hours, manual | Cache → JSON → deploy |

### CLI Commands
```bash
# Sync commands (use in workflow)
dotnet run -- sync github --cache-path ./cache --items-only
dotnet run -- sync github --cache-path ./cache --engagement-only --engagement-count 100
dotnet run -- sync nuget --cache-path ./cache

# Generate command
dotnet run -- generate --from-cache ./cache -o ./data
```

### Cache Structure (`docs-data-cache` branch)
```
docs-data-cache/
├── github/
│   ├── sync-meta.json       # Sync state, rate limits, skip list
│   ├── index.json           # All issues + PRs (sorted by number)
│   └── items/{number}.json  # Full data + engagement per item
├── nuget/
│   ├── sync-meta.json
│   ├── index.json
│   └── packages/{id}.json
```

### Checkpoint Strategy (Fault Tolerance)
```bash
while true; do
  sync github --engagement-only --engagement-count 100
  git commit && push  # Checkpoint every 100 items
  if rate_limited or all_done; then break; fi
done
```
- Atomic file writes prevent JSON corruption
- Git commits provide checkpoints every 100 items
- Crash loses max 100 items of work, next run resumes

### Layered Sync Strategy
- **Layer 1**: Basic item data (all issues/PRs)
- **Layer 2**: Engagement data - loops until rate limit hit

### Smart Engagement Sync
Only fetches engagement for items where `UpdatedAt > EngagementSyncedAt`

### Engagement Scoring
- Formula: `(Comments × 3) + (Reactions × 1) + (Contributors × 2) + (1/DaysSinceActivity) + (1/DaysOpen)`
- Hot detection: Current score > Historical score (7 days ago) AND score > 5

## Context for Next AI Session

When resuming work:
1. Read ALL files in `.ai/` folder first
2. Branch is `docs-dashboard` (renamed from `dashboard`)
3. Data cache is `docs-data-cache` branch
4. Live at https://mono.github.io/SkiaSharp/dashboard/
5. Sync runs hourly (NuGet every 6h), build runs every 6 hours

## Previous Completed Phases

### Phase 3 - Collector CLI (COMPLETE)
- Converted 5 PowerShell scripts to .NET CLI
- Commands: `github`, `nuget`, `community`, `issues`, `pr-triage`
- Spectre.Console UI with progress bars

### Phase 2 - Dashboard Features (COMPLETE)
- NuGet page with grouped layout, 50 packages, legacy toggle
- SPA routing fixed with spa-github-pages approach
- Charts with ApexCharts
- Filters with URL query params
