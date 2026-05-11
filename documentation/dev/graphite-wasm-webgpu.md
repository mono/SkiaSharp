# Graphite on WebAssembly + WebGPU

Companion to [graphite.md](graphite.md). This document covers the WASM-specific path: how SkiaSharp's Graphite backend is brought up over **Emscripten's `-sUSE_WEBGPU=1`** shim (rather than native Dawn) and how consumers turn it on.

For the Vulkan / Metal / Dawn-native paths and the high-level API surface, read [graphite.md](graphite.md) first.

---

## What ships

```
.NET WASM app → SkiaSharp C# → libSkiaSharp.a (built with is_canvaskit=true,
                                              skia_enable_graphite=true,
                                              skia_use_dawn=true,
                                              skia_use_webgpu=true)
              → emcc final link with -sUSE_WEBGPU=1 + skia_wgpu_bridge.js
              → browser navigator.gpu → host GPU
```

A consumer csproj opts in with one property:

```xml
<PropertyGroup>
  <SkiaSharpEnableWebGpu>True</SkiaSharpEnableWebGpu>
</PropertyGroup>
```

`SkiaSharp.NativeAssets.WebAssembly`'s targets file then:
- adds `-s USE_WEBGPU=1` to the final emcc link,
- adds `--js-library skia_wgpu_bridge.js` (a small JS shim that ships in the same nupkg),
- adds `EXPORTED_RUNTIME_METHODS+=WebGPU` (for devtools / diagnostics),
- adds `DEFAULT_LIBRARY_FUNCS_TO_INCLUDE+=$SkiaSharpWebGpuBridge` (force-keeps the bridge symbol so its `__postset` installs `globalThis.skiaSharpWebGpu` at module init).

Default is `False`. Consumers that don't opt in see no change — same WebGL-Ganesh path as before, same binary size.

---

## Consumer integration

### Blazor WASM (Microsoft.NET.Sdk.BlazorWebAssembly) / .NET WASM SDK

```csharp
// In a Razor component or service that runs after the runtime is up.
var bridge = await JS.InvokeAsync<JSWebGpuHandles>(
    "globalThis.skiaSharpWebGpu.initAsync", canvasElementId);

using var bc = new SKGraphiteDawnBackendContext {
    WgpuInstance = (IntPtr)bridge.InstanceId,
    WgpuDevice   = (IntPtr)bridge.DeviceId,
    WgpuQueue    = (IntPtr)bridge.QueueId,
    NonYielding  = true,           // see "non-yielding mode" below
};
using var ctx = SKGraphiteContext.CreateDawn(bc);
using var recorder = ctx.CreateRecorder(
    recorderBudgetBytes: -1,
    imageProvider: SKGraphiteImageProvider.Default);   // see "image provider" below

// per-frame:
var texHandle = await JS.InvokeAsync<int>(
    "globalThis.skiaSharpWebGpu.acquireFrameTexture", canvasElementId, lastTexId);
using var bt = SKGraphiteBackendTexture.CreateDawn((IntPtr)texHandle);
using var surface = SKSurface.Create(recorder, bt, SKColorType.Rgba8888);
surface.Canvas.DrawWhatever(...);
using (var rec = recorder.Snap()) ctx.InsertRecording(rec);
ctx.Submit(new SKGraphiteSubmitInfo { Sync = SKGraphiteSyncToCpu.No });
```

### Uno (Microsoft.NET.Sdk.WebAssembly with Uno.UI.Runtime.Skia.WebAssembly.Browser)

Uno ships a `WebGpuBrowserRenderer` that wraps the above flow. Just opt in at the head:

```xml
<SkiaSharpEnableWebGpu>True</SkiaSharpEnableWebGpu>
```

`Uno.UI.Runtime.Skia.WebAssembly.Browser` selects WebGPU first, then falls back to WebGL, then Software based on what the browser supports.

---

## What the JS bridge does

`skia_wgpu_bridge.js` is merged into the consumer's final emcc link via `--js-library`. Its `__postset` runs at Emscripten module init time and installs `globalThis.skiaSharpWebGpu`, which exposes:

| Function | Purpose |
|---|---|
| `initAsync(canvasId)` | Requests adapter + device, configures `canvas.getContext('webgpu')`, registers the device/queue with the Emscripten `$WebGPU.mgr*` tables, returns `{instanceId, deviceId, queueId, textureId, format}`. |
| `acquireFrameTexture(canvasId, prevTextureId)` | Per-frame hot path: releases the previous handle, grabs `getCurrentTexture()`, registers it, returns the new numeric handle. |
| `releaseTexture(textureId)` | Drops a texture handle. |
| `setSize(canvasId, width, height)` | Updates `canvas.width` / `canvas.height`. The next `getCurrentTexture` honors the new size automatically. |

**Why a JS library instead of `Module.WebGPU.mgrDevice.create(...)` from external JS?** Starting in .NET 10, Blazor and `Microsoft.NET.Sdk.WebAssembly` wrap `dotnet.native.js` in an IIFE and replace the standard Emscripten lifecycle hooks (`preRun` / `onRuntimeInitialized`). The live Emscripten `Module` is unreachable from outside JS. A JS-library file lives *inside* that IIFE so it has direct access to the `$WebGPU` table.

---

## Non-yielding mode

`SKGraphiteDawnBackendContext.NonYielding = true` tells Skia to never block on async WebGPU operations. Required because we don't build with `-sASYNCIFY` (significant binary size cost).

**Trade-offs:**
- `SKGraphiteSyncToCpu.Yes` on Submit is rejected.
- Synchronous pixel readback isn't available — use the async readback helper instead.
- Calling `SKGraphiteContext.Dispose()` while GPU work is in flight triggers
  `fatal error: When ContextOptions::fNeverYieldToWebGPU is specified all GPU
  work must be finished before destroying Context.` at `QueueManager.cpp:45`.
  `FreeGpuResources()` + `CheckAsyncWorkCompletion()` do NOT drain pending work
  in this mode. The recommended pattern is to keep the Context alive for the
  process lifetime (Uno's renderer does this — Context is a field with no
  Dispose). At process exit / page unload the warning fires once and is
  harmless.

If you need synchronous behavior, rebuild with `-sASYNCIFY` and `NonYielding = false`.

---

## Image provider

Pass `SKGraphiteImageProvider.Default` (or a custom subclass) to `CreateRecorder`. Without it, every `DrawImage` of a raster `SkImage` logs:

```
[skia] [graphite] Couldn't convert SkImage to a Graphite-backed representation
[skia] [graphite] Key context creation failed in Device::drawGeometry, draw dropped!
```

and the draw is silently lost.

`SKGraphiteImageProvider.Default` uploads each unique source `SkImage` to a Graphite-backed texture exactly once and LRU-caches the result keyed on `SkImage.UniqueId` (cap: 256 entries). The cache's lifetime is the provider's lifetime — typically the recorder's lifetime.

The `/tmp/sk-wasm-cs-smoke/Pages/Stress.razor` artifact in the validation tree exercises this with 1024 distinct images cycled across 16 frames and verifies that post-eviction re-upload works.

---

## Build internals

If you're maintaining the WASM build (rather than just consuming it), see also:
- [building.md](building.md) for the cross-platform native build flow.
- `native/wasm/build.cake` — invoke with `SUPPORT_GRAPHITE=true dotnet cake --target=externals-wasm --emscriptenVersion=3.1.56 --emscriptenFeatures=_wasmeh,st`. Repeat with `_wasmeh,st,simd` to also produce the SIMD-enabled flavor. `SkiaSharp.NativeAssets.WebAssembly.targets` resolves `3.1.56/$(_SkiaSharpNativeBinaryType)/*.a` at consumer build time.
- `SUPPORT_GRAPHITE=true` flips on `is_canvaskit=true` in GN. This switches Skia's `:graphite` and `:dawn` source sets to `<webgpu/webgpu_cpp.h>` (Emscripten's bundled headers, resolved by `-sUSE_WEBGPU=1` at the consumer's final link) rather than the native-Dawn-generated `webgpu_cpp.h` that doesn't exist on Emscripten. It also tells `externals/skia/third_party/dawn/BUILD.gn` to skip the native Dawn CMake build entirely.
- The cake archive-merge step globs both `*.a` and `*.a.wasm` because `is_canvaskit=true` changes Skia's GN toolchain output extension.
- Emscripten version must match what the .NET WASM SDK ships (currently 3.1.56). Mismatched versions ABI-break.

---

## Headless testing

Install Chromium that supports WebGPU via SwiftShader Vulkan (no GPU required):

```bash
npx playwright install chromium
sudo apt-get install -y libatk1.0-0 libatk-bridge2.0-0 libxcomposite1 libxdamage1 \
                        libasound2t64 libatspi2.0-0 libnss3 libnspr4 libcups2 \
                        libpangocairo-1.0-0 libpango-1.0-0 libgbm1
```

Launch flags:

```js
await chromium.launch({
  headless: true,
  args: [
    '--enable-unsafe-webgpu',
    '--enable-features=Vulkan',
    '--use-vulkan=swiftshader',
    '--use-angle=swiftshader',
    '--disable-vulkan-surface',
    '--no-sandbox',
  ],
});
```

A magenta-clear smoke landed at `/tmp/sk-wasm-cs-smoke/` proves the full Blazor → SkiaSharp → libSkiaSharp.a → Skia Graphite → Dawn → Emscripten WebGPU → Chromium SwiftShader Vulkan chain.

---

## Troubleshooting

| Symptom | Likely cause |
|---|---|
| `wasm-ld: undefined symbol: skgpu::graphite::ContextFactory::MakeDawn` | The cake archive-merge glob didn't pick up Skia's internal static libs. Check that `is_canvaskit` is true AND the glob in `native/wasm/build.cake` covers both `*.a.wasm` and `*.a`. |
| `wasm-ld: undefined symbol: wgpuCreateInstance` etc. | `-sUSE_WEBGPU=1` not in the final emcc link. Check `<SkiaSharpEnableWebGpu>True</SkiaSharpEnableWebGpu>` is set on the WASM head csproj. |
| `globalThis.skiaSharpWebGpu` is undefined at runtime | The JS library didn't get force-included. Check the link rsp has `-s DEFAULT_LIBRARY_FUNCS_TO_INCLUDE=['$SkiaSharpWebGpuBridge']` (MSBuild's `$` escape is `%24`, not `$$`). |
| `System.DllNotFoundException: libSkiaSharp` / `libHarfBuzzSharp` at runtime | The `NativeFileReference` glob didn't expand. On Linux, MSBuild does NOT normalize backslash separators — the targets file must use `/`. |
| `Couldn't convert SkImage to a Graphite-backed representation` | No `SKGraphiteImageProvider` passed to `CreateRecorder`. Pass `SKGraphiteImageProvider.Default`. |
| `canvas.getContext('webgpu') returned null` | The canvas already has a different context type (likely `webgl2` grabbed by Uno's WebGL renderer or by Emscripten's GL auto-init). The WebGPU init must run BEFORE any other code touches the canvas. |
| `When ContextOptions::fNeverYieldToWebGPU is specified all GPU work must be finished before destroying Context` | You're disposing `SKGraphiteContext` while non-yielding mode is on. Keep the Context alive for the process lifetime, or build with `-sASYNCIFY`. |

---

## See also

- [graphite.md](graphite.md) — the main Graphite developer guide (Vulkan / Metal / Dawn-native).
- [graphite-headless.md](graphite-headless.md) — headless Lavapipe testing on Linux for the non-WASM backends.
- [building.md](building.md) — cross-platform native build flow.
