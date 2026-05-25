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
              → emcc final link with -sUSE_WEBGPU=1
              → browser navigator.gpu → host GPU
```

`SkiaSharp.NativeAssets.WebAssembly`'s targets file unconditionally adds
`-s USE_WEBGPU=1` to the consumer's final emcc link. That's the entire
build-side footprint — the WebGPU orchestration that calls into Skia is
plain C# using `[JSImport]` over Emscripten's `$WebGPU` manager tables;
the bindings reach those tables by reading `Module` off
`JSHost.DotnetInstance` at startup and publishing it to globalThis.

WebGPU plumbing is always linked in — consumers that don't use Graphite/Dawn still pay the binary-size cost but the WebGL-Ganesh path keeps working unchanged.

---

## Consumer integration

Consumers orchestrate the WebGPU bring-up entirely from C# via
`[JSImport]`. The pattern: read `Module` off `JSHost.DotnetInstance`
(set by the .NET runtime — see "How C# reaches Emscripten's WebGPU
tables" below), publish it to globalThis, then call
`Module.WebGPU.mgrDevice.create(...)` etc. via normal dotted-path
`[JSImport]`s. A working reference implementation lives in
`tests/Hosts/RenderHost.Wasm/WasmRenderers.cs` (the `SKWebGpu` nested
type) and covers offscreen device acquisition, GPUTexture allocation,
and async readback.

Production renderers typically capability-detect (WebGPU → WebGL → Software).
The WebGPU symbols are always linked; failure to initialize is the caller's signal to fall back.

---

## How C# reaches Emscripten's WebGPU tables

The .NET 10 WASM SDK emits `dotnet.native.js` with Emscripten's
`-sMODULARIZE`, wrapping the Emscripten runtime in an IIFE. `Module`
(and its `Module.WebGPU.mgr*` handle tables) lives inside that closure
and is not on globalThis. Earlier versions of this binding shipped a
`--js-library` file that ran inside the IIFE and published `Module` to
globalThis at module init.

In .NET 8+ that file is no longer necessary: `JSHost.DotnetInstance`
returns a `JSObject` reference to the runtime from inside C#, with
`Module` reachable as a property:

```csharp
var module = JSHost.DotnetInstance.GetPropertyAsJSObject("Module");
JSHost.GlobalThis.SetProperty("skiaSharpModule", module);
// Now `globalThis.skiaSharpModule.WebGPU.mgrDevice.create(...)` is
// callable via [JSImport].
```

For method calls on `JSObject` instances (`adapter.requestDevice()`,
`device.createBuffer(...)`, `buf.mapAsync(...)`) — which `[JSImport]`
can't bind directly because it expects top-level dotted-path functions —
the bindings install a small set of one-line trampoline closures on
`globalThis.skiaSharpWebGpu` via a single `[JSImport("globalThis.eval")]`
call at static-ctor time.

---

## Non-yielding mode

On WebAssembly (browser) targets `SKGraphiteDawnBackendContext` auto-detects non-yielding mode and passes it through to the shim — Skia is told to never block on async WebGPU operations. Required because we don't build with `-sASYNCIFY` (significant binary size cost). Not user-settable.

**Trade-offs:**
- `SKGraphiteSubmitInfo.Sync = true` on Submit is rejected with `InvalidOperationException`.
- Synchronous pixel readback isn't available — use the async readback helper instead.
- Calling `SKGraphiteContext.Dispose()` while GPU work is in flight triggers
  `fatal error: When ContextOptions::fNeverYieldToWebGPU is specified all GPU
  work must be finished before destroying Context.` at `QueueManager.cpp:45`.
  `FreeGpuResources()` + `CheckAsyncWorkCompletion()` do NOT drain pending work
  in this mode. The recommended pattern is to keep the Context alive for the
  process lifetime (store it as a field that's never disposed). At process
  exit / page unload the warning fires once and is harmless.

If you need synchronous behavior, rebuild with `-sASYNCIFY`.

---

## Image provider

Pass an `SKGraphiteImageCache` (or any callback + cleanup pair) to `CreateRecorder`. Without one, every `DrawImage` of a raster `SkImage` logs:

```
[skia] [graphite] Couldn't convert SkImage to a Graphite-backed representation
[skia] [graphite] Key context creation failed in Device::drawGeometry, draw dropped!
```

and the draw is silently lost.

`SKGraphiteImageCache` uploads each unique source `SkImage` to a Graphite-backed texture exactly once and LRU-caches the result keyed on `(SkImage.UniqueId, mipmapped)` (cap: 256 entries). The cache's lifetime is its own — wire `cache.Dispose` as the recorder's `findOrCreateDispose` so cached images are released before the recorder is destroyed.

---

## Build internals

If you're maintaining the WASM build (rather than just consuming it), see also:
- [building.md](building.md) for the cross-platform native build flow.
- `native/wasm/build.cake` — invoke with `dotnet cake --target=externals-wasm --emscriptenVersion=3.1.56 --emscriptenFeatures=_wasmeh,st`. Repeat with `_wasmeh,st,simd` to also produce the SIMD-enabled flavor. `SkiaSharp.NativeAssets.WebAssembly.targets` resolves `3.1.56/$(_SkiaSharpNativeBinaryType)/*.a` at consumer build time.
- `is_canvaskit=true` is hard-wired in the WASM GN args. It switches Skia's `:graphite` and `:dawn` source sets to `<webgpu/webgpu_cpp.h>` (Emscripten's bundled headers, resolved by `-sUSE_WEBGPU=1` at the consumer's final link) rather than the native-Dawn-generated `webgpu_cpp.h` that doesn't exist on Emscripten. It also tells `externals/skia/third_party/dawn/BUILD.gn` to skip the native Dawn CMake build entirely.
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

A magenta-clear smoke under `tests/Hosts/RenderHost.Wasm/` proves the full
.NET WASM → SkiaSharp → libSkiaSharp.a → Skia Graphite → Dawn → Emscripten WebGPU →
Chromium SwiftShader Vulkan chain.

---

## Troubleshooting

| Symptom | Likely cause |
|---|---|
| `wasm-ld: undefined symbol: skgpu::graphite::ContextFactory::MakeDawn` | The cake archive-merge glob didn't pick up Skia's internal static libs. Check that `is_canvaskit` is true AND the glob in `native/wasm/build.cake` covers both `*.a.wasm` and `*.a`. |
| `wasm-ld: undefined symbol: wgpuCreateInstance` etc. | `-sUSE_WEBGPU=1` not in the final emcc link. The NativeAssets.WebAssembly targets file adds it unconditionally — check it's actually being imported (PackageReference vs ProjectReference). |
| `globalThis.skiaSharpModule` is undefined at runtime | The static ctor of `SKWebGpu` didn't run, or `JSHost.DotnetInstance.Module` was unavailable. Confirm you're on .NET 8 or newer; on older runtimes the property doesn't exist. |
| `System.DllNotFoundException: libSkiaSharp` / `libHarfBuzzSharp` at runtime | The `NativeFileReference` glob didn't expand. On Linux, MSBuild does NOT normalize backslash separators — the targets file must use `/`. |
| `Couldn't convert SkImage to a Graphite-backed representation` | No image-upload callback passed to `CreateRecorder`. Wire an `SKGraphiteImageCache`. |
| `canvas.getContext('webgpu') returned null` | The canvas already has a different context type (likely `webgl2` grabbed by an earlier renderer or by Emscripten's GL auto-init). The WebGPU init must run BEFORE any other code touches the canvas. |
| `When ContextOptions::fNeverYieldToWebGPU is specified all GPU work must be finished before destroying Context` | You're disposing `SKGraphiteContext` while non-yielding mode is on. Keep the Context alive for the process lifetime, or build with `-sASYNCIFY`. |

---

## See also

- [graphite.md](graphite.md) — the main Graphite developer guide (Vulkan / Metal / Dawn-native).
- [graphite-headless.md](graphite-headless.md) — headless Lavapipe testing on Linux for the non-WASM backends.
- [building.md](building.md) — cross-platform native build flow.
