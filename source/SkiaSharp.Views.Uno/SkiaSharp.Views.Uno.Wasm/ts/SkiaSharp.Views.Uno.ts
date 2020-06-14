/// <reference path="../../../../externals/typescript/types/emscripten/index.d.ts" />

namespace SkiaSharp.Views.UWP {
	export class SKXamlCanvas {
		static invalidateCanvas(pData: number, canvasId: string, width: number, height: number) {
			var c = document.getElementById(canvasId) as HTMLCanvasElement;
			c.width = width;
			c.height = height;

			var ctx = c.getContext("2d");
			if (!ctx)
				return false;

			var buffer = new Uint8ClampedArray(Module.HEAPU8.buffer, pData, width * height * 4);
			var imageData = new ImageData(buffer, width, height);
			ctx.putImageData(imageData, 0, 0);

			return true;
		}
	}
}
