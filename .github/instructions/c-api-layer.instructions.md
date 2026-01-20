---
applyTo: "externals/skia/include/c/**/*.h,externals/skia/src/c/**/*.cpp"
---

# C API Layer Instructions

You are working in the C API layer that bridges Skia C++ to managed C#.

> **📚 Documentation:**
> - **Architecture:** [documentation/architecture.md](../../documentation/architecture.md)
> - **Memory Management:** [documentation/memory-management.md](../../documentation/memory-management.md)
> - **Error Handling:** [documentation/error-handling.md](../../documentation/error-handling.md)

## Critical Rules

- All functions must use C linkage: `SK_C_API` or `extern "C"`
- Use C-compatible types only (no C++ classes in signatures)
- **Trust C# to validate** - C API is a minimal wrapper
- **No exception handling needed** - Skia rarely throws, C# prevents invalid inputs
- **No parameter validation needed** - C# validates before calling
- Keep implementations simple and direct

## Pointer Type Handling

> **💡 See [documentation/memory-management.md](../../documentation/memory-management.md) for pointer type concepts.**
> Below are C API-specific patterns for each type.

### Raw Pointers (Non-Owning)
```cpp
// Just pass through, no ref counting
SK_C_API sk_canvas_t* sk_canvas_get_surface(sk_canvas_t* canvas);
```

### Owned Pointers
```cpp
// Create/destroy pairs
SK_C_API sk_paint_t* sk_paint_new(void);
SK_C_API void sk_paint_delete(sk_paint_t* paint);
```

### Reference-Counted Pointers
```cpp
// Explicit ref/unref functions
SK_C_API void sk_image_ref(const sk_image_t* image);
SK_C_API void sk_image_unref(const sk_image_t* image);

// When C++ expects sk_sp<T>, use sk_ref_sp to increment ref count
SK_C_API sk_image_t* sk_image_apply_filter(
    const sk_image_t* image,
    const sk_imagefilter_t* filter)
{
    return ToImage(AsImage(image)->makeWithFilter(
        sk_ref_sp(AsImageFilter(filter))).release());
}
```

## Naming Conventions

- **Functions:** `sk_<type>_<action>` (e.g., `sk_canvas_draw_rect`)
- **Types:** `sk_<type>_t` (e.g., `sk_canvas_t`)
- Keep names consistent with C++ equivalents

## Type Conversion

Use macros from `sk_types_priv.h`:
```cpp
AsCanvas(sk_canvas_t*) → SkCanvas*
ToCanvas(SkCanvas*) → sk_canvas_t*
```

Dereference pointers to get references:
```cpp
AsCanvas(canvas)->drawRect(*AsRect(rect), *AsPaint(paint));
```

## Memory Management

- Document ownership transfer in function comments
- Provide explicit create/destroy or ref/unref pairs
- Never assume caller will manage memory unless documented

## Error Handling Patterns (Actual Implementation)

### Boolean Return - Pass Through
```cpp
// C++ method returns bool, C API passes it through
SK_C_API bool sk_bitmap_try_alloc_pixels(sk_bitmap_t* bitmap, const sk_imageinfo_t* info) {
    return AsBitmap(bitmap)->tryAllocPixels(AsImageInfo(info));
}
```

**Note:** C# validates `bitmap` and `info` are non-null before calling.

### Null Return for Factory Failure
```cpp
// Returns nullptr if Skia factory fails
SK_C_API sk_surface_t* sk_surface_new_raster(const sk_imageinfo_t* info) {
    auto surface = SkSurfaces::Raster(AsImageInfo(info));
    return ToSurface(surface.release());
}
```

**Note:** C# checks for `IntPtr.Zero` and throws exception if null.

### Void Methods - Direct Call
```cpp
// Simple pass-through - C# ensures valid parameters
SK_C_API void sk_canvas_draw_rect(
    sk_canvas_t* canvas,
    const sk_rect_t* rect,
    const sk_paint_t* paint)
{
    AsCanvas(canvas)->drawRect(*AsRect(rect), *AsPaint(paint));
}
```

**Design:** C API trusts C# has validated all parameters.

## Common Patterns

### Simple Method Call
```cpp
SK_C_API void sk_canvas_clear(sk_canvas_t* canvas, sk_color_t color) {
    AsCanvas(canvas)->clear(color);
}
```

### Property Getter
```cpp
SK_C_API int sk_image_get_width(const sk_image_t* image) {
    return AsImage(image)->width();
}
```

### Property Setter
```cpp
SK_C_API void sk_paint_set_color(sk_paint_t* paint, sk_color_t color) {
    AsPaint(paint)->setColor(color);
}
```

## What NOT to Do

❌ **Never throw exceptions:**
```cpp
// WRONG
SK_C_API void sk_function() {
    throw std::exception();  // Will crash!
}
```

❌ **Don't use C++ types in signatures:**
```cpp
// WRONG
SK_C_API void sk_function(std::string name);

// CORRECT
SK_C_API void sk_function(const char* name);
```

❌ **Don't add unnecessary validation:**
```cpp
// WRONG - C# already validated
SK_C_API void sk_canvas_draw_rect(sk_canvas_t* canvas, const sk_rect_t* rect, const sk_paint_t* paint) {
    if (!canvas || !rect || !paint)  // Unnecessary - C# validated
        return;
    AsCanvas(canvas)->drawRect(*AsRect(rect), *AsPaint(paint));
}

// CORRECT - trust C# validation
SK_C_API void sk_canvas_draw_rect(sk_canvas_t* canvas, const sk_rect_t* rect, const sk_paint_t* paint) {
    AsCanvas(canvas)->drawRect(*AsRect(rect), *AsPaint(paint));
}
```

❌ **Don't add try-catch unless truly necessary:**
```cpp
// Usually NOT needed - Skia rarely throws, C# validates inputs
SK_C_API sk_image_t* sk_image_new_from_encoded(const sk_data_t* data) {
    return ToImage(SkImages::DeferredFromEncodedData(sk_ref_sp(AsData(data))).release());
}
```

**Current implementation philosophy:** Minimal C API layer, safety enforced in C#.

## Documentation

Document these in function comments:
- Ownership transfer (who owns returned pointers)
- Null parameter handling
- Error conditions
- Thread-safety implications
