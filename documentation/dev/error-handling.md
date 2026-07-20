# Error Handling in SkiaSharp

**Key principle:** C# is the safety boundary. C API is a minimal pass-through.

## Layer Responsibilities

| Layer | Responsibility |
|-------|---------------|
| **C#** | Validates parameters, checks returns, throws exceptions |
| **C API** | Minimal wrapper, trusts C#, returns bool/null on failure |
| **C++** | Native Skia, may throw (but rarely does) |

## Pattern 1: Null Parameter Validation

C# validates before P/Invoke. C API does NOT validate.

```csharp
// C# - validates
public void DrawRect(SKRect rect, SKPaint paint) {
    if (paint == null)
        throw new ArgumentNullException(nameof(paint));
    SkiaApi.sk_canvas_draw_rect(Handle, &rect, paint.Handle);
}
```

```cpp
// C API - trusts C#, no validation
void sk_canvas_draw_rect(sk_canvas_t* canvas, const sk_rect_t* rect, const sk_paint_t* paint) {
    AsCanvas(canvas)->drawRect(*AsRect(rect), *AsPaint(paint));
}
```

## Pattern 2: Factory Methods Return Null

Factory methods return `null` on failure - they do **NOT** throw.

```csharp
public static SKImage FromEncodedData(SKData data) {
    if (data == null)
        throw new ArgumentNullException(nameof(data));
    var handle = SkiaApi.sk_image_new_from_encoded(data.Handle);
    return GetObject(handle);  // Returns null if handle is IntPtr.Zero
}

// Caller MUST check for null
var image = SKImage.FromEncodedData(data);
if (image == null)
    throw new InvalidOperationException("Failed to decode image");
```

## Pattern 3: Boolean Try Methods

Methods that can fail return `bool`.

```cpp
// C API - passes through C++ bool return
bool sk_bitmap_try_alloc_pixels(sk_bitmap_t* bitmap, const sk_imageinfo_t* info) {
    return AsBitmap(bitmap)->tryAllocPixels(AsImageInfo(info));
}
```

```csharp
// C# - provide both Try and throwing versions
public bool TryAllocPixels(SKImageInfo info) {
    return SkiaApi.sk_bitmap_try_alloc_pixels(Handle, &info);
}

public void AllocPixels(SKImageInfo info) {
    if (!TryAllocPixels(info))
        throw new InvalidOperationException("Failed to allocate pixels");
}
```

## Pattern 4: Constructors Throw

Constructors throw on failure (unlike factory methods).

```csharp
public SKBitmap(SKImageInfo info) : base(IntPtr.Zero, true) {
    Handle = SkiaApi.sk_bitmap_new();
    if (Handle == IntPtr.Zero)
        throw new InvalidOperationException("Failed to create bitmap");
    
    if (!SkiaApi.sk_bitmap_try_alloc_pixels(Handle, &info)) {
        SkiaApi.sk_bitmap_destructor(Handle);
        Handle = IntPtr.Zero;
        throw new InvalidOperationException("Failed to allocate pixels");
    }
}
```

## Common Mistakes

1. **Missing null checks in C#** → crash in native code
2. **Not checking factory return values** → `SKImage.FromEncodedData()` returns null on failure, not exception
3. **Assuming constructors return null** → constructors throw, factory methods return null

## Disposal Safety

Never throw from `Dispose()` or `DisposeNative()`:

```csharp
protected override void DisposeNative()
{
    // Never throw from dispose - swallow exceptions if needed
    if (Handle != IntPtr.Zero)
        SkiaApi.sk_paint_delete(Handle);
}
```

## Common Exception Types

| Exception | When |
|-----------|------|
| `ArgumentNullException` | Null parameter |
| `ArgumentOutOfRangeException` | Value out of range |
| `ObjectDisposedException` | Operation on disposed object |
| `InvalidOperationException` | Operation failed |

## Summary

| Scenario | C API | C# |
|----------|-------|-----|
| Void method | Direct call | Validate params first |
| Bool return | Pass through | Return or throw based on method name |
| Pointer return | Return nullptr on fail | Factory→null, Constructor→throw |
