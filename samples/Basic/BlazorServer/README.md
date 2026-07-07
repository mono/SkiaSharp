# SkiaSharp on Blazor Server

This is the **same application** as the [Blazor WebAssembly sample](../BlazorWebAssembly),
hosted with **Blazor Server** instead. The pages, layout, styles, MudBlazor theme and SkiaSharp
drawing code are identical — only the hosting model differs.

It demonstrates [#1194](https://github.com/mono/SkiaSharp/issues/1194): the same `SKCanvasView`
and `SKGLView` components that run in Blazor WebAssembly also work under Blazor Server, because
the views detect their host at runtime and switch to a bridged rendering path (SkiaSharp renders
on the server; frames are streamed to the browser and painted into the `<canvas>`).

## Run

```bash
cd SkiaSharpSample
dotnet run
```

Open the printed URL (for example `http://localhost:5000`). You should see the same three pages
as the WebAssembly sample:

- **CPU** — `SKCanvasView` with gradients, shapes and text.
- **GPU** — `SKGLView` with a continuously animated SkSL shader.
- **Drawing** — `SKCanvasView` with pointer/wheel input and on-demand rendering.

## What differs from the WebAssembly sample

Only the host wiring:

- `SkiaSharpSample.csproj` uses the Web SDK and does not reference the WebAssembly packages or
  the WebAssembly native-asset props.
- `Program.cs` builds a `WebApplication`, calls `AddRazorComponents().AddInteractiveServerComponents()`
  and `MapRazorComponents<App>().AddInteractiveServerRenderMode()`, and registers the same
  `HttpClient` and MudBlazor services.
- `App.razor` is the host document (the equivalent of the WebAssembly `wwwroot/index.html`) and
  loads `blazor.web.js`; `Routes.razor` holds the router that was `App.razor` in the WebAssembly
  sample.

Everything else — `Layout/MainLayout.razor`, `Pages/Home.razor`, `Pages/GPU.razor`,
`Pages/Drawing.razor`, the scoped `*.razor.css`, `wwwroot/css/app.css`, the font and
`FpsCounter`/`SamplePage` — is copied verbatim.

See [documentation/dev/blazor-server-hybrid-rendering.md](../../../documentation/dev/blazor-server-hybrid-rendering.md)
for the design.
