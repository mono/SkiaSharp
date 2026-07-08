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

## Keeping Related Objects Alive (Owned / OwnedBy / Referenced)

When one wrapper depends on another — because native code stores a raw pointer into it, or
because it created a child that must be torn down with it — **do not hand-roll a
`private readonly` field or a `List<T>`** to keep the dependency reachable. `SKObject` has three
built-in helpers (see `binding/SkiaSharp/SKObject.cs`) that record the relationship in one of two
per-object dictionaries, so the runtime manages it for you:

| Helper | Stored in | Keeps child alive? | Disposes child with owner? | Use when |
|--------|-----------|--------------------|-----------------------------|----------|
| `Referenced(owner, child)` | `KeepAliveObjects` | Yes | **No** | Owner holds a raw/borrowed pointer into `child` and just needs it rooted; the caller still owns `child`. |
| `OwnedBy(child, owner)` | `OwnedObjects` | Yes | Yes | `child` is a native-owned sub-object (`owns:false`) that should be disposed when the owner is (e.g. a cached getter result). Returns `child`. |
| `Owned(owner, child)` | `OwnedObjects` | Yes | Yes | `child` was created by managed code and should be disposed with the owner (e.g. a wrapped stream). Returns `owner`. |

Both dictionaries are released when the owner is disposed: `OwnedObjects` entries are disposed
then cleared, `KeepAliveObjects` entries are only cleared (never disposed).

**`Referenced` — root without owning (the "avoid collection" case).** A child that holds a raw
pointer into a parent must root that parent for its whole lifetime, or non-deterministic
finalization can free the parent first → use-after-free. `Referenced` is exactly this:

```csharp
// Idiomatic: park the parent in this.KeepAliveObjects, rooted for the child's lifetime,
// released automatically on dispose. No extra field needed.
internal SpanIterator(SKRegion region, int y, int left, int right)
    : base(SkiaApi.sk_region_spanerator_new(region.Handle, y, left, right), owns: true)
{
    Referenced(this, region);
}
```

Real uses: `SKDocument` roots its output stream, `SKColorSpace` its ICC profile, `SKSVG` its
stream — all via `Referenced(...)`. For a **mutable set** of rooted children, add with
`Referenced(this, child)` and remove with `KeepAliveObjects.TryRemove(child.Handle, out _)` /
`KeepAliveObjects.Clear()` — again, no hand-rolled `List<T>`.

**`Owned` / `OwnedBy` — root *and* dispose.** When the owner should also dispose the child:

```csharp
// OwnedBy: borrowed getter result that must die with the owner (returns the child).
canvas = OwnedBy(SKCanvas.GetObject(SkiaApi.sk_surface_get_canvas(Handle), owns: false), this);

// Owned: managed-created child disposed with the owner (returns the owner).
return Owned(CreatePdf(managedStream), managedStream);
```

For a **one-shot** P/Invoke that stores nothing, a plain `GC.KeepAlive(parent)` after the call is
enough — no dictionary entry is needed.

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

### 5. Pinned pointer lifetime with `fixed`
```csharp
// WRONG: native object outlives the fixed block, GC moves the array
fixed (byte* ptr = data)
{
    blob = new Blob(ptr, data.Length); // blob stores ptr
} // ptr invalid here — data can be moved/collected

// CORRECT: stable pin for native-retained pointers
var handle = GCHandle.Alloc(data, GCHandleType.Pinned);
var ptr = handle.AddrOfPinnedObject();
blob = new Blob(ptr, data.Length, () => handle.Free());
```

Known affected API: `HarfBuzzSharp.Blob.FromStream` — passes temporary `fixed` pointer to native HarfBuzz which stores it, leading to data corruption under GC pressure.

## Summary

| Question | Answer |
|----------|--------|
| Inherits SkRefCnt? | Ref-counted → `ISKReferenceCounted` |
| Inherits SkNVRefCnt? | Ref-counted → `ISKNonVirtualReferenceCounted` |
| Mutable (Canvas/Paint)? | Owned → `DisposeNative()` calls delete |
| Parameter/getter? | Raw → `owns: false` |
