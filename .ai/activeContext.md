# Active Context

> Current focus, recent changes, immediate next steps, and working patterns.
> **Update this file frequently** - it's the "working memory" for AI assistants.

## Current Focus

**Phase**: Phase 2 - Dashboard Restructure
**Status**: Starting implementation

Restructuring the dashboard for better repo management:
- Home page becomes Insights (charts showing hotspots)
- New Issues page with filters
- Enhanced Pull Requests page
- Merged Community page (GitHub + Contributors)

## Recent Changes

### 2026-02-03 (Planning)
1. ✅ Planned Phase 2 restructure with user input
2. ✅ Defined new page structure: Home(Insights), Issues, PRs, Community, NuGet
3. ✅ Identified data needs: 659 issues, 50 PRs, 77 labels

### 2026-02-03 (Earlier)
1. ✅ Dashboard deployed and live
2. ✅ Fixed SPA routing with root 404.html
3. ✅ Microsoft vs Community contributor detection working
4. ✅ Single unified workflow

## Phase 2 Plan Summary

### New Page Structure
| Page | Route | Purpose |
|------|-------|---------|
| Home (Insights) | `/` | Health metrics + charts showing hotspots |
| Issues | `/issues` | Drill-down with filters, sortable list |
| Pull Requests | `/pull-requests` | Triage categories, filters |
| Community | `/community` | Stars, forks, contributors |
| NuGet | `/nuget` | Downloads (unchanged) |

### Data Context
- 659 open issues (paginated: 7 API calls)
- 50 open PRs
- 77 labels with prefixes: `type/`, `area/`, `backend/`, `os/`

### Key Features
- Clickable charts → drill down to filtered pages
- URL-based filters (`/issues?type=bug&age=stale`)
- Labels displayed without prefixes
- Health metrics (median response/close times)

## Implementation Order

1. **Data Layer** - New collectors, updated models
2. **Home/Insights** - Charts with ApexCharts
3. **Issues Page** - Filters, table
4. **PRs Page** - Enhanced triage
5. **Community** - Merge GitHub stats

## Files to Create/Modify

### New Files
- `collectors/collect-issues.ps1` - Paginated issue collection
- `src/Dashboard/Services/IssueStats.cs` - Issue data model
- `src/Dashboard/Pages/Issues.razor` - Issues page

### Modified Files
- `collectors/collect-pr-triage.ps1` - Add labels, size, author type
- `collectors/collect-github.ps1` - Add time metrics
- `src/Dashboard/Pages/Home.razor` - Convert to Insights
- `src/Dashboard/Pages/PrTriage.razor` → `PullRequests.razor`
- `src/Dashboard/Pages/Community.razor` - Add GitHub stats
- `src/Dashboard/Layout/NavMenu.razor` - Update navigation

### Deleted Files
- `src/Dashboard/Pages/GitHub.razor` - Merged into Community

## Working Patterns

### Label Handling
```csharp
// Strip prefix for display
"type/bug" → "bug"
"area/text" → "text"

// Use prefix for categorization
if (label.StartsWith("type/")) types.Add(label[5..]);
if (label.StartsWith("area/")) areas.Add(label[5..]);
```

### URL Filter Pattern
```csharp
// Read from URL
[SupplyParameterFromQuery] public string? Type { get; set; }
[SupplyParameterFromQuery] public string? Age { get; set; }

// Navigate with filter
NavigationManager.NavigateTo($"/issues?type={type}");
```

## Context for Next AI Session

When resuming work:
1. Read ALL files in `.ai/` folder first
2. Check this file for current phase (Phase 2 - Restructure)
3. Check session plan.md for detailed work plan
4. Branch is `dashboard`
5. Live at https://mono.github.io/SkiaSharp/dashboard/

## Notes

- Using Blazor-ApexCharts for charts (~50KB)
- All filtering is client-side (JSON pre-loaded)
- ~60-70 API calls per collection (safe for 6-hour schedule)
