---
name: bug-fix
description: >
  Fix bugs in SkiaSharp C# bindings. Structured workflow for investigating, fixing,
  and testing bug reports.
  
  Triggers: Crash, exception, AccessViolationException, incorrect output, wrong behavior,
  memory leak, disposal issues, "fails", "broken", "doesn't work".
  
  For adding new APIs, use `add-api` skill instead.
---

# Bug Fix Skill

Fix bugs in SkiaSharp with minimal, surgical changes.

## ⚠️ Branch Protection (COMPLIANCE REQUIRED)

> **🛑 NEVER commit directly to protected branches. This is a policy violation.**

| Repository | Protected Branches | Required Action |
|------------|-------------------|-----------------|
| SkiaSharp (parent) | `main` | Create feature branch first |
| externals/skia (submodule) | `main`, `skiasharp` | Create feature branch first |

**BEFORE making any changes**, create a feature branch:

```bash
# Create branch in SkiaSharp repo
git checkout -b dev/issue-NNNN-description

# If bug requires C API changes, also create branch in submodule:
cd externals/skia
git checkout -b dev/issue-NNNN-description
cd ../..
```

## Workflow

```
1. Understand   → Extract symptoms, reproduction steps, expected behavior
2. Locate       → Find affected code, trace to root cause
3. Fix          → Implement minimal fix
4. Test         → Write regression test, run existing tests
5. Verify       → Check for similar issues elsewhere
```

## Phase 1: Understand the Bug

Extract from issue:
- **Symptoms:** What goes wrong? (crash, incorrect output, exception)
- **Reproduction:** Steps to trigger
- **Expected:** What should happen
- **Environment:** Platform-specific? Version-specific?

## Phase 2: Locate the Problem

```bash
# Find the method mentioned in the issue
grep -rn "MethodName" binding/SkiaSharp/

# Check what validation exists (null checks, range checks, state checks)
# Find the native call
grep -r "sk_.*methodname" binding/SkiaSharp/
```

| Symptom | Likely Fix Location |
|---------|---------------------|
| ArgumentNullException | Add null check before P/Invoke |
| AccessViolationException | Missing validation, bad state |
| Incorrect output | Logic error in C# or native |
| Memory leak | Missing dispose, wrong ownership |

## Phase 3: Implement Fix

### Common Patterns

**Missing Null Check:**
```csharp
// Before (crashes)
public void DrawPath(SKPath path, SKPaint paint)
{
    SkiaApi.sk_canvas_draw_path(Handle, path.Handle, paint.Handle);
}

// After (validates)
public void DrawPath(SKPath path, SKPaint paint)
{
    if (path == null)
        throw new ArgumentNullException(nameof(path));
    if (paint == null)
        throw new ArgumentNullException(nameof(paint));
    SkiaApi.sk_canvas_draw_path(Handle, path.Handle, paint.Handle);
}
```

**Same-Instance Return Bug:**
```csharp
// WRONG - may dispose what we're returning
using var source = GetImage();
var result = source.Subset(bounds);
return result;

// CORRECT - check first
var source = GetImage();
var result = source.Subset(bounds);
if (result != source)
    source.Dispose();
return result;
```

Methods that may return same instance: `Subset()`, `ToRasterImage()`, `ToRasterImage(false)`

## Phase 4: Build & Test

### If You Modified C API (Native Code)

> **🛑 If the bug fix required changes to `externals/skia/` (C API), you MUST build the native library.**

```bash
# macOS (Apple Silicon)
dotnet cake --target=externals-macos --arch=arm64

# macOS (Intel)  
dotnet cake --target=externals-macos --arch=x64

# Windows (x64)
dotnet cake --target=externals-windows --arch=x64
```

> ⚠️ `EntryPointNotFoundException` means you forgot to build natives after C API changes.

### If You Only Modified C# Code

For C#-only fixes, pre-built natives work fine:

```bash
dotnet cake --target=externals-download  # Only if output/native/ is empty
```

### Write Regression Test

Write a test BEFORE claiming the fix is complete:

```csharp
[SkippableFact]
public void MethodDoesNotCrashWithEmptyInput()
{
    using var bitmap = new SKBitmap(100, 100);
    using var canvas = new SKCanvas(bitmap);
    using var paint = new SKPaint();
    using var path = new SKPath();  // Empty
    
    // Should not throw
    canvas.DrawTextOnPath("text", path, 0, 0, paint);
}
```

### Run Tests

```bash
dotnet test tests/SkiaSharp.Tests.Console/SkiaSharp.Tests.Console.csproj
```

> **🛑 Tests MUST PASS.** Do not skip tests. Do not claim completion if tests fail.
>
> Skipping is only acceptable for missing hardware (no GPU drivers, no display).

## Phase 5: Verify

- [ ] Fix doesn't break existing tests
- [ ] Check for similar issues in related code
- [ ] Minimal changes only (no unrelated cleanup)

## Error Recovery

| Issue | Recovery |
|-------|----------|
| Fix causes new test failures | Revert changes (`git checkout -- <file>`), re-analyze root cause |
| Can't reproduce the bug | Ask user for more details: platform, version, exact reproduction steps |
| Fix works locally but fails CI | Check platform-specific code paths; may need `#if` conditionals |
| Similar bugs exist in other methods | Consider if root cause is shared; may need broader fix |
| Native crash (AccessViolation) | Check C API, verify pointer ownership and lifetime |
| Memory leak persists | Review disposal pattern; check if object implements `ISKReferenceCounted` |
| `EntryPointNotFoundException` | You modified C API but didn't rebuild natives — run `dotnet cake --target=externals-{platform}` |
