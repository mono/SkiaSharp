# SkiaSharp macOS Sample

Demonstrates all SkiaSharp macOS view types with an `NSTabViewController` toolbar-style tab bar, storyboard-driven views, system dark/light mode support, and mouse interaction.

## Sample Pages

This sample shows how to integrate SkiaSharp views into a native macOS AppKit app using storyboards and `NSViewController` subclasses. Each view type is placed in a storyboard scene and configured in Interface Builder — view controllers only contain event wiring and paint logic. Navigation uses an `NSTabViewController` which renders as a native macOS toolbar tab bar.

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
- **Render loop** — Continuous animation with an FPS counter pill overlay.
- **Mouse interaction** — Mouse position is passed as a shader uniform via `NSEvent` mouse tracking.

### GPU (Metal)

A real-time animated shader running at full frame rate on the GPU via Apple's Metal framework, with mouse interaction.

**Features:**

- **`SKMetalView`** — Hardware-accelerated canvas backed by Metal, Apple's modern low-level GPU API.
- **`SKRuntimeEffect`** — SkSL metaball "lava lamp" shader compiled at runtime with `SKRuntimeEffect.BuildShader`.
- **Render loop** — Continuous animation with an FPS counter pill overlay.
- **Mouse interaction** — Mouse position is passed as a shader uniform via `NSEvent` mouse tracking.

### Drawing

A freehand drawing canvas with a horizontal color palette, brush size slider, and a floating clear button.

**Features:**

- **`SKCanvasView`** — Software-rendered canvas invalidated on demand after each stroke or clear.
- **`SKPath`** — Freehand strokes captured as paths with `MoveTo` and `LineTo` from mouse events.
- **`NSEvent`** — Mouse tracking for press, drag, and release across the canvas.
- **Scroll wheel** — Brush size adjustment via `ScrollWheel` event.
- **Color palette** — Six selectable colors with dark/light mode variants in a horizontal toolbar.
- **Floating toolbox** — Centered translucent toolbar with swatch row, brush size slider, and size label.
- **Floating clear button** — Top-right pill overlay to clear the canvas.

## Requirements

- [.NET 8 SDK](https://dotnet.microsoft.com/download) or later
- macOS with [Xcode](https://developer.apple.com/xcode/) installed
- macOS workload: `dotnet workload install macos`

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

### Light Mode

| CPU | GPU (OpenGL) | GPU (Metal) | Drawing |
|---|---|---|---|
| <img src="screenshots/cpu-light.png" width="300" alt="CPU"> | <img src="screenshots/gpu-gl-light.png" width="300" alt="GPU OpenGL"> | <img src="screenshots/gpu-metal-light.png" width="300" alt="GPU Metal"> | <img src="screenshots/drawing-light.png" width="300" alt="Drawing"> |

### Dark Mode

| CPU | GPU (OpenGL) | GPU (Metal) | Drawing |
|---|---|---|---|
| <img src="screenshots/cpu-dark.png" width="300" alt="CPU Dark"> | <img src="screenshots/gpu-gl-dark.png" width="300" alt="GPU OpenGL Dark"> | <img src="screenshots/gpu-metal-dark.png" width="300" alt="GPU Metal Dark"> | <img src="screenshots/drawing-dark.png" width="300" alt="Drawing Dark"> |
