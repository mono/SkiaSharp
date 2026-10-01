namespace SkiaSharp.Views.Maui.Controls.WPF.Platform;

internal static class WpfCanvasMetrics
{
	public static SKPoint GetTouchLocation(double x, double y, double scaleX, double scaleY, bool ignorePixelScaling) =>
		new((float)(x * (ignorePixelScaling ? 1 : scaleX)), (float)(y * (ignorePixelScaling ? 1 : scaleY)));

	public static SKSizeI GetLogicalSize(double width, double height) =>
		new((int)width, (int)height);
}
