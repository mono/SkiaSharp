# SkiaSharp leak types — reference

The catalogue the `memory-leak-fixer` skill scans against. **Every family below is drawn
from a real, historical SkiaSharp fix** (issue/PR cited), so the hunt targets patterns that
have actually shipped as bugs in this repo — not hypotheticals.

**Scope: managed C# only.** This skill hunts and fixes leaks in the code SkiaSharp owns —
the C# bindings (`binding/**`) and view layers (`source/**`). The native Skia C/C++ under
`externals/skia/**` (including our C shim) is **out of scope**: it is upstream, cannot be
built or validated on a standard runner, and its fixes go through a different process. Every
family here is therefore something you can prove and fix from C#.

Read this alongside [`documentation/dev/memory-management.md`](../../../../documentation/dev/memory-management.md),
which is the authoritative ownership model (pointer types, `owns:` flag, ref-count rules,
the `HandleDictionary`, and the same-instance-return contract). This file adds, per family:
**what it is → why it's bad → a leaking example → the idiomatic fix → a watch-out.**

Code samples are illustrative and trimmed to the essential lines; real wrappers add
argument validation and `GC.KeepAlive`. `✓` = correct, `❌` = the bug.

Each family ends with a **Watch out (❌ don't):** note — the leak-specific *wrong fix* that
turns one bug into another. Re-read the matching one during the pre-PR self-review gate.

Quick index (the `#` is the rotating focus index the skill uses):

| # | Family | One-line signature |
|--:|---|---|
| 0 | Undisposed native handle | owned/ref-counted `SKObject` escapes a factory/cache and is never disposed |
| 1 | Wrong `owns:` flag | borrowed pointer wrapped `owns:true` (double-free) or owned handle `owns:false` (leak) |
| 2 | Same-instance double-dispose | `Subset`/`ToRasterImage`-style self-return disposed twice |
| 3 | Managed retention (Views) | event/handler subscribed but never torn down; `base.Dispose` not chained |
| 4 | `fixed`-pointer lifetime | a `fixed` pointer stored by native code beyond the block |
| 5 | Finalizer / collection ordering | child holds a raw pointer into a parent that can be collected first |
| 6 | Clone / copy double-free | `Clone()` shares one native pointer across two wrappers |
| 7 | Disposing native statics/singletons | an immortal native object reached via a non-protected cache is unref'd |
| 8 | Field not nulled on dispose | disposed native child left referenced → double-dispose / graph retained |
| 9 | Stream / callback / delegate-proxy lifetime | `GCHandle`/proxy freed too early (dangling) or never (leak) |
| 10 | Allocation-failure path | wrapper returned even when native create failed, or half-built object leaked |

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

**Watch out (❌ don't):** don't slap `using`/`Dispose` on a handle you don't actually own —
a *borrowed getter* result (family 1), a *same-instance return* (family 2), or a
*process-wide singleton* (family 7). Confirm the object is genuinely owned before disposing,
or you convert a leak into a double-free.

**Real cases:** the general class behind many reports; see `documentation/dev/memory-management.md`.

---

## 1 — Wrong `owns:` flag

**What it is.** The `owns:` argument to `GetObject`/the wrapper ctor doesn't match the
ownership contract: a *borrowed* pointer (a `_get_` getter that returns an internal pointer)
is wrapped `owns:true`, or an *owned* handle (a `_new_`/create that returns a fresh object)
is wrapped `owns:false`.

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

**Watch out (❌ don't):** don't guess the flag or flip it to make a crash/leak "go away."
Read the P/Invoke name: `_new_`/`_create` returns owned → `owns:true`; `_get_`/property-style
accessors return borrowed → `owns:false`. Getting this backwards just swaps a leak for a
crash. When the contract is genuinely unclear from the managed side, file an issue rather
than flipping blind.

**Real cases:** the counterpart of family 7 (dispose-protected singletons); verify each new
getter against whether it returns a fresh ref or a borrowed pointer.

---

## 2 — Same-instance double-dispose

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

**Watch out (❌ don't):** don't add an *unconditional* `result.Dispose()` — the reference
check `if (result != source)` is the whole fix; dropping it re-introduces the double-free.
And don't dispose the source before you're finished with the result, since they may be the
same object.

**Real cases:** the `Subset`/`ToRasterImage` contract in `documentation/dev/memory-management.md`.

---

## 3 — Managed retention (Views / handlers)

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

**Watch out (❌ don't):** don't unsubscribe from inside a finalizer — a finalizer must not
touch other managed objects (the event source may already be finalized). Do the `-=` in
`Dispose(bool disposing)` under `if (disposing)`. And don't forget to chain
`base.Dispose(disposing)` — a subtle leak that looks fixed but isn't.

**Real cases:** #2955, #2472, #1095; **#3309** — `SKGLElement.Dispose(bool)` never calls
`base.Dispose()` on the OpenTK `GLWpfControl`, leaking the GL context (fix PR #3311 open).

---

## 4 — `fixed`-pointer lifetime

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

**Watch out (❌ don't):** don't "fix" this by adding `GC.KeepAlive(data)` *inside* the `fixed`
block — the pointer escapes the block, so KeepAlive there proves nothing. And don't free the
`GCHandle` before native is finished with the memory; free it in the release delegate.

**Real cases:** open bug **#3472**, open fix PR **#3473** ("Make Blob.FromStream GC safe");
documented in `documentation/dev/memory-management.md`.

---

## 5 — Finalizer / collection ordering

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

**Watch out (❌ don't):** don't root the parent with a pinned `GCHandle` — a plain managed
field is enough and a pinned handle is its own leak (family 9). And don't lean on
`GC.KeepAlive` for a *long-lived* child (an iterator you hold across calls); KeepAlive only
covers the current method, so a stored child needs the field.

**Real cases:** #3796 (SKPath/SKPathBuilder finalizer race), #3291 (SKAutoCanvasRestore).
**Un-filed examples this workflow surfaced:** `SKRegion.SpanIterator` (above); and
`SKPixmap.ExtractSubset`/`WithColorType`/`WithColorSpace`/`WithAlphaType` don't propagate the
`pixelSource` GC-root the way `PeekPixels` does, so a subset pixmap can outlive its backing
bitmap → dangling pixels. Fix: `result.pixelSource = pixelSource ?? this;`.

---

## 6 — Clone / copy double-free

**What it is.** A `Clone()`/copy that **shares** one native pointer between two managed
wrappers, both of which believe they own it and will dispose it.

**Why it's bad.** Both wrappers call `DisposeNative` on the same handle → **double-free**.

**Leak (❌):**
```csharp
public SKPaint Clone() =>
    new SKPaint(Handle, owns: true);   // ❌ two wrappers own the same native paint
```

**Fix (✓):** mint a *fresh* native object via the clone API:
```csharp
public SKPaint Clone() =>
    GetObject<SKPaint>(SkiaApi.sk_compatpaint_clone(Handle));  // fresh handle, owns:true
```

**Watch out (❌ don't):** don't "fix" the double-free by setting `owns:false` on the clone —
that just swaps a double-free for a leak (or a use-after-free if the original is disposed
first). The clone must own a *separate* native object, not borrow the source's.

**Real cases:** #2904 (SKPaint.Clone), #2899.

---

## 7 — Disposing native statics / singletons

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

**Watch out (❌ don't):** don't null out or replace the cached static, and don't wrap it
`owns:true` "just in case." The only correct fix is the dispose-protected accessor with
`unrefExisting:false`; copy an existing correct singleton (`SKBlender` cache) rather than
inventing a new disposal path.

**Real cases:** #1863, #4080, #1224, #3730. The `SKBlender` mode cache and `SKColorFilter`
gamma filters are the canonical correct implementations to copy.

---

## 8 — Field not nulled on dispose

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

**Watch out (❌ don't):** don't null the field *before* disposing the child — you'd drop the
only reference and leak the native object instead. The order is fixed: **dispose, then null.**

**Real cases:** #1256, #1344. `SKSurface` (nulls its cached `SKCanvas`) and `SKPixmap` (nulls
`pixelSource`) are the correct patterns.

---

## 9 — Managed stream / callback / delegate-proxy lifetime

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

**Watch out (❌ don't):** don't `Free()` the `GCHandle` in the same method that hands it to
native — native still holds it. And don't leave the destroy proxy `null` to "avoid a crash";
that leaks. Free it in the destroy/release callback, and only there.

**Real cases:** #3589, #2916, #996, #2446. `SKManagedStream`/`DelegateProxies` use a `Weak`
user-data `GCHandle` freed by the destroy proxy — the reference implementation.

---

## 10 — Allocation-failure path

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

**Watch out (❌ don't):** don't make a *factory* throw when its contract is to return `null`
(that's an ABI/behavior break — add the null-return, don't change the exception surface).
And don't return `null` while leaving an earlier half-built native object un-freed on the
error branch.

**Real cases:** #1784, #1642. `SKCodec.Create` (revokes stream ownership before disposing on
`codec == null`) and `SKColorSpaceIccProfile.Create` (disposes the half-built profile on parse
failure) are the correct patterns.
