# SkiaSharp Mac Catalyst Sample

Demonstrates SkiaSharp views in a Mac Catalyst app with a `UISplitViewController` sidebar, system dark/light mode support, and touch/trackpad interaction.

## Sample Pages

This sample shows how to integrate SkiaSharp views into a Mac Catalyst app using `UIViewController` subclasses. Mac Catalyst brings iPad apps to macOS, and SkiaSharp views work seamlessly in this environment with Metal GPU acceleration.

### CPU

A static scene rendered on the CPU — a radial gradient background overlaid with semi-transparent colored circles and centered "SkiaSharp" text.

**Features:**

- **`SKCanvasView`** — Software-rendered canvas backed by a `UIView`, ideal for static or infrequently updated content.
- **`SKShader`** — Radial gradient background created with `SKShader.CreateRadialGradient`.
- **`SKCanvas.DrawCircle`** — Semi-transparent colored circles composited over the gradient.
- **`SKCanvas.DrawText`** — Centered "SkiaSharp" text rendered with measured alignment.
- **`SKTypeface`** — Custom font loaded from the app bundle via `SKTypeface.FromStream`.

### GPU (Metal)

A real-time animated shader running at full frame rate on the GPU via Apple's Metal framework, with touch/trackpad interaction.

**Features:**

- **`SKMetalView`** — Hardware-accelerated canvas backed by Metal, Apple's modern low-level GPU API.
- **`SKRuntimeEffect`** — SkSL metaball "lava lamp" shader compiled at runtime with `SKRuntimeEffect.BuildShader`.
- **Render loop** — Continuous animation with an FPS counter overlay.
- **Touch interaction** — Touch/trackpad position is passed as a shader uniform.

### Drawing

A freehand drawing canvas with a color palette, brush size control via pinch gesture, and a clear button.

**Features:**

- **`SKCanvasView`** — Software-rendered canvas invalidated on demand after each stroke or clear.
- **`SKPath`** — Freehand strokes captured as paths with `MoveTo` and `LineTo` from touch events.
- **Touch/trackpad input** — Pointer tracking for press, move, and release across the canvas.
- **`UIPinchGestureRecognizer`** — Pinch gesture to adjust brush size.
- **Color palette** — Six selectable colors with dark/light mode variants.

## Screenshots

| CPU | GPU (Metal) | Drawing |
|---|---|---|
| <img src="screenshots/cpu.png" width="300" alt="CPU"> | <img src="screenshots/gpu.png" width="300" alt="GPU Metal"> | <img src="screenshots/drawing.png" width="300" alt="Drawing"> |
