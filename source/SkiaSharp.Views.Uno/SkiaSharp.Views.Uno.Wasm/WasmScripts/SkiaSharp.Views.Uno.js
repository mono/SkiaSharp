"use strict";

var SkiaSharp_Views_UWP_SKXamlCanvas = class SKXamlCanvas {
    static invalidateCanvas(pData, canvasId, width, height) {
        var htmlCanvas = document.getElementById(canvasId);
        htmlCanvas.width = width;
        htmlCanvas.height = height;

        var ctx = htmlCanvas.getContext("2d");
        if (!ctx)
            return false;

        var buffer = new Uint8ClampedArray(Module.HEAPU8.buffer, pData, width * height * 4);
        var imageData = new ImageData(buffer, width, height);
        ctx.putImageData(imageData, 0, 0);

        return true;
    }
};
