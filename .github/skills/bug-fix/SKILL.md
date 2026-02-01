---
name: bug-fix
description: >
  Fix bugs in SkiaSharp C# bindings. Structured workflow for investigating, fixing,
  and testing bug reports.
  
  Triggers (after issue classification determines it's a bug):
  - Crash, exception, AccessViolationException
  - Incorrect output, wrong behavior
  - Memory leak, disposal issues
  - "fails", "broken", "doesn't work"
  
  Do NOT use directly - invoke via issue classification in copilot-instructions.
  For adding new APIs, use `add-api` skill instead.
---

# Bug Fix Skill

Fix bugs in SkiaSharp with minimal, surgical changes.

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

## Phase 4: Test

Write regression test BEFORE claiming fix is complete:

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

Run tests:
```bash
dotnet test tests/SkiaSharp.Tests.Console/SkiaSharp.Tests.Console.csproj
```

## Phase 5: Verify

- [ ] Fix doesn't break existing tests
- [ ] Check for similar issues in related code
- [ ] Minimal changes only (no unrelated cleanup)
