using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace SkiaSharp.Views.Blazor.Internal
{
	internal class SKGLViewInterop : JSModuleInterop
	{
		private const string JsFilename = "./_content/SkiaSharp.Views.Blazor/SKGLView.js";
		private const string InitSymbol = "SKGLView.init";
		private const string DeinitSymbol = "SKGLView.deinit";
		private const string RequestAnimationFrameSymbol = "SKGLView.requestAnimationFrame";

		private readonly ElementReference htmlCanvas;
		private readonly string htmlElementId;
		private readonly ActionHelper callbackHelper;

		private DotNetObjectReference<ActionHelper>? callbackReference;

		public SKGLViewInterop(IJSRuntime js, ElementReference element, Action renderFrameCallback)
			: base(js, JsFilename)
		{
			htmlCanvas = element;
			htmlElementId = element.Id;

			callbackHelper = new ActionHelper(renderFrameCallback);
		}

		protected override Task OnDisposingModuleAsync() =>
			DeinitAsync();

		public Task<Info> InitAsync()
		{
			if (callbackReference != null)
				throw new InvalidOperationException("Unable to initialize the same canvas more than once.");

			callbackReference = DotNetObjectReference.Create(callbackHelper);

			return InvokeAsync<Info>(InitSymbol, htmlCanvas, htmlElementId, callbackReference);
		}

		public async Task DeinitAsync()
		{
			if (callbackReference == null)
				return;

			await InvokeAsync(DeinitSymbol, htmlElementId);

			callbackReference?.Dispose();
		}

		public Task RequestAnimationFrameAsync(bool enableRenderLoop, int rawWidth, int rawHeight) =>
			InvokeAsync(RequestAnimationFrameSymbol, htmlCanvas, enableRenderLoop, rawWidth, rawHeight);

		public record Info(int ContextId, uint FboId, int Stencils, int Samples, int Depth);
	}
}
