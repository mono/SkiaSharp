# SkiaSharp Dashboard - Copilot Instructions

> This file provides context for GitHub Copilot and other AI assistants working on this project.
> For deeper context, see the `.ai/` folder.

## ⚠️ CRITICAL: Memory Bank Updates

**After completing ANY significant work, you MUST update:**
1. `.ai/activeContext.md` - Add to "Recent Changes" section
2. `.ai/progress.md` - Update status, check off completed items

This ensures continuity across AI sessions. Never end a session without updating these files!

## Project Overview

This is the **SkiaSharp Project Dashboard** - a Blazor WebAssembly application that displays community metrics, project health, and PR triage for the SkiaSharp library.

- **Live URL**: https://mono.github.io/SkiaSharp/dashboard/
- **Repository**: mono/SkiaSharp (dashboard branch)
- **Parent Project**: SkiaSharp is a cross-platform 2D graphics library for .NET

## Current Phase: Dashboard Restructure

**Page Structure:**
| Page | Route | Purpose |
|------|-------|---------|
| Home (Insights) | `/` | Health metrics + charts showing hotspots |
| Issues | `/issues` | Drill-down with filters |
| Pull Requests | `/pull-requests` | Triage categories, filters |
| Community | `/community` | Stars, forks, contributors |
| NuGet | `/nuget` | Package downloads |

**Data Context:**
- 659 open issues, 50 PRs
- Labels use prefixes: `type/`, `area/`, `backend/`, `os/`
- Strip prefixes for display, use for categorization

## Tech Stack

- **.NET 10** with **C# 14** (use latest language features)
- **Blazor WebAssembly** (standalone, client-side only)
- **Blazor-ApexCharts** for charts
- **Bootstrap 5** for styling
- **PowerShell** for data collection scripts
- **GitHub Actions** for CI/CD

## Code Conventions

### C# Style
- Use **file-scoped namespaces** (`namespace X;` not `namespace X { }`)
- Use **primary constructors** where appropriate
- Use **records** for immutable data types (DTOs, stats models)
- Enable **nullable reference types** - no null warnings allowed
- Use **collection expressions** (`[1, 2, 3]` not `new[] { 1, 2, 3 }`)
- Prefer **expression-bodied members** for single-line methods/properties
- Private fields use `_camelCase` prefix

### Blazor Patterns
- Pages go in `Pages/` with `@page` directive
- Shared components go in `Components/`
- Layout components go in `Layout/`
- Services use constructor injection via `@inject`
- Data loading happens in `OnInitializedAsync()`
- **⚠️ CRITICAL: Use RELATIVE URLs** - never use absolute paths like `/issues` in `href` or `NavigateTo()`. Always use relative paths like `issues` so they work with the base href (`/SkiaSharp/dashboard/`). Absolute paths bypass the base href and break navigation on GitHub Pages.
- **Don't call NavigateTo() on initial page load** - If a page updates URL based on filters, don't call `NavigateTo()` in `OnInitializedAsync()`. This breaks direct URL navigation. Use a flag like `ApplyFilters(updateUrl: false)` on init.

### JSON Model Types
PowerShell collectors output floats for calculated values (e.g., `5.0` for days). C# models must match:
- Use `double` not `int` for: `DaysOpen`, `DaysSinceActivity`
- Use `long?` (nullable) for NuGet download counts (API returns null sometimes)

### Naming
- Pages: `{Feature}.razor` (e.g., `GitHub.razor`, `PrTriage.razor`)
- Services: `{Feature}Service.cs` (e.g., `DashboardDataService.cs`)
- Models: Descriptive records (e.g., `GitHubStats`, `TriagedPullRequest`)

## Project Structure

```
src/Dashboard/
├── Pages/           # Routable pages (@page directive)
├── Components/      # Reusable UI components
├── Layout/          # MainLayout, NavMenu
├── Services/        # Data services and models
└── wwwroot/
    ├── data/        # JSON data files (updated by collectors)
    ├── css/         # Stylesheets
    └── images/      # Static images

src/SkiaSharp.Collector/     # .NET CLI for data collection
├── Commands/                # all, github, nuget, community, issues, pr-triage
├── Services/                # GitHubService, NuGetService, LabelParser
└── Models/                  # Type-safe data models

.github/workflows/   # CI/CD pipelines
.ai/                 # AI memory bank (deep context)
```

## Collector CLI

The `SkiaSharp.Collector` .NET console app collects all dashboard data:

```bash
# Run all collectors (default)
dotnet run --project src/SkiaSharp.Collector -- all -o src/Dashboard/wwwroot/data

# Run specific collector
dotnet run --project src/SkiaSharp.Collector -- github -o ./data

# NuGet with custom minimum version (for 4.x release)
dotnet run --project src/SkiaSharp.Collector -- nuget --min-version 4 -o ./data
```

**Commands**: `all`, `github`, `nuget`, `community`, `issues`, `pr-triage`
**Options**: `-o/--output`, `--owner`, `--repo`, `-v/--verbose`, `-q/--quiet`

## Data Architecture

**Key principle**: Data is decoupled from the app.
- JSON files in `wwwroot/data/` are loaded at runtime
- Collectors update JSON independently of app builds
- App can be rebuilt without touching data
- Data can be refreshed without rebuilding app

### Data Files
| File | Purpose | Updated By |
|------|---------|------------|
| `github-stats.json` | Stars, issues, PRs, commits | `collect-github.ps1` |
| `nuget-stats.json` | Package downloads (975M+) | `collect-nuget.ps1` |
| `community-stats.json` | Contributors, activity | `collect-community.ps1` |
| `pr-triage.json` | AI-analyzed PR recommendations | `collect-pr-triage.ps1` |

### NuGet Collector Details
The NuGet collector (`collect-nuget.ps1`) has special logic:

1. **Dynamic package list**: Fetches `VERSIONS.txt` from main and release/2.x branches, extracts packages matching `<id> nuget <version>` pattern
2. **Uses Search API**: `azuresearch-usnc.nuget.org/query` (NOT Registration API which returns null downloads)
3. **Legacy detection**: Packages without stable version with major >= `$minSupportedMajorVersion` (default 3) are marked `isLegacy: true`
4. **Future-proof**: When SkiaSharp 4.x releases, just change the threshold to 4

## Branch Strategy

- **dashboard** (this branch): Orphan branch, no history from main
- **docs-live**: Deployed GitHub Pages site
- **main**: SkiaSharp library source (different project)
- **docs**: Documentation source (different project)

**Never commit directly to main or docs-live.**

## CI/CD Workflows

| Workflow | Trigger | Purpose |
|----------|---------|---------|
| `build-dashboard.yml` | Push to dashboard, manual | Build and deploy app |
| `update-dashboard-data.yml` | Every 6 hours, manual | Refresh data JSON files |

Both deploy to `docs-live/dashboard/` using `keep_files: true` to preserve other content.

## Local Development

```bash
cd src/Dashboard
dotnet run                    # Runs on http://localhost:5000
dotnet build                  # Build only
dotnet publish -c Release     # Production build
```

**Base href**: Uses `/` locally, CI changes to `/SkiaSharp/dashboard/` via sed.

## Testing

- Use **Playwright** for visual testing (WebKit browser)
- Run data collectors locally: `pwsh collectors/collect-github.ps1 -OutputPath test.json`
- Manual testing: Navigate all 5 pages, verify data loads

## GitHub Pages SPA Routing

This app uses the [spa-github-pages](https://github.com/rafrex/spa-github-pages) approach for client-side routing on GitHub Pages. This is the **official Microsoft-recommended approach** per [MS Learn docs](https://learn.microsoft.com/en-us/aspnet/core/blazor/host-and-deploy/webassembly/github-pages).

### How It Works

1. **404.html** - When GitHub Pages can't find a file (e.g., `/dashboard/issues`), it serves 404.html which redirects to `/?p=/issues` with the path encoded in a query string
2. **index.html script** - Restores the original URL via `history.replaceState` before Blazor starts
3. **Blazor Router** - Sees the correct URL and routes to the right component

### Critical Configuration for Nested Subdirectories

Our app lives at `/SkiaSharp/dashboard/` (2 segments deep). Key settings:

| File | Setting | Purpose |
|------|---------|---------|
| `wwwroot/404.html` | `segmentCount = 2` | Preserves `/SkiaSharp/dashboard` in redirect |
| `wwwroot/index.html` | spa-github-pages script in `<head>` | Restores URL before Blazor loads |
| `wwwroot/root-404.html` | Deployed to site root | Root 404 intercepts ALL site 404s |
| CI workflow | `sed` to set base href | Changes `<base href="/">` to `<base href="/SkiaSharp/dashboard/">` |

### ⚠️ CRITICAL: URL Path Rules

Per [MS Learn](https://learn.microsoft.com/en-us/aspnet/core/blazor/host-and-deploy/app-base-path#configure-the-app-base-path):

**NavigateTo calls:**
- ❌ `Navigation.NavigateTo("/issues")` - Goes to site root!
- ✅ `Navigation.NavigateTo("issues")` - Relative to base href
- ✅ `Navigation.NavigateTo("./issues")` - Also works

**Anchor hrefs:**
- ❌ `<a href="/issues">` - Goes to site root!
- ✅ `<a href="issues">` - Relative to base href
- ✅ `<NavLink href="issues">` - Correct

**@page routes:**
- ✅ `@page "/issues"` - Routes DO use leading slash (this is correct)

### Debugging Navigation Issues

If clicking links navigates to wrong URLs (e.g., `/issues` instead of `/SkiaSharp/dashboard/issues`):

1. Check for leading slashes in `NavigateTo()` calls
2. Check for leading slashes in `href` attributes  
3. Verify `<base href="/SkiaSharp/dashboard/">` is set (trailing slash required!)
4. Check browser console for navigation errors
5. Verify 404.html has correct `segmentCount`

## What NOT To Do

- ❌ Don't add server-side code (this is client-only WASM)
- ❌ Don't commit secrets or API keys
- ❌ Don't modify `docs-live` branch directly
- ❌ Don't use old C# patterns (var everywhere, expression bodies, etc.)
- ❌ Don't add heavy dependencies (keep WASM bundle small)
- ❌ Don't use leading slashes in `NavigateTo()` or `href` (breaks GitHub Pages)
- ❌ Don't call `NavigateTo()` in `OnInitializedAsync()` without a guard

## Common Bugs & Fixes

| Symptom | Cause | Fix |
|---------|-------|-----|
| Clicking link goes to `mono.github.io/issues` instead of `/SkiaSharp/dashboard/issues` | Leading slash in NavigateTo or href | Use `"issues"` not `"/issues"` |
| Page loads then redirects to wrong URL | `NavigateTo()` called on init | Add `updateUrl: false` param for initial load |
| Direct URL access shows 404 | Missing or wrong 404.html | Check segmentCount matches path depth |
| JSON fails to deserialize | Type mismatch (int vs double) | Use `double` for day calculations |
| NuGet downloads show as 0 | Non-nullable type for null API response | Use `long?` for Downloads |

## Getting Help

- Check `.ai/` folder for deeper context on architecture and decisions
- Check `.ai/progress.md` for current status and backlog
- Check `.ai/activeContext.md` for what's currently being worked on
