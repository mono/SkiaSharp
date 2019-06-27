var SkiaSharp;
(function (SkiaSharp) {
    class SurfaceManager {
        static registerManagedStream() {
            SurfaceManager._readInternal = Module.mono_bind_static_method("[SkiaSharp] SkiaSharp.ManagedStreamHelper:ReadInternal");
            SurfaceManager._peekInternal = Module.mono_bind_static_method("[SkiaSharp] SkiaSharp.ManagedStreamHelper:PeekInternal");
            SurfaceManager._isAtEndInternal = Module.mono_bind_static_method("[SkiaSharp] SkiaSharp.ManagedStreamHelper:IsAtEndInternal");
            SurfaceManager._hasPositionInternal = Module.mono_bind_static_method("[SkiaSharp] SkiaSharp.ManagedStreamHelper:HasPositionInternal");
            SurfaceManager._hasLengthInternal = Module.mono_bind_static_method("[SkiaSharp] SkiaSharp.ManagedStreamHelper:HasLengthInternal");
            SurfaceManager._rewindInternal = Module.mono_bind_static_method("[SkiaSharp] SkiaSharp.ManagedStreamHelper:RewindInternal");
            SurfaceManager._getPositionInternal = Module.mono_bind_static_method("[SkiaSharp] SkiaSharp.ManagedStreamHelper:GetPositionInternal");
            SurfaceManager._seekInternal = Module.mono_bind_static_method("[SkiaSharp] SkiaSharp.ManagedStreamHelper:SeekInternal");
            SurfaceManager._moveInternal = Module.mono_bind_static_method("[SkiaSharp] SkiaSharp.ManagedStreamHelper:MoveInternal");
            SurfaceManager._getLengthInternal = Module.mono_bind_static_method("[SkiaSharp] SkiaSharp.ManagedStreamHelper:GetLengthInternal");
            SurfaceManager._createNewInternal = Module.mono_bind_static_method("[SkiaSharp] SkiaSharp.ManagedStreamHelper:CreateNewInternal");
            SurfaceManager._destroyInternal = Module.mono_bind_static_method("[SkiaSharp] SkiaSharp.ManagedStreamHelper:DestroyInternal");
            var fRead = Module.addFunction((managedStreamPtr, buffer, size) => SurfaceManager._readInternal(managedStreamPtr, buffer, size), 'iiii');
            var fPeek = Module.addFunction((managedStreamPtr, buffer, size) => SurfaceManager._peekInternal(managedStreamPtr, buffer, size), 'iiii');
            var fIsAtEnd = Module.addFunction((managedStreamPtr) => SurfaceManager._isAtEndInternal(managedStreamPtr), 'ii');
            var fHasPosition = Module.addFunction((managedStreamPtr) => SurfaceManager._hasPositionInternal(managedStreamPtr), 'ii');
            var fHasLength = Module.addFunction((managedStreamPtr) => SurfaceManager._hasLengthInternal(managedStreamPtr), 'ii');
            var fRewind = Module.addFunction((managedStreamPtr) => SurfaceManager._rewindInternal(managedStreamPtr), 'ii');
            var fGetPosition = Module.addFunction((managedStreamPtr) => SurfaceManager._getPositionInternal(managedStreamPtr), 'ii');
            var fSeek = Module.addFunction((managedStreamPtr, position) => SurfaceManager._seekInternal(managedStreamPtr, position), 'ii');
            var fMove = Module.addFunction((managedStreamPtr, offset) => SurfaceManager._moveInternal(managedStreamPtr, offset), 'ii');
            var fGetLength = Module.addFunction((managedStreamPtr) => SurfaceManager._getLengthInternal(managedStreamPtr), 'ii');
            var fCreateNew = Module.addFunction((managedStreamPtr) => SurfaceManager._createNewInternal(managedStreamPtr), 'ii');
            var fDestroy = Module.addFunction((managedStreamPtr) => SurfaceManager._destroyInternal(managedStreamPtr), 'vi');
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
                fCreateNew,
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
