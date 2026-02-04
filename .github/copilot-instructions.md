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
- **Repository**: mono/SkiaSharp (docs-dashboard branch)
- **Parent Project**: SkiaSharp is a cross-platform 2D graphics library for .NET

## Solution Structure

**Solution**: `SkiaSharp.slnx` (new XML-based format)

**Projects**:
| Project | Path | Purpose |
|---------|------|---------|
| Dashboard | `src/Dashboard/` | Blazor WASM app |
| SkiaSharp.Collector | `src/SkiaSharp.Collector/` | .NET CLI for data sync & generation |

**Page Structure:**
| Page | Route | Purpose |
|------|-------|---------|
| Home (Insights) | `/` | Health metrics, charts, hot issues |
| Issues | `/issues` | All issues with filters, hot issues section |
| Pull Requests | `/pull-requests` | Triage categories, filters |
| Community | `/community` | Stars, forks, contributors |
| NuGet | `/nuget` | Package downloads (grouped layout) |

## Branch Strategy

| Branch | Purpose | Triggers |
|--------|---------|----------|
| `docs-dashboard` | Dashboard source code (orphan) | Push triggers sync + build |
| `docs-data-cache` | Cached API data (GitHub/NuGet) | Updated hourly by sync |
| `docs-live` | Deployed GitHub Pages site | Updated by build workflow |
| `main` | SkiaSharp library source | Do not modify |
| `docs` | Documentation source | Do not modify |

**Never commit directly to main, docs, or docs-live.**

## Data Architecture

**Key principle**: Data sync is **decoupled** from dashboard build.

```
HOURLY + ON PUSH (sync-data-cache.yml):
  Step 1a: NuGet API     ─→ sync nuget      ─→ push (every 6 hours)
  Step 1b: Community API ─→ sync community  ─→ push (every 6 hours)
  Step 2:  GitHub API    ─→ sync github --items-only ─→ push
  Step 3:  GitHub API    ─→ sync github --engagement-only (loop until rate limit)
           └─→ 100 items per iteration ─→ push (checkpoint) ─→ repeat

EVERY 6 HOURS (build-dashboard.yml):  
  docs-data-cache ─→ generate command ─→ dashboard JSON ─→ deploy to docs-live
```

### Cache Structure (`docs-data-cache` branch)
```
docs-data-cache/
├── github/
│   ├── sync-meta.json       # Sync state, rate limits, skip list
│   ├── repo.json            # Stars, forks, watchers, topics
│   ├── index.json           # All issues + PRs (sorted by number)
│   └── items/{number}.json  # Full data + engagement per item
├── community/
│   ├── sync-meta.json       # Last sync time
│   └── contributors.json    # Top 100 contributors with MS flag
├── nuget/
│   ├── sync-meta.json
│   ├── index.json
│   └── packages/{id}.json
```

### Sync Commands
| Command | Schedule | API Calls | Data |
|---------|----------|-----------|------|
| `sync nuget` | Every 6h | ~50 | Package downloads |
| `sync community` | Every 6h | ~21 | Contributors + MS check |
| `sync github --items-only` | Hourly | ~35 pages | All issues/PRs |
| `sync github --engagement-only` | Hourly | Until rate limit | Comments, reactions |

### Checkpoint Strategy (Fault Tolerance)
```bash
while true; do
  sync github --engagement-only --engagement-count 100
  git commit && push  # Checkpoint every 100 items
  if rate_limited or all_done; then break; fi
done
```
- Atomic file writes prevent JSON corruption
- Git commits provide checkpoints
- Crash loses max 100 items of work

### Engagement Scoring
Formula: `(Comments × 3) + (Reactions × 1) + (Contributors × 2) + (1/DaysSinceActivity) + (1/DaysOpen)`

**Hot Issues**: Current score > historical score (7 days ago) AND score > 5

## CI/CD Workflows

| Workflow | File | Triggers | Purpose |
|----------|------|----------|---------|
| Sync Data Cache | `sync-data-cache.yml` | Hourly, push to docs-dashboard | Sync GitHub/NuGet/Community → cache |
| Build Dashboard | `build-dashboard.yml` | Every 6 hours, manual | Generate JSON + build + deploy |

**Sync workflow steps:**
1. NuGet + Community (every 6 hours) → push
2. GitHub items (Layer 1) → push  
3. GitHub engagement (Layer 2) → loop: 100 items → push → repeat until rate limit

Both use `concurrency: cancel-in-progress: false` to allow queuing.

## Collector CLI

The `SkiaSharp.Collector` .NET console app has two main modes:

### Sync Mode (populates cache)
```bash
# Sync GitHub data (items + engagement)
dotnet run -- sync github --cache-path ./cache

# Sync NuGet data
dotnet run -- sync nuget --cache-path ./cache

# Sync community data (contributors + MS membership)
dotnet run -- sync community --cache-path ./cache

# GitHub options
--engagement-count 100    # Items per batch (default: 100)
--items-only              # Skip engagement sync (Layer 1 only)
--engagement-only         # Skip items sync (Layer 2 only)
--full                    # Force full sync (ignore timestamps)
```

### Exit Codes
| Code | Meaning |
|------|---------|
| 0 | Success (batch complete or all done) |
| 1 | Rate limit hit (commit what we have, retry later) |

### Generate Mode (cache → dashboard JSON)
```bash
# Generate all dashboard JSON from cache
dotnet run -- generate --from-cache ./cache -o ./data
```

### Legacy Direct-API Commands (still available)
```bash
dotnet run -- github -o ./data        # Just GitHub stats
dotnet run -- nuget -o ./data         # Just NuGet stats
```

## Local Development

```bash
# One-time setup: Create worktree for cache
git worktree add .data-cache docs-data-cache

# Sync data locally (all sources)
dotnet run --project src/SkiaSharp.Collector -- sync github --cache-path .data-cache
dotnet run --project src/SkiaSharp.Collector -- sync nuget --cache-path .data-cache
dotnet run --project src/SkiaSharp.Collector -- sync community --cache-path .data-cache

# Generate dashboard JSON
dotnet run --project src/SkiaSharp.Collector -- generate --from-cache .data-cache -o src/Dashboard/wwwroot/data

# Run dashboard
cd src/Dashboard && dotnet run
```

**Base href**: Uses `/` locally, CI changes to `/SkiaSharp/dashboard/` via sed.

## Tech Stack

- **.NET 10** with **C# 14** (use latest language features)
- **Blazor WebAssembly** (standalone, client-side only)
- **Blazor-ApexCharts** for charts
- **Bootstrap 5** for styling
- **Spectre.Console** for CLI UI
- **Octokit** for GitHub API
- **NuGet.Protocol** for NuGet API

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
- **⚠️ CRITICAL: Use RELATIVE URLs** - never use `/issues`, always use `issues`

### JSON Model Types
- Use `double` not `int` for: `DaysOpen`, `DaysSinceActivity`
- Use `long?` (nullable) for NuGet download counts

## Project Structure

```
src/Dashboard/
├── Pages/           # Routable pages (@page directive)
├── Components/      # Reusable UI components
├── Layout/          # MainLayout, NavMenu
├── Services/        # Data services and models
└── wwwroot/
    ├── data/        # JSON data files (generated)
    ├── css/         # Stylesheets
    └── images/      # Static images

src/SkiaSharp.Collector/
├── Commands/        # CLI commands (sync, generate, legacy)
├── Services/        # GitHubService, NuGetService, CacheService
└── Models/          # CacheModels, output models

.github/workflows/   # CI/CD pipelines
.ai/                 # AI memory bank (deep context)
```

## Generated Data Files

| File | Source | Purpose |
|------|--------|---------|
| `github-stats.json` | Cache | Stars, issues, PRs, commits |
| `nuget-stats.json` | Cache | Package downloads |
| `community-stats.json` | Cache | Contributors, activity |
| `issues.json` | Cache | All issues with engagement scores |
| `pr-triage.json` | Cache | PR analysis and triage categories |

## GitHub Pages SPA Routing

Uses [spa-github-pages](https://github.com/rafrex/spa-github-pages) approach:

1. **404.html** - Redirects unknown paths to `/?p=/path`
2. **index.html script** - Restores original URL via `history.replaceState`
3. **Blazor Router** - Handles the route

**Critical**: `segmentCount = 2` in 404.html (for `/SkiaSharp/dashboard/`)

### URL Rules
- ❌ `NavigateTo("/issues")` - Goes to site root!
- ✅ `NavigateTo("issues")` - Relative to base href
- ❌ `<a href="/issues">` - Wrong!
- ✅ `<a href="issues">` - Correct

## Error Handling (Sync)

- **Proactive rate limit check**: Stop if < 100 API calls remaining
- **Skip list**: Failed items get cooldown periods (404=7d, 403=1d, other=1h)
- **Retry logic**: Items automatically retry after cooldown expires
- **No data = empty files**: Generate always writes files to support cache resets

## What NOT To Do

- ❌ Don't add server-side code (client-only WASM)
- ❌ Don't commit secrets or API keys
- ❌ Don't modify `docs-live`, `main`, or `docs` branches directly
- ❌ Don't use leading slashes in `NavigateTo()` or `href`
- ❌ Don't call `NavigateTo()` in `OnInitializedAsync()` without a guard

## Getting Help

- Check `.ai/` folder for deeper context on architecture and decisions
- Check `.ai/progress.md` for current status and backlog
- Check `.ai/activeContext.md` for what's currently being worked on
