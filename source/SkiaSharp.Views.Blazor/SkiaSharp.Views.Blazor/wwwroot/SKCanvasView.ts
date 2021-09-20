
declare let Module: EmscriptenModule;

export class SKCanvasView {
	public static invalidate(htmlCanvas: HTMLCanvasElement, pData: number, width: number, height: number) {
		if (!htmlCanvas) {
			console.error(`No canvas element was provided.`);
			return false;
		}

		if (!pData || width <= 0 || width <= 0)
			return false;

		var ctx = htmlCanvas.getContext('2d');
		if (!ctx) {
			console.error(`Failed to obtain 2D canvas context.`);
			return false;
		}

		// make sure the canvas is scaled correctly for the drawing
		htmlCanvas.width = width;
		htmlCanvas.height = height;

		// set the canvas to be the bytes
		var buffer = new Uint8ClampedArray(Module.HEAPU8.buffer, pData, width * height * 4);
		var imageData = new ImageData(buffer, width, height);
		ctx.putImageData(imageData, 0, 0);

		return true;
	}
}
