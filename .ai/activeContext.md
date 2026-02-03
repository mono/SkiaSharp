# Active Context

> Current focus, recent changes, immediate next steps, and working patterns.
> **Update this file frequently** - it's the "working memory" for AI assistants.

## Current Focus

**Phase**: Live & Iterating
**Status**: Dashboard deployed with real data, SPA routing fixed

The dashboard is live at https://mono.github.io/SkiaSharp/dashboard/ with real GitHub and Community data.

## Recent Changes (Last Session)

### 2026-02-03 (Latest)
1. ✅ Moved MS/Community breakdown from Home to Community page
2. ✅ Fixed Microsoft detection - now uses public user profile `company` field
3. ✅ Added Microsoft badge to top contributors on Community page

### 2026-02-03 (Evening)
1. ✅ Merged build + data workflows into single `build-dashboard.yml`
2. ✅ Fixed SPA routing 404 issue with root-level 404.html
3. ✅ Added Microsoft vs Community contributor breakdown
4. ✅ Improved card layouts (3-col stats, full-width rows)
5. ✅ Fixed contributor collector to check Microsoft org membership
6. ✅ Updated memory bank files

### 2026-02-03 (Earlier)
1. ✅ Created orphan `dashboard` branch
2. ✅ Initialized Blazor WASM project (.NET 10, C# 14)
3. ✅ Added all 5 pages: Overview, GitHub, NuGet, Community, PR Triage
4. ✅ Created data models and DashboardDataService
5. ✅ Created 4 PowerShell collector scripts
6. ✅ Fixed base href for GitHub Pages (sed AFTER publish)
7. ✅ Set up Playwright MCP for visual testing
8. ✅ Created AI context system (copilot-instructions + memory bank)
9. ✅ Added SkiaSharp logo (nuget.png)

## Key Technical Fixes Applied

### SPA Routing on GitHub Pages
- **Problem**: Direct links like `/dashboard/community` return 404
- **Solution**: Two-part fix:
  1. Root `404.html` in docs-live - redirects `/dashboard/*` URLs to `/dashboard/` with path in sessionStorage
  2. `index.html` script reads sessionStorage and updates browser history
  3. Blazor then handles the route normally

### Base href Timing
- **Problem**: `sed` was running before publish, but publish regenerates index.html
- **Solution**: Run `sed` AFTER `dotnet publish` on the output file

### Data Workflow Race Condition
- **Problem**: Separate build and data workflows caused data to be overwritten
- **Solution**: Single unified workflow that collects data → builds → deploys

## Files Recently Modified

- `.github/workflows/build-dashboard.yml` - Unified workflow with root 404.html deploy
- `collectors/collect-community.ps1` - Added Microsoft org membership check
- `src/Dashboard/Services/CommunityStats.cs` - Added MicrosoftContributors, CommunityContributors, IsMicrosoft
- `src/Dashboard/Pages/Home.razor` - Microsoft/Community contributor display
- `src/Dashboard/wwwroot/css/dashboard.css` - New grid classes, MS/Community colors
- `src/Dashboard/wwwroot/index.html` - SessionStorage redirect handling
- `src/Dashboard/wwwroot/root-404.html` - New file for root-level 404

## Immediate Next Steps

1. [ ] Verify SPA routing fix works (test direct link to /dashboard/community)
2. [ ] Fix NuGet total downloads (shows 0)
3. [ ] Verify PR Triage data displays correctly
4. [ ] Take final screenshot with all data working

## Working Patterns

### When Adding a New Page
1. Create `Pages/{Name}.razor` with `@page "/{route}"` directive
2. Inject `DashboardDataService` if data needed
3. Add loading state and null checks
4. Add navigation link in `Layout/NavMenu.razor`
5. Add entry to Overview page if it should appear there

### When Adding New Data
1. Create/update model in `Services/`
2. Add JSON file in `wwwroot/data/`
3. Add method to `DashboardDataService`
4. Create/update collector script in `collectors/`
5. Workflow automatically runs collectors

### When Debugging
1. Use Playwright MCP to navigate and screenshot
2. Check browser console for errors
3. Verify JSON files are valid and accessible
4. Check network tab for failed requests

## Key Decisions Made This Session

| Decision | Rationale |
|----------|-----------|
| Single unified workflow | Avoids race conditions between data and build |
| Root 404.html redirect | Only way to handle SPA routing on GitHub Pages |
| Microsoft org check | Shows contributor breakdown; limited to top 20 to avoid API rate limits |
| sessionStorage for redirect | Preserves original URL across redirect |

## Context for Next AI Session

When resuming work:
1. Read ALL files in `.ai/` folder first
2. Check `progress.md` for current status
3. Check this file for recent changes
4. Branch is `dashboard` (orphan, no relation to main)
5. Live at https://mono.github.io/SkiaSharp/dashboard/

## Open Questions

1. Should PR triage use actual AI? (Would need API key management)
2. How to handle historical data for trends? (Store snapshots over time?)
3. Should we add charts/visualizations?

## Notes

- NuGet totalDownloads shows 0 - collector calculation may be wrong
- PR Triage shows dashes - may be enum deserialization issue
- Microsoft org check requires `read:org` scope - may show 0 if token lacks permission
