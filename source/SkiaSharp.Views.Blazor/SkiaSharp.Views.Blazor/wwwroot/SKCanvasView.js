
export class SKCanvasView {
    static invalidateCanvas(htmlCanvas, pData, width, height) {
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

        SKCanvasView.resizeCanvas(htmlCanvas, width, height);

        var buffer = new Uint8ClampedArray(Module.HEAPU8.buffer, pData, width * height * 4);
        var imageData = new ImageData(buffer, width, height);
        ctx.putImageData(imageData, 0, 0);

        return true;
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
}
