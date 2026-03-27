# Schema Cheatsheet — Skia Update Review

Quick reference for `skia-review-schema.json` v1.0.

## Top-Level Structure

```json
{
  "meta": { ... },
  "summary": "Executive summary of the PR review...",
  "recommendations": ["Action item 1", "Action item 2"],
  "generatedFiles": { "status": "...", "summary": "...", "recommendations": [...], ... },
  "upstreamIntegrity": { "status": "...", "summary": "...", "recommendations": [...], ... },
  "interopIntegrity": { "status": "...", "summary": "...", "recommendations": [...], ... },
  "depsAudit": { "status": "...", "summary": "...", "recommendations": [...], ... },
  "riskAssessment": "HIGH",
  "companionPr": { "prNumber": N, "summary": "...", "recommendations": [...], "findings": [...] }
}
```

Every section has `summary` (brief overview) and `recommendations` (actionable items for reviewer).

**`companionPr` is required** — a Skia update always has a companion SkiaSharp PR.

## Scripts

| Script | What it checks | Output key |
|--------|---------------|------------|
| `check_generated_files.py` | Regenerate P/Invokes independently | `generatedFiles` |
| `check_source.py` | Diff-of-diffs for upstream + interop | `upstreamIntegrity` + `interopIntegrity` |
| `check_deps.py` | DEPS dependency changes | `depsAudit` |
| `run_review.py` | Orchestrator — runs all three above | `raw-results.json` |

## Status Values

All sections use `PASS` or `REVIEW_REQUIRED`. There is no WARN or FAIL for source/deps — all changes need human review.

| Field | Values |
|-------|--------|
| `generatedFiles.status` | `PASS`, `FAIL` |
| `upstreamIntegrity.status` | `PASS`, `REVIEW_REQUIRED` |
| `interopIntegrity.status` | `PASS`, `REVIEW_REQUIRED` |
| `depsAudit.status` | `PASS`, `REVIEW_REQUIRED` |
| `riskAssessment` | `LOW`, `MEDIUM`, `HIGH` |

## Risk Assessment

| Condition | Risk |
|-----------|------|
| `generatedFiles.status == FAIL` | HIGH |
| `upstreamIntegrity.status == REVIEW_REQUIRED` | HIGH |
| `interopIntegrity.status == REVIEW_REQUIRED` | MEDIUM |
| `depsAudit.status == REVIEW_REQUIRED` | MEDIUM |
| All PASS | LOW |

## Source Integrity (upstream + interop)

Both use the same structure (diff-of-diffs):

| Category | Meaning |
|----------|---------|
| `added` | Patched in new fork but NOT in old — new modifications |
| `removed` | Patched in old fork but NOT in new — patches dropped |
| `changed` | Patched in both but patch content differs |
| `unchanged` | Count of identical patches carried forward |

Each item in added/removed:
```json
{ "path": "include/core/SkBitmap.h", "summary": "...", "diff": "...", "relatedFiles": [...] }
```

Each item in changed:
```json
{ "path": "src/core/SkFont.cpp", "summary": "...", "diff": "...", "oldDiff": "...", "newDiff": "...", "patchDiff": "...", "relatedFiles": [...] }
```

- `diff` — Direct branch-to-branch diff (`base→PR head`). The simplest view — what actually changed in the fork. **Default view in the HTML viewer.**
- `patchDiff` — Diff-of-diffs (`old patch` vs `new patch`). Shows how the fork's *modifications* changed. Useful for understanding *why* something changed relative to upstream.
- `oldDiff` / `newDiff` — The full fork-vs-upstream patches for old and new branches respectively.

**No per-file status.** AI provides factual summaries only. ALL items need human review.

## Related Files (Cross-Links)

Each source file item can include an optional `relatedFiles` array to link to other files in the report:

```json
{
  "path": "include/core/SkFontMgr.h",
  "summary": "Fork patch added MakeDefault()...",
  "relatedFiles": [
    { "path": "src/c/sk_default_fontmgr.cpp", "relationship": "replaced by" },
    { "path": "src/c/sk_typeface.cpp", "relationship": "consumer" }
  ]
}
```

| Field | Required | Description |
|-------|----------|-------------|
| `path` | Yes | File path (relative to skia or SkiaSharp repo root) |
| `relationship` | No | Short label: `replaced by`, `moved to`, `moved from`, `depends on`, `consumer`, `header`, `implementation`, `C# wrapper` |

The HTML viewer renders these as clickable pills. Clicking navigates to the matching file in the report (scrolling + highlighting), or opens on GitHub if not found in the report.

**When to add `relatedFiles`:**
- A removed patch has a replacement in a different section (e.g., upstream removed → interop added)
- A header and implementation file are both changed
- An interop file has a corresponding C# wrapper in the companion PR

## DEPS Audit

Same added/removed/changed pattern:

Each added/removed item:
```json
{ "name": "icu4x", "summary": "...", "url": "...", "revision": "..." }
```

Each changed item:
```json
{ "name": "libpng", "summary": "...", "oldUrl": "...", "oldRevision": "...", "newUrl": "...", "newRevision": "..." }
```

## Interop Directories

Files in these dirs go to `interopIntegrity`; everything else to `upstreamIntegrity`:
- `include/c/`
- `include/xamarin/`
- `src/c/`
- `src/xamarin/`

## Validation Rules

- `REVIEW_REQUIRED` sections must have at least one added/removed/changed item
- Each source file item must have `path` + `summary`
- Each dep item must have `name` + `summary`
- `meta.skiasharpPrNumber` is required (integer, not null)
- `meta.shas.upstream` is the NEW upstream SHA (the milestone being merged in)
- `companionPr` must have `prNumber`, `summary`, `recommendations`
- Each companion PR finding must have `file`, `type`, `summary`
- Finding `type` must be one of: `new_api`, `changed_api`, `disposal_pattern`, `abi_concern`, `missing_test`, `platform_specific`, `other`
- `generatedFiles` can include `generatorError` (string) when the generator itself crashes
- No absolute paths (redact `/Users/...` → relative)
- Schema version must be `"1.0"`
