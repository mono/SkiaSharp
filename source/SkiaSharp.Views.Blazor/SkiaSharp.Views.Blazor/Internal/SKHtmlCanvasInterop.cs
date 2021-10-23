using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace SkiaSharp.Views.Blazor.Internal
{
	internal class SKHtmlCanvasInterop : JSModuleInterop
	{
		private const string JsFilename = "./_content/SkiaSharp.Views.Blazor/SKHtmlCanvas.js";
		private const string InitGLSymbol = "SKHtmlCanvas.initGL";
		private const string InitRasterSymbol = "SKHtmlCanvas.initRaster";
		private const string DeinitSymbol = "SKHtmlCanvas.deinit";
		private const string RequestAnimationFrameSymbol = "SKHtmlCanvas.requestAnimationFrame";
		private const string PutImageDataSymbol = "SKHtmlCanvas.putImageData";

		private readonly ElementReference htmlCanvas;
		private readonly string htmlElementId;
		private readonly ActionHelper callbackHelper;

		private DotNetObjectReference<ActionHelper>? callbackReference;

		public static async Task<SKHtmlCanvasInterop> ImportAsync(IJSRuntime js, ElementReference element, Action callback)
		{
			var interop = new SKHtmlCanvasInterop(js, element, callback);
			await interop.ImportAsync();
			return interop;
		}

		public SKHtmlCanvasInterop(IJSRuntime js, ElementReference element, Action renderFrameCallback)
			: base(js, JsFilename)
		{
			htmlCanvas = element;
			htmlElementId = element.Id;

			callbackHelper = new ActionHelper(renderFrameCallback);
		}

		protected override async Task OnDisposingModuleAsync() =>
			await DeinitAsync();

		public async Task<GLInfo> InitGLAsync()
		{
			if (callbackReference != null)
				throw new InvalidOperationException("Unable to initialize the same canvas more than once.");

			callbackReference = DotNetObjectReference.Create(callbackHelper);

			return await InvokeAsync<GLInfo>(InitGLSymbol, htmlCanvas, htmlElementId, callbackReference);
		}

		public async Task<bool> InitRasterAsync()
		{
			if (callbackReference != null)
				throw new InvalidOperationException("Unable to initialize the same canvas more than once.");

			callbackReference = DotNetObjectReference.Create(callbackHelper);

			return await InvokeAsync<bool>(InitRasterSymbol, htmlCanvas, htmlElementId, callbackReference);
		}

		public async Task DeinitAsync()
		{
			if (callbackReference == null)
				return;

			await InvokeAsync(DeinitSymbol, htmlElementId);

			callbackReference?.Dispose();
		}

		public async Task RequestAnimationFrame(bool enableRenderLoop, int rawWidth, int rawHeight) =>
			await InvokeAsync(RequestAnimationFrameSymbol, htmlCanvas, enableRenderLoop, rawWidth, rawHeight);

		public async Task PutImageData(IntPtr intPtr, SKSizeI rawSize) =>
			await InvokeAsync(PutImageDataSymbol, htmlCanvas, intPtr.ToInt64(), rawSize.Width, rawSize.Height);

		public record GLInfo(int ContextId, uint FboId, int Stencils, int Samples, int Depth);
	}
}
