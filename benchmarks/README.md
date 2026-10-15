# SkiaSharp benchmarks

BenchmarkDotNet micro-benchmarks for the managed SkiaSharp binding. This is the harness the
[`performance-fixer`](../.agents/skills/performance-fixer/SKILL.md) skill uses to **prove a change
is faster** (its companion proof — that the change is *behaviour-identical* — is an equivalence test
in `tests/SkiaSharp.Tests.Console`).

## Prerequisites

Native binaries must exist. For managed-only work, download the pre-built natives once:

```bash
dotnet cake --target=externals-download
```

The project references the **local** binding (`binding/SkiaSharp`), and is listed in
`SkiaSharpAssemblyInfo.cs`'s `InternalsVisibleTo`, so benchmarks can call **internal** `SkiaApi.sk_*`
directly — that is how you measure a native path as the `Old` baseline against a managed `New`.

## Add a benchmark

1. Copy [`Benchmarks/TemplateBenchmark.cs`](SkiaSharp.Benchmarks/Benchmarks/TemplateBenchmark.cs), rename the class.
2. Replace the two method bodies. The switcher auto-discovers every `[Benchmark]` class in the
   assembly — no registration needed.

Look at the existing benchmarks for the real patterns:

- [`SKColorParseBenchmark.cs`](SkiaSharp.Benchmarks/Benchmarks/SKColorParseBenchmark.cs) — `New` (shipped) vs `Old` (the
  pre-fix implementation copied verbatim inline), `[Params]` over input shapes.
- [`SurfaceCanvasBenchmark.cs`](SkiaSharp.Benchmarks/Benchmarks/SurfaceCanvasBenchmark.cs) — a realistically shaped
  render loop (1,000 varied items/frame), not a synthetic tight loop.
- [`PaintFastBoundsBenchmark.cs`](SkiaSharp.Benchmarks/Benchmarks/PaintFastBoundsBenchmark.cs) — isolating a P/Invoke
  crossing's cost.

## Conventions (keep these)

- **`[MemoryDiagnoser]`** on the class — allocations matter as much as time; read the `Allocated`
  column.
- **`New` vs `Old` in one process** — `Old` is `[Benchmark(Baseline = true)]` and holds the
  **current shipped behaviour** (copy it verbatim inline, or call the current native path via
  `SkiaApi.sk_*`); `New` is the proposed fast path. Same job/TFM ⇒ honest `Ratio`.
- **Return a sink** from every `[Benchmark]` so the JIT can't fold the work to a constant and
  report a meaningless ~0 ns. Feed inputs via `[Params]`/fields, not `const`s.
- **Shape the workload like the real caller** (per-frame, per-point batch, startup parse); use
  `[Params]` to cover sizes/shapes that hit different branches.
- **Single TFM.** This project targets `$(TFMCurrent)` only. **Do not add cross-runtime
  `[SimpleJob(RuntimeMoniker.NetXX)]` attributes** — the .NET Framework toolchain only runs on
  Windows (it *silently skips* elsewhere) and other runtime monikers need those runtimes installed.

## Run

```bash
# Discover (fast): list every benchmark
dotnet run -c Release --project benchmarks/SkiaSharp.Benchmarks -- --list flat

# Iterate (fast, directional signal): one class, ShortRun job. Wide confidence intervals —
# use it to see "is New clearly faster than Old?", not for final numbers.
dotnet run -c Release --project benchmarks/SkiaSharp.Benchmarks -- --filter '*MyBenchmark*' --job short

# Final numbers: the default job, run at least twice. Report Mean/Error/StdDev/Ratio/Allocated.
dotnet run -c Release --project benchmarks/SkiaSharp.Benchmarks -- --filter '*MyBenchmark*'
```

Tips:
- Run in **Release**, outside the debugger.
- The dominant cost is BenchmarkDotNet building a host per benchmark × `[Params]` combination, so
  **narrow the `--filter` and trim `[Params]`** while iterating; widen for the final run.
- A win is only real if it is **outside the Mean/Error bands and stable across ≥2 runs**, with **no
  allocation regression**. Small L1-resident arrays and memory-bandwidth-bound loops swing run to
  run — that is noise, not a finding.
- Use `[DisassemblyDiagnoser]` when a change is codegen-sensitive (inlining, `sealed`, bounds
  checks) to confirm the JIT did what you expect.

## Pixel comparer: timing, allocations, and tracking

`PixelComparerComparisonBenchmark` measures the public image/bitmap/pixmap comparison overloads
and comparison with both tolerance and ignore masks. `PixelComparerDiagnosticBenchmark` measures
all four difference-image styles, including an eight-pixel checksum and output disposal. Their shared `PixelComparerFixture`
preallocates deterministic decoded inputs and masks in `GlobalSetup` and releases them in
`GlobalCleanup`; no decode or fixture construction is timed. Unchanged, tolerated, mismatched,
alpha-only, mask-tolerated, and ignored regions coexist in each input. RGB/RGBA mean three/four
selected channels, using BGRA8888 Unpremul inputs and normalization.

Four comparison scenarios (32×32 RGB, 256×256 RGBA, 800×600 RGB, 1920×1080 RGBA) and three diagnostic
scenarios (all except 32×32) make **28 image cases**, not a size/channel/style Cartesian product.
`PixelMismatchBudgetBenchmark` adds three small cases for percentage-boundary arithmetic,
including scientific notation, independently of image scanning.
Timing covers each whole public call, including its internal normalization and native allocations.
Diagnostics also include the checksum's temporary pixmap wrapper and disposal.
`MemoryDiagnoser`'s `Allocated` column counts **managed allocations only**: native pixel/image
buffers are **not counted**, including diagnostic output buffers. These are persistent workload
measurements, not copied `Old` implementations or an assertion that one overload is a baseline.

```bash
dotnet run -c Release --project benchmarks/SkiaSharp.Benchmarks -- --filter '*PixelComparer*' '*PixelMismatchBudget*' --job short --exporters fulljson
```

The command exports full BenchmarkDotNet JSON alongside the normal reports. The PR source
leg of the tracking workflow builds the matching Testing assembly and includes these workloads.
For local source tracking, first build matching source assemblies, then opt into the **same classes** in the
existing tracking source runner (which already exports full JSON):

```bash
dotnet build source/SkiaSharp.Testing/SkiaSharp.Testing.csproj -c Release \
  -p:BasicTargetFrameworks=net10.0 -p:AllTargetFrameworks=net10.0 -p:BuildingInsideVisualStudio=true
dotnet run -c Release --project benchmarks/SkiaSharp.Benchmarks.Tracking.Source \
  -p:IncludePixelComparisonBenchmarks=true -- --filter '*PixelComparer*' '*PixelMismatchBudget*'
```

The source runner uses its configured in-process job; do not add `--job short`, which also
creates an out-of-process job that cannot reuse the source-only build properties.
Use the default job and repeat runs for final numbers. The experimental API opt-in is explicit
(`NoWarn=SKIA4001`). Tracking is **source-only**: SkiaSharp.Testing is not yet published and cannot
run against historical core packages. The package tracking project is unchanged, so these cases
appear in PR source results, not the persisted daily package history yet.
