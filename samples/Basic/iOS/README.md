# SkiaSharp iOS Sample

Demonstrates all SkiaSharp iOS view types with a `UITabBarController`, storyboard-driven views, system dark/light mode support, and touch interaction.

## Sample Pages

This sample shows how to integrate SkiaSharp views into an iOS app using storyboards and `UIViewController` subclasses. Each view type is placed in a storyboard scene and configured in Interface Builder — view controllers only contain event wiring and paint logic. Navigation uses a `UITabBarController` with SF Symbol tab icons.

### CPU

A static scene rendered on the CPU — a radial gradient background overlaid with semi-transparent colored circles and centered "SkiaSharp" text.

**Features:**

- **`SKCanvasView`** — Software-rendered canvas backed by a `UIView`, ideal for static or infrequently updated content.
- **`SKShader`** — Radial gradient background created with `SKShader.CreateRadialGradient`.
- **`SKCanvas.DrawCircle`** — Semi-transparent colored circles composited over the gradient.
- **`SKCanvas.DrawText`** — Centered "SkiaSharp" text rendered with measured alignment.

### GPU

A real-time animated shader running at full frame rate on the GPU, with a segmented sub-tab bar at the top (`GpuViewController`) that swaps between the Metal (`GpuMetalViewController` / `SKMetalView`) and OpenGL ES (`GpuGLViewController` / `SKGLView`) Storyboard scenes, and touch interaction that adds a white-hot blob to the metaball field.

**Features:**

- **`SKMetalView`** — Hardware-accelerated canvas backed by Metal, Apple's modern low-level GPU API (default).
- **`SKGLView`** — Hardware-accelerated canvas backed by GLKit's `GLKView`, using OpenGL ES for rendering.
- **GPU sub-tabs (`UISegmentedControl`)** — Top segmented bar that swaps full child `UIViewController` Storyboard scenes, exercising full view controller appearance/disappearance lifecycle when switching backends.
- **`SKRuntimeEffect`** — SkSL metaball "lava lamp" shader compiled at runtime with `SKRuntimeEffect.BuildShader`.
- **Render loop** — Continuous animation with `MTKView` pause/resume lifecycle (Metal) or `CADisplayLink` (OpenGL ES) and an FPS counter overlay.
- **Touch interaction** — Touch position is passed as a shader uniform via `UITouch` events.

### Drawing

A freehand drawing canvas with a color palette, brush size control via pinch gesture, and a clear button.

**Features:**

- **`SKCanvasView`** — Software-rendered canvas invalidated on demand after each stroke or clear.
- **`SKPath`** — Freehand strokes captured as paths with `MoveTo` and `LineTo` via `UIPanGestureRecognizer`.
- **`UIPinchGestureRecognizer`** — Pinch gesture to adjust brush size.
- **Color palette** — Six selectable colors with dark/light mode variants.
- **Floating toolbox** — Blur-effect toolbar with swatch row, slider, and clear button.

## Requirements

- [.NET 8 SDK](https://dotnet.microsoft.com/download) or later
- macOS with [Xcode](https://developer.apple.com/xcode/) installed
- iOS workload: `dotnet workload install ios`

## Running the Sample

Build and deploy to a simulator or device:

```bash
dotnet build -t:Run
```

To start on a different page or GPU backend, change `DefaultPage` or `DefaultGpuBackend` in `AppDelegate.cs`:

```csharp
public static SamplePage DefaultPage { get; set; } = SamplePage.Gpu;
public static GpuBackend DefaultGpuBackend { get; set; } = GpuBackend.Metal;
```

Available pages: `Cpu` (default), `Gpu`, `Drawing`

Available GPU backends: `Metal` (default), `GL`

## Screenshots

### Light Mode

| CPU | GPU (OpenGL) | GPU (Metal) | Drawing |
|---|---|---|---|
| <img src="screenshots/cpu.png" width="200" alt="CPU"> | <img src="screenshots/gpu-gl.png" width="200" alt="GPU OpenGL"> | <img src="screenshots/gpu-metal.png" width="200" alt="GPU Metal"> | <img src="screenshots/drawing.png" width="200" alt="Drawing"> |

### Dark Mode

| CPU | GPU (OpenGL) | GPU (Metal) | Drawing |
|---|---|---|---|
| <img src="screenshots/cpu-dark.png" width="200" alt="CPU Dark"> | <img src="screenshots/gpu-gl-dark.png" width="200" alt="GPU OpenGL Dark"> | <img src="screenshots/gpu-metal-dark.png" width="200" alt="GPU Metal Dark"> | <img src="screenshots/drawing-dark.png" width="200" alt="Drawing Dark"> |
