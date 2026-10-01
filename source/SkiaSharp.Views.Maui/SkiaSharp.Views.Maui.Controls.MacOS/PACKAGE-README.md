# SkiaSharp.Views.Maui.Controls.MacOS (experimental)

Native **AppKit** handlers for `SKCanvasView` (CPU/CoreGraphics) and `SKGLView`
(GPU/Metal) in the `Microsoft.Maui.Platforms.MacOS` backend. This is **not**
Mac Catalyst. Requires .NET 10, macOS 14+, MAUI **10.0.41**, and
`Microsoft.Maui.Platforms.MacOS` **0.1.0-preview.12.26421.1**.

This package depends on the stable `SkiaSharp.Views.Maui.Controls` and
`SkiaSharp.Views.Maui.Core` assemblies. It reuses their controls, interfaces,
event arguments, and image sources without duplicating type identities.
Register its AppKit handlers after the backend:

```csharp
using Microsoft.Maui.Platforms.MacOS.Hosting;
using SkiaSharp.Views.Maui.Controls.Hosting;

var builder = MauiApp.CreateBuilder()
    .UseMauiAppMacOS<App>()
    .UseSkiaSharpMacOS();
```

For XAML, use the stable
`xmlns:skia="clr-namespace:SkiaSharp.Views.Maui.Controls;assembly=SkiaSharp.Views.Maui.Controls"`.
`SKCanvasView.PaintSurface` receives a temporary surface with `Info` (user
coordinates) and `RawInfo` (backing pixels). `SKGLView.PaintSurface` receives a
temporary Metal render target and a `GRContext`; do not dispose the surface,
render target, or GPU context in callbacks. Enable `HasRenderLoop` for continuous
frames, or call `InvalidateSurface()` for on-demand drawing. Mouse input requires
`EnableTouchEvents=true`; its positions are top-left canvas coordinates.
Disabling touch, disconnecting, or pressing again before release cancels an
active mouse contact; leaving the canvas does not cancel a drag. A
native NSView frame resize refreshes both CPU and Metal surfaces and canvas
sizes. In the current backend, changing a canvas's `WidthRequest` after initial
layout did not resize its NSView even after invalidating the canvas and parent
`VerticalStackLayout`; marking the native parent `NeedsLayout` and calling
`LayoutSubtreeIfNeeded()` applied the requested width. This is a backend
native-layout invalidation gap, not a renderer resize failure.

Disconnecting the Metal handler pauses rendering and clears the managed
control's context reference. The underlying Metal context remains owned by the
native NSView until that NSView is disposed; do not treat handler disconnect
alone as native GPU resource disposal.

The backend's current `ImageHandler` does not resolve custom image source
services. `UseSkiaSharpMacOS()` opts into once-per-process `PropertyMapper`
extensions on the backend's static `ImageHandler` and `ImageButtonHandler`
mappers: only SkiaSharp-backed
`SKImageImageSource`, `SKBitmapImageSource`, `SKPixmapImageSource`, and
`SKPictureImageSource` are converted to `NSImage`; all standard MAUI sources
remain backend-owned. Conversion creates an encoded copy; it does not transfer
ownership of the original SkiaSharp image, bitmap, pixmap, or picture. This
conversion is synchronous (`IsLoading` is false afterward). The backend's
asynchronous URI/stream image loading is not cancellable by this adapter: a
pending ordinary image load may overwrite a newer SkiaSharp source.

Gallery popup chrome needs native runtime verification after Gallery is ready:
although the backend maps `ZIndex`, native container hit testing and click
recognizers may affect scrim/panel dismissal. Live OS theme changes and
overlay appearance have not been exercised with these handlers.

The package declares itself trimmable. A trimmed macOS smoke-app publish and
runtime test succeeded; NativeAOT deployment and trimming of a production
application using the MAUI Labs AppKit backend have not been validated.
