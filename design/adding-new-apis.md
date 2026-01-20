# Adding New APIs to SkiaSharp

> **Hands-on tutorial:** [QUICKSTART.md](QUICKSTART.md)  
> **Quick patterns:** [AGENTS.md](../AGENTS.md)

---

## Introduction

This guide walks through the complete process of adding a new Skia API to SkiaSharp, from identifying the C++ API to testing the final C# binding.

SkiaSharp uses a **three-layer architecture** to bridge native C++ code with managed C#:

```
C++ Skia Library (externals/skia/)
    ↓ Type casting
C API Layer (externals/skia/include/c/, externals/skia/src/c/)
    ↓ P/Invoke
C# Wrapper Layer (binding/SkiaSharp/)
```

**Why this architecture?**
- C++ exceptions cannot cross P/Invoke boundaries
- C APIs are ABI-stable and easier to maintain
- C# provides safety, validation, and idiomatic .NET patterns

### Simple Example: SKPaint.IsAntialias

Before diving into complex examples, here's a simple property to illustrate the pattern:

**Layer 1: C++ API** (`externals/skia/include/core/SkPaint.h`)
```cpp
class SK_API SkPaint {
public:
  bool isAntiAlias() const;
  void setAntiAlias(bool aa);
};
```

**Layer 2: C API** (`externals/skia/src/c/sk_paint.cpp`)
```cpp
bool sk_paint_is_antialias(const sk_paint_t* cpaint) {
  return AsPaint(cpaint)->isAntiAlias();
}

void sk_paint_set_antialias(sk_paint_t* cpaint, bool aa) {
  AsPaint(cpaint)->setAntiAlias(aa);
}
```

**Layer 3: P/Invoke** (`binding/SkiaSharp/SkiaApi.cs`)
```csharp
[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
public extern static bool sk_paint_is_antialias(sk_paint_t t);

[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
public extern static void sk_paint_set_antialias(sk_paint_t t, bool aa);
```

**Layer 4: C# Wrapper** (`binding/SkiaSharp/SKPaint.cs`)
```csharp
public class SKPaint : SKObject
{
  public bool IsAntialias {
    get { return SkiaApi.sk_paint_is_antialias(Handle); }
    set { SkiaApi.sk_paint_set_antialias(Handle, value); }
  }
}
```

As a result, the C# API is idiomatic and appears similar to the C++ API.

### The Mono/Skia Fork

Since the C API is currently a work in progress of the Skia project, we maintain a fork at https://github.com/mono/skia that has our additions. We intend to upstream those changes to Google where appropriate.

## Prerequisites

Before adding a new API, you should understand:
- [Architecture Overview](architecture-overview.md) - The three-layer structure
- [Memory Management](memory-management.md) - **Critical:** Pointer types and ownership
- [Error Handling](error-handling.md) - How errors propagate

## The Process

Adding a new API involves **four steps** through the three layers:

```
1. Analyze C++ API      →  Identify pointer type & error handling
2. Add C API Layer      →  Create C wrapper functions  
3. Add P/Invoke         →  Declare C# interop
4. Add C# Wrapper       →  Create idiomatic C# API
```

**Critical decisions:**
- **Pointer type** - Determines disposal pattern (see [memory-management.md](memory-management.md))
- **Error handling** - Can it fail? Returns bool/null or throws?
- **Parameter types** - Reference-counted parameters need `sk_ref_sp`

**File locations:**
```
C++:   externals/skia/include/core/SkCanvas.h
C API: externals/skia/src/c/sk_canvas.cpp
       externals/skia/include/c/sk_canvas.h  
C#:    binding/SkiaSharp/SKCanvas.cs
       binding/SkiaSharp/SkiaApi.cs
```

## Step 1: Analyze the C++ API

### Find the C++ API

Locate the API in Skia's C++ headers:

```bash
# Search Skia headers
grep -r "drawArc" externals/skia/include/core/

# Common locations:
# - externals/skia/include/core/SkCanvas.h
# - externals/skia/include/core/SkPaint.h  
# - externals/skia/include/core/SkImage.h
```

**Example:** Let's add `SkCanvas::drawArc()`

```cpp
// In SkCanvas.h
class SK_API SkCanvas {
public:
    void drawArc(const SkRect& oval, SkScalar startAngle, SkScalar sweepAngle,
                 bool useCenter, const SkPaint& paint);
};
```

### Determine Pointer Type and Ownership

**Key questions to answer:**

1. **What type of object is this method on?**
   - `SkCanvas` → Owned pointer (mutable, not ref-counted)

2. **What parameters does it take?**
   - `const SkRect&` → Raw pointer (non-owning, value parameter)
   - `const SkPaint&` → Raw pointer (non-owning, borrowed)
   - `SkScalar` → Value type (primitive)
   - `bool` → Value type (primitive)

3. **Does it return anything?**
   - `void` → No return value

4. **Can it fail?**
   - Drawing operations typically don't fail
   - May clip or do nothing if parameters invalid
   - No error return needed

**Pointer type analysis:**
- Canvas: Owned (must exist for call duration)
- Paint: Borrowed (only used during call)
- Rect: Value (copied, safe to stack allocate)

**See [Memory Management](memory-management.md) for detailed pointer type identification.**

### Check Skia Documentation

```cpp
// From SkCanvas.h comments:
/** Draws arc of oval bounded by oval_rect.
    @param oval        rect bounds of oval containing arc
    @param startAngle  starting angle in degrees
    @param sweepAngle  sweep angle in degrees
    @param useCenter   if true, include center of oval
    @param paint       paint to use
*/
void drawArc(const SkRect& oval, SkScalar startAngle, SkScalar sweepAngle,
             bool useCenter, const SkPaint& paint);
```

## Step 2: Add C API Layer

### File Location

Add to existing or create new C API files:
- Header: `externals/skia/include/c/sk_canvas.h`
- Implementation: `externals/skia/src/c/sk_canvas.cpp`

### Add Function Declaration to Header

```cpp
// In externals/skia/include/c/sk_canvas.h

SK_C_API void sk_canvas_draw_arc(
    sk_canvas_t* ccanvas,
    const sk_rect_t* oval,
    float startAngle,
    float sweepAngle,
    bool useCenter,
    const sk_paint_t* cpaint);
```

**Naming convention:**
- Function: `sk_<type>_<action>`
- Example: `sk_canvas_draw_arc`

**Parameter types:**
- C++ `SkCanvas*` → C `sk_canvas_t*`
- C++ `const SkRect&` → C `const sk_rect_t*`
- C++ `SkScalar` → C `float`
- C++ `const SkPaint&` → C `const sk_paint_t*`
- C++ `bool` → C `bool`

### Add Implementation

```cpp
// In externals/skia/src/c/sk_canvas.cpp

void sk_canvas_draw_arc(
    sk_canvas_t* ccanvas,
    const sk_rect_t* oval,
    float startAngle,
    float sweepAngle,
    bool useCenter,
    const sk_paint_t* cpaint)
{
    // Defensive null checks (optional but recommended)
    if (!ccanvas || !oval || !cpaint)
        return;
    
    // Convert C types to C++ types and call
    AsCanvas(ccanvas)->drawArc(
        *AsRect(oval),        // Dereference pointer to get reference
        startAngle,           // SkScalar is float
        sweepAngle,
        useCenter,
        *AsPaint(cpaint));    // Dereference pointer to get reference
}
```

**Key points:**
- Type conversion macros: `AsCanvas()`, `AsRect()`, `AsPaint()`
- Dereference pointers (`*`) to get C++ references
- Keep implementation simple - C# validates parameters
- No try-catch needed (C# prevents invalid inputs)

### Special Cases

#### Returning Owned Pointers

```cpp
SK_C_API sk_canvas_t* sk_canvas_new_from_bitmap(const sk_bitmap_t* bitmap) {
    // Create new canvas - caller owns
    return ToCanvas(new SkCanvas(*AsBitmap(bitmap)));
}
```

#### Returning Reference-Counted Pointers

```cpp
SK_C_API sk_image_t* sk_image_new_from_bitmap(const sk_bitmap_t* bitmap) {
    // SkImages::RasterFromBitmap returns sk_sp<SkImage>
    // .release() transfers ownership (ref count = 1)
    return ToImage(SkImages::RasterFromBitmap(*AsBitmap(bitmap)).release());
}
```

#### Taking Reference-Counted Parameters

```cpp
SK_C_API sk_image_t* sk_image_apply_filter(
    const sk_image_t* image,
    const sk_imagefilter_t* filter)
{
    // Filter is ref-counted, C++ wants sk_sp
    // sk_ref_sp increments ref count before passing
    return ToImage(AsImage(image)->makeWithFilter(
        sk_ref_sp(AsImageFilter(filter))).release());
}
```

#### Boolean Return for Success/Failure

```cpp
SK_C_API bool sk_bitmap_try_alloc_pixels(
    sk_bitmap_t* bitmap,
    const sk_imageinfo_t* info)
{
    // C++ method naturally returns bool
    return AsBitmap(bitmap)->tryAllocPixels(AsImageInfo(info));
}
```

**Note:** C# validates `bitmap` and `info` before calling.

#### Null Return for Factory Failure

```cpp
SK_C_API sk_surface_t* sk_surface_new_raster(const sk_imageinfo_t* info) {
    auto surface = SkSurfaces::Raster(AsImageInfo(info));
    return ToSurface(surface.release());  // Returns nullptr if Skia factory fails
}
```

**Note:** C# checks for `IntPtr.Zero` and throws exception.

## Step 3: Add P/Invoke Declaration

### Manual Declaration

For simple functions, add to `binding/SkiaSharp/SkiaApi.cs`:

```csharp
// In SkiaApi.cs
internal static partial class SkiaApi
{
    [DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
    public static extern void sk_canvas_draw_arc(
        sk_canvas_t canvas,
        sk_rect_t* oval,
        float startAngle,
        float sweepAngle,
        [MarshalAs(UnmanagedType.I1)] bool useCenter,
        sk_paint_t paint);
}
```

**Type mappings:**
- C `sk_canvas_t*` → C# `sk_canvas_t` (IntPtr alias)
- C `const sk_rect_t*` → C# `sk_rect_t*` (pointer)
- C `float` → C# `float`
- C `bool` → C# `bool` with `MarshalAs(UnmanagedType.I1)`
- C `sk_paint_t*` → C# `sk_paint_t` (IntPtr alias)

**Note:** Bool marshaling ensures correct size (1 byte).

### Generated Declaration

For bulk additions, update generator config and regenerate:

```bash
dotnet run --project utils/SkiaSharpGenerator/SkiaSharpGenerator.csproj -- generate
```

The generator creates `SkiaApi.generated.cs` with P/Invoke declarations.

## Step 4: Add C# Wrapper

### Determine Wrapper Pattern

Based on the pointer type analysis:

| Pointer Type | C# Pattern | Example |
|--------------|------------|---------|
| Owned | `SKObject` with `DisposeNative()` | `SKCanvas`, `SKPaint` |
| Reference-Counted | `SKObject, ISKReferenceCounted` | `SKImage`, `SKShader` |
| Non-Owning | `OwnsHandle = false` | Returned child objects |

### Add Method to C# Class

```csharp
// In binding/SkiaSharp/SKCanvas.cs

public unsafe class SKCanvas : SKObject
{
    public void DrawArc(SKRect oval, float startAngle, float sweepAngle, bool useCenter, SKPaint paint)
    {
        // Step 1: Validate parameters
        if (paint == null)
            throw new ArgumentNullException(nameof(paint));
        
        // Step 2: Call P/Invoke
        SkiaApi.sk_canvas_draw_arc(Handle, &oval, startAngle, sweepAngle, useCenter, paint.Handle);
    }
}
```

**Best practices:**
1. **Parameter validation** - Check nulls, disposed objects
2. **Proper marshaling** - Use `&` for struct pointers
3. **Resource tracking** - Handle ownership transfers if needed
4. **Documentation** - Add XML comments

### Handle Different Return Types

#### Void Return (No Error)

```csharp
public void DrawArc(SKRect oval, float startAngle, float sweepAngle, bool useCenter, SKPaint paint)
{
    if (paint == null)
        throw new ArgumentNullException(nameof(paint));
    
    SkiaApi.sk_canvas_draw_arc(Handle, &oval, startAngle, sweepAngle, useCenter, paint.Handle);
}
```

#### Boolean Return (Success/Failure)

```csharp
public bool TryAllocPixels(SKImageInfo info)
{
    var nInfo = SKImageInfoNative.FromManaged(ref info);
    return SkiaApi.sk_bitmap_try_alloc_pixels(Handle, &nInfo);
}

// Or throw on failure:
public void AllocPixels(SKImageInfo info)
{
    if (!TryAllocPixels(info))
        throw new InvalidOperationException($"Failed to allocate {info.Width}x{info.Height} pixels");
}
```

#### Owned Pointer Return

```csharp
public static SKCanvas Create(SKBitmap bitmap)
{
    if (bitmap == null)
        throw new ArgumentNullException(nameof(bitmap));
    
    var handle = SkiaApi.sk_canvas_new_from_bitmap(bitmap.Handle);
    
    if (handle == IntPtr.Zero)
        throw new InvalidOperationException("Failed to create canvas");
    
    // Returns owned object
    return GetOrAddObject(handle, owns: true, (h, o) => new SKCanvas(h, o));
}
```

#### Reference-Counted Pointer Return

```csharp
public static SKImage FromBitmap(SKBitmap bitmap)
{
    if (bitmap == null)
        throw new ArgumentNullException(nameof(bitmap));
    
    var handle = SkiaApi.sk_image_new_from_bitmap(bitmap.Handle);
    
    // Returns ref-counted object (ref count = 1), or null if failed
    return GetObject(handle);
}

// ✅ Usage - check for null
var image = SKImage.FromBitmap(bitmap);
if (image == null)
    throw new InvalidOperationException("Failed to create image");
```

#### Non-Owning Pointer Return

```csharp
public SKSurface Surface
{
    get {
        var handle = SkiaApi.sk_get_surface(Handle);
        if (handle == IntPtr.Zero)
            return null;
        
        // Surface owned by canvas, return non-owning wrapper
        return GetOrAddObject(handle, owns: false, (h, o) => new SKSurface(h, o));
    }
}
```

### Handle Ownership Transfer

```csharp
public void DrawDrawable(SKDrawable drawable, SKMatrix matrix)
{
    if (drawable == null)
        throw new ArgumentNullException(nameof(drawable));
    
    // Canvas takes ownership of drawable
    drawable.RevokeOwnership(this);
    
    SkiaApi.sk_canvas_draw_drawable(Handle, drawable.Handle, &matrix);
}
```

### Add Overloads for Convenience

```csharp
// Core method with all parameters
public void DrawArc(SKRect oval, float startAngle, float sweepAngle, bool useCenter, SKPaint paint)
{
    if (paint == null)
        throw new ArgumentNullException(nameof(paint));
    SkiaApi.sk_canvas_draw_arc(Handle, &oval, startAngle, sweepAngle, useCenter, paint.Handle);
}

// Overload with SKPoint center and radius
public void DrawArc(SKPoint center, float radius, float startAngle, float sweepAngle, bool useCenter, SKPaint paint)
{
    var oval = new SKRect(
        center.X - radius, center.Y - radius,
        center.X + radius, center.Y + radius);
    DrawArc(oval, startAngle, sweepAngle, useCenter, paint);
}

// Overload with individual coordinates
public void DrawArc(float left, float top, float right, float bottom, 
                    float startAngle, float sweepAngle, bool useCenter, SKPaint paint)
{
    DrawArc(new SKRect(left, top, right, bottom), startAngle, sweepAngle, useCenter, paint);
}
```

## Complete Example: Adding DrawArc

### C API Header

```cpp
// externals/skia/include/c/sk_canvas.h
SK_C_API void sk_canvas_draw_arc(
    sk_canvas_t* ccanvas,
    const sk_rect_t* oval,
    float startAngle,
    float sweepAngle,
    bool useCenter,
    const sk_paint_t* cpaint);
```

### C API Implementation

```cpp
// externals/skia/src/c/sk_canvas.cpp
void sk_canvas_draw_arc(
    sk_canvas_t* ccanvas,
    const sk_rect_t* oval,
    float startAngle,
    float sweepAngle,
    bool useCenter,
    const sk_paint_t* cpaint)
{
    if (!ccanvas || !oval || !cpaint)
        return;
    
    AsCanvas(ccanvas)->drawArc(*AsRect(oval), startAngle, sweepAngle, useCenter, *AsPaint(cpaint));
}
```

### P/Invoke Declaration

```csharp
// binding/SkiaSharp/SkiaApi.cs
[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
public static extern void sk_canvas_draw_arc(
    sk_canvas_t canvas,
    sk_rect_t* oval,
    float startAngle,
    float sweepAngle,
    [MarshalAs(UnmanagedType.I1)] bool useCenter,
    sk_paint_t paint);
```

### C# Wrapper

```csharp
// binding/SkiaSharp/SKCanvas.cs
public unsafe class SKCanvas : SKObject
{
    /// <summary>
    /// Draws an arc of an oval.
    /// </summary>
    /// <param name="oval">Bounds of oval containing arc.</param>
    /// <param name="startAngle">Starting angle in degrees.</param>
    /// <param name="sweepAngle">Sweep angle in degrees; positive is clockwise.</param>
    /// <param name="useCenter">If true, include the center of the oval.</param>
    /// <param name="paint">Paint to use for the arc.</param>
    public void DrawArc(SKRect oval, float startAngle, float sweepAngle, bool useCenter, SKPaint paint)
    {
        if (paint == null)
            throw new ArgumentNullException(nameof(paint));
        
        SkiaApi.sk_canvas_draw_arc(Handle, &oval, startAngle, sweepAngle, useCenter, paint.Handle);
    }
}
```

## Testing Your Changes

### Build and Test

```bash
# Build the project
dotnet cake --target=libs

# Run tests
dotnet cake --target=tests
```

### Write Unit Tests

```csharp
// tests/SkiaSharp.Tests/SKCanvasTest.cs
[Fact]
public void DrawArcRendersCorrectly()
{
    using (var bitmap = new SKBitmap(100, 100))
    using (var canvas = new SKCanvas(bitmap))
    using (var paint = new SKPaint { Color = SKColors.Red, Style = SKPaintStyle.Stroke, StrokeWidth = 2 })
    {
        canvas.Clear(SKColors.White);
        canvas.DrawArc(new SKRect(10, 10, 90, 90), 0, 90, false, paint);
        
        // Verify arc was drawn
        Assert.NotEqual(SKColors.White, bitmap.GetPixel(50, 10));
    }
}

[Fact]
public void DrawArcThrowsOnNullPaint()
{
    using (var bitmap = new SKBitmap(100, 100))
    using (var canvas = new SKCanvas(bitmap))
    {
        Assert.Throws<ArgumentNullException>(() => 
            canvas.DrawArc(new SKRect(10, 10, 90, 90), 0, 90, false, null));
    }
}
```

### Manual Testing

```csharp
using SkiaSharp;

using (var bitmap = new SKBitmap(400, 400))
using (var canvas = new SKCanvas(bitmap))
using (var paint = new SKPaint { Color = SKColors.Blue, Style = SKPaintStyle.Stroke, StrokeWidth = 4 })
{
    canvas.Clear(SKColors.White);
    
    // Test various arcs
    canvas.DrawArc(new SKRect(50, 50, 150, 150), 0, 90, false, paint);      // Open arc
    canvas.DrawArc(new SKRect(200, 50, 300, 150), 0, 90, true, paint);      // Closed arc
    canvas.DrawArc(new SKRect(50, 200, 150, 300), -45, 180, false, paint);  // Larger sweep
    
    // Save to file
    using (var image = SKImage.FromBitmap(bitmap))
    using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
    using (var stream = File.OpenWrite("arc_test.png"))
    {
        data.SaveTo(stream);
    }
}
```

## Common Patterns and Examples

### Pattern: Simple Drawing Method

**C++:** `void SkCanvas::drawCircle(SkPoint center, SkScalar radius, const SkPaint& paint)`

```cpp
// C API
SK_C_API void sk_canvas_draw_circle(sk_canvas_t* canvas, float cx, float cy, float radius, const sk_paint_t* paint);

void sk_canvas_draw_circle(sk_canvas_t* canvas, float cx, float cy, float radius, const sk_paint_t* paint) {
    AsCanvas(canvas)->drawCircle(cx, cy, radius, *AsPaint(paint));
}
```

```csharp
// C#
public void DrawCircle(float cx, float cy, float radius, SKPaint paint)
{
    if (paint == null)
        throw new ArgumentNullException(nameof(paint));
    SkiaApi.sk_canvas_draw_circle(Handle, cx, cy, radius, paint.Handle);
}

public void DrawCircle(SKPoint center, float radius, SKPaint paint) =>
    DrawCircle(center.X, center.Y, radius, paint);
```

### Pattern: Factory with Reference Counting

**C++:** `sk_sp<SkImage> SkImages::DeferredFromEncodedData(sk_sp<SkData> data)`

```cpp
// C API
SK_C_API sk_image_t* sk_image_new_from_encoded(const sk_data_t* data);

sk_image_t* sk_image_new_from_encoded(const sk_data_t* data) {
    return ToImage(SkImages::DeferredFromEncodedData(sk_ref_sp(AsData(data))).release());
}
```

```csharp
// C#
public static SKImage FromEncodedData(SKData data)
{
    if (data == null)
        throw new ArgumentNullException(nameof(data));
    
    var handle = SkiaApi.sk_image_new_from_encoded(data.Handle);
    if (handle == IntPtr.Zero)
        throw new InvalidOperationException("Failed to decode image");
    
    return GetObject(handle);
}
```

### Pattern: Mutable Object Creation

**C++:** `SkPaint::SkPaint()`

```cpp
// C API
SK_C_API sk_paint_t* sk_paint_new(void);
SK_C_API void sk_paint_delete(sk_paint_t* paint);

sk_paint_t* sk_paint_new(void) {
    return ToPaint(new SkPaint());
}

void sk_paint_delete(sk_paint_t* paint) {
    delete AsPaint(paint);
}
```

```csharp
// C#
public class SKPaint : SKObject, ISKSkipObjectRegistration
{
    public SKPaint() : base(IntPtr.Zero, true)
    {
        Handle = SkiaApi.sk_paint_new();
    }
    
    protected override void DisposeNative()
    {
        SkiaApi.sk_paint_delete(Handle);
    }
}
```

### Pattern: Property Getter/Setter

**C++:** `SkColor SkPaint::getColor()` and `void SkPaint::setColor(SkColor color)`

```cpp
// C API
SK_C_API sk_color_t sk_paint_get_color(const sk_paint_t* paint);
SK_C_API void sk_paint_set_color(sk_paint_t* paint, sk_color_t color);

sk_color_t sk_paint_get_color(const sk_paint_t* paint) {
    return AsPaint(paint)->getColor();
}

void sk_paint_set_color(sk_paint_t* paint, sk_color_t color) {
    AsPaint(paint)->setColor(color);
}
```

```csharp
// C#
public SKColor Color
{
    get => (SKColor)SkiaApi.sk_paint_get_color(Handle);
    set => SkiaApi.sk_paint_set_color(Handle, (uint)value);
}
```

## Checklist for Adding New APIs

### Analysis Phase
- [ ] Located C++ API in Skia headers
- [ ] Identified pointer type (raw, owned, ref-counted)
- [ ] Determined ownership semantics
- [ ] Checked error conditions
- [ ] Read Skia documentation/comments

### C API Layer
- [ ] Added function declaration to header
- [ ] Implemented function in .cpp file
- [ ] Used correct type conversion macros
- [ ] Handled ref-counting correctly (if applicable)
- [ ] Used `.release()` on `sk_sp<T>` returns
- [ ] Used `sk_ref_sp()` for ref-counted parameters

### P/Invoke Layer
- [ ] Added P/Invoke declaration
- [ ] Used correct type mappings
- [ ] Applied correct marshaling attributes
- [ ] Specified calling convention

### C# Wrapper Layer
- [ ] Added method to appropriate class
- [ ] Validated parameters
- [ ] Checked return values
- [ ] Handled ownership correctly
- [ ] Added XML documentation
- [ ] Created convenience overloads

### Testing
- [ ] Built project successfully
- [ ] Wrote unit tests
- [ ] Manual testing completed
- [ ] Verified memory management (no leaks)
- [ ] Tested error cases

## Troubleshooting

### Common Build Errors

**"Cannot find sk_canvas_draw_arc"**
- C API function not exported from native library
- Rebuild native library: `dotnet cake --target=externals`

**"Method not found" at runtime**
- P/Invoke signature doesn't match C API
- Check calling convention and parameter types

**Memory leaks**
- Check pointer type identification
- Verify ownership transfer
- Use memory profiler to track leaks

### Common Runtime Errors

**Crash in native code**
- Null pointer passed to C API
- Add null checks in C API layer
- Add validation in C# layer

**ObjectDisposedException**
- Using disposed object
- Check object lifecycle
- Don't cache references to child objects

**InvalidOperationException**
- C API returned error
- Check return value handling
- Verify error conditions

## Next Steps

- Review [Architecture Overview](architecture-overview.md) for context
- Study [Memory Management](memory-management.md) for pointer types
- Read [Error Handling](error-handling.md) for error patterns
- See [Layer Mapping](layer-mapping.md) for detailed type mappings

## Additional Resources

- Existing wiki: [Creating Bindings](https://github.com/mono/SkiaSharp/wiki/Creating-Bindings)
- Skia C++ documentation: https://skia.org/docs/
- Example PRs adding new APIs in SkiaSharp repository
