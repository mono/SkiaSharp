# Progress & Backlog

> Project status, completed work, pending tasks, and future ideas.

## Current Status

**Overall**: 🟡 MVP Complete, Not Yet Deployed

| Area | Status | Notes |
|------|--------|-------|
| Foundation | ✅ Complete | Project setup, structure |
| UI Pages | ✅ Complete | All 5 pages working |
| Data Models | ✅ Complete | All DTOs defined |
| Collectors | ✅ Complete | 4 scripts created |
| CI/CD | ✅ Complete | 2 workflows ready |
| Local Testing | ✅ Complete | Runs on localhost |
| Playwright | ✅ Complete | MCP configured |
| AI Context | ✅ Complete | Memory bank created |
| Deployment | 🟡 Pushed | Waiting for workflow to run |
| Real Data | ⏳ Pending | Collectors not run yet |
| Polish | ⏳ Pending | Logo, branding |

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
- [x] Create Community collector
- [x] Create PR Triage collector

### Milestone 3: UI ✅
- [x] Overview page with summary cards
- [x] GitHub stats page
- [x] NuGet downloads page
- [x] Community page
- [x] PR Triage page
- [x] Navigation sidebar
- [x] Responsive layout
- [x] Custom dashboard CSS

### Milestone 4: CI/CD ✅
- [x] build-dashboard.yml (push trigger)
- [x] update-dashboard-data.yml (schedule + manual)
- [x] Base href sed replacement
- [x] Concurrency controls

### Milestone 5: Testing & Documentation ✅
- [x] Playwright MCP setup
- [x] Screenshot capability
- [x] README with screenshot
- [x] copilot-instructions.md
- [x] Memory bank files

## Current Sprint / In Progress

### Deployment & Verification
- [ ] Push dashboard branch to origin
- [ ] Verify build workflow runs
- [ ] Verify site deploys to GitHub Pages
- [ ] Run manual data collection
- [ ] Verify data appears on live site

## Backlog (Prioritized)

### High Priority
- [ ] Add SkiaSharp logo to sidebar
- [ ] Run collectors to populate real data
- [ ] Test all pages with real data
- [ ] Handle empty data states gracefully
- [ ] Add error boundaries for failed data loads

### Medium Priority
- [ ] Add loading spinners/skeletons
- [ ] Improve mobile responsiveness
- [ ] Add "last updated" to all pages
- [ ] Add links to specific issues/PRs on GitHub
- [ ] Smoke tests for CI

### Low Priority
- [ ] Add charts for trends (stars over time, etc.)
- [ ] Add historical data storage
- [ ] Dark mode support
- [ ] PWA support (offline)
- [ ] Performance optimization

## Future Ideas (Icebox)

These are ideas that may or may not be implemented:

1. **AI-powered PR triage**: Use OpenAI/Copilot API for actual analysis
2. **Issue categorization**: Auto-categorize issues by type
3. **Release tracking**: Show release timeline and changelog
4. **Dependency dashboard**: Track dependency updates
5. **Build status**: Show CI/CD pipeline status
6. **Discussion highlights**: Show popular discussions
7. **Contributor spotlight**: Featured contributor of the month
8. **Download trends**: NuGet download charts over time
9. **API health**: Monitor SkiaSharp API surface changes
10. **Community events**: Show upcoming events/meetups

## Known Issues

| Issue | Severity | Workaround |
|-------|----------|------------|
| Logo missing | Low | Shows text "Dashboard" instead |
| Data is zeros | Expected | Will be populated by collectors |
| No error handling | Medium | Page shows generic error |

## Metrics (Once Deployed)

Track these to measure success:

- [ ] Page load time (target: < 3s)
- [ ] Data freshness (target: < 6 hours old)
- [ ] Mobile usability score
- [ ] Error rate in browser console

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 0.1.0 | 2026-02-03 | Initial implementation |

## Release Checklist (For Future Releases)

- [ ] Update version in progress.md
- [ ] Test all pages locally
- [ ] Verify collectors work
- [ ] Check mobile responsiveness
- [ ] Update activeContext.md
- [ ] Commit with clear message
- [ ] Push and verify deployment
- [ ] Check live site
