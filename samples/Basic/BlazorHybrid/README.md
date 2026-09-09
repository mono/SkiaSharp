# SkiaSharp on Blazor Hybrid (.NET MAUI)

This is the **same application** as the [Blazor WebAssembly](../BlazorWebAssembly) and
[Blazor Server](../BlazorServer) samples, packaged as a **.NET MAUI Blazor Hybrid** mobile app.
The pages, layout, styles, MudBlazor theme and SkiaSharp drawing code are identical — only the
host is different (a native `BlazorWebView` instead of a browser or a server).

It demonstrates [#1194](https://github.com/mono/SkiaSharp/issues/1194): the same `SKCanvasView`
and `SKGLView` components run under Blazor Hybrid because the views detect their host at runtime
(a WebView) and render on the .NET side in-process, presenting frames into the WebView's
`<canvas>`.

## Run

```bash
# Mac (Mac Catalyst)
dotnet build SkiaSharpSample -t:Run -f net10.0-maccatalyst

# Android (device or emulator running)
dotnet build SkiaSharpSample -t:Run -f net10.0-android

# iOS (simulator or device)
dotnet build SkiaSharpSample -t:Run -f net10.0-ios
```

You should see the same three pages as the other samples, via the drawer:

- **CPU** — `SKCanvasView` with gradients, shapes and text.
- **GPU** — `SKGLView` with a continuously animated SkSL shader (on a WebGL-backed canvas).
- **Drawing** — `SKCanvasView` with pointer input and on-demand rendering.

## What differs from the WebAssembly / Server samples

Only the host wiring — the shared pages, layout, scoped CSS, `FpsCounter`, `SamplePage` and
`wwwroot/css/app.css` are byte-identical.

- Scaffolded from the `maui-blazor` template, so all the platform projects (`Platforms/`),
  `MauiProgram.cs`, `App.xaml`, `MainPage.xaml` (the `BlazorWebView` hosting `Routes` at `#app`)
  and `Resources/` are standard MAUI.
- `MauiProgram.cs` registers `AddMauiBlazorWebView()` and `AddMudServices()`.
- `wwwroot/index.html` is the WebView host page (loads `blazor.webview.js`, MudBlazor and
  `css/app.css`).
- **Font loading:** `Home.razor` loads `NotoSans-Regular.ttf` as a **raw app-package asset** via
  `FileSystem.OpenAppPackageFileAsync` (no `HttpClient`), since a native WebView has no server to
  fetch from. The font is included both as a `Resources/Raw` asset (for the C# canvas text) and
  in `wwwroot/fonts` (for the CSS `@font-face`). This is the only line that differs from the
  WebAssembly/Server `Home.razor`.
- `Program.DefaultPage` (used by the shared `MainLayout`) is replaced with `App.DefaultPage`
  in `App.xaml.cs` — the idiomatic MAUI place and the same approach as the native
  [`samples/Basic/Maui`](../Maui) sample. This is the one line that differs in the Hybrid
  `MainLayout.razor` from the WebAssembly/Server samples.
- `ValidateXcodeVersion=false` lets the sample build with a newer Xcode than the Apple SDK's exact
  recommended version (as the repo's test projects do).

See [documentation/dev/blazor-server-hybrid-rendering.md](../../../documentation/dev/blazor-server-hybrid-rendering.md)
for the design.
