using SkiaSharp.Views.Windows;

namespace SkiaSharp.Views.Maui.Platform
{
	public static class SKCanvasViewExtensions
	{
		public static void UpdateIgnorePixelScaling(this SKXamlCanvas nativeView, ISKCanvasView canvasView) =>
			nativeView.IgnorePixelScaling = canvasView?.IgnorePixelScaling ?? false;
	}
}
