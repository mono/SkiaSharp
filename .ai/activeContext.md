# Active Context

> Current focus, recent changes, immediate next steps, and working patterns.
> **Update this file frequently** - it's the "working memory" for AI assistants.

## Current Focus

**Phase**: Phase 4 - Data Cache Architecture (IN PROGRESS)
**Status**: Implementing cache-based sync system

## Recent Changes

### 2026-02-04 (Data Cache Architecture - IN PROGRESS)
1. ✅ Branch renamed: `dashboard` → `docs-dashboard`
2. ✅ Created `docs-data-cache` orphan branch for cached API data
3. ✅ Updated `build-dashboard.yml` to use `generate --from-cache`
4. ✅ Created `sync-data-cache.yml` workflow (hourly sync)
5. 🔄 Implementing `sync` and `generate` commands in collector
6. ⏳ Engagement scoring (hot issues) - pending

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

## Context for Next AI Session

When resuming work:
1. Read ALL files in `.ai/` folder first
2. Branch is `docs-dashboard` (renamed from `dashboard`)
3. Data cache is `docs-data-cache` branch
4. Live at https://mono.github.io/SkiaSharp/dashboard/
5. **NEXT**: Implement `SyncCommand` and `GenerateCommand` in collector

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
