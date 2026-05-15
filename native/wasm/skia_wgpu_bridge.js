// JS library merged into the consumer's final emcc link (via --js-library)
// for any .NET WASM project that references SkiaSharp.NativeAssets.WebAssembly.
//
// Why: starting in .NET 10, the WASM SDKs wrap dotnet.native.js in an IIFE
// and replace the standard Emscripten lifecycle hooks (preRun /
// onRuntimeInitialized), making `Module.WebGPU` unreachable from external
// JS. This file ships INSIDE dotnet.native.js after linking, so its code
// has direct access to the live $WebGPU shim and to the JsValStore
// manager objects — exactly what Skia's Graphite-Dawn shim needs to
// register a caller's GPUDevice/Queue/Texture by numeric handle.
//
// `__postset` injects the bridge installer at module load time. By the
// time a consumer JS function calls `globalThis.skiaSharpWebGpu.*`, the
// $WebGPU.initManagers() preRun has already populated `mgr*` tables.

mergeInto(LibraryManager.library, {
    $SkiaSharpWebGpuBridge__deps: ['$WebGPU'],
    $SkiaSharpWebGpuBridge__postset: 'SkiaSharpWebGpuBridge();',
    $SkiaSharpWebGpuBridge: function () {
        var canvasContexts = {};
        var _offscreenDeviceId = 0;  // set by initOffscreenAsync; used by readTextureRgbaAsync

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

            // ------------------------------------------------------------
            // Offscreen (canvas-less) helpers used by visual-test render
            // hosts. With Skia's non-yielding mode + no ASYNCIFY, calling
            // SKGraphiteContext.ReadPixels from C# deadlocks: the readback
            // path internally awaits a GPUBuffer.mapAsync promise, but the
            // JS event loop can't tick while C# is in a sync P/Invoke. We
            // sidestep by giving callers an explicit "render into MY
            // texture, then read it back via JS" path:
            //
            //   1. C# calls createOffscreenTexture(w, h) -> textureId.
            //   2. C# wraps that handle as a Graphite BackendTexture, draws,
            //      and Submit()s. Submit is fire-and-forget on Dawn.
            //   3. C# calls readTextureRgbaAsync(textureId, w, h) - an
            //      async JS function that does copyTextureToBuffer + a
            //      buffer.mapAsync. The await returns control to JS, the
            //      event loop ticks, mapAsync resolves, bytes come back.
            //   4. C# calls releaseOffscreenTexture(textureId).
            //
            // No Skia readback ever runs from a sync C# stack, so the
            // non-yielding deadlock is avoided.

            createOffscreenTexture: function (width, height) {
                try {
                    var device = WebGPU.mgrDevice.get(_offscreenDeviceId);
                    if (!device) {
                        console.error('[skiaSharpWebGpu] createOffscreenTexture: device not initialized');
                        return 0;
                    }
                    // RENDER_ATTACHMENT for Skia draw target, COPY_SRC for
                    // our readback copy.
                    var tex = device.createTexture({
                        size: { width: width, height: height, depthOrArrayLayers: 1 },
                        format: 'rgba8unorm',
                        usage: 0x10 | 0x01, // RENDER_ATTACHMENT | COPY_SRC
                    });
                    return WebGPU.mgrTexture.create(tex);
                } catch (e) {
                    console.error('[skiaSharpWebGpu] createOffscreenTexture failed:', e);
                    return 0;
                }
            },

            readTextureRgbaAsync: async function (textureId, width, height) {
                try {
                    var device = WebGPU.mgrDevice.get(_offscreenDeviceId);
                    var tex = WebGPU.mgrTexture.get(textureId);
                    if (!device || !tex) return null;

                    // bytesPerRow must be a multiple of 256 per WebGPU spec.
                    var bytesPerRow = Math.ceil((width * 4) / 256) * 256;
                    var bufSize = bytesPerRow * height;
                    var buf = device.createBuffer({
                        size: bufSize,
                        usage: 0x09, // COPY_DST | MAP_READ
                    });
                    var enc = device.createCommandEncoder();
                    enc.copyTextureToBuffer(
                        { texture: tex },
                        { buffer: buf, bytesPerRow: bytesPerRow, rowsPerImage: height },
                        { width: width, height: height, depthOrArrayLayers: 1 });
                    device.queue.submit([enc.finish()]);

                    await buf.mapAsync(0x01); // GPUMapMode.READ

                    var mapped = new Uint8Array(buf.getMappedRange());
                    // Repack from padded rows to width*4 bytes per row.
                    var packed = new Uint8Array(width * height * 4);
                    var widthBytes = width * 4;
                    for (var r = 0; r < height; r++)
                        packed.set(mapped.subarray(r * bytesPerRow, r * bytesPerRow + widthBytes), r * widthBytes);
                    buf.unmap();
                    buf.destroy();

                    // Base64 — JSExport's byte[] marshalling is sluggish for
                    // medium buffers, and base64 over the wire is what our
                    // sister renderers already use.
                    var s = '';
                    var CHUNK = 0x8000;
                    for (var i = 0; i < packed.length; i += CHUNK)
                        s += String.fromCharCode.apply(null, packed.subarray(i, i + CHUNK));
                    return btoa(s);
                } catch (e) {
                    console.error('[skiaSharpWebGpu] readTextureRgbaAsync failed:', e);
                    return null;
                }
            },

            releaseOffscreenTexture: function (textureId) {
                if (!textureId) return;
                try { WebGPU.mgrTexture.release(textureId); } catch (_) {}
            },

            // Canvas-less init: acquire a WebGPU device + queue + instance
            // without ever touching DOM. Used by offscreen renderers (tests
            // that read pixels back from a Graphite-allocated GPUTexture
            // rather than presenting to a canvas swap-chain). Returns the
            // numeric handles only — no textureId, no format, no canvas
            // bound. Caller is responsible for creating its own GPUTexture
            // (or letting Skia allocate one internally via SKSurface.Create
            // (recorder, info)).
            initOffscreenAsync: async function () {
                try {
                    if (!navigator.gpu) { console.error('[skiaSharpWebGpu] navigator.gpu unavailable'); return null; }
                    var adapter = await navigator.gpu.requestAdapter({ powerPreference: 'low-power' });
                    if (!adapter) { console.error('[skiaSharpWebGpu] navigator.gpu.requestAdapter returned null'); return null; }
                    var device = await adapter.requestDevice();
                    if (!device) { console.error('[skiaSharpWebGpu] adapter.requestDevice returned null'); return null; }

                    var queueId    = WebGPU.mgrQueue.create(device.queue);
                    var deviceId   = WebGPU.mgrDevice.create(device, { queueId: queueId });
                    var instanceId = (typeof _wgpuCreateInstance === 'function')
                        ? _wgpuCreateInstance(0)
                        : (WebGPU.mgrInstance ? WebGPU.mgrInstance.create({}) : 1);
                    _offscreenDeviceId = deviceId;
                    return { instanceId: instanceId, deviceId: deviceId, queueId: queueId };
                } catch (e) {
                    console.error('[skiaSharpWebGpu] initOffscreenAsync failed:', e);
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
