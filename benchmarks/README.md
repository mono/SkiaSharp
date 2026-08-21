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
