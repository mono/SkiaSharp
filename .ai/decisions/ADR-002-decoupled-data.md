# ADR-002: Decoupled Data Architecture

## Status
**Accepted** - 2026-02-03

## Context
The dashboard displays data that changes independently of the application code. We needed to decide how data flows into the application.

## Decision
Use **decoupled JSON files** stored in `wwwroot/data/` that are:
1. Loaded at runtime via HTTP fetch
2. Updated independently by scheduled workflows
3. Separate from application deployment

## Rationale
- **Independent update cycles**: Data can refresh every 6 hours without rebuilding the app
- **Simpler caching**: JSON files can be cached by browser/CDN
- **Easier testing**: Can swap in test data files
- **No secrets in client**: Collectors run in CI with secrets, app only reads public JSON

## Consequences

### Positive
- Fast deployments (app rarely needs rebuild)
- Data stays fresh via scheduled updates
- Clear separation of concerns
- Easy to debug (just inspect JSON files)

### Negative
- Two workflows to maintain
- Slight delay on data updates (6 hour schedule)
- Extra HTTP request per data file

## Data Flow
```
GitHub API → Collector (CI) → JSON file → Dashboard App → User
```

## Alternatives Considered

1. **Build-time data**: Fetch during build, embed in app
   - Rejected: Requires full rebuild for data updates

2. **Client-side API calls**: App fetches from APIs directly
   - Rejected: CORS issues, rate limits, exposed in browser

3. **Backend server**: Proxy API calls through server
   - Rejected: Can't run server on GitHub Pages

## Related
- Architecture: `.ai/architecture.md`
- Workflows: `.github/workflows/`
