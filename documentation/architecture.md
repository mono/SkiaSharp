# SkiaSharp Architecture

This document explains SkiaSharp's three-layer design and how data flows between C++, C, and C#.

## Three-Layer Architecture

```
C# Wrapper Layer (binding/SkiaSharp/)
    ↓ P/Invoke
C API Layer (externals/skia/src/c/)
    ↓ Type casting
C++ Skia Library (externals/skia/)
```

**Why this design?**
- C++ exceptions cannot cross P/Invoke boundaries
- C APIs are ABI-stable
- C# provides safety, validation, and idiomatic .NET patterns

## Key Design Principles

1. **Handle-based:** Native objects are `IntPtr` handles in C#
2. **Object identity:** Global `HandleDictionary` ensures one C# wrapper per native handle
3. **Memory safety:** `IDisposable` for cleanup, finalizers as backup
4. **Exception boundaries:** C API never throws; C# validates and throws

## Call Flow Example

```csharp
// C# - validates then calls P/Invoke
public void DrawRect(SKRect rect, SKPaint paint) {
    if (paint == null)
        throw new ArgumentNullException(nameof(paint));
    SkiaApi.sk_canvas_draw_rect(Handle, &rect, paint.Handle);
}
```

```cpp
// C API - minimal wrapper, trusts C# validation
void sk_canvas_draw_rect(sk_canvas_t* canvas, const sk_rect_t* rect, const sk_paint_t* paint) {
    AsCanvas(canvas)->drawRect(*AsRect(rect), *AsPaint(paint));
}
```

## Type Mappings

| C++ | C API | C# P/Invoke | C# Wrapper |
|-----|-------|-------------|------------|
| `SkCanvas*` | `sk_canvas_t*` | `IntPtr` | `SKCanvas` |
| `SkPaint*` | `sk_paint_t*` | `IntPtr` | `SKPaint` |
| `sk_sp<SkImage>` | `sk_image_t*` | `IntPtr` | `SKImage` |
| `SkRect` | `sk_rect_t` | `SKRect` | `SKRect` |
| `SkColor` | `sk_color_t` | `uint` | `SKColor` |

**Naming pattern:** `SkType` → `sk_type_t` → `SKType`

## Function Naming

| C++ | C API | C# |
|-----|-------|-----|
| `SkCanvas::drawRect()` | `sk_canvas_draw_rect()` | `SKCanvas.DrawRect()` |
| `SkPaint::getColor()` | `sk_paint_get_color()` | `SKPaint.Color` (get) |
| `SkPaint::setColor()` | `sk_paint_set_color()` | `SKPaint.Color` (set) |

## Type Conversion Macros (C API)

```cpp
// In sk_types_priv.h - converts between C and C++ types
AsCanvas(sk_canvas_t*) → SkCanvas*
ToCanvas(SkCanvas*)    → sk_canvas_t*
AsPaint(sk_paint_t*)   → SkPaint*
AsRect(sk_rect_t*)     → SkRect*
```

## File Organization

| Layer | Files |
|-------|-------|
| C++ API | `externals/skia/include/core/SkCanvas.h` |
| C API header | `externals/skia/include/c/sk_canvas.h` |
| C API impl | `externals/skia/src/c/sk_canvas.cpp` |
| P/Invoke | `binding/SkiaSharp/SkiaApi.generated.cs` |
| C# wrapper | `binding/SkiaSharp/SKCanvas.cs` |

## Threading Model

| Object Type | Thread-Safe? | Notes |
|-------------|--------------|-------|
| SKCanvas, SKPaint, SKPath | ❌ No | Keep thread-local |
| SKImage, SKShader, SKTypeface | ✅ Yes | Immutable, can share |
| Object creation | ✅ Yes | Creating different objects on different threads is safe |

**Rule:** Don't use the same mutable object (Canvas/Paint/Path) from multiple threads.

## Code Generation

- **C API layer:** Hand-written (`externals/skia/src/c/*.cpp`)
- **P/Invoke:** Auto-generated from C headers (`SkiaApi.generated.cs`)
- **C# wrappers:** Hand-written (`binding/SkiaSharp/SK*.cs`)

```bash
dotnet run --project utils/SkiaSharpGenerator/SkiaSharpGenerator.csproj -- generate
```
