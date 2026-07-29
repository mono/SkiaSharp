# Golden-image tests (visual regression)

A cross-backend pixel-comparison harness that renders the same *scene* through
every available SkiaSharp *backend* and diffs the result against a committed
golden PNG. Each `(renderer × scene)` pair is one xUnit theory case.

The harness lives in the main test suite under
`tests/Tests/SkiaSharp/Visual/` and runs **in-process inside the test runners we
already ship** — there is no separate render-host app and no
Playwright/`adb`/`simctl` orchestration. Because the portable code is linked by
the shared `SkiaSharp.Tests` project, the same matrix compiles and runs in:

- `SkiaSharp.Tests.Console` — desktop (Windows / macOS / Linux), including the
  desktop GPU renderers.
- `SkiaSharp.Tests.Devices` — MAUI device tests (Android / iOS / Mac Catalyst /
  Windows).
- `SkiaSharp.Tests.Wasm` — browser (WebAssembly).

Each renderer declares the GPU backend it drives; whether that backend runs on
this host is decided centrally by `GpuPolicy` (see
[GPU policy](#gpu-policy)). A cell skips only when the policy says
the backend is not required here — never because a bring-up threw. The harness is
built on existing primitives — `SKPixelComparer`, the `GlContexts/` abstraction,
`TestConfig`, and the `Content` embed/copy pipeline — rather than reinventing
them.

---

## Concepts

| Piece | Type | Where | Role |
|---|---|---|---|
| Scene | `ISkiaScene` | `Visual/Scenes/` | Deterministic draw op; same bytes every run on a backend |
| Renderer | `IRenderer` | `Visual/Renderers/` (+ `Renderers/Desktop/`) | Renders a scene through one backend, returns RGBA8888 / premultiplied pixels |
| Scene catalog | `SceneCatalog` | `Visual/` | Reflection-discovers every public parameterless `ISkiaScene` |
| Renderer catalog | `RendererCatalog` | `Visual/` | Reflection-discovers every public parameterless `IRenderer` |
| Matrix test | `VisualMatrixTests` | `Visual/Tests/` | `[Theory]` over the full catalog product; emits the capture + compares to golden |
| Comparison | `SKPixelComparer` (extended) | `tests/Tests/Utils/` | Tolerance-aware per-channel diff + colored diff image |
| Tolerance policy | `GoldenTolerance` | `Visual/` | Per-renderer + per-(renderer, scene) tolerance |
| Golden I/O | `GoldenStore` | `Visual/` | Resolves and loads goldens (read-only); encodes captured pixels to PNG |
| Goldens | PNG files | `tests/Content/Goldens/` | `{renderer}.{platform}/` per-platform override, `{renderer}/` shared-across-platforms golden |

```
ISkiaScene.Draw(canvas) ─▶ IRenderer.RenderAsync ─▶ byte[] RGBA8888/Premul
                                                        │
              emit ##SKIA-GOLDEN-IMAGE## marker (PNG) into the test results (TRX)
                                                        │
   GoldenStore.TryLoad: {renderer}.{platform} ▸ {renderer}
                                                        │
                             SKPixelComparer.Compare(golden, actual, tolerance)
                                                       │
   pass  │  FAIL (out of tolerance, OR unseeded — captured PNG is in the TRX)  │  Skip (backend genuinely absent)
```

---

## The seam (interfaces)

```csharp
namespace SkiaSharp.Tests.Visual;

public interface ISkiaScene
{
    string Name { get; }              // golden file basename
    SKImageInfo Info { get; }         // surface size + pixel format
    void Draw(SKCanvas canvas);
}

public interface IRenderer : IDisposable
{
    string Name { get; }              // golden subfolder + GpuPolicy id, e.g. "ganesh-metal"
    Task<byte[]> RenderAsync(ISkiaScene scene, SKImageInfo info, CancellationToken ct);
}
```

`RenderAsync` returns pixels normalized to **RGBA8888 / premultiplied** — the
single format every golden is stored and compared in (see `RendererPixels`).

Renderers must be **cheap to construct**: the catalog instantiates every
renderer just to enumerate the matrix, so a constructor must not bring up a GPU
context. Do heavy work lazily inside `RenderAsync`.

A renderer never gates itself on the platform, and never catches a failed
bring-up. Its `Name` *is* the `GpuPolicy` backend id, and the policy owns the
"which OS has which API" table.

---

## Failure discipline

A cell may **skip** only when `GpuPolicy` says the renderer's backend is not
required on this host — either the platform is not in the backend's `RequiredOn`
set, or it was opted out for this agent with `SKIASHARP_TEST_SKIP_GPU`. Both are
declared; neither is inferred from an exception.

**Every other outcome is a hard failure:**

- `RenderAsync` throws **anything at all** — a missing device, an absent driver
  or ICD, no display, a null context, or a broken binding,
- a golden that **does exist** is out of tolerance,
- the renderer ran but **no golden has been recorded yet** for this
  `(renderer, scene)` on this platform — an *unseeded* cell.

A renderer must not catch a failed bring-up and turn it into a skip. If a CI
agent legitimately can't run a backend, that belongs in the policy table or in
`SKIASHARP_TEST_SKIP_GPU` where it is reviewable — not in a catch block where it
silently erodes coverage.

An unseeded cell is likewise a **failure, not a skip**. The backend was available
and produced pixels, so a green result would be a coverage hole. There is no
silent "skip until someone records a golden" state to hide a regression in.

This is safe to enforce because **every cell publishes its rendered PNG into the
test results on pass *and* fail** (the `##SKIA-GOLDEN-IMAGE##` marker, see below).
So an unseeded cell fails loudly *and* hands you exactly the bytes to commit:
harvest the marker from the TRX, commit it, re-run, and the cell goes green. No
second "record" run is needed.

---

## GPU policy

`GpuPolicy` decides whether a backend must work on this host. It is the only
place in the suite allowed to skip a GPU test, and it is used by the plain GPU
tests (`SKTest.CreateGlContext`, `VKTest`, `Direct3DTest`, the Graphite release
tests) as well as the visual matrix.

A backend is **required** unless one of two things is true:

| Not required because | Example | Configured? |
|---|---|---|
| the platform is not in its `RequiredOn` set | Metal on Windows, Vulkan on macOS | no |
| it was disabled for this agent | `ganesh-gl` on a headless CI agent | **yes** |

> The table describes **platforms**; the environment variable describes
> **agents**. You never set anything to make Metal skip on Windows.

| Id | RequiredOn | Absent elsewhere because |
|---|---|---|
| `raster` | all | — (CPU, always required) |
| `ganesh-gl` | Windows, macOS, Linux, Nano Server | no `GlContext` for the device/browser hosts |
| `ganesh-vulkan` | Windows, Linux, Android | not built for Apple, Nano Server or the browser |
| `graphite-vulkan` | Windows, Linux, Android | not built for Apple, Nano Server or the browser |
| `ganesh-vulkan-sharpvk` | Windows | SharpVk cannot create a context off Windows |
| `ganesh-metal` | Apple | Metal is an Apple-only API |
| `graphite-metal` | Apple | Metal is an Apple-only API |
| `ganesh-direct3d` | Windows | Direct3D is Windows-only; not built for Nano Server |
| `graphite-dawn` | Browser | Dawn is only built for WebAssembly |

`RequiredOn` mirrors the `gn` args in `native/*/build.cake` — `skia_use_metal`,
`skia_use_vulkan`, `skia_use_dawn`, `skia_use_direct3d`. **Keep the two in sync.**
Nano Server is its own platform because `native/nanoserver/build.cake` passes
`supportVulkan=false` and `supportDirect3D=false`.

### Opting out

```bash
SKIASHARP_TEST_SKIP_GPU=ganesh-gl,graphite-dawn   # specific backends
SKIASHARP_TEST_SKIP_GPU=all                       # every GPU backend
```

Comma, semicolon or whitespace separated, case-insensitive. **An unrecognised id
is a hard error**, so a typo cannot quietly leave a backend required.

Device and browser hosts never see the agent environment, so the same list is
baked into `runtimeconfig.json` from the `SkiaSharpTestSkipGpu` MSBuild property
and read through `AppContext`:

```bash
dotnet cake --target=tests-android --skipGpu=ganesh-vulkan
```

In CI each opt-out is a bootstrapper `env:` value in
`scripts/azure-templates-stages-test.yml`, so what a leg can do is visible in the
pipeline definition:

| Leg(s) | Opted out | Why |
|---|---|---|
| iOS | `ganesh-metal`, `graphite-metal` | Runs the *simulator*, whose virtualized Metal leaves dispatch-queue state that hangs the test host on shutdown. macOS and Mac Catalyst drive real Metal on the same agent and stay required. |
| Windows (.NET Framework + .NET Core) | `ganesh-gl` | No GPU driver, so Windows falls back to GDI generic OpenGL 1.1 with no `WGL_ARB_*` extensions. Vulkan and Direct3D stay required. |
| Azure Linux, Alpine (+ NoDeps ×2), Nano Server | `ganesh-gl` | No X server, no Mesa, no ICD in those images. |

### Known gap: software OpenGL on Windows

`ganesh-gl` on the Windows agents is a provisioning gap, not a platform limit.
Mesa's llvmpipe fixes it with **no test-code change**: drop its `opengl32.dll` +
`libgallium_wgl.dll` into `System32`/`SysWOW64` (the shape `install-vulkan-icd.ps1`
already uses) and set `GALLIUM_DRIVER=llvmpipe`. Mesa's WGL extension string
statically advertises both `WGL_ARB_pixel_format` and `WGL_ARB_pbuffer`, and its
`wglChoosePixelFormatARB` reports `WGL_FULL_ACCELERATION_ARB` — exactly what
`WglContext` requires.

The blocker is provenance: `Silk.NET` ships no desktop-GL package (and its
`Silk.NET.OpenGLES.ANGLE.Native` has 32-bit binaries in `runtimes/win-x64/` in
every published version), and the community NuGet alternatives are incomplete.
Mirroring the upstream `mesa-dist-win` MSVC release into a trusted feed unblocks
it.

### Adding a backend

1. Add a const to `GpuBackends` and a row to `GpuPolicy`'s `requiredOn` table.
2. Point the tests at it — an `IRenderer` whose `Name` is that id, or
   `GpuPolicy.RequireOrSkip` at the top of a bring-up helper.
3. Run it. On the platforms in `RequiredOn` it is required, so an unseeded golden
   or a failed bring-up is red until you seed or fix it.

There is no path that downgrades a real regression to a skip or a warning, and a
golden that exists is *always* compared strictly.

---

## Seeding goldens (harvest from the test results)

There is **no in-process record mode and no environment variable**. Goldens are
seeded from the **captured PNGs the matrix already emits into the test results**:

1. Every cell writes a single-line marker into the test log on pass *and* fail:

   ```
   ##SKIA-GOLDEN-IMAGE## path={renderer}.{platform}/{scene}.png size=WxH base64=<png bytes>
   ```

   The bytes are base64 (no whitespace, XML-safe) so the marker survives intact on
   one line inside a `.trx`.

2. Run the matrix with a TRX report, then harvest the markers into the goldens
   tree and commit:

   ```bash
   # from the build output directory:
   ./SkiaSharp.Tests --filter-trait "Category=Visual" --report-trx --report-trx-filename visual.trx

   # from the repo root, harvest every marker in the TRX into tests/Content/Goldens:
   python3 scripts/infra/tests/extract-visual-goldens.py path/to/visual.trx
   git add tests/Content/Goldens && git commit
   ```

The first run of a new cell **fails** (unseeded); after the harvest+commit it
compares strictly and goes green. The same flow works on **every host**: the TRX
is the one output channel that exists uniformly on desktop, MAUI device, and WASM
hosts — including the device/browser hosts where the filesystem is
sandboxed/embedded and an in-process write-to-source-tree is impossible. That is
why a captured-image marker, not a disk write, is the seed channel.

The harvest writes the per-platform path (`{renderer}.{platform}/`) by default. To
**share one golden across platforms**, move byte-identical per-platform PNGs up to
the platform-portable `{renderer}/` folder and delete the per-platform copies; the
harvest then *skips* re-creating a per-platform file whenever the captured bytes
are byte-identical to an existing `{renderer}/` golden, so the promotion sticks
across future harvests. A genuine per-platform divergence has different bytes and
is still written as a `{renderer}.{platform}/` override.

Always review harvested PNGs before committing — the harvest trusts whatever the
renderer produced.

---

## Golden storage and lookup

Goldens live under `tests/Content/Goldens/` so they ride the **existing**
`Content` pipeline: `SkiaSharp.Tests.Console` copies `Content/**` next to the
binary, and `SkiaSharp.Tests.Devices` / `SkiaSharp.Tests.Wasm` embed `Content/**`
as resources. No per-project golden globbing is required.

Layout:

```
tests/Content/Goldens/
  <renderer>.<platform>/<scene>.png  ← per-platform override (this OS/driver diverges)
  <renderer>/<scene>.png             ← the renderer's golden, shared across platforms
```

`<platform>` is a short tag from `VisualPlatform.Tag`: `macos`, `windows`,
`linux`, `android`, `ios`, `maccatalyst`, `tvos`, `browser`.

**Read lookup order** (`GoldenStore.Candidates` / `TryLoad`), first hit wins:

1. `Goldens/{renderer}.{platform}/{scene}.png` — the per-platform override.
2. `Goldens/{renderer}/{scene}.png` — the renderer's platform-portable golden.

**The fallback generalizes over *platform* only, never over *renderer*.** That is
the one safe generalization: the same backend rendering the same scene produces
the same bytes on every OS *for the common cases* — CPU raster and software GL are
deterministic across OSes and architectures — so one committed `{renderer}/` PNG
can serve every platform. When a particular OS/driver genuinely diverges (a
hardware GPU's antialiasing, a platform font scaler for the `Text` scene), that
platform gets a `{renderer}.{platform}/` override that wins over the shared golden.

The harness **never** falls back from one renderer to another. Different backends
legitimately differ (GPU AA vs. CPU AA), so a missing GPU golden must never be
satisfied by the CPU baseline — that would compare apples to oranges and hide a
real regression. (This is why there is no cross-renderer `_shared` folder: the
shared layer is *per renderer*, at `{renderer}/`.)

So in practice the portable geometric scenes keep a single `raster/` golden shared
across `macos`/`linux`/`windows`, while a hardware GPU cell, or a font-sensitive
scene, carries its own `{renderer}.{platform}/` reference.

Each candidate directory is probed **on disk first** — the build-copied runtime
folder, then `TestConfig.PathRoot/Content`, then a walk up the source tree so the
inner loop can edit a golden and re-run without a rebuild — and then as an
**embedded resource** (device / browser hosts where the filesystem copy isn't
available).

---

## Tolerance

Comparison uses the tolerance-aware `SKPixelComparer.Compare(golden, actual,
channelTolerance)` overload, which counts a pixel as mismatched only when its
largest per-channel delta (including alpha) exceeds `channelTolerance`, and
reports the maximum observed delta. `GoldenTolerance` supplies, per cell:

- `ChannelTolerance` — max allowed absolute per-channel delta, and
- `MaxOutlierFraction` — fraction of pixels allowed to exceed it.

Defaults (`GoldenTolerance.For`):

| Renderer | ChannelTolerance | MaxOutlierFraction |
|---|---|---|
| `raster` | 2 | 0.002 |
| `ganesh-gl`, `ganesh-metal`, `ganesh-vulkan`, `direct3d` | 12 | 0.02 |

The `raster` tolerance is intentionally just above bit-exact (rather than `0`):
the platform-portable `raster/` golden is captured on one architecture (e.g. macOS
arm64) but compared on others (Linux/Windows x64), and the CPU antialiaser's
rounding can differ by a single level on a few edge pixels across architectures.
`(2, 0.002)` absorbs that without admitting a real geometric regression. If a
specific portable scene proves to diverge by more than this across architectures,
give it a per-platform `raster.{platform}` golden instead of widening the renderer
tolerance.

Software-driver GPU cells (CI Mesa GL / Lavapipe Vulkan) can be tightened toward
the deterministic end. Add a per-(renderer, scene) override in
`GoldenTolerance.ByRendererScene` for an individually divergent cell (e.g. the
`Text` scene on a particular backend) instead of loosening a whole renderer.

---

## Running locally

Bootstrap natives once (C#-only change → pre-built natives are fine):

```bash
dotnet cake --target=externals-download
```

Build and run just the matrix in the desktop host:

```bash
dotnet build tests/SkiaSharp.Tests.Console/SkiaSharp.Tests.Console.csproj -c Release

# from the build output directory:
cd tests/SkiaSharp.Tests.Console/bin/Release/net*/
./SkiaSharp.Tests --filter-trait "Category=Visual"
```

Every matrix cell carries `[Trait("Category", "Visual")]`, so the whole suite can
be steered from one switch:

| Goal | Flag |
|---|---|
| Run **only** the visual matrix | `--filter-trait "Category=Visual"` |
| Run everything **except** the visual matrix | `--filter-not-trait "Category=Visual"` |
| Run one renderer/scene cell | `--filter-class "SkiaSharp.Tests.Visual.Tests.VisualMatrixTests"` then inspect `--list-tests` |

The matrix is ordinary shared test code, so it also runs as part of an unfiltered
test run and inside the existing CI stages — there is no dedicated visual build
target.

The base Console host runs the `raster`, `ganesh-gl`, and `ganesh-metal` cells.
`ganesh-vulkan` lives in the Vulkan satellite, so run it from there:

```bash
dotnet build tests/SkiaSharp.Vulkan.Tests.Console/SkiaSharp.Vulkan.Tests.Console.csproj -c Release
cd tests/SkiaSharp.Vulkan.Tests.Console/bin/Release/net*/
./SkiaSharp.Vulkan.Tests --filter-trait "Category=Visual"
```

(On a box with no Vulkan ICD — e.g. a stock macOS agent — those cells skip with a
reason; on a Linux/Windows agent with a driver or software ICD they render and, on
the first run, fail as unseeded until their goldens are harvested.)

On a failure the runner logs the GOLDEN, ACTUAL, and DIFF images as base64 (decode
with any base64-to-image tool) and, on desktop, writes
`_visualfailures/{renderer}/{scene}.actual.png` and `.diff.png` next to the binary.
In the colored diff, **red** = over tolerance, **amber** = a sub-tolerance
difference, dimmed = matching. (Every cell *also* logs its rendered PNG as a
`##SKIA-GOLDEN-IMAGE##` marker regardless of pass/fail — that is the seed channel,
see below.)

### Seeding / updating goldens

Goldens are seeded by harvesting the captured PNGs from the test results, not by a
record mode — see [*Seeding goldens*](#seeding-goldens-harvest-from-the-test-results)
above. The short version:

```bash
# 1. run with a TRX report (from the build output directory):
./SkiaSharp.Tests --filter-trait "Category=Visual" --report-trx --report-trx-filename visual.trx

# 2. harvest the markers into tests/Content/Goldens and commit (from the repo root):
python3 scripts/infra/tests/extract-visual-goldens.py path/to/visual.trx --dry-run   # preview
python3 scripts/infra/tests/extract-visual-goldens.py path/to/visual.trx             # write
git add tests/Content/Goldens && git commit
```

A new cell **fails** as unseeded on its first run; after the harvest+commit it
compares strictly. This is the *only* seeding path — there is no
`SKIASHARP_UPDATE_GOLDENS` env var and no `tests-visual` record target. It works
identically on desktop, device, and browser hosts because the TRX, not the
filesystem, carries the captured image. Always review harvested PNGs before
committing.

---

## Hosting and project wiring

- **Portable files** (interfaces, catalogs, `VisualMatrixTestsBase`,
  `VisualMatrixTests`, scenes, `RasterRenderer`, `GaneshMetalRenderer`,
  `GoldenStore`, `GoldenTolerance`, `VisualPlatform`, `RendererPixels`,
  `GpuRenderGate`) live where the shared `SkiaSharp.Tests` project compiles them,
  so they run in Console, Devices, and Wasm. `GaneshMetalRenderer` is
  portable-but-Apple-gated (the policy marks Metal `Unsupported` off Apple), so the
  same file gives macOS Console *and* the iOS / Mac Catalyst device hosts their Metal
  cell with no per-host code. `VisualMatrixTestsBase` carries the whole per-cell
  pipeline (render → emit `##SKIA-GOLDEN-IMAGE##` → compare-or-fail) so every host
  shares one engine; `VisualMatrixTests` is the thin `[Theory]` over every renderer
  auto-discovered in the base assembly.
- **Desktop-only renderers** (`Renderers/Desktop/GaneshGlRenderer.cs`) depend on
  the desktop `GlContexts/` implementations, so they are **excluded from the shared
  project** the same way `GlContexts/*` already is:

  ```xml
  <!-- tests/SkiaSharp.Tests/SkiaSharp.Tests.csproj -->
  <Compile Include="..\Tests\**\*.cs"
           Exclude="..\Tests\SkiaSharp\GlContexts\*\**;..\Tests\SkiaSharp\Visual\Renderers\Desktop\**" ... />
  ```

  `SkiaSharp.Tests.Console` includes everything (no exclusion), so the desktop GL
  renderer compiles and runs there. GL needs no extra NuGet package (it reuses the
  in-repo `GlContexts/` abstraction), so it stays in the base Console host.
- **Package-dependent GPU renderers live in their satellite host project, not the
  base host.** A backend that needs an extra NuGet — Vulkan (`SharpVk`), Direct3D
  (Vortice) — would otherwise drag that dependency into the base test assembly that
  the MAUI device and WASM builds consume. SkiaSharp already ships dedicated
  satellites for exactly these (`SkiaSharp.Vulkan.Tests.Console`,
  `SkiaSharp.Direct3D.Tests.Console`); each references the base Console project,
  adds only its own GPU package, and is already built and run by CI
  (`tests-netcore`). So `GaneshVulkanRenderer.cs` lives in the Vulkan satellite
  (`tests/VulkanTests/Visual/`), beside a **thin** test class:

  ```csharp
  // tests/VulkanTests/Visual/VulkanVisualTests.cs
  public class VulkanVisualTests : VisualMatrixTestsBase
  {
      [Theory, MemberData(nameof(Matrix))]
      public Task RenderMatchesGolden(string r, string s) =>
          RunCellAsync(RendererCatalog.Get(r), SceneCatalog.Get(s));

      public static IEnumerable<object[]> Matrix() // renderers declared in THIS assembly × scenes
      {
          foreach (var r in RendererCatalog.NamesIn(Assembly.GetExecutingAssembly()))
              foreach (var s in SceneCatalog.AllNames)
                  yield return new object[] { r, s };
      }
  }
  ```

  xUnit only discovers tests compiled into the assembly it runs, so the base
  `VisualMatrixTests` does **not** re-run in the satellite (it is only referenced).
  `RendererCatalog.NamesIn(thisAssembly)` filters the catalog to the renderers the
  satellite compiles in (today just `ganesh-vulkan`), so the shared raster / GL /
  Metal cells are never double-run. Dropping another Vulkan-family renderer into the
  satellite (e.g. Graphite's) makes it join automatically — no edit to the test
  class.

Because the matrix is ordinary shared test code, it runs inside the **existing**
CI stages (`tests-netcore`, `tests-android`, `tests-ios`, `tests-maccatalyst`,
`tests-wasm`) — there is no dedicated visual stage. The Vulkan and Direct3D
satellites are part of the `tests-netcore` project list, so their visual cells run
on the same Win/macOS/Linux agents.

### Continuous integration

The matrix ships wired into CI as part of `scripts/azure-templates-stages-test.yml`:

- **Software GPU on the Linux .NET Core agent.** The Linux `netcore` job installs
  `xvfb mesa-utils libgl1-mesa-dri mesa-vulkan-drivers vulkan-tools`, starts a
  virtual X server, and exports the env that pins GL/Vulkan to Mesa's software
  rasterizers (`LIBGL_ALWAYS_SOFTWARE=1`, llvmpipe, and the lavapipe
  `VK_ICD_FILENAMES`). That gives `ganesh-gl` / `ganesh-vulkan` deterministic
  output on a headless agent. This provisioning is **required**, not best-effort:
  `ganesh-gl` and `ganesh-vulkan` are required on Linux, so if any piece is missing
  or misconfigured the GPU cells go **red**. That is deliberate — the previous
  fail-safe behaviour meant a broken provisioning step silently dropped GPU
  coverage and nobody noticed. (A useful side effect: the existing `GRContextTest`
  / `GRGlInterfaceTest` GL tests also exercise llvmpipe.)

  This provisioning is also what lets a GPU cell be *seeded* on CI: with the
  software ICDs present the cell actually renders and emits its
  `##SKIA-GOLDEN-IMAGE##` marker, which the published TRX carries back for
  harvesting.
- **Declared GPU opt-outs.** Legs that genuinely cannot run a backend declare it
  with the bootstrapper's `env:` parameter, e.g. `SKIASHARP_TEST_SKIP_GPU: ganesh-gl`
  on the container legs, so what a leg can and cannot do is visible in the
  pipeline definition rather than inferred at runtime. Skips land in the TRX with
  their reason. See [GPU policy](#gpu-policy) for the current set.
- **Failure / capture artifacts.** Every cell's rendered PNG is in the published
  TRX as a `##SKIA-GOLDEN-IMAGE##` marker (on pass and fail). A failing cell
  additionally emits its golden and colored diff as `##SKIA-VISUAL-IMAGE##`
  markers, and every cell emits a `##SKIA-VISUAL-CELL## … outcome={pass|mismatch|unseeded}`
  marker recording its verdict. So a new platform's goldens are seeded by
  downloading its TRX and running the harvest script — no extra collection step.
- **Browsable failure images.** An `always()` post-test step in
  `azure-templates-stages-test.yml` runs
  `extract-visual-goldens.py … --failures-out output/logs/testlogs/visual-failures`,
  which decodes the markers from the just-produced TRX into the published
  `testlogs_*` artifact as ordinary PNGs, grouped by outcome:
  `visual-failures/unseeded/{renderer}.{platform}/{scene}.actual.png` (harvest it)
  and `visual-failures/mismatch/{renderer}.{platform}/{scene}.{actual,golden,diff}.png`
  (investigate it). It reads the TRX — the one channel present on every host — so it
  works uniformly on desktop, device, and WASM, and the `outcome` tag keeps an
  unseeded cell (seed its golden) from being confused with a regression (which must
  never be blindly harvested, or it would bless the bad pixels as the new golden).

The device/browser GPU lanes are seeded as those renderers land (see below); until
a cell is seeded it **fails** as unseeded, which is the intended signal to harvest
and commit its golden.

### Seeding a platform from CI

Goldens for platforms you can't run on a dev box (Linux/Windows GL+Vulkan,
Android, iOS, Mac Catalyst, WASM) are seeded from that platform's CI run, because
their bytes can't be reproduced locally. The per-platform lifecycle is:

1. Land the renderer + (for desktop GPU) the software-ICD provisioning. On the
   first run the platform's cells **fail** as unseeded — that is the signal, and
   the failing run's TRX already contains the captured PNGs.
2. Download that lane's published TRX and harvest it into the goldens tree:

   ```bash
   python3 scripts/infra/tests/extract-visual-goldens.py path/to/downloaded.trx
   ```

   Review the new `tests/Content/Goldens/{renderer}.{platform}/*.png` and commit.
3. Re-run. The now-committed goldens are strictly compared; a *missing* golden for
   an available backend is a failure (a deleted reference), and a changed pixel is
   a regression. No flag flip is needed — strict comparison is always on.

To share one golden across platforms once they agree, promote byte-identical
per-platform PNGs up to `{renderer}/` (see
[*Seeding goldens*](#seeding-goldens-harvest-from-the-test-results)); the harvest
then stops re-creating the per-platform copies.

---

## Backend coverage

Which renderers a host *compiles* is a matter of project references; which of
those are **required** to pass is decided by
[`GpuPolicy`](#gpu-policy), whose table is the authoritative
version of this list.

| Host | Platform | raster | GPU renderers compiled in |
|---|---|---|---|
| Console | macOS | ✓ | `ganesh-gl` (CGL), `ganesh-metal`, `graphite-metal` |
| Console | Linux | ✓ | `ganesh-gl` (GLX/EGL, Mesa sw) |
| Console | Windows | ✓ | `ganesh-gl` (WGL) |
| Vulkan satellite | Windows / Linux | — | `ganesh-vulkan`, `graphite-vulkan` (SwiftShader / Lavapipe sw) |
| Direct3D satellite | Windows | — | `ganesh-direct3d` (Vortice) |
| Devices | iOS / Mac Catalyst | ✓ | `ganesh-metal`, `graphite-metal` |
| Devices | Android | ✓ | `ganesh-vulkan`, `graphite-vulkan` |
| Wasm | browser | ✓ | `graphite-dawn` |

> The "Vulkan satellite" is `SkiaSharp.Vulkan.Tests.Console`; it runs on the same
> `tests-netcore` Windows/macOS/Linux agents as the base Console, just from its own
> assembly so the `Silk.NET`/`SharpVk` dependency stays out of the shared test code.
> Direct3D (`SkiaSharp.Direct3D.Tests.Console`) is the same pattern. Both are also
> referenced by the Android and Windows device hosts where applicable.

`raster`, `ganesh-metal` and `graphite-metal` run from shared code and compile
into every host; the policy marks Metal `Unsupported` off Apple, so it lights up
on macOS Console and the iOS / Mac Catalyst device hosts alike with no per-host
code. The desktop GL renderer is Console-only.

Each cell's golden is **seeded per platform from its own CI run** (harvest the
TRX); until a cell is seeded it **fails** as unseeded — the captured PNG is in the
TRX, so harvesting and committing it closes the gap (see *Failure discipline* and
*Seeding goldens*).

---

## How to extend

**Add a scene:** drop a public, parameterless `ISkiaScene` under
`Visual/Scenes/`. It appears in every renderer's column automatically. Keep it
deterministic — no system fonts (load one from `tests/Content/fonts`), no clock,
no randomness. On its first run each new cell fails as unseeded; harvest the
captured PNGs from the TRX and commit them (see *Seeding goldens*).

**Add a portable renderer:** drop a public, parameterless `IRenderer` under
`Visual/Renderers/`. It appears in every scene's row of the shared matrix
automatically. Use this only for backends that need no extra NuGet package and are
safe in the MAUI/WASM builds (e.g. an Apple-gated Metal renderer).

**Declare its backend:** a renderer's `Name` is its
[`GpuPolicy`](#gpu-policy) backend id, and that id needs a row in the
policy table giving the platforms it must work on. The renderer itself
must not check the platform and must not catch a failed bring-up — both belong in
the policy.

**Add a desktop GL/Metal renderer:** put it under `Visual/Renderers/Desktop/`
(excluded from the shared project) so it compiles only into `SkiaSharp.Tests.Console`.
Acquire the GPU context through `TestConfig` / `GlContexts` rather than a bespoke
loader, and hold `GpuRenderGate.Sync` while touching the GPU so cells don't race
the driver.

**Add a package-dependent renderer (Vulkan / Direct3D / Graphite):** put the
renderer in the matching satellite host project (`SkiaSharp.Vulkan.Tests.Console`
→ `tests/VulkanTests/`, `SkiaSharp.Direct3D.Tests.Console`) so its NuGet dependency
never reaches the base test assembly. If that satellite already has a
`*VisualTests : VisualMatrixTestsBase` driver (Vulkan does), the renderer is
discovered by `RendererCatalog.NamesIn(thisAssembly)` and joins automatically —
nothing else to write. For a satellite that has none yet (Direct3D), add a ~15-line
driver mirroring `VulkanVisualTests`. Acquire the GPU context from the satellite's
existing helper (`VkContext` / Vortice device) and hold `GpuRenderGate.Sync`.

---

## The Graphite seam

This harness is designed so the in-flight Graphite backend PR (#3968) rebases
onto it by **adding renderer classes and golden PNGs only** — no test, csproj, or
CI changes. Concretely, that PR adds:

- `tests/VulkanTests/Visual/GraphiteVulkanRenderer.cs` in the
  `SkiaSharp.Vulkan.Tests.Console` satellite, beside `GaneshVulkanRenderer`. The
  satellite's `VulkanVisualTests` discovers it via
  `RendererCatalog.NamesIn(thisAssembly)`, so it joins that satellite's matrix with
  no test-class edit,
- `Visual/Renderers/GraphiteMetalRenderer.cs` (shared + Apple-gated, beside
  `GaneshMetalRenderer`, so it runs on macOS Console *and* the iOS / Mac Catalyst
  device hosts and is auto-discovered by the base `VisualMatrixTests`),
- `Content/Goldens/graphite-*.{platform}/*.png` (seeded per platform by harvesting its CI TRX).

The catalogs auto-discover both. Because the seam uses clean names on main rather
than mirroring the prototype, the Graphite renderer files take a small (~5-line)
rebase edit:

- implement **`SkiaSharp.Tests.Visual.IRenderer`** (`Name`, `Backend`,
  `RenderAsync(scene, info, ct)` returning RGBA8888/Premul
  via `RendererPixels.ReadRgba`),
- acquire the GPU device/context from the shared **`TestConfig` / `GlContexts`**
  providers (or the satellite's existing `VkContext` / `GRSharpVkBackendContext`)
  instead of the prototype's `VulkanLoader` / `WglLoader` / `EglLoader`,
- compare via the committed goldens (handled by the harness) instead of an inline
  `ComputeDiff`,
- drop the prototype's out-of-process host sessions and `VisualFactAttribute`
  opt-in gate (the matrix runs by default).
