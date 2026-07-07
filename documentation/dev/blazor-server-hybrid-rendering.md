# SkiaSharp Blazor Views — Behaviour Specification

Component: `SkiaSharp.Views.Blazor` — `SKCanvasView`, `SKGLView`
Related issue: [#1194 — Support SkiaSharp as a Blazor Extension](https://github.com/mono/SkiaSharp/issues/1194)

> This is a **normative specification** of how the SkiaSharp Blazor views behave across every
> Blazor hosting model. It describes the system as a whole and in the absolute — not as a set
> of changes relative to an earlier version. "MUST/SHOULD/MAY" are used in the RFC 2119 sense.
> §14 maps every normative rule to the code that implements it.

---

## 1. Purpose and scope

`SkiaSharp.Views.Blazor` provides two Razor components, `SKCanvasView` and `SKGLView`, that let
an application draw with SkiaSharp and have the result displayed in an HTML `<canvas>`. The same
two components MUST function under every Blazor hosting model:

- **Blazor WebAssembly** — the app's .NET code runs in the browser.
- **Blazor Server** — the app's .NET code runs on the server; the browser is a thin client
  connected over a SignalR circuit.
- **Blazor Hybrid** — the app's .NET code runs in a native host process (`BlazorWebView` in
  MAUI/WPF/WinForms) and the UI is a WebView.
- **Static server-side rendering (SSR)** — the component is prerendered on the server and is
  not (yet) interactive.
- **Interactive Auto** — the component renders with Blazor Server on first visit and with
  Blazor WebAssembly on subsequent visits.

The public API and the drawing contract (§3) are identical across all hosts. What differs is
*where* drawing happens and *how* pixels reach the `<canvas>` — this is captured by the two
**rendering strategies** in §6 and §7, selected per host in §4.

## 2. Terminology

- **Host** — the Blazor hosting model a component instance executes under.
- **Direct strategy** — SkiaSharp draws in the browser and writes pixels straight into the
  `<canvas>` (§6). Used only in WebAssembly.
- **Bridged strategy** — SkiaSharp draws on the .NET side; the frame is transferred to the
  browser and a JavaScript module paints it into the `<canvas>` (§7). Used for Server, Hybrid
  and static SSR.
- **Present** — the act of making a rendered frame visible in the `<canvas>`.
- **Frame** — one rendered image.
- **CSS size** — the layout size of the `<canvas>` element in CSS pixels
  (`clientWidth`/`clientHeight`).
- **Backing-store size** — the pixel grid the `<canvas>` actually holds (`canvas.width` /
  `canvas.height`).
- **DPR** — `window.devicePixelRatio`.

## 3. Public surface and drawing contract

### 3.1 Rendered element

Each component renders exactly one element:

```razor
<canvas @ref="..." @attributes="AdditionalAttributes" />
```

The component MUST splat `AdditionalAttributes` onto this `<canvas>` so that applications can
attach standard Blazor attributes and event handlers (for example `style`, `class`,
`@onpointerdown`, `@onwheel`) uniformly across all hosts.

### 3.2 Components and parameters

`SKCanvasView` (CPU/raster surface) and `SKGLView` (GPU surface) share this parameter surface:

| Member | Meaning |
|--------|---------|
| `OnPaintSurface` | Callback invoked to paint a frame. `SKCanvasView` supplies `SKPaintSurfaceEventArgs`; `SKGLView` supplies `SKPaintGLSurfaceEventArgs`. |
| `EnableRenderLoop` (bool) | When `true`, frames are produced continuously; when `false`, only on demand. Changing it calls `Invalidate()` (§8). |
| `IgnorePixelScaling` (bool) | Selects the coordinate space handed to `OnPaintSurface` (§5.3). Changing it calls `Invalidate()`. |
| `AdditionalAttributes` | Unmatched attributes, splatted onto the `<canvas>`. |
| `Dpi` (double, get) | The device pixel ratio currently in effect (§5). |
| `Invalidate()` | Requests that a frame be rendered (§8). |
| `TransferFormat` (`SKBlazorTransferFormat?`) | Bridged-only frame encoding (§7.4). Ignored by the Direct strategy. |
| `Quality` (`int?`) | JPEG quality for the bridged JPEG format (§7.4). Ignored otherwise. |

`TransferFormat` and `Quality` are available on .NET 9.0 and later (see §12).

### 3.3 Options and dependency injection

- `enum SKBlazorTransferFormat { Png, Jpeg, Put }` — the bridged transfer encodings (§7.4).
- `sealed class SKBlazorOptions { SKBlazorTransferFormat? TransferFormat; int Quality = 85; }` —
  global defaults.
- `IServiceCollection AddSkiaSharpViewsBlazor(Action<SKBlazorOptions>? configure = null)` —
  registers global defaults. Calling it is OPTIONAL; when it is not called, `SKBlazorOptions`
  defaults apply.

All members in §3.2 and §3.3 are additive; no existing public signature changes across hosts.

### 3.4 Failure and lifecycle behaviour

- A component MUST NOT throw during first render on any host; it produces frames once it has a
  positive size and DPR.
- `IDisposable.Dispose()` MUST release the render buffer and any host-specific resources
  (JavaScript module reference, size/DPI observers, native GL objects) and MUST be safe when
  the underlying transport (for example a Server circuit) is already gone.

## 4. Execution location and strategy selection

On first interactive render, a component determines its host and selects a strategy:

| Detected host | `RendererInfo.Name` | Strategy |
|---------------|---------------------|----------|
| WebAssembly | `WebAssembly` | **Direct** (§6) |
| Blazor Server | `Server` | **Bridged** (§7) |
| Blazor Hybrid | `WebView` | **Bridged** (§7) |
| Static SSR | `Static` | **Bridged** (§7), non-interactive (§11) |

Rules:

1. On .NET 9.0+ the host MUST be resolved from `ComponentBase.RendererInfo.Name`.
2. If the name is unavailable (any target framework earlier than .NET 9.0, where
   `RendererInfo` does not exist), the component MUST fall back to
   `OperatingSystem.IsBrowser()`: when `true` the host is WebAssembly (Direct); otherwise the
   host is treated as unknown and the component takes no interactive action.
3. On target frameworks earlier than .NET 9.0 the components are WebAssembly-only (§12) and
   therefore always use the Direct strategy.
4. Because the WebAssembly fallback keys off `OperatingSystem.IsBrowser()`, a WebAssembly host
   ALWAYS resolves to the Direct strategy even if `RendererInfo` is unavailable.

## 5. Coordinate system, sizing and DPI (all hosts)

These rules are identical for the Direct and Bridged strategies.

### 5.1 Size and DPR sources

- The **CSS size** is observed from the `<canvas>` element's `clientWidth`/`clientHeight` and
  is reported whenever it changes.
- The **DPR** is `window.devicePixelRatio`, sampled on a ~1 second interval (there is no DOM
  event for DPR changes) and reported when it changes.
- A change to either the CSS size or the DPR MUST trigger `Invalidate()`.

### 5.2 Backing-store resolution

The rendered frame (and the `<canvas>` backing store) MUST be sized to `cssWidth × dpr` by
`cssHeight × dpr` device pixels (each dimension truncated to a whole number of pixels), so
output is crisp on high-DPI displays. A component MUST NOT render while the CSS size or DPR is
non-positive.

### 5.3 `IgnorePixelScaling`

`OnPaintSurface` receives two `SKImageInfo` values: a *user-visible* info and a *raw* info.

- When `IgnorePixelScaling` is `false` (default): both infos describe the device-pixel
  backing store. The application draws in device pixels.
- When `IgnorePixelScaling` is `true`: the drawing canvas is pre-scaled by `dpr` (via
  `canvas.Scale(dpr)` followed by `Save`), and the user-visible info describes the CSS-pixel
  size, so the application can draw in CSS-logical units while output remains full-resolution.

### 5.4 `Dpi` property

`Dpi` MUST return the DPR currently in effect for the active strategy (the value most recently
reported by the DPI source).

### 5.5 Input coordinate mapping

Pointer events delivered by Blazor report `OffsetX`/`OffsetY` in CSS pixels. To map an input
point to the coordinate space a frame was drawn in, an application multiplies by
`canvas.width / canvas.clientWidth` (and the height equivalent); this single ratio folds in
both the DPR and any additional CSS scaling of the element. This mapping is identical for all
hosts and is the application's responsibility (the components expose raw events per §3.1).

## 6. Direct strategy (WebAssembly)

In WebAssembly the component owns an in-browser JavaScript module (`SKHtmlCanvas`, §9.2) via
`[JSImport]`/`IJSObjectReference`, keyed to the element id `"_bl_" + ElementReference.Id`, and
SkiaSharp draws using the browser's own WebAssembly `libSkiaSharp`.

### 6.1 Render loop

`Invalidate()` calls the module's `requestAnimationFrame(enableRenderLoop, deviceW, deviceH)`:

- The module sets the `<canvas>` backing-store size to `deviceW × deviceH`.
- It schedules a `requestAnimationFrame`; the callback invokes the .NET render callback, which
  paints one frame (§6.2 / §6.3) and presents it.
- When `EnableRenderLoop` is `true`, the callback reschedules itself each frame, producing a
  browser-paced (display-refresh) loop; when `false`, exactly one frame is produced per
  `Invalidate()`.

### 6.2 `SKCanvasView` (raster)

- The surface uses `SKImageInfo.PlatformColorType` with `SKAlphaType.Opaque`.
- Pixels are rendered into a pinned managed buffer (reused across frames unless the size
  changes) via `SKSurface.Create` over that buffer.
- If `IgnorePixelScaling` is `true`, the canvas is scaled by `dpr` before `OnPaintSurface`.
- Presenting calls the module's `putImageData`, which wraps the pinned buffer as a
  `Uint8ClampedArray` over the emscripten heap and calls `context.putImageData` on a `2d`
  context.

### 6.3 `SKGLView` (GPU)

- The module creates a WebGL context with attributes `{ alpha, depth, stencil: 8, antialias,
  premultipliedAlpha, majorVersion: 2 }`, falling back from WebGL 2 to WebGL 1 if necessary,
  and returns its framebuffer id, sample count, stencil count and depth.
- The component creates a `GRContext` over the GL interface, with a 256 MB resource-cache
  limit, and renders with `SKColorType.Rgba8888` and `GRSurfaceOrigin.BottomLeft`.
- A `GRBackendRenderTarget` is built from the reported framebuffer id / samples / stencils and
  is recreated whenever the size changes or it becomes invalid; the `SKSurface` is created over
  it.
- `OnPaintSurface` receives `SKPaintGLSurfaceEventArgs` referencing the real render target. If
  `IgnorePixelScaling` is `true`, the canvas is scaled by `dpr`. After painting, the canvas and
  the `GRContext` are flushed.

### 6.4 Sizing / DPI / lifecycle

- Size is observed with a `ResizeObserver` on the `<canvas>` (`SizeWatcher`, §9.4); DPR with a
  ~1 second poll (`DpiWatcher`, §9.5). Both report to the component per §5.1.
- Disposal deinitialises the module, unsubscribes the watchers and frees the pinned buffer /
  GL objects.

## 7. Bridged strategy (Server, Hybrid, static SSR)

Under a bridged host, SkiaSharp draws on the .NET side and the frame is transferred to the
browser through `IJSRuntime`. The **same** C# code serves Server and Hybrid; only the transport
differs (SignalR for Server, in-process for the Hybrid WebView). The browser side uses a
host-agnostic module (`SKHtmlCanvasBridge`, §9.3) that has no dependency on emscripten and MUST
NOT be the WebAssembly module of §6.

### 7.1 Initialisation and metrics

On first interactive render the component imports `SKHtmlCanvasBridge` and calls its
`initialize(canvas, dotNetRef, isGL)`, where `isGL` is `false` for `SKCanvasView` and `true`
for `SKGLView`. The module then, and whenever they change, reports the element's CSS size and
DPR back to .NET via `dotNetRef.invokeMethodAsync('OnMetricsChanged', width, height, dpr)`. A
metrics report updates the component's size/DPR (§5) and calls `Invalidate()`.

A component MUST be ready to receive the first metrics callback before its initialisation call
returns (the first report is synchronous inside `initialize`); it MUST NOT drop that first
render.

### 7.2 Frame production

For each frame (once size and DPR are positive):

1. Compute the device backing-store info per §5.2 with `SKColorType.Rgba8888` and
   `SKAlphaType.Premul`, into a reused pinned buffer.
2. Create an `SKSurface` over the buffer; apply `IgnorePixelScaling` scaling per §5.3.
3. Invoke `OnPaintSurface` (`SKCanvasView` → `SKPaintSurfaceEventArgs`; `SKGLView` → §7.6).
4. Produce the transfer payload for the resolved format (§7.4).
5. **Suppress** the frame if its bytes are byte-identical to the previously transferred frame.
6. Otherwise transfer it via the module's `present(canvas, bytes, deviceW, deviceH, format,
   isGL)`.

### 7.3 Present backends (by view type)

The browser `<canvas>` MUST keep the context type matching the view:

- `SKCanvasView` presents through a **2D** context: `putImageData` for raw pixels, or
  `createImageBitmap(blob)` + `drawImage` for encoded frames.
- `SKGLView` presents through a **WebGL** context: the frame is uploaded as a texture and drawn
  with a full-screen quad (from a raw buffer via `texImage2D`, or from a decoded `ImageBitmap`).
  The presenter flips the texture vertically so a top-left-origin raster frame displays
  upright.

The module sets the `<canvas>` backing-store size to the frame's device size before presenting.

### 7.4 Transfer formats

`SKBlazorTransferFormat` selects how a frame becomes bytes:

| Format | Bytes | Browser present | Notes |
|--------|-------|-----------------|-------|
| `Png` | `SKImage.Encode(Png)` | decode → draw/texture | lossless, keeps alpha, larger |
| `Jpeg` | `SKImage.Encode(Jpeg, quality)` | decode → draw/texture | small; no alpha; quality clamped to 0–100 |
| `Put` | raw RGBA (unpremultiplied) | `putImageData` / `texImage2D` | no encode/decode; largest; bytes are already RGBA and directly usable |

The effective format MUST be resolved with this precedence:

1. the per-control `TransferFormat` parameter, else
2. the global `SKBlazorOptions.TransferFormat`, else
3. a host default: `Put` for Hybrid, `Jpeg` otherwise.

The effective JPEG quality is the per-control `Quality`, else `SKBlazorOptions.Quality`
(default `85`).

The transfer format is independent of the host, so any host may be configured to use any
format (this makes every transfer path exercisable from a single host in tests).

### 7.5 Render loop, invalidation and backpressure

The loop semantics match §8. Additionally, a bridged component MUST apply backpressure: it MUST
NOT begin producing the next frame until the previous `present` transfer has completed, and it
MUST always draw the latest state rather than queueing frames. Concurrent `Invalidate()` calls
while a frame is in flight coalesce into a single subsequent frame. As a consequence, a slow
transport self-limits the effective frame rate without unbounded memory growth. There are no
framework-imposed frame-rate caps.

The loop MUST yield to the host dispatcher on every iteration so the host remains responsive and
the loop stays cancellable — including when a frame is suppressed (§7.2 step 5) and therefore
performs no transfer that would otherwise provide a yield point. Without this a continuous render
loop over a momentarily static scene would spin synchronously and starve the dispatcher,
preventing input, metrics and even disposal from being processed.

### 7.6 `SKGLView` under a bridged host

Server-side drawing is CPU raster. `SKGLView` still keeps a WebGL-backed `<canvas>` (§7.3) and
its frame is presented via the WebGL texture path. `OnPaintSurface` receives an
`SKPaintGLSurfaceEventArgs` whose surface is the raster surface and whose `GRBackendRenderTarget`
is a lightweight placeholder sized to the frame; applications draw through `Surface.Canvas` as
usual. Real GPU drawing on the .NET side (a headless `GRContext`) is a permitted future
extension that would feed the same present path unchanged.

### 7.7 Lifecycle

Disposal MUST stop the render loop, call the module's `deinit(canvas)` (detaching observers and
releasing the presenter's GL state), dispose the JavaScript module reference and the
`DotNetObjectReference`, and free the render buffer. Disposal MUST tolerate an already-closed
transport.

## 8. Render loop and invalidation (all hosts)

- `Invalidate()` requests one frame; if a size/DPR is not yet known it is a no-op until one is
  reported.
- `EnableRenderLoop = true` produces frames continuously; `EnableRenderLoop = false` produces
  frames only in response to `Invalidate()` (including the implicit `Invalidate()` from size,
  DPR, `EnableRenderLoop` and `IgnorePixelScaling` changes).
- The application owns the frame rate. To run slower, an application disables the render loop
  and drives frames with `Invalidate()`.
- The Direct strategy is paced by `requestAnimationFrame`; the Bridged strategy is paced by
  transfer completion (§7.5). Both honour the same `EnableRenderLoop`/`Invalidate()` contract.

## 9. JavaScript modules

All modules are shipped as static web assets under
`_content/SkiaSharp.Views.Blazor/` and are ES modules.

### 9.1 Module responsibilities and import rules

| Module | Responsibility | Imported by |
|--------|----------------|-------------|
| `SKCanvasPresenter` | Pure-browser paint primitives (2D + WebGL). No emscripten, no .NET callbacks, no loop. | The Direct and Bridged modules (internally). |
| `SKHtmlCanvas` | WebAssembly Direct path: emscripten heap/GL access, the `requestAnimationFrame` loop and the .NET draw callback. | Direct strategy only. |
| `SKHtmlCanvasBridge` | Host-agnostic bridged present: `initialize`/`present`/`deinit`, metrics reporting. No emscripten. | Bridged strategy only. |
| `SizeWatcher` | `ResizeObserver` reporting CSS size. | Direct strategy. |
| `DpiWatcher` | ~1 s DPR poll. | Direct strategy. |

A bridged host MUST NOT import `SKHtmlCanvas`: it references emscripten globals (`Module`,
`GL`) that exist only in a WebAssembly runtime, not in a Server client browser or a Hybrid
WebView. The Bridged module MUST perform its painting through `SKCanvasPresenter` so the
pixel-to-canvas logic is defined in one place; the Direct module SHOULD do the same, and today
still contains equivalent paint primitives inline (folding it onto `SKCanvasPresenter` is an
internal cleanup with no behavioural effect).

### 9.2 `SKHtmlCanvas` (Direct)

Provides `initGL`/`initRaster`, the `requestAnimationFrame` loop, `putImageData` (from the
emscripten heap) and emscripten WebGL context creation, as described in §6.

### 9.3 `SKHtmlCanvasBridge` (Bridged)

Provides `initialize(canvas, dotNetRef, isGL)` (attach `ResizeObserver` + DPR poll, report
metrics), `present(canvas, bytes, width, height, format, isGL)` (`format` is `"png"`, `"jpeg"`
or `"put"`) and `deinit(canvas)`, delegating painting to `SKCanvasPresenter`.

### 9.4 `SizeWatcher`

Observes an element with a `ResizeObserver` and reports `clientWidth`/`clientHeight`.

### 9.5 `DpiWatcher`

A shared instance that samples `window.devicePixelRatio` on a ~1 second interval and reports
changes; returns the current DPR immediately on subscribe.

## 10. Interactive Auto

A component MUST behave correctly across the Auto transition. Blazor renders the component with
Interactive Server on the first visit and with Interactive WebAssembly on later visits (after
the WebAssembly bundle is cached); it never changes the runtime of a component instance already
on the page. Because a single component adapts by host (§4) — Bridged on the Server leg, Direct
on the WebAssembly leg — no consumer action is required. The paint surface is stateless (the
application redraws in `OnPaintSurface`), so no state need be persisted across the transition.

## 11. Static SSR

A component prerendered under static SSR is not interactive: `OnAfterRender`/JavaScript interop
do not run, so no frames are presented and the `<canvas>` remains in its initial state until the
component becomes interactive (for example under Interactive Server or Interactive Auto), at
which point the Bridged strategy begins presenting. Emitting a prerendered poster frame is a
permitted future enhancement and is not required by this specification.

## 12. Framework support

- The Direct strategy (WebAssembly) is supported on all target frameworks the package targets
  (currently `net6.0`, `net9.0`, `net10.0`).
- The Bridged strategy (Server, Hybrid, static SSR) and the `RendererInfo`-based host detection
  require **.NET 9.0 or later**. On earlier target frameworks the components are
  WebAssembly-only and use the Direct strategy exclusively; the bridged-only parameters
  (`TransferFormat`, `Quality`) have no effect there.

## 13. Packaging and native assets

- The WebAssembly native asset (`SkiaSharp.NativeAssets.WebAssembly`) and the emscripten
  link-flag workaround are relevant only to WebAssembly builds and MUST NOT affect
  Server/Hybrid consumers, which obtain the platform native `libSkiaSharp` from their own
  `SkiaSharp` reference.
- All view code lives in the single `SkiaSharp.Views.Blazor` package so that one shared
  component assembly satisfies both the server and client projects of an Interactive Auto app.

## 14. Conformance map (spec → code)

The following verifies that the current implementation satisfies this specification. Paths are
under `source/SkiaSharp.Views/SkiaSharp.Views.Blazor/`.

| Spec | Implementation | Status |
|------|----------------|--------|
| §3.1 element + attribute splat | `SKCanvasView.razor`, `SKGLView.razor` | ✅ |
| §3.2 parameters | `SKCanvasView.razor.cs`, `SKGLView.razor.cs` | ✅ |
| §3.3 enum/options/DI | `SKBlazorTransferFormat.cs`, `SKBlazorOptions.cs`, `SKBlazorServiceCollectionExtensions.cs` | ✅ |
| §4 host detection + fallback | `Internal/SKBlazorHost.Resolve`, both views' `OnAfterRenderAsync` (`RendererInfo.Name`, `OperatingSystem.IsBrowser()`) | ✅ |
| §5.2 backing-store = cssSize×dpr | `CreateSize` (views) and `SKBlazorBridgedRenderer.EnsureBuffer` | ✅ |
| §5.3 `IgnorePixelScaling` | `OnRenderFrame` (views) and `RenderAndPresentAsync` | ✅ |
| §5.4 `Dpi` | `Dpi` property (returns bridged renderer DPR when bridged) | ✅ |
| §6.1 RAF loop | `Internal/SKHtmlCanvasInterop`, `wwwroot/SKHtmlCanvas.ts` | ✅ |
| §6.2 raster direct | `SKCanvasView.OnRenderFrame` (`PlatformColorType`/`Opaque`, `PutImageData`) | ✅ |
| §6.3 GL direct | `SKGLView.OnRenderFrame` (`GRContext`, `Rgba8888`, `BottomLeft`, 256 MB, WebGL2→1) | ✅ |
| §6.4 size/DPI watchers | `Internal/SizeWatcherInterop`, `Internal/DpiWatcherInterop`, `wwwroot/SizeWatcher.ts`, `wwwroot/DpiWatcher.ts` | ✅ |
| §7.1 init + metrics + no dropped first frame | `SKBlazorBridgedRenderer.InitializeAsync`/`OnMetricsChanged` (`initialized` set before JS `initialize`) | ✅ |
| §7.2 frame production (RGBA/Premul, reused buffer) | `SKBlazorBridgedRenderer.RenderAndPresentAsync`/`EnsureBuffer` | ✅ |
| §7.3 present backends by view type | `wwwroot/SKCanvasPresenter.ts` (2D + WebGL), `wwwroot/SKHtmlCanvasBridge.ts` | ✅ |
| §7.4 formats + resolution precedence + quality | `Internal/SKBlazorFrameProducer`, `Internal/SKBlazorHost.ResolveTransferFormat` | ✅ |
| §7.5 backpressure + identical-frame suppression | `SKBlazorBridgedRenderer.RenderLoopAsync`/`RenderAndPresentAsync` | ✅ |
| §7.6 `SKGLView` bridged placeholder GL target | `SKGLView.PaintBridgedFrame` | ✅ |
| §7.7 disposal | `SKBlazorBridgedRenderer.DisposeAsync`, both views' `Dispose` | ✅ |
| §8 loop/invalidation contract | `Invalidate` (views) + `SKBlazorBridgedRenderer` | ✅ |
| §9.1 module import rules (bridge never imports `SKHtmlCanvas`) | `Internal/SKHtmlCanvasBridgeInterop`, `Internal/SKHtmlCanvasInterop` | ✅ |
| §10 Auto (single adaptive component) | strategy chosen per render in `OnAfterRenderAsync` | ✅ |
| §12 net9+ gating of bridged features | `#if NET9_0_OR_GREATER` in both views | ✅ |
| §11 static-SSR poster frame | not implemented (blank until interactive) | ⛔ future (permitted by spec) |
| §13 packaging non-leakage | verified at project-reference level (Server sample); NuGet-pack verification | ⚠️ pending |
| §9.1 Direct paint folded onto `SKCanvasPresenter` | `SKHtmlCanvas.ts` still paints inline | ⚠️ internal cleanup (no behavioural effect) |

Automated verification: `tests/SkiaSharp.Tests.Blazor` covers §7.4 (frame producer), §4/§7.4
(host + format resolution) and §3.3 (DI). The Blazor Server sample
(`samples/Basic/BlazorServer`) exercises §4/§5/§7/§8 end to end.
