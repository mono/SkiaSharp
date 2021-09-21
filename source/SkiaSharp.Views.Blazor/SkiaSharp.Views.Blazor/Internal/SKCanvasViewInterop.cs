using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace SkiaSharp.Views.Blazor.Internal
{
	internal class SKCanvasViewInterop : JSModuleInterop
	{
		private const string JsFilename = "./_content/SkiaSharp.Views.Blazor/SKCanvasView.js";
		private const string InvalidateSymbol = "SKCanvasView.invalidate";

		private readonly ElementReference htmlCanvas;

		public static async Task<SKCanvasViewInterop> ImportAsync(IJSRuntime js, ElementReference element)
		{
			var interop = new SKCanvasViewInterop(js, element);
			await interop.ImportAsync();
			return interop;
		}

		public SKCanvasViewInterop(IJSRuntime js, ElementReference element)
			: base(js, JsFilename)
		{
			htmlCanvas = element;
		}

		public bool Invalidate(IntPtr intPtr, SKSizeI rawSize) =>
			Invoke<bool>(InvalidateSymbol, htmlCanvas, intPtr.ToInt64(), rawSize.Width, rawSize.Height);
	}
}
