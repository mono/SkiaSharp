// JS library merged into the consumer's final emcc link (via --js-library)
// when <SkiaSharpEnableWebGpu>True</SkiaSharpEnableWebGpu> is set on a
// .NET WASM project that references SkiaSharp.NativeAssets.WebAssembly.
//
// Why: Blazor (Microsoft.NET.Sdk.BlazorWebAssembly) — and starting in
// .NET 10 also Uno's Microsoft.NET.Sdk.WebAssembly path — encapsulates the
// Emscripten Module inside an IIFE and replaces the standard lifecycle
// hooks (preRun/onRuntimeInitialized), making `Module.WebGPU` unreachable
// from external JS. The library file ships INSIDE dotnet.native.js after
// linking, so its code has direct access to the live $WebGPU shim and to
// the JsValStore manager objects — exactly what Skia's Graphite-Dawn
// shim needs to register a caller's GPUDevice/Queue/Texture by numeric
// handle.
//
// `__postset` injects the bridge installer at module load time. By the
// time a consumer JS function calls `globalThis.skiaSharpWebGpu.*`, the
// $WebGPU.initManagers() preRun has already populated `mgr*` tables.

mergeInto(LibraryManager.library, {
    $SkiaSharpWebGpuBridge__deps: ['$WebGPU'],
    $SkiaSharpWebGpuBridge__postset: 'SkiaSharpWebGpuBridge();',
    $SkiaSharpWebGpuBridge: function () {
        var canvasContexts = {};

        globalThis.skiaSharpWebGpu = {
            // Acquire adapter + device, configure the named canvas's
            // GPUCanvasContext, register the device/queue/instance/initial
            // texture in WebGPU's manager tables, return all four numeric
            // handles as a plain object.
            //
            // Returns null on any failure (no navigator.gpu, no adapter, no
            // canvas, configure threw). Caller should fall back to
            // SkiaSharp's WebGL path.
            initAsync: async function (canvasId) {
                try {
                    if (!navigator.gpu) { console.error('[skiaSharpWebGpu] navigator.gpu unavailable'); return null; }
                    var adapter = await navigator.gpu.requestAdapter({
                        powerPreference: 'low-power',
                    });
                    if (!adapter) { console.error('[skiaSharpWebGpu] navigator.gpu.requestAdapter returned null'); return null; }
                    var device = await adapter.requestDevice();
                    if (!device) { console.error('[skiaSharpWebGpu] adapter.requestDevice returned null'); return null; }

                    var canvas = document.getElementById(canvasId);
                    if (!canvas) { console.error('[skiaSharpWebGpu] no canvas with id ' + canvasId); return null; }
                    var ctx = canvas.getContext('webgpu');
                    if (!ctx) { console.error('[skiaSharpWebGpu] canvas.getContext(webgpu) returned null'); return null; }

                    var format = navigator.gpu.getPreferredCanvasFormat();
                    ctx.configure({
                        device: device,
                        format: format,
                        usage: 0x10 | 0x04 | 0x01,  // RENDER_ATTACHMENT | TEXTURE_BINDING | COPY_SRC
                        alphaMode: 'premultiplied',
                    });
                    canvasContexts[canvasId] = ctx;

                    var queueId    = WebGPU.mgrQueue.create(device.queue);
                    var deviceId   = WebGPU.mgrDevice.create(device, { queueId: queueId });
                    // emscripten 3.1.56's WebGPU shim has no mgrInstance —
                    // wgpuCreateInstance returns a hard-coded value. Newer
                    // versions (4.0.7+) added a real instance table. Skia's
                    // DawnBackendContext just stores fInstance opaquely, so
                    // any non-null number works; we use the wgpuCreateInstance
                    // export if available, else 1 as a sentinel.
                    var instanceId = (typeof _wgpuCreateInstance === 'function')
                        ? _wgpuCreateInstance(0)
                        : (WebGPU.mgrInstance ? WebGPU.mgrInstance.create({}) : 1);

                    // Pre-acquire the first frame's texture so the caller
                    // can render immediately.
                    var tex = ctx.getCurrentTexture();
                    var textureId = WebGPU.mgrTexture.create(tex);

                    return {
                        instanceId: instanceId,
                        deviceId: deviceId,
                        queueId: queueId,
                        textureId: textureId,
                        format: format,
                    };
                } catch (e) {
                    console.error('[skiaSharpWebGpu] initAsync failed:', e);
                    return null;
                }
            },

            // Per-frame: release the previous texture (Skia already
            // dropped its retained ref by the time the next frame is
            // requested), grab the canvas context's current swap-chain
            // texture, register it with WebGPU.mgrTexture, return the new
            // numeric handle.
            acquireFrameTexture: function (canvasId, previousTextureId) {
                if (previousTextureId) {
                    try { WebGPU.mgrTexture.release(previousTextureId); } catch (_) {}
                }
                var ctx = canvasContexts[canvasId];
                if (!ctx) return 0;
                var tex = ctx.getCurrentTexture();
                return WebGPU.mgrTexture.create(tex);
            },

            releaseTexture: function (textureId) {
                if (!textureId) return;
                try { WebGPU.mgrTexture.release(textureId); } catch (_) {}
            },

            // Resize: WebGPU canvases honor canvas.width/height
            // automatically on the next getCurrentTexture call — no
            // re-configure needed.
            setSize: function (canvasId, width, height) {
                var ctx = canvasContexts[canvasId];
                if (!ctx) return;
                ctx.canvas.width = width;
                ctx.canvas.height = height;
            },
        };
    },

    // A dummy exported function so the library is actually linked (some
    // emcc paths drop libraries with no referenced exports). Calling it
    // is a no-op; its only purpose is to anchor SkiaSharpWebGpuBridge.
    sk_wasm_wgpu_bridge_anchor__deps: ['$SkiaSharpWebGpuBridge'],
    sk_wasm_wgpu_bridge_anchor__sig: 'v',
    sk_wasm_wgpu_bridge_anchor: function () {},
});
