# SkiaSharp on Blazor Server

A minimal Blazor Web App (Interactive Server) that draws with SkiaSharp **on the server** and
streams the rendered frames to the browser over the Blazor Server (SignalR) circuit.

This demonstrates [#1194](https://github.com/mono/SkiaSharp/issues/1194): the same
`SKCanvasView` that runs in Blazor WebAssembly also works under Blazor Server, because the
view detects its host at runtime and switches to a bridged rendering path.

## Run

```bash
cd SkiaSharpSample
dotnet run
```

Then open the printed URL (for example `http://localhost:5000`). The canvas is rendered by the
.NET SkiaSharp runtime on the server, encoded (JPEG by default for Server), and streamed to the
browser. Move the mouse over the canvas to interact.

## How it works

- `Program.cs` registers Interactive Server components and (optionally) configures global
  SkiaSharp view defaults via `AddSkiaSharpViewsBlazor(...)`.
- `Components/Pages/Home.razor` uses `<SKCanvasView>` exactly as it would in WebAssembly. The
  `OnPaintSurface` callback draws with the normal SkiaSharp API.
- Because the page uses `@rendermode InteractiveServer`, the view renders on the server and
  streams frames; in a WebAssembly app the identical component would draw directly in the
  browser.

See [documentation/dev/blazor-server-hybrid-rendering.md](../../../../documentation/dev/blazor-server-hybrid-rendering.md)
for the full design.

## Notes

- Server streams frames over the network, so `EnableRenderLoop` animations are best kept at a
  modest rate; the render loop is backpressured (it never outruns the transport).
- The transfer format is configurable globally (in `Program.cs`) and per control (via the
  `TransferFormat` / `Quality` parameters on `SKCanvasView`). Server defaults to JPEG; set
  `TransferFormat="SKBlazorTransferFormat.Png"` when you need transparency.
