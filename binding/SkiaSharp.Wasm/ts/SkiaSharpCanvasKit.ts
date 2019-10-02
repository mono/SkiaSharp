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
        static _duplicateInternal: any;
        static _forkInternal: any;
        static _destroyInternal: any;


        static registerManagedStream() {

            SurfaceManager._readInternal = (<any>Module).mono_bind_static_method("[SkiaSharp] SkiaSharp.SKAbstractManagedStream:ReadInternal");
            SurfaceManager._peekInternal = (<any>Module).mono_bind_static_method("[SkiaSharp] SkiaSharp.SKAbstractManagedStream:PeekInternal");
            SurfaceManager._isAtEndInternal = (<any>Module).mono_bind_static_method("[SkiaSharp] SkiaSharp.SKAbstractManagedStream:IsAtEndInternal");
            SurfaceManager._hasPositionInternal = (<any>Module).mono_bind_static_method("[SkiaSharp] SkiaSharp.SKAbstractManagedStream:HasPositionInternal");
            SurfaceManager._hasLengthInternal = (<any>Module).mono_bind_static_method("[SkiaSharp] SkiaSharp.SKAbstractManagedStream:HasLengthInternal");
            SurfaceManager._rewindInternal = (<any>Module).mono_bind_static_method("[SkiaSharp] SkiaSharp.SKAbstractManagedStream:RewindInternal");
            SurfaceManager._getPositionInternal = (<any>Module).mono_bind_static_method("[SkiaSharp] SkiaSharp.SKAbstractManagedStream:GetPositionInternal");
            SurfaceManager._seekInternal = (<any>Module).mono_bind_static_method("[SkiaSharp] SkiaSharp.SKAbstractManagedStream:SeekInternal");
            SurfaceManager._moveInternal = (<any>Module).mono_bind_static_method("[SkiaSharp] SkiaSharp.SKAbstractManagedStream:MoveInternal");
            SurfaceManager._getLengthInternal = (<any>Module).mono_bind_static_method("[SkiaSharp] SkiaSharp.SKAbstractManagedStream:GetLengthInternal");
            SurfaceManager._duplicateInternal = (<any>Module).mono_bind_static_method("[SkiaSharp] SkiaSharp.SKAbstractManagedStream:DuplicateInternal");
            SurfaceManager._forkInternal = (<any>Module).mono_bind_static_method("[SkiaSharp] SkiaSharp.SKAbstractManagedStream:ForkInternal");
            SurfaceManager._destroyInternal = (<any>Module).mono_bind_static_method("[SkiaSharp] SkiaSharp.SKAbstractManagedStream:DestroyInternal");

            var fRead = (<any>Module).addFunction((managedStreamPtr: number, context: number, buffer: number, size: number) => SurfaceManager._readInternal(managedStreamPtr, context, buffer, size), 'iiiii');
            var fPeek = (<any>Module).addFunction((managedStreamPtr: number, context: number, buffer: number, size: number) => SurfaceManager._peekInternal(managedStreamPtr, context, buffer, size), 'iiiii');
            var fIsAtEnd = (<any>Module).addFunction((managedStreamPtr: number, context: number) => SurfaceManager._isAtEndInternal(managedStreamPtr, context), 'iii');
            var fHasPosition = (<any>Module).addFunction((managedStreamPtr: number, context: number) => SurfaceManager._hasPositionInternal(managedStreamPtr, context), 'iii');
            var fHasLength = (<any>Module).addFunction((managedStreamPtr: number, context: number) => SurfaceManager._hasLengthInternal(managedStreamPtr, context), 'iii');
            var fRewind = (<any>Module).addFunction((managedStreamPtr: number, context: number) => SurfaceManager._rewindInternal(managedStreamPtr, context), 'iii');
            var fGetPosition = (<any>Module).addFunction((managedStreamPtr: number, context: number) => SurfaceManager._getPositionInternal(managedStreamPtr, context), 'iii');
            var fSeek = (<any>Module).addFunction((managedStreamPtr: number, context: number, position: any) => SurfaceManager._seekInternal(managedStreamPtr, context, position), 'iiii');
            var fMove = (<any>Module).addFunction((managedStreamPtr: number, context: number, offset: any) => SurfaceManager._moveInternal(managedStreamPtr, context, offset), 'iiii');
            var fGetLength = (<any>Module).addFunction((managedStreamPtr: number, context: number) => SurfaceManager._getLengthInternal(managedStreamPtr, context), 'iii');
            var fDuplicate = (<any>Module).addFunction((managedStreamPtr: number, context:number) => SurfaceManager._duplicateInternal(managedStreamPtr, context), 'iii');
            var fForkNew = (<any>Module).addFunction((managedStreamPtr: number, context: number) => SurfaceManager._forkInternal(managedStreamPtr, context), 'iii');
            var fDestroy = (<any>Module).addFunction((managedStreamPtr: number, context: number) => SurfaceManager._destroyInternal(managedStreamPtr, context), 'vii');

            return [
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
                fDuplicate,
                fForkNew,
                fDestroy
            ];
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