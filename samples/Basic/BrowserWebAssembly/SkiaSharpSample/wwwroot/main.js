import { dotnet } from './_framework/dotnet.js'

// ── Bootstrap .NET WASM runtime ──────────────────────────────────────
const { setModuleImports, getAssemblyExports, getConfig } = await dotnet.create();

// Shared render callback: copies pixel buffer from C# onto the active canvas
setModuleImports('main.js', {
    renderer: {
        render: (width, height, buffer) => {
            const cvs = getActiveCanvas();
            if (!cvs) return;
            cvs.width = width;
            cvs.height = height;
            const ctx = cvs.getContext('2d');
            const clamped = new Uint8ClampedArray(buffer.slice());
            const image = new ImageData(clamped, width, height);
            ctx.putImageData(image, 0, 0);
        }
    }
});

const config = getConfig();
const exports = await getAssemblyExports(config.mainAssemblyName);
const api = exports.SampleRenderer;

// Start the .NET runtime — Main loops forever to keep it alive for JSExport calls.
// Don't await — we need to proceed to set up the JS UI immediately.
dotnet.run().catch(() => {});

// ── Hide loading spinner ─────────────────────────────────────────────
document.getElementById('loading')?.remove();

// Small delay to let the runtime fully initialize
await new Promise(r => setTimeout(r, 500));

// Render the initial CPU page now that runtime is ready
try {
    const [w, h] = canvasSize();
    api.RenderCpu(w, h);
} catch (e) {
    console.error('Initial CPU render failed:', e);
}

// ── Helpers ──────────────────────────────────────────────────────────
function getActiveCanvas() {
    const pane = document.querySelector('.tab-pane.active');
    return pane?.querySelector('canvas');
}

function canvasSize() {
    const cvs = getActiveCanvas();
    return cvs ? [cvs.width, cvs.height] : [800, 600];
}

// ── Tab switching ────────────────────────────────────────────────────
let currentTab = 'cpu';
let gpuRunning = false;

document.querySelectorAll('#sampleTabs button').forEach(btn => {
    btn.addEventListener('shown.bs.tab', () => {
        const id = btn.id.replace('-tab', '');
        currentTab = id;
        gpuRunning = false;

        if (id === 'cpu') {
            const [w, h] = canvasSize();
            api.RenderCpu(w, h);
        } else if (id === 'gpu') {
            startGpuLoop();
        } else if (id === 'drawing') {
            const [w, h] = canvasSize();
            api.DrawingClear(w, h);
        }
    });
});

// ── CPU page: rendered once on init (see dotnet.run above) ───────────

// ── GPU page: animation loop ─────────────────────────────────────────
let gpuTouchActive = false;
let gpuTouchX = 0;
let gpuTouchY = 0;
let fpsFrames = 0;
let fpsLast = performance.now();

function startGpuLoop() {
    gpuRunning = true;
    const startTime = performance.now();

    function frame() {
        if (!gpuRunning || currentTab !== 'gpu') return;

        const [w, h] = canvasSize();
        const time = (performance.now() - startTime) / 1000;
        api.RenderGpu(w, h, time, gpuTouchX, gpuTouchY, gpuTouchActive);

        // FPS counter
        fpsFrames++;
        const now = performance.now();
        if (now - fpsLast >= 500) {
            const fps = (fpsFrames / ((now - fpsLast) / 1000)).toFixed(0);
            const badge = document.getElementById('fps-badge');
            if (badge) badge.textContent = fps + ' FPS';
            fpsFrames = 0;
            fpsLast = now;
        }

        requestAnimationFrame(frame);
    }
    requestAnimationFrame(frame);
}

// GPU pointer events (use document-level delegation for the active canvas)
document.addEventListener('pointerdown', e => {
    if (currentTab !== 'gpu') return;
    const cvs = getActiveCanvas();
    if (e.target !== cvs) return;
    const rect = cvs.getBoundingClientRect();
    gpuTouchActive = true;
    gpuTouchX = (e.clientX - rect.left) / rect.width;
    gpuTouchY = (e.clientY - rect.top) / rect.height;
});
document.addEventListener('pointermove', e => {
    if (!gpuTouchActive) return;
    const cvs = getActiveCanvas();
    if (!cvs) return;
    const rect = cvs.getBoundingClientRect();
    gpuTouchX = (e.clientX - rect.left) / rect.width;
    gpuTouchY = (e.clientY - rect.top) / rect.height;
});
document.addEventListener('pointerup', () => { gpuTouchActive = false; });
document.addEventListener('pointercancel', () => { gpuTouchActive = false; });

// ── Drawing page ─────────────────────────────────────────────────────
let drawingColorIndex = 0;
let isDrawing = false;

// Color swatches
document.querySelectorAll('.color-swatch').forEach(el => {
    el.addEventListener('click', () => {
        document.querySelectorAll('.color-swatch').forEach(s => s.classList.remove('active'));
        el.classList.add('active');
        drawingColorIndex = parseInt(el.dataset.color, 10);
    });
});

// Clear button
document.getElementById('clear-btn')?.addEventListener('click', () => {
    const [w, h] = canvasSize();
    api.DrawingClear(w, h);
});

// Pointer events for drawing
document.addEventListener('pointerdown', e => {
    if (currentTab !== 'drawing') return;
    const cvs = getActiveCanvas();
    if (e.target !== cvs || !e.isPrimary) return;
    cvs.setPointerCapture(e.pointerId);
    isDrawing = true;
    const rect = cvs.getBoundingClientRect();
    const x = (e.clientX - rect.left) * (cvs.width / rect.width);
    const y = (e.clientY - rect.top) * (cvs.height / rect.height);
    const size = parseFloat(document.getElementById('brush-size')?.value || '4');
    api.DrawingPointerDown(cvs.width, cvs.height, x, y, drawingColorIndex, size);
});

document.addEventListener('pointermove', e => {
    if (!isDrawing || currentTab !== 'drawing' || !e.isPrimary) return;
    const cvs = getActiveCanvas();
    if (!cvs) return;
    const rect = cvs.getBoundingClientRect();
    const x = (e.clientX - rect.left) * (cvs.width / rect.width);
    const y = (e.clientY - rect.top) * (cvs.height / rect.height);
    api.DrawingPointerMove(cvs.width, cvs.height, x, y);
});

document.addEventListener('pointerup', e => {
    if (!isDrawing || currentTab !== 'drawing' || !e.isPrimary) return;
    isDrawing = false;
    const [w, h] = canvasSize();
    api.DrawingPointerUp(w, h);
});

document.addEventListener('pointercancel', () => { isDrawing = false; });
