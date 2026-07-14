# SkiaSharp.Benchmarks.Tracking

Permanent **regression-tracking** benchmarks for SkiaSharp — distinct from the
sibling `SkiaSharp.Benchmarks` project, which is for local A/B (`Old` vs `New`)
performance-fixer investigations.

The difference matters:

| | `SkiaSharp.Benchmarks` (sibling) | `SkiaSharp.Benchmarks.Tracking` (here) |
|---|---|---|
| Purpose | local A/B while optimizing | permanent time-series, run daily in CI |
| Lifetime | transient — deleted after a fix merges | forever — names are the primary key |
| Builds against | the repo source (project refs + native) | the published **nightly** SkiaSharp NuGet |
| Naming | free to churn | **never rename** (renaming severs history) |

## Rules for benchmarks in this project

- **Never rename** a benchmark class, method, or `[Params]` value once it has
  shipped a data point — the full name (`Namespace.Type.Method(Param: value)`) is
  the key the history is stored under. To change what a benchmark measures, add a
  new one with a new name and delete the old one.
- **Public API only.** This project references the NuGet package, so it cannot
  reach internal `SkiaApi`. Keep benchmarks deterministic and machine-independent
  (fixed seeds, no external fonts/content) so they run identically on every OS.
- Allocations matter as much as time — `MemoryDiagnoser` is enabled centrally in
  `Program.cs`, and the tracker records `Allocated` bytes for every benchmark.

## Run locally

```bash
# fast smoke test (Dry job = 1 op) — confirms the harness + JSON export work
dotnet run -c Release --project benchmarks/SkiaSharp.Benchmarks.Tracking -- --filter '*' --job dry

# real numbers for one benchmark
dotnet run -c Release --project benchmarks/SkiaSharp.Benchmarks.Tracking -- --filter '*CanvasDraw*'
```

Results land in `BenchmarkDotNet.Artifacts/results/*-report-full.json`, which the
CI workflow (`.github/workflows/track-benchmarks.yml`) parses via
`.github/scripts/track-benchmarks.py`.

## CI

`track-benchmarks.yml` runs daily on Linux, Windows and macOS, appends each run to
a per-OS rolling history (kept ~60 days, 10 displayed), and renders a dashboard to
the workflow run summary. It benchmarks the newest `-nightly.*` build from the
[EAP feed](https://aka.ms/skiasharp-eap/index.json), the same source the artifact
size tracker uses.
