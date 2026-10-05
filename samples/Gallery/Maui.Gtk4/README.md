# Experimental Linux GTK4 MAUI Gallery

This head reuses the merged [MAUI Gallery](../Maui/README.md) XAML, code-behind, resources, and shared sample catalog without copying the UI. It runs on the experimental [MAUI Labs GTK4 backend](https://github.com/dotnet/maui-labs/tree/main/platforms/Linux.Gtk4) through `UseMauiAppLinuxGtk4<App>().UseSkiaSharpGtk4()`. The GTK4 SkiaSharp handlers map the shared `SKCanvasView` to `SKDrawingArea` and `SKGLView` to the real `SKGLArea` OpenGL surface; the CPU/GPU selector and sample controls are unchanged.

Build the head on a Linux GTK4 desktop with .NET 10 and the GTK4, libepoxy, WebKitGTK 6, and GObject introspection runtime packages. In a source checkout, first build the matching Skia milestone 156 native binaries for your Linux architecture:

```sh
dotnet cake --target=externals-linux --arch=x64
dotnet build samples/Gallery/Maui.Gtk4/SkiaSharpSample.Maui.Gtk4.csproj
dotnet run --project samples/Gallery/Maui.Gtk4/SkiaSharpSample.Maui.Gtk4.csproj
```

The isolated `samples/Gallery/SkiaSharpSample.Gtk4.slnx` keeps experimental GTK packages out of the regular Gallery solution and shipping MAUI project.

This project pins MAUI Controls **10.0.51** and MAUI Labs GTK4/Essentials `0.1.0-preview.12.26421.1`; neither experimental package enters the shipping MAUI Gallery or stable MAUI bindings. The existing XAML controls, filter/search/sort panels, Info and appearance popovers, and per-window backend choice remain shared. The CPU and GPU views report physical backing pixels by default and support logical paint coordinates through `IgnorePixelScaling`. The native GTK4 backend currently ignores numeric `ZIndex`, so the shared Gallery inserts overlays after sibling content. GTK4 `Image` and `ImageButton` do not consume SkiaSharp-specific image sources through the registered image services; standard GTK sources remain available.

Before relying on this experimental backend, check wide and narrow filter/search layouts, category/API tag counts, CPU/GPU switching, Info and appearance popovers, canvas pointer coordinates, and generated-file Open/Share via GTK Essentials on a Linux GTK4 display. A successful macOS managed build does not verify Linux stacking, GPU presentation, touch, theme changes, or file integration.

The rebase onto Skia milestone 156 requires matching native binaries built from the checked-out Skia submodule. Never substitute an older milestone's prebuilt native download.
