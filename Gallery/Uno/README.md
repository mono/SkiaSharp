# SkiaSharp Gallery — Uno Platform

Browse interactive SkiaSharp demos in the browser or a native app. Search and
category filters help find samples; each sample opens with a canvas and its
controls. Uno's Skia renderer draws the application UI.

## Run on desktop

Install the .NET 10 SDK, then run from this folder:

```sh
dotnet run --project SkiaSharpSample.Uno.csproj -f net10.0-desktop
```

The desktop target supports Windows, macOS, and Linux.

## Run in a browser

Install the WASM workload and publish a static site:

```sh
dotnet workload install wasm-tools
dotnet publish SkiaSharpSample.Uno.csproj \
  -c Release -f net10.0-browserwasm -o output/gallery
python3 -m http.server 5050 --directory output/gallery/wwwroot
```

Open <http://localhost:5050/>. The local server command requires Python 3.

## Other platforms

| Platform | Framework | Requirements |
| --- | --- | --- |
| Windows / WinUI | `net10.0-windows10.0.26100` | Windows |
| Android | `net10.0-android` | Android workload and device/emulator |
| iOS | `net10.0-ios` | macOS, Xcode, iOS workload and device/simulator |

Use `dotnet run --project SkiaSharpSample.Uno.csproj -f FRAMEWORK` with the chosen
framework. See the [Uno setup guide](https://platform.uno/docs/articles/intro.html)
for native platform prerequisites.

## Try the gallery

Search or choose a category, then open a sample card. **All categories** clears
the category selection; **Reset filters** clears an empty search.
The detail page provides drawing controls and a back link.
