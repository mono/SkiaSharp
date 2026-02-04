# Progress & Backlog

> Project status, completed work, pending tasks, and future ideas.

## Current Status

**Overall**: ✅ Phase 4 - Data Cache Architecture COMPLETE

| Area | Status | Notes |
|------|--------|-------|
| Phase 1 | ✅ Complete | Initial dashboard |
| Phase 2 | ✅ Complete | Restructure with charts |
| Phase 2.7 | ✅ Complete | NuGet grouped layout + legacy toggle |
| Phase 3 | ✅ Complete | .NET collector CLI replaces PowerShell |
| Phase 4 | ✅ Complete | Data cache with engagement scoring |
| Deployment | ✅ Working | https://mono.github.io/SkiaSharp/dashboard/ |

---

## Phase 4: Data Cache Architecture ✅ (Complete)

### Branch Setup ✅
- [x] Rename `dashboard` → `docs-dashboard`
- [x] Create `docs-data-cache` orphan branch
- [x] Update workflows for new architecture

### Cache Infrastructure ✅
- [x] `CacheModels.cs` - All cache record types
- [x] `CacheService.cs` - Read/write cache files, atomic writes
- [x] Skip list management with error types and cooldowns
- [x] Index.json sorted by item number (stable for diffs)
- [x] `github/repo.json` - Stars, forks, watchers
- [x] `community/contributors.json` - Top 100 with MS flag

### Sync Commands ✅
- [x] `sync github` - Layer 1 (items) + Layer 2 (engagement)
- [x] `sync nuget` - Package downloads and versions
- [x] `sync community` - Contributors + MS membership check
- [x] `--items-only` flag - Skip engagement (Layer 1 only)
- [x] `--engagement-only` flag - Skip items (Layer 2 only)
- [x] `--engagement-count` flag - Batch size (default: 100)
- [x] Proactive rate limit checking (exit code 1 when hit)
- [x] Smart engagement sync (only items updated since last sync)

### Generate Command ✅
- [x] Reads repo stats from `github/repo.json`
- [x] Reads contributors from `community/contributors.json`
- [x] Engagement scoring with hot issue detection
- [x] Always writes files, even with empty cache

### Dashboard UI ✅
- [x] Hot issues section on Issues page
- [x] Hot issues card on Home page (top 3)
- [x] Fixed TriageCategory enum (Untriaged, Draft)
- [x] Removed Recent Commits section (not needed)

### Workflow Structure ✅
- [x] 4-step workflow: NuGet → Community → GitHub items → GitHub engagement
- [x] NuGet + Community sync every 6 hours
- [x] GitHub items sync every hour
- [x] GitHub engagement: while loop, 100 items/checkpoint
- [x] Concurrency: `cancel-in-progress: false`

### Fault Tolerance ✅
- [x] Atomic file writes (write to .tmp, then move)
- [x] Git commits as checkpoints
- [x] Exit code 1 on rate limit
- [x] Next run resumes from last checkpoint

---

## Phase 1 (Completed)

### Milestone 1: Foundation ✅
- [x] Orphan dashboard branch
- [x] Blazor WASM project (.NET 10, C# 14)
- [x] Project structure

### Milestone 2: Data Layer ✅
- [x] GitHub, NuGet, Community, PR Triage collectors
- [x] Data models and services
- [x] Microsoft vs Community detection

### Milestone 3: Initial UI ✅
- [x] 5 pages: Home, GitHub, NuGet, Community, PR Triage
- [x] Navigation, layout, styling
- [x] SkiaSharp logo

### Milestone 4: CI/CD ✅
- [x] Single unified workflow
- [x] SPA routing fix (root 404.html)
- [x] 6-hour scheduled builds

---

## Phase 2: Dashboard Restructure ✅

### Page Structure
| Page | Route | Status |
|------|-------|--------|
| Home (Insights) | `/` | ✅ Complete |
| Issues | `/issues` | ✅ Complete |
| Pull Requests | `/pull-requests` | ✅ Complete |
| Community | `/community` | ✅ Complete |
| NuGet | `/nuget` | ✅ Grouped layout with legacy toggle |

### Phase 2.1: Data Layer ✅
- [x] Create `collect-issues.ps1` (paginated, all open issues)
- [x] Update `collect-pr-triage.ps1` (add labels, size, author type)
- [x] Create `IssueStats.cs` data model (renamed to IssuesData)
- [x] Update `PrTriageStats.cs` with new fields
- [x] Add `GetIssuesDataAsync()` to service

### Phase 2.2: Home Page (Insights) ✅
- [x] Add Blazor-ApexCharts NuGet package
- [x] Create health metrics cards row
- [x] Create "by type" bar chart
- [x] Create "by age" bar chart
- [x] Create "by area" horizontal bar chart
- [x] Create "PRs by size" chart
- [x] Add triage summary section
- [x] Add click handlers → navigate to filtered pages

### Phase 2.3: Issues Page ✅
- [x] Create `Issues.razor` page
- [x] Add filter dropdowns (Type, Area, Backend, OS, Age)
- [x] Add stats bar (showing X of Y)
- [x] Add sortable issues table
- [x] Implement URL-based filter state (`?type=bug&age=stale`)
- [x] Add expandable issue rows

### Phase 2.4: Pull Requests Page ✅
- [x] Add dual routes (`/pr-triage` and `/pull-requests`)
- [x] Add filter dropdowns (Size, Age, Author Type, Draft)
- [x] Add clickable triage summary cards (5 categories)
- [x] Add categorized PR lists with badges
- [x] Implement URL-based filter state

### Phase 2.5: Community Page ✅
- [x] Add stars, forks, watchers from GitHub stats
- [x] Keep contributors section with MS/Community breakdown
- [x] Add issues/PRs overview with 30-day activity
- [x] Remove GitHub.razor page
- [x] Update navigation

---

## Phase 2.7: NuGet Page Redesign ✅

### Collector Enhancements
- [x] Dynamic package list from VERSIONS.txt (main + release/2.x branches)
- [x] Union gives 50 packages total
- [x] `isLegacy` flag based on configurable `$minSupportedMajorVersion`
- [x] Switch from Registration API to Search API (better download stats)

### Page Layout
- [x] Total downloads hero card (975M+)
- [x] Collapsible group structure with subgroups for ALL groups:
  - SkiaSharp (Core, Native Assets)
  - SkiaSharp Views (Native Platform, .NET MAUI, Xamarin.Forms, Uno Platform, Web)
  - SkiaSharp Extensions (Text, Animation, GPU Backends)
  - HarfBuzzSharp (Core, Native Assets)
- [x] "Show legacy packages" checkbox (default off)
- [x] Group/subgroup subtotals
- [x] Styled cards with package info

---

## Phase 3: Collector CLI Migration ✅

### .NET Console App
- [x] Create `src/SkiaSharp.Collector/` project (.NET 10)
- [x] Add Spectre.Console.Cli, Octokit, NuGet.Protocol packages
- [x] Create 6 commands: all, github, nuget, community, issues, pr-triage

### Shared Services
- [x] GitHubService - API client with rate limiting, MS membership check
- [x] NuGetService - Search API, dynamic package list, legacy detection
- [x] LabelParser - Shared label parsing and age/size categories
- [x] OutputService - JSON serialization

### Project Cleanup
- [x] Move project from `collectors/` to `src/`
- [x] Add to solution
- [x] Delete PowerShell scripts (5 files, 975 lines removed)
- [x] Convert solution to `.slnx` format

### CI/CD Integration
- [x] Update workflow: single `dotnet run -- all` instead of 5 `pwsh` calls

---

## Known Issues

None! 🎉

---

## SPA Routing (SOLVED)

Navigation bugs were fixed by:
1. Using relative URLs in `NavigateTo()` - no leading slash
2. Not calling `NavigateTo()` on initial page load
3. Using spa-github-pages redirect pattern with segmentCount=2

See `.github/copilot-instructions.md` for full documentation.

## Future Ideas (Icebox)

1. Pagination for large issue/PR lists (currently shows all)
2. Trend charts (historical data over time)
3. Milestone tracking
4. CI/CD build status integration
5. Release progress view
6. AI-powered PR triage recommendations
7. Historical engagement data for trend analysis

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 0.9.0 | 2026-02-04 | NuGet download trend charts with hybrid API (Registration + Search) |
| 0.8.0 | 2026-02-04 | Community sync: contributors, MS membership, repo stats |
| 0.7.1 | 2026-02-04 | Checkpoint-based engagement sync (100 items/commit, loop until rate limit) |
| 0.7.0 | 2026-02-04 | Workflow restructure: NuGet first, batched engagement, 3-step sync |
| 0.6.1 | 2026-02-04 | Added push trigger to sync workflow |
| 0.6.0 | 2026-02-04 | Data cache architecture with engagement scoring |
| 0.5.0 | 2026-02-04 | .NET collector CLI replaces 5 PowerShell scripts |
| 0.4.0 | 2026-02-04 | NuGet page redesign - grouped layout, legacy toggle, 50 packages |
| 0.3.2 | 2026-02-03 | NuGet collector fixed - 822M+ downloads now shown |
| 0.3.1 | 2026-02-03 | SPA routing fix - navigation fully working |
| 0.3.0 | 2026-02-03 | Phase 2: Charts, filters, restructure |
| 0.2.0 | 2026-02-03 | MS/Community split, SPA fix, unified workflow |
| 0.1.0 | 2026-02-03 | Initial implementation |
