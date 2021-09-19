
export class SKGLView {
    static init(htmlCanvas, callback) {
        if (!htmlCanvas) {
            console.error(`No canvas element was provided.`);
            return null;
        }

        var ctx = SKGLView.createWebGLContext(htmlCanvas);
        if (!ctx || ctx < 0) {
            console.error(`Failed to create WebGL context: err ${ctx}`);
            return null;
        }

        // make current
        GL.makeContextCurrent(ctx);

        // read values
        var fbo = GLctx.getParameter(GLctx.FRAMEBUFFER_BINDING);
        var info = {
            context: ctx,
            fboId: fbo ? fbo.id : 0,
            stencil: GLctx.getParameter(GLctx.STENCIL_BITS),
            sample: 0, // TODO: GLctx.getParameter(GLctx.SAMPLES)
            depth: GLctx.getParameter(GLctx.DEPTH_BITS),
        };

        htmlCanvas.SKGLView = {
            info: info,
            renderLoopEnabled: false,
            renderLoopRequest: 0,
            renderFrameCallback: callback
        };

        return info;
    }

    static requestAnimationFrame(htmlCanvas, renderLoop, width, height) {
        if (!htmlCanvas)
            return;

        // optionally update the render loop
        if (renderLoop !== undefined && htmlCanvas.SKGLView.renderLoopEnabled !== renderLoop)
            SKGLView.setEnableRenderLoop(htmlCanvas, renderLoop);

        // make sure the canvas is scaled correctly for the drawing
        SKGLView.resizeCanvas(htmlCanvas, width, height);

        // skip because we have a render loop
        if (htmlCanvas.SKGLView.renderLoopRequest !== 0)
            return;

        // add the draw to the next frame
        htmlCanvas.SKGLView.renderLoopRequest = window.requestAnimationFrame(() => {
            htmlCanvas.SKGLView.renderFrameCallback
                .invokeMethodAsync('Invoke')
                .then(function () {
                    htmlCanvas.SKGLView.renderLoopRequest = 0;

                    // we may want to draw the next frame
                    if (htmlCanvas.SKGLView.renderLoopEnabled)
                        SKGLView.requestAnimationFrame(htmlCanvas);
                });
        });
    }

    static setEnableRenderLoop(htmlCanvas, enable) {
        if (!htmlCanvas)
            return;

        htmlCanvas.SKGLView.renderLoopEnabled = enable;

        // either start the new frame or cancel the existing one
        if (enable) {
            SKGLView.requestAnimationFrame(htmlCanvas);
        } else if (htmlCanvas.SKGLView.renderLoopRequest !== 0) {
            window.cancelAnimationFrame(htmlCanvas.SKGLView.renderLoopRequest);
            htmlCanvas.SKGLView.renderLoopRequest = 0;
        }
    }

    static resizeCanvas(htmlCanvas, width, height) {
        if (!htmlCanvas)
            return;

        const newWidth = (width / window.devicePixelRatio) + "px";
        const newHeight = (height / window.devicePixelRatio) + 'px';

        if (htmlCanvas.style.width != newWidth)
            htmlCanvas.style.width = newWidth;
        if (htmlCanvas.style.height != newHeight)
            htmlCanvas.style.height = newHeight;
    }

    static createWebGLContext(htmlCanvas) {
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

        var ctx = GL.createContext(htmlCanvas, contextAttributes);
        if (!ctx && contextAttributes.majorVersion > 1) {
            console.warn('Falling back to WebGL 1.0');
            contextAttributes.majorVersion = 1;
            contextAttributes.minorVersion = 0;
            ctx = GL.createContext(htmlCanvas, contextAttributes);
        }

        return ctx;
    }
}
