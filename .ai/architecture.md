# Architecture Overview

> System design, component relationships, and key technical decisions.

## High-Level Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                        GitHub Pages                              │
│                  (docs-live branch)                              │
├─────────────────────────────────────────────────────────────────┤
│  /                    │  /docs/              │  /dashboard/      │
│  (redirect to docs)   │  (DocFX docs)        │  (This project)   │
└─────────────────────────────────────────────────────────────────┘
                                                      │
                                    ┌─────────────────┴─────────────────┐
                                    │     Blazor WASM App               │
                                    │  ┌─────────────────────────────┐  │
                                    │  │  Pages (5)                  │  │
                                    │  │  - Home (Overview)          │  │
                                    │  │  - GitHub                   │  │
                                    │  │  - NuGet                    │  │
                                    │  │  - Community                │  │
                                    │  │  - PR Triage                │  │
                                    │  └─────────────────────────────┘  │
                                    │              │                    │
                                    │  ┌───────────▼─────────────────┐  │
                                    │  │  DashboardDataService       │  │
                                    │  │  (HTTP fetch JSON)          │  │
                                    │  └───────────┬─────────────────┘  │
                                    │              │                    │
                                    │  ┌───────────▼─────────────────┐  │
                                    │  │  wwwroot/data/*.json        │  │
                                    │  │  (Static JSON files)        │  │
                                    │  └─────────────────────────────┘  │
                                    └───────────────────────────────────┘
```

## Data Flow

```
┌──────────────────┐     ┌──────────────────┐     ┌──────────────────┐
│  GitHub API      │     │  NuGet API       │     │  (Future: AI)    │
└────────┬─────────┘     └────────┬─────────┘     └────────┬─────────┘
         │                        │                        │
         ▼                        ▼                        ▼
┌──────────────────────────────────────────────────────────────────┐
│                    PowerShell Collectors                          │
│  collect-github.ps1 │ collect-nuget.ps1 │ collect-pr-triage.ps1  │
└────────────────────────────────┬─────────────────────────────────┘
                                 │
                                 ▼
┌──────────────────────────────────────────────────────────────────┐
│                    JSON Data Files                                │
│  github-stats.json │ nuget-stats.json │ pr-triage.json           │
└────────────────────────────────┬─────────────────────────────────┘
                                 │
                                 ▼
┌──────────────────────────────────────────────────────────────────┐
│              GitHub Actions Commit & Deploy                       │
│         (update-dashboard-data.yml → docs-live branch)           │
└────────────────────────────────┬─────────────────────────────────┘
                                 │
                                 ▼
┌──────────────────────────────────────────────────────────────────┐
│                    Blazor WASM App                                │
│              (fetches JSON at runtime)                            │
└──────────────────────────────────────────────────────────────────┘
```

## Component Breakdown

### Pages (5 total)

| Page | Route | Purpose | Data Source |
|------|-------|---------|-------------|
| Home | `/` | Overview dashboard with summary cards | All JSON files |
| GitHub | `/github` | Detailed GitHub stats | `github-stats.json` |
| NuGet | `/nuget` | Package download stats | `nuget-stats.json` |
| Community | `/community` | Contributors and activity | `community-stats.json` |
| PR Triage | `/pr-triage` | AI-analyzed PR queue | `pr-triage.json` |

### Services

| Service | Purpose |
|---------|---------|
| `DashboardDataService` | Fetches and deserializes JSON data files |

### Data Models (Records)

| Model | File | Purpose |
|-------|------|---------|
| `GitHubStats` | `GitHubStats.cs` | Repository, issues, PRs, activity |
| `NuGetStats` | `NuGetStats.cs` | Packages and download counts |
| `CommunityStats` | `CommunityStats.cs` | Contributors and growth |
| `PrTriageStats` | `PrTriageStats.cs` | Triaged PRs with AI reasoning |

### Collectors (PowerShell)

| Script | APIs Used | Output |
|--------|-----------|--------|
| `collect-github.ps1` | GitHub REST API | Stars, forks, issues, PRs, commits |
| `collect-nuget.ps1` | NuGet API | Downloads per package and version |
| `collect-community.ps1` | GitHub REST API | Contributors, recent commits |
| `collect-pr-triage.ps1` | GitHub REST API + (future) AI | Categorized PRs with reasoning |

## Deployment Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                     GitHub Repository                            │
│                     mono/SkiaSharp                               │
├─────────────────┬─────────────────┬─────────────────────────────┤
│  main branch    │  docs branch    │  dashboard branch (orphan)  │
│  (library code) │  (DocFX source) │  (this project)             │
└─────────────────┴─────────────────┴──────────────┬──────────────┘
                                                   │
                            ┌──────────────────────┴──────────────┐
                            │                                     │
                    ┌───────▼───────┐                 ┌───────────▼───────────┐
                    │ Push trigger  │                 │ Schedule (6h) trigger │
                    │ build-dashboard│                 │ update-dashboard-data │
                    └───────┬───────┘                 └───────────┬───────────┘
                            │                                     │
                            │  1. Build WASM                      │  1. Run collectors
                            │  2. Set base href                   │  2. Update JSON
                            │  3. Deploy                          │  3. Build WASM
                            │                                     │  4. Deploy
                            │                                     │
                            └─────────────────┬───────────────────┘
                                              │
                                              ▼
                            ┌─────────────────────────────────────┐
                            │  peaceiris/actions-gh-pages@v4      │
                            │  - publish_branch: docs-live        │
                            │  - destination_dir: dashboard       │
                            │  - keep_files: true                 │
                            └─────────────────────────────────────┘
                                              │
                                              ▼
                            ┌─────────────────────────────────────┐
                            │  https://mono.github.io/SkiaSharp/  │
                            │           └── dashboard/            │
                            └─────────────────────────────────────┘
```

## Key Design Decisions

### 1. Orphan Branch
**Decision**: Dashboard lives on an orphan branch with no history from main.
**Why**: 
- Dashboard is a separate project, not derived from SkiaSharp source
- Keeps git history clean and relevant
- Smaller clone size for dashboard-only contributors

### 2. Decoupled Data
**Decision**: JSON files are separate from the app; updated independently.
**Why**:
- Can update data without rebuilding the app
- Can update app without touching data
- Simpler CI/CD (two focused workflows instead of one complex one)
- Data persists across app rebuilds

### 3. Static Site (Blazor WASM)
**Decision**: Client-side only, no server.
**Why**:
- GitHub Pages is free and reliable
- No server infrastructure to maintain
- WASM provides rich interactivity without backend

### 4. PowerShell Collectors
**Decision**: Use PowerShell for data collection scripts.
**Why**:
- Cross-platform (works in GitHub Actions Linux runners)
- Rich HTTP/JSON support built-in
- Easy to read and modify
- Familiar to .NET developers

### 5. Heuristic-based PR Triage (for now)
**Decision**: Use simple rules for PR categorization initially.
**Why**:
- AI integration (OpenAI/Copilot API) adds complexity and cost
- Heuristics work for basic triage (old PRs, small PRs, approved PRs)
- Can add AI layer later without changing the data format

### 6. keep_files Deployment
**Decision**: Use `keep_files: true` in GitHub Pages action.
**Why**:
- Allows dashboard and docs to deploy independently
- Neither deployment overwrites the other
- Simple solution to multi-source single-branch problem
