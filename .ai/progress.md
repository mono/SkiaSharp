# Progress & Backlog

> Project status, completed work, pending tasks, and future ideas.

## Current Status

**Overall**: 🟡 Phase 2 - Dashboard Restructure In Progress

| Area | Status | Notes |
|------|--------|-------|
| Phase 1 | ✅ Complete | Initial dashboard live |
| Phase 2 Data | 🔄 Starting | New collectors needed |
| Phase 2 UI | ⏳ Pending | After data layer |
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

## Phase 2: Dashboard Restructure

### Context
- 659 open issues (7 paginated API calls)
- 50 open PRs
- 77 labels: `type/`, `area/`, `backend/`, `os/`

### New Page Structure
| Page | Route | Status |
|------|-------|--------|
| Home (Insights) | `/` | ⏳ Pending |
| Issues | `/issues` | ⏳ Pending |
| Pull Requests | `/pull-requests` | ⏳ Pending |
| Community | `/community` | ⏳ Pending (merge GitHub) |
| NuGet | `/nuget` | ✅ No changes |

### Phase 2.1: Data Layer
- [ ] Create `collect-issues.ps1` (paginated, all open issues)
- [ ] Update `collect-pr-triage.ps1` (add labels, size, author type)
- [ ] Update `collect-github.ps1` (add time metrics)
- [ ] Create `IssueStats.cs` data model
- [ ] Update `PrTriageStats.cs` with new fields
- [ ] Add `GetIssueStatsAsync()` to service

### Phase 2.2: Home Page (Insights)
- [ ] Add Blazor-ApexCharts NuGet package
- [ ] Create health metrics cards row
- [ ] Create "by type" horizontal bar chart
- [ ] Create "by area" horizontal bar chart
- [ ] Create "by backend" horizontal bar chart
- [ ] Create "by OS" horizontal bar chart
- [ ] Create age distribution bar chart
- [ ] Add click handlers → navigate to Issues with filter

### Phase 2.3: Issues Page
- [ ] Create `Issues.razor` page
- [ ] Add filter dropdowns (Type, Area, Backend, OS, Age)
- [ ] Add stats bar (showing X of Y)
- [ ] Add sortable issues table
- [ ] Implement URL-based filter state (`?type=bug&age=stale`)

### Phase 2.4: Pull Requests Page
- [ ] Rename `PrTriage.razor` → `PullRequests.razor`
- [ ] Add filter dropdowns (Size, Age, Review Status, Author)
- [ ] Add triage summary cards
- [ ] Add categorized PR lists
- [ ] Implement URL-based filter state

### Phase 2.5: Community Page
- [ ] Add stars, forks, watchers from GitHub stats
- [ ] Keep contributors section
- [ ] Remove GitHub.razor page
- [ ] Update navigation

### Phase 2.6: Polish
- [ ] Update NavMenu with new routes
- [ ] Update Home overview cards (if any remain)
- [ ] Mobile responsive charts
- [ ] Test all filter combinations
- [ ] Update memory bank

---

## Known Issues

| Issue | Severity | Status |
|-------|----------|--------|
| NuGet totalDownloads shows 0 | Low | Pending fix |
| PR Triage shows dashes | Low | May be fixed with restructure |

## Future Ideas (Icebox)

1. AI-powered PR triage (needs API key management)
2. Historical data for trend charts
3. Milestone tracking
4. Release progress view

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 0.3.0 | TBD | Phase 2 restructure |
| 0.2.0 | 2026-02-03 | MS/Community split, SPA fix, unified workflow |
| 0.1.0 | 2026-02-03 | Initial implementation |
