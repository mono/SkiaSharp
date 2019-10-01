var SkiaSharp;
(function (SkiaSharp) {
    class SurfaceManager {
        static registerManagedStream() {
            SurfaceManager._readInternal = Module.mono_bind_static_method("[SkiaSharp] SkiaSharp.SKAbstractManagedStream:ReadInternal");
            SurfaceManager._peekInternal = Module.mono_bind_static_method("[SkiaSharp] SkiaSharp.SKAbstractManagedStream:PeekInternal");
            SurfaceManager._isAtEndInternal = Module.mono_bind_static_method("[SkiaSharp] SkiaSharp.SKAbstractManagedStream:IsAtEndInternal");
            SurfaceManager._hasPositionInternal = Module.mono_bind_static_method("[SkiaSharp] SkiaSharp.SKAbstractManagedStream:HasPositionInternal");
            SurfaceManager._hasLengthInternal = Module.mono_bind_static_method("[SkiaSharp] SkiaSharp.SKAbstractManagedStream:HasLengthInternal");
            SurfaceManager._rewindInternal = Module.mono_bind_static_method("[SkiaSharp] SkiaSharp.SKAbstractManagedStream:RewindInternal");
            SurfaceManager._getPositionInternal = Module.mono_bind_static_method("[SkiaSharp] SkiaSharp.SKAbstractManagedStream:GetPositionInternal");
            SurfaceManager._seekInternal = Module.mono_bind_static_method("[SkiaSharp] SkiaSharp.SKAbstractManagedStream:SeekInternal");
            SurfaceManager._moveInternal = Module.mono_bind_static_method("[SkiaSharp] SkiaSharp.SKAbstractManagedStream:MoveInternal");
            SurfaceManager._getLengthInternal = Module.mono_bind_static_method("[SkiaSharp] SkiaSharp.SKAbstractManagedStream:GetLengthInternal");
            SurfaceManager._duplicateInternal = Module.mono_bind_static_method("[SkiaSharp] SkiaSharp.SKAbstractManagedStream:DuplicateInternal");
            SurfaceManager._forkInternal = Module.mono_bind_static_method("[SkiaSharp] SkiaSharp.SKAbstractManagedStream:ForkInternal");
            SurfaceManager._destroyInternal = Module.mono_bind_static_method("[SkiaSharp] SkiaSharp.SKAbstractManagedStream:DestroyInternal");
            var fRead = Module.addFunction((managedStreamPtr, context, buffer, size) => SurfaceManager._readInternal(managedStreamPtr, context, buffer, size), 'iiiii');
            var fPeek = Module.addFunction((managedStreamPtr, context, buffer, size) => SurfaceManager._peekInternal(managedStreamPtr, context, buffer, size), 'iiiii');
            var fIsAtEnd = Module.addFunction((managedStreamPtr, context) => SurfaceManager._isAtEndInternal(managedStreamPtr, context), 'iii');
            var fHasPosition = Module.addFunction((managedStreamPtr, context) => SurfaceManager._hasPositionInternal(managedStreamPtr, context), 'iii');
            var fHasLength = Module.addFunction((managedStreamPtr, context) => SurfaceManager._hasLengthInternal(managedStreamPtr, context), 'iii');
            var fRewind = Module.addFunction((managedStreamPtr, context) => SurfaceManager._rewindInternal(managedStreamPtr, context), 'iii');
            var fGetPosition = Module.addFunction((managedStreamPtr, context) => SurfaceManager._getPositionInternal(managedStreamPtr, context), 'iii');
            var fSeek = Module.addFunction((managedStreamPtr, context, position) => SurfaceManager._seekInternal(managedStreamPtr, context, position), 'iiii');
            var fMove = Module.addFunction((managedStreamPtr, context, offset) => SurfaceManager._moveInternal(managedStreamPtr, context, offset), 'iiii');
            var fGetLength = Module.addFunction((managedStreamPtr, context) => SurfaceManager._getLengthInternal(managedStreamPtr, context), 'iii');
            var fDuplicate = Module.addFunction((managedStreamPtr, context) => SurfaceManager._duplicateInternal(managedStreamPtr, context), 'iii');
            var fForkNew = Module.addFunction((managedStreamPtr, context) => SurfaceManager._forkInternal(managedStreamPtr, context), 'iii');
            var fDestroy = Module.addFunction((managedStreamPtr, context) => SurfaceManager._destroyInternal(managedStreamPtr, context), 'vii');
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
        static invalidateCanvas(pData, canvasId, width, height) {
            var c = document.getElementById(canvasId);
            c.width = width;
            c.height = height;
            var ctx = c.getContext("2d");
            var buffer = new Uint8ClampedArray(Module.HEAPU8.buffer, pData, width * height * 4);
            var imageData = new ImageData(buffer, width, height);
            ctx.putImageData(imageData, 0, 0);
            return true;
        }
    }
    SkiaSharp.SurfaceManager = SurfaceManager;
})(SkiaSharp || (SkiaSharp = {}));
