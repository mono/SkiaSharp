# Measuring: prove faster AND prove identical

Every SkiaSharp perf fix needs **two proofs**, and this file is how to produce both against this
repo's real harness. Neither alone is sufficient: a benchmark with no parity test can ship a
rendering regression; a parity test with no benchmark can ship a "fix" that isn't faster.

Bootstrap first (managed-C# work uses pre-built natives — see [repo-helpers.md](repo-helpers.md)):

```bash
dotnet cake --target=externals-download
```

## Proof 1 — faster (BenchmarkDotNet, New vs Old, one process)

The harness already exists: **`benchmarks/SkiaSharp.Benchmarks`** (BenchmarkDotNet, references the
local binding, single `$(TFMCurrent)` TFM, `InternalsVisibleTo` so benchmarks can call
`SkiaApi.sk_*`). **Read [`benchmarks/README.md`](../../../../benchmarks/README.md) first** — it is
the on-ramp (conventions + exact run commands). **Copy
[`benchmarks/SkiaSharp.Benchmarks/Benchmarks/TemplateBenchmark.cs`](../../../../benchmarks/SkiaSharp.Benchmarks/Benchmarks/TemplateBenchmark.cs)**
(a working `New` vs `Old` + `[MemoryDiagnoser]` skeleton), rename it, and replace the two method
bodies; or model on `SKColorParseBenchmark`/`SurfaceCanvasBenchmark`/`PaintFastBoundsBenchmark`. The
switcher auto-discovers every `[Benchmark]` class — no registration needed.

```bash
# Discover (fast):
dotnet run -c Release --project benchmarks/SkiaSharp.Benchmarks -- --list flat
# Iterate (fast directional signal — wide CIs, one class, ShortRun):
dotnet run -c Release --project benchmarks/SkiaSharp.Benchmarks -- --filter '*YourBenchmark*' --job short
# Final numbers (default job, run ≥2×):
dotnet run -c Release --project benchmarks/SkiaSharp.Benchmarks -- --filter '*YourBenchmark*'
```
The dominant cost is BenchmarkDotNet building a host per benchmark × `[Params]` combo, so **narrow
the `--filter` and trim `[Params]` while iterating**; widen for the final run. Don't add
cross-runtime `[SimpleJob(RuntimeMoniker.NetXX)]` attributes — they silently skip off-Windows.

House style, from the README and existing benchmarks:

- **`[MemoryDiagnoser]`** on the class — allocations matter as much as time; a big part of many
  wins is going zero-alloc (`Allocated` column). A time win that *adds* allocations is usually a
  net loss under GC.
- **`New` vs `Old` in the same process.** Put the **current shipped behaviour** as the
  `[Benchmark(Baseline = true)]` `Old` method — copy the current implementation *verbatim* inline
  if the fix will replace it (see `SKColorParseBenchmark.OldTryParse`), or call the current native
  path via `SkiaApi.sk_*`. Put the **proposed fast path** as `New`. Same job/TFM ⇒ honest ratio.
- **A realistically shaped scene, not a synthetic tight loop.** `SurfaceCanvasBenchmark` renders a
  varied 1,000-item frame; `PaintFastBoundsBenchmark` scatters items so ~75% are cullable. Shape
  the workload like the real caller (per-frame, per-point batch, startup parse) and use `[Params]`
  to cover sizes/shapes that take different branches.
- **Defeat constant-folding.** A benchmark whose result the JIT folds to a constant measures
  nothing. **Return/consume every result** (return it, or accumulate into a sink), feed inputs via
  `[Params]`/non-`const` fields, and sanity-check the absolute times are plausible for the work.

Read the result with statistical rigor — BenchmarkDotNet noise is the default failure mode:

- Look at the full row: **Mean, Error, StdDev, Ratio, RatioSD.** A "win" whose Error/StdDev bands
  overlap `Old`'s is **within noise — not a finding.**
- **Run it at least twice** (separate processes). If the ratio is not stable, it is noise. Small
  L1-resident arrays and memory-bandwidth-bound loops swing run-to-run (#4241 saw 0.68×↔1.48× for
  identical code at 1,024 points).
- **No allocation regression** (`Allocated`/Gen0 must not rise).
- **No regression on any real shape** (SIMD `Vector256` ran 5.7–6.5× slower on ARM64 NEON in
  #4241 — gate per-runtime or drop it).
- Use `[DisassemblyDiagnoser]` when the change is codegen-sensitive (inlining, `sealed`, bounds
  checks) to confirm the JIT did what you expect.

Record the exact numbers (Mean/Error/StdDev/ratio/allocations), hardware, and TFM — they go
verbatim into the issue/PR.

## Proof 2 — behaviour-identical (equivalence/parity test)

The moment you optimize, the risk is a *different answer*, so the primary safety test asserts
**parity with the original / native path** — not just "the new code returns something sensible".
Add it to the console test project (source under `tests/Tests/`, run via
`tests/SkiaSharp.Tests.Console`). The model is PR #4241's `SKMatrixManagedTests`, which call the
native Skia C API directly and assert bit-equality — that class ships with the SKMatrix PR, so if it
is not yet in the tree, write a new test that calls `SkiaApi.sk_*` directly as the oracle. Minimal
shape:

```csharp
[SkippableFact]
public void ManagedResultMatchesNative()
{
    foreach (var input in EdgeAndRandomInputs())   // incl. NaN/±0/Inf/degenerate/odd counts
    {
        var managed = Subject.FastPath(input);          // the new managed code
        NativeOracle(ref input, out var native);        // SkiaApi.sk_* — internal, via InternalsVisibleTo
        // bit-exact for floats: compare raw bits, not ==, so -0/NaN/Inf are caught
        Assert.Equal(BitConverter.SingleToInt32Bits(native.X), BitConverter.SingleToInt32Bits(managed.X));
        Assert.Equal(BitConverter.SingleToInt32Bits(native.Y), BitConverter.SingleToInt32Bits(managed.Y));
    }
}
```

Parity means more than "same return value" — a binding has observable behaviour beyond the result:

- **Return value** — for numeric ports, **bit-exact** (compare the raw `int`/`uint` bits of each
  `float`, not `==`, so `-0`/`NaN`/`Inf` are caught).
- **Edge inputs**, deliberately: `-0`, `Inf`, `NaN`, `MaxValue`, `Epsilon`, degenerate/
  non-invertible matrices, empty/`#`/named-color strings, odd/small/large batch counts to hit SIMD
  lane loops *and* the scalar tail, in-place (`dst == src`) and overlapping spans. The bugs live in
  the edges; right on the happy path but wrong on `NaN` is still wrong.
- **Exceptions & validation** — same inputs throw the same exception types in the same order (a
  "faster" path that skips a null/range check *changes behaviour*).
- **Side effects & ownership** — same native refcount / `owns:` / `OwnedBy` effects, no changed
  disposal semantics, and `GC.KeepAlive` on any argument whose managed wrapper must outlive the
  native call (a cache/`in`/`ref` refactor can drop a keep-alive and introduce a GC race).
- **Rendered pixels** — for anything reaching drawing (a matrix map feeding `SKCanvas`, a shaping/
  path change), add a **rendered-bitmap parity** check (draw before/after, compare pixel bytes/
  hash). Bit-exact numbers are necessary but a full render is the real proof.

Call the **native oracle directly** (`SkiaApi.sk_*`, reachable via `InternalsVisibleTo`) and assert
the managed result equals it. Confirm the equivalence test **passes against the current tree**
before you change the implementation (proves the oracle/harness is right).

The equivalence test is a **guardrail, not a red→green** — its job is to fail the instant the
*new* code diverges. To prove it genuinely catches divergence, temporarily introduce a deliberate
wrong result and confirm it goes **red**, then remove that.

## After the fix: no regressions

A faster path that changes behaviour is a regression. Rebuild and run the type's existing test
class plus neighbouring consumers (e.g. for an `SKMatrix` change, also `SKPath`/`SKCanvas`/
`SKShader` tests that map through it), and keep the new benchmark in the repo so the win can be
re-measured later.

```bash
dotnet build binding/SkiaSharp/SkiaSharp.csproj
dotnet test tests/SkiaSharp.Tests.Console/SkiaSharp.Tests.Console.csproj --filter "FullyQualifiedName~<YourType>"
```

## Reading native Skia to prove an invariant

Families `geometry-math` and `handles-and-collections` need you to verify an algorithm or a
pointer-stability invariant in the pinned Skia C++. The runner does not check out the submodule, so
read the SHA (`git submodule status externals/skia` or `.gitmodules`) and view the file in
`mono/skia` at that commit, and cite it. This is **read-only** verification — never edit or build
native code.
