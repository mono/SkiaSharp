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

## Prerequisites

- GitHub API access (fetch issues, search issues, read comments)
- Git with push access
- Docker for cross-platform testing (check: `docker --version`)

## Workflow

```
1. Understand   → Fetch issue, extract key details
2. Create PR    → Start tracking immediately (living document)
3. Research     → Search related issues, read ALL comments, collect info
4. Reproduce    → Test on target platform, document all attempts in PR
5. Investigate  → Find root cause (may loop back to 4)
6. Fix          → Minimal change
7. Test         → Regression test + existing tests
8. Finalize     → Rewrite PR description, link all fixed issues
```

Phases 4-5 often iterate together. Update PR throughout.

---

## Phase 1: Understand the Issue

Fetch the issue from GitHub. Extract:
- Symptoms, error messages, stack traces
- Platform (OS, arch, .NET version, SkiaSharp version)
- Last working version (if mentioned)
- Reproduction steps or code snippets

This phase is quick — get enough context to create the PR.

---

## Phase 2: Create Draft PR

Create PR **immediately** to track the entire journey.

```bash
git checkout -b copilot/issue-NNNN-short-description
git commit --allow-empty -m "Investigating #NNNN: [description]"
git push -u origin copilot/issue-NNNN-short-description
```

Use investigation template from [references/pr-templates.md](references/pr-templates.md).

**The PR description is your living document:**
- All collected info, links, and related issues (added as you find them)
- WHY each related issue is similar (same platform? same error? same root cause?)
- Your investigation plan with checkboxes
- Progress log (add rows as you work)
- Alternatives tried (add when something doesn't work)

**Update the PR description OFTEN** — after every significant step.

---

## Phase 3: Research Related Issues

Search GitHub issues for:
- Same error message (e.g., the exception type or error text)
- Same platform (e.g., the OS and architecture from the issue)
- Same SkiaSharp version
- Keywords from title

**For EACH related issue found:**
1. Read ALL comments on that issue (not just the issue body)
2. Note: issue number, title, WHY it's related (same symptom? platform? root cause?)
3. Extract: workarounds mentioned, root cause analysis, resolution if closed
4. Record links to external issues mentioned in comments

**Update PR** with all related issues and extracted information.

---

## Phase 4: Reproduce

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

---

## Phase 5: Investigate Root Cause

### 5.1 Locate the Problem

```bash
grep -rn "MethodName" binding/SkiaSharp/
grep -r "sk_.*methodname" binding/SkiaSharp/
readelf -d output/native/linux/arm64/libSkiaSharp.so | grep NEEDED
```

### 5.2 Symptom → Location

| Symptom | Fix Location |
|---------|--------------|
| ArgumentNullException | Add null check before P/Invoke |
| AccessViolationException | Validation, state, or native linking |
| undefined symbol | `native/linux/build.cake` linker flags |
| Platform-specific crash | `native/{platform}/build.cake` |

### 5.3 Exit Criteria

Stop investigating when you can answer:
- [ ] What exact code causes the bug?
- [ ] Why does it fail?
- [ ] Why doesn't it fail elsewhere?
- [ ] What single change fixes it?

**Update PR** with root cause analysis before proceeding to fix.

---

## Phase 6: Fix

For common fix patterns, see [references/fix-patterns.md](references/fix-patterns.md).

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

---

## Phase 8: Finalize

Rewrite PR description using final template from [references/pr-templates.md](references/pr-templates.md).

Link ALL fixed issues:
```markdown
Fixes #3369
Fixes #3272
```

---

## Error Recovery

| Issue | Recovery |
|-------|----------|
| Can't reproduce | Ask user for exact environment details |
| Fix causes test failures | Revert, re-analyze root cause |
| `EntryPointNotFoundException` | Rebuild natives after C API changes |
| Can't test on required platform | Use Docker or ask user to verify |

## Checklist

- [ ] Collected info from issue and related issues
- [ ] Reproduced on target platform
- [ ] Found and documented root cause
- [ ] Implemented minimal fix with regression test
- [ ] All tests pass
- [ ] PR description finalized with "Fixes #NNNN"
