using System;
using SkiaSharp.Views.Maui.Controls.Gtk4;
using Xunit;

namespace SkiaSharp.Views.Maui.Gtk4.Tests;

public class Gtk4SizingTests
{
	[Theory]
	[InlineData(1, false, 120, 80)]
	[InlineData(2, false, 120, 80)]
	[InlineData(2, true, 60, 40)]
	[InlineData(3, true, 40, 26)]
	public void DisplayInfoPreservesPhysicalRawSize(int scale, bool ignorePixelScaling, int width, int height)
	{
		var raw = new SKImageInfo(120, 80, SKColorType.Rgba8888, SKAlphaType.Premul);

		var info = Gtk4Sizing.GetDisplayInfo(raw, scale, ignorePixelScaling);

		Assert.Equal(new SKSizeI(width, height), info.Size);
		Assert.Equal(new SKSizeI(120, 80), raw.Size);
		Assert.Equal(raw.ColorType, info.ColorType);
	}

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
		var raw = new SKImageInfo(10, 10);
		Assert.Throws<ArgumentOutOfRangeException>(() => Gtk4Sizing.GetDisplayInfo(raw, 0, false));
		Assert.Throws<ArgumentOutOfRangeException>(() => Gtk4Sizing.GetTouchLocation(1, 2, 0, false));
	}

	[Fact]
	public void MauiGpuHandlerUsesNativeGtkView() =>
		Assert.Contains(typeof(SkiaSharp.Views.Gtk.SKGLView),
			typeof(SKGLViewHandler).BaseType!.GenericTypeArguments);
}
