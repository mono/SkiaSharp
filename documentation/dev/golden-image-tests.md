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
two Vulkan renderers live under `tests/VulkanTests/` so their Silk.NET dependency
stays out of the shared test assembly. Those sources build into
`SkiaSharp.Vulkan.Tests.Console` for desktop and into the platform-restricted
`SkiaSharp.Vulkan.Tests` library the Android and Windows device hosts register.

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

Visual and sample references use separate roots and one resolver:

```
tests/Goldens/
  Visual/mac/base/visual-diagonal-lines.ganesh-gl.arm64.png
  Visual/mac/base/visual-diagonal-lines.ganesh-gl.png
  Samples/mac/base/blazor-cpu.png
  Visual/portable/base/visual-diagonal-lines.raster.png
  Samples/iphone-16-pro/26.3.1/maui-basic-cpu.png
  Samples/iphone-16-pro/base/gallery-gradient-cpu.png
  Samples/pixel-9/26/gallery-gradient-cpu.x64.png
  Samples/pixel-9/36/gallery-gradient-cpu.png
```

The grammar is `<profile>/<version-or-base>/<case>[.<variant>][.<architecture>].png`.
Profiles describe the actual model/render environment, not Appium, Playwright,
Docker or a test suite. Cases identify semantic output; `visual-*` cases require
an exact backend variant. Functional variant precedes architecture.
Browser samples start engine-neutral; only measured differences justify engine
variants. Case names are lowercase kebab-case without dots.

For runtime `12.6.2`, lookup tries exact `12.6.2`, matching minor `12.6`,
family `12`, then `base`. At each tier, current architecture wins over neutral.
Matching prefix baselines outrank older exact captures. When those are absent,
existing older captures within the same family are tried before `base`.
`12.6` is a minor baseline, distinct from an exact `12.6.0` capture.
`latest` is input-only and must resolve to an actual version before lookup.

| Policy | Numeric fallback boundary |
| --- | --- |
| iOS / modern macOS | Same product major; historical macOS 10.x stays within its minor |
| Android | Same API and registered image flavor; optional platform/image revision tiers |
| Windows | Same Client/Server/Server Core/Nano installation kind and build train, then servicing revisions |
| Linux | Same distro/libc/release series: Ubuntu 22.04, Alpine 3.23, Azure Linux 3.0, Debian 12 |

Generic Windows/Linux profiles hold explicitly approved `base` files.
Specific distro/product profiles may use these portable baselines; numeric
lookup never crosses distro or build families. Visual scenes additionally allow
portable same-renderer references. Nano Server retains an explicit Windows
visual edge because its font scaler differs.

**Never fall back across device models, renderer backends or incompatible
dimensions.** The selected reference is authoritative: corruption, wrong size
or mismatching pixels fail rather than triggering another lookup.
Minimum-version/exact-version barriers invalidate older/base references when
a deliberate rendering contract changes.

`GoldenAssets.targets` copies or embeds the owning root with explicit
`SkiaSharp.Goldens/<key>` names. Disk/resource enumeration uses the same parser,
without authored catalogs or name mangling. Packaging roots do not affect
matching. Validate the source trees:

```sh
python3 scripts/infra/tests/extract-visual-goldens.py --check tests/Goldens/Visual
python3 scripts/infra/tests/extract-visual-goldens.py --check tests/Goldens/Samples
python3 -m unittest discover -s scripts/infra/tests -p test_golden_images.py
```

## Tolerance

`GoldenTolerance` gives each test a `ChannelTolerance` (max per-channel delta)
and a `MaxOutlierFraction` (share of pixels allowed to exceed it).

| Renderer | Tolerance |
|---|---|
| `raster` | `(2, 0.002)` |
| every GPU renderer | `(12, 0.03)` |

Raster is not bit-exact because the shared `portable/` golden is captured on one
architecture and replayed on others. Tolerance is chosen by renderer alone —
`GoldenTolerance.For` sees only the renderer name.

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
# Review output/golden-proposals, then copy approved files to the owning root.
```

For a platform you can't run locally, download that CI leg's published TRX and
export the same way. Exports stage exact-environment proposals, never overwrite
the selected shared baseline. Always review PNGs before committing; never blind-accept a
*mismatch*; that would bless a regression as the new golden.

Promote a reviewed reference to a family tier, `base`, or neutral architecture
only after replaying its supported profiles. Pixel-identical PNGs can have
different encodings; review decoded pixels, not just hashes. Keep demonstrated
overrides. Conflicting captures for one proposed key fail; no last-writer wins
or automatic reference approval.

## Triage

Versioned single-line records work across hosts:

| Marker | Carries |
|---|---|
| `##SKIA-GOLDEN## role=actual` | the rendered PNG |
| `##SKIA-GOLDEN## role=reference` | the selected reviewed reference |
| `##SKIA-GOLDEN## role=diff` | the comparison difference |

Records carry the exact capture `key`, separate `selected` reference key,
dimensions and base64 bytes. Resolution output includes candidate chain/location
on success as well as failure. Samples attach a `.reference.json` run artifact
with hash and comparison paths; it is not a source-authored catalog.

An `always()` CI step runs `extract-visual-goldens.py … --images-out` to decode
them into published test artifacts as browsable PNGs mirroring the canonical
tree, grouped by results file and occurrence. A scene with only an `.actual.png`
had no golden; the test result and its message say whether the rest passed.

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
| `tests/Goldens/{Visual,Samples}/` | committed reference PNGs |
| `tests/Tests/Utils/GoldenReferences.cs` | shared key parser, policies and resolver |
| `tests/GoldenAssets.targets` | copy/embed packaging for the owning root |
| `scripts/infra/tests/extract-visual-goldens.py` | marker → PNG harvest |
