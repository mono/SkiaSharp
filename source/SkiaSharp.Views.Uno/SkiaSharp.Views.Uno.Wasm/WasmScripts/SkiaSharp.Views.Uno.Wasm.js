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

                requestAnimationFrame() {
                    if (this.canvas) {
                        var scale = window.devicePixelRatio || 1;
                        this.canvas.width = this.canvas.clientWidth * scale;
                        this.canvas.height = this.canvas.clientHeight * scale;
                    }

                    window.requestAnimationFrame(() => {
                        Uno.Foundation.Interop.ManagedObject.dispatch(this.managedHandle, 'RenderFrame', null);
                    });
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
