---
applyTo: "binding/SkiaSharp/**/*.cs"
---

# C# Bindings Instructions

You are working in the C# wrapper layer that provides .NET access to Skia via P/Invoke.

> **📚 Documentation:**
> - **Quick Start:** [design/QUICKSTART.md](../../design/QUICKSTART.md)
> - **Architecture:** [design/architecture-overview.md](../../design/architecture-overview.md)
> - **Memory Management:** [design/memory-management.md](../../design/memory-management.md)
> - **Adding APIs:** [design/adding-new-apis.md](../../design/adding-new-apis.md)

## Critical Rules

- All `IDisposable` types MUST dispose native handles
- Use `SKObject` base class for handle management
- Never expose `IntPtr` directly in public APIs
- Always validate parameters before P/Invoke calls
- Check return values from C API

## Pointer Type to C# Mapping

> **💡 See [design/memory-management.md](../../design/memory-management.md) for pointer type concepts.**
> Below are C#-specific patterns for each type.

### Raw Pointers (Non-Owning)
```csharp
// OwnsHandle = false, no disposal
public SKSurface Surface {
    get {
        var handle = SkiaApi.sk_canvas_get_surface(Handle);
        return GetOrAddObject(handle, owns: false, (h, o) => new SKSurface(h, o));
    }
}
```

### Owned Pointers
```csharp
public class SKCanvas : SKObject
{
    public SKCanvas(SKBitmap bitmap) : base(IntPtr.Zero, true)
    {
        if (bitmap == null)
            throw new ArgumentNullException(nameof(bitmap));
        Handle = SkiaApi.sk_canvas_new_from_bitmap(bitmap.Handle);
    }
    
    protected override void DisposeNative()
    {
        SkiaApi.sk_canvas_destroy(Handle);
    }
}
```

### Reference-Counted Pointers
```csharp
public class SKImage : SKObject, ISKReferenceCounted
{
    public static SKImage FromBitmap(SKBitmap bitmap)
    {
        if (bitmap == null)
            throw new ArgumentNullException(nameof(bitmap));
        
        var handle = SkiaApi.sk_image_new_from_bitmap(bitmap.Handle);
        if (handle == IntPtr.Zero)
            throw new InvalidOperationException("Failed to create image");
        
        return GetObject(handle);  // For ISKReferenceCounted
    }
}
```

## Parameter Validation

### Before P/Invoke
```csharp
public void DrawRect(SKRect rect, SKPaint paint)
{
    // 1. Validate parameters
    if (paint == null)
        throw new ArgumentNullException(nameof(paint));
    
    // 2. Check object state
    if (Handle == IntPtr.Zero)
        throw new ObjectDisposedException(nameof(SKCanvas));
    
    // 3. Call native
    SkiaApi.sk_canvas_draw_rect(Handle, &rect, paint.Handle);
}
```

## Error Handling

Factory methods return null on failure:
```csharp
public static SKImage FromEncodedData(SKData data)
{
    if (data == null)
        throw new ArgumentNullException(nameof(data));
    
    var handle = SkiaApi.sk_image_new_from_encoded(data.Handle);
    return GetObject(handle);  // Returns null if handle is IntPtr.Zero
}

// Callers should check for null:
var image = SKImage.FromEncodedData(data);
if (image == null)
    throw new InvalidOperationException("Failed to decode image");
```

Constructors throw on failure:
```csharp
public SKBitmap(SKImageInfo info) : base(IntPtr.Zero, true)
{
    var nInfo = SKImageInfoNative.FromManaged(ref info);
    Handle = SkiaApi.sk_bitmap_new();
    
    if (Handle == IntPtr.Zero)
        throw new InvalidOperationException("Failed to create bitmap");
}
```

## What NOT to Do

❌ **Don't expose IntPtr directly in public APIs**
❌ **Don't skip parameter validation**
❌ **Don't ignore return values**
