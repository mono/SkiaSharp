# Experimental SkiaSharp MAUI WPF handlers

`SkiaSharp.Views.Maui.Controls.WPF` targets `net10.0-windows` for the **WPF**
backend in `dotnet/maui-labs`, not the standard MAUI WinUI backend. It depends
on `Microsoft.Maui.Platforms.Windows.WPF` **0.1.0-preview.12.26421.1**, whose
published package requires MAUI **10.0.41** or newer. Version `0.1.0` is not
published on NuGet. Keep these preview dependencies in WPF-only applications;
do not add them to stable SkiaSharp MAUI projects.

In a WPF MAUI application using `UseWPF` and `UseMaui`, register the WPF
backend before these SkiaSharp handlers:

```csharp
using Microsoft.Maui.Controls.Hosting.WPF;
using SkiaSharp.Views.Maui.Controls.Hosting;

var builder = MauiApp.CreateBuilder()
	.UseMauiAppWPF<App>()
	.UseSkiaSharpWPF();
```

`UseSkiaSharpWPF()` includes `UseSkiaSharp()`'s registrations, then replaces
its canvas and GL handlers with WPF implementations. If you call
`UseSkiaSharp()` separately, call it **before** `UseSkiaSharpWPF()`; calling it
afterward reinstalls the non-WPF handler. Existing MAUI `SKCanvasView` and
`SKGLView` types and their paint/touch events remain unchanged. The opt-in WPF
`PropertyMapper` additions display `SKImageImageSource`, `SKBitmapImageSource`,
`SKPixmapImageSource` and `SKPictureImageSource` in MAUI `Image` and
`ImageButton` controls using the WPF `ToWriteableBitmap` converters; null
Skia content clears the image. Other sources still use the backend's
original mapping. The Skia image data must remain valid until conversion;
the returned WPF bitmap owns its copied pixels. No native Skia objects are
disposed by the mapper.
The preview backend's ordinary `Image` mapping does not clear a previous
image when its source becomes null; an already-started stream image load can
also complete after a newer Skia source and replace it. This WPF package does
not replace the backend's private image-loading pipeline; verify transitions
from Skia to ordinary sources and back on Windows.

The software handler forwards WPF `SKElement`'s borrowed `SKSurface` during
its paint callback; do not retain or dispose that surface. The GL handler
forwards `SKGLElement`'s real OpenGL `GRContext` and render target (not a
software substitute). `IgnorePixelScaling` makes paint/touch coordinates
logical while retaining physical `RawInfo`. The GL render loop runs only while
the native element is loaded; unloading or disconnecting cancels the timer and
reports a lost graphics context. Mouse, pen-promoted mouse, and touch events
are forwarded, including cancellation on lost capture or detachment.
Only WPF-specific rendering, input and image adapters live in this package;
portable MAUI views, interfaces, events and services are referenced from the
stable assemblies rather than duplicated. It uses direct mapper delegates
and generic handler registration, with no new reflection or trim descriptors.
The preview WPF backend's trimming/AOT compatibility needs separate Windows
validation.

**Windows runtime validation required:** On Windows, verify CPU/GL rendering
and resize across mixed DPI displays; `CanvasSize`, `Info` and `RawInfo` with
scaling both on and off; GL context loss/recreation and render-loop navigation;
mouse, pen and multitouch capture/cancellation; and MAUI image-source service
behavior, including Skia sources, null clearing and non-Skia fallback. A
macOS cross-build cannot validate WPF rendering or input.

The parent Gallery's WPF app must upgrade its MAUI dependencies from 10.0.20
to at least 10.0.41 to satisfy this backend package; that change belongs to
the Gallery owner. This package does not change Gallery or stable MAUI assets.

For a macOS cross-build of the project and its WPF project references, pass
`-p:EnableWindowsTargeting=true -p:WindowsDesktopTargetFrameworks=net10.0-windows`
to `dotnet build` or `dotnet pack`. Run the focused test project in
`tests/SkiaSharp.Views.Maui.Controls.WPF.Tests` on Windows; it cannot execute
with the Windows Desktop runtime on macOS.
