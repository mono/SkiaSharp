require(
    ["canvaskit"],
    (ckInit: any) => {
        ckInit().then((i: any) => {
            (<any>window).CanvasKit = i;

            MonoSupport.jsCallDispatcher.registerScope("Skia", SkiaSharp.SkiaApi);
        });
    }
);

namespace SkiaSharp {

    export class SurfaceManager {
        static _readInternal: any;
        static _peekInternal: any;
        static _isAtEndInternal: any;
        static _hasPositionInternal: any;
        static _hasLengthInternal: any;
        static _rewindInternal: any;
        static _getPositionInternal: any;
        static _seekInternal: any;
        static _moveInternal: any;
        static _getLengthInternal: any;
        static _createNewInternal: any;
        static _destroyInternal: any;


        static registerManagedStream() {

            SurfaceManager._readInternal = (<any>Module).mono_bind_static_method("[SkiaSharp.Wasm] SkiaSharp.ManagedStreamHelper:ReadInternal");
            SurfaceManager._peekInternal = (<any>Module).mono_bind_static_method("[SkiaSharp.Wasm] SkiaSharp.ManagedStreamHelper:PeekInternal");
            SurfaceManager._isAtEndInternal = (<any>Module).mono_bind_static_method("[SkiaSharp.Wasm] SkiaSharp.ManagedStreamHelper:IsAtEndInternal");
            SurfaceManager._hasPositionInternal = (<any>Module).mono_bind_static_method("[SkiaSharp.Wasm] SkiaSharp.ManagedStreamHelper:HasPositionInternal");
            SurfaceManager._hasLengthInternal = (<any>Module).mono_bind_static_method("[SkiaSharp.Wasm] SkiaSharp.ManagedStreamHelper:HasLengthInternal");
            SurfaceManager._rewindInternal = (<any>Module).mono_bind_static_method("[SkiaSharp.Wasm] SkiaSharp.ManagedStreamHelper:RewindInternal");
            SurfaceManager._getPositionInternal = (<any>Module).mono_bind_static_method("[SkiaSharp.Wasm] SkiaSharp.ManagedStreamHelper:GetPositionInternal");
            SurfaceManager._seekInternal = (<any>Module).mono_bind_static_method("[SkiaSharp.Wasm] SkiaSharp.ManagedStreamHelper:SeekInternal");
            SurfaceManager._moveInternal = (<any>Module).mono_bind_static_method("[SkiaSharp.Wasm] SkiaSharp.ManagedStreamHelper:MoveInternal");
            SurfaceManager._getLengthInternal = (<any>Module).mono_bind_static_method("[SkiaSharp.Wasm] SkiaSharp.ManagedStreamHelper:GetLengthInternal");
            SurfaceManager._createNewInternal = (<any>Module).mono_bind_static_method("[SkiaSharp.Wasm] SkiaSharp.ManagedStreamHelper:CreateNewInternal");
            SurfaceManager._destroyInternal = (<any>Module).mono_bind_static_method("[SkiaSharp.Wasm] SkiaSharp.ManagedStreamHelper:DestroyInternal");

            var fRead = CanvasKit.addFunction((managedStreamPtr: number, buffer: number, size: number) => SurfaceManager.readInternal(managedStreamPtr, buffer, size))
            var fPeek = CanvasKit.addFunction((managedStreamPtr: number, buffer: number, size: number) => SurfaceManager.peekInternal(managedStreamPtr, buffer, size))
            var fIsAtEnd = CanvasKit.addFunction((managedStreamPtr: number) => SurfaceManager._isAtEndInternal(managedStreamPtr))
            var fHasPosition = CanvasKit.addFunction((managedStreamPtr: number) => SurfaceManager._hasPositionInternal(managedStreamPtr))
            var fHasLength = CanvasKit.addFunction((managedStreamPtr: number) => SurfaceManager._hasLengthInternal(managedStreamPtr))
            var fRewind = CanvasKit.addFunction((managedStreamPtr: number) => SurfaceManager._rewindInternal(managedStreamPtr))
            var fGetPosition = CanvasKit.addFunction((managedStreamPtr: number) => SurfaceManager._getPositionInternal(managedStreamPtr))
            var fSeek = CanvasKit.addFunction((managedStreamPtr: number, position: any) => SurfaceManager._seekInternal(managedStreamPtr, position))
            var fMove = CanvasKit.addFunction((managedStreamPtr: number, offset: any) => SurfaceManager._moveInternal(managedStreamPtr, offset))
            var fGetLength = CanvasKit.addFunction((managedStreamPtr: number) => SurfaceManager._getLengthInternal(managedStreamPtr))
            var fCreateNew = CanvasKit.addFunction((managedStreamPtr: number) => SurfaceManager._createNewInternal(managedStreamPtr))
            var fDestroy = CanvasKit.addFunction((managedStreamPtr: number) => SurfaceManager._destroyInternal(managedStreamPtr))

            CanvasKit._sk_managedstream_set_delegates(
                fRead,
                fPeek,
                fIsAtEnd,
                fHasPosition,
                fHasLength,
                fRewind,
                fGetPosition,
                fSeek,
                fMove,
                fGetLength,
                fCreateNew,
                fDestroy
            );

            return true;
        }

        static readInternal(managedStreamPtr: number, buffer: number, size: number): number {

            var ckBuffer = CanvasKit._malloc(size);
            try {
                var ret = SurfaceManager._readInternal(managedStreamPtr, ckBuffer, size);

                for (var i = 0; i < size; i++) {
                    CanvasKit.HEAPU8[buffer + i] = Module.HEAPU8[ckBuffer + i];
                }

                return ret;
            }
            finally {
                CanvasKit._free(ckBuffer);
            }
        }

        static peekInternal(managedStreamPtr: number, buffer: number, size: number): number {

            var ckBuffer = CanvasKit._malloc(size);
            try {
                var ret = SurfaceManager._peekInternal(managedStreamPtr, ckBuffer, size);

                for (var i = 0; i < size; i++) {
                    CanvasKit.HEAPU8[buffer + i] = Module.HEAPU8[ckBuffer + i];
                }

                return ret;
            }
            finally {
                CanvasKit._free(ckBuffer);
            }
        }

        static invalidateCanvas(pData: number, canvasId: string, width: number, height: number) {
            var c = document.getElementById(canvasId) as HTMLCanvasElement;
            c.width = width;
            c.height = height;

            var ctx = c.getContext("2d");
            var buffer = new Uint8ClampedArray(CanvasKit.HEAPU8.buffer, pData, width * height * 4);
            var imageData = new ImageData(buffer, width, height);
            ctx.putImageData(imageData, 0, 0);

            return true;
        }
    }

    export class ApiOverride {

        static sk2MonoMap: { [id: number]: any } = {};
        static mono2SkMap: { [id: number]: any } = {};
        static bitmapPixels: { [id: number]: any } = {};
        static memoryStreamMap: { [id: number]: any } = {};

        static memcpy(sourceArray: any, pSource: any, destArray: any, pDest: any, length: number) {
            destArray.set(sourceArray.slice(pSource, pSource + length), pDest);
        }

        static memcpy_Mono2Sk(pMono: any, pSk: any, length: number) {
            ApiOverride.memcpy(Module.HEAPU8, pMono, CanvasKit.HEAPU8, pSk, length);
        }

        static memcpy_Sk2Mono(pSk: any, pMono: any, length: number) {
            ApiOverride.memcpy(CanvasKit.HEAPU8, pSk, Module.HEAPU8, pMono, length);
        }

        static sk_bitmap_get_pixels_0_Post(skPixels: any, parms: any, retStruct: any): any {

            var monoPixels = this.sk2MonoMap[skPixels];

            if (!monoPixels) {
                monoPixels = Module._malloc(retStruct.length);

                this.sk2MonoMap[skPixels] = monoPixels;
                this.mono2SkMap[monoPixels] = skPixels;
                this.bitmapPixels[parms.b] = monoPixels;
            }

            // Synchronize the pixels content
            this.memcpy_Sk2Mono(skPixels, monoPixels, retStruct.length);

            return monoPixels;
        }

        static sk_bitmap_destructor_0_Post(ret:any, parms:any): any {
            var monoPixels = this.bitmapPixels[parms.b];

            if (monoPixels) {
                delete this.bitmapPixels[parms.b];

                var skPixels = this.mono2SkMap[monoPixels];
                delete this.mono2SkMap[monoPixels];
                delete this.sk2MonoMap[skPixels];

                Module._free(monoPixels);
            }
        }

        static sk_codec_get_pixels_0_Pre(parms: any) {

            var skPixels = this.mono2SkMap[parms.pixels];

            if (skPixels) {
                parms.pixels = skPixels;
            }
            else {
                throw `Unknown pixels pointer in sk_codec_get_pixels`;
            }

        }

        static sk_memorystream_set_memory_Pre(parms: any) {

            var skStream = this.memoryStreamMap[parms.s];

            if (!skStream) {
                skStream = CanvasKit._malloc(parms.length);
            }

            // Synchronize the pixels content
            this.memcpy_Mono2Sk(parms.data, skStream, parms.length);

            parms.data = skStream;
        }

        static sk_memorystream_destroy_Post(ret: any, parms: any): any {

            var skStream = this.memoryStreamMap[parms.stream];

            if (skStream) {
                delete this.memoryStreamMap[parms.stream];
                CanvasKit._free(skStream);
            }

            return ret;
        }
    }
}