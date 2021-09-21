using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

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

		public static async Task<(SKGLViewInterop, Info)> ImportAsync(IJSRuntime js, ElementReference element, Action callback)
		{
			var interop = new SKGLViewInterop(js, element, callback);
			await interop.ImportAsync();
			var info = interop.Init();
			return (interop, info);
		}

		public SKGLViewInterop(IJSRuntime js, ElementReference element, Action renderFrameCallback)
			: base(js, JsFilename)
		{
			htmlCanvas = element;
			htmlElementId = element.Id;

			callbackHelper = new ActionHelper(renderFrameCallback);
		}

		protected override void OnDisposingModule() =>
			Deinit();

		public Info Init()
		{
			if (callbackReference != null)
				throw new InvalidOperationException("Unable to initialize the same canvas more than once.");

			callbackReference = DotNetObjectReference.Create(callbackHelper);

			return Invoke<Info>(InitSymbol, htmlCanvas, htmlElementId, callbackReference);
		}

		public void Deinit()
		{
			if (callbackReference == null)
				return;

			Invoke(DeinitSymbol, htmlElementId);

			callbackReference?.Dispose();
		}

		public void RequestAnimationFrame(bool enableRenderLoop, int rawWidth, int rawHeight) =>
			Invoke(RequestAnimationFrameSymbol, htmlCanvas, enableRenderLoop, rawWidth, rawHeight);

		public record Info(int ContextId, uint FboId, int Stencils, int Samples, int Depth);
	}
}
