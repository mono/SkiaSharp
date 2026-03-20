# Fix Schema Cheatsheet

Quick reference for `ai-fix/{number}.json` fields.

## Required Fields (top-level)
- meta, status, summary, rootCause, changes, tests, verification

## meta
- schemaVersion: "1.0" (const)
- number: issue number (int)
- repo: "mono/SkiaSharp" (pattern)
- analyzedAt: ISO 8601 UTC
- inputs: consumed triage/repro files (optional)

## status
- value: "fixed" | "in-progress" | "cannot-fix" | "needs-info" | "duplicate"
- When to use each:
  - fixed: PR created, tests pass, root cause addressed
  - in-progress: investigation ongoing (checkpoint)
  - cannot-fix: upstream Skia issue, platform limitation
  - needs-info: couldn't reproduce, need reporter detail
  - duplicate: same as existing fix

## rootCause
- category: "logic-error" | "memory-safety" | "threading" | "api-misuse" | "dependency" | "upstream-skia" | "missing-feature" | "other"
- area: "managed" | "binding" | "native" | "build" | "packaging" | "tests" | "docs"
- description: ≥20 chars
- confidence: 0.0-1.0
- affectedFiles: array of relative paths

## changes
- files: array of { path, changeType: "added"|"modified"|"removed", summary }
- breakingChange: boolean
- risk: "low" | "medium" | "high"
  - low: isolated fix, no API change
  - medium: touches public API surface, multiple files
  - high: ABI impact, cross-platform, native code

## tests
- regressionTestAdded: boolean (SHOULD be true for status=fixed)
- testsAdded: array of { file, name, description? }
- command: exact test command
- result: "passed" | "failed" | "not-run"

## verification
- reproScenario: "passed" | "failed" | "not-run" | "not-applicable"
- method: "automated-test" | "manual-repro" | "visual-inspection" | "code-review"
- notes: string

## Conditional Required
- pr: REQUIRED if status=fixed (number?, url, status: "draft"|"open"|"merged"|"closed")
- blockers: REQUIRED if status=cannot-fix or needs-info (minItems: 1)
- relatedIssues: REQUIRED if status=duplicate (minItems: 1)

## feedback (optional)
- corrections: array of { source: "triage"|"repro", topic, upstream, corrected }

## Common Mistakes
- ❌ Setting optional fields to null (OMIT them instead)
- ❌ Using absolute paths in affectedFiles (use repo-relative paths)
- ❌ Forgetting pr field when status=fixed
- ❌ Not running validate-fix.ps1 before persisting
- ❌ Using "memory-management" instead of "memory-safety" for rootCause.category
- ❌ Using "c-sharp"/"c-api" instead of "managed"/"binding"/"native" for rootCause.area
