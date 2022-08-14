var SkiaSharp;
(function (SkiaSharp) {
    var Views;
    (function (Views) {
        var UWP;
        (function (UWP) {

            class SKXamlCanvas {
                static invalidateCanvas(pData, canvasId, width, height) {
                    var htmlCanvas = document.getElementById(canvasId);
                    htmlCanvas.width = width;
                    htmlCanvas.height = height;

                    var ctx = htmlCanvas.getContext('2d');
                    if (!ctx)
                        return false;

                    var buffer = new Uint8ClampedArray(Module.HEAPU8.buffer, pData, width * height * 4);
                    var imageData = new ImageData(buffer, width, height);
                    ctx.putImageData(imageData, 0, 0);

                    return true;
                }
            }

            class SKSwapChainPanel {
                static activeInstances = {};

                constructor(managedHandle) {
                    this.managedHandle = managedHandle;
                    this.canvas = undefined;
                    this.jsInfo = undefined;
                    this.renderLoop = false;
                    this.currentRequest = 0;
                }

                // JSObject
                static createInstance(managedHandle, jsHandle) {
                    SKSwapChainPanel.activeInstances[jsHandle] = new SKSwapChainPanel(managedHandle);
                }
                static getInstance(jsHandle) {
                    return SKSwapChainPanel.activeInstances[jsHandle];
                }
                static destroyInstance(jsHandle) {
                    delete SKSwapChainPanel.activeInstances[jsHandle];
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
                        Uno.Foundation.Interop.ManagedObject.dispatch(this.managedHandle, 'RenderFrame', null);

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

                setEnableRenderLoop(enable) {
                    this.renderLoop = enable;

                    // either start the new frame or cancel the existing one
                    if (enable) {
                        this.requestAnimationFrame();
                    } else if (this.currentRequest !== 0) {
                        window.cancelAnimationFrame(this.currentRequest);
                        this.currentRequest = 0;
                    }
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

                    var ctx = SKSwapChainPanel.createWebGLContext(canvas);
                    if (!ctx || ctx < 0)
                        throw `Failed to create WebGL context: err ${ctx}`;

                    // make current
                    GL.makeContextCurrent(ctx);

                    // read values
                    this.canvas = canvas;
                    var info = {
                        ctx: ctx,
                        fbo: GLctx.getParameter(GLctx.FRAMEBUFFER_BINDING),
                        stencil: GLctx.getParameter(GLctx.STENCIL_BITS),
                        sample: 0, // TODO: GLctx.getParameter(GLctx.SAMPLES)
                        depth: GLctx.getParameter(GLctx.DEPTH_BITS),
                    };

                    // format as array for nicer parsing
                    this.jsInfo = [
                        info.ctx,
                        info.fbo ? info.fbo.id : 0,
                        info.stencil,
                        info.sample,
                        info.depth,
                    ];
                    return this.jsInfo;
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

            UWP.SKXamlCanvas = SKXamlCanvas;
            UWP.SKSwapChainPanel = SKSwapChainPanel;

        })(UWP = Views.UWP || (Views.UWP = {}));
    })(Views = SkiaSharp.Views || (SkiaSharp.Views = {}));
})(SkiaSharp || (SkiaSharp = {}));
