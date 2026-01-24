# Bug Fix Implementation

Workflow for fixing bugs in SkiaSharp.

## Workflow

### 1. Understand the Bug

Extract from the issue:
- **Symptoms:** What goes wrong? (crash, incorrect output, exception)
- **Reproduction:** Steps to trigger the bug
- **Expected:** What should happen instead
- **Environment:** Platform-specific? Version-specific?

### 2. Locate the Problem

```bash
# Find the method mentioned in the issue
grep -rn "MethodName" binding/SkiaSharp/

# Check what validation exists
# Look for: null checks, range checks, state checks

# Find the native call
grep -r "sk_.*methodname" binding/SkiaSharp/
```

### 3. Determine Fix Location

| Symptom | Likely Fix Location |
|---------|---------------------|
| ArgumentNullException | Add null check before P/Invoke |
| AccessViolationException | Missing validation, bad state |
| Incorrect output | Logic error in C# or native |
| Memory leak | Missing dispose, wrong ownership |

### 4. Implementation Checklist

- [ ] Reproduce the bug (write failing test first if possible)
- [ ] Identify root cause
- [ ] Determine fix approach
- [ ] Implement fix with minimal changes
- [ ] Add regression test
- [ ] Verify fix doesn't break existing tests
- [ ] Check for similar issues in related code

## Common Bug Patterns

### Missing Null Check

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

### Missing State Check

```csharp
// Before (crashes on empty)
public void DrawTextOnPath(string text, SKPath path, ...)
{
    SkiaApi.sk_canvas_draw_text_on_path(...);
}

// After (handles edge case)
public void DrawTextOnPath(string text, SKPath path, ...)
{
    if (path.IsEmpty)
        return;  // No-op for empty path
    SkiaApi.sk_canvas_draw_text_on_path(...);
}
```

### Same-Instance Return Bug

Some Skia methods return the same instance as an optimization. Check before disposing:

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

**Methods that may return same instance:**
- `Subset()` - when subset equals full bounds
- `ToRasterImage()` - when already raster
- `ToRasterImage(false)` - when already non-texture

## Writing Regression Tests

```csharp
[SkippableFact]
public void MethodDoesNotCrashWithEmptyInput()
{
    using var canvas = new SKCanvas(new SKBitmap(100, 100));
    using var paint = new SKPaint();
    using var path = new SKPath();  // Empty
    
    // Should not throw
    canvas.DrawTextOnPath("text", path, 0, 0, paint);
}
```

## Key Files

See [context-checklist.md](context-checklist.md) for file locations.
