using System;
using Xunit;
using SkiaSharp;
using SkiaSharp.Views.Gtk;

namespace SkiaSharp.Views.Gtk4.Tests
{
	public class GTKExtensionsTest
	{
		static GTKExtensionsTest()
		{
			GLib.Module.Initialize();
		}

		// Rectangle conversions

		[Fact]
		public void GdkRectangleToSKRectI()
		{
			var gdkRect = new Gdk.Rectangle();
			gdkRect.X = 10;
			gdkRect.Y = 20;
			gdkRect.Width = 30;
			gdkRect.Height = 40;

			var skRect = gdkRect.ToSKRectI();

			Assert.Equal(10, skRect.Left);
			Assert.Equal(20, skRect.Top);
			Assert.Equal(40, skRect.Right);
			Assert.Equal(60, skRect.Bottom);
			Assert.Equal(30, skRect.Width);
			Assert.Equal(40, skRect.Height);
		}

		[Fact]
		public void SKRectIToGdkRectangle()
		{
			var skRect = new SKRectI(10, 20, 40, 60);

			var gdkRect = skRect.ToGdkRectangle();

			Assert.Equal(10, gdkRect.X);
			Assert.Equal(20, gdkRect.Y);
			Assert.Equal(30, gdkRect.Width);
			Assert.Equal(40, gdkRect.Height);
		}

		[Fact]
		public void RectangleRoundTrip()
		{
			var original = new SKRectI(5, 10, 100, 200);

			var gdkRect = original.ToGdkRectangle();
			var roundTripped = gdkRect.ToSKRectI();

			Assert.Equal(original, roundTripped);
		}

		// Color conversions

		[Fact]
		public void GdkRGBAToSKColor()
		{
			var rgba = new Gdk.RGBA();
			rgba.Red = 1.0f;
			rgba.Green = 0.5f;
			rgba.Blue = 0.0f;
			rgba.Alpha = 1.0f;

			var skColor = rgba.ToSKColor();

			Assert.Equal(255, skColor.Red);
			Assert.Equal(127, skColor.Green);
			Assert.Equal(0, skColor.Blue);
			Assert.Equal(255, skColor.Alpha);
		}

		[Fact]
		public void SKColorToGdkRGBA()
		{
			var skColor = new SKColor(255, 128, 0, 255);

			var rgba = skColor.ToGdkRGBA();

			Assert.Equal(1.0f, rgba.Red, 0.01f);
			Assert.InRange(rgba.Green, 0.49f, 0.51f);
			Assert.Equal(0.0f, rgba.Blue, 0.01f);
			Assert.Equal(1.0f, rgba.Alpha, 0.01f);
		}

		[Fact]
		public void TransparentColorConversion()
		{
			var skColor = new SKColor(0, 0, 0, 0);

			var rgba = skColor.ToGdkRGBA();

			Assert.Equal(0.0f, rgba.Red, 0.01f);
			Assert.Equal(0.0f, rgba.Green, 0.01f);
			Assert.Equal(0.0f, rgba.Blue, 0.01f);
			Assert.Equal(0.0f, rgba.Alpha, 0.01f);
		}

		[Fact]
		public void WhiteColorConversion()
		{
			var skColor = SKColors.White;

			var rgba = skColor.ToGdkRGBA();

			Assert.Equal(1.0f, rgba.Red, 0.01f);
			Assert.Equal(1.0f, rgba.Green, 0.01f);
			Assert.Equal(1.0f, rgba.Blue, 0.01f);
			Assert.Equal(1.0f, rgba.Alpha, 0.01f);
		}

		[Fact]
		public void ColorRoundTrip()
		{
			var original = new SKColor(128, 64, 32, 255);

			var rgba = original.ToGdkRGBA();
			var roundTripped = rgba.ToSKColor();

			// Allow for rounding due to float<->byte conversion
			Assert.InRange(roundTripped.Red, (byte)(original.Red - 1), (byte)(original.Red + 1));
			Assert.InRange(roundTripped.Green, (byte)(original.Green - 1), (byte)(original.Green + 1));
			Assert.InRange(roundTripped.Blue, (byte)(original.Blue - 1), (byte)(original.Blue + 1));
			Assert.Equal(original.Alpha, roundTripped.Alpha);
		}

		// Zero/empty rectangle

		[Fact]
		public void ZeroRectangleConversion()
		{
			var gdkRect = new Gdk.Rectangle();

			var skRect = gdkRect.ToSKRectI();

			Assert.Equal(0, skRect.Left);
			Assert.Equal(0, skRect.Top);
			Assert.Equal(0, skRect.Right);
			Assert.Equal(0, skRect.Bottom);
		}
	}
}
