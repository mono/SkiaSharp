// Shared, pure-browser canvas presentation primitives ("render steps").
//
// This module contains no emscripten and no .NET-callback dependencies, so it is safe to
// import in every Blazor host (WebAssembly, Server, Hybrid, static SSR). Both the WebAssembly
// direct path (SKHtmlCanvas) and the bridged path (SKHtmlCanvasBridge) funnel their final
// paint through here so the pixel-to-canvas logic is not duplicated.

type GLBlitState = {
	gl: WebGL2RenderingContext | WebGLRenderingContext;
	program: WebGLProgram;
	texture: WebGLTexture;
	positionBuffer: WebGLBuffer;
	positionLocation: number;
	texCoordLocation: number;
};

const glStates = new WeakMap<HTMLCanvasElement, GLBlitState>();

export function sizeCanvas(canvas: HTMLCanvasElement, width: number, height: number): void {
	if (!canvas || width <= 0 || height <= 0)
		return;
	if (canvas.width !== width)
		canvas.width = width;
	if (canvas.height !== height)
		canvas.height = height;
}

export function present2DPixels(canvas: HTMLCanvasElement, bytes: Uint8Array, width: number, height: number): boolean {
	if (!canvas || !bytes || width <= 0 || height <= 0)
		return false;

	const ctx = canvas.getContext('2d');
	if (!ctx)
		return false;

	sizeCanvas(canvas, width, height);

	const buffer = bytes.buffer as ArrayBuffer;
	const clamped = new Uint8ClampedArray(buffer, bytes.byteOffset, bytes.byteLength);
	const imageData = new ImageData(clamped, width, height);
	ctx.putImageData(imageData, 0, 0);
	return true;
}

export function present2DBitmap(canvas: HTMLCanvasElement, bitmap: ImageBitmap, width: number, height: number): boolean {
	if (!canvas || !bitmap)
		return false;

	const ctx = canvas.getContext('2d');
	if (!ctx)
		return false;

	sizeCanvas(canvas, width || bitmap.width, height || bitmap.height);
	ctx.clearRect(0, 0, canvas.width, canvas.height);
	ctx.drawImage(bitmap, 0, 0);
	return true;
}

const VERTEX_SHADER = `#version 300 es
in vec2 a_position;
in vec2 a_texCoord;
out vec2 v_texCoord;
void main() {
	gl_Position = vec4(a_position, 0.0, 1.0);
	v_texCoord = a_texCoord;
}`;

const FRAGMENT_SHADER = `#version 300 es
precision mediump float;
in vec2 v_texCoord;
uniform sampler2D u_image;
out vec4 outColor;
void main() {
	outColor = texture(u_image, v_texCoord);
}`;

function compileShader(gl: WebGL2RenderingContext, type: number, source: string): WebGLShader | null {
	const shader = gl.createShader(type);
	if (!shader)
		return null;
	gl.shaderSource(shader, source);
	gl.compileShader(shader);
	if (!gl.getShaderParameter(shader, gl.COMPILE_STATUS)) {
		console.error('SKCanvasPresenter shader error: ' + gl.getShaderInfoLog(shader));
		gl.deleteShader(shader);
		return null;
	}
	return shader;
}

function ensureGLState(canvas: HTMLCanvasElement): GLBlitState | null {
	let state = glStates.get(canvas);
	if (state)
		return state;

	const gl = canvas.getContext('webgl2') as WebGL2RenderingContext;
	if (!gl)
		return null;

	const vs = compileShader(gl, gl.VERTEX_SHADER, VERTEX_SHADER);
	const fs = compileShader(gl, gl.FRAGMENT_SHADER, FRAGMENT_SHADER);
	if (!vs || !fs)
		return null;

	const program = gl.createProgram()!;
	gl.attachShader(program, vs);
	gl.attachShader(program, fs);
	gl.linkProgram(program);
	if (!gl.getProgramParameter(program, gl.LINK_STATUS)) {
		console.error('SKCanvasPresenter link error: ' + gl.getProgramInfoLog(program));
		return null;
	}

	const positionLocation = gl.getAttribLocation(program, 'a_position');
	const texCoordLocation = gl.getAttribLocation(program, 'a_texCoord');

	// two triangles covering the viewport, with matching (flipped) texture coords
	const positionBuffer = gl.createBuffer()!;
	gl.bindBuffer(gl.ARRAY_BUFFER, positionBuffer);
	gl.bufferData(gl.ARRAY_BUFFER, new Float32Array([
		// x, y, u, v
		-1, -1, 0, 1,
		 1, -1, 1, 1,
		-1,  1, 0, 0,
		-1,  1, 0, 0,
		 1, -1, 1, 1,
		 1,  1, 1, 0,
	]), gl.STATIC_DRAW);

	const texture = gl.createTexture()!;
	gl.bindTexture(gl.TEXTURE_2D, texture);
	gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_S, gl.CLAMP_TO_EDGE);
	gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_T, gl.CLAMP_TO_EDGE);
	gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl.LINEAR);
	gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MAG_FILTER, gl.LINEAR);

	state = { gl, program, texture, positionBuffer, positionLocation, texCoordLocation };
	glStates.set(canvas, state);
	return state;
}

function drawGL(state: GLBlitState, canvas: HTMLCanvasElement): void {
	const gl = state.gl;
	gl.viewport(0, 0, canvas.width, canvas.height);
	gl.clearColor(0, 0, 0, 0);
	gl.clear(gl.COLOR_BUFFER_BIT);

	gl.useProgram(state.program);
	gl.bindBuffer(gl.ARRAY_BUFFER, state.positionBuffer);
	gl.enableVertexAttribArray(state.positionLocation);
	gl.vertexAttribPointer(state.positionLocation, 2, gl.FLOAT, false, 16, 0);
	gl.enableVertexAttribArray(state.texCoordLocation);
	gl.vertexAttribPointer(state.texCoordLocation, 2, gl.FLOAT, false, 16, 8);

	gl.drawArrays(gl.TRIANGLES, 0, 6);
}

export function presentGLPixels(canvas: HTMLCanvasElement, bytes: Uint8Array, width: number, height: number): boolean {
	if (!canvas || !bytes || width <= 0 || height <= 0)
		return false;

	sizeCanvas(canvas, width, height);
	const state = ensureGLState(canvas);
	if (!state)
		return false;

	const gl = state.gl;
	const pixels = bytes instanceof Uint8Array ? bytes : new Uint8Array(bytes);
	gl.bindTexture(gl.TEXTURE_2D, state.texture);
	gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA, width, height, 0, gl.RGBA, gl.UNSIGNED_BYTE, pixels);
	drawGL(state, canvas);
	return true;
}

export function presentGLBitmap(canvas: HTMLCanvasElement, bitmap: ImageBitmap, width: number, height: number): boolean {
	if (!canvas || !bitmap)
		return false;

	sizeCanvas(canvas, width || bitmap.width, height || bitmap.height);
	const state = ensureGLState(canvas);
	if (!state)
		return false;

	const gl = state.gl;
	gl.bindTexture(gl.TEXTURE_2D, state.texture);
	gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA, gl.RGBA, gl.UNSIGNED_BYTE, bitmap);
	drawGL(state, canvas);
	return true;
}

export function disposePresenter(canvas: HTMLCanvasElement): void {
	glStates.delete(canvas);
}
