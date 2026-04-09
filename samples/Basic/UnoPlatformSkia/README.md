# SkiaSharp Uno Platform Sample (Skia Renderer)

Demonstrates SkiaSharp views in an Uno Platform app using the **Skia rendering** path. This sample uses `SKXamlCanvas` for CPU-rendered content and Uno's `SKCanvasElement` for GPU-accelerated rendering via the Skia composition pipeline.

> **See also:** The [UnoPlatform](../UnoPlatform/) sample demonstrates the **native rendering** path using `SKSwapChainPanel` for GPU rendering.

## Rendering Approach

Uno Platform 6.x uses Skia as its primary renderer on all platforms — Desktop, WebAssembly, iOS, Android, and more. The `SKCanvasElement` control (from `Uno.UI.Composition`) integrates directly into Uno's Skia composition pipeline, providing the best performance on Skia-rendered targets.

| Control | Rendering | Use Case |
|---------|-----------|----------|
| `SKXamlCanvas` | Software (CPU) | Static or on-demand content — works everywhere |
| `SKCanvasElement` | Skia composition (GPU) | Continuous animation — optimal for Skia-rendered targets |

### Supported Platforms

`SKCanvasElement` works on all Skia-rendered targets:
- **Desktop** (Linux, macOS, Windows via Skia) — Skia composition pipeline
- **WebAssembly** — Skia rendering via canvas
- **iOS / Android** — Skia rendering (default in Uno 6.x)

> **Note:** `SKCanvasElement` is **not available** on native WinUI (`net10.0-windows`). Use the [UnoPlatform](../UnoPlatform/) sample with `SKSwapChainPanel` for Windows native rendering.

## Sample Pages

### CPU

A static scene rendered on the CPU — a radial gradient background overlaid with semi-transparent colored circles and centered "SkiaSharp" text.

**Features:**

- **`SKXamlCanvas`** — Software-rendered canvas that integrates into XAML layout on all Uno target platforms.
- **`SKShader`** — Radial gradient background created with `SKShader.CreateRadialGradient`.
- **`SKCanvas.DrawCircle`** — Semi-transparent colored circles composited over the gradient.
- **`SKCanvas.DrawText`** — Centered "SkiaSharp" text rendered with measured alignment.

### GPU

A real-time animated shader running via Uno's Skia composition pipeline, with pointer interaction.

**Features:**

- **`SKCanvasElement`** — Custom `SkiaGpuView` control extending `Uno.UI.Composition.SKCanvasElement` for Skia composition rendering.
- **`SKRuntimeEffect`** — SkSL metaball "lava lamp" shader compiled at runtime with `SKRuntimeEffect.BuildShader`.
- **Animation** — Continuous animation driven by `DispatcherTimer` with `Invalidate()` calls and an FPS counter overlay.
- **Pointer interaction** — Pointer position is passed as a shader uniform.

### Drawing

A freehand drawing canvas with a color palette, brush size slider, and clear button.

**Features:**

- **`SKXamlCanvas`** — Software-rendered canvas invalidated on demand after each stroke or clear.
- **`SKPath`** — Freehand strokes captured as paths with `MoveTo` and `LineTo` from pointer events.
- **Pointer events** — `PointerPressed`, `PointerMoved`, `PointerReleased` for cross-device input.
- **`PointerWheelChanged`** — Scroll wheel to adjust brush size (desktop).
- **Color palette** — Six selectable colors with dark/light mode variants and selection highlight.
- **Responsive layout** — Toolbox adapts between vertical and horizontal orientation based on window width.

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download) or later
- Uno Platform workload: `dotnet workload install uno-platform`
- Platform-specific workloads for target platforms (e.g., `android`, `ios`, `wasm-tools`)

## Running the Sample

Build and run the desktop target:

```bash
dotnet run -f net10.0-desktop
```

Or target a specific platform:

```bash
dotnet build -f net10.0-browserwasm     # WebAssembly
dotnet build -f net10.0-android         # Android
dotnet build -f net10.0-ios             # iOS
```
