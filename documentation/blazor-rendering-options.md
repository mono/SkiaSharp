# SkiaSharp Blazor Rendering: Architecture Options Analysis

> **Status:** Draft for team review  
> **Date:** February 2026  
> **Scope:** Extending SkiaSharp.Views.Blazor beyond WASM to Blazor Server, MAUI Blazor Hybrid, and mixed render modes

---

## Table of Contents

1. [Current State](#1-current-state)
2. [The Problem Space](#2-the-problem-space)
3. [Blazor Hosting Models & What They Mean for Skia](#3-blazor-hosting-models--what-they-mean-for-skia)
4. [Rendering Strategy Options](#4-rendering-strategy-options)
5. [MAUI Blazor Hybrid: The Special Case](#5-maui-blazor-hybrid-the-special-case)
6. [Decision Matrix](#6-decision-matrix)
7. [Recommended Architecture](#7-recommended-architecture)
8. [What's Actionable vs. Speculative](#8-whats-actionable-vs-speculative)
9. [Open Questions](#9-open-questions)

---

## 1. Current State

### What We Ship Today

**Package:** `SkiaSharp.Views.Blazor` — targets `net6.0` through `net9.0`, platform = `browser` only.

| Component | Backend | How It Works |
|-----------|---------|--------------|
| `SKGLView` | WebGL (Ganesh) | Skia renders directly into a WebGL context via Emscripten GL bindings. GPU-accelerated, zero pixel copying. **Preferred.** |
| `SKCanvasView` | CPU rasterizer | Skia renders to a pinned `byte[]`, raw RGBA pixels sent to browser via `putImageData()`. Universal fallback. |

**Architecture:**
```
[.NET WASM] SKGLView/SKCanvasView
      ↓ OnPaintSurface event
[.NET WASM] User draws on SKCanvas/SKSurface
      ↓ GPU flush or putImageData
[Browser]   WebGL framebuffer or Canvas2D
```

**Key implementation details:**
- JS interop via `SKHtmlCanvasInterop` (dual-path: `[JSImport]` on NET7+, `IJSInProcessRuntime` pre-NET7)
- `SizeWatcherInterop` wraps `ResizeObserver` for responsive sizing
- `DpiWatcherInterop` polls `window.devicePixelRatio` for HiDPI support
- Rendering is on-demand via `InvalidateSurface()` → `requestAnimationFrame`
- TypeScript source compiled to JS, shipped in `wwwroot/`

**What works well:** Zero network overhead, full Skia fidelity, GPU acceleration via WebGL, touch/input handling.

**What doesn't exist:** Any support for Blazor Server, Static SSR, MAUI Blazor Hybrid, or Interactive Auto.

### WASM Bundle Size Reality

From empirical measurement (SkiaSharp 3.x, .NET 8):

| Asset | Raw Size | Brotli Compressed |
|-------|----------|-------------------|
| `dotnet.native.wasm` (with SkiaSharp) | ~7.6 MB | ~2.1 MB |
| SkiaSharp delta over baseline | ~4.9 MB | ~1.4 MB |
| Full WASM payload (all assemblies) | ~12-15 MB | ~3-4 MB |

**Note:** `dotnet.native.wasm` is statically linked at build time. Blazor's `LazyAssemblyLoader` only defers managed `.dll` assemblies — the native WASM binary (containing libSkiaSharp) loads at startup. True lazy loading of Skia's native code would require architectural changes to the .NET WASM toolchain.

---

## 2. The Problem Space

Customers are asking for SkiaSharp rendering in scenarios where WASM isn't available or desirable:

| Scenario | Why WASM Doesn't Work |
|----------|----------------------|
| **Blazor Server apps** | No WASM runtime — .NET runs on server, browser only has SignalR connection |
| **MAUI Blazor Hybrid** | .NET runs natively on device — WebView renders HTML but there's no WASM runtime |
| **Interactive Auto** | Starts as Server (no WASM yet), transitions to WASM after download |
| **Static SSR** | No interactivity at all — just HTML response |
| **Corporate lockdown** | Some enterprises block WASM execution via CSP or browser policy |
| **First-paint performance** | 3-4 MB WASM download before any canvas renders |

**The core tension:** `SKGLView` and `SKCanvasView` assume Skia and the browser canvas are **in the same process**. When they're not (Server, Hybrid), we need a bridge.

---

## 3. Blazor Hosting Models & What They Mean for Skia

### 3.1 Interactive WebAssembly ✅ (Existing — Works Today)

```
Browser Process:
  [.NET WASM Runtime] → [SkiaSharp] → [WebGL/Canvas2D]
```

- Skia runs in-browser, renders directly to canvas
- **Our existing `SkiaSharp.Views.Blazor` package handles this**
- Zero changes needed for this path

**Verdict:** Ship as-is. This is the gold standard.

### 3.2 Static SSR (No Interactivity)

```
Server:  [SkiaSharp] → [SKBitmap] → [Encode WebP/PNG]
                                          ↓
Browser: <img src="/api/canvas/123" />
```

- No persistent connection, no SignalR, no interactivity
- SkiaSharp renders on the server, produces static images served via HTTP endpoints
- Standard HTTP caching (ETags, Cache-Control) applies
- Works with **any** browser — just an `<img>` tag

**Verdict:** Already possible today without any library changes. Users just need `SkiaSharp` + native assets on the server and a Minimal API endpoint. We could provide a convenience component or documentation, but no core library work is needed.

**Best for:** Reports, thumbnails, chart snapshots, document previews, email-embedded images.

### 3.3 Interactive Server (SignalR)

```
Server:  [SkiaSharp] → [SKSurface] → [Encode dirty region] → SignalR/WebSocket push
                                                                     ↓
Browser: [JS] → createImageBitmap() → drawImage() on <canvas>
```

- Persistent SignalR connection — server can push frames to browser
- SkiaSharp renders on the server, encodes changed pixels, streams to browser
- Browser input (mouse/touch/keyboard) flows back to server via SignalR
- Round-trip latency affects interactivity (typically 50-200ms)

**This is the "pixel streaming" approach.** It's architecturally similar to RDP/VNC — proven, well-understood, but latency-bound.

**Verdict:** Feasible and valuable. Requires new library code. See [Section 4.1](#41-pixel-streaming-server-renders-browser-displays).

### 3.4 Interactive Auto (Server → WASM Handoff)

```
Phase 1 (Server):  Pixel streaming (same as Interactive Server)
Phase 2 (WASM):    Standard SKGLView/SKCanvasView (same as existing)
Transition:        Server session tears down, WASM takes over
```

- Starts as Interactive Server while WASM bundle downloads
- Transitions to WASM once bundle is ready
- **Requires clean handoff** — server pixel stream stops, client-side Skia activates

**Verdict:** High complexity, questionable value. The handoff is a footgun — state synchronization, visual glitches during transition, and the user experiences two different latency profiles. **Recommend against** unless we have a clear customer demand. If needed, the WASM Island pattern (Section 7) is a better alternative.

### 3.5 MAUI Blazor Hybrid (Native .NET + WebView)

```
Device:  [.NET Native Runtime] → [SkiaSharp Native] → ???
WebView: [HTML/CSS/JS] ← Razor components render here
```

This is the most interesting and nuanced case. See [Section 5](#5-maui-blazor-hybrid-the-special-case) for deep analysis.

---

## 4. Rendering Strategy Options

These are the concrete technical approaches for getting Skia pixels into a browser canvas when Skia isn't running in WASM.

### 4.1 Pixel Streaming (Server Renders → Browser Displays)

**How it works:**
1. SkiaSharp renders to `SKSurface`/`SKBitmap` on the server
2. Diff against previous frame to find dirty regions
3. Encode dirty region as WebP (lossy, quality 80-85)
4. Push encoded bytes over SignalR/WebSocket
5. Browser decodes with `createImageBitmap()` (off main thread) and draws to `<canvas>`

**Optimization layers (progressive — implement as needed):**

| Layer | Optimization | Impact |
|-------|-------------|--------|
| 1 | **Encoding format** — WebP lossy @ 85 instead of PNG | 60-80% size reduction |
| 2 | **Dirty region tracking** — only send changed pixels | 80-95% bandwidth reduction for typical UI |
| 3 | **Tile-based dirty tracking** — 256×256 tile grid | Handles sparse changes efficiently |
| 4 | **Binary transport** — MessagePack or raw WebSocket instead of JSON/base64 | 33% overhead elimination |
| 5 | **Back-pressure / ACK** — client ACKs before server sends next frame | Prevents latency growth |
| 6 | **Adaptive quality** — lower quality during motion, restore when idle | Smooth interaction feel |
| 7 | **Motion compensation** — detect scrolling, send move commands + new pixels | 90%+ savings during scroll |

**Bandwidth estimates (1080p canvas):**

| Scenario | Without Optimization | With Dirty Regions + WebP |
|----------|---------------------|---------------------------|
| Static (idle) | 0 | 0 |
| Single widget update | ~200 KB/frame | ~5-20 KB/frame |
| Full repaint | ~200 KB/frame | ~200 KB/frame |
| Steady 30fps interaction | ~6 MB/s | ~150-600 KB/s |

**Pros:**
- Works in **any** browser (just needs `<canvas>` or even `<img>`)
- Full Skia fidelity — every pixel is Skia-rendered
- Simple mental model: server owns rendering, browser is a dumb display
- No WASM needed on client

**Cons:**
- Latency-bound — round-trip affects feel for drag/hover interactions
- Server CPU cost — encoding per user per frame
- Bandwidth at scale — concurrent users multiply encoding + transfer costs
- Not suitable for 60fps interactive content (fine for 10-30fps UI)

**Implementation effort:** Medium. Need server-side renderer, dirty tracker, encoder, SignalR hub, JS canvas manager. The architecture doc has working pseudocode for all of this.

### 4.2 SKP Command Streaming (Record on Server → Replay on Client)

**How it works:**
1. Server renders into `SKPictureRecorder` instead of a real surface
2. Serialize `SKPicture` via `SKPicture.Serialize()` → `SKData` bytes
3. Send serialized SKP to browser
4. Browser-side thin WASM shim deserializes and replays onto a canvas

**What SkiaSharp already supports:**
- `SKPictureRecorder` — records draw commands into `SKPicture`
- `SKPicture.Serialize()` → produces `SKData` (byte stream)
- `SKPicture.Deserialize(SKData)` → reconstructs picture from bytes
- The underlying Skia format is well-defined (uses zlib compression)

**Pros:**
- Much smaller payloads than pixels (draw commands vs. pixel data)
- Perfect visual fidelity (same Skia renderer on both ends)
- App logic stays on server — client is a dumb replay engine
- Client WASM payload is minimal (~1-2 MB for a stripped Skia player)

**Cons:**
- **Still requires WASM on the client** (a thin Skia player, but WASM nonetheless)
- Need to build and maintain a separate thin WASM Skia player
- SKP format is Skia-internal and version-sensitive — Skia version must match between server and client
- SKPs can reference external resources (images, fonts) that need separate transfer
- No existing infrastructure for this in SkiaSharp — would need significant new development

**Verdict:** Architecturally elegant but **high effort and still requires WASM**. The version-coupling between server Skia and client Skia is a maintenance hazard. **Park this idea** unless there's a specific use case where pixel streaming bandwidth is unacceptable but WASM is available.

### 4.3 Canvas 2D Command Streaming (Map Skia Calls → Browser Canvas API)

**How it works:**
1. Intercept Skia draw calls on the server (custom `SkCanvas` backend or wrapper)
2. Serialize as compact command list: `drawRect(x, y, w, h, paint)`, `drawPath(...)`, etc.
3. Send to browser over SignalR
4. JS dispatcher calls equivalent `CanvasRenderingContext2D` methods

**Pros:**
- Zero WASM on the client — pure JS rendering
- Smallest possible client footprint
- Commands are typically smaller than pixels

**Cons:**
- **Canvas 2D is NOT feature-equivalent to Skia** — advanced features don't map:
  - Complex blend modes, path effects, custom shaders → no Canvas2D equivalent
  - Text rendering differs between platforms (Skia vs browser font engine)
  - Blur filters, color filters → partial mapping at best
- Would require restricting Skia API usage or implementing fallbacks
- Maintaining the mapping layer is a permanent tax
- Performance depends on command volume (complex scenes = many commands)

**Verdict:** **Discard.** The feature gap between Skia and Canvas 2D is too large for a general-purpose solution. This only works if you control the drawing code and can restrict it to Canvas2D-expressible operations — which defeats the purpose of using Skia. Community libraries like `Blazor.Extensions.Canvas` already provide Canvas 2D interop directly; no Skia wrapper needed.

### 4.4 SVG Streaming

**How it works:**
1. Server renders via Skia's SVG backend (`SkSVGCanvas`)
2. Push SVG fragments to browser via Blazor DOM diffing
3. Browser renders SVG natively

**Verdict:** **Discard for our purposes.** Skia's SVG backend is incomplete, SVG doesn't handle raster content well, and complex scenes produce bloated SVG. This is a niche approach for simple vector diagrams, not a general rendering solution. Anyone needing SVG output can already use `SKSvgCanvas` directly.

---

## 5. MAUI Blazor Hybrid: The Special Case

This is the most nuanced scenario and deserves dedicated analysis.

### 5.1 How Blazor Hybrid Works

In MAUI Blazor Hybrid:
- **.NET runs natively** on the device (not in WASM)
- **Razor components** render HTML/CSS into an **embedded WebView**:
  - Windows: WebView2 (Chromium-based)
  - Android: `android.webkit.WebView` (Chromium-based)
  - iOS/Mac Catalyst: `WKWebView` (WebKit/Safari)
- **JS interop** works via `IJSRuntime` (async only — `IJSInProcessRuntime` is NOT available)
- **`RendererInfo.Name`** reports `"WebView"` (not `"Server"` or `"WebAssembly"`)
- Render modes are `null` — everything is interactive by default

### 5.2 Why Our Current Blazor Package Doesn't Work in Hybrid

`SkiaSharp.Views.Blazor` fails in Hybrid for several reasons:

| Issue | Why It Fails |
|-------|-------------|
| **`[JSImport]` / `[JSExport]`** | These are WASM-only APIs — they don't exist in the Hybrid runtime |
| **`IJSInProcessRuntime`** | Hybrid only supports async `IJSRuntime` — synchronous interop unavailable |
| **Emscripten GL bindings** | `SKGLView` uses Emscripten's GL wrapper which doesn't exist outside WASM |
| **`Module.HEAPU8`** | Direct WASM memory access for pixel transfer doesn't apply |
| **Package platform constraint** | The `.csproj` declares `SupportedPlatform = browser` |

**Known community issue:** SkiaSharp issues #1834, #1921 report "currently only works on WebAssembly" errors when attempting to use `SkiaSharp.Views.Blazor` in Hybrid apps.

### 5.3 Options for MAUI Blazor Hybrid

#### Option A: Native SkiaSharp View Alongside WebView (Overlay)

```
┌──────────────────────────────┐
│  MAUI Native Layout          │
│  ┌────────────────────────┐  │
│  │  BlazorWebView         │  │  ← Razor components (HTML/CSS)
│  │  ┌──────────────────┐  │  │
│  │  │  Placeholder div │  │  │  ← "hole" in WebView
│  │  └──────────────────┘  │  │
│  └────────────────────────┘  │
│  ┌────────────────────────┐  │
│  │  SKCanvasView (Native) │  │  ← SkiaSharp renders natively
│  └────────────────────────┘  │
└──────────────────────────────┘
```

- Use MAUI's existing `SkiaSharp.Views.Maui` (`SKCanvasView`/`SKGLView`) for rendering
- Position the native view behind/over the WebView using MAUI layout
- Coordinate between Blazor and native view via a bridge service (DI)
- WebView shows UI chrome; native SkiaSharp view handles the canvas

**Pros:** Full native Skia performance, no pixel copying, uses existing MAUI views.  
**Cons:** Complex layout coordination, platform-specific positioning, can't truly "embed" canvas inside HTML flow. Z-ordering and hit-testing between WebView and native view is tricky.

**Feasibility:** Medium. This is how some apps solve the "native content in WebView" problem, but it's fiddly.

#### Option B: Render to Image, Display in WebView via Data URI / Blob URL

```
[.NET Native] SkiaSharp renders → encode to PNG/WebP → base64 data URI
                                                            ↓
[WebView]     <img src="data:image/webp;base64,..." /> or Blob URL via JS interop
```

- SkiaSharp renders on the native .NET side (no WASM)
- Encode frame as WebP/PNG
- Push to WebView via JS interop: `IJSRuntime.InvokeVoidAsync("updateCanvas", base64Data)`
- WebView displays via `<img>` tag or `drawImage()` on a `<canvas>`

**Pros:** Simple, works everywhere, full Skia fidelity, no WASM needed.  
**Cons:** Pixel encoding + base64 overhead (use Blob URLs to avoid base64 33% bloat), latency of async JS interop, no GPU path.

**This is essentially pixel streaming but local** — the "network" is the in-process JS interop channel instead of SignalR. Latency should be minimal since it's all on-device.

**Feasibility:** High. This is the simplest viable approach and could share infrastructure with the Server pixel streaming path.

#### Option C: WebView Canvas via Async JS Interop

```
[.NET Native] SkiaSharp renders → pixel bytes
                                      ↓ (async JS interop)
[WebView JS]  Receive bytes → putImageData() on <canvas>
```

Similar to Option B but uses raw pixel transfer instead of encoded images. The challenge is that `IJSRuntime` in Hybrid is **async only** — no synchronous `putImageData`. This means:
- Pixels must be marshalled across the interop boundary (byte array → JS)
- Each frame requires an async round-trip
- For large canvases, the byte array transfer may be slow

**Feasibility:** Medium. Performance depends on the WebView's JS interop throughput for large byte arrays. Needs benchmarking.

#### Option D: Custom WebView Protocol Handler

MAUI's `BlazorWebView` supports custom scheme handlers. We could:
- Register a custom URI scheme (e.g., `skia://frame/latest`)
- SkiaSharp renders and stores the latest frame in memory
- WebView's `<img>` tag requests `skia://frame/latest`
- Custom handler serves the frame bytes directly — no base64, no JS interop overhead

**Pros:** Efficient binary transfer, no base64 bloat, leverages WebView's native image loading.  
**Cons:** Requires platform-specific protocol handler registration, HTTP-like semantics (request/response, not push).

**Feasibility:** Medium-High. This avoids the JS interop bottleneck entirely.

### 5.4 Hybrid Recommendation

**Start with Option B** (encode to image, push via JS interop). It's the simplest, it works, and the shared pixel-streaming infrastructure serves both Server and Hybrid scenarios. Optimize with Option D (custom scheme handler) if JS interop throughput is a bottleneck.

**Do NOT try Option A** (native overlay) as a first approach — it's platform-specific, fragile, and doesn't compose well with HTML layout.

---

## 6. Decision Matrix

### By Hosting Model → What Strategy to Use

| Hosting Model | Skia Runs Where | Rendering Strategy | Client Requires | Effort |
|---------------|----------------|-------------------|-----------------|--------|
| **Interactive WASM** | Browser (WASM) | Direct canvas (existing) | WASM + WebGL | ✅ Done |
| **Static SSR** | Server | Image endpoint (`<img>`) | Nothing special | 📄 Docs only |
| **Interactive Server** | Server | Pixel streaming (dirty regions + WebP over SignalR) | `<canvas>` + JS | 🔧 Medium |
| **Interactive Auto** | Server → Browser | Pixel stream → WASM handoff | WASM + `<canvas>` | ⚠️ High (avoid) |
| **MAUI Hybrid** | Device (native) | Local pixel streaming (encode → JS interop → `<canvas>`) | WebView | 🔧 Medium |

### By Use Case → What to Recommend

| Use Case | Best Approach | Why |
|----------|--------------|-----|
| Rich interactive canvas (charts, drawing, maps) | **WASM Island** in SSR page | Best performance, full Skia fidelity, no latency |
| WASM blocked by policy | **Pixel streaming** (Interactive Server) | Only viable option without WASM |
| MAUI app with Blazor UI | **Local pixel streaming** or native overlay | .NET + Skia run natively, push to WebView |
| Static reports / thumbnails | **Server-side image endpoint** | Simplest, cacheable, works everywhere |
| Dashboard with mixed content | **SSR page + WASM island** for canvas | Fast page load, interactive canvas where needed |
| Real-time data visualization | **Pixel streaming** @ 15-30fps | Server has data, no need to ship to client |

---

## 7. Recommended Architecture: The WASM Island Pattern

For most applications, the **WASM Island** pattern is the recommended default. It combines fast page loads with full Skia performance:

```
┌─────────────────────────────────────────────────┐
│  Page Shell (Static SSR) — instant load          │
│                                                  │
│  <NavBar />                    → Static SSR      │
│  <DataPanel />                 → Interactive Svr  │
│                                                  │
│  ┌──────────────────────────────────────────┐    │
│  │  <SkiaCanvasIsland                       │    │
│  │     @rendermode InteractiveWebAssembly>  │    │
│  │                                          │    │
│  │  SKGLView runs here — pure WASM          │    │
│  │  Full Skia, zero latency                 │    │
│  └──────────────────────────────────────────┘    │
│                                                  │
│  <Footer />                    → Static SSR      │
└─────────────────────────────────────────────────┘
```

**How it works:**
1. Page loads via Static SSR — fast, SEO-friendly, no WASM blocking navigation
2. A server-rendered **snapshot image** shows as placeholder while WASM downloads
3. `SKGLView` activates inside the WASM island once ready
4. Image fades out, live canvas fades in (CSS transition)

**State passing:**
- Low-frequency: Blazor component parameters (serialized across WASM boundary)
- High-frequency: JS event bus side-channel (bypasses Blazor's re-render pipeline)

**When WASM is unavailable:** Detect at runtime and fall back to pixel streaming.

```csharp
// Runtime detection
var rendererName = RendererInfo.Name; // "Static", "Server", "WebAssembly", "WebView"
var isInteractive = RendererInfo.IsInteractive;
```

---

## 8. What's Actionable vs. Speculative

### ✅ Actionable — Can Build Now

| Item | Description | Effort |
|------|-------------|--------|
| **Documentation: Static SSR image endpoint** | Guide + sample showing SkiaSharp as server-side image renderer with Minimal API | Small |
| **Documentation: WASM Island pattern** | Guide + sample showing SKGLView as WASM island in SSR page with snapshot placeholder | Small |
| **Pixel Streaming component for Interactive Server** | `SKPixelStreamView` — server-side Skia renderer + dirty region tracker + WebP encoder + SignalR push + JS canvas receiver | Medium-Large |
| **MAUI Hybrid pixel streaming** | Same encoder/renderer as Server, but push via `IJSRuntime` instead of SignalR | Medium (shares infra with Server) |
| **Runtime render mode detection** | Utility to detect hosting model and select appropriate rendering strategy | Small |

### 🔬 Needs Exploration / Prototyping

| Item | Question to Answer | How to Validate |
|------|-------------------|-----------------|
| **JS interop throughput in Hybrid** | How fast can we push encoded frames through `IJSRuntime` in MAUI WebView? | Build minimal prototype, benchmark on each platform |
| **Custom scheme handler for Hybrid** | Can we serve frames via `skia://` protocol more efficiently than JS interop? | Prototype with MAUI's `BlazorWebView` scheme handler |
| **Dirty region tracking accuracy** | Tile-based (256px) vs bounding-box — which is better for typical UI workloads? | Instrument real-world scenes, measure bandwidth |
| **WebP encode performance** | Can we sustain 30fps encoding for 1080p on typical server hardware per-user? | Benchmark SkiaSharp's WebP encoder under load |

### ❌ Discard / Park

| Item | Reason |
|------|--------|
| **Canvas 2D command streaming** | Feature gap too large — Skia and Canvas 2D are not equivalent. Breaks the "full Skia fidelity" promise. |
| **SVG streaming** | Skia's SVG backend is incomplete, bloated output, only works for simple vectors. |
| **SKP command streaming** | Still requires WASM, version-coupling between server and client Skia, high effort for marginal benefit over pixel streaming. Park unless specific bandwidth-constrained use case emerges. |
| **Interactive Auto render mode** | Server→WASM handoff is complex and error-prone. WASM Island is strictly better for the use cases that matter. |

---

## 9. Open Questions

1. **Do we ship pixel streaming as part of `SkiaSharp.Views.Blazor` or as a separate package?**
   - Pro separate: cleaner dependency graph, Server users don't need browser-platform constraint
   - Candidate: `SkiaSharp.Views.Blazor.Server` or `SkiaSharp.Views.Blazor.PixelStream`

2. **Do we ship a MAUI Hybrid package or just document how to wire it up?**
   - If the infrastructure is shared with Server pixel streaming, a package makes sense
   - Candidate: `SkiaSharp.Views.Maui.Blazor`

3. **What's the minimum viable pixel streaming implementation?**
   - Could start with full-frame WebP (no dirty tracking) and iterate
   - Dirty regions are a big win but add complexity

4. **Should the pixel streaming component expose the same `OnPaintSurface` event pattern?**
   - Consistency with existing API would help adoption
   - Server-side rendering may need different lifecycle (no `requestAnimationFrame`)

5. **How do we handle input routing for pixel streaming?**
   - Mouse/touch/keyboard from browser → server via SignalR is natural for Interactive Server
   - Need to define the event model (coordinates, modifiers, touch points)
   - Existing `SKTouchInterop` patterns could be reused

6. **What about concurrent users on Interactive Server?**
   - Each user = separate render + encode pipeline
   - Need to understand server capacity limits
   - Consider: shared rendering for read-only content (everyone sees the same chart)

---

## Appendix A: Comparison of Pixel Streaming vs Direct WASM

| Dimension | Pixel Streaming (Server/Hybrid) | Direct WASM (Existing) |
|-----------|---------------------------------|------------------------|
| **Skia fidelity** | 100% (server renders everything) | 100% |
| **Visual quality** | Near-lossless at WebP 85-92 | Lossless (native canvas) |
| **Input latency** | 50-200ms (network round-trip) | <1ms (in-process) |
| **Bandwidth** | 150KB-6MB/s depending on scene | 0 (local rendering) |
| **Server cost** | CPU per user per frame | 0 (client-side) |
| **Client requirements** | `<canvas>` + JS (any browser) | WASM + WebGL |
| **First paint** | Instant (SSR + first frame push) | 3-5s (WASM download) |
| **Offline capable** | No (needs server) | Yes (after initial load) |
| **Best for** | Locked-down environments, server-has-data, MAUI Hybrid | Rich interactive, low-latency, offline |

## Appendix B: Blazor Render Mode Detection

.NET 8+ provides runtime detection of the hosting model:

```csharp
// In any Razor component:
var name = RendererInfo.Name;
// Returns: "Static", "Server", "WebAssembly", "WebView"

var isInteractive = RendererInfo.IsInteractive;
// true when interactive, false during prerender or SSR

var mode = AssignedRenderMode;
// InteractiveServerRenderMode, InteractiveWebAssemblyRenderMode,
// InteractiveAutoRenderMode, or null (Hybrid/unassigned)
```

This enables a **single component** that adapts its rendering strategy:

```csharp
@if (RendererInfo.Name == "WebAssembly")
{
    <SKGLView OnPaintSurface="OnPaint" />  // Direct WASM rendering
}
else if (RendererInfo.IsInteractive)
{
    <SKPixelStreamView OnPaintSurface="OnPaint" />  // Pixel streaming
}
else
{
    <img src="/api/canvas/snapshot/@Id" />  // Static fallback
}
```

## Appendix C: Package Structure Proposal

```
SkiaSharp.Views.Blazor                    (existing — WASM components)
SkiaSharp.Views.Blazor.Server             (new — pixel streaming for Interactive Server)
SkiaSharp.Views.Maui.Blazor               (new — pixel streaming for MAUI Hybrid WebView)
SkiaSharp.Views.Blazor.Common             (new — shared: dirty tracker, encoder, canvas JS)
```

Or, if we want to keep it simpler:

```
SkiaSharp.Views.Blazor                    (existing — add Server/Hybrid support alongside WASM)
```

The tradeoff: single package is simpler for users but pulls in dependencies they may not need. Separate packages are cleaner but more confusing.

---

*This document consolidates analysis from the Claude architecture chat, the formalized architecture doc, Microsoft Learn documentation on Blazor render modes and Hybrid, and direct inspection of the SkiaSharp.Views.Blazor source code. It's intended as a starting point for team discussion — not a final design.*
