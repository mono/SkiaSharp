# SkiaSharp Browser WebAssembly Sample

Demonstrates SkiaSharp rendering in the browser via raw .NET WebAssembly (no Blazor). Uses Bootstrap 5 for the UI and vanilla JavaScript for interop.

## Sample Pages

### CPU

A static scene rendered on the CPU — a radial gradient background overlaid with semi-transparent colored circles and centered "SkiaSharp" text.

**Features:**

- **`SKBitmap`** — CPU-rendered bitmap transferred to an HTML `<canvas>` via `[JSExport]` / `[JSImport]` interop.
- **`SKShader`** — Radial gradient background created with `SKShader.CreateRadialGradient`.
- **`SKCanvas.DrawCircle`** — Semi-transparent colored circles composited over the gradient.
- **`SKCanvas.DrawText`** — Centered "SkiaSharp" text rendered with measured alignment.

### GPU

A real-time animated shader running at full frame rate, with touch/click interaction.

**Features:**

- **`SKRuntimeEffect`** — SkSL metaball "lava lamp" shader compiled at runtime and rendered via `SKRuntimeEffectBuilder`.
- **`requestAnimationFrame`** — Continuous animation loop driven from JavaScript, calling C# each frame.
- **Touch interaction** — Pointer position is passed as a shader uniform; clicking/touching adds an extra metaball.
- **FPS counter** — Measured in JavaScript and displayed as a Bootstrap badge.

### Drawing

A freehand drawing canvas with a color palette, adjustable brush size, and clear button.

**Features:**

- **`SKPath`** — Freehand strokes captured as paths with `MoveTo` and `LineTo` from pointer events.
- **Pointer events** — JavaScript captures pointer input and calls `[JSExport]` methods on C#.
- **Color palette** — Six selectable colors via clickable swatches.
- **Brush size slider** — Adjustable stroke width via an HTML range input.

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download) or later
- WebAssembly tools workload: `dotnet workload install wasm-tools`

## Running the Sample

Build and run:

```bash
dotnet run --project SkiaSharpSample/SkiaSharpSample.csproj
```

Then open the URL shown in the console (typically `https://localhost:7184`) in a browser.
