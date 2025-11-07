# Layer Mapping Reference

This document provides detailed mappings between the three layers of SkiaSharp, serving as a quick reference for understanding how types, functions, and patterns translate across layer boundaries.

## Type Naming Conventions

### C++ to C API Mapping

| C++ Type | C API Type | Notes |
|----------|------------|-------|
| `SkCanvas` | `sk_canvas_t*` | Opaque pointer |
| `SkPaint` | `sk_paint_t*` | Opaque pointer |
| `SkImage` | `sk_image_t*` | Opaque pointer |
| `SkBitmap` | `sk_bitmap_t*` | Opaque pointer |
| `SkPath` | `sk_path_t*` | Opaque pointer |
| `SkRect` | `sk_rect_t` | Value type struct |
| `SkPoint` | `sk_point_t` | Value type struct |
| `SkColor` | `sk_color_t` | `uint32_t` typedef |
| `SkScalar` | `float` | Float primitive |
| `bool` | `bool` | Boolean primitive |

**Pattern:** `SkType` → `sk_type_t*` (for classes) or `sk_type_t` (for structs)

### C API to C# Mapping

| C API Type | C# Type Alias | Actual C# Type | Notes |
|------------|---------------|----------------|-------|
| `sk_canvas_t*` | `sk_canvas_t` | `IntPtr` | Handle to native object |
| `sk_paint_t*` | `sk_paint_t` | `IntPtr` | Handle to native object |
| `sk_image_t*` | `sk_image_t` | `IntPtr` | Handle to native object |
| `sk_rect_t` | `SKRect` | `struct SKRect` | Marshaled value type |
| `sk_point_t` | `SKPoint` | `struct SKPoint` | Marshaled value type |
| `sk_color_t` | `SKColor` | `uint` | Primitive |
| `float` | `float` | `float` | Primitive |
| `bool` | `bool` | `bool` | Marshaled as I1 |

**Pattern:** `sk_type_t*` → `IntPtr` handle → `SKType` C# wrapper class

### Complete Three-Layer Mapping

| Concept | C++ Layer | C API Layer | C# P/Invoke Layer | C# Wrapper Layer |
|---------|-----------|-------------|-------------------|------------------|
| **Canvas** | `SkCanvas*` | `sk_canvas_t*` | `sk_canvas_t` (IntPtr) | `SKCanvas` |
| **Paint** | `SkPaint*` | `sk_paint_t*` | `sk_paint_t` (IntPtr) | `SKPaint` |
| **Image** | `sk_sp<SkImage>` | `sk_image_t*` | `sk_image_t` (IntPtr) | `SKImage` |
| **Rectangle** | `SkRect` | `sk_rect_t` | `SKRect` | `SKRect` |
| **Point** | `SkPoint` | `sk_point_t` | `SKPoint` | `SKPoint` |
| **Color** | `SkColor` | `sk_color_t` | `uint` | `SKColor` (uint) |

## Function Naming Conventions

### Pattern: `sk_<type>_<action>[_<details>]`

**Examples:**

| C++ Method | C API Function | C# Method |
|------------|----------------|-----------|
| `SkCanvas::drawRect()` | `sk_canvas_draw_rect()` | `SKCanvas.DrawRect()` |
| `SkPaint::getColor()` | `sk_paint_get_color()` | `SKPaint.Color` (get) |
| `SkPaint::setColor()` | `sk_paint_set_color()` | `SKPaint.Color` (set) |
| `SkImage::width()` | `sk_image_get_width()` | `SKImage.Width` |
| `SkCanvas::save()` | `sk_canvas_save()` | `SKCanvas.Save()` |

**C# Naming Conventions:**
- Methods: PascalCase (`DrawRect`, not `drawRect`)
- Properties instead of get/set methods
- Parameters: camelCase
- Events: PascalCase with `Event` suffix (if applicable)

## File Organization Mapping

### Canvas Example

| Layer | File Path | Contents |
|-------|-----------|----------|
| **C++ API** | `externals/skia/include/core/SkCanvas.h` | `class SkCanvas` declaration |
| **C++ Impl** | `externals/skia/src/core/SkCanvas.cpp` | `SkCanvas` implementation |
| **C API Header** | `externals/skia/include/c/sk_canvas.h` | `sk_canvas_*` function declarations |
| **C API Impl** | `externals/skia/src/c/sk_canvas.cpp` | `sk_canvas_*` function implementations |
| **C# P/Invoke** | `binding/SkiaSharp/SkiaApi.cs` or `SkiaApi.generated.cs` | `sk_canvas_*` P/Invoke declarations |
| **C# Wrapper** | `binding/SkiaSharp/SKCanvas.cs` | `SKCanvas` class implementation |

### Type Conversion Helpers

| Layer | Location | Purpose |
|-------|----------|---------|
| **C API** | `externals/skia/src/c/sk_types_priv.h` | Type conversion macros: `AsCanvas()`, `ToCanvas()` |
| **C#** | `binding/SkiaSharp/SKObject.cs` | Base class with handle management |
| **C#** | `binding/SkiaSharp/SkiaApi.cs` | Type aliases: `using sk_canvas_t = IntPtr;` |

## Type Conversion Macros (C API Layer)

### Macro Definitions

```cpp
// In sk_types_priv.h

#define DEF_CLASS_MAP(SkType, sk_type, Name)
    // Generates:
    // static inline const SkType* As##Name(const sk_type* t)
    // static inline SkType* As##Name(sk_type* t)
    // static inline const sk_type* To##Name(const SkType* t)
    // static inline sk_type* To##Name(SkType* t)

// Example:
DEF_CLASS_MAP(SkCanvas, sk_canvas_t, Canvas)
// Generates: AsCanvas(), ToCanvas()
```

### Common Conversion Macros

| Macro | Purpose | Example |
|-------|---------|---------|
| `AsCanvas(sk_canvas_t*)` | Convert C type to C++ | `AsCanvas(ccanvas)` → `SkCanvas*` |
| `ToCanvas(SkCanvas*)` | Convert C++ type to C | `ToCanvas(canvas)` → `sk_canvas_t*` |
| `AsPaint(sk_paint_t*)` | Convert C type to C++ | `AsPaint(cpaint)` → `SkPaint*` |
| `ToPaint(SkPaint*)` | Convert C++ type to C | `ToPaint(paint)` → `sk_paint_t*` |
| `AsImage(sk_image_t*)` | Convert C type to C++ | `AsImage(cimage)` → `SkImage*` |
| `ToImage(SkImage*)` | Convert C++ type to C | `ToImage(image)` → `sk_image_t*` |
| `AsRect(sk_rect_t*)` | Convert C type to C++ | `AsRect(crect)` → `SkRect*` |
| `ToRect(SkRect*)` | Convert C++ type to C | `ToRect(rect)` → `sk_rect_t*` |

**Full list of generated macros:**

```cpp
// Pointer types
DEF_CLASS_MAP(SkCanvas, sk_canvas_t, Canvas)
DEF_CLASS_MAP(SkPaint, sk_paint_t, Paint)
DEF_CLASS_MAP(SkImage, sk_image_t, Image)
DEF_CLASS_MAP(SkBitmap, sk_bitmap_t, Bitmap)
DEF_CLASS_MAP(SkPath, sk_path_t, Path)
DEF_CLASS_MAP(SkShader, sk_shader_t, Shader)
DEF_CLASS_MAP(SkData, sk_data_t, Data)
DEF_CLASS_MAP(SkSurface, sk_surface_t, Surface)
// ... and many more

// Value types
DEF_MAP(SkRect, sk_rect_t, Rect)
DEF_MAP(SkIRect, sk_irect_t, IRect)
DEF_MAP(SkPoint, sk_point_t, Point)
DEF_MAP(SkIPoint, sk_ipoint_t, IPoint)
DEF_MAP(SkColor4f, sk_color4f_t, Color4f)
// ... and many more
```

## Parameter Passing Patterns

### By Value vs By Pointer/Reference

| C++ Parameter | C API Parameter | C# Parameter | Notes |
|---------------|-----------------|--------------|-------|
| `int x` | `int x` | `int x` | Simple value |
| `bool flag` | `bool flag` | `[MarshalAs(UnmanagedType.I1)] bool flag` | Bool needs marshaling |
| `const SkRect& rect` | `const sk_rect_t* rect` | `sk_rect_t* rect` or `ref SKRect rect` | Struct by pointer |
| `const SkPaint& paint` | `const sk_paint_t* paint` | `sk_paint_t paint` (IntPtr) | Object handle |
| `SkRect* outRect` | `sk_rect_t* outRect` | `sk_rect_t* outRect` or `out SKRect outRect` | Output parameter |

### Ownership Transfer Patterns

| C++ Pattern | C API Pattern | C# Pattern | Ownership |
|-------------|---------------|------------|-----------|
| `const SkPaint&` | `const sk_paint_t*` | `SKPaint paint` | Borrowed (no transfer) |
| `new SkCanvas()` | `sk_canvas_t* sk_canvas_new()` | `new SKCanvas()` | Caller owns |
| `sk_sp<SkImage>` returns | `sk_image_t* sk_image_new()` | `SKImage.FromX()` | Caller owns (ref count = 1) |
| Takes `sk_sp<SkData>` | `sk_data_t*` with `sk_ref_sp()` | `SKData data` | Shared (ref count++) |

## Memory Management Patterns

### Owned Objects (Delete on Dispose)

| Layer | Pattern | Example |
|-------|---------|---------|
| **C++** | `new`/`delete` | `auto canvas = new SkCanvas(bitmap); delete canvas;` |
| **C API** | `_new()`/`_delete()` | `sk_canvas_t* c = sk_canvas_new(); sk_canvas_delete(c);` |
| **C#** | Constructor/`Dispose()` | `var c = new SKCanvas(bitmap); c.Dispose();` |

**C# Implementation:**
```csharp
public class SKCanvas : SKObject
{
    public SKCanvas(SKBitmap bitmap) : base(IntPtr.Zero, true)
    {
        Handle = SkiaApi.sk_canvas_new_from_bitmap(bitmap.Handle);
    }
    
    protected override void DisposeNative()
    {
        SkiaApi.sk_canvas_destroy(Handle);
    }
}
```

### Reference-Counted Objects (Unref on Dispose)

| Layer | Pattern | Example |
|-------|---------|---------|
| **C++** | `sk_sp<T>` or `ref()`/`unref()` | `sk_sp<SkImage> img = ...; // auto unref` |
| **C API** | `_ref()`/`_unref()` | `sk_image_ref(img); sk_image_unref(img);` |
| **C#** | `ISKReferenceCounted` | `var img = SKImage.FromX(); img.Dispose();` |

**C# Implementation:**
```csharp
public class SKImage : SKObject, ISKReferenceCounted
{
    public static SKImage FromBitmap(SKBitmap bitmap)
    {
        var handle = SkiaApi.sk_image_new_from_bitmap(bitmap.Handle);
        return GetObject(handle);  // Ref count = 1
    }
    
    // DisposeNative inherited from SKObject calls SafeUnRef() for ISKReferenceCounted
}
```

### Non-Owning References

| Layer | Pattern | Example |
|-------|---------|---------|
| **C++** | Raw pointer from getter | `SkSurface* s = canvas->getSurface();` |
| **C API** | Pointer without ownership | `sk_surface_t* s = sk_canvas_get_surface(c);` |
| **C#** | `OwnsHandle = false` | `var s = canvas.Surface; // non-owning wrapper` |

**C# Implementation:**
```csharp
public SKSurface Surface
{
    get {
        var handle = SkiaApi.sk_canvas_get_surface(Handle);
        return GetOrAddObject(handle, owns: false, (h, o) => new SKSurface(h, o));
    }
}
```

## Common API Patterns

### Pattern 1: Simple Method Call

```cpp
// C++
void SkCanvas::clear(SkColor color);
```

```cpp
// C API
SK_C_API void sk_canvas_clear(sk_canvas_t* canvas, sk_color_t color);

void sk_canvas_clear(sk_canvas_t* canvas, sk_color_t color) {
    AsCanvas(canvas)->clear(color);
}
```

```csharp
// C# P/Invoke
[DllImport("libSkiaSharp")]
public static extern void sk_canvas_clear(sk_canvas_t canvas, uint color);

// C# Wrapper
public void Clear(SKColor color)
{
    SkiaApi.sk_canvas_clear(Handle, (uint)color);
}
```

### Pattern 2: Property Get

```cpp
// C++
int SkImage::width() const;
```

```cpp
// C API
SK_C_API int sk_image_get_width(const sk_image_t* image);

int sk_image_get_width(const sk_image_t* image) {
    return AsImage(image)->width();
}
```

```csharp
// C# P/Invoke
[DllImport("libSkiaSharp")]
public static extern int sk_image_get_width(sk_image_t image);

// C# Wrapper
public int Width => SkiaApi.sk_image_get_width(Handle);
```

### Pattern 3: Property Set

```cpp
// C++
void SkPaint::setColor(SkColor color);
```

```cpp
// C API
SK_C_API void sk_paint_set_color(sk_paint_t* paint, sk_color_t color);

void sk_paint_set_color(sk_paint_t* paint, sk_color_t color) {
    AsPaint(paint)->setColor(color);
}
```

```csharp
// C# P/Invoke
[DllImport("libSkiaSharp")]
public static extern void sk_paint_set_color(sk_paint_t paint, uint color);

// C# Wrapper
public SKColor Color
{
    get => (SKColor)SkiaApi.sk_paint_get_color(Handle);
    set => SkiaApi.sk_paint_set_color(Handle, (uint)value);
}
```

### Pattern 4: Factory Method (Owned)

```cpp
// C++
SkCanvas* SkCanvas::MakeRasterDirect(const SkImageInfo& info, void* pixels, size_t rowBytes);
```

```cpp
// C API
SK_C_API sk_canvas_t* sk_canvas_new_from_raster(
    const sk_imageinfo_t* info, void* pixels, size_t rowBytes);

sk_canvas_t* sk_canvas_new_from_raster(
    const sk_imageinfo_t* info, void* pixels, size_t rowBytes)
{
    return ToCanvas(SkCanvas::MakeRasterDirect(AsImageInfo(info), pixels, rowBytes).release());
}
```

```csharp
// C# P/Invoke
[DllImport("libSkiaSharp")]
public static extern sk_canvas_t sk_canvas_new_from_raster(
    sk_imageinfo_t* info, IntPtr pixels, IntPtr rowBytes);

// C# Wrapper
public static SKCanvas Create(SKImageInfo info, IntPtr pixels, int rowBytes)
{
    var nInfo = SKImageInfoNative.FromManaged(ref info);
    var handle = SkiaApi.sk_canvas_new_from_raster(&nInfo, pixels, (IntPtr)rowBytes);
    if (handle == IntPtr.Zero)
        throw new InvalidOperationException("Failed to create canvas");
    return new SKCanvas(handle, true);
}
```

### Pattern 5: Factory Method (Reference-Counted)

```cpp
// C++
sk_sp<SkImage> SkImages::RasterFromBitmap(const SkBitmap& bitmap);
```

```cpp
// C API
SK_C_API sk_image_t* sk_image_new_from_bitmap(const sk_bitmap_t* bitmap);

sk_image_t* sk_image_new_from_bitmap(const sk_bitmap_t* bitmap) {
    return ToImage(SkImages::RasterFromBitmap(*AsBitmap(bitmap)).release());
}
```

```csharp
// C# P/Invoke
[DllImport("libSkiaSharp")]
public static extern sk_image_t sk_image_new_from_bitmap(sk_bitmap_t bitmap);

// C# Wrapper
public static SKImage FromBitmap(SKBitmap bitmap)
{
    if (bitmap == null)
        throw new ArgumentNullException(nameof(bitmap));
    
    var handle = SkiaApi.sk_image_new_from_bitmap(bitmap.Handle);
    if (handle == IntPtr.Zero)
        throw new InvalidOperationException("Failed to create image");
    
    return GetObject(handle);  // ISKReferenceCounted, ref count = 1
}
```

### Pattern 6: Method with Struct Parameter

```cpp
// C++
void SkCanvas::drawRect(const SkRect& rect, const SkPaint& paint);
```

```cpp
// C API
SK_C_API void sk_canvas_draw_rect(
    sk_canvas_t* canvas, const sk_rect_t* rect, const sk_paint_t* paint);

void sk_canvas_draw_rect(sk_canvas_t* canvas, const sk_rect_t* rect, const sk_paint_t* paint) {
    AsCanvas(canvas)->drawRect(*AsRect(rect), *AsPaint(paint));
}
```

```csharp
// C# P/Invoke
[DllImport("libSkiaSharp")]
public static extern void sk_canvas_draw_rect(
    sk_canvas_t canvas, sk_rect_t* rect, sk_paint_t paint);

// C# Wrapper
public unsafe void DrawRect(SKRect rect, SKPaint paint)
{
    if (paint == null)
        throw new ArgumentNullException(nameof(paint));
    
    SkiaApi.sk_canvas_draw_rect(Handle, &rect, paint.Handle);
}
```

### Pattern 7: Output Parameter

```cpp
// C++
bool SkCanvas::getLocalClipBounds(SkRect* bounds) const;
```

```cpp
// C API
SK_C_API bool sk_canvas_get_local_clip_bounds(sk_canvas_t* canvas, sk_rect_t* bounds);

bool sk_canvas_get_local_clip_bounds(sk_canvas_t* canvas, sk_rect_t* bounds) {
    return AsCanvas(canvas)->getLocalClipBounds(AsRect(bounds));
}
```

```csharp
// C# P/Invoke
[DllImport("libSkiaSharp")]
[return: MarshalAs(UnmanagedType.I1)]
public static extern bool sk_canvas_get_local_clip_bounds(
    sk_canvas_t canvas, sk_rect_t* bounds);

// C# Wrapper
public unsafe bool TryGetLocalClipBounds(out SKRect bounds)
{
    fixed (SKRect* b = &bounds)
    {
        return SkiaApi.sk_canvas_get_local_clip_bounds(Handle, b);
    }
}

public SKRect LocalClipBounds
{
    get {
        TryGetLocalClipBounds(out var bounds);
        return bounds;
    }
}
```

## Enum Mapping

Enums typically map 1:1 across all layers:

| C++ Enum | C API Enum | C# Enum |
|----------|------------|---------|
| `SkCanvas::PointMode` | `sk_point_mode_t` | `SKPointMode` |
| `SkBlendMode` | `sk_blendmode_t` | `SKBlendMode` |
| `SkColorType` | `sk_colortype_t` | `SKColorType` |

```cpp
// C++
enum class SkBlendMode {
    kClear,
    kSrc,
    kDst,
    // ...
};
```

```cpp
// C API
typedef enum {
    SK_BLENDMODE_CLEAR = 0,
    SK_BLENDMODE_SRC = 1,
    SK_BLENDMODE_DST = 2,
    // ...
} sk_blendmode_t;
```

```csharp
// C#
public enum SKBlendMode
{
    Clear = 0,
    Src = 1,
    Dst = 2,
    // ...
}
```

**Cast pattern in C API:**
```cpp
void sk_canvas_draw_color(sk_canvas_t* canvas, sk_color_t color, sk_blendmode_t mode) {
    AsCanvas(canvas)->drawColor(color, (SkBlendMode)mode);
}
```

## Struct Mapping

Value structs also map across layers:

```cpp
// C++
struct SkRect {
    float fLeft, fTop, fRight, fBottom;
};
```

```cpp
// C API
typedef struct {
    float left, top, right, bottom;
} sk_rect_t;
```

```csharp
// C#
[StructLayout(LayoutKind.Sequential)]
public struct SKRect
{
    public float Left;
    public float Top;
    public float Right;
    public float Bottom;
    
    // Plus constructors, properties, methods
}
```

## Quick Reference: Type Categories

### Pointer Types (Objects)

| Category | C++ | C API | C# | Examples |
|----------|-----|-------|-----|----------|
| **Owned** | Class with new/delete | `_new()/_delete()` | `SKObject`, owns handle | SKCanvas, SKPaint, SKPath |
| **Ref-Counted** | `sk_sp<T>`, `SkRefCnt` | `_ref()/_unref()` | `ISKReferenceCounted` | SKImage, SKShader, SKData |
| **Non-Owning** | Raw pointer | Pointer | `OwnsHandle=false` | Canvas.Surface getter |

### Value Types

| Category | C++ | C API | C# | Examples |
|----------|-----|-------|-----|----------|
| **Struct** | `struct` | `typedef struct` | `[StructLayout] struct` | SKRect, SKPoint, SKMatrix |
| **Primitive** | `int`, `float`, `bool` | `int`, `float`, `bool` | `int`, `float`, `bool` | Coordinates, sizes |
| **Enum** | `enum class` | `typedef enum` | `enum` | SKBlendMode, SKColorType |
| **Color** | `SkColor` (uint32_t) | `sk_color_t` (uint32_t) | `SKColor` (uint) | Color values |

## Summary

This layer mapping reference provides a quick lookup for:
- Type naming conventions across layers
- Function naming patterns
- File organization
- Type conversion macros
- Parameter passing patterns
- Memory management patterns
- Common API patterns

For deeper understanding:
- [Architecture Overview](architecture-overview.md) - High-level architecture
- [Memory Management](memory-management.md) - Pointer types and ownership
- [Error Handling](error-handling.md) - Error propagation patterns
- [Adding New APIs](adding-new-apis.md) - Step-by-step guide
