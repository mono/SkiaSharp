using SkiaSharp.Views.Tizen.NUI;

namespace SkiaSharp.Views.Maui.Platform
{
	public static class SKCanvasViewExtensions
	{
		public static void UpdateIgnorePixelScaling(this SKCanvasView nativeView, ISKCanvasView canvasView) =>
			nativeView.IgnorePixelScaling = canvasView?.IgnorePixelScaling ?? false;
	}
}
