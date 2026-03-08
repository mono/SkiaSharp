# SkiaSharp macOS Sample

Demonstrates all SkiaSharp macOS view types with an `NSSplitView` sidebar, system dark/light mode support, and mouse interaction.

## Sample Pages

This sample shows how to integrate SkiaSharp views into a native macOS AppKit app. Each view type is placed within `NSViewController` subclasses and composed using `NSSplitView` for sidebar navigation with mouse and scroll wheel interaction.

### CPU

A static scene rendered on the CPU — a radial gradient background overlaid with semi-transparent colored circles and centered "SkiaSharp" text.

**Features:**

- **`SKCanvasView`** — Software-rendered canvas backed by an `NSView`, ideal for static or infrequently updated content.
- **`SKShader`** — Radial gradient background created with `SKShader.CreateRadialGradient`.
- **`SKCanvas.DrawCircle`** — Semi-transparent colored circles composited over the gradient.
- **`SKCanvas.DrawText`** — Centered "SkiaSharp" text rendered with measured alignment.
- **`SKTypeface`** — Custom font loaded from the app bundle via `SKTypeface.FromStream`.

### GPU (OpenGL)

A real-time animated shader running at full frame rate on the GPU via OpenGL, with mouse interaction that adds a white-hot blob to the metaball field.

**Features:**

- **`SKGLView`** — Hardware-accelerated canvas backed by `NSOpenGLView`, using OpenGL for rendering.
- **`SKRuntimeEffect`** — SkSL metaball "lava lamp" shader compiled at runtime with `SKRuntimeEffect.BuildShader`.
- **Render loop** — Continuous animation with an FPS counter overlay.
- **Mouse interaction** — Mouse position is passed as a shader uniform via `NSEvent` mouse tracking.

### GPU (Metal)

A real-time animated shader running at full frame rate on the GPU via Apple's Metal framework, with mouse interaction.

**Features:**

- **`SKMetalView`** — Hardware-accelerated canvas backed by Metal, Apple's modern low-level GPU API.
- **`SKRuntimeEffect`** — SkSL metaball "lava lamp" shader compiled at runtime with `SKRuntimeEffect.BuildShader`.
- **Render loop** — Continuous animation with an FPS counter overlay.
- **Mouse interaction** — Mouse position is passed as a shader uniform via `NSEvent` mouse tracking.

### Drawing

A freehand drawing canvas with a color palette, brush size control via scroll wheel, and a clear button.

**Features:**

- **`SKCanvasView`** — Software-rendered canvas invalidated on demand after each stroke or clear.
- **`SKPath`** — Freehand strokes captured as paths with `MoveTo` and `LineTo` from mouse events.
- **`NSEvent`** — Mouse tracking for press, drag, and release across the canvas.
- **Scroll wheel** — Brush size adjustment via `ScrollWheel` event.
- **Color palette** — Six selectable colors with dark/light mode variants.

## Running the Sample

Build and run:

```bash
dotnet build -f net8.0-macos
dotnet run -f net8.0-macos
```

To start on a different page, change `DefaultPage` in `AppDelegate.cs`:

```csharp
public static SamplePage DefaultPage { get; set; } = SamplePage.GpuMetal;
```

Available pages: `Cpu` (default), `GpuGL`, `GpuMetal`, `Drawing`

## Screenshots

| CPU | GPU (OpenGL) | GPU (Metal) | Drawing |
|-----|-------------|-------------|---------|
| ![CPU](screenshots/cpu-light.png) | ![GPU GL](screenshots/gpu-gl-light.png) | ![GPU Metal](screenshots/gpu-metal-light.png) | ![Drawing](screenshots/drawing-light.png) |
