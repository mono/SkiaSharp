# SkiaSharp Tizen Sample

Demonstrates SkiaSharp views in a Tizen NUI app with a `TabView` for navigating between CPU rendering, GPU shader animation, and interactive drawing.

## Sample Pages

This sample uses `Tizen.NUI.Components.TabView` with three tabs, each demonstrating a different SkiaSharp rendering approach.

### CPU

A static scene rendered on the CPU — a radial gradient background overlaid with semi-transparent colored circles and centered "SkiaSharp" text.

**Features:**

- **`SKCanvasView`** — Software-rendered canvas integrated into the NUI view tree.
- **`SKShader`** — Radial gradient background created with `SKShader.CreateRadialGradient`.
- **`SKCanvas.DrawCircle`** — Semi-transparent colored circles composited over the gradient.
- **`SKCanvas.DrawText`** — Centered "SkiaSharp" text rendered with measured alignment.

### GPU

A real-time animated shader running at full frame rate via the NUI GPU surface, with touch interaction.

**Features:**

- **`SKGLSurfaceView`** — GPU-composited canvas using `NativeImageQueue` double-buffering through the NUI rendering pipeline.
- **`SKRuntimeEffect`** — SkSL metaball "lava lamp" shader compiled at runtime with `SKRuntimeEffect.BuildShader`.
- **Render loop** — Continuous animation driven by a `Timer` at ~60 fps with an FPS counter overlay.
- **Touch interaction** — Touch position is passed as a shader uniform (`iTouchPos`) to create an interactive white blob.

### Drawing

A freehand drawing canvas with a color palette toolbar and touch-based input.

**Features:**

- **`SKCanvasView`** — Software-rendered canvas invalidated on demand after each stroke or clear.
- **`SKPath`** — Freehand strokes captured as paths with `MoveTo` and `LineTo` from NUI touch events.
- **Color palette** — Six selectable color swatches with visual selection indicator.
- **Clear button** — Removes all strokes and resets the canvas.

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download) or later
- Tizen workload: `dotnet workload install tizen`

## Running the Sample

Build:

```bash
dotnet build -f net10.0-tizen
```
