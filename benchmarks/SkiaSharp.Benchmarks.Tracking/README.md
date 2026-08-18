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
`scripts/infra/perf/benchmarks/track.py`.

## CI

`track-benchmarks.yml` runs daily (and on every PR/push) on Linux, Windows and macOS.
For each OS it benchmarks several **version roles**:

- `nightly` — the newest `-nightly.*` build from the [EAP feed](https://aka.ms/skiasharp-eap/index.json) (the daily trend);
- `curr-stable` / `prev-stable` / `prev-major` — released baselines (same roles as the artifact-size tracker);
- `pr` — a **full source build** (native + managed) of the checkout, so native/C-API
  changes show up in the ⭐ "this PR" column. See the sibling
  `SkiaSharp.Benchmarks.Tracking.Source` project.

History is **persisted to the `aw-data` branch** (via `persist-aw-data.yml`, only on
main/schedule) rather than the Actions cache, so the time-series survives indefinitely.
Each leg reads its existing history from that branch, appends today's point, and
re-uploads it; unchanged released baselines are skipped by a fingerprint check.

Every run produces:

- a Markdown dashboard (time **and** allocations, per OS) in the run summary;
- a `perf-dashboard` artifact — a **self-contained interactive HTML page**
  (rendered from `scripts/infra/perf/templates/dashboard.html` with the data
  embedded) you can download from any run, including a PR run, and open in a browser;
- the same page, committed to the `aw-data` branch, serves as a **live** dashboard
  that reads the latest persisted data straight from that branch.

Results land in `BenchmarkDotNet.Artifacts/results/*-report-full.json`, which
`scripts/infra/perf/benchmarks/track.py` parses.
