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
4. ✅ Added progress indicators with percentage, count, and ETA
5. ✅ Restructured workflow: NuGet first, then GitHub
6. ✅ NuGet now syncs every 6 hours (0, 6, 12, 18 UTC)
7. ✅ Index.json now sorted by number (stable for diffs)
8. ✅ Updated all documentation

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
                    └─ 10 batches of 25 items ──→ push each
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
dotnet run -- sync github --cache-path ./cache --engagement-only --engagement-count 25
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

### Progress Indicators
```
Fetching... 34% (1,200/3,474) ~1m 42s remaining
✓ 3,474 items synced in 2m 31s
```

### Layered Sync Strategy
- **Layer 1**: Basic item data (all issues/PRs) - ~35 pages
- **Layer 2**: Engagement data - 25 items/batch, 10 batches/run

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
