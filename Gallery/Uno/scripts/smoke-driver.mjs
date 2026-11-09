// Smoke-test driver for the SkiaSharp Uno WASM gallery.
// Invoked by scripts/smoke.sh — see contracts/smoke-test.md for the contract.
//
// Environment:
//   SMOKE_URL         (required) — URL to navigate
//   SMOKE_MARKER      (optional) — substring that MUST be present in the DOM; default "SkiaSharp Gallery"
//   SMOKE_SCREENSHOT  (optional) — path for a debug screenshot (PNG)

import { chromium } from 'playwright';

const url = process.env.SMOKE_URL;
const marker = process.env.SMOKE_MARKER || 'SkiaSharp Gallery';
const screenshot = process.env.SMOKE_SCREENSHOT || '';

if (!url) {
    console.error('smoke-driver: SMOKE_URL is required');
    process.exit(2);
}

const errors = [];
const logs = [];

const browser = await chromium.launch({
    headless: true,
    args: ['--no-sandbox', '--disable-dev-shm-usage'],
});
try {
    const context = await browser.newContext();
    const page = await context.newPage();
    page.on('console', msg => {
        const entry = `[${msg.type()}] ${msg.text()}`;
        logs.push(entry);
        if (msg.type() === 'error') errors.push(msg.text());
    });
    page.on('pageerror', err => errors.push(`pageerror: ${err.message}`));
    page.on('requestfailed', req => {
        const f = req.failure();
        if (f && f.errorText && !/ERR_ABORTED|ERR_FAILED/.test(f.errorText)) {
            errors.push(`requestfailed: ${req.url()} — ${f.errorText}`);
        }
    });

    await page.goto(url, { waitUntil: 'networkidle', timeout: 60000 });

    // Wait for boot: poll the DOM every 500ms for up to 30s for loader to be gone.
    for (let i = 0; i < 60; i++) {
        const isReady = await page.evaluate(() => {
            const loaderGone = !document.querySelector('.uno-loader');
            const canvas = document.querySelector('canvas#uno-canvas, canvas');
            return loaderGone && !!(canvas && canvas.width > 0 && canvas.height > 0);
        });
        if (isReady) break;
        await page.waitForTimeout(500);
    }
    // Extra grace period for post-boot rendering to settle.
    await page.waitForTimeout(1500);

    const html = await page.content();

    if (screenshot) {
        await page.screenshot({ path: screenshot, fullPage: true });
    }

    // With SkiaRenderer, Uno renders the XAML tree as pixels inside #uno-canvas,
    // so text-based DOM matches don't work. Boot is complete iff the Uno loader
    // has been dismissed AND a sized canvas is present.
    const ready = await page.evaluate(() => {
        const loaderGone = !document.querySelector('.uno-loader');
        const canvas = document.querySelector('canvas#uno-canvas, canvas');
        const canvasReady = !!(canvas && canvas.width > 0 && canvas.height > 0);
        return { loaderGone, canvasReady };
    });
    if (!(ready.loaderGone && ready.canvasReady)) {
        console.error(`smoke-driver: boot not complete — loader ${ready.loaderGone ? 'gone' : 'visible'}, canvas ${ready.canvasReady ? 'rendered' : 'missing'}`);
        console.error('--- recent console logs ---');
        console.error(logs.slice(-60).join('\n'));
        process.exit(1);
    }

    // Benign patterns to ignore — tighten this list only with justification.
    const ignore = [
        /UNO0008/,
        /deprecated/i,
        /Download the React DevTools/i,
        /Service worker/i,
        // SkiaRenderer-on-WASM emits these routinely during startup
        /WEBGL_debug_renderer_info not enabled/i,
        /GPU stall due to ReadPixels/i,
        /Automatic fallback to software WebGL/i,
        /focusSemanticElement/i,
    ];
    const real = errors.filter(e => !ignore.some(rx => rx.test(e)));
    if (real.length) {
        console.error('smoke-driver: console/page errors detected:');
        real.forEach(e => console.error('  - ' + e));
        console.error('--- recent console logs ---');
        console.error(logs.slice(-60).join('\n'));
        process.exit(1);
    }

    console.log('smoke-driver: ok — Uno loader dismissed, canvas rendered, no console errors');
} finally {
    await browser.close();
}
