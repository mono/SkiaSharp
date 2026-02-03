# ADR-001: Orphan Branch for Dashboard

## Status
**Accepted** - 2026-02-03

## Context
The SkiaSharp repository has existing branches (`main`, `docs`, `docs-live`) with established history. We needed to decide how to structure the new dashboard branch.

## Decision
Create the `dashboard` branch as an **orphan branch** using `git checkout --orphan dashboard`.

## Rationale
- Dashboard is a separate project, not derived from SkiaSharp source code
- Keeps git history clean and relevant to each branch's purpose
- Smaller clone size if someone only wants the dashboard code
- Clear separation of concerns

## Consequences

### Positive
- Clean, minimal history for dashboard
- No accidental merge of unrelated history
- Clear ownership and purpose

### Negative
- Cannot easily merge FROM other branches
- New contributors might be confused by disconnected history

## Alternatives Considered

1. **Branch from main**: Would carry SkiaSharp source history
2. **Separate repository**: Would complicate deployment to same GitHub Pages
3. **Folder in main**: Would clutter main branch with unrelated code

## Related
- Architecture: `.ai/architecture.md`
