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
| Scene | `ISkiaScene` | `Visual/Scenes/` | Deterministic draw op; same bytes every run on a backend. `IsPlatformDependent` marks scenes whose CPU output isn't portable (e.g. `Text`) |
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
                  pass  │  FAIL (golden exists, out of tolerance; writes .actual/.diff PNG + base64)  │  Skip (absent backend OR unseeded cell)
```

---

## The seam (interfaces)

```csharp
namespace SkiaSharp.Tests.Visual;

public interface ISkiaScene
{
    string Name { get; }              // golden file basename
    SKImageInfo Info { get; }         // surface size + pixel format
    bool IsPlatformDependent { get; } // true → never use the shared CPU baseline (e.g. Text)
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
prototype got wrong. A cell may **skip** *only* when there is genuinely nothing
to assert:

- `IRenderer.IsAvailable` is `false` (wrong OS, no GPU wiring for this host), or
- `IRenderer.RenderAsync` throws `RendererUnavailableException` (a runtime probe
  found no device / no driver feature / no context), or
- the renderer ran but **no golden has been recorded yet** for this
  `(renderer, scene)` on this platform — an *unseeded* cell. There is no oracle
  to compare against, so it is loudly skipped (with the path it looked in and how
  to seed it) rather than failed. This is what keeps CI green on platforms that
  can only be seeded on the agent (Linux/Windows/device/browser GPU output) until
  their goldens are recorded.

**Every other outcome is a hard failure:**

- `RenderAsync` throws anything else (including `EntryPointNotFoundException` /
  `MissingMethodException` from a broken binding),
- a golden that **does exist** is out of tolerance.

There is no path that downgrades a real regression to a skip or a warning, and a
golden that exists is *always* compared strictly. In particular, the desktop GL
renderer rethrows `EntryPointNotFoundException` / `MissingMethodException` from
context creation (those mean a broken binding) and only converts a genuine "no
GL context could be created" into a skip.

**Locking coverage in.** An unseeded skip is a transition state, not a hiding
place. Set `SKIASHARP_VISUAL_REQUIRE_GOLDENS=1` to turn every unseeded cell into
a hard failure. CI lanes flip this on **per platform once that platform is
seeded**, so from then on a missing golden means a deleted reference, not an
un-recorded one. The recommended lifecycle is: land the infra (cells skip where
unseeded) → seed a platform's goldens on its agent (see *Recording*) → enable
`SKIASHARP_VISUAL_REQUIRE_GOLDENS` for that platform.

---

## Golden storage and lookup

Goldens live under `tests/Content/Goldens/` so they ride the **existing**
`Content` pipeline: `SkiaSharp.Tests.Console` copies `Content/**` next to the
binary, and `SkiaSharp.Tests.Devices` / `SkiaSharp.Tests.Wasm` embed `Content/**`
as resources. No per-project golden globbing is required.

Layout:

```
tests/Content/Goldens/
  _shared/<scene>.png              ← portable CPU baseline (desktop raster, portable scenes only)
  <renderer>/<scene>.png           ← per-renderer override (platform-independent)
  <renderer>.<platform>/<scene>.png ← per-renderer, per-platform override
```

`<platform>` is a short tag from `VisualPlatform.Tag`: `macos`, `windows`,
`linux`, `android`, `ios`, `maccatalyst`, `tvos`, `browser`.

**Read lookup order** (`GoldenStore.TryLoad`), first hit wins:

1. `Goldens/{renderer}.{platform}/{scene}.png`
2. `Goldens/{renderer}/{scene}.png`
3. `Goldens/_shared/{scene}.png` — **only** when the cell is eligible for the
   shared baseline (see below)

**The `_shared` baseline is deliberately narrow.** A single committed PNG is only
reused across platforms when its bytes are genuinely identical everywhere, which
is true for exactly one combination:

```
renderer == "raster"  &&  !scene.IsPlatformDependent  &&  VisualPlatform.IsDesktop
```

- **GPU renderers never use `_shared`.** GPU output legitimately varies by driver
  and antialiasing, so each GPU cell records a platform-tagged golden. Falling a
  GPU cell back to a CPU baseline would compare apples to oranges and fail every
  non-recording platform.
- **Platform-dependent scenes never use `_shared`.** `ISkiaScene.IsPlatformDependent`
  marks scenes whose CPU output is *not* portable — the `Text` scene is `true`
  because the font scaler/hinting differs by OS, so it records a per-platform
  `raster.{platform}` golden. The geometric scenes are `false`.
- **Device/browser raster never uses `_shared`.** `VisualPlatform.IsDesktop` is
  only `macos`/`windows`/`linux`, so Android/iOS/Mac Catalyst/WASM raster each get
  their own `raster.{platform}` folder (their AA/scaler differs from desktop) —
  matching the prototype's `android-raster` / `ios-raster` / `wasm-raster` split.

So on desktop, the portable geometric scenes share one tolerance-checked CPU
golden across arm64/x64, while `Text`, every GPU cell, and every device/browser
cell carry their own platform-tagged reference.

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
the desktop `_shared` baseline is recorded on one architecture (e.g. macOS arm64)
but compared on others (Linux/Windows x64), and the CPU antialiaser's rounding can
differ by a single level on a few edge pixels across architectures. `(2, 0.002)`
absorbs that without admitting a real geometric regression. If a specific portable
scene proves to diverge by more than this across architectures, give it a
per-platform `raster.{platform}` golden instead of widening the renderer tolerance.

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

There is also a focused Cake target that builds the Console host and runs just the
`Category=Visual` cells (strict — a non-zero exit fails the build), which is what
CI and the seeding workflow drive:

```bash
dotnet cake --target=tests-visual
```

On a failure the runner logs the GOLDEN, ACTUAL, and DIFF images as base64 (decode
with any base64-to-image tool) and writes `_visualfailures/{renderer}/{scene}.actual.png`
and `.diff.png` next to the binary. In the colored diff, **red** = over tolerance,
**amber** = a sub-tolerance difference, dimmed = matching.

### Recording / updating goldens

```bash
# record every cell this host can run, into the source tree:
SKIASHARP_UPDATE_GOLDENS=1 ./SkiaSharp.Tests --filter-trait "Category=Visual"

# or via the focused target (forwards to --updateGoldens):
dotnet cake --target=tests-visual --updateGoldens=true
```

The destination directory follows `SKIASHARP_GOLDEN_SCOPE` (the Cake target
exposes it as `--goldenScope`):

| `SKIASHARP_GOLDEN_SCOPE` | Records into |
|---|---|
| *(unset)* | `_shared/` only for a desktop portable **raster** cell; `{renderer}.{platform}/` for everything else (GPU, `Text`, device/browser) |
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
- **Failure artifacts.** Every .NET Core agent (Windows / macOS / Linux) runs
  `scripts/infra/tests/collect-visual-failures.ps1` after the test step, which
  copies any `_visualfailures/{renderer}/{scene}.actual.png|.diff.png` the matrix
  emitted into the published `testlogs` artifact tree, so a red visual cell is
  triageable straight from the build artifacts (in addition to the base64 PNGs in
  the TRX, which is the only retrieval path on device / browser hosts).

Windows software-Vulkan and the device/browser GPU lanes are seeded/enforced as
those renderers land (see below); until then their cells skip as unseeded.

### Seeding a platform and enforcing it

Goldens for non-recording platforms (Linux/Windows GL+Vulkan, Android, iOS, Mac
Catalyst, WASM) must be recorded **on that platform's agent**, because their bytes
can't be reproduced on a dev macOS box. The per-platform lifecycle is:

1. Land the renderer + CI provisioning. Its cells **skip** as unseeded — CI stays
   green.
2. On that platform's agent, run the matrix once with `SKIASHARP_UPDATE_GOLDENS=1`
   (a temporary record build), download the recorded PNGs from the artifacts, and
   commit them under `tests/Content/Goldens/{renderer}.{platform}/`.
3. Turn on `SKIASHARP_VISUAL_REQUIRE_GOLDENS=1` for that platform's lane. From then
   on the now-committed goldens are strictly compared, and a *missing* one is a
   failure (it means a deleted reference, not an un-recorded one).

### Environment variables

| Variable | Effect |
|---|---|
| `SKIASHARP_UPDATE_GOLDENS=1` | Record mode — write the rendered pixels as the golden instead of comparing |
| `SKIASHARP_GOLDEN_SCOPE` | `shared` / `renderer` / `platform` — where record mode writes (default: auto, per the table above) |
| `SKIASHARP_VISUAL_REQUIRE_GOLDENS=1` | Turn an *unseeded* cell from a skip into a hard failure (per-platform CI enforcement) |

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
Direct3D console project. Each cell's golden is **seeded per platform on its own
agent**; until a platform is seeded its GPU cells skip as unseeded rather than
fail (see *Failure discipline*).

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

- `Visual/Renderers/Desktop/GraphiteVulkanRenderer.cs` (Console desktop, beside
  `GaneshVulkanRenderer`),
- `Visual/Renderers/GraphiteMetalRenderer.cs` (shared + Apple-gated, beside
  `GaneshMetalRenderer`, so it runs on macOS Console *and* the iOS / Mac Catalyst
  device hosts),
- `Content/Goldens/graphite-*.{platform}/*.png` (seeded per platform on its agent).

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
