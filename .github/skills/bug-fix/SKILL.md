---
name: bug-fix
description: >
  Fix bugs in SkiaSharp C# bindings. Structured workflow for investigating, fixing,
  and testing bug reports.
  
  Triggers: Crash, exception, AccessViolationException, incorrect output, wrong behavior,
  memory leak, disposal issues, "fails", "broken", "doesn't work", "investigate issue",
  "fix issue", "look at #NNNN", any GitHub issue number referencing a bug.
  
  For adding new APIs, use `add-api` skill instead.
---

# Bug Fix Skill

Fix bugs in SkiaSharp with minimal, surgical changes.

> ⚠️ **MANDATORY: Follow phases in order. Do NOT skip phases.**
> 
> Each phase has a **GATE** — you cannot proceed until the gate criteria are met.
> The workflow exists to prevent wasted investigation. Trust the process.

---

## Quick Diagnosis Table (Check FIRST)

Before deep investigation, check if the symptom matches a known pattern:

| Symptom | Likely Cause | Fix Location |
|---------|--------------|--------------|
| `undefined symbol: X` | Missing library link | `externals/skia/third_party/BUILD.gn` or `native/linux/build.cake` |
| `EntryPointNotFoundException` | C API not rebuilt | Rebuild natives after C API changes |
| `AccessViolationException` | Memory/disposal bug | C# validation or native code |
| `ArgumentNullException` | Missing null check | C# wrapper before P/Invoke |
| Platform-specific crash | Build config | `native/{platform}/build.cake` |
| GLIBC version error | Build environment | Docker base image version |

**If the symptom matches, the fix location is usually obvious. Research phase will confirm.**

---

## Prerequisites

- GitHub API access (fetch issues, search issues, read comments)
- Git with push access
- Docker for cross-platform testing (check: `docker --version`)

---

## Workflow Overview

```
1. Understand   → Fetch issue, extract key details
2. Create PR    → Start tracking immediately (living document)  
3. Research     → Search related issues, READ ALL COMMENTS, find existing diagnosis
4. Reproduce    → Test on target platform (skip if build config issue)
5. Investigate  → Find root cause (often already found in Phase 3!)
6. Fix          → Minimal change
7. Test         → Regression test + existing tests
8. Finalize     → Rewrite PR description, link all fixed issues
```

---

## Phase 1: Understand the Issue

Fetch the issue from GitHub. Extract:
- Symptoms, error messages, stack traces
- Platform (OS, arch, .NET version, SkiaSharp version)
- Last working version (if mentioned)
- Reproduction steps or code snippets

This phase is quick — get enough context to create the PR.

### ✅ GATE: Do not proceed until you have:
- [ ] Issue title, symptoms, and error message (if any)
- [ ] Target platform identified

---

## Phase 2: Create Draft PR

> 🛑 **MANDATORY: Create the PR NOW, before any investigation.**
> 
> The PR is your living document. Skip this and you lose track of your work.

```bash
git checkout -b copilot/issue-NNNN-short-description
git commit --allow-empty -m "Investigating #NNNN: [description]"
git push -u origin copilot/issue-NNNN-short-description
```

Create PR using investigation template from [references/pr-templates.md](references/pr-templates.md).

**The PR description is your living document:**
- All collected info, links, and related issues (added as you find them)
- WHY each related issue is similar (same platform? same error? same root cause?)
- Your investigation plan with checkboxes
- Progress log (add rows as you work)
- Alternatives tried (add when something doesn't work)

**Update the PR description OFTEN** — after every significant step.

### ✅ GATE: Do not proceed until you have:
- [ ] Feature branch created and pushed
- [ ] Draft PR opened with investigation template

---

## Phase 3: Research Related Issues

> 🛑 **CRITICAL: This phase often SOLVES the bug.**
> 
> The community may have already diagnosed the root cause in issue comments.
> READ ALL COMMENTS on related issues before investigating yourself.

Search GitHub issues for:
- Same error message (e.g., `undefined symbol: uuid_generate_random`)
- Same platform (e.g., `Linux ARM64`)
- Same SkiaSharp version
- Keywords from title

**For EACH related issue found:**
1. **Read ALL comments** (not just the issue body) — diagnosis is often in comments!
2. Note: issue number, title, WHY it's related
3. Extract: workarounds mentioned, root cause analysis, resolution if closed
4. Check for links to external issues (other projects that use SkiaSharp)

**Update PR** with all related issues and extracted information.

### ✅ GATE: Do not proceed until you have:
- [ ] Searched for related issues (at least 2-3 search queries)
- [ ] Read ALL comments on the most relevant related issues
- [ ] Updated PR with related issues and any diagnosis found
- [ ] Checked the Quick Diagnosis Table above

**If a related issue already contains the root cause diagnosis, skip to Phase 5 or 6.**
When skipping, still document the known root cause in the PR before proceeding.

---

## Phase 4: Reproduce

> 💡 **When to skip reproduction:**
> - Build configuration bugs (undefined symbol, linker errors) — verify via code inspection
> - Root cause already confirmed in related issue comments
> - Fix is obviously correct from code review
>
> If skipping, document WHY in PR and proceed to Phase 5 or 6.

### 4.1 Target Platform Requirements

| Attribute | Must Match |
|-----------|------------|
| OS (macOS/Windows/Linux) | ✅ |
| Architecture (x64/ARM64) | ✅ |
| .NET version | ✅ |
| SkiaSharp version | ✅ |

### 4.2 Docker Testing

For cross-platform testing, see [references/docker-testing.md](references/docker-testing.md).

Example (adapt platform to match the issue):
```bash
# Replace with the platform from the issue
docker run --platform linux/arm64 -it <dotnet-sdk-image> bash
```

### 4.3 Document Results in PR

Add to PR description:

| Environment | Version | Result |
|-------------|---------|--------|
| [Platform from issue] | [version] | ❌ Crashes |
| [Different platform] | [version] | ✅ Works |

### 4.4 If Reproduction Fails

**Try hard and exhaust all options** before giving up:
1. Try different Docker base images (different Linux distros)
2. Try older/newer .NET versions
3. Try the exact SkiaSharp version AND the last known working version
4. Check if issue mentions specific hardware or configurations

Document each attempt in PR. After exhausting options: ask user for details, proceed with code review.

### ✅ GATE: Do not proceed until you have:
- [ ] Reproduced the issue OR documented why reproduction is unnecessary
- [ ] Updated PR with reproduction results

---

## Phase 5: Investigate Root Cause

> 💡 **Often already done!** If Phase 3 found a diagnosis in related issue comments,
> this phase is just confirmation. Don't re-investigate what's already known.

### 5.1 Check Quick Diagnosis Table First

Refer back to the Quick Diagnosis Table at the top of this document. 
If symptom matches, go directly to the fix location.

### 5.2 Locate the Problem (if needed)

```bash
grep -rn "MethodName" binding/SkiaSharp/
grep -r "sk_.*methodname" binding/SkiaSharp/
readelf -d output/native/linux/arm64/libSkiaSharp.so | grep NEEDED
```

### 5.3 Exit Criteria

Stop investigating when you can answer:
- [ ] What exact code causes the bug?
- [ ] Why does it fail?
- [ ] Why doesn't it fail elsewhere?
- [ ] What single change fixes it?

### ✅ GATE: Do not proceed until you have:
- [ ] Identified root cause
- [ ] Updated PR with root cause analysis

---

## Phase 6: Fix

For common fix patterns, see [references/fix-patterns.md](references/fix-patterns.md).

**Principle: Minimal change.** Fix only what's broken.

### ✅ GATE: Do not proceed until you have:
- [ ] Made the fix
- [ ] Committed with descriptive message referencing issue number

---

## Phase 7: Build & Test

### If Modified Native Code

```bash
dotnet cake --target=externals-macos --arch=arm64   # macOS ARM64
dotnet cake --target=externals-linux --arch=arm64   # Linux ARM64 (in Docker)
```

### If Modified C# Only

```bash
dotnet cake --target=externals-download  # If output/native/ empty
```

### Write Regression Test

| Affected Class | Test File |
|----------------|-----------|
| SKCanvas | `tests/Tests/SKCanvasTest.cs` |
| SKBitmap | `tests/Tests/SKBitmapTest.cs` |
| SKImage | `tests/Tests/SKImageTest.cs` |
| Other | Find matching `*Test.cs` |

Name: `Issue_NNNN_BriefDescription()`

### Run Tests

```bash
dotnet test tests/SkiaSharp.Tests.Console/SkiaSharp.Tests.Console.csproj
```

Tests MUST pass. Verify fix on original platform.

### ✅ GATE: Do not proceed until you have:
- [ ] Built successfully
- [ ] All tests pass
- [ ] Verified fix resolves the original issue (if possible)

---

## Phase 8: Finalize

Rewrite PR description using final template from [references/pr-templates.md](references/pr-templates.md).

Link ALL fixed issues (including related issues that have the same root cause):
```markdown
Fixes #3369
Fixes #3272
```

Mark PR as ready for review (remove draft status).

### ✅ GATE: Complete when:
- [ ] PR description rewritten with final template
- [ ] All related issues linked with "Fixes #NNNN"
- [ ] PR marked ready for review

---

## Error Recovery

| Issue | Recovery |
|-------|----------|
| Can't reproduce | Ask user for exact environment details |
| Fix causes test failures | Revert, re-analyze root cause |
| `EntryPointNotFoundException` | Rebuild natives after C API changes |
| Can't test on required platform | Use Docker or ask user to verify |

---

## Final Checklist

Before marking complete, verify ALL gates were passed:

- [ ] **Phase 1**: Issue understood, key details extracted
- [ ] **Phase 2**: Draft PR created and used as living document
- [ ] **Phase 3**: Related issues searched, ALL comments read, diagnosis collected
- [ ] **Phase 4**: Reproduced OR documented why skipped
- [ ] **Phase 5**: Root cause identified and documented
- [ ] **Phase 6**: Minimal fix implemented
- [ ] **Phase 7**: Build passes, tests pass
- [ ] **Phase 8**: PR finalized with "Fixes #NNNN"
