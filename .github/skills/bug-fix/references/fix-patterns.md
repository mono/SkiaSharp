# Common Fix Patterns

## Missing Null Check

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

## Same-Instance Return Bug

Some methods may return the **same instance** passed in. Always check before disposing:

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

**Methods that may return same instance:** `Subset()`, `ToRasterImage()`, `ToRasterImage(false)`

## Native Linking Issues (undefined symbol)

When you see `undefined symbol: xxx` errors on Linux:

1. Check if the symbol is from a missing dependency:
   ```bash
   readelf -d output/native/linux/arm64/libSkiaSharp.so | grep NEEDED
   ```

2. Compare with working platform (e.g., x64):
   ```bash
   readelf -d output/native/linux/x64/libSkiaSharp.so | grep NEEDED
   ```

3. Fix in `native/linux/build.cake` by adding the missing library to linker flags

## Platform-Specific Crashes

Check build configuration in `native/{platform}/build.cake`:
- Linker flags (`extra_ldflags`)
- Compiler flags (`extra_cflags`)
- GN args for the specific architecture

## Memory Leaks

1. Check if the object implements `ISKReferenceCounted` or `ISKNonVirtualReferenceCounted`
2. Verify disposal pattern matches ownership:
   - `owns: false` → Don't dispose (temporary reference)
   - `owns: true` or manual ownership → Must dispose
3. Check for circular references between native objects
