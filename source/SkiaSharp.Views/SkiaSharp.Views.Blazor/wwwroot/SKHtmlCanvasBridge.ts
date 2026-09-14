// Bridged presentation for Blazor Server / Hybrid / static SSR.
//
// Unlike SKHtmlCanvas (the WebAssembly direct path) this module has NO emscripten dependency,
// so it can run in a Blazor Server client browser or a Hybrid WebView where SkiaSharp runs on
// the .NET side and only the encoded/raw frame bytes arrive here. All painting is delegated to
// the shared SKCanvasPresenter primitives.

import * as Presenter from './SKCanvasPresenter.js';

type DotNetRef = { invokeMethodAsync: (method: string, ...args: any[]) => Promise<any>; };

type BridgeState = {
	dotNetRef: DotNetRef;
	isGL: boolean;
	resizeObserver?: ResizeObserver;
	dpiTimer?: number;
	lastWidth: number;
	lastHeight: number;
	lastDpr: number;
};

const states = new WeakMap<HTMLCanvasElement, BridgeState>();

function measure(canvas: HTMLCanvasElement) {
	return {
		width: Math.max(0, Math.round(canvas.clientWidth || 0)),
		height: Math.max(0, Math.round(canvas.clientHeight || 0)),
		dpr: window.devicePixelRatio || 1,
	};
}

function reportMetrics(canvas: HTMLCanvasElement, state: BridgeState, force: boolean): void {
	const m = measure(canvas);
	if (!force && m.width === state.lastWidth && m.height === state.lastHeight && Math.abs(m.dpr - state.lastDpr) < 0.001)
		return;

	state.lastWidth = m.width;
	state.lastHeight = m.height;
	state.lastDpr = m.dpr;

	if (state.dotNetRef)
		state.dotNetRef.invokeMethodAsync('OnMetricsChanged', m.width, m.height, m.dpr);
}

export function initialize(canvas: HTMLCanvasElement, dotNetRef: DotNetRef, isGL: boolean): void {
	if (!canvas)
		return;

	deinit(canvas);

	const state: BridgeState = {
		dotNetRef,
		isGL: !!isGL,
		lastWidth: -1,
		lastHeight: -1,
		lastDpr: -1,
	};
	states.set(canvas, state);

	if (typeof ResizeObserver === 'function') {
		state.resizeObserver = new ResizeObserver(() => reportMetrics(canvas, state, false));
		state.resizeObserver.observe(canvas);
	}

	// devicePixelRatio has no event; poll like the existing DpiWatcher.
	state.dpiTimer = window.setInterval(() => reportMetrics(canvas, state, false), 1000);

	reportMetrics(canvas, state, true);
}

export function getMetrics(canvas: HTMLCanvasElement) {
	return measure(canvas);
}

export function present(
	canvas: HTMLCanvasElement,
	bytes: Uint8Array,
	width: number,
	height: number,
	format: string,
	isGL: boolean): void | Promise<void> {

	if (!canvas || !bytes || width <= 0 || height <= 0)
		return;

	const raw = format === 'put';
	if (raw) {
		const pixels = bytes instanceof Uint8Array ? bytes : new Uint8Array(bytes);
		if (isGL)
			Presenter.presentGLPixels(canvas, pixels, width, height);
		else
			Presenter.present2DPixels(canvas, pixels, width, height);
		return;
	}

	const type = format === 'png' ? 'image/png' : 'image/jpeg';
	const blob = new Blob([bytes as BlobPart], { type });
	return createImageBitmap(blob).then(bitmap => {
		try {
			if (isGL)
				Presenter.presentGLBitmap(canvas, bitmap, width, height);
			else
				Presenter.present2DBitmap(canvas, bitmap, width, height);
		} finally {
			bitmap.close();
		}
	}).catch(err => console.error('SKHtmlCanvasBridge present error: ' + err));
}

export function deinit(canvas: HTMLCanvasElement): void {
	const state = states.get(canvas);
	if (!state)
		return;

	if (state.resizeObserver)
		state.resizeObserver.disconnect();
	if (state.dpiTimer)
		window.clearInterval(state.dpiTimer);

	Presenter.disposePresenter(canvas);
	states.delete(canvas);
}
