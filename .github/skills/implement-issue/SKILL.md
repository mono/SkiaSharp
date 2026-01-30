---
name: implement-issue
description: >
  Implement SkiaSharp GitHub issues with proper context gathering and planning.
  Use when user provides a GitHub issue URL (github.com/mono/SkiaSharp/issues/NNNN),
  says "implement issue #NNNN", "work on #NNNN", "fix #NNNN", or asks to implement
  a feature/fix from a GitHub issue. Handles new APIs, bug fixes, and enhancements.
  For documentation-only issues, edit docs directly without this skill.
  For native Skia C++ issues, these require upstream changes to google/skia.
---

# Implement Issue

Gather context and create implementation plans for SkiaSharp GitHub issues.

## Workflow

```
1. FETCH      → Get issue from GitHub
2. CLASSIFY   → Determine type: New API | Bug Fix | Enhancement
3. GATHER     → Find affected code, similar patterns, tests
4. PLAN       → Create checklist, show patterns, note concerns
5. CONFIRM    → Review with user before implementing
```

## Step 1: Fetch Issue

Use GitHub MCP to get issue details:
- Title and description
- Labels
- Comments (often contain clarifications)
- Linked PRs or issues

## Step 2: Classify

| Type | Indicators |
|------|------------|
| **New API** | "add", "expose", "support", "convenience method" |
| **Bug Fix** | "crash", "exception", "incorrect", "fails", "broken" |
| **Enhancement** | "improve", "optimize", "performance", "better" |

Then determine scope:
- C# binding only? Or needs C API changes?
- Which class(es) affected?
- Test changes needed?

**Note:** Enhancements typically follow new-api (if adding features) or bug-fix (if improving existing behavior) workflows.

## Step 3: Gather Context

Load [references/context-checklist.md](references/context-checklist.md) for what to gather from the issue and codebase.

### Type-Specific Gathering

- **New API:** See [references/new-api.md](references/new-api.md)
- **Bug Fix:** See [references/bug-fix.md](references/bug-fix.md)

## Step 4: Create Plan

Present findings to user with:

1. **Understanding** — One paragraph summarizing the issue
2. **Classification** — Type, scope, ABI impact
3. **Code Found** — Relevant snippets from codebase
4. **Checklist** — Implementation steps
5. **Concerns** — Questions or risks identified

### Plan Output Example

```markdown
## Issue Analysis: #9999 - Add SKPaint.SetDashEffect convenience method

### Understanding
User requests a convenience method to set dash effects without manually 
creating SKPathEffect. Similar pattern exists for Shader property.

### Classification
- **Type:** New API
- **Scope:** C# binding only (C API exists)
- **ABI Impact:** ✅ Safe - adding new method only

### Relevant Code Found
**Similar pattern in SKPaint.cs line 245:**
public SKShader Shader {
    get => SKShader.GetObject(SkiaApi.sk_paint_get_shader(Handle));
    set => SkiaApi.sk_paint_set_shader(Handle, value?.Handle ?? IntPtr.Zero);
}

### Checklist
- [ ] Add SetDashEffect method to SKPaint.cs
- [ ] Validate intervals array
- [ ] Add tests in SKPaintTest.cs
- [ ] Build and verify

### Concerns
- Should previous PathEffect be disposed?
- Need overload without phase parameter?
```

**Note:** Same format applies to bug fixes - adjust Classification type accordingly.

## Step 5: Confirm

Before implementing, ask user to confirm the plan. Use ask_user tool:
- "Proceed with implementation?"
- "Need to adjust the approach?"
- "Need more information?"

## ABI Safety Check

**Critical for any public API change:**

| ✅ Safe | ❌ Never |
|---------|---------|
| Add new overloads | Modify existing signatures |
| Add new methods | Remove public APIs |
| Add new classes | Change return types |

If change affects public API, explicitly note ABI safety in the plan.

## References

- **New API workflow:** [references/new-api.md](references/new-api.md)
- **Bug fix workflow:** [references/bug-fix.md](references/bug-fix.md)
- **Context checklist:** [references/context-checklist.md](references/context-checklist.md)
- **Key docs:** `documentation/api-design.md`, `adding-apis.md`, `error-handling.md`
