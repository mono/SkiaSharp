# Experimental SkiaSharp MAUI WPF handlers

`SkiaSharp.Views.Maui.Controls.WPF` is an experimental, opt-in package for the
[dotnet/maui-labs](https://github.com/dotnet/maui-labs) **WPF** backend, not MAUI's
WinUI backend. It targets Windows with .NET 10 and MAUI Controls **10.0.51**,
and pins `Microsoft.Maui.Platforms.Windows.WPF` to **0.1.0-preview.12.26421.1**.
Keep this preview dependency in WPF applications, separate from stable MAUI apps.

In an application configured with `UseWPF` and `UseMaui`, register the WPF backend
and SkiaSharp handlers:

```csharp
using Microsoft.Maui.Controls.Hosting.WPF;
using Microsoft.Maui.Hosting;
using SkiaSharp.Views.Maui.Controls.Hosting;

var builder = MauiApp.CreateBuilder()
	.UseMauiAppWPF<App>()
	.UseSkiaSharpWPF();
```

This registers only `SKCanvasView` and `SKGLView`,
using software-backed `SKElement` and real OpenGL-backed `SKGLElement`.
Paint and touch events retain their existing semantics, including pixel scaling,
physical `RawInfo`, render-loop lifecycle, and pointer capture cancellation.
Continuous rendering follows WPF compositor timing, not a fixed-rate timer.
Paint surfaces are borrowed; do not retain or dispose them.
Use `UseSkiaSharpWPF()` instead of calling `UseSkiaSharp()` separately.
SkiaSharp image sources are not supported by this experimental backend yet.
