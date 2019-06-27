declare namespace SkiaSharp {
    class SurfaceManager {
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
        static registerManagedStream(): any[];
        static invalidateCanvas(pData: number, canvasId: string, width: number, height: number): boolean;
    }
}
declare const CanvasKit: any;
declare const MonoRuntime: Uno.UI.Interop.IMonoRuntime;
declare const MonoSupport: any;
declare module Uno.UI.Interop {
    interface IMonoRuntime {
        mono_string(str: string): Interop.IMonoStringHandle;
        conv_string(strHandle: Interop.IMonoStringHandle): string;
    }
    interface IMonoStringHandle {
    }
}
