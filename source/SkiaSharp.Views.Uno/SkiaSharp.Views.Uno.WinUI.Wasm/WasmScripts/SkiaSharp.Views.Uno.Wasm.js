var SkiaSharp;
(function (SkiaSharp) {
    var Views;
    (function (Views) {
        var Windows;
        (function (Windows) {

            class SKXamlCanvas {
                static buffers = [];

                static invalidateCanvas(pData, canvasId, width, height) {
                    var htmlCanvas = document.getElementById(canvasId);
                    htmlCanvas.width = width;
                    htmlCanvas.height = height;

                    var ctx = htmlCanvas.getContext('2d');
                    if (!ctx)
                        return false;

                    var byteLength = width * height * 4;

                    if (isSecureContext) {
                        // In a secure context (e.g. with threading enabled), creating a view
                        // from Module.HEAPU8.buffer is not supported, so we're making an
                        // explicit copy of the wasm memory.
                        var buffer = SKXamlCanvas.buffers[canvasId];

                        if (!buffer || buffer.length != byteLength) {
                            SKXamlCanvas.buffers[canvasId] = buffer = new Uint8ClampedArray(new ArrayBuffer(byteLength));
                        }

                        var slice = Module.HEAPU8.buffer.slice(pData, pData + byteLength);
                        buffer.set(new Uint8ClampedArray(slice), 0);
                        var imageData = new ImageData(buffer, width, height);
                        ctx.putImageData(imageData, 0, 0);
                    }
                    else {
                        var buffer = new Uint8ClampedArray(Module.HEAPU8.buffer, byteLength);
                        var imageData = new ImageData(buffer, width, height);
                        ctx.putImageData(imageData, 0, 0);
                    }

                    return true;
                }

                static clearCanvas(canvasId) {
                    if (isSecureContext) {
                        delete SKXamlCanvas.buffers[canvasId];
                    }
                }
            }

            class SKSwapChainPanel {
                static activeInstances = {};

                constructor(managedHandle) {
                    this.managedHandle = managedHandle;
                    this.canvas = undefined;
                    this.renderLoop = false;
                    this.currentRequest = 0;
                    this.requestRender = undefined;

                    this.buildImports();
                }

                async buildImports() {
                    if (Module.getAssemblyExports !== undefined) {
                        const skiaSharpExports = await Module.getAssemblyExports("SkiaSharp.Views.Windows");

                        this.requestRender = () => skiaSharpExports.SkiaSharp.Views.Windows.SKSwapChainPanel.RenderFrame(this.managedHandle);
                    }
                    else {
                        this.requestRender =
                            () => Uno.Foundation.Interop.ManagedObject.dispatch(this.managedHandle, 'RenderFrame', null);
                    }
                }

                // JSObject
                static createInstanceLegacy(managedHandle, jsHandle) {
                    SKSwapChainPanel.activeInstances[jsHandle] = new SKSwapChainPanel(managedHandle);
                }
                static getInstanceLegacy(jsHandle) {
                    return SKSwapChainPanel.activeInstances[jsHandle];
                }
                static destroyInstanceLegacy(jsHandle) {
                    delete SKSwapChainPanel.activeInstances[jsHandle];
                }

                static createInstance(managedHandle) {
                    return new SKSwapChainPanel(managedHandle);
                }

                requestAnimationFrame(renderLoop) {
                    // optionally update the render loop
                    if (renderLoop !== undefined && this.renderLoop !== renderLoop)
                        this.setEnableRenderLoop(renderLoop);

                    // skip because we have a render loop
                    if (this.currentRequest !== 0)
                        return;

                    // make sure the canvas is scaled correctly for the drawing
                    this.resizeCanvas();

                    // add the draw to the next frame
                    this.currentRequest = window.requestAnimationFrame(() => {

                        if (this.requestRender) {
                            // make current for this canvas instance
                            GL.makeContextCurrent(this.glCtx);

                            this.requestRender();
                        }

                        this.currentRequest = 0;

                        // we may want to draw the next frame
                        if (this.renderLoop)
                            this.requestAnimationFrame();
                    });
                }

                resizeCanvas() {
                    if (!this.canvas)
                        return;

                    var scale = window.devicePixelRatio || 1;
                    var w = this.canvas.clientWidth * scale
                    var h = this.canvas.clientHeight * scale;

                    if (this.canvas.width !== w)
                        this.canvas.width = w;
                    if (this.canvas.height !== h)
                        this.canvas.height = h;
                }

                static setEnableRenderLoop(instance, enable) {
                    instance.setEnableRenderLoopInternal(enable);
                }

                setEnableRenderLoopInternal(enable) {
                    this.renderLoop = enable;

                    // either start the new frame or cancel the existing one
                    if (enable) {
                        this.requestAnimationFrame();
                    } else if (this.currentRequest !== 0) {
                        window.cancelAnimationFrame(this.currentRequest);
                        this.currentRequest = 0;
                    }
                }

                createContextLegacy(canvasOrCanvasId) {

                    var jsInfo = this.createContext(canvasOrCanvasId);

                    // format as array for nicer parsing
                    jsInfo = [
                        info.ctx,
                        info.fbo ? info.fbo.id : 0,
                        info.stencil,
                        info.sample,
                        info.depth,
                    ];

                    return jsInfo;
                }

                static createContextStatic(instance, canvasOrCanvasId) {
                    return instance.createContext(canvasOrCanvasId);
                }

                createContext(canvasOrCanvasId) {
                    if (!canvasOrCanvasId)
                        throw 'No <canvas> element or ID was provided';

                    var canvas = canvasOrCanvasId;
                    if (canvas.tagName !== 'CANVAS') {
                        canvas = document.getElementById(canvasOrCanvasId);
                        if (!canvas)
                            throw `No <canvas> with id ${canvasOrCanvasId} was found`;
                    }

                    this.glCtx = SKSwapChainPanel.createWebGLContext(canvas);
                    if (!this.glCtx || this.glCtx < 0)
                        throw `Failed to create WebGL context: err ${ctx}`;

                    // make current
                    GL.makeContextCurrent(this.glCtx);

                    // Starting from .NET 7 the GLctx is defined in an inaccessible scope
                    // when the current GL context changes. We need to pick it up from the
                    // GL.currentContext instead.
                    let currentGLctx = GL.currentContext && GL.currentContext.GLctx;

                    if (!currentGLctx)
                        throw `Failed to get current WebGL context`;

                    // read values
                    this.canvas = canvas;
                    return {
                        ctx: this.glCtx,
                        fbo: currentGLctx.getParameter(currentGLctx.FRAMEBUFFER_BINDING),
                        stencil: currentGLctx.getParameter(currentGLctx.STENCIL_BITS),
                        sample: 0, // TODO: currentGLctx.getParameter(GLctx.SAMPLES)
                        depth: currentGLctx.getParameter(currentGLctx.DEPTH_BITS),
                    };

                }

                static createWebGLContext(canvas) {
                    var contextAttributes = {
                        alpha: 1,
                        depth: 1,
                        stencil: 8,
                        antialias: 1,
                        premultipliedAlpha: 1,
                        preserveDrawingBuffer: 0,
                        preferLowPowerToHighPerformance: 0,
                        failIfMajorPerformanceCaveat: 0,
                        majorVersion: 2,
                        minorVersion: 0,
                        enableExtensionsByDefault: 1,
                        explicitSwapControl: 0,
                        renderViaOffscreenBackBuffer: 0,
                    };

                    var ctx = GL.createContext(canvas, contextAttributes);
                    if (!ctx && contextAttributes.majorVersion > 1) {
                        console.warn('Falling back to WebGL 1.0');
                        contextAttributes.majorVersion = 1;
                        contextAttributes.minorVersion = 0;
                        ctx = GL.createContext(canvas, contextAttributes);
                    }

                    return ctx;
                }
            }

            Windows.SKXamlCanvas = SKXamlCanvas;
            Windows.SKSwapChainPanel = SKSwapChainPanel;

        })(Windows = Views.Windows || (Views.Windows = {}));
    })(Views = SkiaSharp.Views || (SkiaSharp.Views = {}));
})(SkiaSharp || (SkiaSharp = {}));
