"use strict";
/// <reference path="../../../../externals/typescript/types/emscripten/index.d.ts" />
var SkiaSharp;
(function (SkiaSharp) {
    var Views;
    (function (Views) {
        var UWP;
        (function (UWP) {
            class SKXamlCanvas {
                static invalidateCanvas(pData, canvasId, width, height) {
                    var c = document.getElementById(canvasId);
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
            UWP.SKXamlCanvas = SKXamlCanvas;
        })(UWP = Views.UWP || (Views.UWP = {}));
    })(Views = SkiaSharp.Views || (SkiaSharp.Views = {}));
})(SkiaSharp || (SkiaSharp = {}));
