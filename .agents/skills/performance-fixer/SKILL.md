---
name: performance-fixer
description: >
  Scan SkiaSharp for managed-C# performance opportunities AND fix them, proving each with a
  BenchmarkDotNet measurement plus a behaviour-parity test. Two modes: (1) SCAN — hunt the
  SkiaSharp perf signature (pure math round-tripping through native P/Invoke, an allocating
  parse/convert helper or missing Span overload, a hot getter redoing native lookups every
  call, per-element interop in a loop, avoidable marshalling/struct copies, or an unsized/
  contended collection) and prove the win with a benchmark; (2) FIX — implement the minimal
  managed optimization, prove it is faster AND behaviour-identical, and open a PR.

  Triggers: "performance", "perf scan", "optimize", "make it faster", "hot path", "reduce
  allocations", "P/Invoke overhead", "interop overhead", "speed up", "port to managed", "add
  Span overload", "cache the wrapper", "why is this slow", any request to find or fix
  SkiaSharp managed performance problems.

  For a functional bug use `issue-fix`; for a memory/disposal leak use `memory-leak-fixer`.
---

# Performance Fixer

Proactively **find** and **fix** performance problems in SkiaSharp — a thin managed wrapper over
native Skia, so its recurring, high-impact family is **the managed layer's own overhead between the
caller and Skia**: a P/Invoke transition paid for math that is a few float ops, an allocation on a
hot parse/convert path, a native lookup redone on every getter, per-element marshalling in a loop.
This is *not* about making Skia's C++ rasterizer faster (that is upstream); it is about removing the
tax the C# layer imposes. Every fix is **measured** (a benchmark) and **behaviour-preserving** (an
equivalence test).

**Scope: managed C# only** — `binding/**` and `source/**`. Everything under `externals/skia/**`
(including our C shim) is upstream Skia: out of scope to edit or build, though you **may read** the
pinned source to verify an invariant. Every candidate must be provable and fixable from C#.

Read [`references/decision-framework.md`](references/decision-framework.md) (is it worth it? the
impact×complexity rubric + the two-proof gate) and [`references/measuring.md`](references/measuring.md)
(how to prove faster **and** identical) first — they are the model this skill runs on. Background on
the interop boundary is in [`documentation/dev/memory-management.md`](../../../documentation/dev/memory-management.md)
and [`documentation/dev/architecture.md`](../../../documentation/dev/architecture.md).

## Golden rules (non-negotiable)

1. **One optimization per run.** Pick the single strongest candidate; a perf PR is only reviewable
   as one before/after with one benchmark.
2. **Two proofs, always — speed AND correctness** (details in [measuring.md](references/measuring.md)):
   a BenchmarkDotNet `New` vs `Old` shows a **meaningful, repeatable** speedup with no allocation
   regression; an equivalence test proves the result is **identical to the original/native path**
   (bit-exact for numeric ports) across normal *and* edge inputs. No speedup ⇒ nothing to fix. Any
   behaviour change ⇒ reject — a faster answer that differs from Skia is a **rendering regression**.
3. **Never trade correctness for speed.** No "approximation", no dropped edge case
   (NaN/±0/Inf/degenerate/overflow), no changed rounding, no skipped validation. If the only way
   faster changes what the method returns, **stand down**.
4. **Never weaken, skip, mute, `[Obsolete]`-hide, or delete a test.** If a correctness test goes red,
   fix the change, not the test.
5. **Never edit generated files or upstream Skia.** `*.generated.cs` and `externals/skia/**` are
   off-limits to edit/build. You **may READ** the pinned Skia C++ (fetch at the submodule's pinned
   commit and cite it) to verify an algorithm or pointer-stability invariant.
6. **ABI stability.** Change method **bodies** or add **overloads**; never change/remove a public
   signature. (#4241 changed only bodies; #4345 added `ReadOnlySpan<char>` overloads.)
7. **Float determinism across runtimes.** A managed port of native float math is bit-exact only on
   SSE2/NEON runtimes; **x86 .NET Framework (x87) diverges** — any float port must keep a native
   fallback there (a `RuntimeInformation`-gated `static readonly bool`, as #4241 did). Never ship a
   float port without it.
8. **Honest, numeric scope note.** Report the **actual measured numbers** (Mean/Error/StdDev,
   allocations, ratio) on named hardware/TFM; say what is *empirically measured* vs *statically
   reasoned*, plus ABI impact. Never claim a speedup you did not measure.
9. **Finding nothing is the expected outcome.** SkiaSharp is mature; most obvious overhead is already
   optimized. Most runs should end with **no candidate**. A 2% win on a synthetic micro-loop no real
   caller hits is **not** a finding. A quiet run is a first-class success — emit a `noop`.

## How to use this skill

1. **Decide if it's worth it.** [decision-framework.md](references/decision-framework.md): be
   aggressive with low-complexity wins on hot paths; reserve high-complexity (native-math ports,
   SIMD, caching) for measured cases. Confirm a **realistic hot caller** first.
2. **Reuse before you build.** [repo-helpers.md](references/repo-helpers.md) — a shared helper
   (`Utils.RentArray`, `RentHandlesArray`, `SKString`) or the native oracle may already fit.
3. **Route from the signal.** [signals.md](references/signals.md) maps *what the code does* → the
   hot-path / bcl-pattern reference that covers it.
4. **Prove it.** [measuring.md](references/measuring.md) — both proofs, against this repo's harness.

## The cheap wins (apply by default on hot paths)

Low complexity, high impact. Prefer them whenever you write or touch hot-path code.

- Prefer the span/`Try*` overload over the allocating one; add a `ReadOnlySpan<char>` overload where
  only the `string`/`T[]` one exists (additive, ABI-safe).
- Pre-size and pool: give collections a `capacity`, rent from `Utils.RentArray`/`ArrayPool`.
- `stackalloc` a small, **bounded** buffer instead of allocating (cap the size; never in a loop).
- Cache a stable native wrapper across calls when the four preconditions hold (pointer identity,
  lifetime, disposal invalidation, thread model).
- Size the specialized type: `SearchValues<T>` for repeated set search, `FrozenDictionary` for
  build-once maps.
- Let the JIT help: `sealed` internal types, `[MethodImpl(AggressiveInlining)]` on trivial wrappers,
  `in`/`ref readonly` on large structs (internal / new overloads only), avoid LINQ/boxing in loops.

## Be cautious with (measure first, isolate, keep all TFMs safe)

High complexity — apply only on a **proven** hot path, behind a clean API, with the two proofs. Even
when you recommend the simpler option, report the faster high-complexity one and its tradeoff.

- Porting native float math to managed C# (bit-exact + the x87 fallback).
- Manual SIMD / `Vector128`/`Vector256` (ARM64 NEON `Vector256` was **5.7–6.5× slower** in #4241).
- `unsafe`, raw pointers, `MemoryMarshal.Cast`/`Unsafe.As` reinterpretation.
- Any change to the `HandleDictionary` locking discipline.

## Hot-path references — where the wins live (primary)

Route here from [signals.md](references/signals.md); each file has the *where to look* grep, the
slow→fast, the watch-out, and the real PR.

| SkiaSharp area | Reference |
|---|---|
| Geometry & math (SKMatrix/SKRect/SKPoint native-math ports) | [hot-paths/geometry-math.md](references/hot-paths/geometry-math.md) |
| Color parse/convert (SKColor/SKColorF, span overloads) | [hot-paths/color.md](references/hot-paths/color.md) |
| Handles & collections (getter caching, sizing, HandleDictionary) | [hot-paths/handles-and-collections.md](references/hot-paths/handles-and-collections.md) |
| Text & fonts (glyph loops, HarfBuzz marshalling, shaping memoization) | [hot-paths/text-and-fonts.md](references/hot-paths/text-and-fonts.md) |
| Pixels & images (bitmap/pixmap bulk copy, blittable reinterpret) | [hot-paths/pixels-and-images.md](references/hot-paths/pixels-and-images.md) |

## BCL pattern references — the techniques (foundation)

The general .NET fast-API guidance behind the patterns above, with TFM guards.

| Area | Reference |
|---|---|
| Strings & spans | [bcl-patterns/strings-and-spans.md](references/bcl-patterns/strings-and-spans.md) |
| Numerics, SIMD & codegen | [bcl-patterns/numerics-and-simd.md](references/bcl-patterns/numerics-and-simd.md) |
| Memory & buffers | [bcl-patterns/memory-and-buffers.md](references/bcl-patterns/memory-and-buffers.md) |
| Collections & searching | [bcl-patterns/collections.md](references/bcl-patterns/collections.md) |
| Interop & marshalling | [bcl-patterns/interop-and-marshalling.md](references/bcl-patterns/interop-and-marshalling.md) |

---

## Mode selection

| You were asked to… | Do this |
|---|---|
| Scan **and** fix (the default; what the agentic workflow runs) | Phases 0 → 5 below: hunt → prove faster → implement + prove identical → file the finding + a linked draft PR (`Fixes #…`). |
| Find an opportunity (scan only) / file an issue | Phases 0 → 2, then file a `[performance]` issue with the numbers, **framed as an unvalidated hypothesis** — a benchmarked *proposed* fast path is not yet proof of behaviour parity. Don't use "proven/fixable" language without the Phase 3 parity proof. |
| Author or review perf code interactively (a human is driving) | Route via [signals.md](references/signals.md), apply low-complexity hot-path wins inline, and report medium/high ones with their tradeoff. Still hold the two-proof bar before claiming a win. |

---

## The autonomous workflow (scan → prove → fix → file)

### Phase 0 — Setup
`dotnet cake --target=externals-download` (pre-built natives; you never build native). The benchmark
harness is `benchmarks/SkiaSharp.Benchmarks` — read [`benchmarks/README.md`](../../../benchmarks/README.md)
and copy `Benchmarks/TemplateBenchmark.cs` to start; the test project is `tests/SkiaSharp.Tests.Console`.
See [measuring.md](references/measuring.md) and [repo-helpers.md](references/repo-helpers.md).

### Phase 1 — Scan (find ONE candidate)
**1.1 Pick a focus area (round-robin).** If the run supplies an explicit focus area (a bare number
0–4), use it and skip rotation. Otherwise rotate over the **5 hot-path areas** so consecutive runs
differ:
```bash
DOY=$(date -u +%j); HOUR=$(date -u +%H)          # zero-padded day-of-year + hour
FOCUS=$(( (10#$DOY * 24 + 10#$HOUR) % 5 ))       # 10# forces base-10; 0..4
echo "focus area: $FOCUS"   # 0 geometry-math · 1 color · 2 handles-and-collections · 3 text-and-fonts · 4 pixels-and-images
```
Open that `hot-paths/` reference and its **Where to look** grep. Widen to a neighbour only if it's
exhausted.

**1.2 Establish the hot path and cost** — with `file:line` citations: the realistic caller and how
often it runs; the concrete overhead (which the reference names); and the invariant that makes the
fast path *still correct*. If you can't name that invariant, drop it. Skip anything already optimized
(the references list the hardened spots).

**1.3 De-dup** against open issues/PRs (search the `[performance]` prefix **and** the specific
type/API name — real perf work is often `perf(...)`/`Optimize …`):
```bash
gh issue list --repo "$GITHUB_REPOSITORY" --search '"[performance]" in:title' --state open --json number,title
gh pr    list --repo "$GITHUB_REPOSITORY" --search 'SKMatrix in:title' --state open --json number,title
```
Respect in-flight work (#4241 SKMatrix, #4276/#3699 bench CI, #3489 CopyTo, #4182 dict sizing,
#3033 DrawShapedText). Pick the ONE strongest candidate; if none convinces, **stop** (`noop`).

### Phase 2 — Prove it is faster
Follow [measuring.md](references/measuring.md) §"Proof 1": a `New` vs `Old` benchmark in one process,
`[MemoryDiagnoser]`, realistic workload, statistical rigor (Mean/Error/StdDev, ≥2 runs, no alloc
regression, no regression on any real shape). **No measurable/repeatable win ⇒ not a finding.**

### Phase 3 — Fix + prove identical
Write the equivalence test **first** ([measuring.md](references/measuring.md) §"Proof 2") — full
behaviour parity (return value bit-exact for numeric ports; edge inputs; exceptions/validation;
ownership/`GC.KeepAlive`; rendered pixels), confirmed to catch a deliberately-wrong result. Then
implement the minimal fix using the matching hot-path + bcl-pattern references, honouring that
family's **Watch out** and **all TFMs** (guard newer APIs; a float port keeps the x87 fallback).
Confirm: identical (equivalence passes), faster (benchmark holds), no regressions (type's test class
+ neighbours).

**Self-review gate — before the PR** (all must tick, else fix or `noop`):
- [ ] Real, repeatable speedup outside the error bands, ≥2 runs, no alloc regression, realistic workload.
- [ ] Full behaviour parity proven (value/edges/exceptions/ownership/pixels) and the test catches a
      deliberately-wrong result.
- [ ] Behaviour unchanged; SkiaSharp still renders identically.
- [ ] Fix in `binding/**`/`source/**` only — no `*.generated.cs`, no `externals/skia/**`.
- [ ] No public signature changed (body/additive overload only).
- [ ] All TFMs handled; no ARM64/x86 SIMD regression; float port keeps the x87 fallback.
- [ ] The matching **Watch out** does not describe what you did; not already covered by an open issue/PR.

### Phase 4 — File the finding, then the linked fix PR
Two linked safe outputs so the finding auto-closes on merge:
- **Issue** (`create_issue`, `temporary_id` like `aw_perf1`) — the **hot path + measured cost**
  (family, `file:line`, the realistic caller, the Phase 2 benchmark table, the scope note).
- **PR** (`create_pull_request`, draft, branch `dev/perf-<desc>`) — the fix (what changed + the
  invariant that keeps it correct), **proof faster** (benchmark table + command), **proof identical**
  (the equivalence test + what edges it covers + that it catches a wrong result), and `Fixes #aw_perf1`
  on its own line.
- **Labels** — both the issue and PR carry `tenet/performance`; add the matching **`perf/*`
  sub-type** chosen by the dominant, measured driver of the win (a removed P/Invoke → `perf/interop`,
  removed managed allocations → `perf/allocations`, else `perf/rendering`/`perf/throughput`/
  `perf/startup`/`perf/memory-leak`/`perf/size`). Canonical taxonomy:
  `.agents/skills/issue-triage/references/labels.md`. Usually one sub-type. When run from the agentic
  workflow, its guardrail 8 restates this.
- If the only real win is native/upstream → the **issue alone** (finding + evidence + proposal).

### Phase 5 — Report
Short summary: area, candidate (`file:line`), benchmark result (New vs Old, ratio, allocations),
equivalence coverage, and the issue + PR links — or "no convincing candidate this run". End with the
right safe output: the **issue + PR pair**, the **issue alone** (native/upstream), or a single
**`noop`** (quiet/dry run). Never finish with no safe output.
