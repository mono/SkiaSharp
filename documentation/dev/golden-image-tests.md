# Golden-image tests (visual regression)

A cross-backend pixel-comparison harness: it renders the same *scene* through
every backend the host can drive and diffs the result against a committed golden
PNG. Each `(renderer × scene)` pair is one xUnit theory case.

It runs in-process inside the test runners we already ship — no separate render
host, no Playwright/`adb`/`simctl` orchestration — so the same matrix compiles
and runs in `SkiaSharp.Tests.Console` (desktop), `SkiaSharp.Tests.Devices` (MAUI)
and `SkiaSharp.Tests.Wasm` (browser).

## Renderers

Three families, all equal citizens:

| Family | Renderers |
|---|---|
| CPU | `raster` |
| Ganesh | `ganesh-gl`, `ganesh-metal`, `ganesh-vulkan` |
| Graphite | `graphite-metal`, `graphite-vulkan`, `graphite-dawn` |

Most live in `Visual/Renderers/` and compile into every host. `ganesh-gl` is
desktop-only (`Renderers/Desktop/`, it needs the `GlContexts` abstraction); the
two Vulkan renderers live in the `SkiaSharp.Vulkan.Tests.Console` satellite so
its Silk.NET dependency stays out of the shared test assembly.

A renderer's `Name` is also its [`GpuPolicy`](gpu-test-policy.md) backend id and
its golden directory name, so one word identifies a backend everywhere.

## Interfaces

```csharp
public interface ISkiaScene
{
    string Name { get; }              // golden file basename
    SKImageInfo Info { get; }         // surface size + pixel format
    void Draw(SKCanvas canvas);
}

public interface IRenderer : IDisposable
{
    string Name { get; }              // golden subfolder + GpuPolicy id
    Task<byte[]> RenderAsync(ISkiaScene scene, SKImageInfo info, CancellationToken ct);
}
```

`RenderAsync` returns RGBA8888 / premultiplied — the one format goldens are
stored and compared in (see `RendererPixels`).

Renderers must be **cheap to construct**: `RendererCatalog` instantiates every
one just to enumerate the matrix. Do the GPU work lazily in `RenderAsync`, never
in a constructor. A renderer must not check the platform and must not catch a
failed bring-up — both belong to the policy.

Each driver runs the renderers declared in *its own* assembly against every
scene, so the base matrix and a satellite never double-run a test.

## How a test runs

1. `GpuPolicy.RequireOrSkip(renderer.Name)` — skip here or commit to rendering.
2. `RenderAsync`. **Any** exception fails the test.
3. Emit *actual*.
4. Look up the golden. None → fail as **unseeded**.
5. Emit *golden* + *diff*, then pass or fail as **mismatch** on tolerance.

So the markers alone say what happened: no *golden* means none was committed, and
a *diff* only exists when there was something to diff against. A pass emits all
three too, so a near-miss can still be eyeballed.

Unseeded is a failure, not a skip: the backend produced pixels, so a green result
would be a coverage hole. That is affordable because the captured PNG is already
in the TRX — harvest it, commit, and the test goes green.

## Goldens

They live in `tests/Content/Goldens/` and ride the existing `Content` pipeline
(copied next to the binary on desktop, embedded as resources on device/browser).

```
tests/Content/Goldens/
  <renderer>.<platform>/<scene>.png   ← per-platform override
  <renderer>/<scene>.png              ← shared across platforms
```

Lookup takes the first hit over `VisualPlatform.Tags` then the shared folder.
Tags are `macos`, `windows`, `linux`, `android`, `ios`, `maccatalyst`, `tvos`,
`browser`, `nanoserver`. Nano Server yields two tags, so it probes
`{renderer}.nanoserver` → `{renderer}.windows` → `{renderer}`: it *is* Windows,
but rasterizes text with FreeType instead of DirectWrite.

Each directory is probed on disk first (build output, `TestConfig.PathRoot`, then
a walk up the source tree so you can edit a golden and re-run without a rebuild),
then as an embedded resource.

**The fallback generalizes over platform only, never over renderer.** Different
backends legitimately differ, so a missing GPU golden must never be satisfied by
the CPU baseline.

## Tolerance

`GoldenTolerance` gives each test a `ChannelTolerance` (max per-channel delta)
and a `MaxOutlierFraction` (share of pixels allowed to exceed it).

| Renderer | Tolerance |
|---|---|
| `raster` | `(2, 0.002)` |
| every GPU renderer | `(12, 0.02)` |

Raster is not bit-exact because the shared `raster/` golden is captured on one
architecture and replayed on others. For a single divergent case add a
`(renderer, scene)` entry to `GoldenTolerance.ByRendererScene` rather than
loosening a whole renderer.

## Running locally

```bash
dotnet cake --target=externals-download
dotnet build tests/SkiaSharp.Tests.Console/SkiaSharp.Tests.Console.csproj -c Release

cd tests/SkiaSharp.Tests.Console/bin/Release/net*/
./SkiaSharp.Tests --filter-trait "Category=Visual"
```

Every test is tagged `Category=Visual`, so `--filter-not-trait "Category=Visual"`
excludes the suite. The Vulkan renderers live in the satellite — build and run
`SkiaSharp.Vulkan.Tests.Console` the same way.

On a mismatch the runner writes `_visualfailures/{renderer}/{scene}.actual.png`
and `.diff.png` next to the binary. In the diff, **red** is over tolerance,
**amber** is a sub-tolerance difference.

## Seeding goldens

There is no record mode and no environment variable. Goldens are harvested from
the markers the run already emits, which is the only channel available on device
and browser hosts:

```bash
# from the build output directory
./SkiaSharp.Tests --filter-trait "Category=Visual" --report-trx --report-trx-filename visual.trx

# from the repo root
python3 scripts/infra/tests/extract-visual-goldens.py path/to/visual.trx --dry-run
python3 scripts/infra/tests/extract-visual-goldens.py path/to/visual.trx
git add tests/Content/Goldens && git commit
```

For a platform you can't run locally, download that CI leg's published TRX and
harvest the same way. Always review harvested PNGs before committing — the
harvest trusts whatever the renderer produced, so never blind-harvest a
*mismatch*; that would bless a regression as the new golden.

To share one golden across platforms, move byte-identical per-platform PNGs up to
`{renderer}/` and delete the copies; the harvest then stops re-creating them.

## Triage

Three single-line markers carry everything needed to triage from a TRX, on every
host:

| Marker | Carries |
|---|---|
| `##SKIA-VISUAL-ACTUAL##` | the rendered PNG |
| `##SKIA-VISUAL-GOLDEN##` | the committed reference PNG, when one exists |
| `##SKIA-VISUAL-DIFF##` | the difference PNG, when a golden exists |

All three share one `path` — the golden key — so the marker name is the only
thing that distinguishes them.

An `always()` CI step runs `extract-visual-goldens.py … --images-out` to decode
them into the published `testlogs_*` artifact as browsable PNGs mirroring the
goldens tree: `{renderer}.{platform}/{scene}.{actual,golden,diff}.png`. A scene
with only an `.actual.png` had no golden; the test result and its message say
whether the rest passed.

## Extending

**A scene:** a public, parameterless `ISkiaScene` in `Visual/Scenes/`. It joins
every renderer's row automatically. Keep it deterministic — no system fonts (load
one from `tests/Content/fonts`), no clock, no randomness.

**A renderer:** a public, parameterless `IRenderer`. Put it in
`Visual/Renderers/` if it needs no extra package; in `Renderers/Desktop/` if it
depends on the desktop `GlContexts`; in a satellite project if it needs a NuGet
dependency that must not reach the shared assembly. Give its `Name` a row in
`GpuPolicy.RequiredOn`, and join the driving test class to the GPU rendering
collection so renderers never run concurrently.

Either way the catalogs discover it, and its first run fails as unseeded until
you harvest the goldens.

## Code map

| Path | What |
|---|---|
| `tests/Tests/SkiaSharp/Visual/` | interfaces, catalogs, `GoldenStore`, `GoldenTolerance`, `VisualPlatform` |
| `Visual/Renderers/`, `Renderers/Desktop/` | shared and desktop-only renderers |
| `Visual/Tests/VisualMatrixTestsBase.cs` | the render/compare pipeline every host shares |
| `tests/VulkanTests/Visual/` | the Vulkan satellite's renderers and driver |
| `tests/Content/Goldens/` | committed reference PNGs |
| `scripts/infra/tests/extract-visual-goldens.py` | marker → PNG harvest |
