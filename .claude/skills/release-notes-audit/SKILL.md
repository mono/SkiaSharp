---
name: release-notes-audit
description: >
  Compare Skia upstream release notes against SkiaSharp C# bindings to produce a
  structured report (JSON + HTML) of new, missing, deprecated, and removed APIs
  across milestone ranges. Use this skill whenever the user mentions Skia release
  notes, milestone changes, API gaps, binding coverage, or wants a report of what
  changed between Skia milestones. Trigger on: "release notes", "what changed
  between milestones", "what's new in skia", "generate a report", "missing APIs",
  "deprecated APIs", "milestone bump", "binding audit", "API coverage", "what
  should we add", "what should we deprecate". DO NOT use for: fixing bugs, adding
  a single specific API binding, updating the skia submodule, security audits, or
  writing documentation.
---

# Release Notes Audit Skill

Compare Skia upstream release notes against SkiaSharp's C API shim and C# bindings to identify new features to add, deprecated APIs to mark, and missing bindings. Produces a structured JSON report and standalone HTML dashboard.

## Key References

- **[references/report-schema.md](references/report-schema.md)** — JSON schema for structured output
- **[scripts/render-release-notes-audit.py](scripts/render-release-notes-audit.py)** — Renders JSON → standalone HTML
- **[scripts/viewer.html](scripts/viewer.html)** — HTML template (Bootstrap 5)

## Workflow

```
1. Determine milestone range (current vs previous)
2. Fetch Skia release notes from upstream
3. Classify each change (new feature, deprecation, removal, behavior change, internal)
4. Check C API (mono/skia include/c/) for each relevant item
5. Check C# bindings (binding/SkiaSharp/) for each relevant item
6. Cross-validate findings with a second model
7. Assemble structured JSON report (per report-schema.md)
8. Render HTML from JSON (render-release-notes-audit.py)
9. Present markdown summary to user
```

### Step 1: Determine Milestone Range

Find the current and previous SkiaSharp milestones. The user may specify these or you can detect them:

```bash
# Check current milestone from submodule (if checked out)
cat externals/skia/include/core/SkMilestone.h 2>/dev/null
# Or check git log for milestone references
git --no-pager log --oneline -20 | grep -i milestone
```

If the submodule isn't checked out, ask the user or check recent PRs/issues for milestone references.

### Step 2: Fetch Skia Release Notes

Get the release notes from upstream Google Skia:

```
https://raw.githubusercontent.com/google/skia/main/RELEASE_NOTES.md
```

This file is large — paginate with start_index if needed (content may exceed 20K chars). Parse each milestone section between `Milestone NNN` headers.

### Step 3: Classify Each Change

For each entry in the release notes within the milestone range, classify it:

| Category | Description | Example |
|----------|------------|---------|
| `new_feature` | New public API added | `SkColorSpace::MakeCICP` |
| `deprecation` | API marked deprecated | `SkTypeface::MakeDefault()` |
| `removal` | API removed from public surface | `SkDrawLooper` removed |
| `behavior_change` | Existing API changed behavior | `kRec709` transfer fn changed |
| `header_reorg` | Headers moved/renamed (usually N/A for SkiaSharp) | `include/gpu/` → `include/gpu/ganesh/` |
| `internal` | Build system, Graphite-only, Dawn-only | Precompile context threading |

**Filter out items irrelevant to SkiaSharp's C API surface:**
- Graphite-only APIs (SkiaSharp uses Ganesh for GPU)
- Dawn-only changes (SkiaSharp uses GL/Vulkan/Metal)
- Build system / GN flag changes
- SkSL language specification changes (unless they affect runtime effects)

### Step 4: Check C API

For each relevant item, check if the C API shim in mono/skia exposes it.

If the submodule is checked out:
```bash
grep -rn "function_name" externals/skia/include/c/
grep -rn "function_name" externals/skia/src/c/
```

If NOT checked out, use the GitHub API:
```
github-mcp-server-get_file_contents owner=mono repo=skia path=include/c/sk_<relevant>.h
```

Key C API header files to check:
- `sk_types.h` — enums, structs
- `sk_image.h`, `sk_canvas.h`, `sk_path.h`, `sk_codec.h` — core APIs
- `sk_colorspace.h`, `sk_font.h`, `sk_typeface.h` — specialized APIs
- `sk_imagefilter.h`, `sk_shader.h`, `sk_paint.h` — effects

### Step 5: Check C# Bindings

For each relevant item, check the C# wrappers:

```bash
grep -rn "MethodName\|method_name" binding/SkiaSharp/
```

Key C# files to check:
- `Definitions.cs` — enums (SKColorType, SKAlphaType, etc.)
- `SkiaApi.generated.cs` — P/Invoke declarations (DO NOT EDIT but check for presence)
- `SK*.cs` files — wrapper classes

**Binding status classification:**

| Status | Meaning |
|--------|---------|
| `full` | C API + C# wrapper both exist and cover the feature |
| `partial` | C API exists but C# wrapper is incomplete, or vice versa |
| `missing` | Neither C API nor C# wrapper exists |
| `not_applicable` | Skia change doesn't require a binding (header reorg, internal, etc.) |
| `correctly_absent` | Skia removed this and SkiaSharp never wrapped it |
| `action_needed` | SkiaSharp wraps something Skia deprecated/removed without marking `[Obsolete]` |

### Step 6: Cross-Validate

For accuracy, use a second model (via `task` tool with `model` parameter) to independently verify the key findings. At minimum, validate:
- All items marked `full` (confirm they actually exist)
- All items marked `action_needed` (confirm the deprecation status)
- All items marked `missing` that are high priority

### Step 7: Assemble Structured JSON Report

> 🛑 **MANDATORY:** The audit MUST produce a JSON file conforming to [references/report-schema.md](references/report-schema.md).

Save as `output/ai/release-notes-audit-{date}.json` in the repo.

### Step 8: Render HTML Report

> 🛑 **MANDATORY:** Always generate the HTML report.

```bash
python3 .claude/skills/release-notes-audit/scripts/render-release-notes-audit.py \
  output/ai/release-notes-audit-{date}.json
```

This produces a self-contained HTML file alongside the JSON. Present the output path:
```
✅ release-notes-audit-2026-04-15.html (52 KB)
   m119 → m133 • 52 items • 6 missing • 5 action needed
```

### Step 9: Present Markdown Summary

After generating JSON and HTML, present a concise markdown summary in the conversation. Group items by urgency:

1. 🔴 **Critical** — APIs that will break on next Skia bump
2. ⚠️ **Action Needed** — Deprecated APIs missing `[Obsolete]` markers
3. ❌ **Missing (High)** — New features in current milestone window with no binding
4. 🔶 **Missing (Medium)** — Useful features to plan for
5. 🟢 **Missing (Low)** — Nice-to-have features
6. ✅ **Full** — Features already bound
7. 🚫 **N/A** — Items correctly absent

## Priority Classification

When classifying priority, use these guidelines:

| Priority | Criteria |
|----------|----------|
| 🔴 Critical | Will cause compile/link/runtime failures on next Skia bump. E.g., removed headers, immutable SkPath. |
| ⚠️ Action Needed | SkiaSharp wraps something Skia deprecated. Should add `[Obsolete]`. |
| 🔶 High | New API in current milestone window (between previous and current). Commonly requested. |
| 🟡 Medium | New API in future milestones. Useful but not urgent. |
| 🟢 Low | Niche feature, or C# already has equivalent functionality. |

## Handoff

After the audit, the user may want to:
- **Add new APIs** → Use the `add-api` skill for each missing binding
- **Mark deprecations** → Edit `binding/SkiaSharp/SK*.cs` to add `[Obsolete]` attributes
- **Plan a Skia bump** → Use the `update-skia` skill
- **Check security** → Use the `security-audit` skill

## Tips for Accuracy

1. **Don't trust file absence as evidence of API absence.** A grep returning no results might mean the submodule isn't checked out. Always verify via GitHub API fallback.

2. **Check both C API AND C# bindings.** Sometimes the C API has a function but the C# wrapper wasn't created. This is `partial` status.

3. **Enum ordering matters.** The C API `sk_types.h` may reorder enum values vs upstream Skia. Compare by name, not by ordinal.

4. **Generated files reflect C API.** `SkiaApi.generated.cs` is auto-generated from C API headers. If a function is in the C API, it will be in the generated file.

5. **The mono/skia fork may retain deprecated APIs** that upstream removed. This isn't a bug — it's intentional for backward compatibility. Flag it but don't classify as broken.
