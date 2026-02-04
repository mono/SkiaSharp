# Technical Context

> Technologies, development setup, external dependencies, and constraints.

## Tech Stack

| Layer | Technology | Version | Notes |
|-------|------------|---------|-------|
| Runtime | .NET | 10.0 | Latest |
| Language | C# | 14 | Use latest features |
| Framework | Blazor WebAssembly | 10.0 | Standalone (no server) |
| Charts | Blazor-ApexCharts | Latest | Interactive charts |
| CSS | Bootstrap | 5.x | From template |
| Styling | Custom CSS | - | `dashboard.css` |
| CLI UI | Spectre.Console | Latest | Progress bars, tables |
| GitHub API | Octokit | Latest | Type-safe client |
| NuGet API | NuGet.Protocol | Latest | Package metadata |
| CI/CD | GitHub Actions | - | Two workflows |
| Hosting | GitHub Pages | - | Static files only |

## Development Setup

### Prerequisites
```bash
# Required
dotnet --version  # 10.0.100 or higher

# For visual testing (optional)
npx playwright install webkit
```

### Local Development Commands
```bash
# Navigate to dashboard
cd src/Dashboard

# Run with hot reload
dotnet watch run

# Run without hot reload  
dotnet run --urls "http://localhost:5050"

# Build only
dotnet build

# Production build
dotnet publish -c Release -o ../../publish
```

### Working with Data Cache
```bash
# One-time setup: Create worktree for cache branch
git worktree add .data-cache docs-data-cache

# Sync data from APIs to cache
dotnet run --project src/SkiaSharp.Collector -- sync --cache-path .data-cache

# Generate dashboard JSON from cache
dotnet run --project src/SkiaSharp.Collector -- generate --from-cache .data-cache -o src/Dashboard/wwwroot/data

# Full refresh workflow
dotnet run --project src/SkiaSharp.Collector -- sync --cache-path .data-cache
dotnet run --project src/SkiaSharp.Collector -- generate --from-cache .data-cache -o src/Dashboard/wwwroot/data
```

### Collector CLI Reference
```bash
# Sync commands (populate cache)
dotnet run -- sync github              # GitHub Layer 1 + Layer 2
dotnet run -- sync nuget               # NuGet only
dotnet run -- sync github --items-only # Skip engagement (Layer 1 only)
dotnet run -- sync github --engagement-only  # Skip items (Layer 2 only)
dotnet run -- sync github --engagement-count 25  # Batch size (default: 25)
dotnet run -- sync --full              # Ignore timestamps, full sync

# Generate command (cache → JSON)
dotnet run -- generate --from-cache ./cache -o ./data

# Legacy direct-API commands (still work)
dotnet run -- github -o ./data
dotnet run -- nuget -o ./data
dotnet run -- issues -o ./data
dotnet run -- pr-triage -o ./data
```

## External APIs

### GitHub REST API
- **Base URL**: `https://api.github.com`
- **Auth**: Bearer token (provided by `GITHUB_TOKEN`)
- **Rate Limit**: 5000/hr authenticated
- **Used For**: Repository stats, issues, PRs, comments, reactions

**Key Endpoints**:
| Endpoint | Purpose |
|----------|---------|
| `GET /repos/{owner}/{repo}` | Stars, forks, watchers |
| `GET /repos/{owner}/{repo}/issues` | Issues and PRs (paginated) |
| `GET /repos/{owner}/{repo}/issues/{number}/comments` | Comments |
| `GET /repos/{owner}/{repo}/issues/{number}/reactions` | Reactions |
| `GET /repos/{owner}/{repo}/pulls/{number}` | PR details |
| `GET /repos/{owner}/{repo}/pulls/{number}/reviews` | PR reviews |

### NuGet API
- **Base URL**: `https://api.nuget.org`
- **Auth**: None required
- **Rate Limit**: Generous (be conservative)
- **Used For**: Package download counts

**Key Endpoints**:
| Endpoint | Purpose |
|----------|---------|
| `GET /v3/registration5-gz-semver2/{id}/index.json` | Package metadata |
| `GET /query?q={id}` | Search (better download stats) |

## File Locations

### Source Code
```
src/Dashboard/
├── Dashboard.csproj        # Project file
├── Program.cs              # Entry point, DI setup
├── App.razor               # Root component, router
├── _Imports.razor          # Global usings
├── Pages/                  # Routable pages
│   ├── Home.razor          # Overview + hot issues
│   ├── Issues.razor        # Issues + hot issues section
│   ├── PullRequests.razor  # PR triage
│   ├── Community.razor     # Contributors
│   └── NuGet.razor         # Package downloads
├── Layout/                 # MainLayout, NavMenu
├── Services/               # DashboardDataService + models
└── wwwroot/
    ├── index.html          # HTML shell + SPA redirect script
    ├── 404.html            # SPA routing redirect
    ├── css/                # Stylesheets
    ├── data/               # JSON data files (generated)
    └── images/             # Static images

src/SkiaSharp.Collector/
├── SkiaSharp.Collector.csproj
├── Program.cs              # CLI entry point
├── Commands/
│   ├── SyncCommand.cs      # sync orchestrator
│   ├── SyncGitHubCommand.cs # GitHub Layer 1+2
│   ├── SyncNuGetCommand.cs # NuGet sync
│   ├── GenerateCommand.cs  # Cache → JSON
│   └── (legacy commands)
├── Services/
│   ├── CacheService.cs     # Read/write cache files
│   ├── GitHubService.cs    # GitHub API client
│   ├── NuGetService.cs     # NuGet API client
│   ├── EngagementCalculator.cs # Hot issue scoring
│   └── LabelParser.cs      # Parse type/area labels
└── Models/
    ├── CacheModels.cs      # SyncMeta, CachedItem, etc.
    └── (output models)
```

### Cache Structure (docs-data-cache branch)
```
/
├── github/
│   ├── sync-meta.json      # Last sync time, rate limits, skip list
│   ├── index.json          # All items sorted by number
│   └── items/
│       ├── 1.json          # Full item + engagement data
│       ├── 2.json
│       └── ...
├── nuget/
│   ├── sync-meta.json
│   ├── index.json          # Package list with download totals
│   └── packages/
│       ├── SkiaSharp.json
│       └── ...
```

### Configuration Files
```
/
├── .editorconfig           # C# code style
├── global.json             # .NET SDK version
├── SkiaSharp.slnx          # Solution (XML format)
└── .github/
    ├── copilot-instructions.md
    └── workflows/
        ├── sync-data-cache.yml   # Hourly + on push (3-step)
        └── build-dashboard.yml   # Every 6 hours
```

## Workflow Structure

### sync-data-cache.yml (3 steps with checkpoint loop)
```yaml
# Step 1: NuGet (every 6 hours: 0, 6, 12, 18 UTC)
- run: dotnet run -- sync nuget --cache-path $CACHE
- run: git commit && git push  # if changes

# Step 2: GitHub items (every run)
- run: dotnet run -- sync github --items-only --cache-path $CACHE
- run: git commit && git push  # if changes

# Step 3: GitHub engagement (loop until rate limit)
- run: |
    while true; do
      dotnet run -- sync github --engagement-only --engagement-count 100
      EXIT_CODE=$?
      git commit && git push  # Checkpoint every 100 items
      if [ $EXIT_CODE -ne 0 ]; then break; fi  # Rate limit hit
      if [ no_changes ]; then break; fi        # All done
    done
```

### Exit Codes
| Code | Meaning | Workflow Action |
|------|---------|-----------------|
| 0 | Batch complete | Continue loop (or exit if no changes) |
| 1 | Rate limit hit | Commit checkpoint, exit loop |

## Build Configuration

### Dashboard Project (`Dashboard.csproj`)
```xml
<TargetFramework>net10.0</TargetFramework>
<LangVersion>latest</LangVersion>
<Nullable>enable</Nullable>
<RootNamespace>SkiaSharp.Dashboard</RootNamespace>
```

### Collector Project (`SkiaSharp.Collector.csproj`)
```xml
<TargetFramework>net10.0</TargetFramework>
<LangVersion>latest</LangVersion>
<Nullable>enable</Nullable>
<OutputType>Exe</OutputType>

<!-- Dependencies -->
<PackageReference Include="Spectre.Console.Cli" />
<PackageReference Include="Octokit" />
<PackageReference Include="NuGet.Protocol" />
```

### Base Href Handling
- **Local**: `<base href="/" />` in `index.html`
- **Production**: CI uses `sed` to change to `<base href="/SkiaSharp/dashboard/" />`

## Constraints & Limitations

### GitHub Pages
- Static files only (no server-side code)
- Single branch deployment (`docs-live`)
- Must handle SPA routing (404.html redirect)

### Blazor WASM
- Initial load includes .NET runtime (~2-3MB)
- No access to server-side secrets
- All API calls must go through collectors

### Rate Limits
- GitHub: 5000 requests/hour (authenticated)
- Sync checks remaining calls, stops at < 100
- Skip list prevents hammering failed items

### Browser Support
- Modern browsers only (WASM requirement)
- IE11 not supported
- Mobile browsers work but slower

## Environment Variables

### GitHub Actions
| Variable | Used In | Purpose |
|----------|---------|---------|
| `GITHUB_TOKEN` | Workflows | Auto-provided by Actions |

### Local Development
| Variable | Purpose |
|----------|---------|
| `GITHUB_TOKEN` | Higher rate limits for sync |

## Dependencies

### Dashboard NuGet Packages
| Package | Purpose |
|---------|---------|
| `Microsoft.AspNetCore.Components.WebAssembly` | Blazor WASM |
| `Blazor-ApexCharts` | Charts |

### Collector NuGet Packages
| Package | Purpose |
|---------|---------|
| `Spectre.Console.Cli` | CLI framework + UI |
| `Octokit` | GitHub API client |
| `NuGet.Protocol` | NuGet API client |
