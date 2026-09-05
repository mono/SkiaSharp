# 8 — Field not nulled on dispose

**Where to look.** `binding/SkiaSharp/**`. `grep -rnE "DisposeManaged|= null;" binding/SkiaSharp` — a managed field that remains after the wrapper/native teardown, whether it held an owned child or a borrowed dependency.

**What it is.** A `Dispose`/`DisposeManaged` completes wrapper/native teardown but leaves
a managed field rooted even though it is no longer needed. This can be a cached native child
that the wrapper owns, or a caller-owned dependency that the wrapper merely borrows.

**Why it's bad.** An owned child left pointing at a disposed wrapper can be disposed again or
read after disposal → **double-dispose / `AccessViolationException`**. A borrowed dependency
must not be disposed by this wrapper, but retaining its stale field still keeps the caller's
object graph rooted for as long as the disposed wrapper remains reachable.

**Leak (❌):**
```csharp
protected override void DisposeManaged()
{
    TearDownNativeUsage();
    if (ownsDependency)
        dependency?.Dispose();
    // ❌ `dependency` stays rooted when it was borrowed.
}
```

**Fix (✓):**
```csharp
protected override void DisposeManaged()
{
    TearDownNativeUsage();         // native callbacks can no longer use `dependency`
    if (ownsDependency)
        dependency?.Dispose();     // dispose only what this wrapper owns
    dependency = null;             // clear owned and borrowed dependencies after teardown
}
```

**Watch out (❌ don't):** don't clear a field while callbacks or native teardown can still use
it, and never dispose a caller-owned resource. Once teardown no longer needs the field, the
order for an owned resource is **dispose, then null**; for a borrowed resource, **do not
dispose, but null**. The abbreviated example shows the required lifetime rule, not a claim
that every wrapper with a similarly shaped field is defective.

**Real cases:** #1256, #1344. `SKSurface` (nulls its cached `SKCanvas`) and `SKPixmap` (nulls
`pixelSource`) are the correct patterns.
