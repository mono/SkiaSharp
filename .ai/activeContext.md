# Active Context

> Current focus, recent changes, immediate next steps, and working patterns.
> **Update this file frequently** - it's the "working memory" for AI assistants.

## Current Focus

**Phase**: Phase 4 - Data Cache Architecture (COMPLETE ✅)
**Status**: All infrastructure complete, sync runs hourly + on push

## Recent Changes

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
   - SkiaSharp Core, .NET MAUI Views, HarfBuzzSharp
   - Skottie Animation, GPU Backends (Direct3D + Vulkan), Blazor
2. ✅ Store ALL versions with publish dates
3. ✅ New `nuget-charts.json` output file
4. ✅ Sorting filters on Issues page (6 options)
5. ✅ Sorting filters on PRs page (7 options)
6. ✅ Fixed engagement loop infinite commits (exit code 2)

### 2026-02-04 (Community Sync & Documentation)
1. ✅ Updated .ai/techContext.md with CLI commands
2. ✅ Updated .ai/architecture.md with 4-step workflow
3. ✅ Updated copilot-instructions.md with cache structure
4. ✅ Updated progress.md with v0.8.0

### 2026-02-04 (Community Sync Implementation)
1. ✅ **Community sync feature**:
   - New `sync community` command
   - Fetches top 100 contributors (1 API call)
   - Checks MS membership for top 20 (20 API calls)
   - Stores in `community/contributors.json`
2. ✅ **Repo stats in cache**:
   - GitHub sync saves stars/forks/watchers to `github/repo.json`
   - Generate reads from cache (no more hardcoded zeros)
3. ✅ Removed Recent Commits from Community page

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
