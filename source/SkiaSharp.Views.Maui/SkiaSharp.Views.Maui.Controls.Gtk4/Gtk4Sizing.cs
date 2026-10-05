using System;

namespace SkiaSharp.Views.Maui.Controls.Gtk4;

internal static class Gtk4Sizing
{
	public static SKPoint GetTouchLocation(double x, double y, int scale, bool ignorePixelScaling)
	{
		if (scale < 1)
			throw new ArgumentOutOfRangeException(nameof(scale));

		var factor = ignorePixelScaling ? 1 : scale;
		return new SKPoint((float)(x * factor), (float)(y * factor));
	}
}
