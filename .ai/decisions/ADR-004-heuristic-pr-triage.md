# ADR-004: Heuristic PR Triage (Phase 1)

## Status
**Accepted** - 2026-02-03

## Context
The PR Triage feature needs to categorize pull requests into:
- Ready to Merge (simple, approved, good idea)
- Needs Review (complex, needs attention)
- Consider Closing (stale, concept doesn't fit)

We needed to decide how to implement this categorization.

## Decision
**Phase 1**: Use **rule-based heuristics** based on PR metadata.

## Rationale
- **Simpler to implement**: No API keys, no external AI service
- **Predictable**: Rules are explicit and debuggable
- **No cost**: No per-call charges for AI API
- **Fast**: No network round-trip to AI service
- **Privacy**: PR data stays in workflow, not sent to third party

## Heuristics Used

```
ReadyToMerge if:
  - Has approval AND no changes requested
  - OR: Small PR (< 10 files) AND has approval

NeedsReview if:
  - Has changes requested
  - OR: Large PR (> 20 files)
  - OR: No reviews yet AND older than 7 days

ConsiderClosing if:
  - Older than 90 days with no activity
  - OR: Has "stale" label
  - OR: Author deleted/inactive
```

## Consequences

### Positive
- Works immediately without setup
- No secrets management
- Deterministic, easy to test

### Negative
- Less nuanced than AI analysis
- Won't catch semantic issues (bad idea, duplicate, etc.)
- Needs manual tuning of thresholds

## Future: AI Enhancement (Phase 2)

When/if we add AI:
1. Use GitHub Copilot API or Azure OpenAI
2. Send PR title + description + file list
3. Get nuanced classification with reasoning
4. Store reasoning in JSON for display

## Related
- Collector: `collectors/collect-pr-triage.ps1`
- Page: `src/Dashboard/Pages/PrTriage.razor`
