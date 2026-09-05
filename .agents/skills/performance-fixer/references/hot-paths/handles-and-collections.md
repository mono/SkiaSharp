# Hot path: handles & collections (getters, HandleDictionary, object tracking)

Two related overheads in the binding's object-tracking machinery (see
[../repo-helpers.md](../repo-helpers.md) for the machinery itself).

---

## A. Redone work in a hot getter (cache a stable native wrapper)

**The signature:** a getter does real work on **every** access — a P/Invoke, a locked
`HandleDictionary` lookup (via `PlatformLock` — `ReaderWriterLockSlim` on non-Windows, a Win32
critical section on Windows), and an `OwnedBy`/`ConcurrentDictionary` registration — even though the
native pointer it wraps is **stable for the owner's lifetime**, so the managed wrapper could be
cached.

**Why it matters.** Draw-heavy loops fetch these per draw call. #4247: `SKSurface.Canvas` was hit
up to 1,000×/frame, each redoing P/Invoke + locked lookup + registration. Caching the wrapper
removes all of it after the first access.

**Complexity: MEDIUM** — caching plants a pointer-stability + disposal-invalidation invariant a
reader must be told about. Comment it and prove the four preconditions below.

### Where to look
```bash
rg -n "GetObject<|GetObject \(|OwnedBy|owns: false|unrefExisting" binding/SkiaSharp --glob '!*.generated.cs'
```
Candidates: `SKSurface.SurfaceProperties`/`Context`, `SKCanvas.Surface`/`Context`,
`SKPictureRecorder.RecordingCanvas`, immutable-object accessors like `SKImage.ColorSpace` /
`SKPixmap.ColorSpace`. **Already optimized (skip):** `SKSurface.Canvas` (the #4247 win),
`SKPaint.GetFont`/`SKTypeface.GetFont` (`font ??=`).

### Slow → Fast
**Slow (❌):**
```csharp
public SKCanvas Canvas =>
    OwnedBy(SKCanvas.GetObject(SkiaApi.sk_surface_get_canvas(Handle), false), this);  // every call
```
**Fast (✓):**
```csharp
public SKCanvas Canvas {
    get {
        // Skia returns the SAME SkCanvas* for the surface's lifetime (getCachedCanvas),
        // so cache the wrapper. Re-fetch if never cached or disposed out from under us.
        if (canvas == null || canvas.Handle == IntPtr.Zero)
            canvas = OwnedBy(SKCanvas.GetObject(SkiaApi.sk_surface_get_canvas(Handle), false, unrefExisting: false), this);
        GC.KeepAlive(this);
        return canvas;
    }
}
// and drop the cache in DisposeManaged so a disposed owner holds no stale wrapper.
```

### Watch out (❌ don't)
Don't cache a pointer that is **not** actually stable. Before caching, prove **all four
preconditions** — otherwise file it issue-only:
1. **Pointer identity** — Skia returns the *same* native pointer for the owner's lifetime (verify in
   the pinned Skia source — #4247 traced `getCachedCanvas` to show `fCachedCanvas` is assigned once,
   never reset). `Context`/`SurfaceProperties`/`ColorSpace` may **not** have this.
2. **Owner lifetime** — the cached child cannot be invalidated independently of the owner.
3. **Disposal invalidation** — clear the cache in `DisposeManaged` and re-fetch if the cached
   wrapper's `Handle` went zero, or it becomes a **use-after-free / stale-handle** bug.
4. **Thread model** — the getter is not racing to build two wrappers concurrently.

This borders on `memory-leak-fixer` territory; getting any of the four wrong turns a perf win into a
crash. `GC.KeepAlive(this)` after reading keeps the owner rooted.

**Real case:** #4247 (cache the `SKSurface.Canvas` wrapper across calls).

---

## B. Repeated invariant work in a global registry / handle lookup

**The signature:** a global object-tracking lookup repeats invariant reflection, metadata,
liveness, or object-probe work for every lookup. The result may be stable for the relevant type,
registry entry, or critical-section observation, but that is an invariant to establish, not an
assumption.

**Historical case:** [#4629](https://github.com/mono/SkiaSharp/issues/4629) and
[#4630](https://github.com/mono/SkiaSharp/pull/4630) found that the global `HandleDictionary`
path re-evaluated a per-type reflection predicate for each `GetInstance`/`GetOrAddObject` call.
PR #4630 cached that type-invariant registration decision, removing repeated work while preserving
the existing behavior.

**Candidate, not a blanket rule.** A cached or snapshotted observation can be a candidate only
when it preserves the original observation's lifetime, type, disposal, ownership, concurrency,
and debug behavior. Do not assume every repeated lookup is safely cacheable or that current code
has a qualifying invariant.

### Where to look
```bash
rg -n "HandleDictionary|GetInstance|GetOrAddObject|SkipObjectRegistration|ISKSkipObjectRegistration|IsAssignableFrom" binding/SkiaSharp --glob '!*.generated.cs'
```

Start at global registry and handle-lookup critical sections, then trace the callers that make a
path hot. Look for computation that is repeated despite being invariant for the type, registry
entry, or protected observation; distinguish it from a read whose timing is part of the
synchronization design.

### Proof obligation

Benchmark the **actual lookup method and its normal routing path** with BenchmarkDotNet `New` vs
`Old`; do not benchmark an isolated predicate or metadata read. Use realistic tracked-wrapper
workloads and report allocations as well as timing.

Before changing the implementation, add an equivalence test over the behavior the existing lookup
protects: successful and absent lookups, relevant type/disposal/ownership outcomes, Debug-only
diagnostics, and concurrent GC/dispose/create pressure where those operations can interact. The
test must establish that the retained observation has not weakened a safety check or changed which
object is returned.

Temporarily make the proposed optimization deliberately wrong, such as by inverting its cached
decision or removing a protected outcome, and prove the equivalence test goes red before removing
that change. If the test cannot distinguish the error, strengthen it or leave this candidate alone.

### Watch out (❌ don't)

Do not cache or snapshot work merely because it looks redundant. Changing when an observation is
made or retained can alter lifetime, type, disposal, ownership, concurrency, or debug semantics.
Preserve every existing check only when the test and stress evidence establish behavior parity. If
the benchmark is noise, the equivalence evidence is incomplete, or the concurrency model is
unclear, record no candidate.

---

## C. Unsized / contended collections

**The signature:** a collection central to object tracking is created with **default capacity /
concurrency**, so it rehashes/resizes under load; or a shared structure serialises hot access
through a single lock where sizing (or finer-grained locking) would cut contention.

**Why it matters.** Every native object creation touches the `HandleDictionary`. Default-sized
concurrent maps rehash and contend as thousands of handles are registered; #4182 initialises the
`ConcurrentDictionary` with an explicit concurrency level + capacity; #4102 prototyped a
lock-striped `HandleDictionary`.

**Complexity: LOW** for sizing, **HIGH** for touching the locking discipline.

### Where to look
```bash
rg -n "new (Concurrent)?Dictionary|new List<|ReaderWriterLock|lock \(|GetOrAdd" binding/SkiaSharp binding/Binding.Shared --glob '!*.generated.cs'
rg -n "OwnedObjects|KeepAliveObjects|OwnedBy|HandleDictionary|PlatformLock" binding/SkiaSharp --glob '!*.generated.cs'
```
Candidates: `HandleDictionary` (global map under a platform lock), `SKObject`'s per-object owned/
keep-alive `ConcurrentDictionary` (default-sized), `SKRuntimeEffect`/`SKFont.GlyphPathCache` dicts
without the known count as capacity. **Already hardened (skip):** `SKBlender` mode cache (pre-sized),
`PlatformLock` (exists to avoid a Windows alertable-lock deadlock — don't "simplify" it),
`SKObject.Dispose` (holds the write lock only for the flag CAS).

### Slow → Fast
```csharp
// Slow (❌):
static readonly ConcurrentDictionary<IntPtr, SKObject> instances = new();  // default sizing
// Fast (✓): size for the expected count and the machine's parallelism.
static readonly ConcurrentDictionary<IntPtr, SKObject> instances =
    new(concurrencyLevel: Environment.ProcessorCount, capacity: 1024);
```

### Watch out (❌ don't)
This is core, concurrency-sensitive infrastructure — a "faster" locking scheme that gets the memory
model subtly wrong causes **rare, non-deterministic corruption** no micro-benchmark surfaces. Sizing
is low-risk and easy to prove; rewriting the locking is high-risk (#4102 stayed a prototype) — demand
a heavy concurrent create/dispose stress test or file an issue instead. **Size to evidence, not a
big default:** a large capacity on a *global/shared* map is justified; a large capacity on a
*per-object* map (thousands of `SKObject`s) **multiplies memory across every instance** — size those
to the small known count (`names.Length`, `glyphCount`) or leave them default. Trading a rehash for
megabytes of bloat is a regression.

**Real case:** #4182 (sized `ConcurrentDictionary`); #4102 (lock-striped `HandleDictionary`
prototype — did **not** ship).
