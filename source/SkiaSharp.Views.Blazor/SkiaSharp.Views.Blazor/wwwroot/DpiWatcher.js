
export class DpiWatcher {
    static getDpi() {
        return window.devicePixelRatio;
    }

    static start() {
        DpiWatcher.lastDpi = window.devicePixelRatio;
        DpiWatcher.timerId = window.setInterval(DpiWatcher.update, 1000);
    }

    static stop() {
        window.clearInterval(DpiWatcher.timerId);
    }

    static update() {
        const currentDpi = window.devicePixelRatio;
        const lastDpi = DpiWatcher.lastDpi;
        DpiWatcher.lastDpi = currentDpi;

        if (Math.abs(lastDpi - currentDpi) > 0.001) {
            DotNet.invokeMethodAsync('SkiaSharp.Views.Blazor', 'UpdateDpi', lastDpi, currentDpi);
        }
    }
}
