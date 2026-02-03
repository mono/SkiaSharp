# Progress & Backlog

> Project status, completed work, pending tasks, and future ideas.

## Current Status

**Overall**: 🟢 MVP Live with Real Data

| Area | Status | Notes |
|------|--------|-------|
| Foundation | ✅ Complete | Project setup, structure |
| UI Pages | ✅ Complete | All 5 pages working |
| Data Models | ✅ Complete | All DTOs defined |
| Collectors | ✅ Complete | 4 scripts, MS/Community split |
| CI/CD | ✅ Complete | Single unified workflow |
| Local Testing | ✅ Complete | Runs on localhost |
| Playwright | ✅ Complete | MCP configured |
| AI Context | ✅ Complete | Memory bank created |
| Deployment | ✅ Live | https://mono.github.io/SkiaSharp/dashboard/ |
| Real Data | ✅ Working | GitHub, Community stats show real data |
| SPA Routing | ✅ Fixed | Root 404.html handles deep links |
| Logo | ✅ Added | nuget.png used |

## Completed Milestones

### Milestone 1: Foundation ✅
- [x] Create orphan dashboard branch
- [x] Initialize Blazor WASM project
- [x] Configure .NET 10, C# 14
- [x] Add .editorconfig, global.json
- [x] Set up project structure

### Milestone 2: Data Layer ✅
- [x] Define data models (records)
- [x] Create DashboardDataService
- [x] Create placeholder JSON files
- [x] Create GitHub collector
- [x] Create NuGet collector
- [x] Create Community collector (with MS/Community split)
- [x] Create PR Triage collector

### Milestone 3: UI ✅
- [x] Overview page with summary cards
- [x] GitHub stats page
- [x] NuGet downloads page
- [x] Community page
- [x] PR Triage page
- [x] Navigation sidebar with logo
- [x] Responsive layout
- [x] Custom dashboard CSS
- [x] Card layout improvements (3-col + full-width)
- [x] Microsoft vs Community contributor breakdown

### Milestone 4: CI/CD ✅
- [x] Single unified workflow (build-dashboard.yml)
- [x] Data collection integrated into build
- [x] Base href sed replacement (after publish)
- [x] 404.html for SPA routing in /dashboard/
- [x] Root 404.html for deep link routing
- [x] Concurrency controls

### Milestone 5: Testing & Documentation ✅
- [x] Playwright MCP setup
- [x] Screenshot capability
- [x] README with screenshot
- [x] copilot-instructions.md
- [x] Memory bank files

## Known Issues

| Issue | Severity | Status |
|-------|----------|--------|
| NuGet totalDownloads shows 0 | Low | Collector calculation issue |
| PR Triage shows dashes | Low | May be enum deserialization |

## Backlog (Prioritized)

### High Priority
- [ ] Fix NuGet total downloads calculation
- [ ] Verify PR Triage data displays correctly
- [ ] Add error boundaries for failed data loads

### Medium Priority
- [ ] Add loading spinners/skeletons
- [ ] Improve mobile responsiveness
- [ ] Add "last updated" timestamp to all pages
- [ ] Add links to specific issues/PRs on GitHub

### Low Priority
- [ ] Add charts for trends (stars over time, etc.)
- [ ] Add historical data storage
- [ ] Dark mode support
- [ ] PWA support (offline)

## Future Ideas (Icebox)

1. **AI-powered PR triage**: Use OpenAI/Copilot API for actual analysis
2. **Issue categorization**: Auto-categorize issues by type
3. **Release tracking**: Show release timeline and changelog
4. **Dependency dashboard**: Track dependency updates
5. **Build status**: Show CI/CD pipeline status

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 0.2.0 | 2026-02-03 | MS/Community split, SPA routing fix, unified workflow |
| 0.1.0 | 2026-02-03 | Initial implementation |
