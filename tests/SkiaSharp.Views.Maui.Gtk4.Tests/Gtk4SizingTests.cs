using System;
using SkiaSharp.Views.Maui.Controls.Gtk4;
using Xunit;

namespace SkiaSharp.Views.Maui.Gtk4.Tests;

public class Gtk4SizingTests
{
	[Theory]
	[InlineData(1, false, 4, 7)]
	[InlineData(2, false, 8, 14)]
	[InlineData(2, true, 4, 7)]
	[InlineData(3, false, 12, 21)]
	public void TouchLocationMatchesPaintCoordinates(int scale, bool ignorePixelScaling, int x, int y) =>
		Assert.Equal(new SKPoint(x, y), Gtk4Sizing.GetTouchLocation(4, 7, scale, ignorePixelScaling));

	[Fact]
	public void InvalidDeviceScaleIsRejected()
	{
		Assert.Throws<ArgumentOutOfRangeException>(() => Gtk4Sizing.GetTouchLocation(1, 2, 0, false));
	}

	[Fact]
	public void MauiGpuHandlerUsesNativeGtkView() =>
		Assert.Contains(typeof(SkiaSharp.Views.Gtk.SKGLArea),
			typeof(SKGLViewHandler).BaseType!.GenericTypeArguments);
}
