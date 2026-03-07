# SkiaSharp Tizen Sample

Demonstrates SkiaSharp views in a Tizen app with Naviframe navigation, ElmSharp UI controls, and system theme detection.

## Sample Pages

This sample shows how to integrate SkiaSharp views into a Tizen app using ElmSharp widgets. The `SKCanvasView` and `SKGLSurfaceView` are created programmatically and added to ElmSharp container widgets, with navigation handled by the Tizen `Naviframe` pattern.

### CPU

A static scene rendered on the CPU — a radial gradient background overlaid with semi-transparent colored circles and centered "SkiaSharp" text.

**Features:**

- **`SKCanvasView`** — Software-rendered canvas backed by a Tizen `EvasObject`, integrated into the ElmSharp widget tree.
- **`SKShader`** — Radial gradient background created with `SKShader.CreateRadialGradient`.
- **`SKCanvas.DrawCircle`** — Semi-transparent colored circles composited over the gradient.
- **`SKCanvas.DrawText`** — Centered "SkiaSharp" text rendered with measured alignment.
- **`SKTypeface`** — Custom font loaded via `SKTypeface.FromStream`.

### GPU

A real-time animated shader running at full frame rate on the GPU via OpenGL ES (EFL), with touch interaction.

**Features:**

- **`SKGLSurfaceView`** — Hardware-accelerated canvas backed by OpenGL ES through the Tizen EFL graphics stack.
- **`SKRuntimeEffect`** — SkSL metaball "lava lamp" shader compiled at runtime with `SKRuntimeEffect.BuildShader`.
- **Render loop** — Continuous animation with an FPS counter overlay.
- **Touch interaction** — Touch position is passed as a shader uniform via Tizen `GestureLayer`.

### Drawing

A freehand drawing canvas with a cycling color palette and touch-based input.

**Features:**

- **`SKCanvasView`** — Software-rendered canvas invalidated on demand after each stroke or clear.
- **`SKPath`** — Freehand strokes captured as paths with `MoveTo` and `LineTo` from touch events.
- **`GestureLayer`** — Tizen gesture API for tracking press, move, and release.
- **Color palette** — Six selectable colors cycled via a button.
