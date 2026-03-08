# SkiaSharp WPF Sample

Demonstrates SkiaSharp views in a WPF desktop app with sidebar navigation, XAML layouts, and system dark/light mode support.

## Sample Pages

This sample shows how to integrate SkiaSharp views into a WPF app using XAML. The `SKElement` and `SKGLElement` controls are placed declaratively in `.xaml` files alongside standard WPF controls, and can be configured in the Visual Studio XAML designer.

### CPU

A static scene rendered on the CPU — a radial gradient background overlaid with semi-transparent colored circles and centered "SkiaSharp" text.

**Features:**

- **`SKElement`** — Software-rendered canvas backed by a WPF `FrameworkElement`, integrated into the WPF visual tree and layout system.
- **`SKShader`** — Radial gradient background created with `SKShader.CreateRadialGradient`.
- **`SKCanvas.DrawCircle`** — Semi-transparent colored circles composited over the gradient.
- **`SKCanvas.DrawText`** — Centered "SkiaSharp" text rendered with measured alignment.
- **`SKTypeface`** — Custom font loaded from an embedded resource via `SKTypeface.FromStream`.

### GPU

A real-time animated shader running at full frame rate on the GPU via OpenGL (WGL), with mouse interaction that adds a white-hot blob to the metaball field.

**Features:**

- **`SKGLElement`** — Hardware-accelerated canvas backed by OpenGL via WGL, integrated into the WPF visual tree.
- **`SKRuntimeEffect`** — SkSL metaball "lava lamp" shader compiled at runtime with `SKRuntimeEffect.BuildShader`.
- **Render loop** — Continuous animation with an FPS counter overlay.
- **Mouse interaction** — Mouse position is passed as a shader uniform via WPF `MouseMove` events.

### Drawing

A freehand drawing canvas with a color palette, brush size slider, and clear button.

**Features:**

- **`SKElement`** — Software-rendered canvas invalidated on demand after each stroke or clear.
- **`SKPath`** — Freehand strokes captured as paths with `MoveTo` and `LineTo` from mouse events.
- **WPF mouse events** — `MouseDown`, `MouseMove`, `MouseUp` for pointer tracking.
- **`MouseWheel`** — Scroll wheel to adjust brush size.
- **Color palette** — Six selectable colors with dark/light mode variants.

## Running the Sample

Build and run:

```bash
dotnet run --project SkiaSharpSample/SkiaSharpSample.csproj
```

To start on a different page, change `DefaultPage` in `MainWindow.xaml.cs`:

```csharp
public static SamplePage DefaultPage { get; set; } = SamplePage.Gpu;
```

Available pages: `Cpu` (default), `Gpu`, `Drawing`
