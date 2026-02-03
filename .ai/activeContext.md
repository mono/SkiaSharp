# Active Context

> Current focus, recent changes, immediate next steps, and working patterns.
> **Update this file frequently** - it's the "working memory" for AI assistants.

## Current Focus

**Phase**: Phase 2 - Dashboard Restructure
**Status**: COMPLETE ✅

All major pages implemented and deployed:
- ✅ Home (Insights) with ApexCharts
- ✅ Issues page with filters
- ✅ Pull Requests page with triage & filters
- ✅ Community page merged with GitHub stats
- ✅ NuGet page (unchanged)

## Recent Changes

### 2026-02-03 (Phase 2 Complete)
1. ✅ Phase 2.1: Data Layer - collectors, models, ApexCharts
2. ✅ Phase 2.2: Home/Insights with 4 charts
3. ✅ Phase 2.3: Issues page with filters & table
4. ✅ Phase 2.4: Pull Requests with triage cards & filters
5. ✅ Phase 2.5: Community merged with GitHub stats
6. ✅ Deleted GitHub.razor (merged into Community)

### Key Commits
- `b8467de4` - Phase 2.3: Issues page with filters
- `8d18029e` - Phase 2.4-2.5: Enhanced PRs & merged Community

## Current Page Structure
| Page | Route | Status |
|------|-------|--------|
| Home (Insights) | `/` | ✅ Charts, metrics, triage summary |
| Issues | `/issues` | ✅ Filters, sortable table |
| Pull Requests | `/pull-requests` | ✅ Triage cards, filters |
| Community | `/community` | ✅ Repo stats + contributors |
| NuGet | `/nuget` | ✅ Downloads |

## Implementation Details

### Charts (ApexCharts)
- Issues by Type (bar)
- Issues by Age (bar)
- Issues by Area (horizontal bar)
- PRs by Size (donut)

### Filters
- **Issues**: Type, Area, Age, Backend, OS
- **PRs**: Category (triage), Size, Age, Author Type, Draft
- All filters use URL query params for shareable links

### Triage Categories (5)
1. ReadyToMerge - Approved, no changes requested
2. QuickReview - Small, fresh, no blockers
3. NeedsReview - Awaiting first review
4. NeedsAuthor - Changes requested
5. ConsiderClosing - Very old or stale

## Working Patterns

### Label Handling
```csharp
// Strip prefix for display
"type/bug" → "bug"
"area/text" → "text"

// Use prefix for categorization
if (label.StartsWith("type/")) types.Add(label[5..]);
```

### URL Filter Pattern
```csharp
[SupplyParameterFromQuery] public string? Type { get; set; }
NavigationManager.NavigateTo($"/issues?type={type}", replace: true);
```

## Context for Next AI Session

When resuming work:
1. Read ALL files in `.ai/` folder first
2. Phase 2 is COMPLETE
3. Branch is `dashboard`
4. Live at https://mono.github.io/SkiaSharp/dashboard/

## Potential Future Work

- Add pagination to Issues/PRs tables for very large lists
- Add more chart types (trends over time)
- CI/CD status integration
- Milestone tracking
