# Active Context

> Current focus, recent changes, immediate next steps, and working patterns.
> **Update this file frequently** - it's the "working memory" for AI assistants.

## Current Focus

**Phase**: Phase 4 - Data Cache Architecture (COMPLETE ✅)
**Status**: All infrastructure complete, awaiting first scheduled sync

## Recent Changes

### 2026-02-04 (Data Cache Architecture - COMPLETE)
1. ✅ Branch renamed: `dashboard` → `docs-dashboard`
2. ✅ Created `docs-data-cache` orphan branch for cached API data
3. ✅ Updated `build-dashboard.yml` to use `generate --from-cache`
4. ✅ Created `sync-data-cache.yml` workflow (hourly sync)
5. ✅ Implemented `sync` commands (github, nuget, all)
6. ✅ Implemented `generate` command (cache → dashboard JSON)
7. ✅ Engagement scoring with hot issue detection
8. ✅ Dashboard UI updates (hot issues on Home and Issues pages)
9. ✅ Generate command always writes files (even when cache is empty)

**Note**: The sync workflow runs on schedule (hourly at :00). Cannot be manually triggered until workflow file exists on default branch.

### New CLI Commands
```bash
# Cache-based workflow
dotnet run -- sync github -c ./cache    # Layer 1 + Layer 2
dotnet run -- sync nuget -c ./cache     # NuGet packages
dotnet run -- sync all -c ./cache       # All sources
dotnet run -- generate -c ./cache -o ./data  # Generate dashboard JSON

# Legacy direct-API commands still available
dotnet run -- all -o ./data
```

### Architecture Overview
```
HOURLY (sync-data-cache.yml):
  GitHub API → sync command → docs-data-cache branch
  NuGet API  →

EVERY 6 HOURS (build-dashboard.yml):
  docs-data-cache → generate command → dashboard JSON → deploy
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

### Error Handling
- Proactive rate limit checking (stop if < 100 remaining)
- Skip list for failed items with cooldown periods
- Resume from checkpoint on next run

### Engagement Scoring
- Formula: `(Comments × 3) + (Reactions × 1) + (Contributors × 2) + (1/DaysSinceActivity) + (1/DaysOpen)`
- Hot detection: Current score > Historical score (7 days ago)

## Context for Next AI Session

When resuming work:
1. Read ALL files in `.ai/` folder first
2. Branch is `docs-dashboard` (renamed from `dashboard`)
3. Data cache is `docs-data-cache` branch
4. Live at https://mono.github.io/SkiaSharp/dashboard/
5. **NEXT**: Add hot issues to dashboard UI (Issues page + Home page)

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
