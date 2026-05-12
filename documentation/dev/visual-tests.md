# Visual tests

A cross-backend pixel-comparison harness that renders the same scene through every backend SkiaSharp ships (raster, Ganesh-GL, Ganesh-Vulkan, Ganesh-Metal, Graphite-Vulkan, Graphite-Metal, plus the WASM, Android, and iOS variants of those) and diffs the result against a golden PNG. Each cell is one xUnit theory case.

These tests are **opt-in**. A plain `dotnet test` skips all of them — they need a published WASM payload, a built Android APK, a booted simulator, etc., and on a machine without that infrastructure the matrix is mostly skips. Turn them on with `SKIASHARP_VISUAL_TESTS=1`.

The gate is a custom xUnit attribute (`[VisualTheory]` / `[VisualFact]`, defined in `tests/Tests/SkiaSharp/Visual/VisualFactAttribute.cs`) that checks the env var at discovery time. Without the env var the unrun matrix collapses to a single skipped theory in the test report rather than 42 individual skip lines.

---

## TL;DR

```bash
# Run every visual cell that can run on this host.
SKIASHARP_VISUAL_TESTS=1 \
  dotnet test tests/SkiaSharp.Tests.Console -c Release \
  --filter "FullyQualifiedName~VisualMatrixTests"

# Update goldens (per-renderer override) for a specific cell.
SKIASHARP_VISUAL_TESTS=1 \
SKIASHARP_UPDATE_GOLDENS=1 SKIASHARP_GOLDEN_SCOPE=renderer \
  dotnet test tests/SkiaSharp.Tests.Console -c Release \
  --filter "DisplayName~android-raster"
```

Without `SKIASHARP_VISUAL_TESTS=1`, every cell skips with:

> Visual tests are opt-in. Set SKIASHARP_VISUAL_TESTS=1 to enable them.

---

## The matrix

| Renderer | Backend | Hosts on |
|---|---|---|
| `raster` | CPU | any |
| `ganesh-gl` | Ganesh + desktop GL (EGL/WGL) | Linux, Windows |
| `ganesh-vulkan` | Ganesh + Vulkan | Linux, Windows, *macOS (needs MoltenVK)* |
| `ganesh-metal` | Ganesh + Metal | macOS |
| `graphite-vulkan` | Graphite + Vulkan | Linux, Windows, *macOS (needs MoltenVK)* |
| `graphite-metal` | Graphite + Metal | macOS |
| `wasm-raster` | CPU, in browser via WASM host | any host with Playwright + Chromium |
| `wasm-ganesh-gles` | Ganesh + WebGL2 | any host with Playwright + Chromium |
| `wasm-graphite-dawn` | Graphite + WebGPU | any host with `--headless=new` Chromium |
| `android-raster` | CPU, on Android emulator | any host with `adb` + emulator |
| `android-ganesh-vulkan` | Ganesh + Vulkan, on Android emulator | any host with `adb` + emulator |
| `ios-raster` | CPU, on iOS Simulator | macOS only (uses `xcrun simctl`) |
| `ios-ganesh-metal` | Ganesh + Metal, on iOS Simulator | macOS only |
| `ios-graphite-metal` | Graphite + Metal, on iOS Simulator | macOS only |

Scenes today (`tests/Tests/SkiaSharp/Visual/Scenes/`): `DiagonalLines`, `FilledCircle`, `RedRoundedRectOnWhite`. Every renderer runs every scene — there is no Caps/Requires filter. Cells that can't run on this host skip cleanly with a reason (no Vulkan, no booted simulator, no APK, …).

---

## Per-host setup

A cell with no infrastructure for it (no booted iOS Simulator, no published WASM, …) **skips** with a descriptive message; it does not fail. Set up only what you want to cover.

### Linux

Required for `ganesh-gl`, `ganesh-vulkan`, `graphite-vulkan`, `wasm-*`:

| Cell group | Setup |
|---|---|
| `raster` | nothing |
| `ganesh-gl` | `libEGL.so.1` from `mesa-utils` / `libegl1` |
| `ganesh-vulkan`, `graphite-vulkan` | Lavapipe — see [graphite-headless.md](graphite-headless.md) |
| `wasm-*` | `dotnet publish tests/Hosts/RenderHost.Wasm -c Release` + Playwright (`pwsh tests/SkiaSharp.Tests.Console/bin/Release/net10.0/playwright.ps1 install chromium`) |
| `android-*` | `dotnet build tests/Hosts/RenderHost.Android -c Release` + `adb` on PATH + booted emulator |

### Windows

Same as Linux except `ganesh-gl` uses **WGL** (no extra deps — `opengl32.dll` ships with Windows). For `wasm-graphite-dawn` use full Chromium (`--headless=new`); legacy headless-shell doesn't expose WebGPU.

### macOS

| Cell group | Setup |
|---|---|
| `raster` | nothing |
| `ganesh-metal`, `graphite-metal` | nothing (Metal framework is on every Mac) |
| `ganesh-vulkan`, `graphite-vulkan` | not currently supported — needs MoltenVK + a SkiaSharp built with `skia_use_vulkan=true` for macOS |
| `wasm-*` | as Linux |
| `ios-*` | `dotnet build tests/Hosts/RenderHost.iOS -c Release` + `xcrun simctl boot <device-udid>` (or open Simulator.app) |

---

## Golden files

```
tests/Tests/SkiaSharp/Visual/Goldens/
├── _shared/                       # canonical baseline (used by default)
│   ├── DiagonalLines.png
│   ├── FilledCircle.png
│   └── RedRoundedRectOnWhite.png
├── android-raster/                # per-renderer overrides
│   └── ...
├── ios-raster/
│   └── ...
└── ...
```

**Lookup is per-renderer first, shared as fallback.** A test for `(android-raster, FilledCircle)` first checks `Goldens/android-raster/FilledCircle.png`; if that doesn't exist it falls back to `Goldens/_shared/FilledCircle.png`.

Comparison tolerance is **`MaxChannelDelta = 2`** (any single R/G/B/A channel can differ by up to 2/255). That rides out 1-bit rounding between deterministic software ICDs while still flagging real regressions. Two pixels that disagree by 3+ in any channel = failure.

### When does a renderer need its own override?

When the renderer produces pixels that legitimately differ from the shared baseline beyond the tolerance:

- **CPU vs GPU AA on curves.** `raster` and `wasm-raster` use different edge-coverage math than GPU backends — circle/rounded-rect edges diverge by ~30–50 channel-delta on a handful of pixels.
- **Different software Vulkan ICDs.** Lavapipe vs SwiftShader (Android emulator) produce different AA paths.
- **Browser GL/WebGPU drivers.** Headless Chromium's ANGLE-on-SwiftShader is yet another software stack.

These are not bugs — they're documented platform differences. Record an override and move on.

### Recording goldens

`SKIASHARP_UPDATE_GOLDENS=1` flips the harness into write mode. The scope env var picks the destination:

| Scope | Destination |
|---|---|
| (unset) — default | `_shared/<scene>.png` — only do this for a brand-new scene |
| `renderer` | `<renderer>/<scene>.png` — per-renderer override |

The harness only writes when the cell **diverges from the existing golden**. Cells that already match aren't touched, so you don't accidentally produce redundant overrides.

```bash
# New scene → record the shared baseline once, against a known-good renderer.
SKIASHARP_VISUAL_TESTS=1 SKIASHARP_UPDATE_GOLDENS=1 \
  dotnet test tests/SkiaSharp.Tests.Console -c Release \
  --filter "DisplayName~ganesh-vulkan.*MyNewScene"

# Legitimate per-renderer divergence → record an override.
SKIASHARP_VISUAL_TESTS=1 SKIASHARP_UPDATE_GOLDENS=1 SKIASHARP_GOLDEN_SCOPE=renderer \
  dotnet test tests/SkiaSharp.Tests.Console -c Release \
  --filter "DisplayName~android-raster"
```

When a cell fails, the actual + diff PNGs are written to `tests/SkiaSharp.Tests.Console/bin/Release/net10.0/_failures/<renderer>/<scene>.{actual,diff}.png` for inspection.

---

## Adding a new scene

1. Implement `ISkiaScene` in `tests/Tests/SkiaSharp/Visual/Scenes/`:

   ```csharp
   public sealed class MyScene : ISkiaScene
   {
       public string Name => "MyScene";
       public SKImageInfo SuggestedInfo =>
           new SKImageInfo (256, 256, SKColorType.Rgba8888, SKAlphaType.Premul);
       public void Draw (SKCanvas canvas) { /* ... */ }
   }
   ```

2. Compile-include it into the out-of-process hosts that need it. Each `tests/Hosts/RenderHost.*` csproj has a block of `<Compile Include="..\..\Tests\SkiaSharp\Visual\Scenes\..." />` lines — add yours next to the others.

3. Record a shared golden from a deterministic renderer (Lavapipe is the canonical choice):

   ```bash
   SKIASHARP_VISUAL_TESTS=1 SKIASHARP_UPDATE_GOLDENS=1 \
     dotnet test tests/SkiaSharp.Tests.Console -c Release \
     --filter "DisplayName~ganesh-vulkan.*MyScene"
   ```

4. Run the full matrix; record per-renderer overrides for whichever cells diverge.

---

## Adding a new renderer

Implement `IRenderer` in `tests/Tests/SkiaSharp/Visual/Renderers/`. The interface is small:

```csharp
public interface IRenderer : IDisposable
{
    string Name { get; }
    bool IsAvailable { get; }            // cheap probe; no GPU bring-up
    string UnavailableReason { get; }     // for the skip message
    Task<byte[]> RenderAsync (ISkiaScene scene, SKImageInfo info, CancellationToken ct);
}
```

Two rules:

- **Constructors must be cheap.** Discovery instantiates every renderer to read its `Name`; bringing up a GPU context there would balloon test-discovery time. Move GPU work into `RenderAsync` (or a lazy singleton like `AndroidHostSession` / `iOSHostSession`).
- **Throw `RendererUnavailableException` from `RenderAsync` when the runtime decides this host can't actually run it** (e.g. the connect to the simulator timed out). The harness translates that into a `Skip`, not a fail.

The reflection-based `RendererCatalog` picks up new renderers automatically.
