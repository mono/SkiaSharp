# Memory Management in SkiaSharp

Understanding pointer types is **critical** - wrong type = memory leaks or crashes.

## Three Pointer Types

| Type | C++ | C API | C# | Cleanup | Examples |
|------|-----|-------|-----|---------|----------|
| **Raw** | `const SkType&` params | Pass through | `owns: false` | None | Paint in `DrawRect()` |
| **Owned** | `new SkType()` | `sk_type_new/delete` | `DisposeNative()` calls delete | Explicit delete | SKCanvas, SKPaint, SKPath |
| **Ref-counted** | `sk_sp<SkType>` | `sk_type_ref/unref` | `ISKReferenceCounted` | Call unref | SKImage, SKShader, SKData |

## How to Identify Pointer Type

Check the C++ class declaration:
- Inherits `SkRefCnt` or `SkRefCntBase`? → **Virtual ref-counted** (`ISKReferenceCounted`)
- Inherits `SkNVRefCnt<T>`? → **Non-virtual ref-counted** (`ISKNonVirtualReferenceCounted`)
- Mutable class (Canvas, Paint, Path, Bitmap)? → **Owned**
- Parameter or getter return? → **Raw (non-owning)**

### Pointer Type Decision Tree

```
Is it wrapped in sk_sp<T>?
├─ Yes → Is it SkRefCnt or SkNVRefCnt?
│        ├─ SkRefCnt → ISKReferenceCounted (virtual ref counting)
│        └─ SkNVRefCnt<T> → ISKNonVirtualReferenceCounted
└─ No → Is it a parameter or getter return?
         ├─ Yes → Raw pointer (owns: false)
         └─ No → Owned (DisposeNative deletes)
```

## Common Types by Category

| Category | Types |
|----------|-------|
| **Owned** | SKCanvas, SKPaint, SKPath, SKBitmap, SKRegion |
| **Ref-counted (virtual)** | SKImage, SKShader, SKSurface, SKPicture, SKColorFilter, SKTypeface |
| **Ref-counted (non-virtual)** | SKData, SKTextBlob, SKVertices, SKColorSpace |

## Raw Pointers (Non-Owning)

Borrowed references - caller or parent owns the object.

```cpp
// C API - just pass through
SK_C_API void sk_canvas_draw_paint(sk_canvas_t* canvas, const sk_paint_t* paint) {
    AsCanvas(canvas)->drawPaint(*AsPaint(paint));
}
```

```csharp
// C# - create non-owning wrapper
public SKSurface Surface {
    get {
        var handle = SkiaApi.sk_canvas_get_surface(Handle);
        return GetOrAddObject(handle, owns: false, (h, o) => new SKSurface(h, o));
    }
}
```

## Owned Pointers

Single owner, explicit delete on dispose.

```cpp
// C API - new/delete pairs
SK_C_API sk_paint_t* sk_paint_new(void) { return ToPaint(new SkPaint()); }
SK_C_API void sk_paint_delete(sk_paint_t* paint) { delete AsPaint(paint); }
```

```csharp
// C# - DisposeNative calls delete
public class SKPaint : SKObject {
    public SKPaint() : base(SkiaApi.sk_paint_new(), true) { }
    protected override void DisposeNative() => SkiaApi.sk_paint_delete(Handle);
}
```

## Reference-Counted Pointers

Shared ownership via ref counting. Two variants:
- **Virtual** (`SkRefCnt`): SKImage, SKShader, SKSurface - use `ISKReferenceCounted`
- **Non-virtual** (`SkNVRefCnt<T>`): SKData, SKTextBlob - use `ISKNonVirtualReferenceCounted`

```cpp
// C API - ref/unref functions, use sk_ref_sp when C++ expects sk_sp<T>
SK_C_API sk_image_t* sk_image_new_from_encoded(const sk_data_t* data) {
    return ToImage(SkImages::DeferredFromEncodedData(sk_ref_sp(AsData(data))).release());
}
```

```csharp
// C# - implements ISKReferenceCounted, disposal calls unref
public class SKImage : SKObject, ISKReferenceCounted {
    public static SKImage FromEncodedData(SKData data) {
        if (data == null) throw new ArgumentNullException(nameof(data));
        return GetObject(SkiaApi.sk_image_new_from_encoded(data.Handle));
    }
}
```

## HandleDictionary

The `HandleDictionary` maintains a mapping from native `IntPtr` handles to C# wrapper objects.

**Why needed:**
- Ensures only one C# wrapper exists per native handle (object identity)
- Prevents duplicate wrappers that could cause double-free bugs
- Critical for reference-counted objects where multiple C# refs share one native object

**How it works:**
- On wrapper creation: registers handle → wrapper mapping
- On getting existing object: returns existing wrapper if handle already registered
- On dispose: removes handle from dictionary
- Thread-safe via reader/writer lock

## Common Mistakes

### 1. Wrong cleanup for ref-counted types
```cpp
// WRONG: delete on ref-counted type
SK_C_API void sk_image_destroy(sk_image_t* image) { delete AsImage(image); }
// CORRECT: unref
SK_C_API void sk_image_unref(const sk_image_t* image) { SkSafeUnref(AsImage(image)); }
```

### 2. Missing sk_ref_sp when C++ expects sk_sp<T>
```cpp
// WRONG: no ref increment
return ToImage(SkImages::Make(AsData(data)).release());
// CORRECT: sk_ref_sp increments ref count
return ToImage(SkImages::Make(sk_ref_sp(AsData(data))).release());
```

### 3. Disposing borrowed objects
```csharp
// WRONG: will destroy surface owned by canvas
return new SKSurface(handle, true);
// CORRECT: non-owning wrapper
return GetOrAddObject(handle, owns: false, ...);
```

### 4. Forgetting .release() on sk_sp returns
```cpp
// WRONG: sk_sp destructor decrements ref to 0
return ToImage(image);
// CORRECT: .release() transfers ownership
return ToImage(image.release());
```

## Summary

| Question | Answer |
|----------|--------|
| Inherits SkRefCnt? | Ref-counted → `ISKReferenceCounted` |
| Inherits SkNVRefCnt? | Ref-counted → `ISKNonVirtualReferenceCounted` |
| Mutable (Canvas/Paint)? | Owned → `DisposeNative()` calls delete |
| Parameter/getter? | Raw → `owns: false` |
