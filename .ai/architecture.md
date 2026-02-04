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
                                    │  │  - Home (Insights + Hot)    │  │
                                    │  │  - Issues (Hot + Filters)   │  │
                                    │  │  - Pull Requests (Triage)   │  │
                                    │  │  - Community                │  │
                                    │  │  - NuGet (Grouped)          │  │
                                    │  └─────────────────────────────┘  │
                                    │              │                    │
                                    │  ┌───────────▼─────────────────┐  │
                                    │  │  DashboardDataService       │  │
                                    │  │  (HTTP fetch JSON)          │  │
                                    │  └───────────┬─────────────────┘  │
                                    │              │                    │
                                    │  ┌───────────▼─────────────────┐  │
                                    │  │  wwwroot/data/*.json        │  │
                                    │  │  (Generated from cache)     │  │
                                    │  └─────────────────────────────┘  │
                                    └───────────────────────────────────┘
```

## Data Flow (New Cache Architecture)

```
┌──────────────────┐  ┌──────────────────┐  ┌──────────────────┐
│  GitHub API      │  │  NuGet API       │  │  Contributors    │
└────────┬─────────┘  └────────┬─────────┘  └────────┬─────────┘
         │                     │                     │
         ▼                     ▼                     ▼
┌──────────────────────────────────────────────────────────────────┐
│              sync-data-cache.yml (Hourly + On Push)              │
│                                                                  │
│  Step 1a: NuGet (every 6 hours)                                 │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │ sync nuget → commit & push                                 │ │
│  └────────────────────────────────────────────────────────────┘ │
│                                                                  │
│  Step 1b: Community (every 6 hours)                             │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │ sync community → commit & push (~21 API calls)             │ │
│  └────────────────────────────────────────────────────────────┘ │
│                                                                  │
│  Step 2: GitHub items (Layer 1)                                 │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │ sync github --items-only → commit & push                   │ │
│  │ Also saves repo.json (stars, forks, watchers)              │ │
│  └────────────────────────────────────────────────────────────┘ │
│                                                                  │
│  Step 3: GitHub engagement (Layer 2) - checkpoint loop          │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │ while true:                                                │ │
│  │   sync github --engagement-only --engagement-count 100     │ │
│  │   → commit & push (checkpoint)                             │ │
│  │   if rate_limited or all_done: break                       │ │
│  └────────────────────────────────────────────────────────────┘ │
└────────────────────────────────┬─────────────────────────────────┘
                                 │
                                 ▼
┌──────────────────────────────────────────────────────────────────┐
│                  docs-data-cache branch                          │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐  │
│  │ github/         │  │ community/      │  │ nuget/          │  │
│  │ ├─ repo.json    │  │ └─ contributors │  │ ├─ index.json   │  │
│  │ ├─ index.json   │  │    .json        │  │ └─ packages/*   │  │
│  │ └─ items/*.json │  │                 │  │                 │  │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘  │
└────────────────────────────────┬─────────────────────────────────┘
                                 │
                                 ▼
┌──────────────────────────────────────────────────────────────────┐
│              build-dashboard.yml (Every 6 Hours)                 │
│                                                                  │
│  ┌─────────────────────────────────────────────────────────────┐ │
│  │ SkiaSharp.Collector generate command                        │ │
│  │ - Reads repo.json → github-stats.json (stars, forks)       │ │
│  │ - Reads contributors.json → community-stats.json           │ │
│  │ - Calculates engagement scores, hot issues                  │ │
│  └─────────────────────────────────────────────────────────────┘ │
└────────────────────────────────┬─────────────────────────────────┘
                                 │
                                 ▼
┌──────────────────────────────────────────────────────────────────┐
│                    Dashboard JSON Files                          │
│  github-stats │ community-stats │ nuget-stats │ issues │ pr-triage
└────────────────────────────────┬─────────────────────────────────┘
                                 │
                                 ▼
┌──────────────────────────────────────────────────────────────────┐
│              peaceiris/actions-gh-pages@v4                       │
│              → docs-live/dashboard/                              │
└────────────────────────────────┬─────────────────────────────────┘
                                 │
                                 ▼
┌──────────────────────────────────────────────────────────────────┐
│              https://mono.github.io/SkiaSharp/dashboard/         │
└──────────────────────────────────────────────────────────────────┘
```

## Branch Relationships

```
┌─────────────────────────────────────────────────────────────────┐
│                     GitHub Repository                            │
│                     mono/SkiaSharp                               │
├─────────────────┬─────────────────┬─────────────────────────────┤
│  main branch    │  docs branch    │  docs-dashboard (orphan)    │
│  (library code) │  (DocFX source) │  (dashboard source)         │
│                 │                 │                             │
│  DO NOT MODIFY  │  DO NOT MODIFY  │  ← Active development       │
└─────────────────┴─────────────────┴──────────────┬──────────────┘
                                                   │
                                    ┌──────────────┼──────────────┐
                                    │              │              │
                            ┌───────▼───────┐  ┌───▼────────────┐ │
                            │docs-data-cache│  │ docs-live      │ │
                            │(orphan)       │  │                │ │
                            │               │  │ /dashboard/    │ │
                            │ Cached API    │  │ /docs/         │ │
                            │ data (JSON)   │  │ /              │ │
                            └───────────────┘  └────────────────┘ │
                                    ▲                    ▲        │
                                    │                    │        │
                            sync workflow        build workflow   │
                            (hourly)             (6-hourly)       │
                                                                  │
                                    Push to docs-dashboard ───────┘
                                    triggers BOTH workflows
```

## Component Breakdown

### Pages (5 total)

| Page | Route | Purpose | Key Features |
|------|-------|---------|--------------|
| Home | `/` | Overview dashboard | Summary cards, charts, top 3 hot issues |
| Issues | `/issues` | Issue exploration | Hot issues section, filters, engagement scores |
| Pull Requests | `/pull-requests` | PR triage | 5 categories, size/age filters |
| Community | `/community` | Contributors | Stars, forks, MS/community breakdown |
| NuGet | `/nuget` | Package downloads | Grouped layout, legacy toggle, 50 packages |

### Collector CLI Commands

| Command | Purpose | API Calls |
|---------|---------|-----------|
| `sync github --items-only` | Layer 1: items + repo.json | ~35 pages |
| `sync github --engagement-only` | Layer 2: comments/reactions | 100×3 per batch |
| `sync community` | Contributors + MS membership | ~21 |
| `sync nuget` | Package downloads | ~50 |
| `generate` | Cache → dashboard JSON | 0 (file I/O) |

### Services

| Service | Location | Purpose |
|---------|----------|---------|
| `DashboardDataService` | Dashboard | Fetches JSON at runtime |
| `GitHubService` | Collector | GitHub API client |
| `NuGetService` | Collector | NuGet API client |
| `CacheService` | Collector | Read/write cache files |
| `EngagementCalculator` | Collector | Hot issue scoring |
| `LabelParser` | Collector | Parse type/area/backend labels |

## Key Design Decisions

### 1. Orphan Branches
**Decision**: Dashboard and cache live on orphan branches.
**Why**: 
- Completely separate projects, no shared history
- Smaller clone size
- Clear separation of concerns

### 2. Decoupled Sync and Build
**Decision**: Sync hourly, build every 6 hours.
**Why**:
- Sync can fail partially and resume
- Build uses whatever data is available
- Rate limits don't block dashboard updates

### 3. Layered Sync with Checkpoint Loop
**Decision**: Layer 1 (all items) + Layer 2 (engagement, 100/batch in while loop).
**Why**:
- Get full coverage quickly (Layer 1)
- Build up engagement data over time (Layer 2)
- Commit after every 100 items (checkpoint)
- While loop continues until rate limit or done
- Exit code 1 signals "more work to do"

### 4. Smart Engagement Sync
**Decision**: Only fetch engagement for items where `UpdatedAt > EngagementSyncedAt`.
**Why**:
- No wasted API calls on unchanged items
- Prioritizes recently active items (`OrderByDescending(UpdatedAt)`)
- Skip list excludes failed items with cooldowns
- Efficient even with 3000+ items in cache

### 5. Skip List with Cooldowns
**Decision**: Failed items go to skip list with error-specific cooldowns.
**Why**:
- 404 (deleted) = 7 days (rarely recovers)
- 403 (forbidden) = 1 day (permissions may change)
- Other errors = 1 hour (transient)

### 6. Community Sync Separately
**Decision**: Separate `sync community` command, runs every 6 hours with NuGet.
**Why**:
- Contributors list needs ~21 API calls (1 list + 20 MS checks)
- Doesn't need hourly updates
- Separate concerns from issue/PR sync
- Cache stores MS membership to avoid re-checking

### 7. Always Write Files
**Decision**: Generate always writes JSON, even if cache is empty.
**Why**:
- Dashboard reflects actual cache state
- Supports cache reset scenarios
- No stale data from previous commits

### 8. Engagement Scoring
**Decision**: Score = `(Comments × 3) + (Reactions × 1) + (Contributors × 2) + freshness bonuses`
**Why**:
- Comments show active discussion
- Multiple contributors show broader interest
- Recent activity gets bonus
- Simple, explainable formula

### 9. Hot Issue Detection
**Decision**: Hot = current score > historical score AND score > 5
**Why**:
- Trending up (not just high absolute score)
- Minimum threshold avoids noise
- 7-day comparison window

## SPA Routing on GitHub Pages

```
User visits: /SkiaSharp/dashboard/issues
                    │
                    ▼
┌─────────────────────────────────────────────────────────────────┐
│  GitHub Pages: File not found → serve 404.html                  │
└─────────────────────────────────────────────────────────────────┘
                    │
                    ▼
┌─────────────────────────────────────────────────────────────────┐
│  404.html (segmentCount = 2):                                   │
│  1. Keep /SkiaSharp/dashboard                                   │
│  2. Encode /issues in query string                              │
│  3. Redirect to /SkiaSharp/dashboard/?p=/issues                 │
└─────────────────────────────────────────────────────────────────┘
                    │
                    ▼
┌─────────────────────────────────────────────────────────────────┐
│  index.html script:                                             │
│  1. Read ?p=/issues from URL                                    │
│  2. history.replaceState to /SkiaSharp/dashboard/issues         │
│  3. Blazor router handles /issues route                         │
└─────────────────────────────────────────────────────────────────┘
                    │
                    ▼
        User sees Issues page ✓
```
