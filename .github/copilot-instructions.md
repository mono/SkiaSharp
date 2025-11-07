# GitHub Copilot Instructions for SkiaSharp

This file provides context and guidelines for AI assistants (like GitHub Copilot) working on the SkiaSharp codebase. It supplements the detailed documentation in the `design/` folder.

## Project Overview

SkiaSharp is a cross-platform 2D graphics API for .NET that wraps Google's Skia Graphics Library using a three-layer architecture:

1. **C++ Skia Layer** (`externals/skia/`) - Native Skia graphics engine
2. **C API Layer** (`externals/skia/include/c/`, `externals/skia/src/c/`) - C wrapper for P/Invoke
3. **C# Wrapper Layer** (`binding/SkiaSharp/`) - Managed .NET API

**Key Principle:** C++ exceptions cannot cross the C API boundary. All error handling must use return values, not exceptions.

## Architecture Quick Reference

### Three-Layer Call Flow

```
C# Application
    ↓ (calls method)
SKCanvas.DrawRect() in binding/SkiaSharp/SKCanvas.cs
    ↓ (validates parameters, P/Invoke)
sk_canvas_draw_rect() in externals/skia/src/c/sk_canvas.cpp
    ↓ (type conversion, AsCanvas macro)
SkCanvas::drawRect() in native Skia C++ code
    ↓ (renders)
Native Graphics
```

### File Locations by Layer

| What | C++ | C API | C# |
|------|-----|-------|-----|
| **Headers** | `externals/skia/include/core/*.h` | `externals/skia/include/c/*.h` | `binding/SkiaSharp/SkiaApi.cs` |
| **Implementation** | `externals/skia/src/` | `externals/skia/src/c/*.cpp` | `binding/SkiaSharp/*.cs` |
| **Example** | `SkCanvas.h` | `sk_canvas.h`, `sk_canvas.cpp` | `SKCanvas.cs`, `SkiaApi.cs` |

## Critical Memory Management Rules

### Three Pointer Type Categories

SkiaSharp uses three distinct pointer/ownership patterns. **You must identify which type before adding or modifying APIs.**

#### 1. Raw Pointers (Non-Owning)
- **C++:** `SkType*` or `const SkType&` parameters/returns from getters
- **C API:** `sk_type_t*` passed or returned, no create/destroy functions
- **C#:** `OwnsHandle = false`, often in `OwnedObjects` collection
- **Cleanup:** None (owned elsewhere)
- **Examples:** Parameters to draw methods, `Canvas.Surface` getter

```csharp
// Non-owning example
public SKSurface Surface {
    get {
        var handle = SkiaApi.sk_canvas_get_surface(Handle);
        return GetOrAddObject(handle, owns: false, (h, o) => new SKSurface(h, o));
    }
}
```

#### 2. Owned Pointers (Unique Ownership)
- **C++:** Mutable classes, `new`/`delete`, or `std::unique_ptr`
- **C API:** `sk_type_new()`/`sk_type_delete()` or `sk_type_destroy()`
- **C#:** `SKObject` with `OwnsHandle = true`, calls delete in `DisposeNative()`
- **Cleanup:** `delete` or `destroy` function
- **Examples:** `SKCanvas`, `SKPaint`, `SKPath`, `SKBitmap`

```csharp
// Owned pointer example
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

#### 3. Reference-Counted Pointers (Shared Ownership)

Skia has **two variants** of reference counting:

**Variant A: Virtual Reference Counting (`SkRefCnt`)**
- **C++:** Inherits `SkRefCnt`, has virtual destructor
- **C API:** `sk_type_ref()`/`sk_type_unref()` or `sk_refcnt_safe_ref()`
- **C#:** `SKObject` implements `ISKReferenceCounted`, calls `unref` in `DisposeNative()`
- **Cleanup:** `unref()` (automatic via `ISKReferenceCounted`)
- **Examples:** `SKImage`, `SKShader`, `SKColorFilter`, `SKImageFilter`, `SKTypeface`, `SKSurface`

**Variant B: Non-Virtual Reference Counting (`SkNVRefCnt<T>`)**
- **C++:** Inherits `SkNVRefCnt<T>` template, no virtual destructor (lighter weight)
- **C API:** Type-specific functions like `sk_data_ref()`/`sk_data_unref()`
- **C#:** `SKObject` implements `ISKNonVirtualReferenceCounted`, calls type-specific unref
- **Cleanup:** Type-specific `unref()` (automatic via interface)
- **Examples:** `SKData`, `SKTextBlob`, `SKVertices`, `SKColorSpace`

```csharp
// Virtual ref-counted example (most common)
public class SKImage : SKObject, ISKReferenceCounted
{
    public static SKImage FromBitmap(SKBitmap bitmap)
    {
        var handle = SkiaApi.sk_image_new_from_bitmap(bitmap.Handle);
        return GetObject(handle);  // Ref count = 1, will unref on dispose
    }
}

// Non-virtual ref-counted example (lighter weight)
public class SKData : SKObject, ISKNonVirtualReferenceCounted
{
    void ISKNonVirtualReferenceCounted.ReferenceNative() => SkiaApi.sk_data_ref(Handle);
    void ISKNonVirtualReferenceCounted.UnreferenceNative() => SkiaApi.sk_data_unref(Handle);
}
```

**Why two variants:**
- `SkRefCnt`: Most types, supports inheritance/polymorphism (8-16 byte overhead)
- `SkNVRefCnt`: Performance-critical types, no inheritance (4 byte overhead)

### How to Identify Pointer Type

**Check the C++ API:**
1. **Inherits `SkRefCnt` or `SkRefCntBase`?** → Virtual reference-counted
2. **Inherits `SkNVRefCnt<T>`?** → Non-virtual reference-counted  
3. **Returns `sk_sp<T>`?** → Reference-counted (either variant)
4. **Mutable class (Canvas, Paint, Path)?** → Owned pointer
5. **Parameter or getter return?** → Raw pointer (non-owning)

**In C API layer:**
- Type-specific `sk_data_ref()`/`sk_data_unref()` exist? → Non-virtual ref-counted
- Generic `sk_type_ref()`/`sk_type_unref()` or `sk_refcnt_safe_ref()`? → Virtual ref-counted
- `sk_type_new()` and `sk_type_delete()`? → Owned
- Neither? → Raw pointer

**In C# layer:**
- Implements `ISKNonVirtualReferenceCounted`? → Non-virtual ref-counted
- Implements `ISKReferenceCounted`? → Virtual ref-counted
- Has `DisposeNative()` calling `delete` or `destroy`? → Owned
- Created with `owns: false`? → Raw pointer

## Error Handling Rules

### C API Layer (Exception Firewall)

**Never let exceptions cross the C API boundary:**

```cpp
// ❌ WRONG - Exception will crash
SK_C_API void sk_function() {
    throw std::exception();  // NEVER DO THIS
}

// ✅ CORRECT - Catch and convert to error code
SK_C_API bool sk_function() {
    try {
        // C++ code that might throw
        return true;
    } catch (...) {
        return false;  // Convert to bool/null/error code
    }
}
```

**Error signaling patterns:**
- Return `bool` for success/failure
- Return `nullptr` for factory failures
- Use out parameters for detailed error codes
- Add defensive null checks

### C# Layer (Validation)

**Validate before calling native code:**

```csharp
public void DrawRect(SKRect rect, SKPaint paint)
{
    // 1. Validate parameters
    if (paint == null)
        throw new ArgumentNullException(nameof(paint));
    
    // 2. Check object state
    if (Handle == IntPtr.Zero)
        throw new ObjectDisposedException("SKCanvas");
    
    // 3. Call native (safe, validated)
    SkiaApi.sk_canvas_draw_rect(Handle, &rect, paint.Handle);
}

// For factory methods, check return values
public static SKImage FromData(SKData data)
{
    if (data == null)
        throw new ArgumentNullException(nameof(data));
    
    var handle = SkiaApi.sk_image_new_from_encoded(data.Handle);
    
    if (handle == IntPtr.Zero)
        throw new InvalidOperationException("Failed to decode image");
    
    return GetObject(handle);
}
```

## Common Patterns and Examples

### Pattern 1: Adding a Drawing Method

```cpp
// C API (externals/skia/src/c/sk_canvas.cpp)
void sk_canvas_draw_rect(sk_canvas_t* canvas, const sk_rect_t* rect, const sk_paint_t* paint) {
    if (!canvas || !rect || !paint)  // Defensive null checks
        return;
    AsCanvas(canvas)->drawRect(*AsRect(rect), *AsPaint(paint));
}
```

```csharp
// C# (binding/SkiaSharp/SKCanvas.cs)
public unsafe void DrawRect(SKRect rect, SKPaint paint)
{
    if (paint == null)
        throw new ArgumentNullException(nameof(paint));
    SkiaApi.sk_canvas_draw_rect(Handle, &rect, paint.Handle);
}
```

### Pattern 2: Property with Get/Set

```cpp
// C API
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

### Pattern 3: Factory Returning Reference-Counted Object

```cpp
// C API - Notice sk_ref_sp() for ref-counted parameter
sk_image_t* sk_image_new_from_encoded(const sk_data_t* data) {
    try {
        // sk_ref_sp increments ref count when creating sk_sp
        auto image = SkImages::DeferredFromEncodedData(sk_ref_sp(AsData(data)));
        return ToImage(image.release());  // .release() returns pointer, ref count = 1
    } catch (...) {
        return nullptr;
    }
}
```

```csharp
// C# - GetObject() for reference-counted objects
public static SKImage FromEncodedData(SKData data)
{
    if (data == null)
        throw new ArgumentNullException(nameof(data));
    
    var handle = SkiaApi.sk_image_new_from_encoded(data.Handle);
    if (handle == IntPtr.Zero)
        throw new InvalidOperationException("Failed to decode image");
    
    return GetObject(handle);  // For ISKReferenceCounted types
}
```

### Pattern 4: Taking Reference-Counted Parameter

```cpp
// When C++ expects sk_sp<T>, use sk_ref_sp() to increment ref count
SK_C_API sk_image_t* sk_image_apply_filter(const sk_image_t* image, const sk_imagefilter_t* filter) {
    // filter is ref-counted, C++ wants sk_sp - use sk_ref_sp to increment ref
    return ToImage(AsImage(image)->makeWithFilter(
        sk_ref_sp(AsImageFilter(filter))).release());
}
```

## Type Conversion Reference

### C API Type Conversion Macros

Located in `externals/skia/src/c/sk_types_priv.h`:

```cpp
AsCanvas(sk_canvas_t*)   → SkCanvas*      // C to C++
ToCanvas(SkCanvas*)      → sk_canvas_t*   // C++ to C
AsPaint(sk_paint_t*)     → SkPaint*
ToPaint(SkPaint*)        → sk_paint_t*
AsImage(sk_image_t*)     → SkImage*
ToImage(SkImage*)        → sk_image_t*
AsRect(sk_rect_t*)       → SkRect*
// ... and many more
```

**Usage:**
- `AsXxx()`: Converting from C API type to C++ type (reading parameter)
- `ToXxx()`: Converting from C++ type to C API type (returning value)
- Dereference with `*` to convert pointer to reference: `*AsRect(rect)`

### C# Type Aliases

In `binding/SkiaSharp/SkiaApi.generated.cs`:

```csharp
using sk_canvas_t = System.IntPtr;
using sk_paint_t = System.IntPtr;
using sk_image_t = System.IntPtr;
// All opaque pointer types are IntPtr in C#
```

## Naming Conventions

### Across Layers

| C++ | C API | C# |
|-----|-------|-----|
| `SkCanvas` | `sk_canvas_t*` | `SKCanvas` |
| `SkCanvas::drawRect()` | `sk_canvas_draw_rect()` | `SKCanvas.DrawRect()` |
| `SkPaint::getColor()` | `sk_paint_get_color()` | `SKPaint.Color` (property) |
| `SkImage::width()` | `sk_image_get_width()` | `SKImage.Width` (property) |

### Function Naming

**C API pattern:** `sk_<type>_<action>[_<details>]`

Examples:
- `sk_canvas_draw_rect` - Draw method
- `sk_paint_get_color` - Getter
- `sk_paint_set_color` - Setter  
- `sk_image_new_from_bitmap` - Factory
- `sk_canvas_save_layer` - Method with detail

**C# conventions:**
- PascalCase for methods and properties
- Use properties instead of get/set methods
- Add convenience overloads
- Use XML documentation comments

## Code Generation

SkiaSharp has both hand-written and generated code:

### Hand-Written
- C API layer: All `.cpp` files in `externals/skia/src/c/`
- C# wrappers: Logic in `binding/SkiaSharp/*.cs`
- Some P/Invoke: `binding/SkiaSharp/SkiaApi.cs`

### Generated
- P/Invoke declarations: `binding/SkiaSharp/SkiaApi.generated.cs`
- Generator: `utils/SkiaSharpGenerator/`

**Don't manually edit generated files.** Regenerate with:
```bash
dotnet run --project utils/SkiaSharpGenerator/SkiaSharpGenerator.csproj -- generate
```

## Threading Considerations

**Skia is NOT thread-safe:**
- Most objects should only be accessed from one thread
- Canvas operations must be single-threaded
- Immutable objects (SKImage) can be shared after creation
- Reference counting is atomic (thread-safe)
- Handle dictionary uses ConcurrentDictionary

**In code:**
- Don't add locks to wrapper code
- Document thread-safety requirements
- Let users handle synchronization

## Common Mistakes to Avoid

### ❌ Wrong Pointer Type
```csharp
// WRONG - SKImage is reference-counted, not owned
public class SKImage : SKObject  // Missing ISKReferenceCounted
{
    protected override void DisposeNative()
    {
        SkiaApi.sk_image_delete(Handle);  // Should be unref, not delete!
    }
}
```

### ❌ Not Incrementing Ref Count
```cpp
// WRONG - C++ expects sk_sp but we're not incrementing ref count
sk_image_t* sk_image_apply_filter(const sk_image_t* image, const sk_imagefilter_t* filter) {
    return ToImage(AsImage(image)->makeWithFilter(
        AsImageFilter(filter)).release());  // Missing sk_ref_sp!
}
```

### ❌ Exception Crossing Boundary
```cpp
// WRONG - Exception will crash
SK_C_API sk_image_t* sk_image_from_data(sk_data_t* data) {
    auto image = SkImages::DeferredFromEncodedData(sk_ref_sp(AsData(data)));
    if (!image)
        throw std::runtime_error("Failed");  // DON'T THROW!
    return ToImage(image.release());
}
```

### ❌ Disposing Borrowed Objects
```csharp
// WRONG - Surface is owned by canvas, not by this wrapper
public SKSurface Surface {
    get {
        var handle = SkiaApi.sk_canvas_get_surface(Handle);
        return new SKSurface(handle, true);  // Should be owns: false!
    }
}
```

### ❌ Missing Parameter Validation
```csharp
// WRONG - No validation before P/Invoke
public void DrawRect(SKRect rect, SKPaint paint)
{
    SkiaApi.sk_canvas_draw_rect(Handle, &rect, paint.Handle);
    // What if paint is null? What if this object is disposed?
}
```

## Checklist for AI-Assisted Changes

When adding or modifying APIs:

### Analysis
- [ ] Located C++ API in Skia headers
- [ ] Identified pointer type (raw/owned/ref-counted)
- [ ] Checked if operation can fail
- [ ] Verified parameter types

### C API Layer
- [ ] Added defensive null checks
- [ ] Used correct conversion macros (AsXxx/ToXxx)
- [ ] Handled reference counting correctly (sk_ref_sp when needed)
- [ ] Caught exceptions (if operation can throw)
- [ ] Returned appropriate error signal (bool/null/code)

### C# Layer  
- [ ] Validated parameters before P/Invoke
- [ ] Checked return values
- [ ] Used correct wrapper pattern (owned vs ref-counted)
- [ ] Applied correct marshaling (bool → UnmanagedType.I1)
- [ ] Added XML documentation
- [ ] Threw appropriate exceptions

## Quick Decision Trees

### "What wrapper pattern should I use?"

```
Does C++ type inherit SkRefCnt or SkRefCntBase?
├─ Yes → Use ISKReferenceCounted (virtual ref-counting)
└─ No → Does C++ type inherit SkNVRefCnt<T>?
    ├─ Yes → Use ISKNonVirtualReferenceCounted (non-virtual ref-counting)
    └─ No → Is it mutable (Canvas, Paint, Path)?
        ├─ Yes → Use owned pattern (DisposeNative calls delete/destroy)
        └─ No → Is it returned from a getter?
            ├─ Yes → Use non-owning pattern (owns: false)
            └─ No → Default to owned pattern
```

### "How should I handle errors?"

```
Where am I working?
├─ C API layer → Catch exceptions, return bool/null
├─ C# wrapper → Validate parameters, check return values, throw exceptions
└─ C++ layer → Use normal C++ error handling
```

### "How do I pass a ref-counted parameter?"

```
Is the C++ parameter sk_sp<T>?
├─ Yes → Use sk_ref_sp() in C API to increment ref count
└─ No (const SkType* or SkType*) → Use AsType() without sk_ref_sp
```

## Documentation References

For detailed information, see `design/` folder:

- **[architecture-overview.md](design/architecture-overview.md)** - Three-layer architecture, call flow, design principles
- **[memory-management.md](design/memory-management.md)** - Pointer types, ownership, lifecycle patterns, examples
- **[error-handling.md](design/error-handling.md)** - Error propagation, exception boundaries, patterns
- **[adding-new-apis.md](design/adding-new-apis.md)** - Step-by-step guide with complete examples
- **[layer-mapping.md](design/layer-mapping.md)** - Type mappings, naming conventions, quick reference

## Example Workflows

### Adding a New Drawing Method

1. Find C++ API: `void SkCanvas::drawArc(const SkRect& oval, float start, float sweep, bool useCenter, const SkPaint& paint)`
2. Identify types: Canvas (owned), Rect (value), Paint (borrowed), primitives
3. Add C API in `sk_canvas.cpp`:
   ```cpp
   void sk_canvas_draw_arc(sk_canvas_t* c, const sk_rect_t* oval, 
                           float start, float sweep, bool useCenter, const sk_paint_t* paint) {
       if (!c || !oval || !paint) return;
       AsCanvas(c)->drawArc(*AsRect(oval), start, sweep, useCenter, *AsPaint(paint));
   }
   ```
4. Add P/Invoke in `SkiaApi.cs`:
   ```csharp
   [DllImport("libSkiaSharp")]
   public static extern void sk_canvas_draw_arc(sk_canvas_t canvas, sk_rect_t* oval,
       float start, float sweep, [MarshalAs(UnmanagedType.I1)] bool useCenter, sk_paint_t paint);
   ```
5. Add wrapper in `SKCanvas.cs`:
   ```csharp
   public unsafe void DrawArc(SKRect oval, float startAngle, float sweepAngle, 
                               bool useCenter, SKPaint paint)
   {
       if (paint == null)
           throw new ArgumentNullException(nameof(paint));
       SkiaApi.sk_canvas_draw_arc(Handle, &oval, startAngle, sweepAngle, useCenter, paint.Handle);
   }
   ```

### Adding a Factory Method for Reference-Counted Object

1. Find C++ API: `sk_sp<SkImage> SkImages::DeferredFromEncodedData(sk_sp<SkData> data)`
2. Identify: Returns ref-counted (sk_sp), takes ref-counted parameter
3. Add C API:
   ```cpp
   sk_image_t* sk_image_new_from_encoded(const sk_data_t* data) {
       try {
           // sk_ref_sp increments ref count on data
           auto image = SkImages::DeferredFromEncodedData(sk_ref_sp(AsData(data)));
           return ToImage(image.release());  // Ref count = 1
       } catch (...) {
           return nullptr;
       }
   }
   ```
4. Add C# wrapper:
   ```csharp
   public static SKImage FromEncodedData(SKData data)
   {
       if (data == null)
           throw new ArgumentNullException(nameof(data));
       
       var handle = SkiaApi.sk_image_new_from_encoded(data.Handle);
       if (handle == IntPtr.Zero)
           throw new InvalidOperationException("Failed to decode image");
       
       return GetObject(handle);  // For ISKReferenceCounted
   }
   ```

## Summary

Key concepts for working with SkiaSharp:

1. **Three-layer architecture** - C++ → C API → C#
2. **Three pointer types** - Raw (non-owning), Owned, Reference-counted
3. **Exception firewall** - C API never throws, converts to error codes
4. **Reference counting** - Use `sk_ref_sp()` when C++ expects `sk_sp<T>`
5. **Validation** - C# validates before calling native code
6. **Naming** - `SkType` → `sk_type_t*` → `SKType`

When in doubt, find a similar existing API and follow its pattern. The codebase is consistent in its approaches.
