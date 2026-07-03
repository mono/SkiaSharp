# SkiaSharp leak types — reference

The catalogue the `memory-leak-fixer` skill scans against. **Every family below is drawn
from a real, historical SkiaSharp fix** (issue/PR cited), so the hunt targets patterns that
have actually shipped as bugs in this repo — not hypotheticals.

Read this alongside [`documentation/dev/memory-management.md`](../../../../documentation/dev/memory-management.md),
which is the authoritative ownership model (pointer types, `owns:` flag, ref-count rules,
the `HandleDictionary`, and the same-instance-return contract). This file adds, per family:
**what it is → why it's bad → a leaking example → the idiomatic fix.**

Code samples are illustrative and trimmed to the essential lines; real wrappers add
argument validation and `GC.KeepAlive`. `✓` = correct, `❌` = the bug.

Quick index (the `#` is the rotating focus index the skill uses):

| # | Family | One-line signature |
|--:|---|---|
| 0 | Undisposed native handle | owned/ref-counted `SKObject` escapes a factory/cache and is never disposed |
| 1 | Wrong `owns:` flag | borrowed pointer wrapped `owns:true` (double-free) or owned handle `owns:false` (leak) |
| 2 | C-API ownership mismatch | missing `sk_ref_sp`/`.release()`, or `delete` on a ref-counted type in the C shim |
| 3 | Same-instance double-dispose | `Subset`/`ToRasterImage`-style self-return disposed twice |
| 4 | Managed retention (Views) | event/handler subscribed but never torn down; `base.Dispose` not chained |
| 5 | `fixed`-pointer lifetime | a `fixed` pointer stored by native code beyond the block |
| 6 | Finalizer / collection ordering | child holds a raw pointer into a parent that can be collected first |
| 7 | Clone / copy double-free | `Clone()` shares one native pointer across two wrappers |
| 8 | Disposing native statics/singletons | an immortal native object reached via a non-protected cache is unref'd |
| 9 | Field not nulled on dispose | disposed native child left referenced → double-dispose / graph retained |
| 10 | Stream / callback / delegate-proxy lifetime | `GCHandle`/proxy freed too early (dangling) or never (leak) |
| 11 | Allocation-failure path | wrapper returned even when native create failed, or half-built object leaked |

---

## 0 — Undisposed native handle

**What it is.** A factory, getter, or cache mints an *owned* or *ref-counted* `SKObject`
(pixels, GPU resources, font tables, encoded data) that escapes without ever being disposed
— or is parked in a static/instance cache that is never cleared.

**Why it's bad.** The native allocation is only reclaimed by the finalizer, which runs
non-deterministically and late. Under load (per-frame image decode, GPU surfaces) native
memory and GPU handles pile up far faster than the finalizer frees them → native OOM,
GPU resource exhaustion, or a monotonically growing process while managed heap looks fine.

**Leak (❌):**
```csharp
// Decodes a fresh SKImage every frame and drops it on the floor.
foreach (var frame in frames) {
    var image = SKImage.FromEncodedData(frame);   // owns a native SkImage
    canvas.DrawImage(image, 0, 0);
    // image never disposed → native pixels accumulate until finalization
}
```

**Fix (✓):**
```csharp
foreach (var frame in frames) {
    using var image = SKImage.FromEncodedData(frame);
    canvas.DrawImage(image, 0, 0);
}   // native pixels freed deterministically at end of scope
```
For a cache, dispose evicted entries and clear the cache on teardown.

**Real cases:** the general class behind many reports; see `documentation/dev/memory-management.md`.

---

## 1 — Wrong `owns:` flag

**What it is.** The `owns:` argument to `GetObject`/the wrapper ctor doesn't match the
C-API contract: a *borrowed* pointer (a `_get_` getter that returns an internal pointer) is
wrapped `owns:true`, or an *owned* handle (a `_new_`/create that returns a fresh object) is
wrapped `owns:false`.

**Why it's bad.** `owns:true` on a borrowed pointer → the wrapper's `DisposeNative` deletes
or unrefs an object it doesn't own → **double-free / `AccessViolationException`**, often in
unrelated code later. `owns:false` on an owned handle → **the object is never freed → leak**.

**Leak (❌):**
```csharp
// sk_foo_get_bar returns a BORROWED pointer owned by the parent foo.
public SKBar Bar =>
    GetObject<SKBar>(SkiaApi.sk_foo_get_bar(Handle), owns: true);  // ❌ double-free
```

**Fix (✓):**
```csharp
public SKBar Bar =>
    GetObject<SKBar>(SkiaApi.sk_foo_get_bar(Handle), owns: false); // borrowed → don't free
```
Conversely, a `sk_bar_new(...)` result is a fresh object and must be `owns: true`.

**Real cases:** the counterpart of family 8 (dispose-protected singletons); verify each new
getter against whether the C shim returns a fresh ref or a borrowed pointer.

---

## 2 — C-API ownership mismatch (the C shim)

**What it is.** In our C shim (`externals/skia/src/c/**`): C++ takes a `sk_sp<T>` but the shim
passes a raw pointer without `sk_ref_sp`; a function returns an `sk_sp<T>` but forgets
`.release()`; or `delete` is used on a `SkRefCnt`/`SkNVRefCnt` type instead of unref.

**Why it's bad.** Missing `.release()` → the local `sk_sp` destructor unrefs the object we
just handed back to managed code → **returned pointer is already dead**. Missing `sk_ref_sp`
where C++ adopts a reference → the object is unref'd once too often → **premature free**.
`delete` on a ref-counted type → **double-free** against the reference count.

**Leak/crash (❌):**
```cpp
sk_image_t* sk_image_new_from_foo(const sk_data_t* cdata) {
    // SkImages::Foo returns sk_sp<SkImage>; without release() the temporary
    // unrefs the image as it goes out of scope → dangling return.
    return ToImage(SkImages::Foo(sk_ref_sp(AsData(cdata))));   // ❌ missing .release()
}
```

**Fix (✓):**
```cpp
sk_image_t* sk_image_new_from_foo(const sk_data_t* cdata) {
    return ToImage(SkImages::Foo(sk_ref_sp(AsData(cdata))).release());  // hand off the ref
}
```
After any C-shim change: `pwsh ./utils/generate.ps1`, then **rebuild natives from source**
(`dotnet cake --target=externals-<platform>`) — never `externals-download`.

**Note:** requires the `externals/skia` submodule to be checked out to read/verify. If it
isn't, flag the candidate "needs C-shim verification" rather than guessing.

---

## 3 — Same-instance double-dispose

**What it is.** Some methods may return the **same** instance rather than a new one —
`SKImage.Subset` (can return `this`), `SKImage.ToRasterImage(ensurePixelData:false)`,
`SKImage.Encode` (routes through `ToRasterImage`). A caller that disposes both the source and
the "result" then disposes the same native object twice.

**Why it's bad.** Double-free → `AccessViolationException`, or the source is destroyed out
from under the caller who still needs it.

**Leak/crash (❌):**
```csharp
var raster = image.ToRasterImage();   // may return `image` itself
raster.Encode(...);
raster.Dispose();
image.Dispose();                       // ❌ if raster == image, second free crashes
```

**Fix (✓):** the framework guards this internally, and callers should too:
```csharp
var raster = image.ToRasterImage();
raster.Encode(...);
if (raster != image)                   // never dispose a same-instance return twice
    raster.Dispose();
image.Dispose();
```
Framework-side pattern (see `SKImage.Encode`): `if (this != raster) raster.Dispose();`.

**Real cases:** the `Subset`/`ToRasterImage` contract in `documentation/dev/memory-management.md`.

---

## 4 — Managed retention (Views / handlers)

**What it is.** In `source/SkiaSharp.Views*`: a handler, control, or renderer subscribes to
an event (`PaintSurface`, `PropertyChanged`, an invalidation ticker, a platform peer callback)
in a ctor / `Connect` / `Loaded`, but the matching `-=` / `Disconnect` / `Unloaded` /
`Dispose` is missing. Or a derived control's `Dispose(bool)` never chains `base.Dispose(bool)`
when the base owns native resources.

**Why it's bad.** The long-lived event *source* now roots the transient view, so the whole
visual subtree — and the native surfaces/GL contexts it owns — is never collected. Repeated
navigation leaks a surface each time.

**Leak (❌):**
```csharp
public MyCanvasControl()
{
    _ticker.Tick += OnTick;         // subscribe
}
protected override void Dispose(bool disposing)
{
    _surface?.Dispose();
    // ❌ _ticker.Tick -= OnTick never happens → ticker roots `this` forever
}
```

**Fix (✓):**
```csharp
protected override void Dispose(bool disposing)
{
    if (disposing)
        _ticker.Tick -= OnTick;      // symmetric teardown
    _surface?.Dispose();
    base.Dispose(disposing);          // chain if the base owns native resources
}
```

**Real cases:** #2955, #2472, #1095; **#3309** — `SKGLElement.Dispose(bool)` never calls
`base.Dispose()` on the OpenTK `GLWpfControl`, leaking the GL context (fix PR #3311 open).

---

## 5 — `fixed`-pointer lifetime

**What it is.** A `fixed` block produces a pointer into a managed array and hands it to native
code that **stores** the pointer (a non-copying mode) and outlives the block. Once the block
exits, the array is unpinned.

**Why it's bad.** After `fixed` ends the GC is free to move or collect the array, but native
code still holds the old address → **use-after-free / silent data corruption** under GC
pressure. Intermittent, load-dependent, extremely hard to reproduce.

**Leak (❌):** the real, still-unfixed `HarfBuzzSharp.Blob.FromStream`:
```csharp
using var ms = new MemoryStream();
stream.CopyTo(ms);
var data = ms.ToArray();
fixed (byte* dataPtr = data) {
    // MemoryMode.ReadOnly = non-copying: HarfBuzz keeps dataPtr.
    // `data` is unpinned the instant this fixed block exits → dangling.
    return new Blob((IntPtr)dataPtr, data.Length, MemoryMode.ReadOnly, () => ms.Dispose());
}
```

**Fix (✓):** pin stably with a `GCHandle` and free it when native releases the blob:
```csharp
var data = ms.ToArray();
var handle = GCHandle.Alloc(data, GCHandleType.Pinned);
return new Blob(handle.AddrOfPinnedObject(), data.Length,
                MemoryMode.ReadOnly, () => handle.Free());
```
(Or use `MemoryMode.Duplicate` so HarfBuzz copies and no pin is needed.)

**Real cases:** open bug **#3472**, open fix PR **#3473** ("Make Blob.FromStream GC safe");
documented in `documentation/dev/memory-management.md`.

---

## 6 — Finalizer / collection ordering

**What it is.** A child wrapper holds a **raw** pointer into a parent's native object but
keeps no managed reference to the parent. Finalization order is non-deterministic, so the
parent can be collected/finalized while the child is still alive.

**Why it's bad.** The child then dereferences freed parent memory → **use-after-free**.
The fix is cheap (root the parent) and has zero ABI impact.

**Leak (❌):** `SKRegion.SpanIterator` keeps no parent field, though its siblings do:
```csharp
public class SpanIterator : SKObject, ISKSkipObjectRegistration
{
    internal SpanIterator(SKRegion region, int y, int left, int right)
        : base(SkiaApi.sk_region_spanerator_new(region.Handle, y, left, right), true)
    {
        // ❌ `region` is discarded; native spanerator holds a raw SkRegion*.
    }
}
```

**Fix (✓):** mirror the sibling `RectIterator`/`ClipIterator`, which root the parent:
```csharp
public class SpanIterator : SKObject, ISKSkipObjectRegistration
{
    private readonly SKRegion region;    // keep the parent alive
    internal SpanIterator(SKRegion region, int y, int left, int right)
        : base(SkiaApi.sk_region_spanerator_new(region.Handle, y, left, right), true)
    {
        this.region = region;
    }
}
```
The lighter-weight alternative for one-shot calls is `GC.KeepAlive(parent)` after the P/Invoke.

**Real cases:** #3796 (SKPath/SKPathBuilder finalizer race), #3291 (SKAutoCanvasRestore).
**Un-filed examples this workflow surfaced:** `SKRegion.SpanIterator` (above); and
`SKPixmap.ExtractSubset`/`WithColorType`/`WithColorSpace`/`WithAlphaType` don't propagate the
`pixelSource` GC-root the way `PeekPixels` does, so a subset pixmap can outlive its backing
bitmap → dangling pixels. Fix: `result.pixelSource = pixelSource ?? this;`.

---

## 7 — Clone / copy double-free

**What it is.** A `Clone()`/copy that **shares** one native pointer between two managed
wrappers, both of which believe they own it and will dispose it.

**Why it's bad.** Both wrappers call `DisposeNative` on the same handle → **double-free**.

**Leak (❌):**
```csharp
public SKPaint Clone() =>
    new SKPaint(Handle, owns: true);   // ❌ two wrappers own the same native paint
```

**Fix (✓):** mint a *fresh* native object via the clone C-API:
```csharp
public SKPaint Clone() =>
    GetObject<SKPaint>(SkiaApi.sk_compatpaint_clone(Handle));  // fresh handle, owns:true
```

**Real cases:** #2904 (SKPaint.Clone), #2899.

---

## 8 — Disposing native statics / singletons

**What it is.** An *immortal* native object — the default/empty typeface, the sRGB /
sRGB-linear color spaces and gamma color filters, the blend-mode blender cache, `SKData.Empty`
— reached through an accessor that is **not** dispose-protected, so the wrapper's
`DisposeNative` unrefs or deletes an object that must live for the whole process.

**Why it's bad.** Unref'ing / deleting a process-wide singleton corrupts it for **every**
caller — crashes or wrong rendering far from the disposal site.

**Leak/crash (❌):**
```csharp
public static SKColorSpace CreateSrgb() =>
    GetObject<SKColorSpace>(SkiaApi.sk_colorspace_new_srgb(), owns: true);  // ❌ singleton
```

**Fix (✓):** route through the dispose-protected accessor so `DisposeNative` is skipped:
```csharp
public static SKColorSpace CreateSrgb() =>
    GetDisposeProtectedObject<SKColorSpace>(
        SkiaApi.sk_colorspace_new_srgb(), owns: false, unrefExisting: false);
```

**Real cases:** #1863, #4080, #1224, #3730. The `SKBlender` mode cache and `SKColorFilter`
gamma filters are the canonical correct implementations to copy.

---

## 9 — Field not nulled on dispose

**What it is.** A `Dispose`/`DisposeManaged` frees a cached native child (a canvas, a
sub-object) but leaves the managed field still pointing at the now-dead wrapper.

**Why it's bad.** A later `Dispose` (or a caller re-reading the field) hits the freed object →
**double-dispose / `AccessViolationException`**; or the stale reference keeps a whole native
graph rooted → leak.

**Leak (❌):**
```csharp
protected override void DisposeManaged()
{
    _canvas?.Dispose();
    // ❌ _canvas still references the disposed wrapper; a second Dispose double-frees.
    base.DisposeManaged();
}
```

**Fix (✓):**
```csharp
protected override void DisposeManaged()
{
    _canvas?.Dispose();
    _canvas = null;               // clear the link → second dispose is a no-op
    base.DisposeManaged();
}
```

**Real cases:** #1256, #1344. `SKSurface` (nulls its cached `SKCanvas`) and `SKPixmap` (nulls
`pixelSource`) are the correct patterns.

---

## 10 — Managed stream / callback / delegate-proxy lifetime

**What it is.** A managed object handed to native code as a callback sink — an
`SKManagedStream`/`SKManagedWStream`/`SKAbstractManagedStream`, a delegate or function-pointer
proxy, or a `GCHandle` pinned for a release/destroy callback — is freed at the wrong time.

**Why it's bad.** Freed **too early** → native invokes a delegate/`GCHandle` that's already
gone → **crash**. Freed **never** → the `GCHandle` and everything it roots leak for the
process lifetime.

**Leak (❌):**
```csharp
// Allocate a GCHandle for the release proc, but never wire the destroy proxy that frees it.
DelegateProxies.Create(releaseProc, out _, out var ctx);
return HarfBuzzApi.hb_blob_create(ptr, len, mode, (void*)ctx, null);  // ❌ proxy = null → ctx leaks
```

**Fix (✓):** pass the destroy proxy so native frees the `GCHandle` when it's done:
```csharp
DelegateProxies.Create(releaseProc, out _, out var ctx);
var proxy = releaseProc != null ? DelegateProxies.DestroyProxy : null;
return HarfBuzzApi.hb_blob_create(ptr, len, mode, (void*)ctx, proxy);
```
Keep the handle rooted for **exactly** the native object's lifetime — not shorter, not longer.

**Real cases:** #3589, #2916, #996, #2446. `SKManagedStream`/`DelegateProxies` use a `Weak`
user-data `GCHandle` freed by the destroy proxy — the reference implementation.

---

## 11 — Allocation-failure path

**What it is.** A factory wraps and returns a managed object even when the native
create/decode returned `null`/`0` or failed, or leaks a half-built native object on the error
path.

**Why it's bad.** A wrapper around `IntPtr.Zero` throws `NullReferenceException` /
`AccessViolationException` on first use, far from the real failure. A half-built native left
un-freed on the error branch is a straight leak.

**Leak/crash (❌):**
```csharp
public static SKFoo Create(...)
{
    var handle = SkiaApi.sk_foo_new(...);   // may return IntPtr.Zero on failure
    return new SKFoo(handle, owns: true);   // ❌ wraps a null handle
}
```

**Fix (✓):**
```csharp
public static SKFoo? Create(...)
{
    var handle = SkiaApi.sk_foo_new(...);
    if (handle == IntPtr.Zero)
        return null;                        // factory returns null on failure
    return new SKFoo(handle, owns: true);
}
```
On multi-step builds, free any partial native objects before returning on the error path.

**Real cases:** #1784, #1642. `SKCodec.Create` (revokes stream ownership before disposing on
`codec == null`) and `SKColorSpaceIccProfile.Create` (disposes the half-built profile on parse
failure) are the correct patterns.
