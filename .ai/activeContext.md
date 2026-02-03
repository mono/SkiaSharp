# Active Context

> Current focus, recent changes, immediate next steps, and working patterns.
> **Update this file frequently** - it's the "working memory" for AI assistants.

## Current Focus

**Phase**: Phase 2 - Dashboard Restructure COMPLETE ✅
**Status**: Navigation Fixed & Verified

All pages working with click navigation AND direct URL access.

## Recent Changes

### 2026-02-03 (SPA Routing Fix - COMPLETE)
1. ✅ Reset index.html to clean Blazor template + spa-github-pages script
2. ✅ Created proper 404.html with segmentCount=2 for nested subdirectory
3. ✅ Created root-404.html to replace old sessionStorage approach at site root
4. ✅ **CRITICAL FIX**: Removed leading slash from NavigateTo URLs:
   - Issues.razor: `/issues` → `issues`
   - PrTriage.razor: `/pull-requests` → `pull-requests`
5. ✅ **CRITICAL FIX**: Don't call NavigateTo on initial page load:
   - Split `ApplyFilters()` into parameterless (for @bind:after) and `ApplyFiltersInternal(updateUrl)`
   - Call with `updateUrl: false` in OnInitializedAsync
6. ✅ Updated copilot-instructions.md with comprehensive SPA routing docs + bug fixes table

### 2026-02-03 (Earlier Bug Fixes)
1. ✅ Fixed model type mismatches (int → double for DaysOpen fields)
2. ✅ Fixed NuGet model (nullable Downloads)

### Key Commits
- `1bb2d910` - Fix method group conversion errors in filter bindings
- `e1f5741e` - Fix NavigateTo leading slash
- `219a9a66` - Root 404.html with spa-github-pages approach
- `072251ce` - Reset to clean template with spa-github-pages

### Verified Working ✅
- ✅ Click navigation between pages
- ✅ Direct URL access (e.g., typing `/SkiaSharp/dashboard/issues` directly)
- ✅ Filter URLs (e.g., `/issues?type=bug`)
- ✅ Browser back/forward buttons

## ⚠️ CRITICAL: SPA Routing on GitHub Pages

### The Problem
Blazor WASM apps on GitHub Pages subdirectories have navigation issues because:
1. GitHub Pages doesn't support server-side URL rewriting
2. The `<base href>` setting affects how Blazor computes navigation URLs
3. Root 404.html intercepts ALL 404s site-wide

### The Solution (spa-github-pages approach)
**Per [MS Learn docs](https://learn.microsoft.com/en-us/aspnet/core/blazor/host-and-deploy/webassembly/github-pages)**:

1. **404.html** - Redirects `/SkiaSharp/dashboard/issues` to `/SkiaSharp/dashboard/?p=/issues`
2. **index.html script** - Restores URL via `history.replaceState` before Blazor loads
3. **segmentCount=2** - For nested paths like `/SkiaSharp/dashboard/`

### URL Rules (MEMORIZE THESE)
```csharp
// ❌ WRONG - Goes to site root /issues
Navigation.NavigateTo("/issues");

// ✅ CORRECT - Relative to base href
Navigation.NavigateTo("issues");
Navigation.NavigateTo("./issues");

// ❌ WRONG - Goes to site root
<a href="/issues">

// ✅ CORRECT - Relative to base
<a href="issues">
<NavLink href="issues">

// ✅ @page routes DO use leading slash
@page "/issues"
```

### Files Involved
| File | Purpose |
|------|---------|
| `wwwroot/index.html` | spa-github-pages restore script in `<head>` |
| `wwwroot/404.html` | Redirect script with segmentCount=2 |
| `wwwroot/root-404.html` | Deployed to site root to handle all 404s |
| `.github/workflows/build-dashboard.yml` | Sets base href via sed |

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
```

### JSON Model Types
PowerShell outputs floats (`5.0`) - use `double` not `int` for DaysOpen fields.
NuGet API returns `null` for some downloads - use `long?`.

## Context for Next AI Session

When resuming work:
1. Read ALL files in `.ai/` folder first
2. Branch is `dashboard`
3. Live at https://mono.github.io/SkiaSharp/dashboard/
4. **CHECK NAVIGATION WORKS** - click Issues/PRs links, verify URL stays correct

## Potential Future Work

- Add pagination to Issues/PRs tables for very large lists
- Add more chart types (trends over time)
- CI/CD status integration
- Milestone tracking
