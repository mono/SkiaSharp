# SkiaSharp Gallery — Uno Platform

Uno Platform host for the shared SkiaSharp gallery catalog
(`samples/Gallery/Shared/`), alongside the existing Blazor gallery at
`samples/Gallery/Blazor/`.

**Hosted gallery (WebAssembly)**: https://mono.github.io/SkiaSharp/gallery-uno/ *(live after this feature merges)*

## Target matrix

This is a full Uno single-project sample. The project file declares:

| TFM | Platform | Build on | CI auto-deploy |
|---|---|---|---|
| `net10.0-browserwasm` | Browser (WebAssembly) | any OS | **yes** — `build-site.yml` |
| `net10.0-desktop` | Skia Desktop (Win/macOS/Linux/X11) | any OS | no |
| `net10.0-windows10.0.26100` | WinUI 3 (WinAppSDK) | Windows only | no |
| `net10.0-android` | Android | Windows / macOS | no |
| `net10.0-ios` | iOS | Windows / macOS | no |

The CI workflow publishes only the WebAssembly head
(`dotnet publish -f net10.0-browserwasm`) because that is what the docs site
deploys. The remaining heads are there for local exploration.

## Prerequisites

From the repo root:

```bash
# 1. Download the pre-built native SkiaSharp binaries (one-time)
dotnet cake --target=externals-download

# 2. Install the .NET WASM workload (only needed for the browserwasm target)
dotnet workload install wasm-tools

# 3. (Optional) Install the workloads for the native heads you want to build
#    on your machine, e.g.:
#      dotnet workload install android
#      dotnet workload install ios
#      dotnet workload install maui
```

If you're modifying native SkiaSharp C code, use `dotnet cake --target=externals-linux --arch=wasm`
instead of `externals-download`. See the top-level `AGENTS.md` for details.

## Build and run

### WebAssembly (what CI ships)

```bash
dotnet publish samples/Gallery/Uno/SkiaSharpSample.Uno.csproj \
  -c Release \
  -f net10.0-browserwasm \
  -o output/gallery-uno-publish

python3 -m http.server 5050 --directory output/gallery-uno-publish/wwwroot
```

Then open `http://localhost:5050/`. The gallery loads with all samples from
the shared catalog and renders through Uno's Skia renderer.

### Desktop (Skia on X11 / Win32 / macOS)

```bash
dotnet run --project samples/Gallery/Uno/SkiaSharpSample.Uno.csproj -f net10.0-desktop
```

### Windows (WinUI 3)

On Windows only, from Visual Studio or CLI:

```bash
dotnet run --project samples/Gallery/Uno/SkiaSharpSample.Uno.csproj -f net10.0-windows10.0.26100
```

### Android / iOS

Requires the corresponding workloads and a connected device/simulator. See
the Uno Platform docs at https://platform.uno/docs/articles/intro.html for
the full native-head setup.

## Headless smoke test

```bash
bash samples/Gallery/Uno/scripts/smoke.sh output/gallery-uno-publish/wwwroot
```

First run installs Playwright and its Chromium binary into
`samples/Gallery/Uno/scripts/node_modules` and `~/.cache/ms-playwright`
(subsequent runs reuse). Total cost: ~60s on `ubuntu-latest`. Exit code `0`
means boot completed (Uno loader dismissed, `#uno-canvas` sized > 0×0) and
no unignored console errors were captured. See
`specs/001-uno-wasm-sample/contracts/smoke-test.md` for the full contract.

Optional: `--screenshot /path/to/debug.png` saves a rendered page screenshot
for manual inspection.

## Structure

```
samples/Gallery/Uno/
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

The csproj imports `samples/_UnoPlatformSamples.targets` *before* declaring
its `ProjectReference` to `samples/Gallery/Shared/SkiaSharpSample.Shared.csproj`.
The targets file strips the ambient NuGet `SkiaSharp*` / `HarfBuzzSharp*`
package assets and re-points them at the in-tree projects per-TFM, so the
host builds with the local SkiaSharp instead of a pinned NuGet version.

### UI parity with the Blazor gallery

The host mirrors the Blazor gallery structurally: navy top bar with brand +
CPU/GPU backend segment + theme toggle + GitHub link; hero with gradient
accent + search; category-color filter chips; responsive card grid (left
accent bar + tinted background per category + title + description + category
badge + type badge). Sample detail: back link + sample header + SkiaSharp
canvas + collapsible controls sidebar + canvas debug footer. The drawing
code itself (`SampleBase.DrawSample(SKCanvas, …)`) is reused verbatim from
`samples/Gallery/Shared/`, so the two hosts always paint the same pixels.

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
- **`samples/Gallery/Uno/nuget.config`** adds nuget.org as a scoped source,
  because `Uno.WinUI.Runtime.Skia.WebAssembly.Browser` is on nuget.org but
  not mirrored into the repo-wide `dotnet-public` feed.

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
