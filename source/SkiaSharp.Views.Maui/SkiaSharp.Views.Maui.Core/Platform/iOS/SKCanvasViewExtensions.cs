using SkiaSharp.Views.iOS;

namespace SkiaSharp.Views.Maui.Platform
{
	public static class SKCanvasViewExtensions
	{
		public static void UpdateIgnorePixelScaling(this SKCanvasView nativeView, ISKCanvasView canvasView) =>
			nativeView.IgnorePixelScaling = canvasView?.IgnorePixelScaling ?? false;
	}
}
