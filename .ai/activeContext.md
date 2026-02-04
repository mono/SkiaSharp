# Active Context

> Current focus, recent changes, immediate next steps, and working patterns.
> **Update this file frequently** - it's the "working memory" for AI assistants.

## Current Focus

**Phase**: Phase 4 - Data Cache Architecture (COMPLETE ✅)
**Status**: All infrastructure complete, sync runs hourly + on push

## Recent Changes

### 2026-02-04 (Data Cache Architecture - COMPLETE)
1. ✅ Branch renamed: `dashboard` → `docs-dashboard`
2. ✅ Created `docs-data-cache` orphan branch for cached API data
3. ✅ Updated `build-dashboard.yml` to use `generate --from-cache`
4. ✅ Created `sync-data-cache.yml` workflow (hourly + on push)
5. ✅ Implemented `sync` commands (github, nuget, all)
6. ✅ Implemented `generate` command (cache → dashboard JSON)
7. ✅ Engagement scoring with hot issue detection
8. ✅ Dashboard UI updates (hot issues on Home and Issues pages)
9. ✅ Generate command always writes files (even when cache is empty)
10. ✅ Added push trigger to sync workflow

## Architecture Summary

```
Push to docs-dashboard
         │
         ├──→ sync-data-cache.yml (triggered)
         │         │
         │         ▼
         │    docs-data-cache (updated)
         │
         └──→ build-dashboard.yml (NOT triggered on push)
                   │
                   ▼ (runs every 6 hours)
              docs-live/dashboard/ (deployed)
```

### Workflows
| Workflow | Triggers | Purpose |
|----------|----------|---------|
| `sync-data-cache.yml` | Hourly, push to docs-dashboard | Sync APIs → cache |
| `build-dashboard.yml` | Every 6 hours, manual | Cache → JSON → deploy |

### CLI Commands
```bash
# Cache-based workflow (production)
dotnet run -- sync --cache-path ./cache           # Layer 1 + Layer 2
dotnet run -- sync github --cache-path ./cache    # GitHub only
dotnet run -- sync nuget --cache-path ./cache     # NuGet only
dotnet run -- generate --from-cache ./cache -o ./data

# Legacy direct-API commands (still work)
dotnet run -- all -o ./data
```

### Cache Structure (`docs-data-cache` branch)
```
docs-data-cache/
├── github/
│   ├── sync-meta.json       # Sync state, rate limits, skip list
│   ├── index.json           # All issues + PRs (lightweight)
│   └── items/{number}.json  # Full data + engagement per item
├── nuget/
│   ├── sync-meta.json
│   ├── index.json
│   └── packages/{id}.json
```

### Layered Sync Strategy
- **Layer 1**: Basic item data (all issues/PRs) - ~15 API calls
- **Layer 2**: Engagement data (comments, reactions) - 50 items/run, builds up over time

### Engagement Scoring
- Formula: `(Comments × 3) + (Reactions × 1) + (Contributors × 2) + (1/DaysSinceActivity) + (1/DaysOpen)`
- Hot detection: Current score > Historical score (7 days ago) AND score > 5

### Error Handling
- Proactive rate limit checking (stop if < 100 remaining)
- Skip list for failed items with cooldown periods
- Resume from checkpoint on next run

## Context for Next AI Session

When resuming work:
1. Read ALL files in `.ai/` folder first
2. Branch is `docs-dashboard` (renamed from `dashboard`)
3. Data cache is `docs-data-cache` branch
4. Live at https://mono.github.io/SkiaSharp/dashboard/
5. Sync runs on push AND hourly, build runs every 6 hours

## Previous Completed Phases

### Phase 3 - Collector CLI (COMPLETE)
- Converted 5 PowerShell scripts to .NET CLI
- Commands: `all`, `github`, `nuget`, `community`, `issues`, `pr-triage`
- Spectre.Console UI with progress bars

### Phase 2 - Dashboard Features (COMPLETE)
- NuGet page with grouped layout, 50 packages, legacy toggle
- SPA routing fixed with spa-github-pages approach
- Charts with ApexCharts
- Filters with URL query params
