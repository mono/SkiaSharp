# 0 — Undisposed native handle

**Where to look.** `binding/SkiaSharp/**`. `grep -rnE "GetObject\(|new SK[A-Za-z]+\(" binding/SkiaSharp`, then trace ownership of each hit through to a `Dispose`/`using`.

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
a *borrowed getter* result (area 1), a *same-instance return* (area 2), or a
*process-wide singleton* (area 7). Confirm the object is genuinely owned before disposing,
or you convert a leak into a double-free.

**Real cases:** the general class behind many reports; see `documentation/dev/memory-management.md`.
