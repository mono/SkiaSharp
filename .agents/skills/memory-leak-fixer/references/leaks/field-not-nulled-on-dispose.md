# 8 — Field not nulled on dispose

**Where to look.** `binding/SkiaSharp/**`. `grep -rnE "DisposeManaged|= null;" binding/SkiaSharp` — a freed native child field that isn't cleared afterwards.

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
