# Project Brief: SkiaSharp Dashboard

> This is the foundation document. All other context files build on this.

## Vision

Create a **community-facing dashboard** for the SkiaSharp project that provides transparency into project health, community activity, and helps maintainers triage contributions efficiently.

## Goals

1. **Community Visibility**: Show project health metrics (stars, forks, downloads) publicly
2. **Maintainer Efficiency**: Engagement scoring to identify hot issues and priority items
3. **Transparency**: Open display of activity, contributors, and project velocity
4. **Low Maintenance**: Automated data updates, minimal manual intervention

## Non-Goals

- ❌ Not a replacement for GitHub's native insights
- ❌ Not an admin panel (no write operations)
- ❌ Not a documentation site (that's the `docs` branch)
- ❌ Not monitoring/alerting system

## Target Users

| User | Needs |
|------|-------|
| **Contributors** | See project activity, find good-first-issues, understand review queue |
| **Maintainers** | Quickly triage PRs, track community health, identify hot issues |
| **Community** | Understand project health before adopting SkiaSharp |

## Success Criteria

- [x] Dashboard loads in < 3 seconds
- [x] Data refreshes automatically (hourly sync, 6-hour build)
- [x] Hot issues highlight trending discussions
- [x] All pages work on mobile devices
- [x] Zero manual intervention required for normal operation

## Architecture

```
docs-dashboard branch          docs-data-cache branch        docs-live branch
(Source code)                  (Cached API data)             (Deployed site)
       │                              │                            │
       │                              │                            │
       └──→ sync workflow ──────────→ │                            │
       │    (hourly + on push)        │                            │
       │                              │                            │
       └──→ build workflow ←─────────→┘                            │
            (every 6 hours)           │                            │
                   │                  │                            │
                   └─────────────────→┴───────────────────────────→│
                                      generate + deploy
```

## Relationship to SkiaSharp

This dashboard is a **companion project** to the main SkiaSharp library:

- **SkiaSharp** (main branch): Cross-platform 2D graphics library for .NET
- **Documentation** (docs branch): API docs and conceptual guides
- **Dashboard** (docs-dashboard branch): This project - community metrics
- **Data Cache** (docs-data-cache branch): Cached API data for dashboard

The dashboard lives on orphan branches to keep concerns separated. It shares the same repository for deployment convenience (GitHub Pages serves from `docs-live`).

## Key Stakeholders

- **Matthew** (Project Owner): Defines requirements, approves direction
- **SkiaSharp Maintainers**: Primary users of PR triage feature
- **Community**: Consumers of public metrics

## Constraints

1. **GitHub Pages hosting**: Static files only, no server-side code
2. **API rate limits**: GitHub API (5000/hr), proactive checking
3. **Bundle size**: WASM should stay reasonable for load times
4. **No secrets in client**: All data must be pre-computed

## Project Timeline

| Phase | Status | Description |
|-------|--------|-------------|
| Phase 1 | ✅ Complete | Foundation - project setup, basic structure |
| Phase 2 | ✅ Complete | Dashboard UI - all 5 pages, charts, filters |
| Phase 2.7 | ✅ Complete | NuGet redesign - grouped layout, 50 packages |
| Phase 3 | ✅ Complete | Collector CLI - .NET replaces PowerShell |
| Phase 4 | ✅ Complete | Data Cache - decoupled sync, engagement scoring |

## Live Site

**URL**: https://mono.github.io/SkiaSharp/dashboard/
