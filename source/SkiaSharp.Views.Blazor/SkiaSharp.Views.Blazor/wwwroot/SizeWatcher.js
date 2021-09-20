export class SizeWatcher {
    static observe(element, elementId, callback) {
        if (!element || !callback)
            return;
        console.info(`Adding size watcher observation with callback ${callback._id}...`);
        SizeWatcher.init();
        const watcherElement = element;
        watcherElement.SizeWatcher = {
            callback: callback
        };
        SizeWatcher.elements[elementId] = element;
        SizeWatcher.observer.observe(element);
        SizeWatcher.invokeAsync(element);
    }
    static unobserve(elementId) {
        if (!elementId || !SizeWatcher.observer)
            return;
        console.info('Removing size watcher observation...');
        const element = SizeWatcher.elements[elementId];
        SizeWatcher.elements.delete(elementId);
        SizeWatcher.observer.unobserve(element);
    }
    static init() {
        if (SizeWatcher.observer)
            return;
        console.info('Starting size watcher...');
        SizeWatcher.elements = new Map();
        SizeWatcher.observer = new ResizeObserver((entries) => {
            for (let entry of entries) {
                SizeWatcher.invokeAsync(entry.target);
            }
        });
    }
    static invokeAsync(element) {
        const watcherElement = element;
        const instance = watcherElement.SizeWatcher;
        if (!instance || !instance.callback)
            return;
        return instance.callback.invokeMethodAsync('Invoke', element.clientWidth, element.clientHeight);
    }
}
