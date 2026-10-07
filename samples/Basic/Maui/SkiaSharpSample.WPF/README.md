# Experimental Basic MAUI WPF sample

This Windows-only .NET 10 head links the existing Basic MAUI app, Shell, pages,
and resources. It uses the experimental dotnet/maui-labs WPF backend
**0.1.0-preview.12.26421.1**, MAUI Controls **10.0.51**, and
`UseSkiaSharpWPF()`; no preview dependencies are added to the shared MAUI project.
Debug builds enable the WPF DevFlow agent.

From the repository root, after the repository's native bootstrap:

```powershell
dotnet build samples\Basic\Maui\SkiaSharpSample.WPF\SkiaSharpSample.WPF.csproj
dotnet run --project samples\Basic\Maui\SkiaSharpSample.WPF\SkiaSharpSample.WPF.csproj
```

Use the Shell flyout for **CPU**, **GPU**, and **Drawing**.
GPU rendering uses the real WPF OpenGL control. SkiaSharp image-source support
is deferred for this experimental backend.

This head is included in the existing MAUI sample solution and Windows filter.
WPF, the MAUI workload, and the .NET 10 SDK are required on Windows.
