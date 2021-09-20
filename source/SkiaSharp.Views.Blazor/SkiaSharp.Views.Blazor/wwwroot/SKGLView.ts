
declare let GL: any;
declare let GLctx: WebGLRenderingContext;

type SKGLViewInfo = {
	context: WebGLRenderingContext | WebGL2RenderingContext | undefined;
	fboId: number;
	stencil: number;
	sample: number;
	depth: number;
}

type SKGLViewCanvasElement = {
	SKGLView: SKGLView
} & HTMLCanvasElement

export class SKGLView {
	static elements: Map<string, HTMLCanvasElement>;

	htmlCanvas: HTMLCanvasElement;
	info: SKGLViewInfo;
	renderFrameCallback: DotNet.DotNetObjectReference;
	renderLoopEnabled: boolean = false;
	renderLoopRequest: number = 0;

	public static init(element: HTMLCanvasElement, elementId: string, callback: DotNet.DotNetObjectReference): SKGLViewInfo {
		var htmlCanvas = element as SKGLViewCanvasElement;
		if (!htmlCanvas) {
			console.error(`No canvas element was provided.`);
			return null;
		}

		if (!SKGLView.elements)
			SKGLView.elements = new Map<string, HTMLCanvasElement>();
		SKGLView.elements[elementId] = element;

		const view = new SKGLView(element, callback);

		htmlCanvas.SKGLView = view;

		return view.info;
	}

	public static deinit(elementId: string) {
		if (!elementId)
			return;

		const element = SKGLView.elements[elementId];
		SKGLView.elements.delete(elementId);

		const htmlCanvas = element as SKGLViewCanvasElement;
		if (!htmlCanvas || !htmlCanvas.SKGLView)
			return;

		htmlCanvas.SKGLView.deinit();
		htmlCanvas.SKGLView = undefined;
	}

	public static requestAnimationFrame(element: HTMLCanvasElement, renderLoop?: boolean, width?: number, height?: number) {
		const htmlCanvas = element as SKGLViewCanvasElement;
		if (!htmlCanvas || !htmlCanvas.SKGLView)
			return;

		htmlCanvas.SKGLView.requestAnimationFrame(renderLoop, width, height);
	}

	public static setEnableRenderLoop(element: HTMLCanvasElement, enable: boolean) {
		const htmlCanvas = element as SKGLViewCanvasElement;
		if (!htmlCanvas || !htmlCanvas.SKGLView)
			return;

		htmlCanvas.SKGLView.setEnableRenderLoop(enable);
	}

	public constructor(element: HTMLCanvasElement, callback: DotNet.DotNetObjectReference) {
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
			sample: 0, // TODO: GLctx.getParameter(GLctx.SAMPLES)
			depth: GLctx.getParameter(GLctx.DEPTH_BITS),
		};

		this.renderFrameCallback = callback;
	}

	public deinit() {
		this.setEnableRenderLoop(false);
	}

	public requestAnimationFrame(renderLoop?: boolean, width?: number, height?: number) {
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

			this.renderFrameCallback.invokeMethod('Invoke');
			this.renderLoopRequest = 0;

			// we may want to draw the next frame
			if (this.renderLoopEnabled)
				this.requestAnimationFrame();
		});
	}

	public setEnableRenderLoop(enable: boolean) {
		this.renderLoopEnabled = enable;

		// either start the new frame or cancel the existing one
		if (enable) {
			console.info(`Enabling render loop with callback ${this.renderFrameCallback._id}...`);
			this.requestAnimationFrame();
		} else if (this.renderLoopRequest !== 0) {
			window.cancelAnimationFrame(this.renderLoopRequest);
			this.renderLoopRequest = 0;
		}
	}

	createWebGLContext(htmlCanvas: HTMLCanvasElement): WebGLRenderingContext | WebGL2RenderingContext {
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

		let ctx: WebGLRenderingContext = GL.createContext(htmlCanvas, contextAttributes);
		if (!ctx && contextAttributes.majorVersion > 1) {
			console.warn('Falling back to WebGL 1.0');
			contextAttributes.majorVersion = 1;
			contextAttributes.minorVersion = 0;
			ctx = GL.createContext(htmlCanvas, contextAttributes);
		}

		return ctx;
	}
}
