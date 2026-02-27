// Minimal JS for rendering SkiaSharp server-rendered pixels onto <canvas> elements.
window.SkiaCanvas = {

    // Draws a WebP/PNG image (as byte array) onto a canvas using createImageBitmap.
    // This is the efficient path: compressed bytes transfer, off-thread decode.
    drawImageFromBytes: async function (canvasId, imageBytes) {
        const canvas = document.getElementById(canvasId);
        if (!canvas) return;

        const ctx = canvas.getContext("2d");
        const blob = new Blob([new Uint8Array(imageBytes)], { type: "image/webp" });
        const bitmap = await createImageBitmap(blob);

        ctx.drawImage(bitmap, 0, 0);
        bitmap.close();
    },

    // Draws raw RGBA pixel data onto a canvas using putImageData.
    // This is the lossless path: uncompressed pixels, larger payload, pixel-perfect.
    putPixelData: function (canvasId, rgbaBytes, width, height) {
        const canvas = document.getElementById(canvasId);
        if (!canvas) return;

        const ctx = canvas.getContext("2d");
        const clampedArray = new Uint8ClampedArray(rgbaBytes);
        const imageData = new ImageData(clampedArray, width, height);

        ctx.putImageData(imageData, 0, 0);
    }
};

// Dynamic server-rendered image management.
// Watches container size and periodically refreshes from /api/skia endpoint.
window.SkiaDynamic = {
    _instances: new Map(),

    // Start watching a dynamic image element.
    // Options: { variant, interval (ms, 0 = no animation), onResize (bool) }
    start: function (imgId, variant, interval, onResize) {
        const img = document.getElementById(imgId);
        if (!img) return;

        const state = { img, variant, interval, timer: null, resizeObserver: null, lastW: 0, lastH: 0, loading: false };
        this._instances.set(imgId, state);

        const refresh = () => {
            const rect = img.getBoundingClientRect();
            // Cap at 800px to keep images compact; scale down on small screens
            const w = Math.min(Math.round(rect.width) || 800, 800);
            const h = Math.round(w * 3 / 8);
            if (w < 10) return;

            // Skip if size hasn't changed and this is a resize-triggered refresh
            if (!interval && w === state.lastW && h === state.lastH) return;
            state.lastW = w;
            state.lastH = h;

            if (state.loading) return;
            state.loading = true;

            const url = `/api/skia/${variant}?w=${w}&h=${h}&t=${Date.now()}`;
            const tempImg = new Image();
            tempImg.onload = () => {
                img.src = tempImg.src;
                state.loading = false;
            };
            tempImg.onerror = () => { state.loading = false; };
            tempImg.src = url;
        };

        // Initial fetch at correct size
        requestAnimationFrame(refresh);

        // Watch for container resize
        if (onResize && typeof ResizeObserver !== 'undefined') {
            state.resizeObserver = new ResizeObserver(() => refresh());
            state.resizeObserver.observe(img.parentElement || img);
        }

        // Periodic refresh for animation (clock)
        if (interval > 0) {
            state.timer = setInterval(refresh, interval);
        }
    },

    // Stop watching a dynamic image element.
    stop: function (imgId) {
        const state = this._instances.get(imgId);
        if (!state) return;
        if (state.timer) clearInterval(state.timer);
        if (state.resizeObserver) state.resizeObserver.disconnect();
        this._instances.delete(imgId);
    },

    // Stop all instances (cleanup)
    stopAll: function () {
        for (const id of this._instances.keys()) {
            this.stop(id);
        }
    }
};
