# Technical Context

> Technologies, development setup, external dependencies, and constraints.

## Tech Stack

| Layer | Technology | Version | Notes |
|-------|------------|---------|-------|
| Runtime | .NET | 10.0 | Latest LTS |
| Language | C# | 14 | Use latest features |
| Framework | Blazor WebAssembly | 10.0 | Standalone (no server) |
| CSS | Bootstrap | 5.x | From template |
| Styling | Custom CSS | - | `dashboard.css` |
| CI/CD | GitHub Actions | - | Two workflows |
| Hosting | GitHub Pages | - | Static files only |
| Scripts | PowerShell | 7.x | Cross-platform |

## Development Setup

### Prerequisites
```bash
# Required
dotnet --version  # 10.0.100 or higher

# For data collection testing
pwsh --version    # PowerShell 7.x

# For visual testing (optional)
npx playwright install webkit
```

### Local Development Commands
```bash
# Navigate to project
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

### Testing Data Collectors
```bash
# GitHub stats (requires GITHUB_TOKEN for higher rate limits)
export GITHUB_TOKEN=ghp_xxxxx
pwsh collectors/collect-github.ps1 -OutputPath test-github.json

# NuGet stats (no auth required)
pwsh collectors/collect-nuget.ps1 -OutputPath test-nuget.json

# Community stats
pwsh collectors/collect-community.ps1 -OutputPath test-community.json

# PR triage
pwsh collectors/collect-pr-triage.ps1 -OutputPath test-triage.json
```

## External APIs

### GitHub REST API
- **Base URL**: `https://api.github.com`
- **Auth**: Bearer token (optional, but increases rate limit)
- **Rate Limit**: 60/hr unauthenticated, 5000/hr authenticated
- **Used For**: Repository stats, issues, PRs, commits, contributors

**Endpoints Used**:
| Endpoint | Purpose |
|----------|---------|
| `GET /repos/{owner}/{repo}` | Stars, forks, watchers |
| `GET /search/issues` | Issue/PR counts and filters |
| `GET /repos/{owner}/{repo}/commits` | Recent commits |
| `GET /repos/{owner}/{repo}/contributors` | Contributor list |
| `GET /repos/{owner}/{repo}/pulls/{number}` | PR details |
| `GET /repos/{owner}/{repo}/pulls/{number}/reviews` | PR reviews |

### NuGet API
- **Base URL**: `https://api.nuget.org`
- **Auth**: None required
- **Rate Limit**: Unknown, be conservative
- **Used For**: Package download counts

**Endpoints Used**:
| Endpoint | Purpose |
|----------|---------|
| `GET /v3-flatcontainer/{id}/index.json` | Package versions |
| `GET /v3/registration5-gz-semver2/{id}/index.json` | Download counts |

## File Locations

### Source Code
```
src/Dashboard/
├── Dashboard.csproj        # Project file
├── Program.cs              # Entry point, DI setup
├── App.razor               # Root component, router
├── _Imports.razor          # Global usings
├── Pages/                  # Routable pages
├── Layout/                 # MainLayout, NavMenu
├── Services/               # Data service + models
├── Properties/
│   └── launchSettings.json # Dev server settings
└── wwwroot/
    ├── index.html          # HTML shell
    ├── css/                # Stylesheets
    ├── data/               # JSON data files
    ├── images/             # Static images
    └── lib/                # Bootstrap
```

### Configuration Files
```
/
├── .editorconfig           # C# code style
├── global.json             # .NET SDK version
├── .gitignore              # Git ignores
└── .github/
    ├── copilot-instructions.md  # Copilot context
    └── workflows/
        ├── build-dashboard.yml
        └── update-dashboard-data.yml
```

### AI Context Files
```
.ai/
├── projectbrief.md         # Vision, goals, constraints
├── architecture.md         # System design
├── techContext.md          # This file
├── activeContext.md        # Current work
├── progress.md             # Status, backlog
└── decisions/              # ADRs
```

## Build Configuration

### Project Settings (`Dashboard.csproj`)
```xml
<TargetFramework>net10.0</TargetFramework>
<LangVersion>latest</LangVersion>
<Nullable>enable</Nullable>
<ImplicitUsings>enable</ImplicitUsings>
<RootNamespace>SkiaSharp.Dashboard</RootNamespace>
```

### Base Href Handling
- **Local**: `<base href="/" />` in `index.html`
- **Production**: CI uses `sed` to change to `<base href="/SkiaSharp/dashboard/" />`

## Constraints & Limitations

### GitHub Pages
- Static files only (no server-side code)
- Single branch deployment (we use `docs-live`)
- Must handle SPA routing (404.html or redirect)

### Blazor WASM
- Initial load includes .NET runtime (~2-3MB)
- No access to server-side secrets
- All API calls are client-side (CORS restrictions)

### Rate Limits
- GitHub: 5000 requests/hour (authenticated)
- Collectors run every 6 hours, well within limits
- If rate limited, collectors fail gracefully

### Browser Support
- Modern browsers only (WASM requirement)
- IE11 not supported
- Mobile browsers work but WASM is slower

## Environment Variables

### GitHub Actions
| Variable | Used In | Purpose |
|----------|---------|---------|
| `GITHUB_TOKEN` | All collectors | API authentication |
| `secrets.GITHUB_TOKEN` | Workflows | Auto-provided by Actions |

### Local Development
| Variable | Purpose |
|----------|---------|
| `GITHUB_TOKEN` | Higher rate limits for collector testing |

## Dependencies

### NuGet Packages
| Package | Version | Purpose |
|---------|---------|---------|
| `Microsoft.AspNetCore.Components.WebAssembly` | 10.0.0 | Blazor WASM framework |
| `Microsoft.AspNetCore.Components.WebAssembly.DevServer` | 10.0.0 | Dev server (dev only) |

### Frontend Libraries
| Library | Version | Purpose |
|---------|---------|---------|
| Bootstrap | 5.x | CSS framework |
| (none) | - | No JS frameworks |

## Useful Commands Reference

```bash
# Git
git status
git add -A
git commit -m "message"
git push -u origin dashboard

# .NET
dotnet build
dotnet run
dotnet publish -c Release
dotnet clean

# PowerShell
pwsh script.ps1 -Param value

# Playwright (for testing)
npx playwright install webkit
```
