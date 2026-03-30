# Writing Summaries & Building the Report

After `run_review.py` completes, you have `raw-results.json` with all mechanical check outputs.
Your job is to add human-readable summaries and assemble the final report.

## Step 1 — Write Summaries

For **every** item in `added`, `removed`, and `changed` across all four sections
(`upstreamIntegrity`, `interopIntegrity`, `depsAudit`, `companionPr`), add a `summary` field.

### Rules

- **Factual only** — describe what the change does, not whether it's good or bad
- **No per-item PASS/FAIL** — all items need human review
- **Read the actual diff** — summaries based on file names alone are useless

### Upstream Integrity Summaries

Note WHY this file was modified. Common reasons:
- API change (function signature, return type, namespace move)
- Build fix (GN config, includes, platform guards)
- Security patch (overflow fix, bounds check)
- Merge artifact (whitespace, comment, no functional change)

Example: `"SkBitmap::getSubset() return type changed from SkIRect to SkRect — C API callers may need updating"`

### Interop Integrity Summaries

Describe what C API function/type was changed:
- New function added (`sk_surface_draw_with_sampling`)
- Function removed (typeface factory methods)
- Signature changed (parameter added/removed/retyped)
- Type/enum changed (new values, renamed members)

Example: `"Added sk_default_fontmgr.cpp/h — new platform singleton replacing removed SkFontMgr::RefDefault()"`

### DEPS Audit Summaries

Note what the dependency is and what changed:
- Version bump (security fix? feature update?)
- New dependency (why was it added?)
- URL change (mirror move? fork switch?)

Example: `"libpng bumped from 1.6.40 to 1.6.43 — includes fixes for CVE-2024-31497"`

### Verifying Removed Patches

For every **removed** item in `upstreamIntegrity`, you MUST verify WHY the patch was
dropped. The orchestrator leaves the working tree checked out with upstream refs fetched,
so you can inspect upstream directly.

**For each removed patch:**

1. Read the old fork diff (in raw-results.json) to understand what the patch did
2. Identify the key change (function added, constant changed, ifdef added, etc.)
3. Check the new upstream version of the file:
   ```bash
   git show upstream/{new_upstream_branch}:{file_path}   # in externals/skia
   ```
   Pipe through `grep` or `head` for large files — search for the specific
   function, constant, or pattern from the old patch.
4. State what you found as evidence in the summary

**Resolution categories:**

| Resolution | Evidence needed |
|-----------|----------------|
| Upstream resolved | Show the specific upstream line/value that addresses the fork's concern |
| Replaced by interop | Point to the new interop file that handles this |
| API removed upstream | Show that the patched API no longer exists in upstream |
| Unverified | Could not determine — flag for human reviewer |

**Examples:**

- ✅ `"Fork patch increased buffer limit from 16 to 128. Patch correctly dropped — new upstream sets limit to 65536 (verified at line N), which exceeds the fork's requirement."`
- ✅ `"Fork patch added FooClass::BarMethod(). Dropped — upstream removed FooClass entirely; functionality moved to C API shim layer (see src/c/sk_foo.cpp)."`
- ❌ `"This is handled differently in the new upstream or was deemed unnecessary."` — NEVER write this. Verify or say "unverified".

**The same principle applies to changed patches:** if a patch changed, check upstream to
understand whether the change was forced by an upstream API change vs. a deliberate fork modification.

### Companion PR Summaries

For each file in the companion PR's `added` and `changed` arrays, describe what the C# code does:
- New wrapper (what API it exposes, what Skia function it wraps)
- Changed wrapper (what was modified and why — e.g., new overload, updated disposal pattern)
- Test file (what API/behavior is being tested)
- Build/config change (what target/platform is affected)

Use `relatedFiles` to cross-link to the corresponding interop files. This enables
one-click navigation between a C# wrapper and its C API implementation.

Example: `"New SKFoo wrapper — factory method FromData() wrapping sk_foo_new_from_data(). Validates data parameter, returns null on native failure."`

Include diff content for all companion PR files — the viewer renders diffs the same
way as upstream/interop sections (Direct/Patch/Old/New for changed files).

## Step 2 — Build the JSON Report

Build JSON conforming to `skia-review-schema.json` v1.0. Use `schema-cheatsheet.md` for quick reference.

### Required fields

Every section needs:
- `status` — from the raw results (don't change it)
- `summary` — brief overview of the section (50+ chars)
- `recommendations` — actionable items specific to that section (array, at least 1)

Top level needs:
- `summary` — executive summary for 30-second comprehension
- `recommendations` — top-level action items
- `riskAssessment` — see risk rules below

### Risk Assessment

| Condition | Risk |
|-----------|------|
| `generatedFiles.status == FAIL` | HIGH |
| `upstreamIntegrity.status == REVIEW_REQUIRED` | HIGH |
| `interopIntegrity.status == REVIEW_REQUIRED` | MEDIUM |
| `depsAudit.status == REVIEW_REQUIRED` | MEDIUM |
| All PASS | LOW |

Highest wins (e.g., if upstream is REVIEW_REQUIRED and deps is REVIEW_REQUIRED → HIGH).

### Cross-link related files

For each source file item, add `relatedFiles` when the change has connections to other files:

```json
{
  "path": "include/core/SkFontMgr.h",
  "summary": "Fork patch added MakeDefault(). Patch dropped — C API now uses sk_create_default_fontmgr().",
  "relatedFiles": [
    { "path": "src/c/sk_default_fontmgr.cpp", "relationship": "replaced by" },
    { "path": "src/c/sk_default_fontmgr.h", "relationship": "replaced by" }
  ]
}
```

Common patterns:
- **Removed upstream patch → new interop file**: Use `"replaced by"` on the removed item, `"replaces"` on the added item
- **Header + implementation pair**: Use `"header"` / `"implementation"`
- **Interop file → companion PR C# wrapper**: Link to the binding file path

This enables one-click navigation in the HTML viewer between related changes.

### Critical: Include actual diffs

The `diff`, `oldDiff`, `newDiff`, and `patchDiff` fields MUST contain actual diff text,
NOT file path references. The JSON must be self-contained — a dashboard renders diffs directly
from it. The raw results already contain the full diff strings; copy them as-is.

### Save location

Save the report to `{OutputDir}/{pr_number}.json` — the **same directory** where the orchestrator
wrote `raw-results.json`. The orchestrator prints this path at the end of its output.
Do NOT save to any other location (not `skia-review/`, not the repo root, not a custom path).
