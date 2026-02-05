# Active Context

> Current focus, recent changes, immediate next steps, and working patterns.
> **Update this file frequently** - it's the "working memory" for AI assistants.

## Current Focus

**Phase**: Multi-Repository Dashboard Extension ✅ COMPLETE
**Status**: Full sync complete, dashboard live with both repos
**Version**: v0.11.0

## Recent Changes

### 2026-02-05 (Multi-Repository Extension - v0.11.0)

**Data Sync Complete ✅**
- Migrated existing cache data to `repos/mono-SkiaSharp/` structure
- Triggered full parallel sync for both repos
- SkiaSharp: 3,183 issues/PRs, 50 NuGet packages synced
- Extended: 329 issues/PRs, 4 NuGet packages synced
- Dashboard build deployed with merged multi-repo data

**Live Dashboard Stats**:
| Metric | Total | SkiaSharp | Extended |
|--------|-------|-----------|----------|
| Stars | 5,514 | 5,257 | 257 |
| Open Issues | 690 | 658 | 32 |
| Open PRs | 70 | 52 | 18 |
| NuGet Downloads | 984M | 50 pkgs | 4 pkgs |
| Contributors | 105 | (merged) | (merged) |

**Phase 1: Cache Restructure ✅**
- Created `config.json` with repo list (SkiaSharp + Extended)
- Created `ConfigModels.cs` and `ConfigService.cs`
- Updated `CacheService` for per-repo paths: `repos/{key}/github/`, `repos/{key}/nuget/`
- Contributors moved to `github/` folder (it's GitHub API data)

**Phase 2: Multi-Repo Sync ✅**
- Updated all sync commands to accept `--repo owner/name` argument
- `SyncGitHubCommand`, `SyncCommunityCommand`: per-repo cache paths
- `NuGetService`: Added `DiscoverPackagesAsync()` with two strategies:
  - `versions-txt`: Parse VERSIONS.txt files for SkiaSharp
  - `nuget-search`: Search NuGet API for `SkiaSharp.Extended*` by Microsoft
- `SyncNuGetCommand`: Uses per-repo config and cache paths

**Phase 3: Parallel Workflow ✅**
- Split `sync-data-cache.yml` into parallel jobs:
  - `sync-skiasharp`: Syncs mono/SkiaSharp
  - `sync-extended`: Syncs mono/SkiaSharp.Extended
- Added rebase-retry push logic (different folders = no conflicts)
- Added `--repo` filter in workflow_dispatch for testing

**Phase 4: Generate Consolidation ✅**
- Updated `GenerateCommand` to discover repos from `repos/*/` structure
- Added `repo`, `repoSlug`, `repoColor` fields to issues/PRs
- Merged contributors (deduplicate by login, track per-repo contributions)
- Per-repo breakdown in all stats (byRepo dictionaries)
- MonthlyTrend includes per-repo breakdown for stacked charts

**Phase 5: Dashboard UI ✅**
- Updated all service models for multi-repo fields
- `MultiRepoGitHubStats` replaces `GitHubStats` with `Repos` + `Total`
- Home/Community pages use `Total` stats
- All models backward compatible (new fields optional)

## Architecture Summary

```
docs-dashboard/                      # SOURCE CODE BRANCH
├── src/SkiaSharp.Collector/
│   ├── config.json                  # Repo list + NuGet discovery settings
│   └── ...

docs-data-cache/                     # DATA CACHE BRANCH
└── repos/
    ├── mono-SkiaSharp/
    │   ├── github/
    │   │   ├── sync-meta.json
    │   │   ├── repo.json
    │   │   ├── contributors.json
    │   │   ├── index.json
    │   │   └── items/*.json
    │   └── nuget/
    │       ├── sync-meta.json
    │       ├── index.json
    │       └── packages/*.json
    └── mono-SkiaSharp.Extended/
        └── ... (same structure)
```

### Parallel Sync with Rebase-Retry
```yaml
jobs:
  sync-skiasharp:
    # Syncs to repos/mono-SkiaSharp/*
    # Uses rebase-retry for push conflicts
    
  sync-extended:  # Runs in PARALLEL
    # Syncs to repos/mono-SkiaSharp.Extended/*
    # Different folders = no git conflicts on rebase
```

### NuGet Discovery
| Repo | Method | Details |
|------|--------|---------|
| mono/SkiaSharp | versions-txt | Parse VERSIONS.txt from main + release/2.x |
| mono/SkiaSharp.Extended | nuget-search | Search `SkiaSharp.Extended*` by Microsoft |

## Context for Next AI Session

When resuming work:
1. Read ALL files in `.ai/` folder first
2. Branch is `docs-dashboard`
3. Data cache is `docs-data-cache` branch with `repos/` structure
4. Live at https://mono.github.io/SkiaSharp/dashboard/
5. Multi-repo sync runs in parallel (two jobs)

## Remaining Enhancements (Future)

- [ ] Repo filter dropdown on Issues/PRs pages
- [ ] Repo color badges on issue/PR table rows
- [ ] Stacked area charts for trends (using per-repo breakdown)
- [ ] Per-repo breakdown cards on Home page

## Previous Completed Phases

### Phase 5 - Issue/PR Trend Charts (v0.10.0) ✅
- Monthly activity charts with time range dropdown
- Stats cards for issues and PRs
- Merged/MergedAt fields for PR tracking

### Phase 4 - Data Cache Architecture ✅
- Separate cache branch with sync commands
- Engagement scoring with hot issue detection
- Checkpoint-based sync with rate limit handling

### Phase 3 - Collector CLI ✅
- .NET CLI replaces PowerShell scripts
- Commands: sync github/nuget/community, generate

### Phase 2 - Dashboard Features ✅
- NuGet page with grouped layout
- SPA routing fixed
- Charts with ApexCharts
