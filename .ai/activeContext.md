# Active Context

> Current focus, recent changes, immediate next steps, and working patterns.
> **Update this file frequently** - it's the "working memory" for AI assistants.

## Current Focus

**Phase**: Phase 6 - Multi-Repository Support (v0.11.0)
**Status**: In Progress - Infrastructure complete, GenerateCommand needs update

## Recent Changes

### 2026-02-05 (Multi-Repository Support - v0.11.0)
1. ✅ **Extended cache to support multiple repositories**:
   - New `ReposConfig` model with list of `RepoDefinition` entries
   - Cache now uses repo-scoped folders: `mono-SkiaSharp/`, `mono-SkiaSharp.Extended/`, `mono-skia/`
   - `CacheService` updated to support repo scoping via `ForRepo()` factory method
2. ✅ **Updated models with repo field**:
   - `IssueInfo` now includes `Repo` and `RepoShortName` fields
   - `PullRequestInfo` now includes `Repo` and `RepoShortName` fields
   - Added `RepoCount` record for aggregation
   - `IssuesData` and `PrTriageData` now include `ByRepo` for filtering
3. ✅ **Updated sync commands**:
   - `SyncGitHubCommand` uses repo-scoped cache path
   - `SyncCommunityCommand` uses repo-scoped cache path
   - `SyncNuGetCommand` uses repo-scoped cache path (primary repo only)
4. 🚧 **GenerateCommand** (partial):
   - Currently processes primary repo only (mono/SkiaSharp)
   - Models include repo field set to "mono/SkiaSharp"
   - **TODO**: Refactor to load data from all repo folders and merge
5. ✅ **Updated Dashboard UI**:
   - Added Repository filter dropdown to Issues page
   - Added Repository filter dropdown to Pull Requests page
   - Updated filter logic and URL query params
6. ✅ **Updated sync workflow**:
   - Syncs GitHub items from all 3 repos
   - Syncs community from SkiaSharp and SkiaSharp.Extended
   - Syncs NuGet from SkiaSharp only
   - Engagement sync from primary repo (SkiaSharp) due to rate limits

### Next Steps
1. Refactor `GenerateCommand` to:
   - Load repos config
   - Iterate through all repo folders
   - Merge issues/PRs with correct repo field from each folder
   - Aggregate stats across repos

### 2026-02-05 (Issue/PR Trend Charts - v0.10.0) ✅
1. ✅ **Added `Merged` and `MergedAt` fields to PR sync**:
   - Updated CacheModels with new fields
   - Updated SyncGitHubCommand to fetch from API
   - Added `ClosedAt` to IndexItem for trend calculations
2. ✅ **New `github-trends.json` output**:
   - Issue stats: total, closed, open, closure rate, avg days to close
   - PR stats: total, merged, closed, open, merge rate, avg days to merge
   - Monthly trends (all-time history, ~120 months)
3. ✅ **Issues page enhancements**:
   - 6 stats cards (total created, closed, open, rate, avg days, oldest)
   - Monthly activity line chart (created vs closed)
   - Time range dropdown (6mo, 1yr, 2yr, 5yr, all time)
4. ✅ **PRs page enhancements**:
   - 6 stats cards (total opened, merged, closed, open, rate, avg days)
   - Monthly activity line chart (opened vs merged)
   - Time range dropdown (6mo, 1yr, 2yr, 5yr, all time)
5. ✅ **Full sync completed**: 941 merged PRs, 2,472 items with closedAt

### 2026-02-04 (NuGet.Protocol SDK Refactor - v0.9.1)
1. ✅ **Refactored to NuGet.Protocol SDK**:
   - Replaced raw HTTP calls with official NuGet SDK
   - PackageSearchResource: All versions + download counts
   - PackageMetadataResource: All versions + publish dates
   - Cleaner, more maintainable code
2. ✅ **All versions now have downloads**:
   - 145 SkiaSharp versions (was 5)
   - Previews now show real download counts
   - No more 0-download entries

### 2026-02-04 (NuGet Download Trend Charts - v0.9.0)
1. ✅ **6 cumulative download charts** on NuGet page:
   - SkiaSharp, HarfBuzzSharp, .NET MAUI Views
   - Skottie Animation, GPU Backends (Direct3D + Vulkan), Blazor
2. ✅ Store ALL versions with publish dates
3. ✅ New `nuget-charts.json` output file
4. ✅ Sorting filters on Issues page (6 options)
5. ✅ Sorting filters on PRs page (7 options)
6. ✅ Fixed engagement loop infinite commits (exit code 2)

### 2026-02-04 (Checkpoint-Based Engagement)
1. ✅ Changed engagement batch size: 25 → 100
2. ✅ CLI returns exit code 1 when rate limit hit
3. ✅ Workflow uses while-loop with checkpoint commits
4. ✅ Fixed TriageCategory enum (added Untriaged, Draft)

### 2026-02-04 (Data Cache Architecture)
1. ✅ Branch renamed: `dashboard` → `docs-dashboard`
2. ✅ Created `docs-data-cache` orphan branch
3. ✅ Implemented sync commands (github, nuget)
4. ✅ Engagement scoring with hot issue detection

## Architecture Summary

```
Push to docs-dashboard
         │
         ▼
sync-data-cache.yml (hourly + on push)
         │
         ├─ Step 1a: NuGet (every 6h) ──→ push
         ├─ Step 1b: Community (every 6h) ──→ push
         │
         ├─ Step 2: GitHub items (Layer 1) ──→ push
         │
         └─ Step 3: GitHub engagement (Layer 2)
                    └─ while loop: 100 items ──→ push ──→ repeat until rate limit
```

### Cache Structure (`docs-data-cache` branch)
```
docs-data-cache/
├── github/
│   ├── repo.json           # Stars, forks, watchers
│   ├── sync-meta.json      # Sync state, rate limits
│   ├── index.json          # All issues + PRs
│   └── items/*.json        # Full item + engagement
├── community/
│   ├── sync-meta.json
│   └── contributors.json   # Top 100 with MS flag
├── nuget/
│   ├── sync-meta.json
│   ├── index.json
│   └── packages/*.json
```

### CLI Commands
```bash
# Sync commands (populate cache)
dotnet run -- sync github --cache-path ./cache --items-only
dotnet run -- sync github --cache-path ./cache --engagement-only --engagement-count 100
dotnet run -- sync community --cache-path ./cache
dotnet run -- sync nuget --cache-path ./cache

# Generate command (cache → JSON)
dotnet run -- generate --from-cache ./cache -o ./data
```

### Checkpoint Strategy
```bash
while true; do
  sync github --engagement-only --engagement-count 100
  git commit && push  # Checkpoint every 100 items
  if rate_limited or all_done; then break; fi
done
```
- Atomic file writes prevent JSON corruption
- Git commits provide checkpoints every 100 items
- Exit code 1 = rate limit hit (more work to do)

## Context for Next AI Session

When resuming work:
1. Read ALL files in `.ai/` folder first
2. Branch is `docs-dashboard` (renamed from `dashboard`)
3. Data cache is `docs-data-cache` branch
4. Live at https://mono.github.io/SkiaSharp/dashboard/
5. Sync runs hourly (NuGet/Community every 6h), build runs every 6 hours

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
