# Progress & Backlog

> Project status, completed work, pending tasks, and future ideas.

## Current Status

**Overall**: 🔧 AI Triage Skill — Core pipeline complete, ready for testing with real AI
**Notes**: JSON Schema, preprocessor, validator all built and tested. SKILL.md rewritten for pipeline workflow.

| Area | Status | Notes |
|------|--------|-------|
| Phase 1 | ✅ Complete | Initial dashboard |
| Phase 2 | ✅ Complete | Restructure with charts |
| Phase 2.7 | ✅ Complete | NuGet grouped layout + legacy toggle |
| Phase 3 | ✅ Complete | .NET collector CLI replaces PowerShell |
| Phase 4 | ✅ Complete | Data cache with engagement scoring |
| Phase 5 | ✅ Complete | Multi-repo extension |
| Deployment | ✅ Working | https://mono.github.io/SkiaSharp/dashboard/ |
| AI Triage | 🔧 In Progress | Pipeline built, needs real AI testing |

---

## AI Triage Skill 🔧 (In Progress)

### Core Pipeline ✅
- [x] JSON Schema (`triage-schema.json`) — draft 2020-12 with cross-field rules
- [x] Preprocessor (`issue-to-markdown.py`) — annotated markdown from cached JSON
- [x] Validator (`validate-triage.py`) — thin jsonschema wrapper
- [x] SKILL.md — rewritten for fetch → preprocess → analyze → validate → write pipeline
- [x] schema.md — updated with reproEvidence documentation
- [x] Tested on real issues: #2794, #1048, #2958
- [x] Cross-field rules tested: 6/6 error cases caught

### Remaining
- [ ] Test with real AI (invoke skill on real issue)
- [ ] Refine skill via skill-creator if needed
- [ ] enums.json sync with triage-schema.json (currently duplicated)

---

## Phase 5: Multi-Repository Extension ✅ (Complete)

### Data Migration & Full Sync ✅
- [x] Migrate existing cache to `repos/mono-SkiaSharp/` structure
- [x] Run full parallel sync (both jobs completed successfully)
- [x] SkiaSharp: 3,183 items, 50 NuGet packages
- [x] Extended: 329 items, 4 NuGet packages  
- [x] Build & deploy with merged multi-repo data

### Cache Restructure ✅
- [x] Create `config.json` with repo list and NuGet discovery settings
- [x] Create `ConfigModels.cs` and `ConfigService.cs`
- [x] Update `CacheService` for per-repo paths: `repos/{key}/github/`, `repos/{key}/nuget/`
- [x] Contributors moved to `github/` folder

### Multi-Repo Sync ✅
- [x] Update `SyncGitHubCommand` with `--repo owner/name` argument
- [x] Update `SyncCommunityCommand` for per-repo
- [x] Add `DiscoverPackagesAsync()` to `NuGetService` (versions-txt + nuget-search)
- [x] Update `SyncNuGetCommand` for per-repo config

### Parallel Workflow ✅
- [x] Split sync into parallel jobs (sync-skiasharp, sync-extended)
- [x] Add rebase-retry push logic for conflict resolution
- [x] Add `--repo` filter in workflow_dispatch

### Generate Consolidation ✅
- [x] Update `GenerateCommand` to discover repos from `repos/*/`
- [x] Add `repo`, `repoSlug`, `repoColor` fields to issues/PRs/packages
- [x] Merge contributors (deduplicate by login, track per-repo)
- [x] Per-repo breakdown in all stats
- [x] MonthlyTrend with per-repo breakdown dictionaries

### Dashboard UI ✅
- [x] Update all service models for multi-repo fields
- [x] `MultiRepoGitHubStats` with `Repos` + `Total`
- [x] Home/Community pages use `Total` stats
- [x] All models backward compatible (new fields optional)

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
| 0.11.0 | 2026-02-05 | Multi-repository extension: per-repo cache, parallel sync, merged stats |
| 0.10.0 | 2026-02-05 | Issue/PR trend charts: stats cards, monthly activity, time range dropdown |
| 0.9.1 | 2026-02-04 | Refactor to NuGet.Protocol SDK, all versions have downloads |
| 0.9.0 | 2026-02-04 | NuGet charts, sorting filters, engagement loop fix |
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
