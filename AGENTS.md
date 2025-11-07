# SkiaSharp - AGENTS.md

## Project Overview

SkiaSharp is a cross-platform 2D graphics API for .NET that wraps Google's Skia Graphics Library. It uses a three-layer architecture to bridge native C++ code with managed C#.

**Key principle:** C++ exceptions cannot cross the C API boundary - all error handling uses return values.

## Architecture

### Three-Layer Design
```
C# Wrapper Layer (binding/SkiaSharp/) 
    ↓ P/Invoke
C API Layer (externals/skia/include/c/, externals/skia/src/c/)
    ↓ Type casting
C++ Skia Library (externals/skia/)
```

**Call flow example:**
```
SKCanvas.DrawRect() → sk_canvas_draw_rect() → SkCanvas::drawRect()
```

## Critical Concepts

### Memory Management - Three Pointer Types

Understanding pointer types is **critical** for correct bindings:

1. **Raw Pointers (Non-Owning)** - `SkType*` or `const SkType&`
   - Parameters, temporary references, borrowed objects
   - No ownership transfer, no cleanup
   - C#: `OwnsHandle = false`

2. **Owned Pointers (Unique Ownership)** - Mutable objects, `new`/`delete`
   - Canvas, Paint, Path, Bitmap
   - One owner, caller deletes
   - C API: `sk_type_new()` / `sk_type_delete()`
   - C#: `SKObject` with `DisposeNative()` calling delete

3. **Reference-Counted Pointers (Shared Ownership)** - Two variants:
   - **Virtual** (`SkRefCnt`): Image, Shader, ColorFilter, Surface (most common)
   - **Non-Virtual** (`SkNVRefCnt<T>`): Data, TextBlob, Vertices, ColorSpace (lighter weight)
   - Both use `sk_sp<T>` and ref/unref pattern
   - C API: `sk_type_ref()` / `sk_type_unref()` or type-specific functions
   - C#: `ISKReferenceCounted` or `ISKNonVirtualReferenceCounted` interface

**Critical:** Getting pointer type wrong → memory leaks or crashes

**How to identify:**
- C++ inherits `SkRefCnt` or `SkNVRefCnt<T>`? → Reference-counted
- C++ is mutable (Canvas, Paint)? → Owned
- C++ is a parameter or getter return? → Raw (non-owning)

### Error Handling

**C API Layer** (exception firewall):
- Never throws exceptions
- Returns `bool` (success/failure), `null` (factory failure), or error codes
- Uses defensive null checks

**C# Layer** (validation):
- Validates parameters before P/Invoke
- Checks return values
- Throws appropriate C# exceptions (`ArgumentNullException`, `InvalidOperationException`, etc.)

## File Organization

### Naming Convention
```
C++: SkCanvas.h → C API: sk_canvas.h, sk_canvas.cpp → C#: SKCanvas.cs
Pattern: SkType → sk_type_t* → SKType
```

### Key Directories

**Do Not Modify:**
- `docs/` - Auto-generated API documentation

**Core Areas:**
- `externals/skia/include/c/` - C API headers
- `externals/skia/src/c/` - C API implementation
- `binding/SkiaSharp/` - C# wrappers and P/Invoke
- `design/` - Architecture documentation (comprehensive guides)

## Adding New APIs - Quick Steps

1. **Find C++ API** in `externals/skia/include/core/`
2. **Identify pointer type** (check if inherits `SkRefCnt`, mutable, or parameter)
3. **Add C API function** in `externals/skia/src/c/sk_*.cpp`
   ```cpp
   void sk_canvas_draw_rect(sk_canvas_t* c, const sk_rect_t* r, const sk_paint_t* p) {
       if (!c || !r || !p) return;  // Defensive checks
       AsCanvas(c)->drawRect(*AsRect(r), *AsPaint(p));
   }
   ```
4. **Add C API header** in `externals/skia/include/c/sk_*.h`
5. **Add P/Invoke** in `binding/SkiaSharp/SkiaApi.cs`
   ```csharp
   [DllImport("libSkiaSharp")]
   public static extern void sk_canvas_draw_rect(sk_canvas_t canvas, sk_rect_t* rect, sk_paint_t paint);
   ```
6. **Add C# wrapper** in `binding/SkiaSharp/SK*.cs`
   ```csharp
   public unsafe void DrawRect(SKRect rect, SKPaint paint) {
       if (paint == null) throw new ArgumentNullException(nameof(paint));
       SkiaApi.sk_canvas_draw_rect(Handle, &rect, paint.Handle);
   }
   ```

### Special Cases

**Reference-counted parameters** (C++ expects `sk_sp<T>`):
```cpp
// Use sk_ref_sp() to increment ref count
sk_image_t* sk_image_apply_filter(..., const sk_imagefilter_t* filter) {
    return ToImage(AsImage(image)->makeWithFilter(
        sk_ref_sp(AsImageFilter(filter))).release());
}
```

**Factory methods returning ref-counted objects**:
```csharp
// Use GetObject() for ISKReferenceCounted types
public static SKImage FromBitmap(SKBitmap bitmap) {
    var handle = SkiaApi.sk_image_new_from_bitmap(bitmap.Handle);
    if (handle == IntPtr.Zero)
        throw new InvalidOperationException("Failed to create image");
    return GetObject(handle);  // Handles ref counting
}
```

## Common Pitfalls

❌ **Wrong pointer type** → Use ref-counted wrapper for owned type
❌ **Missing ref count increment** → Use `sk_ref_sp()` when C++ expects `sk_sp<T>`
❌ **Disposing borrowed objects** → Use `owns: false` for non-owning references
❌ **Exception crossing C boundary** → Always catch in C API, return error code
❌ **Missing parameter validation** → Validate in C# before P/Invoke
❌ **Ignoring return values** → Check for null/false in C#

## Code Generation

- **Hand-written:** C API layer (all `.cpp` in `externals/skia/src/c/`)
- **Generated:** Some P/Invoke declarations (`SkiaApi.generated.cs`)
- **Tool:** `utils/SkiaSharpGenerator/`

To regenerate:
```bash
dotnet run --project utils/SkiaSharpGenerator/SkiaSharpGenerator.csproj -- generate
```

## Testing Checklist

- [ ] Pointer type correctly identified
- [ ] Memory properly managed (no leaks)
- [ ] Object disposes correctly
- [ ] Error cases handled (null params, failed operations)
- [ ] P/Invoke signature matches C API
- [ ] Parameters validated in C#
- [ ] Return values checked

## Threading

**⚠️ Skia is NOT thread-safe** - Most objects must be used from a single thread.

### Quick Thread-Safety Matrix

| Type | Thread-Safe? | Can Share? | Notes |
|------|--------------|------------|-------|
| **SKCanvas** | ❌ No | No | Single-threaded only |
| **SKPaint** | ❌ No | No | Each thread needs own instance |
| **SKPath** | ❌ No | No | Build on one thread |
| **SKImage** | ✅ Yes* | Yes | Read-only after creation |
| **SKShader** | ✅ Yes* | Yes | Immutable, shareable |
| **SKTypeface** | ✅ Yes* | Yes | Immutable, shareable |

*Read-only safe: Can be shared across threads once created, but creation should be single-threaded.

### Threading Rules

1. **✅ DO:** Keep mutable objects (Canvas, Paint, Path) thread-local
2. **✅ DO:** Share immutable objects (Image, Shader, Typeface) across threads
3. **✅ DO:** Create objects on background threads for offscreen rendering
4. **❌ DON'T:** Share SKCanvas across threads
5. **❌ DON'T:** Modify SKPaint while another thread uses it

### Safe Pattern: Background Rendering

```csharp
// ✅ Each thread has its own objects
var image = await Task.Run(() => {
    using var surface = SKSurface.Create(info);
    using var canvas = surface.Canvas;  // Thread-local
    using var paint = new SKPaint();    // Thread-local
    canvas.DrawCircle(100, 100, 50, paint);
    return surface.Snapshot();  // Immutable, safe to share
});
```

**Details:** See [architecture-overview.md - Threading](design/architecture-overview.md#threading-model-and-concurrency)

## Build Commands

```bash
# Build managed code only (after downloading native bits)
dotnet cake --target=libs

# Run tests
dotnet cake --target=tests

# Download pre-built native libraries
dotnet cake --target=externals-download
```

## Documentation

**Quick reference:** This file + code comments

**Practical tutorial:** [design/QUICKSTART.md](design/QUICKSTART.md) - 10-minute walkthrough

**Detailed guides** in `design/` folder:
- `QUICKSTART.md` - **Start here!** Practical end-to-end tutorial
- `architecture-overview.md` - Three-layer architecture, design principles
- `memory-management.md` - **Critical**: Pointer types, ownership, lifecycle
- `error-handling.md` - Error propagation patterns through layers
- `adding-new-apis.md` - Complete step-by-step guide with examples
- `layer-mapping.md` - Type mappings and naming conventions

**AI assistant context:** `.github/copilot-instructions.md`

## Quick Decision Trees

**"What wrapper pattern?"**
```
Inherits SkRefCnt? → ISKReferenceCounted
Mutable (Canvas/Paint)? → Owned (DisposeNative calls delete)
Getter/parameter? → Non-owning (owns: false)
```

**"How to handle errors?"**
```
C API → Catch exceptions, return bool/null
C# → Validate params, check returns, throw exceptions
```

**"Reference-counted parameter?"**
```
C++ wants sk_sp<T>? → Use sk_ref_sp() in C API
Otherwise → Use AsType() without sk_ref_sp
```

## Examples

### Simple Method (Owned Objects)
```cpp
// C API
void sk_canvas_clear(sk_canvas_t* canvas, sk_color_t color) {
    AsCanvas(canvas)->clear(color);
}
```
```csharp
// C#
public void Clear(SKColor color) {
    SkiaApi.sk_canvas_clear(Handle, (uint)color);
}
```

### Factory Method (Reference-Counted)
```cpp
// C API - sk_ref_sp increments ref for parameter
sk_image_t* sk_image_new_from_encoded(const sk_data_t* data) {
    return ToImage(SkImages::DeferredFromEncodedData(
        sk_ref_sp(AsData(data))).release());
}
```
```csharp
// C# - GetObject for ISKReferenceCounted
public static SKImage FromEncodedData(SKData data) {
    if (data == null)
        throw new ArgumentNullException(nameof(data));
    var handle = SkiaApi.sk_image_new_from_encoded(data.Handle);
    if (handle == IntPtr.Zero)
        throw new InvalidOperationException("Failed to decode");
    return GetObject(handle);
}
```

## When In Doubt

1. Find similar existing API and follow its pattern
2. Check `design/` documentation for detailed guidance
3. Verify pointer type carefully (most important!)
4. Test memory management thoroughly
5. Ensure error handling at all layers

---

**Remember:** Three layers, three pointer types, exception firewall at C API.
