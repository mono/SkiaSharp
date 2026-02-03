# Active Context

> Current focus, recent changes, immediate next steps, and working patterns.
> **Update this file frequently** - it's the "working memory" for AI assistants.

## Current Focus

**Phase**: Testing & Deployment
**Status**: Ready to push to remote

We have completed the initial implementation and are preparing for first deployment.

## Recent Changes (Last Session)

### 2026-02-03
1. ✅ Created orphan `dashboard` branch
2. ✅ Initialized Blazor WASM project (.NET 10, C# 14)
3. ✅ Added all 5 pages: Overview, GitHub, NuGet, Community, PR Triage
4. ✅ Created data models and DashboardDataService
5. ✅ Created 4 PowerShell collector scripts
6. ✅ Created CI/CD workflows (build + data update)
7. ✅ Fixed base href for local vs production
8. ✅ Set up Playwright MCP for visual testing
9. ✅ Took screenshot for README (1280x720)
10. ✅ Created AI context system (copilot-instructions + memory bank)
11. ✅ Committed initial implementation

## Immediate Next Steps

1. [ ] Push `dashboard` branch to remote origin
2. [ ] Verify GitHub Actions workflow runs
3. [ ] Check deployed site at https://mono.github.io/SkiaSharp/dashboard/
4. [ ] Run data collection manually to populate real data
5. [ ] Add SkiaSharp logo to sidebar

## Current Blockers

None currently.

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
5. Update workflow if new collector added

### When Debugging
1. Use Playwright MCP to navigate and screenshot
2. Check browser console for errors
3. Verify JSON files are valid and accessible
4. Check network tab for failed requests

## Files Currently Being Modified

None - just committed everything.

## Key Decisions Made This Session

| Decision | Rationale |
|----------|-----------|
| Orphan branch | Clean separation from main SkiaSharp code |
| Decoupled data/app | Independent update cycles |
| PowerShell collectors | Cross-platform, familiar to .NET devs |
| `keep_files: true` deployment | Allows dashboard + docs to coexist |
| Heuristic PR triage first | Simpler than AI, can add AI later |
| Playwright MCP | Visual debugging for AI assistants |
| Memory bank pattern | Persistent context across sessions |

## Context for Next AI Session

When resuming work:
1. Read ALL files in `.ai/` folder first
2. Check `progress.md` for current status
3. Check this file for recent changes
4. Branch is `dashboard` (orphan, no relation to main)
5. App is Blazor WASM, runs on localhost:5050

## Open Questions

1. Should we add charts/visualizations? (Consider lightweight Blazor chart library)
2. Should PR triage use actual AI? (Would need API key management)
3. How to handle historical data for trends? (Store snapshots over time?)

## Notes

- Logo is missing - need to add SkiaSharp logo to `wwwroot/images/logo.png`
- Data files have placeholder zeros - will be populated by collectors
- PR triage uses heuristics, not AI, for now
