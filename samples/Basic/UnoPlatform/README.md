# SkiaSharp Uno Platform Sample

Demonstrates SkiaSharp views in an Uno Platform app with `NavigationView` sidebar, XAML layouts, and cross-platform dark/light mode support.

## Sample Pages

This sample shows how to integrate SkiaSharp views into an Uno Platform app using XAML. The `SKCanvasView` and `SKSwapChainPanel` controls are placed declaratively in `.xaml` files alongside standard Uno controls, targeting multiple platforms (Windows, WebAssembly, iOS, Android, macOS) from a single codebase.

### CPU

A static scene rendered on the CPU — a radial gradient background overlaid with semi-transparent colored circles and centered "SkiaSharp" text.

**Features:**

- **`SKCanvasView`** — Software-rendered canvas that maps to the platform-native SkiaSharp view on each target.
- **`SKShader`** — Radial gradient background created with `SKShader.CreateRadialGradient`.
- **`SKCanvas.DrawCircle`** — Semi-transparent colored circles composited over the gradient.
- **`SKCanvas.DrawText`** — Centered "SkiaSharp" text rendered with measured alignment.
- **`SKTypeface`** — Custom font loaded from an embedded resource via `SKTypeface.FromStream`.

### GPU

A real-time animated shader running at full frame rate on the GPU, with pointer interaction that adds a white-hot blob to the metaball field.

**Features:**

- **`SKSwapChainPanel`** — Hardware-accelerated canvas that maps per-platform: ANGLE on Windows, WebGL on WebAssembly.
- **`SKRuntimeEffect`** — SkSL metaball "lava lamp" shader compiled at runtime with `SKRuntimeEffect.BuildShader`.
- **Render loop** — Continuous animation with an FPS counter overlay.
- **Pointer interaction** — Pointer position is passed as a shader uniform.

### Drawing

A freehand drawing canvas with a color palette, brush size slider, and clear button.

**Features:**

- **`SKCanvasView`** — Software-rendered canvas invalidated on demand after each stroke or clear.
- **`SKPath`** — Freehand strokes captured as paths with `MoveTo` and `LineTo` from pointer events.
- **Pointer events** — `PointerPressed`, `PointerMoved`, `PointerReleased` for cross-device input.
- **`PointerWheelChanged`** — Scroll wheel to adjust brush size.
- **Color palette** — Six selectable colors with dark/light mode variants.
