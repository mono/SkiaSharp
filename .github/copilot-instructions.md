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

collectors/          # PowerShell scripts for data collection
.github/workflows/   # CI/CD pipelines
.ai/                 # AI memory bank (deep context)
```

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
| `nuget-stats.json` | Package downloads | `collect-nuget.ps1` |
| `community-stats.json` | Contributors, activity | `collect-community.ps1` |
| `pr-triage.json` | AI-analyzed PR recommendations | `collect-pr-triage.ps1` |

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

## What NOT To Do

- ❌ Don't add server-side code (this is client-only WASM)
- ❌ Don't commit secrets or API keys
- ❌ Don't modify `docs-live` branch directly
- ❌ Don't use old C# patterns (var everywhere, expression bodies, etc.)
- ❌ Don't add heavy dependencies (keep WASM bundle small)

## Getting Help

- Check `.ai/` folder for deeper context on architecture and decisions
- Check `.ai/progress.md` for current status and backlog
- Check `.ai/activeContext.md` for what's currently being worked on
