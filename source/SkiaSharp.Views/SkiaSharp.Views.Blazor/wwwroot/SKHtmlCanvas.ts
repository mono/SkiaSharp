// aliases for emscripten
declare let GL: any;
declare let GLctx: WebGLRenderingContext;
declare let Module: EmscriptenModule;

// container for gl info
type SKGLViewInfo = {
	context: WebGLRenderingContext | WebGL2RenderingContext | undefined;
	fboId: number;
	stencil: number;
	sample: number;
	depth: number;
}

// alias for a potential skia html canvas
type SKHtmlCanvasElement = {
	SKHtmlCanvas: SKHtmlCanvas
} & HTMLCanvasElement

type SKHtmlCanvasCallback = DotNet.DotNetObjectReference | (() => void);

export class SKHtmlCanvas {
	static elements: Map<string, HTMLCanvasElement>;

	htmlCanvas: HTMLCanvasElement;
	glInfo: SKGLViewInfo;
	renderFrameCallback: SKHtmlCanvasCallback;
	renderLoopEnabled: boolean = false;
	renderLoopRequest: number = 0;

	public static initGL(element: HTMLCanvasElement, elementId: string, callback: SKHtmlCanvasCallback): SKGLViewInfo {
		var view = SKHtmlCanvas.init(true, element, elementId, callback);
		if (!view || !view.glInfo)
			return null;

		return view.glInfo;
	}

	public static initRaster(element: HTMLCanvasElement, elementId: string, callback: SKHtmlCanvasCallback): boolean {
		var view = SKHtmlCanvas.init(false, element, elementId, callback);
		if (!view)
			return false;

		return true;
	}

	static init(useGL: boolean, element: HTMLCanvasElement, elementId: string, callback: SKHtmlCanvasCallback): SKHtmlCanvas {
		element = element || document.querySelector('[' + elementId + ']');
		var htmlCanvas = element as SKHtmlCanvasElement;
		if (!htmlCanvas) {
			return null;
		}

		if (!SKHtmlCanvas.elements)
			SKHtmlCanvas.elements = new Map<string, HTMLCanvasElement>();
		SKHtmlCanvas.elements.set(elementId, element);

		const view = new SKHtmlCanvas(useGL, element, callback);

		htmlCanvas.SKHtmlCanvas = view;

		return view;
	}

	public static deinit(elementId: string) {
		if (!elementId)
			return;

		const element = SKHtmlCanvas.elements.get(elementId);
		const removed = SKHtmlCanvas.elements.delete(elementId);

		const htmlCanvas = element as SKHtmlCanvasElement;
		if (!htmlCanvas || !htmlCanvas.SKHtmlCanvas)
			return;

		htmlCanvas.SKHtmlCanvas.deinit();
		htmlCanvas.SKHtmlCanvas = undefined;
	}

	public static requestAnimationFrame(elementId: string, renderLoop?: boolean, width?: number, height?: number) {
		const htmlCanvas = SKHtmlCanvas.elements.get(elementId) as SKHtmlCanvasElement;
		if (!htmlCanvas || !htmlCanvas.SKHtmlCanvas)
			return;

		htmlCanvas.SKHtmlCanvas.requestAnimationFrame(renderLoop, width, height);
	}

	public static setEnableRenderLoop(elementId: string, enable: boolean) {
		const htmlCanvas = SKHtmlCanvas.elements.get(elementId) as SKHtmlCanvasElement;
		if (!htmlCanvas || !htmlCanvas.SKHtmlCanvas)
			return;

		htmlCanvas.SKHtmlCanvas.setEnableRenderLoop(enable);
	}

	public static putImageData(elementId: string, pData: number, width: number, height: number) {
		const htmlCanvas = SKHtmlCanvas.elements.get(elementId) as SKHtmlCanvasElement;
		if (!htmlCanvas || !htmlCanvas.SKHtmlCanvas)
			return;

		htmlCanvas.SKHtmlCanvas.putImageData(pData, width, height);
	}

	public constructor(useGL: boolean, element: HTMLCanvasElement, callback: SKHtmlCanvasCallback) {
		this.htmlCanvas = element;
		this.renderFrameCallback = callback;

		if (useGL) {
			const ctx = SKHtmlCanvas.createWebGLContext(this.htmlCanvas);
			if (!ctx) {
				console.error(`Failed to create WebGL context: err ${ctx}`);
				return null;
			}

			// make current
			const GL = SKHtmlCanvas.getGL();
			GL.makeContextCurrent(ctx);

			// read values
			const GLctx = SKHtmlCanvas.getGLctx();
			const fbo = GLctx.getParameter(GLctx.FRAMEBUFFER_BINDING);
			this.glInfo = {
				context: ctx,
				fboId: fbo ? fbo.id : 0,
				stencil: GLctx.getParameter(GLctx.STENCIL_BITS),
				sample: 0, // TODO: GLctx.getParameter(GLctx.SAMPLES)
				depth: GLctx.getParameter(GLctx.DEPTH_BITS),
			};
		}
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
			if (this.glInfo) {
				// make current
				const GL = SKHtmlCanvas.getGL();
				GL.makeContextCurrent(this.glInfo.context);
			}

			if (typeof this.renderFrameCallback === 'function') {
				this.renderFrameCallback();
			} else {
				this.renderFrameCallback.invokeMethod('Invoke');
			}
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
			//console.info(`Enabling render loop with callback ${this.renderFrameCallback._id}...`);
			this.requestAnimationFrame();
		} else if (this.renderLoopRequest !== 0) {
			window.cancelAnimationFrame(this.renderLoopRequest);
			this.renderLoopRequest = 0;
		}
	}

	public putImageData(pData: number, width: number, height: number): boolean {
		if (this.glInfo || !pData || width <= 0 || width <= 0)
			return false;

		var ctx = this.htmlCanvas.getContext('2d');
		if (!ctx) {
			console.error(`Failed to obtain 2D canvas context.`);
			return false;
		}

		// make sure the canvas is scaled correctly for the drawing
		this.htmlCanvas.width = width;
		this.htmlCanvas.height = height;

		// set the canvas to be the bytes
		const Module = SKHtmlCanvas.getModule();
		var buffer = new Uint8ClampedArray(Module.HEAPU8.buffer, pData, width * height * 4);
		var imageData = new ImageData(buffer, width, height);
		ctx.putImageData(imageData, 0, 0);

		return true;
	}

	static createWebGLContext(htmlCanvas: HTMLCanvasElement): WebGLRenderingContext | WebGL2RenderingContext {
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

		const GL = SKHtmlCanvas.getGL();
		let ctx: WebGLRenderingContext = GL.createContext(htmlCanvas, contextAttributes);
		if (!ctx && contextAttributes.majorVersion > 1) {
			console.warn('Falling back to WebGL 1.0');
			contextAttributes.majorVersion = 1;
			contextAttributes.minorVersion = 0;
			ctx = GL.createContext(htmlCanvas, contextAttributes);
		}

		return ctx;
	}

	static getGL(): any {
		return (globalThis as any).SkiaSharpGL || (Module as any).GL || GL;
	}

	static getModule(): EmscriptenModule {
		return (globalThis as any).SkiaSharpModule || (Module as any);
	}

	static getGLctx(): WebGLRenderingContext {
		const GL = SKHtmlCanvas.getGL();
		return GL.currentContext && GL.currentContext.GLctx || GLctx;
	}
}
