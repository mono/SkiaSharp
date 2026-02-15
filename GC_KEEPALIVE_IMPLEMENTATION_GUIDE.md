# GC.KeepAlive Implementation Guide for SkiaSharp

## Issue Overview
This document describes the implementation of GC.KeepAlive calls to fix a critical GC safety issue in SkiaSharp's P/Invoke layer.

### Background
As described in [Chris Brumme's blog post on Lifetime, GC.KeepAlive, handle recycling](https://learn.microsoft.com/en-us/archive/blogs/cbrumme/lifetime-gc-keepalive-handle-recycling):

> Once we've extracted `_handle` from `this`, there are no further uses of this object. In other words, `this` can be collected even while you are executing an instance method on that object. But what if class `C` has a `Finalize()` method which closes `_handle`? When we call `C.OperateOnHandle()`, we now have a race between the application and the GC / Finalizer. Eventually, that's a race we're going to lose.

**The problem**: The .NET runtime cannot see into native code. Once a handle is extracted from a managed object and passed to native code, the GC may collect the managed object (and run its finalizer) while the native code is still executing.

**The solution**: Use `GC.KeepAlive()` after P/Invoke calls to ensure managed objects remain alive until the native call completes.

## Implementation Pattern

### Basic Pattern
For any public method that:
1. Takes reference type parameters (classes inheriting from SKObject, or other reference types like SKData, arrays, etc.)
2. Calls SkiaApi.* P/Invoke methods
3. Passes `.Handle` from those parameters to the P/Invoke

Add `GC.KeepAlive()` calls after the P/Invoke for each reference type parameter.

### Example: Before
```csharp
public void DrawPicture (SKPicture picture, SKPaint paint = null)
{
    if (picture == null)
        throw new ArgumentNullException (nameof (picture));
    SkiaApi.sk_canvas_draw_picture (Handle, picture.Handle, null, paint == null ? IntPtr.Zero : paint.Handle);
}
```

### Example: After
```csharp
public void DrawPicture (SKPicture picture, SKPaint paint = null)
{
    if (picture == null)
        throw new ArgumentNullException (nameof (picture));
    SkiaApi.sk_canvas_draw_picture (Handle, picture.Handle, null, paint == null ? IntPtr.Zero : paint.Handle);
    GC.KeepAlive (picture);
    GC.KeepAlive (paint);
}
```

### Property Setters
For property setters that call P/Invoke:

**Before:**
```csharp
public SKShader Shader {
    get => SKShader.GetObject (SkiaApi.sk_paint_get_shader (Handle));
    set => SkiaApi.sk_paint_set_shader (Handle, value == null ? IntPtr.Zero : value.Handle);
}
```

**After:**
```csharp
public SKShader Shader {
    get => SKShader.GetObject (SkiaApi.sk_paint_get_shader (Handle));
    set {
        SkiaApi.sk_paint_set_shader (Handle, value == null ? IntPtr.Zero : value.Handle);
        GC.KeepAlive (value);
    }
}
```

### Methods with Multiple Parameters
Add GC.KeepAlive for ALL reference type parameters:

```csharp
public void DrawPath (SKPath path, SKPaint paint)
{
    if (paint == null)
        throw new ArgumentNullException (nameof (paint));
    if (path == null)
        throw new ArgumentNullException (nameof (path));
    SkiaApi.sk_canvas_draw_path (Handle, path.Handle, paint.Handle);
    GC.KeepAlive (path);   // Keep ALL reference type params alive
    GC.KeepAlive (paint);
}
```

### Struct Parameters with Reference Fields
For struct parameters that contain reference type fields (like SKCanvasSaveLayerRec), keep those fields alive:

```csharp
public int SaveLayer (in SKCanvasSaveLayerRec rec)
{
    var native = rec.ToNative ();
    var result = SkiaApi.sk_canvas_save_layer_rec (Handle, &native);
    GC.KeepAlive (rec.Paint);     // Keep reference fields alive
    GC.KeepAlive (rec.Backdrop);
    return result;
}
```

### Constructors
Constructors that use reference type parameters also need protection:

```csharp
public SKCanvas (SKBitmap bitmap)
    : this (IntPtr.Zero, true)
{
    if (bitmap == null)
        throw new ArgumentNullException (nameof (bitmap));
    Handle = SkiaApi.sk_canvas_new_from_bitmap (bitmap.Handle);
    GC.KeepAlive (bitmap);
}
```

## What NOT to Fix

### Don't add GC.KeepAlive for:
1. **Value types** (int, float, SKRect, SKColor, etc.)
2. **Strings** (they are handled specially by the marshaler)
3. **`this.Handle`** (the current object is implicitly kept alive)
4. **Methods that don't call P/Invoke** (wrapper methods that just call other managed methods)

## Files Completed

### Fully Fixed
- [x] **SKCanvas.cs** - 30+ methods including DrawPicture, DrawImage, DrawPath, DrawPaint, etc.
- [x] **SKPaint.cs** - Constructor, properties (Shader, MaskFilter, ColorFilter, ImageFilter, Blender, PathEffect), GetFillPath

### Partially Fixed
- [ ] **SKImage.cs** - 4/17 methods (FromPixelCopy, FromPixels, FromEncodedData, PeekPixels)
- [ ] **SKBitmap.cs** - 1/5 methods (ExtractSubset)
- [ ] **SKPath.cs** - 6/18 methods (Constructor, AddRoundRect, AddPath variants, AddPathReverse)

## Files Requiring Work

### High Priority (Common APIs)
- [ ] **SKPath.cs** - 12 more methods (Op, Simplify, ToWinding, Transform, IsRRect, etc.)
- [ ] **SKFont.cs** - 8 methods
- [ ] **SKBitmap.cs** - 4 more methods (ExtractAlpha, InstallPixels, PeekPixels, Swap)
- [ ] **SKImage.cs** - 13 more methods
- [ ] **SKSurface.cs** - 9 methods
- [ ] **SKShader.cs** - 13 methods
- [ ] **SKImageFilter.cs** - 28 methods
- [ ] **SKTextBlob.cs** - 18 methods
- [ ] **SKRegion.cs** - 11 methods

### Medium Priority
- [ ] **SKPixmap.cs** - 5 methods
- [ ] **SKCodec.cs** - 2 methods
- [ ] **SKColorFilter.cs** - 2 methods
- [ ] **SKColorSpace.cs** - 3 methods
- [ ] **SKData.cs** - 3 methods
- [ ] **SKDocument.cs** - 3 methods
- [ ] **SKDrawable.cs** - 1 method
- [ ] **SKFontManager.cs** - 5 methods
- [ ] **SKFontStyleSet.cs** - 3 methods
- [ ] **SKPathEffect.cs** - 4 methods
- [ ] **SKPathMeasure.cs** - 3 methods
- [ ] **SKPicture.cs** - 4 methods
- [ ] **SKRuntimeEffect.cs** - 6 methods
- [ ] **SKTypeface.cs** - 3 methods

### Lower Priority (Less Common/Advanced APIs)
- [ ] **GRContext.cs** - 1 method
- [ ] **SKColorSpaceStructs.cs** - 1 method
- [ ] **SKGraphics.cs** - 1 method
- [ ] **SKNWayCanvas.cs** - 2 methods
- [ ] **SKObject.cs** - 4 methods
- [ ] **SKOverdrawCanvas.cs** - 1 method
- [ ] **SKRoundRect.cs** - 1 method
- [ ] **SKStream.cs** - 3 methods
- [ ] **SKSVG.cs** - 1 method

## Testing Strategy

After implementing GC.KeepAlive calls:

1. **Compile Tests**: Ensure code compiles without errors
2. **Unit Tests**: Run existing xUnit tests in `tests/Tests/SkiaSharp/`
3. **Stress Tests**: Consider adding GC stress tests that:
   - Create objects
   - Pass them to P/Invoke methods
   - Force GC collection during the call
   - Verify no crashes or corruption

Example stress test pattern:
```csharp
[Fact]
public void DrawPicture_WithGCDuringCall_DoesNotCrash()
{
    using var surface = SKSurface.Create(new SKImageInfo(100, 100));
    using var canvas = surface.Canvas;
    using var recorder = new SKPictureRecorder();
    using var picture = recorder.BeginRecording(SKRect.Create(100, 100));
    
    // Draw with immediate GC pressure
    for (int i = 0; i < 1000; i++)
    {
        canvas.DrawPicture(picture);
        if (i % 10 == 0) GC.Collect(2, GCCollectionMode.Forced);
    }
}
```

## Implementation Checklist for Each File

When fixing a file:
1. [ ] Identify all public methods that call SkiaApi.* with reference type parameters
2. [ ] For each method, identify all reference type parameters
3. [ ] Add GC.KeepAlive calls after the P/Invoke for each reference type parameter
4. [ ] Check property setters that call P/Invoke
5. [ ] Check constructors that use reference type parameters
6. [ ] Review struct parameters for embedded reference types
7. [ ] Verify the change compiles
8. [ ] Commit with descriptive message

## References

- [Original Issue](https://github.com/mono/SkiaSharp/issues/XXXX)
- [Chris Brumme's Blog: Lifetime, GC.KeepAlive, handle recycling](https://learn.microsoft.com/en-us/archive/blogs/cbrumme/lifetime-gc-keepalive-handle-recycling)
- [Uno Platform PR #21660](https://github.com/unoplatform/uno/pull/21660)
- [dotnet/java-interop #719](https://github.com/dotnet/java-interop/issues/719)
