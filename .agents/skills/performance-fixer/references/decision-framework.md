# Performance decision framework

Use this to decide whether a candidate optimization is worth applying **before** you spend effort
proving it. The goal is not maximum micro-optimization everywhere — it is to spend complexity
budget where it buys the most, and leave simple code simple where perf does not matter. In
SkiaSharp specifically, the wins worth chasing are the managed layer's *own* overhead between the
caller and Skia, not Skia's C++ rasterizer (that is upstream).

## The two axes

Score every candidate on two axes:

- **Impact** = how much faster / lower-allocation the optimized version is, **times how often the
  code runs**. A 10× win on code that runs once at startup is low impact. A 1.4× win on a
  per-frame / per-point / per-glyph path is high impact. Name the realistic caller.
- **Complexity** = the structural complexity the optimized version *leaves in the code* (scored
  below), independent of the win and of the one-time effort to write it.

Optimize by **impact ÷ complexity** (return on investment). The 80/20 rule holds: a few
low-complexity patterns deliver most of the available wins.

## Structural complexity: how to score it

Complexity is how much the change adds to the cost of reading, reasoning about, and safely
maintaining the code — independent of how advanced the API looks. A sophisticated API used in a
localized, self-evidently-correct way is *low*; a simple-looking change that plants a fragile
non-local invariant is *not*. Score by the dominant of:

1. **Locality of reasoning** — can a reader confirm correctness from the single expression/method,
   or must they reason across callers, lifetimes, or the whole type?
2. **Self-evidence of any added invariant** — if the change adds a rule the compiler does not
   enforce (write-before-read, return-buffer-once, matching element size/endianness, single-
   threaded access, native-pointer-stable-for-lifetime), is that rule obvious and confined, or
   subtle and easy for a later edit to break?
3. **Safety surface** — memory-safety, use-after-free, data-corruption, or undefined-behaviour
   risk. (A binding over native handles has more of this surface than a pure-managed app.)
4. **Shape change** — a drop-in swap (overload, attribute, type choice) vs. restructured control
   flow, custom types, manual state.
5. **Verification need** — self-evidently correct, or only justified by a benchmark/disassembly.

Levels:

- **Low** — local reasoning, idiomatic drop-in, no safety surface, any added invariant is
  self-evident and contained. Apply by default on hot paths. Examples: a span/`Try*` overload,
  pooling via `Utils.RentArray`, sizing a collection, `sealed`, `[MethodImpl(AggressiveInlining)]`
  on a trivial wrapper, a `stackalloc` with a self-evident write-before-read, a blittable
  `MemoryMarshal.Cast<SKColor,uint>`.
- **Medium** — mostly local, but plants one invariant/lifetime/aliasing concern a reader must be
  *told* (comment-worthy), or a modest custom helper/shape change. Correct by inspection, but a
  careless nearby edit could break it. Examples: **caching a native child wrapper** (the pointer-
  stability + disposal-invalidation invariant, family in `hot-paths/handles-and-collections.md`),
  an `in`/`ref readonly` on a large struct, a reinterpret whose correctness depends on layout.
- **High** — non-local reasoning, multiple/fragile invariants, a real memory-safety/UB surface, or
  a structural rewrite. Reserve for a **proven** hot path, prove first, isolate behind a small
  API, comment, keep a simple reference implementation. Examples: **a managed port of native float
  math** (bit-parity across runtimes + the x87 fallback — `hot-paths/geometry-math.md`), manual
  SIMD, `unsafe`/raw-pointer buffer juggling, any change to the `HandleDictionary` locking.

**Report high-complexity options, don't hide them.** When the fastest approach is medium/high and
you recommend a simpler one, still report the faster option and its tradeoff so the maintainer can
choose.

## The quadrants

- **High impact, low complexity** — always do it. The bulk of this skill (span overloads, sized
  collections, pooling, cached inline helpers).
- **High impact, high complexity** — only on a *proven* hot path, only when the benchmark shows it
  matters; isolate, comment, keep a reference impl. The SKMatrix native→managed port lives here.
- **Low impact, low complexity** — do it when writing new code if it is equally readable; don't
  churn working code just to apply it.
- **Low impact, high complexity** — never.

## Hot path vs cold path (SkiaSharp)

The same change is mandatory or pointless depending on where it runs. Ask: how many times does
this execute under load, and does it sit between the caller and a result they are waiting on?

- **Treat as HOT:** per-frame draw submission and canvas/paint access (`SKCanvas`, `SKSurface`),
  per-point/per-vector geometry (`SKMatrix` map/invert/concat), per-glyph text/shaping
  (`SKFont`, `SkiaSharp.HarfBuzz`), per-pixel/scanline buffer copies (`SKBitmap`, `SKPixmap`),
  startup color/theme hydration (`SKColor.Parse`), and the `HandleDictionary` (touched on **every**
  native object creation).
- **Treat as COLD:** one-time factory setup, `SKFontManager`/typeface enumeration done once,
  configuration, error paths. Prefer the simplest code; allocations there are fine.

Optimize the hot list aggressively; leave the cold list simple. A wasteful method on a cold path
is **not a finding**.

## The SkiaSharp gate: two proofs, both mandatory

This is where SkiaSharp differs from a general perf pass. A candidate only becomes a fix when
**both** hold (details in [measuring.md](measuring.md)):

1. **Faster** — a BenchmarkDotNet `New` vs `Old` comparison shows a **meaningful, repeatable**
   speedup outside the Mean/Error/StdDev bands, stable across ≥2 runs, with **no allocation
   regression**, on a **realistically shaped** workload.
2. **Behaviour-identical** — an equivalence test proves the optimized path returns the **same
   result and side effects as the original/native path** (bit-exact for numeric ports) across
   normal *and* edge inputs. A faster answer that differs from Skia is a **rendering regression**,
   not an optimization.

## Not a finding — when to stand down (emit a `noop`)

SkiaSharp is mature; much of the obvious overhead is already optimized. Standing down on a
hardened surface is the **expected** outcome, not a failure. It is **not** a finding when:

- **No realistic hot caller** — cold path; making it faster helps nobody.
- **No measurable/repeatable win** — within noise, or only wins on an unrealistic size (tiny
  L1-resident array, memory-bandwidth-bound huge loop) and evaporates on re-run. #4241 documented
  a 1,024-point size that measured both **0.68× and 1.48× for identical code** — that is noise.
- **A regression on any real shape** — especially SIMD: `Vector256` is emulated and ran
  **5.7–6.5× slower** on ARM64 NEON in #4241. A win on x64 that regresses ARM64 is not shippable
  without correct per-runtime gating.
- **Correctness can't be preserved** — if the only way faster changes what the method returns
  (rounding, an edge case, validation), stand down. A faster wrong answer is a bug.
- **The real fix is native/upstream** — file the finding as an issue; don't open a PR you can't
  validate.

Never manufacture a 2% micro-win on a synthetic loop no real caller hits.
