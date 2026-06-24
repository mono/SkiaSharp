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

Each renderer's `IsAvailable` reflects what *that* host can do; cells that can't
run skip with a reason. The harness is built on existing primitives —
`SKPixelComparer`, the `GlContexts/` abstraction, `TestConfig`, and the `Content`
embed/copy pipeline — rather than reinventing them.

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
    string Name { get; }              // golden subfolder, e.g. "ganesh-metal"
    bool IsAvailable { get; }         // cheap, side-effect-free probe
    string UnavailableReason { get; } // why IsAvailable is false (else null)
    Task<byte[]> RenderAsync(ISkiaScene scene, SKImageInfo info, CancellationToken ct);
}
```

`RenderAsync` returns pixels normalized to **RGBA8888 / premultiplied** — the
single format every golden is stored and compared in (see `RendererPixels`).

Renderers must be **cheap to construct**: the catalog instantiates every
renderer just to enumerate the matrix, so a constructor must not bring up a GPU
context. Do heavy work lazily inside `RenderAsync`, and keep `IsAvailable` a
metadata-only probe.

---

## Failure discipline

This is the property the harness exists to guarantee, and the thing the prior
prototype got wrong. A cell may **skip** *only* when the backend is genuinely
absent on this host — there is nothing to render, so nothing to assert:

- `IRenderer.IsAvailable` is `false` (wrong OS, no GPU wiring for this host), or
- `IRenderer.RenderAsync` throws `RendererUnavailableException` (a runtime probe
  found no device / no driver feature / no context).

**Every other outcome is a hard failure:**

- `RenderAsync` throws anything else (including `EntryPointNotFoundException` /
  `MissingMethodException` from a broken binding),
- a golden that **does exist** is out of tolerance,
- the renderer ran but **no golden has been recorded yet** for this
  `(renderer, scene)` on this platform — an *unseeded* cell.

The last point is the deliberate difference from the prototype: an unseeded cell
is a **failure, not a skip**. The backend was available and produced pixels, so a
green result would be a coverage hole. There is no silent "skip until someone
records a golden" state to hide a regression in.

This is safe to enforce because **every cell publishes its rendered PNG into the
test results on pass *and* fail** (the `##SKIA-GOLDEN-IMAGE##` marker, see below).
So an unseeded cell fails loudly *and* hands you exactly the bytes to commit:
harvest the marker from the TRX, commit it, re-run, and the cell goes green. No
second "record" run is needed.

There is no path that downgrades a real regression to a skip or a warning, and a
golden that exists is *always* compared strictly. In particular, the desktop GL
renderer rethrows `EntryPointNotFoundException` / `MissingMethodException` from
context creation (those mean a broken binding) and only converts a genuine "no
GL context could be created" into a skip.

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

- **Portable files** (interfaces, catalogs, `VisualMatrixTests`, scenes,
  `RasterRenderer`, `GaneshMetalRenderer`, `GoldenStore`, `GoldenTolerance`,
  `VisualPlatform`, `RendererPixels`, `GpuRenderGate`) live where the shared
  `SkiaSharp.Tests` project compiles them, so they run in Console, Devices, and
  Wasm. `GaneshMetalRenderer` is portable-but-Apple-gated (`IsAvailable` is true
  only on Apple OSes), so the same file gives macOS Console *and* the iOS / Mac
  Catalyst device hosts their Metal cell with no per-host code.
- **Desktop-only renderers** (`Renderers/Desktop/GaneshGlRenderer.cs`,
  `Renderers/Desktop/GaneshVulkanRenderer.cs`) depend on the desktop
  `GlContexts/` implementations / desktop-only native loaders, so they are
  **excluded from the shared project** the same way `GlContexts/*` already is:

  ```xml
  <!-- tests/SkiaSharp.Tests/SkiaSharp.Tests.csproj -->
  <Compile Include="..\Tests\**\*.cs"
           Exclude="..\Tests\SkiaSharp\GlContexts\*\**;..\Tests\SkiaSharp\Visual\Renderers\Desktop\**" ... />
  ```

  `SkiaSharp.Tests.Console` includes everything (no exclusion), so the desktop
  GL/Vulkan renderers compile and run there.
- Host-specific test projects (e.g. the separate Direct3D console project) can
  contribute their own renderer in the **entry** assembly: `CatalogReflection`
  scans the defining assembly ∪ the entry assembly, so those renderers are
  discovered without the shared catalog referencing them.

Because the matrix is ordinary shared test code, it runs inside the **existing**
CI stages (`tests-netcore`, `tests-android`, `tests-ios`, `tests-maccatalyst`,
`tests-wasm`) — there is no dedicated visual stage.

### Continuous integration

The matrix ships wired into CI as part of `scripts/azure-templates-stages-test.yml`:

- **Software GPU on the Linux .NET Core agent.** The Linux `netcore` job installs
  `xvfb mesa-utils libgl1-mesa-dri mesa-vulkan-drivers vulkan-tools`, starts a
  virtual X server, and exports the env that pins GL/Vulkan to Mesa's software
  rasterizers (`LIBGL_ALWAYS_SOFTWARE=1`, llvmpipe, and the lavapipe
  `VK_ICD_FILENAMES`). That gives `ganesh-gl` / `ganesh-vulkan` deterministic
  output on a headless agent. Selection is **fail-safe**: if any piece is missing
  or misconfigured, context creation throws `RendererUnavailableException` and the
  GPU cell skips — it never turns a provisioning gap into a red build. (A useful
  side effect: the existing `GRContextTest` / `GRGlInterfaceTest` GL tests, which
  otherwise skip for lack of a display, now also exercise llvmpipe.)

  This provisioning is also what lets a GPU cell be *seeded* on CI: with the
  software ICDs present the cell actually renders and emits its
  `##SKIA-GOLDEN-IMAGE##` marker, which the published TRX carries back for
  harvesting. Without them the cell would skip and emit nothing.
- **Failure / capture artifacts.** Every cell's rendered PNG is in the published
  TRX as a `##SKIA-GOLDEN-IMAGE##` marker (on pass and fail), and a failing
  desktop cell additionally logs base64 GOLDEN/ACTUAL/DIFF images. So a red visual
  cell is triageable straight from the test results, and a new platform's goldens
  are seeded by downloading its TRX and running the harvest script — no extra
  collection step in the pipeline.

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

| Host | Platform | raster | GPU |
|---|---|---|---|
| Console | macOS | ✓ | `ganesh-gl` (CGL), `ganesh-metal` (in-process) |
| Console | Linux | ✓ | `ganesh-gl` (GLX/EGL, Mesa sw), `ganesh-vulkan` (Lavapipe sw) |
| Console | Windows | ✓ | `ganesh-gl` (WGL), `ganesh-vulkan` (ICD if present) |
| Devices | iOS / Mac Catalyst | ✓ | `ganesh-metal` (shared Apple-gated renderer) |
| Devices | Android | ✓ | per-host GLES / Vulkan — follow-up |
| Wasm | browser | ✓ | WebGL2 — follow-up |

`raster` and `ganesh-metal` run from shared code (Metal is gated to Apple OSes, so
it lights up on macOS Console and the iOS / Mac Catalyst device hosts alike). The
desktop GL/Vulkan renderers are Console-only. `direct3d` is a later addition in the
Direct3D console project. Each cell's golden is **seeded per platform from its own
CI run** (harvest the TRX); until a cell is seeded it **fails** as unseeded — the
captured PNG is in the TRX, so harvesting and committing it closes the gap (see
*Failure discipline* and *Seeding goldens*).

---

## How to extend

**Add a scene:** drop a public, parameterless `ISkiaScene` under
`Visual/Scenes/`. It appears in every renderer's column automatically. Keep it
deterministic — no system fonts (load one from `tests/Content/fonts`), no clock,
no randomness. On its first run each new cell fails as unseeded; harvest the
captured PNGs from the TRX and commit them (see *Seeding goldens*).

**Add a portable renderer:** drop a public, parameterless `IRenderer` under
`Visual/Renderers/`. It appears in every scene's row automatically.

**Add a desktop / host-specific renderer:** put it under
`Visual/Renderers/Desktop/` (excluded from the shared project) or in the
host-specific console project (discovered via the entry assembly). Acquire the
GPU context through `TestConfig` / `GlContexts` rather than a bespoke loader, and
hold `GpuRenderGate.Sync` while touching the GPU so cells don't race the driver.

---

## The Graphite seam

This harness is designed so the in-flight Graphite backend PR (#3968) rebases
onto it by **adding renderer classes and golden PNGs only** — no test, csproj, or
CI changes. Concretely, that PR adds:

- `Visual/Renderers/Desktop/GraphiteVulkanRenderer.cs` (Console desktop, beside
  `GaneshVulkanRenderer`),
- `Visual/Renderers/GraphiteMetalRenderer.cs` (shared + Apple-gated, beside
  `GaneshMetalRenderer`, so it runs on macOS Console *and* the iOS / Mac Catalyst
  device hosts),
- `Content/Goldens/graphite-*.{platform}/*.png` (seeded per platform by harvesting its CI TRX).

`RendererCatalog` auto-discovers them. Because the seam uses clean names on main
rather than mirroring the prototype, the Graphite renderer files take a small
(~5-line) rebase edit:

- implement **`SkiaSharp.Tests.Visual.IRenderer`** (`Name`, `IsAvailable`,
  `UnavailableReason`, `RenderAsync(scene, info, ct)` returning RGBA8888/Premul
  via `RendererPixels.ReadRgba`),
- acquire the GPU device/context from the shared **`TestConfig` / `GlContexts`**
  providers (or the existing `VkContext`) instead of the prototype's
  `VulkanLoader` / `WglLoader` / `EglLoader`,
- compare via the committed goldens (handled by the harness) instead of an inline
  `ComputeDiff`,
- drop the prototype's out-of-process host sessions and `VisualFactAttribute`
  opt-in gate (the matrix runs by default).
