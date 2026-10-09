# SkiaSharp Gallery — .NET MAUI

Explore interactive SkiaSharp demos in a native app for Android, iOS,
Mac Catalyst, and Windows. Search by sample, category, or API; switch between
CPU/GPU rendering; and adjust controls while a sample runs.

Document samples can export files through the platform's Open/Share actions.
PDF previews open in an external viewer; XPS is available on Windows.

## Build and run

Install the .NET 10 SDK and the MAUI workload. Apple targets also require a
compatible Xcode installation. Run these commands from this folder:

```sh
dotnet workload restore SkiaSharpSample.Maui.csproj
dotnet build SkiaSharpSample.Maui.csproj -f net10.0-maccatalyst -t:Run
```

For another platform, replace the framework with:

| Platform | Framework | Build host |
| --- | --- | --- |
| Android | `net10.0-android` | Windows or macOS |
| iOS | `net10.0-ios` | macOS |
| Windows | `net10.0-windows10.0.19041.0` | Windows |

Android/iOS deployment requires a connected device or simulator. If installing
an Android Debug APK manually, build with `-p:EmbedAssembliesIntoApk=true` so it
contains the managed assemblies.

## Try the gallery

Use search and filters to find a sample, then open its card. The detail page
provides a canvas and any controls exposed by the sample. Unsupported samples
remain visible but cannot be opened.

The header offers CPU/GPU selection, System/Light/Dark appearance, and an Info
panel with library versions.
