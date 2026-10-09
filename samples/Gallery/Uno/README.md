# SkiaSharp Gallery — Uno Platform

Uno Platform host for the shared SkiaSharp gallery catalog.

## Target matrix

This is a full Uno single-project sample. The project file declares:

| TFM | Platform | Build on |
|---|---|---|
| `net10.0-browserwasm` | Browser (WebAssembly) | any OS |
| `net10.0-desktop` | Skia Desktop (Win/macOS/Linux/X11) | any OS |
| `net10.0-windows10.0.26100` | WinUI 3 (WinAppSDK) | Windows only |
| `net10.0-android` | Android | Windows / macOS |
| `net10.0-ios` | iOS | Windows / macOS |

## Prerequisites

Install the .NET 10 SDK. From this sample's folder:

```bash
# Install the WASM workload for the browser target.
dotnet workload install wasm-tools

# Optionally install the workloads for the native heads you want to build
#    on your machine, e.g.:
#      dotnet workload install android
#      dotnet workload install ios
#      dotnet workload install maui
```

SkiaSharp and its native assets are restored from NuGet packages.

## Build and run

### WebAssembly

```bash
dotnet publish SkiaSharpSample.Uno.csproj \
  -c Release \
  -f net10.0-browserwasm \
  -o output/gallery-uno-publish

python3 -m http.server 5050 --directory output/gallery-uno-publish/wwwroot
```

Then open `http://localhost:5050/`. The gallery loads with all samples from
the shared catalog and renders through Uno's Skia renderer.
Its category chips show only categories with matches for the current query,
in alphabetical order. A single selected category narrows results; **All
categories** clears that choice. When no results match, **Reset filters**
explicitly clears both the category and search instead of broadening silently.

### Desktop (Skia on X11 / Win32 / macOS)

```bash
dotnet run --project SkiaSharpSample.Uno.csproj -f net10.0-desktop
```

### Windows (WinUI 3)

On Windows only, from Visual Studio or CLI:

```bash
dotnet run --project SkiaSharpSample.Uno.csproj -f net10.0-windows10.0.26100
```

### Android / iOS

Requires the corresponding workloads and a connected device/simulator. See
the Uno Platform docs at https://platform.uno/docs/articles/intro.html for
the full native-head setup.

## Headless smoke test

```bash
bash scripts/smoke.sh output/gallery-uno-publish/wwwroot
```

First run installs Playwright and its Chromium binary into
`scripts/node_modules` and Playwright's browser cache
(subsequent runs reuse). Exit code `0`
means boot completed (Uno loader dismissed, `#uno-canvas` sized > 0×0) and
no unignored console errors were captured.

Optional: `--screenshot /path/to/debug.png` saves a rendered page screenshot
for manual inspection.

## Structure

```
Uno/
├── SkiaSharpSample.Uno.csproj    # Uno.Sdk/6.6.0-dev.208, all platforms, UnoFeatures=SkiaRenderer
├── nuget.config                  # scoped nuget.org source for Uno.WinUI.Runtime.Skia.WebAssembly.Browser
├── App.xaml(.cs)                 # application entry; SampleService singleton
├── MainPage.xaml(.cs)            # navigation shell: top bar + Frame
├── CategoryColors.cs             # palette + Bootstrap-Icons codepoints (matches Blazor)
├── GlobalUsings.cs               # SkiaSharp + shared usings
├── Package.appxmanifest          # WinUI package manifest
├── app.manifest                  # Windows desktop activation manifest
├── Pages/
│   ├── HomePage.xaml(.cs)        # hero + search + category chips + card grid
│   └── SamplePage.xaml(.cs)      # canvas + controls + back link
├── Controls/
│   ├── ControlPanelView.xaml(.cs)
│   └── SampleCard.xaml(.cs)
├── Platforms/
│   ├── Android/                  # Main.Android.cs, AndroidManifest.xml, …
│   ├── Desktop/Program.cs        # UnoPlatformHostBuilder with UseX11/UseWin32/UseMacOS
│   ├── iOS/                      # Main.iOS.cs, Info.plist, Entitlements.plist
│   └── WebAssembly/              # Program.cs (UnoPlatformHostBuilder.UseWebAssembly), LinkerConfig,
│                                 # WasmCSS/Fonts.css, WasmScripts/AppManifest.js, manifest, wwwroot
├── Assets/
│   ├── Fonts/bootstrap-icons.ttf # vendored 1.11.3 — same icon set the Blazor host loads via CDN
│   └── …                         # icon / splash (Uno single-project template)
├── Strings/en/Resources.resw
├── Properties/
│   ├── PublishProfiles/          # win-x64 / win-arm64 / win-x86
│   └── launchSettings.json
└── scripts/
    ├── smoke.sh                  # headless smoke test runner
    ├── smoke-driver.mjs          # Playwright driver
    └── package.json              # pins playwright
```

The project pins `SkiaSharpVersion` so Uno uses the same SkiaSharp version as
the sample's package references, rather than the version bundled with Uno.

### UI parity with the Blazor gallery

The host mirrors the Blazor gallery structurally: navy top bar with brand +
CPU/GPU backend segment + theme toggle + GitHub link; hero with gradient
accent + search; category-color filter chips; responsive card grid (left
accent bar + tinted background per category + title + description + category
badge + type badge). Sample detail: back link + sample header + SkiaSharp
canvas + collapsible controls sidebar + canvas debug footer. The drawing
code itself (`SampleBase.DrawSample(SKCanvas, …)`) is reused verbatim from
the shared catalog, so the two hosts use the same drawing implementation.

Iconography matches the Blazor host: the same Bootstrap-Icons 1.11.3 glyph
set is bundled as a TTF at `Assets/Fonts/bootstrap-icons.ttf` and referenced
via `FontFamily="ms-appx:///Assets/Fonts/bootstrap-icons.ttf#bootstrap-icons"`.
The codepoint lookup in `CategoryColors.Icon(title)` mirrors Blazor's
`Home.GetSampleIcon(title)` switch, so the icon glyph for each sample is
identical across the two hosts (e.g. `bi-arrows-move` for "2D Transforms",
`bi-layers-half` for "Blend Modes", `bi-brush` for unrecognized titles).
The theme toggle uses `bi-moon-stars` / `bi-sun` from the same font.

### SkiaRenderer

The csproj enables the `SkiaRenderer` Uno feature, so Uno composes the whole
XAML tree through its Skia compositor on WebAssembly (and on the Skia-based
desktop heads). Required ingredients:

- **`Uno.Sdk/6.6.0-dev.208`** — the first Uno.Sdk line in which SkiaRenderer
  is wired for WebAssembly out of the box. Re-pin to the 6.6 stable release
  once Uno publishes one.
- **`UnoPlatformHostBuilder` in `Platforms/WebAssembly/Program.cs`**
  (`.UseWebAssembly()` on the builder), not the older
  `Microsoft.UI.Xaml.Application.Start(...)` pattern. SkiaRenderer depends
  on the new hosting API.
- **`nuget.config`** adds nuget.org for Uno and its WebAssembly dependencies.

### Linker

The shared catalog (`SkiaSharpSample.Shared`) enumerates `SampleBase`
subclasses via reflection over `Assembly.DefinedTypes`. Under Uno.Sdk 6.6
Release, the linker is aggressive and trims reflection-only types by
default, which would leave the catalog empty. `Platforms/WebAssembly/LinkerConfig.xml`
preserves both the host assembly and `SkiaSharpSample.Shared` to keep the
full catalog reachable.

### Click handling

`UserControl` children don't always receive `Tapped` / `PointerReleased`
events reliably under Uno 6.6-dev SkiaRenderer (the events get marked
Handled by inner TextBlocks before reaching the UserControl). The fix is
the same pattern Blazor uses: each sample card is wrapped in a
`HyperlinkButton` (Blazor wraps in `<a href="sample/...">`), and the
`Click` event navigates. `HyperlinkButton.Click` fires reliably in
SkiaRenderer.
