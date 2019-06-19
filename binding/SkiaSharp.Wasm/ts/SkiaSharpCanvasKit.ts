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

            SurfaceManager._readInternal = (<any>Module).mono_bind_static_method("[SkiaSharp] SkiaSharp.ManagedStreamHelper:ReadInternal");
            SurfaceManager._peekInternal = (<any>Module).mono_bind_static_method("[SkiaSharp] SkiaSharp.ManagedStreamHelper:PeekInternal");
            SurfaceManager._isAtEndInternal = (<any>Module).mono_bind_static_method("[SkiaSharp] SkiaSharp.ManagedStreamHelper:IsAtEndInternal");
            SurfaceManager._hasPositionInternal = (<any>Module).mono_bind_static_method("[SkiaSharp] SkiaSharp.ManagedStreamHelper:HasPositionInternal");
            SurfaceManager._hasLengthInternal = (<any>Module).mono_bind_static_method("[SkiaSharp] SkiaSharp.ManagedStreamHelper:HasLengthInternal");
            SurfaceManager._rewindInternal = (<any>Module).mono_bind_static_method("[SkiaSharp] SkiaSharp.ManagedStreamHelper:RewindInternal");
            SurfaceManager._getPositionInternal = (<any>Module).mono_bind_static_method("[SkiaSharp] SkiaSharp.ManagedStreamHelper:GetPositionInternal");
            SurfaceManager._seekInternal = (<any>Module).mono_bind_static_method("[SkiaSharp] SkiaSharp.ManagedStreamHelper:SeekInternal");
            SurfaceManager._moveInternal = (<any>Module).mono_bind_static_method("[SkiaSharp] SkiaSharp.ManagedStreamHelper:MoveInternal");
            SurfaceManager._getLengthInternal = (<any>Module).mono_bind_static_method("[SkiaSharp] SkiaSharp.ManagedStreamHelper:GetLengthInternal");
            SurfaceManager._createNewInternal = (<any>Module).mono_bind_static_method("[SkiaSharp] SkiaSharp.ManagedStreamHelper:CreateNewInternal");
            SurfaceManager._destroyInternal = (<any>Module).mono_bind_static_method("[SkiaSharp] SkiaSharp.ManagedStreamHelper:DestroyInternal");

            var fRead = (<any>Module).addFunction((managedStreamPtr: number, buffer: number, size: number) => SurfaceManager._readInternal(managedStreamPtr, buffer, size), 'iiii');
            var fPeek = (<any>Module).addFunction((managedStreamPtr: number, buffer: number, size: number) => SurfaceManager._peekInternal(managedStreamPtr, buffer, size), 'iiii');
            var fIsAtEnd = (<any>Module).addFunction((managedStreamPtr: number) => SurfaceManager._isAtEndInternal(managedStreamPtr), 'ii');
            var fHasPosition = (<any>Module).addFunction((managedStreamPtr: number) => SurfaceManager._hasPositionInternal(managedStreamPtr), 'ii');
            var fHasLength = (<any>Module).addFunction((managedStreamPtr: number) => SurfaceManager._hasLengthInternal(managedStreamPtr), 'ii');
            var fRewind = (<any>Module).addFunction((managedStreamPtr: number) => SurfaceManager._rewindInternal(managedStreamPtr), 'ii');
            var fGetPosition = (<any>Module).addFunction((managedStreamPtr: number) => SurfaceManager._getPositionInternal(managedStreamPtr), 'ii');
            var fSeek = (<any>Module).addFunction((managedStreamPtr: number, position: any) => SurfaceManager._seekInternal(managedStreamPtr, position), 'ii');
            var fMove = (<any>Module).addFunction((managedStreamPtr: number, offset: any) => SurfaceManager._moveInternal(managedStreamPtr, offset), 'ii');
            var fGetLength = (<any>Module).addFunction((managedStreamPtr: number) => SurfaceManager._getLengthInternal(managedStreamPtr), 'ii');
            var fCreateNew = (<any>Module).addFunction((managedStreamPtr: number) => SurfaceManager._createNewInternal(managedStreamPtr), 'ii');
            var fDestroy = (<any>Module).addFunction((managedStreamPtr: number) => SurfaceManager._destroyInternal(managedStreamPtr), 'vi');

            (<any>Module)._sk_managedstream_set_delegates(
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

        static invalidateCanvas(pData: number, canvasId: string, width: number, height: number) {
            var c = document.getElementById(canvasId) as HTMLCanvasElement;
            c.width = width;
            c.height = height;

            var ctx = c.getContext("2d");
            var buffer = new Uint8ClampedArray(Module.HEAPU8.buffer, pData, width * height * 4);
            var imageData = new ImageData(buffer, width, height);
            ctx.putImageData(imageData, 0, 0);

            return true;
        }
    }
}