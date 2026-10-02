# SkiaSharp GTK 4 Sample

Demonstrates SkiaSharp running in a GTK 4 desktop app with tab-based navigation, UI Builder layout support, and dark theme detection.

## Sample Pages

This sample shows how to integrate SkiaSharp views into a GTK 4 app. The UI structure is defined in `.ui` files (editable in GNOME Builder or Cambalache), with `SKDrawingArea` and `SKGLView` widgets injected into the layout containers.

### CPU

A static scene rendered on the CPU — a radial gradient background overlaid with semi-transparent colored circles and centered "SkiaSharp" text.

**Features:**

- **`SKDrawingArea`** — Software-rendered canvas backed by a `Gtk.DrawingArea`, the standard GTK 4 drawing surface.
- **Pixel scaling** — By default, paint coordinates and `CanvasSize` use physical display pixels; set `IgnorePixelScaling` to draw in GTK logical pixels instead. `RawInfo` always describes the backing surface.
- **`SKShader`** — Radial gradient background created with `SKShader.CreateRadialGradient`.
- **`SKCanvas.DrawCircle`** — Semi-transparent colored circles composited over the gradient.
- **`SKCanvas.DrawText`** — Centered "SkiaSharp" text rendered with measured alignment.
- **`SKTypeface`** — Custom font loaded via `SKTypeface.FromStream`.

### GPU

An animated, interactive SkSL metaball shader rendered on a real OpenGL-backed SkiaSharp surface.

**Features:**

- **`SKGLView`** — Hardware-accelerated canvas backed by `Gtk.GLArea` and desktop OpenGL; no CPU fallback.
- **`SKRuntimeEffect`** — Shader compiled once and updated with time, resolution, and mouse-position uniforms each frame.
- **Render loop** — GTK frame-clock-driven animation with an FPS overlay, paused whenever the tab is hidden.
- **Pointer input** — Press and drag to add a bright blob to the scene.

### Drawing

A freehand drawing canvas with a color palette, brush size label, and clear button. Strokes persist across color and size changes.

**Features:**

- **`SKDrawingArea`** — Software-rendered canvas invalidated on demand after each stroke or clear.
- **`SKPath`** — Freehand strokes captured as paths with `MoveTo` and `LineTo` from GTK gesture events.
- **`GestureDrag`** — GTK 4 drag gesture for tracking press, move, and release.
- **`EventControllerScroll`** — Scroll wheel to adjust brush size.
- **Color palette** — Six selectable colors with dark/light mode variants.

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download) or later
- GTK 4 development libraries:
  - **macOS:** `brew install gtk4`
  - **Ubuntu/Debian:** `sudo apt-get install libgtk-4-dev`
  - **Fedora:** `sudo dnf install gtk4-devel`
- A Linux or macOS desktop OpenGL display for the GPU page (`SKGLView` does not support OpenGL ES). The CPU and Drawing pages still work on other GTK 4 hosts.

GTK 4 can use Vulkan to composite its own scene graph, but it does not expose a Vulkan drawing widget analogous to `Gtk.GLArea`. The GPU page uses desktop OpenGL; a future Vulkan-backed SkiaSharp view would need application-owned Vulkan resources and a GTK-compatible texture import/synchronization path.

## Running the Sample

Build and run (Linux):

```bash
dotnet run --project SkiaSharpSample/SkiaSharpSample.csproj
```

On macOS, include Homebrew's native library directory so GirCore can load GTK and Graphene:

```bash
DYLD_LIBRARY_PATH="$(brew --prefix)/lib" dotnet run --project SkiaSharpSample/SkiaSharpSample.csproj
```

To start on a different page, change `DefaultPage` in `MainWindow.cs`:

```csharp
public static SamplePage DefaultPage { get; set; } = SamplePage.Drawing;
```

Available pages: `Cpu` (default), `Gpu`, `Drawing`

## Screenshots

| CPU | GPU | Drawing |
|---|---|---|
| <img src="screenshots/cpu.png" width="350" alt="CPU"> | <img src="screenshots/gpu.png" width="350" alt="GPU shader rendered on macOS OpenGL"> | <img src="screenshots/drawing.png" width="350" alt="Drawing"> |
