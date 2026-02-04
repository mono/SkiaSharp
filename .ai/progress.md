# Progress & Backlog

> Project status, completed work, pending tasks, and future ideas.

## Current Status

**Overall**: ✅ Phase 2.7 - NuGet Page Redesign COMPLETE

| Area | Status | Notes |
|------|--------|-------|
| Phase 1 | ✅ Complete | Initial dashboard |
| Phase 2 | ✅ Complete | Restructure with charts |
| Phase 2.7 | ✅ Complete | NuGet grouped layout + legacy toggle |
| Deployment | ✅ Working | https://mono.github.io/SkiaSharp/dashboard/ |

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

## Known Issues

None! 🎉

## Phase 2.7: NuGet Page Redesign ✅

### Collector Enhancements
- [x] Dynamic package list from VERSIONS.txt (main + release/2.x branches)
- [x] Union gives 50 packages total
- [x] `isLegacy` flag based on configurable `$minSupportedMajorVersion`
- [x] Switch from Registration API to Search API (better download stats)

### Page Layout
- [x] Total downloads hero card (975M+)
- [x] Collapsible group structure:
  - SkiaSharp (core + NativeAssets)
  - SkiaSharp Views (5 subgroups)
  - SkiaSharp Extensions (3 subgroups)
  - HarfBuzzSharp (core + NativeAssets)
- [x] "Show legacy packages" checkbox (default off)
- [x] Group/subgroup subtotals
- [x] Styled cards with package info

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

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 0.4.0 | 2026-02-04 | NuGet page redesign - grouped layout, legacy toggle, 50 packages |
| 0.3.2 | 2026-02-03 | NuGet collector fixed - 822M+ downloads now shown |
| 0.3.1 | 2026-02-03 | SPA routing fix - navigation fully working |
| 0.3.0 | 2026-02-03 | Phase 2: Charts, filters, restructure |
| 0.2.0 | 2026-02-03 | MS/Community split, SPA fix, unified workflow |
| 0.1.0 | 2026-02-03 | Initial implementation |
