# Experimental native AppKit Gallery

This host links the unmodified `../` MAUI XAML gallery and the Shared sample
catalog. It registers `SkiaSharp.Views.Maui.Controls.MacOS` after
`UseMauiAppMacOS<App>()`: CPU canvases use CoreGraphics and GPU canvases use
Metal. It is not Mac Catalyst and requires macOS 14+, .NET 10, MAUI 10.0.51
or newer, and the native `Microsoft.Maui.Platforms.MacOS` preview backend.
The standard Gallery project and stable MAUI packages are unaffected.

After preparing **matching native** SkiaSharp binaries for the current Skia
submodule with `dotnet cake --target=externals-macos --arch=arm64`, build:

```sh
dotnet build samples/Gallery/Maui/AppKit/SkiaSharpSample.MacOS.csproj \
  -f net10.0-macos -p:IsNetMacOSSupported=true
```

Run the resulting `.app/Contents/MacOS/SkiaSharpSample.MacOS` executable.
Never use `externals-download` after a native or milestone update. The shared
Gallery's filter and sort popovers, system/light/dark theme choices, sample
controls and CPU/GPU toggle should be checked in the AppKit window: this
backend's native layout invalidation and overlay hit testing differ from
Mac Catalyst.
The AppKit head supplies layout child commands missing from the pinned preview
backend and requests fresh managed/native layout when MAUI's `BindableLayout`
changes children; without those commands filter facets and active chips
retain stale native views or zero-size frames.

The host registers the backend's `MacOS.Essentials` before the optional Debug
DevFlow Agent. Without Essentials, the agent calls MAUI's portable
`FileSystem.CacheDirectory` reference implementation and fails at startup.
It also bundles the Gallery's Bootstrap font in `Contents/Resources/Fonts/`,
where the AppKit font registrar looks for it.
For a running Debug app, find its port with `maui devflow list`, then run the
shared Gallery smoke script against this host's distinct app ID:

```sh
python3 samples/Gallery/Maui/scripts/smoke.py --port PORT \
  --expected-app-id com.skiasharp.gallery.appkit \
  --output output/appkit-gallery-smoke
```
