# Project Brief: SkiaSharp Dashboard

> This is the foundation document. All other context files build on this.

## Vision

Create a **community-facing dashboard** for the SkiaSharp project that provides transparency into project health, community activity, and helps maintainers triage contributions efficiently.

## Goals

1. **Community Visibility**: Show project health metrics (stars, forks, downloads) publicly
2. **Maintainer Efficiency**: AI-powered PR triage to prioritize review efforts
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
| **Maintainers** | Quickly triage PRs, track community health, identify trends |
| **Community** | Understand project health before adopting SkiaSharp |

## Success Criteria

- [ ] Dashboard loads in < 3 seconds
- [ ] Data refreshes automatically every 6 hours
- [ ] PR triage reduces time-to-first-review
- [ ] All pages work on mobile devices
- [ ] Zero manual intervention required for normal operation

## Relationship to SkiaSharp

This dashboard is a **companion project** to the main SkiaSharp library:

- **SkiaSharp** (main branch): Cross-platform 2D graphics library for .NET
- **Documentation** (docs branch): API docs and conceptual guides
- **Dashboard** (dashboard branch): This project - community metrics

The dashboard lives on an orphan branch to keep concerns separated. It shares the same repository for deployment convenience (GitHub Pages serves from `docs-live`).

## Key Stakeholders

- **Matthew** (Project Owner): Defines requirements, approves direction
- **SkiaSharp Maintainers**: Primary users of PR triage feature
- **Community**: Consumers of public metrics

## Constraints

1. **GitHub Pages hosting**: Static files only, no server-side code
2. **API rate limits**: GitHub API (5000/hr), NuGet API (unknown)
3. **Bundle size**: WASM should stay reasonable for load times
4. **No secrets in client**: All data must be pre-computed

## Timeline

| Phase | Status | Description |
|-------|--------|-------------|
| Foundation | ✅ Complete | Project setup, basic structure |
| Data Collection | ✅ Complete | Collector scripts created |
| Dashboard UI | ✅ Complete | All 5 pages implemented |
| CI/CD | ✅ Complete | Workflows created |
| Testing | 🔄 In Progress | Playwright setup, smoke tests |
| AI Context | 🔄 In Progress | Memory bank creation |
| Deployment | ⏳ Pending | Push to remote, verify live site |
| Polish | ⏳ Pending | Logo, branding, error handling |
