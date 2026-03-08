# SkiaSharp tvOS Sample

Demonstrates SkiaSharp views on tvOS with a `UITabBarController` for navigation and support for both OpenGL and Metal GPU backends.

## Sample Pages

This sample shows how to integrate SkiaSharp views into a tvOS app using storyboards and `UIViewController` subclasses. Navigation uses a tab bar pattern suited for the Apple TV remote. There is no drawing page since tvOS does not support direct touch on the screen.

### CPU

A static scene rendered on the CPU — a radial gradient background overlaid with semi-transparent colored circles and centered "SkiaSharp" text.

**Features:**

- **`SKCanvasView`** — Software-rendered canvas backed by a `UIView`, ideal for static or infrequently updated content.
- **`SKShader`** — Radial gradient background created with `SKShader.CreateRadialGradient`.
- **`SKCanvas.DrawCircle`** — Semi-transparent colored circles composited over the gradient.
- **`SKCanvas.DrawText`** — Centered "SkiaSharp" text rendered with measured alignment.
- **`SKTypeface`** — Custom font loaded from the app bundle via `SKTypeface.FromStream`.

### GPU (OpenGL)

A real-time animated shader running at full frame rate on the GPU via OpenGL ES.

**Features:**

- **`SKGLView`** — Hardware-accelerated canvas backed by GLKit's `GLKView`, using OpenGL ES for rendering.
- **`SKRuntimeEffect`** — SkSL metaball "lava lamp" shader compiled at runtime with `SKRuntimeEffect.BuildShader`.
- **Render loop** — Continuous animation with an FPS counter overlay.

### GPU (Metal)

A real-time animated shader running at full frame rate on the GPU via Apple's Metal framework.

**Features:**

- **`SKMetalView`** — Hardware-accelerated canvas backed by Metal, Apple's modern low-level GPU API.
- **`SKRuntimeEffect`** — SkSL metaball "lava lamp" shader compiled at runtime with `SKRuntimeEffect.BuildShader`.
- **Render loop** — Continuous animation with an FPS counter overlay.
