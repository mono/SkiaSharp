# SkiaSharp.Views.Maui.Controls.MacOS (experimental)

Native **AppKit** handlers for `SKCanvasView` (CPU/CoreGraphics) and `SKGLView`
(GPU/Metal) in the `Microsoft.Maui.Platforms.MacOS` backend. This is **not**
Mac Catalyst. Requires .NET 10, macOS 14+, MAUI **10.0.51 or newer**, and
`Microsoft.Maui.Platforms.MacOS` **0.1.0-preview.12.26421.1**.

This package depends on the stable `SkiaSharp.Views.Maui.Controls` and
`SkiaSharp.Views.Maui.Core` assemblies. It reuses their controls, interfaces,
and event arguments without duplicating type identities.
Register its AppKit handlers after the backend:

```csharp
using Microsoft.Maui.Platforms.MacOS.Hosting;
using SkiaSharp.Views.Maui.Controls.Hosting;

var builder = MauiApp.CreateBuilder()
    .UseMauiAppMacOS<App>()
    .UseSkiaSharpMacOS();
```

The shared XAML Gallery has a separate native AppKit host project at
`samples/Gallery/Maui/AppKit/SkiaSharpSample.MacOS.csproj`; other Gallery
targets continue to use their existing stable MAUI handlers.

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
alone as native GPU resource disposal. Removing and re-adding a child in the
backend's `VerticalStackLayout` did not itself disconnect its handler; both
canvases resumed painting and receiving mouse input afterward.

The experimental handlers currently do not add AppKit image-source services
for the stable SkiaSharp-backed MAUI image sources; ordinary MAUI image sources
remain handled by the backend.

The separate AppKit Gallery host has exercised its filter/sort popovers,
overlay backdrop dismissal, narrow/wide layout, theme menu, and CPU/Metal
sample rendering. The pinned preview backend lacks layout child-command
dispatch, so that host registers a local layout handler for dynamic facets,
chips, and controls; this workaround is not part of the SkiaSharp package.
Live changes to the macOS system appearance have not been exercised.

The package declares itself trimmable. A trimmed macOS test host was previously
published and exercised with prebuilt native assets; a build from current native
source, NativeAOT deployment, and trimming of a production application using
the MAUI Labs AppKit backend have not been validated.
