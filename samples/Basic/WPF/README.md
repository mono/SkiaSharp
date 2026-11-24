# SkiaSharp WPF Sample

Demonstrates all SkiaSharp WPF view types with top tab navigation, dark/light theming, and mouse interaction.

## Sample Pages

This sample shows how to integrate SkiaSharp views into a WPF desktop app using XAML. The `SKElement` and `SKGLElement` controls are placed declaratively in `.xaml` files alongside standard WPF controls, and can be configured in the Visual Studio XAML designer.

### CPU

A static scene rendered entirely on the CPU — a radial gradient background overlaid with semi-transparent colored circles and centered "SkiaSharp" text.

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
- **Render loop** — Continuous animation driven by `DispatcherTimer` with an FPS counter overlay.
- **Mouse interaction** — Mouse position is passed as a shader uniform, adding a white-hot blob to the metaball field.
- **Lifecycle management** — Render timer starts/stops on `Loaded`/`Unloaded` to avoid background GPU work when the page is not visible.

### Drawing

A freehand drawing canvas with a floating toolbox for choosing colors and brush sizes. Strokes persist across color and size changes.

**Features:**

- **`SKElement`** — Software-rendered canvas invalidated on demand after each stroke or clear.
- **`SKPath`** — Freehand strokes captured as paths with `MoveTo` and `LineTo` from mouse events.
- **WPF mouse events** — `MouseDown`, `MouseMove`, `MouseUp` for pointer tracking with mouse capture.
- **`MouseWheel`** — Scroll wheel to adjust brush size.
- **Color palette** — Six selectable colors with dark/light mode variants that update on theme change.
- **Brush size** — Adjustable stroke width (1–50px) via slider or scroll wheel.
- **Brush cursor** — Semi-transparent circle indicator showing brush size at the cursor position.
- **Adaptive layout** — Toolbox wraps for narrow windows using `WrapPanel`.
- **DPI scaling** — Canvas coordinates are scaled via `PresentationSource` to handle high-DPI displays correctly.
- **Theme detection** — Reads Windows dark/light mode from the registry and responds to `UserPreferenceChanged` events.

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download) or later
- Windows 10 (build 19041 or later)

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

## Screenshots

| CPU | GPU | Drawing |
|---|---|---|
| <img src="screenshots/cpu-light.png" width="350" alt="CPU Light"> | <img src="screenshots/gpu-light.png" width="350" alt="GPU Light"> | <img src="screenshots/drawing-light.png" width="350" alt="Drawing Light"> |
| <img src="screenshots/cpu-dark.png" width="350" alt="CPU Dark"> | <img src="screenshots/gpu-dark.png" width="350" alt="GPU Dark"> | <img src="screenshots/drawing-dark.png" width="350" alt="Drawing Dark"> |
