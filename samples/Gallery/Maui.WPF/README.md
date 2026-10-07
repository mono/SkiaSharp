# Experimental WPF MAUI Gallery

This Windows-only Gallery uses the experimental `dotnet/maui-labs` WPF backend,
not WinUI. It links the [shared MAUI Gallery](../Maui/README.md) catalog and XAML
pages, with SkiaSharp's WPF software canvas and real OpenGL handlers.

Use Windows with the .NET 10 SDK and MAUI build prerequisites. This project
requires MAUI Controls 10.0.51 and WPF Labs preview 0.1.0-preview.12.26421.1.
From the repository root, bootstrap native assets before building:

```powershell
dotnet cake --target=externals-download
dotnet build samples\Gallery\Maui.WPF\SkiaSharpSample.Maui.WPF.csproj
dotnet run --project samples\Gallery\Maui.WPF\SkiaSharpSample.Maui.WPF.csproj
```

Use `externals-download` only for managed-only changes. After any native, C API,
dependency, or Skia milestone changes, build the matching native binaries with
`dotnet cake --target=externals-windows --arch=x64` instead.
