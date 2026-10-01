using System;

namespace SkiaSharp.Views.Maui.Controls.Gtk4;

internal static class Gtk4Sizing
{
	public static SKImageInfo GetDisplayInfo(SKImageInfo rawInfo, int scale, bool ignorePixelScaling)
	{
		if (scale < 1)
			throw new ArgumentOutOfRangeException(nameof(scale));

		return ignorePixelScaling
			? rawInfo.WithSize(new SKSizeI(rawInfo.Width / scale, rawInfo.Height / scale))
			: rawInfo;
	}

	public static SKPoint GetTouchLocation(double x, double y, int scale, bool ignorePixelScaling)
	{
		if (scale < 1)
			throw new ArgumentOutOfRangeException(nameof(scale));

		var factor = ignorePixelScaling ? 1 : scale;
		return new SKPoint((float)(x * factor), (float)(y * factor));
	}
}
