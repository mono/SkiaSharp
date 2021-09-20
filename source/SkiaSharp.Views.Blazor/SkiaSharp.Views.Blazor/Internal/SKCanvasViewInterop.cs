using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace SkiaSharp.Views.Blazor.Internal
{
	internal class SKCanvasViewInterop : JSModuleInterop
	{
		private const string JsFilename = "./_content/SkiaSharp.Views.Blazor/SKCanvasView.js";
		private const string InvalidateSymbol = "SKCanvasView.invalidate";

		private readonly ElementReference htmlCanvas;

		public SKCanvasViewInterop(IJSRuntime js, ElementReference element)
			: base(js, JsFilename)
		{
			htmlCanvas = element;
		}

		public Task<bool> InvalidateCanvasAsync(IntPtr intPtr, SKSizeI rawSize) =>
			InvokeAsync<bool>(InvalidateSymbol, htmlCanvas, intPtr.ToInt64(), rawSize.Width, rawSize.Height);
	}
}
