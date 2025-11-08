# Error Handling in SkiaSharp

> **Quick Start:** For a practical tutorial, see [QUICKSTART.md](QUICKSTART.md)  
> **Quick Reference:** For a 2-minute overview, see [AGENTS.md](../AGENTS.md)

## TL;DR

**Safety boundary highlights:**

- **C++ Layer:** Native Skia code can throw; we do not try to surface those exceptions directly.
- **C API Layer:** Thin pass-through functions. They rarely guard inputs and never wrap calls in `try/catch`; they simply forward Skia's return values (void/bool/pointer).
- **C# Layer:** Performs targeted validation where it is required, but behaviour differs by API: constructors usually throw on failure, while many factory/utility methods return `null`, `false`, or default values instead of throwing.

**C# error patterns you will see:**
1. **Null parameter guards** – most methods throw `ArgumentNullException` before calling into native code, e.g. `SKCanvas.DrawRect` checks `paint`.
2. **Constructor validation** – constructors check if native handle creation succeeded and throw `InvalidOperationException` if Handle is IntPtr.Zero.
3. **Return value propagation** – factory methods such as `SKImage.FromEncodedData` simply return `null` and expect the caller to inspect the result.
4. **Try methods** – methods like `SKBitmap.TryAllocPixels` return `false` on failure rather than throwing.

**Key principle:** The managed layer is the safety boundary, but it mixes throwing and non-throwing patterns. Document both behaviours so callers know whether to check return values or catch exceptions.

**Representative code in the repo today:**
```csharp
public void DrawRect(SKRect rect, SKPaint paint)
{
    if (paint == null)
        throw new ArgumentNullException(nameof(paint));

    SkiaApi.sk_canvas_draw_rect(Handle, &rect, paint.Handle);
}

public static SKImage FromEncodedData(SKData data)
{
    if (data == null)
        throw new ArgumentNullException(nameof(data));

    var handle = SkiaApi.sk_image_new_from_encoded(data.Handle);
    return GetObject(handle); // Returns null when decode fails
}
```

```cpp
// C API forwards directly to Skia – no exception handling and minimal validation.
SK_C_API void sk_canvas_draw_rect(sk_canvas_t* canvas, const sk_rect_t* rect, const sk_paint_t* paint) {
    AsCanvas(canvas)->drawRect(*AsRect(rect), *AsPaint(paint));
}
```

---

## Introduction

Error handling in SkiaSharp must navigate the complexities of crossing managed/unmanaged boundaries while maintaining safety and usability. This document explains how errors propagate through the three-layer architecture and the patterns used at each layer.

## Core Challenge: Managed/Unmanaged Boundary

The fundamental challenge in SkiaSharp error handling is preventing invalid operations from reaching native code, where they would cause crashes.

### Safety Strategy: Validate in C#

**SkiaSharp's approach:**
- **C# layer performs the critical validation** where the native API would crash or misbehave, but many high-volume helpers skip extra checks and simply propagate native return values.
- **C API is a minimal wrapper** - no exception handling, no broad input validation.
- **Performance optimization** - most validation happens once (in managed code) when it is needed.

```csharp
// Representative guard: managed code blocks obvious misuse, but not every failure path throws.
public void DrawRect(SKRect rect, SKPaint paint)
{
    // Validation happens here
    if (paint == null)
        throw new ArgumentNullException(nameof(paint));

    SkiaApi.sk_canvas_draw_rect(Handle, &rect, paint.Handle);
}
```

```cpp
// C API trusts C# - no validation needed
SK_C_API void sk_canvas_draw_rect(sk_canvas_t* canvas, const sk_rect_t* rect, const sk_paint_t* paint) {
    AsCanvas(canvas)->drawRect(*AsRect(rect), *AsPaint(paint));
}
```

**Why this works:**
1. C# wrappers are the only supported entry point into the C API.
2. The code paths that *must* guard inputs (for example null pointers) do so before invoking native code.
3. Remaining failures are surfaced through native return values (`false`, `nullptr`, default structs) which managed callers can observe.
4. Keeping the C API thin avoids redundant checks and simplifies maintenance.

## Error Handling Strategy by Layer

```mermaid
graph TB
    subgraph CSharp["C# Layer - Safety Boundary"]
        CS1[Validate critical inputs]
        CS2[Call P/Invoke]
        CS3[Process native result]
        CS4{Failure?}
        CS5[Throw or propagate]
        CS6[Return success value]
        
        CS1 --> CS2
        CS2 --> CS3
        CS3 --> CS4
        CS4 -->|Yes| CS5
        CS4 -->|No| CS6
    end
    
    subgraph CAPI["C API Layer - Minimal Wrapper"]
        C1[Convert types]
        C2[Call C++ method]
        C3[Return result]
        
        C1 --> C2
        C2 --> C3
    end
    
    subgraph CPP["C++ Skia Layer"]
        CPP1[Execute operation]
        CPP2{Error?}
        CPP3[Throw exception]
        CPP4[Return result]
        
        CPP1 --> CPP2
        CPP2 -->|Yes| CPP3
        CPP2 -->|No| CPP4
    end
    
    CS3 -.->|P/Invoke| C1
    C2 -.->|Direct call| CPP1
    CPP3 -.->|Would propagate!| CS3
    C3 -.->|Result| CS4
    
    style CSharp fill:#e1f5e1
    style CAPI fill:#fff4e1
    style CPP fill:#e1e8f5
    style CS1 fill:#90ee90
    style CS2 fill:#90ee90
    style CS6 fill:#ffe1e1
```

**Layer characteristics:**

```
┌─────────────────────────────────────────────────┐
│ C# Layer - SAFETY BOUNDARY                      │
│ ✓ Guards inputs that would crash native code    │
│ ✓ Interprets native return values               │
│ ✓ Throws when APIs guarantee exceptions         │
│ → Defines managed-facing error semantics        │
└─────────────────┬───────────────────────────────┘
                  │
┌─────────────────▼───────────────────────────────┐
│ C API Layer - MINIMAL WRAPPER                   │
│ ✓ Converts opaque pointers to C++ types         │
│ ✓ Calls C++ methods directly                    │
│ ✓ Returns results to C#                         │
│ ✗ Does NOT validate parameters                  │
│ ✗ Does NOT catch exceptions                     │
│ → Trusts C# has validated everything            │
└─────────────────┬───────────────────────────────┘
                  │
┌─────────────────▼───────────────────────────────┐
│ C++ Skia Layer                                  │
│ ✓ May throw C++ exceptions                      │
│ ✓ Uses assertions for invalid states            │
│ ✓ Relies on RAII for cleanup                    │
│ → Only receives valid inputs from C# via C API  │
└─────────────────────────────────────────────────┘
```

## Layer 1: C# Error Handling

The C# layer is responsible for:
1. **Proactive validation** before calling native code
2. **Interpreting error signals** from C API
3. **Throwing appropriate C# exceptions**

### Pattern 1: Parameter Validation

Validate parameters **before** P/Invoking to avoid undefined behavior in native code.

```csharp
public class SKCanvas : SKObject
{
    public void DrawRect(SKRect rect, SKPaint paint)
    {
        // Validate parameters before calling native code
        if (paint == null)
            throw new ArgumentNullException(nameof(paint));
        
        // Check object state
        if (Handle == IntPtr.Zero)
            throw new ObjectDisposedException("SKCanvas");
        
        // Call native - at this point parameters are valid
        SkiaApi.sk_canvas_draw_rect(Handle, &rect, paint.Handle);
    }
}
```

**Common validations:**
- Null checks for reference parameters (most common)
- Handle != IntPtr.Zero checks in constructors after native creation
- Range checks for numeric values (less common)
- Array bounds checks (where applicable)

### Pattern 2: Factory Method Null Returns

**Important:** Static factory methods return `null` on failure, they do NOT throw exceptions.

```csharp
public class SKImage : SKObject, ISKReferenceCounted
{
    // Factory method returns null on failure
    public static SKImage FromEncodedData(SKData data)
    {
        if (data == null)
            throw new ArgumentNullException(nameof(data));
        
        var handle = SkiaApi.sk_image_new_from_encoded(data.Handle);
        return GetObject(handle);  // Returns null if handle is IntPtr.Zero
    }
    
    // ✅ CORRECT usage - always check for null
    public static void Example()
    {
        var image = SKImage.FromEncodedData(data);
        if (image == null)
            throw new InvalidOperationException("Failed to decode image");
        
        // Safe to use image
        canvas.DrawImage(image, 0, 0);
    }
    
    // Boolean return methods let caller decide
    public bool ReadPixels(SKImageInfo dstInfo, IntPtr dstPixels, int dstRowBytes, int srcX, int srcY)
    {
        // Boolean return indicates success/failure
        var success = SkiaApi.sk_image_read_pixels(
            Handle, &dstInfo, dstPixels, dstRowBytes, srcX, srcY, 
            SKImageCachingHint.Allow);
        
        return success;  // Caller can check and decide what to do
    }
}
```

**Affected Methods:** All static factory methods follow this pattern:
- `SKImage.FromEncodedData()` - Returns null on decode failure
- `SKImage.FromBitmap()` - Returns null on failure
- `SKSurface.Create()` - Returns null on allocation failure
- `SKShader.CreateLinearGradient()` - Returns null on failure
- And many more...

### Pattern 3: Constructor Failures

Constructors must ensure valid object creation or throw.

```csharp
public class SKBitmap : SKObject
{
    public SKBitmap(SKImageInfo info)
        : base(IntPtr.Zero, true)
    {
        var nInfo = SKImageInfoNative.FromManaged(ref info);
        Handle = SkiaApi.sk_bitmap_new();
        
        if (Handle == IntPtr.Zero)
            throw new InvalidOperationException("Failed to create bitmap");
        
        // Try to allocate pixels
        if (!SkiaApi.sk_bitmap_try_alloc_pixels(Handle, &nInfo))
        {
            // Clean up partial object
            SkiaApi.sk_bitmap_destructor(Handle);
            Handle = IntPtr.Zero;
            throw new InvalidOperationException("Failed to allocate bitmap pixels");
        }
    }
}
```

### Pattern 4: Disposal Safety

Ensure disposal methods never throw.

```csharp
protected override void DisposeNative()
{
    try
    {
        if (this is ISKReferenceCounted refcnt)
            refcnt.SafeUnRef();
        // Never throw from dispose
    }
    catch
    {
        // Swallow exceptions in dispose
        // Logging could happen here if available
    }
}
```

### Common C# Exception Types

| Exception | When to Use |
|-----------|-------------|
| `ArgumentNullException` | Null parameter passed |
| `ArgumentOutOfRangeException` | Numeric value out of valid range |
| `ArgumentException` | Invalid argument value |
| `ObjectDisposedException` | Operation on disposed object |
| `InvalidOperationException` | Object in wrong state or operation failed |
| `NotSupportedException` | Operation not supported on this platform |

## Layer 2: C API Implementation (Actual)

The C API layer is a **minimal wrapper** that:
1. **Converts types** - Opaque pointers to C++ types
2. **Calls C++ methods** - Direct pass-through
3. **Returns results** - Back to C#

**It does NOT:**
- ❌ Validate parameters (C# does this)
- ❌ Catch exceptions (Skia rarely throws; C# prevents invalid inputs)
- ❌ Check for null pointers (C# ensures valid pointers)

### Actual Pattern: Direct Pass-Through

Most C API functions are simple wrappers with no error handling:

```cpp
// Void methods - direct call
SK_C_API void sk_canvas_draw_rect(sk_canvas_t* canvas, const sk_rect_t* rect, const sk_paint_t* paint) {
    AsCanvas(canvas)->drawRect(*AsRect(rect), *AsPaint(paint));
}

SK_C_API void sk_canvas_clear(sk_canvas_t* canvas, sk_color_t color) {
    AsCanvas(canvas)->clear(color);
}

SK_C_API void sk_paint_set_color(sk_paint_t* paint, sk_color_t color) {
    AsPaint(paint)->setColor(color);
}
```

### Pattern: Boolean Return (Native Result)

Some C++ methods naturally return bool - C API passes it through:

```cpp
// C++ method returns bool, C API passes it through
SK_C_API bool sk_bitmap_try_alloc_pixels(sk_bitmap_t* bitmap, const sk_imageinfo_t* info) {
    return AsBitmap(bitmap)->tryAllocPixels(AsImageInfo(info));
}

SK_C_API bool sk_image_read_pixels(const sk_image_t* image, const sk_imageinfo_t* dstInfo,
                                    void* dstPixels, size_t dstRowBytes, int srcX, int srcY) {
    return AsImage(image)->readPixels(AsImageInfo(dstInfo), dstPixels, dstRowBytes, srcX, srcY);
}
```

**Note:** C# checks the returned `bool` and throws exceptions if needed.

### Pattern: Null Return (Factory Methods)

Factory methods return `nullptr` naturally if creation fails:

```cpp
// Returns nullptr if Skia factory fails
SK_C_API sk_image_t* sk_image_new_from_encoded(const sk_data_t* data) {
    return ToImage(SkImages::DeferredFromEncodedData(sk_ref_sp(AsData(data))).release());
}

SK_C_API sk_surface_t* sk_surface_new_raster(const sk_imageinfo_t* info) {
    return ToSurface(SkSurfaces::Raster(AsImageInfo(info)).release());
}

SK_C_API sk_shader_t* sk_shader_new_linear_gradient(/*...*/) {
    return ToShader(SkGradientShader::MakeLinear(/*...*/).release());
}
```

**Note:** C# checks for `IntPtr.Zero` and throws `InvalidOperationException` if null.

### Why No Exception Handling?

**Design decision reasons:**
1. **Performance** - No overhead from try-catch blocks
2. **Simplicity** - Minimal code in C API layer
3. **Single responsibility** - C# owns all validation
4. **Skia rarely throws** - Most Skia functions don't throw exceptions
5. **Trust boundary** - C API trusts its only caller (C# wrapper)

## Layer 3: C++ Skia Error Handling

The C++ layer can use normal C++ error handling:
- Exceptions for exceptional cases
- Return values for expected failures
- Assertions for programming errors

**Skia's approach:**
- Minimal exception usage (mostly for allocation failures)
- Return nullptr or false for failures
- Assertions (SK_ASSERT) for debug builds
- Graceful degradation when possible

```cpp
// Skia C++ patterns
sk_sp<SkImage> SkImages::DeferredFromEncodedData(sk_sp<SkData> data) {
    if (!data) {
        return nullptr;  // Return null, don't throw
    }
    // ... create image or return nullptr on failure
}

bool SkBitmap::tryAllocPixels(const SkImageInfo& info) {
    // Returns false if allocation fails
    return this->tryAllocPixelsInfo(info);
}
```

## Complete Error Flow Examples

> **Note:** The examples below show hypothetical defensive patterns. The actual SkiaSharp implementation uses minimal C API wrappers without exception handling or null checks (see "Actual Pattern: Direct Pass-Through" section above).

### Example 1: Drawing with Invalid Paint (Actual Implementation)

```csharp
// C# Layer - Validation
public void DrawRect(SKRect rect, SKPaint paint)
{
    if (paint == null)
        throw new ArgumentNullException(nameof(paint));  // ✓ C# validates
    
    SkiaApi.sk_canvas_draw_rect(Handle, &rect, paint.Handle);
}

// C API Layer - Actual implementation (no validation)
SK_C_API void sk_canvas_draw_rect(sk_canvas_t* canvas, const sk_rect_t* rect, const sk_paint_t* paint) {
    // Direct pass-through - trusts C# validated parameters
    AsCanvas(canvas)->drawRect(*AsRect(rect), *AsPaint(paint));
}
```

### Example 2: Image Creation Failure (Actual Implementation)

```csharp
// C# Layer - Actual implementation
public static SKImage FromEncodedData(SKData data)
{
    if (data == null)
        throw new ArgumentNullException(nameof(data));  // ✓ Validate input
    
    var handle = SkiaApi.sk_image_new_from_encoded(data.Handle);
    return GetObject(handle);  // Returns null if handle is IntPtr.Zero
}

// C API Layer - Actual implementation (no try-catch)
SK_C_API sk_image_t* sk_image_new_from_encoded(const sk_data_t* data) {
    // Direct pass-through - returns nullptr if Skia factory fails
    return ToImage(SkImages::DeferredFromEncodedData(sk_ref_sp(AsData(data))).release());
}

// C++ Layer
sk_sp<SkImage> SkImages::DeferredFromEncodedData(sk_sp<SkData> data) {
    if (!data) {
        return nullptr;  // ✓ Return null on invalid input
    }
    
    auto codec = SkCodec::MakeFromData(data);
    if (!codec) {
        return nullptr;  // ✓ Decoding failed, return null
    }
    
    return SkImages::DeferredFromCodec(std::move(codec));
}
```

### Example 3: Operation on Disposed Object

```csharp
// C# Layer
public void DrawRect(SKRect rect, SKPaint paint)
{
    if (Handle == IntPtr.Zero)
        throw new ObjectDisposedException("SKCanvas");  // ✓ Check state
    
    if (paint == null)
        throw new ArgumentNullException(nameof(paint));
    
    if (paint.Handle == IntPtr.Zero)
        throw new ObjectDisposedException("SKPaint");  // ✓ Check parameter state
    
    SkiaApi.sk_canvas_draw_rect(Handle, &rect, paint.Handle);
}
```

### Example 4: Pixel Allocation Failure

```csharp
// C# Layer
public class SKBitmap : SKObject
{
    public SKBitmap(SKImageInfo info)
        : base(IntPtr.Zero, true)
    {
        var nInfo = SKImageInfoNative.FromManaged(ref info);
        Handle = SkiaApi.sk_bitmap_new();
        
        if (Handle == IntPtr.Zero)
            throw new InvalidOperationException("Failed to create bitmap");  // ✓ Check creation
        
        if (!SkiaApi.sk_bitmap_try_alloc_pixels(Handle, &nInfo))
        {
            // ✓ Allocation failed - clean up and throw
            SkiaApi.sk_bitmap_destructor(Handle);
            Handle = IntPtr.Zero;
            throw new InvalidOperationException(
                $"Failed to allocate pixels for {info.Width}x{info.Height} bitmap");
        }
    }
}

// C API Layer - Pass through the bool from C++
SK_C_API bool sk_bitmap_try_alloc_pixels(sk_bitmap_t* bitmap, const sk_imageinfo_t* info) {
    return AsBitmap(bitmap)->tryAllocPixels(AsImageInfo(info));
}

// C++ Layer
bool SkBitmap::tryAllocPixels(const SkImageInfo& info) {
    // Returns false if allocation fails (out of memory, invalid size, etc.)
    if (!this->setInfo(info)) {
        return false;
    }
    
    auto allocator = SkBitmapAllocator::Make(info);
    if (!allocator) {
        return false;  // ✓ Allocation failed
    }
    
    fPixelRef = std::move(allocator);
    return true;
}
```

**Note:** C++ method returns bool naturally, C API passes it through, C# checks it.

## Error Handling Best Practices

### For C# Layer

✅ **DO:**
- Validate reference parameters (null checks) before P/Invoke
- Check Handle != IntPtr.Zero in **constructors** after native creation
- Inspect native return values and choose whether to propagate them or throw, matching existing patterns
- Throw appropriate exception types matching existing API patterns
- Use meaningful error messages when throwing
- Provide context in exception messages

❌ **DON'T:**
- Skip null checks for reference parameters (C API won't check)
- Ignore return values from factory/try methods
- Throw from Dispose/finalizer
- Use generic exceptions without context
- Assume C API will validate anything

**Actual code patterns:**

```csharp
// Pattern 1: Validate reference parameters (most common)
public void DrawRect(SKRect rect, SKPaint paint)
{
    if (paint == null)
        throw new ArgumentNullException(nameof(paint));
    
    SkiaApi.sk_canvas_draw_rect(Handle, &rect, paint.Handle);
    // Note: Does NOT check if Handle is disposed - assumes valid object
}

// Pattern 2: Check Handle in constructors
public SKPaint()
    : this(SkiaApi.sk_compatpaint_new(), true)
{
    if (Handle == IntPtr.Zero)
        throw new InvalidOperationException("Unable to create a new SKPaint instance.");
}

// Pattern 3: Factory methods return null (don't throw)
public static SKImage FromEncodedData(SKData data)
{
    if (data == null)
        throw new ArgumentNullException(nameof(data));
    
    var handle = SkiaApi.sk_image_new_from_encoded(data.Handle);
    return GetObject(handle);  // Returns null if handle is IntPtr.Zero
}
```

**Note:** Most instance methods do NOT check if the object is disposed (Handle == IntPtr.Zero). They assume the object is valid if it exists. The primary validation is null-checking reference parameters.

### For C API Layer

✅ **DO:**
- Keep implementations simple and direct
- Pass through natural return values (bool, null)
- Trust that C# has validated everything
- Use `sk_ref_sp()` when passing ref-counted objects to C++
- Call `.release()` on `sk_sp<T>` when returning

❌ **DON'T:**
- Add unnecessary validation (C# already did it)
- Add try-catch blocks unless truly needed
- Modify Skia return values
- Throw exceptions

**Current implementation pattern:**
```cpp
// Simple, direct wrapper - trusts C# validation
SK_C_API void sk_canvas_draw_rect(sk_canvas_t* canvas, const sk_rect_t* rect, const sk_paint_t* paint) {
    AsCanvas(canvas)->drawRect(*AsRect(rect), *AsPaint(paint));
}

// Pass through natural bool return
SK_C_API bool sk_bitmap_try_alloc_pixels(sk_bitmap_t* bitmap, const sk_imageinfo_t* info) {
    return AsBitmap(bitmap)->tryAllocPixels(AsImageInfo(info));
}

// Factory returns nullptr naturally on failure
SK_C_API sk_image_t* sk_image_new_from_encoded(const sk_data_t* data) {
    return ToImage(SkImages::DeferredFromEncodedData(sk_ref_sp(AsData(data))).release());
}
```

### For Both Layers

✅ **DO:**
- Fail fast with clear errors
- Provide useful error messages
- Clean up resources on failure
- Document error conditions
- Test error paths

❌ **DON'T:**
- Silently ignore errors (unless documented)
- Leave objects in invalid state
- Leak resources on error paths

## Debugging Failed Operations

### When a C# call fails:

1. **Check C# validation** - Did parameter validation catch it?
2. **Check return value** - Is C API returning error?
3. **Check C API implementation** - Is it catching exceptions?
4. **Check C++ behavior** - What does Skia return?
5. **Check documentation** - Is the operation supported?

### Common Failure Scenarios

| Symptom | Likely Cause | Solution |
|---------|--------------|----------|
| `ArgumentNullException` | Null parameter | Check calling code |
| `ObjectDisposedException` | Using disposed object | Check lifecycle |
| `InvalidOperationException` | C API returned error | Check C API return value |
| Crash in native code | Invalid parameter from C# | Add/fix C# validation |
| Silent failure | Error not propagated | Add return value checks |

**Note:** If crashes occur in native code, it usually means C# validation is missing or incomplete.

## Platform-Specific Error Handling

Some operations may fail on specific platforms:

```csharp
public static GRContext CreateGl()
{
    var handle = SkiaApi.gr_direct_context_make_gl(IntPtr.Zero);
    
    if (handle == IntPtr.Zero)
    {
        #if __IOS__ || __TVOS__
        throw new PlatformNotSupportedException("OpenGL not supported on iOS/tvOS");
        #else
        throw new InvalidOperationException("Failed to create OpenGL context");
        #endif
    }
    
    return GetObject(handle);
}
```

## Summary

Error handling in SkiaSharp still relies on the managed layer as the safety boundary, but each layer has clearly defined responsibilities:

1. **C# Layer (Safety Boundary)**: 
    - Guards reference parameters with null checks before calling native code
    - Validates Handle creation in constructors (throws if Handle == IntPtr.Zero after native creation)
    - Inspects native return values and translates them into either propagated results (`null`/`false`) or managed exceptions, depending on the API contract
    - Establishes the public behavior (throwing vs. returning status)

2. **C API Layer (Minimal Wrapper)**: 
    - Calls directly into the C++ API with almost no additional logic
    - Avoids catching exceptions or duplicating validation
    - Performs only the type conversions needed for P/Invoke

3. **C++ Skia Layer**: 
    - Executes the actual work, optionally returning `nullptr`/`false` on failure
    - Relies on upstream layers to pass valid arguments; assertions fire if they do not

Key principles:
- **C# defines the managed contract** – check similar APIs to see whether they throw or return status
- **C API stays thin** – no redundant validation or exception handling
- **Prefer fail-fast guards** for inputs we know will crash native code
- **Propagate native status codes** when the existing API surface expects nullable/bool results
- **Provide clear exception messages** when throwing
- **Do not throw from Dispose/finalizers** – follow current suppression pattern

## Next Steps

- See [Memory Management](memory-management.md) for cleanup on error paths
- See [Adding New APIs](adding-new-apis.md) for implementing error handling in new bindings
