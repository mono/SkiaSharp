export class SKGLView {
    constructor(element, callback) {
        this.renderLoopEnabled = false;
        this.renderLoopRequest = 0;
        this.htmlCanvas = element;
        const ctx = this.createWebGLContext(this.htmlCanvas);
        if (!ctx) {
            console.error(`Failed to create WebGL context: err ${ctx}`);
            return null;
        }
        // make current
        GL.makeContextCurrent(ctx);
        // read values
        const fbo = GLctx.getParameter(GLctx.FRAMEBUFFER_BINDING);
        this.info = {
            context: ctx,
            fboId: fbo ? fbo.id : 0,
            stencil: GLctx.getParameter(GLctx.STENCIL_BITS),
            sample: 0,
            depth: GLctx.getParameter(GLctx.DEPTH_BITS),
        };
        this.renderFrameCallback = callback;
    }
    static init(element, elementId, callback) {
        var htmlCanvas = element;
        if (!htmlCanvas) {
            console.error(`No canvas element was provided.`);
            return null;
        }
        if (!SKGLView.elements)
            SKGLView.elements = new Map();
        SKGLView.elements[elementId] = element;
        const view = new SKGLView(element, callback);
        htmlCanvas.SKGLView = view;
        return view.info;
    }
    static deinit(elementId) {
        if (!elementId)
            return;
        const element = SKGLView.elements[elementId];
        SKGLView.elements.delete(elementId);
        const htmlCanvas = element;
        if (!htmlCanvas || !htmlCanvas.SKGLView)
            return;
        htmlCanvas.SKGLView.deinit();
        htmlCanvas.SKGLView = undefined;
    }
    static requestAnimationFrame(element, renderLoop, width, height) {
        const htmlCanvas = element;
        if (!htmlCanvas || !htmlCanvas.SKGLView)
            return;
        htmlCanvas.SKGLView.requestAnimationFrame(renderLoop, width, height);
    }
    static setEnableRenderLoop(element, enable) {
        const htmlCanvas = element;
        if (!htmlCanvas || !htmlCanvas.SKGLView)
            return;
        htmlCanvas.SKGLView.setEnableRenderLoop(enable);
    }
    deinit() {
        this.setEnableRenderLoop(false);
    }
    requestAnimationFrame(renderLoop, width, height) {
        // optionally update the render loop
        if (renderLoop !== undefined && this.renderLoopEnabled !== renderLoop)
            this.setEnableRenderLoop(renderLoop);
        // make sure the canvas is scaled correctly for the drawing
        if (width && height) {
            this.htmlCanvas.width = width;
            this.htmlCanvas.height = height;
        }
        // skip because we have a render loop
        if (this.renderLoopRequest !== 0)
            return;
        // add the draw to the next frame
        this.renderLoopRequest = window.requestAnimationFrame(() => {
            // make current
            GL.makeContextCurrent(this.info.context);
            this.renderFrameCallback
                .invokeMethodAsync('Invoke')
                .then(() => {
                this.renderLoopRequest = 0;
                // we may want to draw the next frame
                if (this.renderLoopEnabled)
                    this.requestAnimationFrame();
            });
        });
    }
    setEnableRenderLoop(enable) {
        this.renderLoopEnabled = enable;
        // either start the new frame or cancel the existing one
        if (enable) {
            console.info(`Enabling render loop with callback ${this.renderFrameCallback._id}...`);
            this.requestAnimationFrame();
        }
        else if (this.renderLoopRequest !== 0) {
            window.cancelAnimationFrame(this.renderLoopRequest);
            this.renderLoopRequest = 0;
        }
    }
    createWebGLContext(htmlCanvas) {
        const contextAttributes = {
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
        let ctx = GL.createContext(htmlCanvas, contextAttributes);
        if (!ctx && contextAttributes.majorVersion > 1) {
            console.warn('Falling back to WebGL 1.0');
            contextAttributes.majorVersion = 1;
            contextAttributes.minorVersion = 0;
            ctx = GL.createContext(htmlCanvas, contextAttributes);
        }
        return ctx;
    }
}
