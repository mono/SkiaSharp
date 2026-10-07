# Native .NET MAUI gallery

This host shares the **entire** gallery catalog and sample implementations with
the Uno and Blazor hosts. At 920 DIP and wider, a persistent filter rail sits
beside the virtualized sample grid; narrower windows use a search bar and
funnel-triggered filter popover. One shared search field filters samples, categories,
and API facets across the rail, compact bar, and popup. Search uses a themed
border with a native borderless field on Mac Catalyst. Categories and API facets
are alphabetical; only currently matching choices appear. The filter panel
selects one category at a time (or All), intersects all selected API type/method
tags, shows live result counts, and offers a separate compact sort menu with
four orders. Both menus dismiss on the backdrop or system Back;
sort closes on selection, while filters stay open for multi-selection. Active
filters remain visible as removable pills even if no samples match. API tags
use rounded monospace pills in the rail, active filters, and sample details.
A shared navy header carries the
CPU/GPU choice across gallery and detail pages; the choice affects canvases,
not generated documents. The header's Info glyph opens a themed panel with
SkiaSharp, HarfBuzzSharp, and build details instead of occupying a permanent
footer. Its compact appearance menu defaults to **System**
and also offers explicit Light/Dark, with a route back to System. Colors follow
OS changes through MAUI theme bindings when System is selected. On mobile the
navy page background extends behind the status bar, while content observes
safe areas and status icons remain light. Unsupported samples remain visible
but cannot be opened.
The detail view draws with the native SkiaSharp MAUI CPU or GPU view, exposes
sample-provided controls (a wide-screen collapse reclaims the entire canvas
column, while narrow screens keep the toggle below the canvas), and opens/shares
generated PDF, XPS (Windows), and
other downloadable files through platform file APIs. The document preview
explains that native Open/Share actions are available; it is not an embedded PDF
viewer. Temporary exported files are stored in the app's cache.

Page layouts, popovers, cards, search fields, and recursive sample controls are
XAML views with typed compiled bindings. Code-behind coordinates filtering,
navigation, native canvas lifetimes, and the per-window renderer choice; the
collection view is replaced at column breakpoints rather than mutating a live
Apple collection layout.

## Build and run on Mac Catalyst

Use the repository's .NET SDK and the MAUI workload. For a repository checkout,
download the current native binaries once before a managed/sample-only build:

```sh
dotnet cake --target=externals-download
dotnet build samples/Gallery/Maui/SkiaSharpSample.Maui.csproj \
  -f net10.0-maccatalyst -t:Run
```

Do not download pre-built binaries after changing native code, the Skia
submodule, or native version inputs; build the matching natives from source
instead. Published sample archives use NuGet packages and do not need the
repository bootstrap.

## MAUI DevFlow

Run the app in Debug, then identify its own agent before driving it:

```sh
maui devflow list
maui devflow --agent-port PORT agent status
maui devflow --agent-port PORT ui tree --depth 3
```

Replace `PORT` with the **SkiaSharp Gallery** port, especially when other apps
are running. `agent status` must report `com.skiasharp.gallery.maui`. The
gallery deliberately has stable automation IDs for the header, filters,
popovers, sample cards, render surfaces, and parameter controls.

The repeatable smoke check uses one DevFlow batch session, verifies the app
identity, and writes a JSON report, command transcript, and screenshots:

```sh
python3 samples/Gallery/Maui/scripts/smoke.py --port PORT \
  --all-samples --output output/maui-gallery-smoke
```

It exercises search, empty results, single-category/API intersection filtering,
sort, appearance and Info popovers, desktop narrow/wide transitions, native CPU/GPU rendering,
live controls, nested groups, animation navigation, and PDF generation. A
nonzero exit is a failure, not a skipped capability. `--share` requests the
native PDF share sheet last and leaves it open for inspection.

The shared catalog and filter model also have regression tests in the
repository's console test solution; see the [gallery test instructions](../README.md#testing).

## Targets and packaging

Android and iOS use the corresponding `net10.0-android` and `net10.0-ios`
frameworks; Windows uses `net10.0-windows10.0.19041.0` from a Windows host.
Use the SDK's `-t:Run` target to deploy to a selected device. If installing an
Android Debug APK manually rather than using SDK deployment, build with
`-p:EmbedAssembliesIntoApk=true`; otherwise Fast Deployment places managed
assemblies beside the APK and installing that APK alone is insufficient.

This app pins MAUI Controls 10.0.51 without changing the library versions.
This patch includes the Apple layer finalizer correction from
[dotnet/maui#33818](https://github.com/dotnet/maui/pull/33818); earlier versions
can crash while collecting borders and shapes after navigation.
The DevFlow agent and Mac Catalyst network-server entitlement are **Debug
only**. Release builds preserve the reflection-discovered Shared sample
catalog via `TrimmerRoots.xml`; no sample list is maintained by this host.
The app needs no filesystem permission to use its cache or native share sheet.

The bundled Bootstrap Icons font is registered as `BootstrapIcons`; the
gallery uses named glyphs and font-image sources rather than platform-dependent
emoji. Filter and sort content share an attached popover implementation, with
fresh content per opening, a themed scrim, viewport clamping, and cleanup on
dismissal or navigation.
