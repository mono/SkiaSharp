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
| Matrix test | `VisualMatrixTests` | `Visual/Tests/` | `[Theory]` over the full catalog product; compares to golden |
| Comparison | `SKPixelComparer` (extended) | `tests/Tests/Utils/` | Tolerance-aware per-channel diff + colored diff image |
| Tolerance policy | `GoldenTolerance` | `Visual/` | Per-renderer + per-(renderer, scene) tolerance |
| Golden I/O | `GoldenStore` | `Visual/` | Resolves, loads, records, and saves goldens / failure artifacts |
| Goldens | PNG files | `tests/Content/Goldens/` | `{renderer}.{platform}/` and `{renderer}/` overrides, `_shared/` fallback |

```
ISkiaScene.Draw(canvas) ─▶ IRenderer.RenderAsync ─▶ byte[] RGBA8888/Premul
                                                        │
   GoldenStore.TryLoad: {renderer}.{platform} ▸ {renderer} ▸ _shared
                                                        │
                              SKPixelComparer.Compare(golden, actual, tolerance)
                                                        │
                  pass  │  FAIL (writes .actual/.diff PNG + base64)  │  Skip (absent backend)
```

---

## The seam (interfaces)

```csharp
namespace SkiaSharp.Tests.Visual;

public interface ISkiaScene
{
    string Name { get; }       // golden file basename
    SKImageInfo Info { get; }  // surface size + pixel format
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
absent on the host:

- `IRenderer.IsAvailable` is `false` (wrong OS, no GPU wiring for this host), or
- `IRenderer.RenderAsync` throws `RendererUnavailableException` (a runtime probe
  found no device / no driver feature / no context).

**Every other outcome is a hard failure:**

- `RenderAsync` throws anything else (including `EntryPointNotFoundException` /
  `MissingMethodException` from a broken binding),
- the rendered pixels are out of tolerance, or
- no golden exists for the cell.

There is no path that downgrades a real regression to a skip or a warning. In
particular, the desktop GL renderer rethrows `EntryPointNotFoundException` /
`MissingMethodException` from context creation (those mean a broken binding) and
only converts a genuine "no GL context could be created" into a skip.

---

## Golden storage and lookup

Goldens live under `tests/Content/Goldens/` so they ride the **existing**
`Content` pipeline: `SkiaSharp.Tests.Console` copies `Content/**` next to the
binary, and `SkiaSharp.Tests.Devices` / `SkiaSharp.Tests.Wasm` embed `Content/**`
as resources. No per-project golden globbing is required.

Layout:

```
tests/Content/Goldens/
  _shared/<scene>.png              ← portable CPU baseline (raster)
  <renderer>/<scene>.png           ← per-renderer override (platform-independent)
  <renderer>.<platform>/<scene>.png ← per-renderer, per-platform override (GPU)
```

`<platform>` is a short tag from `VisualPlatform.Tag`: `macos`, `windows`,
`linux`, `android`, `ios`, `maccatalyst`, `browser`.

**Read lookup order** (`GoldenStore.TryLoad`), first hit wins:

1. `Goldens/{renderer}.{platform}/{scene}.png`
2. `Goldens/{renderer}/{scene}.png`
3. `Goldens/_shared/{scene}.png`

Each candidate directory is probed **on disk first** — the build-copied runtime
folder, then `TestConfig.PathRoot/Content`, then a walk up the source tree so the
inner loop can edit a golden and re-run without a rebuild — and then as an
**embedded resource** (device / browser hosts where the filesystem copy isn't
available).

Why per-platform GPU folders: CPU raster output is bit-deterministic and shared
across every OS, but GPU output legitimately varies by driver and antialiasing
implementation, so each GPU renderer records a platform-tagged golden.

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
| `raster` | 1 | 0.0 |
| `ganesh-gl`, `ganesh-metal`, `ganesh-vulkan`, `direct3d` | 12 | 0.02 |

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
./SkiaSharp.Tests --filter-class "SkiaSharp.Tests.Visual.Tests.VisualMatrixTests"
```

On a failure the runner logs the GOLDEN, ACTUAL, and DIFF images as base64 (decode
with any base64-to-image tool) and writes `_visualfailures/{renderer}/{scene}.actual.png`
and `.diff.png` next to the binary. In the colored diff, **red** = over tolerance,
**amber** = a sub-tolerance difference, dimmed = matching.

### Recording / updating goldens

```bash
# record every cell this host can run, into the source tree:
SKIASHARP_UPDATE_GOLDENS=1 ./SkiaSharp.Tests --filter-class "SkiaSharp.Tests.Visual.Tests.VisualMatrixTests"
```

The destination directory follows `SKIASHARP_GOLDEN_SCOPE`:

| `SKIASHARP_GOLDEN_SCOPE` | Records into |
|---|---|
| *(unset)* | `_shared/` for `raster`, `{renderer}.{platform}/` for everything else |
| `shared` | `_shared/` |
| `renderer` | `{renderer}/` |
| `platform` | `{renderer}.{platform}/` |

Recording requires a writable source tree, so it is a desktop-Console operation.
Recording on a device / browser host fails loudly rather than silently no-op'ing.
Always review recorded PNGs before committing — recording trusts whatever the
renderer produced.

---

## Hosting and project wiring

- **Portable files** (interfaces, catalogs, `VisualMatrixTests`, scenes,
  `RasterRenderer`, `GoldenStore`, `GoldenTolerance`, `VisualPlatform`,
  `RendererPixels`, `GpuRenderGate`) live where the shared `SkiaSharp.Tests`
  project compiles them, so they run in Console, Devices, and Wasm.
- **Desktop-only renderers** (`Renderers/Desktop/GaneshGlRenderer.cs`,
  `Renderers/Desktop/GaneshMetalRenderer.cs`) depend on the desktop
  `GlContexts/` implementations / desktop P/Invoke, so they are **excluded from
  the shared project** the same way `GlContexts/*` already is:

  ```xml
  <!-- tests/SkiaSharp.Tests/SkiaSharp.Tests.csproj -->
  <Compile Include="..\Tests\**\*.cs"
           Exclude="..\Tests\SkiaSharp\GlContexts\*\**;..\Tests\SkiaSharp\Visual\Renderers\Desktop\**" ... />
  ```

  `SkiaSharp.Tests.Console` includes everything (no exclusion), so the desktop
  GPU renderers compile and run there.
- Host-specific test projects (e.g. the separate Vulkan / Direct3D console
  projects) can contribute their own renderer in the **entry** assembly:
  `CatalogReflection` scans the defining assembly ∪ the entry assembly, so those
  renderers are discovered without the shared catalog referencing them.

Because the matrix is ordinary shared test code, it runs inside the **existing**
CI stages (`tests-netcore`, `tests-android`, `tests-ios`, `tests-maccatalyst`,
`tests-wasm`) — there is no dedicated visual stage. Desktop GPU cells run by
default; CI Linux/Windows .NET agents should be provisioned with software ICDs
(Mesa GL, Lavapipe Vulkan) for deterministic output, and the failure-PNG
directory published as a build artifact.

---

## Backend coverage

| Host | Platform | raster | GPU |
|---|---|---|---|
| Console | macOS | ✓ | `ganesh-gl` (CGL), `ganesh-metal` (in-process) |
| Console | Linux | ✓ | `ganesh-gl` (GLX/EGL), `ganesh-vulkan` (Lavapipe) |
| Console | Windows | ✓ | `ganesh-gl` (WGL), `ganesh-vulkan` |
| Devices | Android / iOS / Mac Catalyst | ✓ | per-host GPU — follow-up |
| Wasm | browser | ✓ | WebGL2 — follow-up |

`raster` runs in every host (it is portable shared code). The desktop GPU
renderers are Console-only. `ganesh-vulkan` is wired through the separate Vulkan
console project (it reuses that project's `VkContext`); `direct3d` is a later
addition in the Direct3D console project.

---

## How to extend

**Add a scene:** drop a public, parameterless `ISkiaScene` under
`Visual/Scenes/`. It appears in every renderer's column automatically. Keep it
deterministic — no system fonts (load one from `tests/Content/fonts`), no clock,
no randomness. Record its goldens and commit them.

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

- `Visual/Renderers/Desktop/GraphiteVulkanRenderer.cs` (or in the Vulkan project),
- `Visual/Renderers/Desktop/GraphiteMetalRenderer.cs`,
- `Content/Goldens/graphite-*.{platform}/*.png`.

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
