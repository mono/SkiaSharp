
type DpiWatcherCallback = DotNet.DotNetObjectReference | ((lastDpi: number, currentDpi: number) => void);

export class DpiWatcher {
	static lastDpi: number;
	static timerId: number;
	static callback: DpiWatcherCallback;

	public static getDpi() {
		return window.devicePixelRatio;
	}

	public static start(callback: DpiWatcherCallback): number {
		//console.info(`Starting DPI watcher with callback ${callback._id}...`);

		DpiWatcher.lastDpi = window.devicePixelRatio;
		DpiWatcher.timerId = window.setInterval(DpiWatcher.update, 1000);
		DpiWatcher.callback = callback;

		return DpiWatcher.lastDpi;
	}

	public static stop() {
		//console.info(`Stopping DPI watcher with callback ${DpiWatcher.callback._id}...`);

		window.clearInterval(DpiWatcher.timerId);

		DpiWatcher.callback = undefined;
	}

	static update() {
		if (!DpiWatcher.callback)
			return;

		const currentDpi = window.devicePixelRatio;
		const lastDpi = DpiWatcher.lastDpi;
		DpiWatcher.lastDpi = currentDpi;

		if (Math.abs(lastDpi - currentDpi) > 0.001) {
			if (typeof DpiWatcher.callback === 'function') {
				DpiWatcher.callback(lastDpi, currentDpi);
			} else {
				DpiWatcher.callback.invokeMethod('Invoke', lastDpi, currentDpi);
			}
		}
	}
}
