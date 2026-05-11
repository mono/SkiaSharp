# Visual tests (scene × renderer matrix)

This directory contains a small visual-regression framework: a *scene* is a
named, deterministic `Action<SKCanvas>`; a *renderer* is a transport-agnostic
"give me pixels" surface. One xUnit theory fans across every compatible
`(renderer × scene)` pair at discovery time.

## Layout

```
Visual/
├── Scenes/        ISkiaScene + SceneCatalog + one .cs per scene
├── Renderers/     IRenderer + RendererCatalog + one .cs per backend
├── Tests/         VisualMatrixTests — the one matrix theory
├── Goldens/
│   ├── _shared/   canonical golden per scene (used by default)
│   └── {name}/    per-renderer override (only if that renderer diverges)
├── VisualTestBase.cs   comparison harness + scope-aware update mode
├── VulkanLoader.cs     headless VkInstance/VkDevice (Lavapipe on Linux, real loader on Windows)
├── EglLoader.cs        headless EGL+llvmpipe (Linux)
└── WglLoader.cs        headless WGL+HWND_MESSAGE (Windows)
```

## Matrix today

| Renderer | Linux | Windows | macOS / iOS |
|---|---|---|---|
| `raster`              | ✓ direct                    | ✓ direct                | ✓ direct |
| `ganesh-gl`           | ✓ EGL + llvmpipe            | ✓ WGL + HWND_MESSAGE    | — (deprecated) |
| `ganesh-vulkan`       | ✓ Lavapipe                  | ✓ vulkan-1.dll          | — |
| `graphite-vulkan`     | ✓ Lavapipe                  | ✓ vulkan-1.dll          | — |
| `graphite-metal`      | —                           | —                       | ✓ direct |
| `wasm-raster`         | ✓ Chromium/Playwright       | ✓ Chromium/Playwright   | ✓ Chromium/Playwright |
| `wasm-ganesh-gles`    | ✓ Chromium/Playwright       | ✓ Chromium/Playwright   | ✓ Chromium/Playwright |
| `wasm-graphite-dawn`  | ✓ Chromium SwiftShader-WebGPU | ✓ Chromium SwiftShader-WebGPU | ✓ Chromium SwiftShader-WebGPU |

`IsAvailable` probes each renderer at run time; any unreachable cell skips
with a clear reason rather than failing.

## Running

```powershell
# All visual cells
dotnet test tests/SkiaSharp.Tests.Console/SkiaSharp.Tests.Console.csproj `
    -c Release --filter "FullyQualifiedName~VisualMatrixTests"

# One renderer (substring match against the theory display name)
dotnet test tests/SkiaSharp.Tests.Console/SkiaSharp.Tests.Console.csproj `
    -c Release --filter "DisplayName~ganesh-gl"

# One specific cell
dotnet test ... --filter "DisplayName~ganesh-gl|DisplayName~DiagonalLines"
```

### Prerequisites

- A `libSkiaSharp` native build that includes Graphite + Vulkan support for
  the renderers' backends. Bootstrap with `dotnet cake --target=externals-{platform}`
  with `SUPPORT_GRAPHITE=true SUPPORT_VULKAN=true`. The repo's pre-built
  `externals-download` may or may not have Graphite — re-bootstrap on a fresh
  clone if `IsBackendAvailable(Vulkan)` reports false.
- For `ganesh-vulkan` / `graphite-vulkan` cells: a Vulkan loader on the
  system. Linux ships `libvulkan.so.1` via `libvulkan1`; Windows GPU drivers
  install `vulkan-1.dll`. Without a loader, those cells skip.
- For `ganesh-gl` on Linux: `libegl1` + `libgl1-mesa-dri` (provides
  llvmpipe). Without those, the cell skips. WSL2 typically has them
  preinstalled.
- For `ganesh-gl` on Windows: any modern GPU driver (NVIDIA/AMD/Intel)
  exposes WGL + OpenGL 3.3+. No extra installation.
- For `wasm-*` cells:
  1. Publish the host first: `dotnet publish tests/Hosts/RenderHost.Wasm -c Release`
  2. `Microsoft.Playwright` will lazily install Chromium on first run
     (~150 MB into `%USERPROFILE%\AppData\Local\ms-playwright`).
  Without a publish, `wasm-*` cells skip with a "publish first" message.

## Goldens

Comparison goes specific → shared:

1. `Goldens/{renderer.Name}/{scene}.png` — exists only if that renderer
   genuinely diverges from `_shared/`
2. `Goldens/_shared/{scene}.png` — canonical baseline (the rendering most
   renderers should match within `MaxChannelDelta = 2`)

If the actual pixels don't match either, the test fails with paths to
`.actual.png` + `.diff.png` saved under `_failures/`. The diff PNG colour-codes
each pixel:

- **red** = out of tolerance (real regression)
- **amber** = within tolerance but non-zero delta
- **dim grey** = bit-exact (silhouette of the scene)

### Update mode

```bash
# Add a new scene → record the canonical golden (default scope)
SKIASHARP_UPDATE_GOLDENS=1 dotnet test ... --filter "DisplayName~MyNewScene"

# Record a per-renderer divergence as expected (override the _shared/ baseline)
SKIASHARP_UPDATE_GOLDENS=1 SKIASHARP_GOLDEN_SCOPE=renderer \
    dotnet test ... --filter "DisplayName~ganesh-gl|DisplayName~MyScene"
```

```powershell
# PowerShell equivalents
$env:SKIASHARP_UPDATE_GOLDENS=1; dotnet test ...
$env:SKIASHARP_UPDATE_GOLDENS=1; $env:SKIASHARP_GOLDEN_SCOPE='renderer'; dotnet test ...
```

The `renderer` scope writes only the cell(s) you matched in `--filter`; the
`_shared/` baseline stays untouched. Use it when a renderer's output is
*supposed* to differ (e.g., CPU raster's antialiasing vs GPU's). Otherwise
the divergence is a real regression — investigate the diff PNG.

## Extending

- **Add a scene**: drop a new `.cs` under `Scenes/` implementing `ISkiaScene`
  (top-level public class, parameterless ctor). Picked up by reflection;
  no other plumbing.
- **Add a renderer**: drop a new `.cs` under `Renderers/` implementing
  `IRenderer` (same auto-discovery rules). Set `IsAvailable=false` on
  platforms where the backend doesn't apply.
- **Add a host transport** (out-of-process): mirror `WasmRendererBase` +
  `WasmHostSession` — IRenderer proxy on the test side + a small host app
  under `tests/Hosts/` that responds to render requests.

## Per-renderer overrides today

| Override | Why |
|---|---|
| `raster/DiagonalLines.png` | CPU AA differs from GPU at antialiased line edges |
| `raster/RedRoundedRectOnWhite.png` | Same — corner AA differs |
| `wasm-raster/DiagonalLines.png` | Same as desktop raster but via WASM build |
| `wasm-raster/RedRoundedRectOnWhite.png` | Same |
| `wasm-ganesh-gles/DiagonalLines.png` | SwiftShader-WebGL2 vs Lavapipe diverges by 6 channels on 28 px |
| `wasm-graphite-dawn/DiagonalLines.png` | SwiftShader-WebGPU vs Lavapipe-Vulkan diverges by 5 channels |
| `wasm-graphite-dawn/GpuOnly_FilledCircle.png` | Same |

When you add a new platform's renderer, expect similar small divergences
under real-hardware GPU drivers (they're not bit-deterministic across
vendors). Record the override the first time you see it, then it's locked
in for future runs.
