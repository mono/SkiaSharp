---
name: api-add-review
description: >
  Add new C# APIs to SkiaSharp by wrapping Skia C++ functionality, or review
  existing API PRs for correctness and style. Two workflows:
  (1) Add: C++ analysis → C API → bindings → C# wrapper → tests → review
  (2) Review: check naming, Span overloads, properties, tests, interop safety.

  Triggers:
  - "add DrawFoo method", "expose SkSurface::draw", "wrap sk_foo_bar"
  - "add API", "expose function", "wrap method", "create binding for"
  - "review this API", "check the API surface", "review PR #NNN for API design"
  - Issue classified as "New API" (after fetching and classification)
---

# Add / Review API Skill

This skill has two modes:

1. **Add mode** — implement a new API from scratch, then review it
2. **Review mode** — review an existing PR/diff for API design correctness

Both modes share the same design rules and quality bar.

## Detecting Mode

| User says | Mode |
|-----------|------|
| "add", "expose", "wrap", "create binding" | Add |
| "review", "check API", "look at PR" | Review |
| Issue classified as "New API" | Add |
| Self-review after adding | Review (automatic) |

## ⚠️ Branch Protection (COMPLIANCE REQUIRED)

> **🛑 NEVER commit directly to protected branches. This is a policy violation.**

| Repository | Protected Branches | Required Action |
|------------|-------------------|-----------------|
| SkiaSharp (parent) | `main` | Create feature branch first |
| externals/skia (submodule) | `main`, `skiasharp` | Create feature branch first |

## ❌ NEVER Do These

| Shortcut | Consequence |
|----------|-------------|
| Commit directly to `main` or `skiasharp` | Policy violation |
| Edit `*.generated.cs` manually | Overwritten on regenerate |
| Skip native build after C API change | `EntryPointNotFoundException` |
| Skip tests | Unacceptable |
| Skip tests because they fail | Unacceptable — fix the issue |
| Use default parameters in public APIs | ABI breaking change |
| Invent type names not in upstream Skia | Confusing, wrong naming |
| Add XML doc comments | Inserted by separate process |
| Fabricate test fonts | Use real fonts from known sources |

## References

All three references work together:

| File | Purpose | When to read |
|------|---------|-------------|
| [references/api-design-rules.md](references/api-design-rules.md) | Naming, properties vs methods, Span patterns, type wrapping, test and sample requirements | Always — before writing or reviewing any API |
| [references/add-workflow.md](references/add-workflow.md) | Step-by-step add workflow with C API patterns, struct conversion, JSON config, gallery samples | Add mode |
| [references/review-workflow.md](references/review-workflow.md) | Structured review checklist, test coverage analysis, sample review, auto-fix mode | Review mode, and as final phase of add mode |
| [references/troubleshooting.md](references/troubleshooting.md) | Common errors and fixes | When something goes wrong |

## Add Mode

1. Read [api-design-rules.md](references/api-design-rules.md)
2. Follow [add-workflow.md](references/add-workflow.md) phases 1-10
3. Run [review-workflow.md](references/review-workflow.md) on your own changes
4. Fix any issues identified by the review
5. Re-run tests to confirm

## Review Mode

1. Read [api-design-rules.md](references/api-design-rules.md)
2. Follow [review-workflow.md](references/review-workflow.md)
3. In fix-first mode: auto-fix high-confidence issues, re-run tests
4. In review-only mode: produce structured feedback report
