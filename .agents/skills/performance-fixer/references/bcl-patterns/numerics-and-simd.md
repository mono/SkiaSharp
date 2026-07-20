# BCL patterns: numerics, SIMD & codegen

Techniques for the compute-heavy managed layer — porting native math, batching with SIMD, and
nudging the JIT. Mind **all TFMs** and always prove **bit-exact** parity for numeric changes
([../measuring.md](../measuring.md)). See [../decision-framework.md](../decision-framework.md) —
most of these are HIGH complexity and belong only on a proven hot path.

## Managed math port + `Vector4`/`Vector128` SIMD
- **Do:** reimplement Skia's math in managed C#, **faithfully mirroring the C++ algorithm**
  (float-vs-double, op order, type-mask/`±0`/`Inf`/`NaN` semantics). Batch paths use
  `System.Numerics.Vector4` (two points per 128-bit lane) with a `Vector128.Shuffle` swizzle on
  net8+ and a portable `Vector4` fallback below.
- **Instead of:** a per-call `SkiaApi.sk_*` P/Invoke for a handful of float ops.
- **Why:** removes the transition entirely — #4241's approach, up to 24×.
- **Complexity:** high · **TFM:** `Vector128` net7+ (fallback below) · **ABI:** internal (bodies).
- **Mandatory x86 .NET Framework fallback:** x87's 80-bit intermediates break bit-parity, so gate a
  `static readonly bool UseNativeMath` on `RuntimeInformation` that routes the ported methods back
  through the native C API on x86 .NET Framework; the equivalence test then asserts bit-parity on
  *all* platforms (see [../hot-paths/geometry-math.md](../hot-paths/geometry-math.md)).

## `AggressiveInlining` for tiny wrappers
- **Do:** `[MethodImpl(MethodImplOptions.AggressiveInlining)]` on one-line forwarding wrappers and
  nibble/parse helpers (SKColor's parse helpers are annotated).
- **Instead of:** letting call overhead swamp a trivial body.
- **Why:** the JIT folds the body in, exposing constants and enabling further optimization.
- **Complexity:** low · **TFM:** any · **ABI:** internal.
- **Watch out:** only for genuinely tiny methods — inlining a large body bloats code and hurts the
  instruction cache. Never `AggressiveOptimization` (bypasses tiered PGO).

## `sealed` and devirtualization
- **Do:** seal internal types that aren't designed for inheritance.
- **Why:** lets the JIT devirtualize and inline virtual calls.
- **Complexity:** low · **TFM:** any · **ABI:** don't seal a *public* unsealed type (breaks
  subclassers) — internal types only.

## .NET 10 escape analysis
- **Do:** keep helper temporaries non-escaping (don't store in fields, don't return them) so the
  net10 JIT can stack-allocate/eliminate some arrays and delegates for free.
- **Why:** small, simple helpers get allocation elimination on net10 with no code change.
- **Complexity:** low · **TFM:** benefit net10 (harmless below) · **ABI:** internal.

## Anti-patterns (turn a "fix" into a regression)
- **Blind SIMD.** `Vector256<T>` is emulated on ARM64 NEON and was measured **5.7–6.5× slower** in
  #4241. Prefer `Vector128`, gate per-architecture, benchmark on *both* x64 and ARM64.
- **Premature SIMD on memory-bandwidth-bound loops** — packing overhead with no end-to-end win
  (#4241's 1M-point case is at parity).
- **A micro-win that only shows on unrealistic sizes or swings on re-run** — noise, not a fix.
