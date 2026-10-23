# Experimental SkiaSharp MAUI GTK4 handlers

MAUI 10 / GirCore GTK4 integration for the existing `SKCanvasView` and `SKGLView`
controls. The canvas uses a raster surface; the GPU view uses GTK's OpenGL context.

Register the handlers after the GTK4 app host:

```csharp
using Microsoft.Maui.Platforms.Linux.Gtk4.Hosting;
using SkiaSharp.Views.Maui.Controls.Hosting;

builder.UseMauiAppLinuxGtk4<App>().UseSkiaSharpGtk4();
```

Use MAUI Controls/Core 10.0.51 with a GirCore-compatible GTK4 runtime,
libepoxy, and matching SkiaSharp native assets for the host OS/architecture.
`SKGLView` needs an OpenGL display. The
[Basic head](https://github.com/mono/SkiaSharp/tree/main/samples/Basic/Maui/SkiaSharpSample.Gtk4)
and [Gallery head](https://github.com/mono/SkiaSharp/tree/main/samples/Gallery/Maui.Gtk4)
show host setup; see [upstream prerequisites](https://github.com/dotnet/maui-labs/tree/main/platforms/Linux.Gtk4).

This preview registers only the two drawing views, not SkiaSharp image sources.
Input tracks one pointer, not concurrent touch contacts. Trimming and NativeAOT
have not been validated.
