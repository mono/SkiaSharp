using System;
using Xunit;
using SkiaSharp;
using SkiaSharp.Views.Gtk;

namespace SkiaSharp.Views.Gtk4.Tests
{
	public class GTKExtensionsTest
	{
		public GTKExtensionsTest()
		{
			try
			{
				Graphene.Point.Zero();
			}
			catch (Exception ex) when (ex is DllNotFoundException or TypeInitializationException)
			{
				throw new SkipException($"Native GTK4 libraries not available: {ex.Message}");
			}
		}

		// Point conversions (Graphene)

		[SkippableFact]
		public void GraphenePointToSKPoint()
		{
			using var gp = Graphene.Point.Alloc();
			gp.Init(1.5f, 2.5f);

			var skPoint = gp.ToSKPoint();

			Assert.Equal(1.5f, skPoint.X);
			Assert.Equal(2.5f, skPoint.Y);
		}

		[SkippableFact]
		public void SKPointToGraphenePoint()
		{
			var skPoint = new SKPoint(3.5f, 4.5f);

			using var gp = skPoint.ToGraphenePoint();

			Assert.Equal(3.5f, gp.X);
			Assert.Equal(4.5f, gp.Y);
		}

		[SkippableFact]
		public void PointRoundTrip()
		{
			var original = new SKPoint(10.25f, 20.75f);

			using var gp = original.ToGraphenePoint();
			var roundTripped = gp.ToSKPoint();

			Assert.Equal(original.X, roundTripped.X);
			Assert.Equal(original.Y, roundTripped.Y);
		}

		// Size conversions (Graphene)

		[SkippableFact]
		public void GrapheneSizeToSKSize()
		{
			using var gs = Graphene.Size.Alloc();
			gs.Init(100.5f, 200.5f);

			var skSize = gs.ToSKSize();

			Assert.Equal(100.5f, skSize.Width);
			Assert.Equal(200.5f, skSize.Height);
		}

		[SkippableFact]
		public void SKSizeToGrapheneSize()
		{
			var skSize = new SKSize(50.25f, 75.75f);

			using var gs = skSize.ToGrapheneSize();

			Assert.Equal(50.25f, gs.Width);
			Assert.Equal(75.75f, gs.Height);
		}

		[SkippableFact]
		public void SizeRoundTrip()
		{
			var original = new SKSize(320.5f, 240.5f);

			using var gs = original.ToGrapheneSize();
			var roundTripped = gs.ToSKSize();

			Assert.Equal(original.Width, roundTripped.Width);
			Assert.Equal(original.Height, roundTripped.Height);
		}

		// Rect conversions (Graphene)

		[SkippableFact]
		public void GrapheneRectToSKRect()
		{
			using var gr = Graphene.Rect.Alloc();
			gr.Init(10f, 20f, 30f, 40f);

			var skRect = gr.ToSKRect();

			Assert.Equal(10f, skRect.Left);
			Assert.Equal(20f, skRect.Top);
			Assert.Equal(40f, skRect.Right);
			Assert.Equal(60f, skRect.Bottom);
			Assert.Equal(30f, skRect.Width);
			Assert.Equal(40f, skRect.Height);
		}

		[SkippableFact]
		public void SKRectToGrapheneRect()
		{
			var skRect = new SKRect(10f, 20f, 40f, 60f);

			using var gr = skRect.ToGrapheneRect();

			Assert.Equal(10f, gr.GetX());
			Assert.Equal(20f, gr.GetY());
			Assert.Equal(30f, gr.GetWidth());
			Assert.Equal(40f, gr.GetHeight());
		}

		[SkippableFact]
		public void RectRoundTrip()
		{
			var original = new SKRect(5f, 10f, 100f, 200f);

			using var gr = original.ToGrapheneRect();
			var roundTripped = gr.ToSKRect();

			Assert.Equal(original.Left, roundTripped.Left);
			Assert.Equal(original.Top, roundTripped.Top);
			Assert.Equal(original.Right, roundTripped.Right);
			Assert.Equal(original.Bottom, roundTripped.Bottom);
		}

		// Rectangle conversions (Gdk)

		[SkippableFact]
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

		[SkippableFact]
		public void SKRectIToGdkRectangle()
		{
			var skRect = new SKRectI(10, 20, 40, 60);

			var gdkRect = skRect.ToGdkRectangle();

			Assert.Equal(10, gdkRect.X);
			Assert.Equal(20, gdkRect.Y);
			Assert.Equal(30, gdkRect.Width);
			Assert.Equal(40, gdkRect.Height);
		}

		[SkippableFact]
		public void RectangleRoundTrip()
		{
			var original = new SKRectI(5, 10, 100, 200);

			var gdkRect = original.ToGdkRectangle();
			var roundTripped = gdkRect.ToSKRectI();

			Assert.Equal(original, roundTripped);
		}

		// Color conversions (Gdk.RGBA)

		[SkippableFact]
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

		[SkippableFact]
		public void SKColorToGdkRGBA()
		{
			var skColor = new SKColor(255, 128, 0, 255);

			var rgba = skColor.ToGdkRGBA();

			Assert.Equal(1.0f, rgba.Red, 0.01f);
			Assert.InRange(rgba.Green, 0.49f, 0.51f);
			Assert.Equal(0.0f, rgba.Blue, 0.01f);
			Assert.Equal(1.0f, rgba.Alpha, 0.01f);
		}

		[SkippableFact]
		public void TransparentColorConversion()
		{
			var skColor = new SKColor(0, 0, 0, 0);

			var rgba = skColor.ToGdkRGBA();

			Assert.Equal(0.0f, rgba.Red, 0.01f);
			Assert.Equal(0.0f, rgba.Green, 0.01f);
			Assert.Equal(0.0f, rgba.Blue, 0.01f);
			Assert.Equal(0.0f, rgba.Alpha, 0.01f);
		}

		[SkippableFact]
		public void WhiteColorConversion()
		{
			var skColor = SKColors.White;

			var rgba = skColor.ToGdkRGBA();

			Assert.Equal(1.0f, rgba.Red, 0.01f);
			Assert.Equal(1.0f, rgba.Green, 0.01f);
			Assert.Equal(1.0f, rgba.Blue, 0.01f);
			Assert.Equal(1.0f, rgba.Alpha, 0.01f);
		}

		[SkippableFact]
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

		// Zero/empty values

		[SkippableFact]
		public void ZeroRectangleConversion()
		{
			var gdkRect = new Gdk.Rectangle();

			var skRect = gdkRect.ToSKRectI();

			Assert.Equal(0, skRect.Left);
			Assert.Equal(0, skRect.Top);
			Assert.Equal(0, skRect.Right);
			Assert.Equal(0, skRect.Bottom);
		}

		[SkippableFact]
		public void ZeroPointConversion()
		{
			var gp = Graphene.Point.Zero();

			var skPoint = gp.ToSKPoint();

			Assert.Equal(0f, skPoint.X);
			Assert.Equal(0f, skPoint.Y);
		}

		[SkippableFact]
		public void ZeroSizeConversion()
		{
			var gs = Graphene.Size.Zero();

			var skSize = gs.ToSKSize();

			Assert.Equal(0f, skSize.Width);
			Assert.Equal(0f, skSize.Height);
		}

		// Point3D conversions (Graphene)

		[SkippableFact]
		public void GraphenePoint3DToSKPoint3()
		{
			using var gp = Graphene.Point3D.Alloc();
			gp.Init(1.5f, 2.5f, 3.5f);

			var skPoint3 = gp.ToSKPoint3();

			Assert.Equal(1.5f, skPoint3.X);
			Assert.Equal(2.5f, skPoint3.Y);
			Assert.Equal(3.5f, skPoint3.Z);
		}

		[SkippableFact]
		public void SKPoint3ToGraphenePoint3D()
		{
			var skPoint3 = new SKPoint3(4.5f, 5.5f, 6.5f);

			using var gp = skPoint3.ToGraphenePoint3D();

			Assert.Equal(4.5f, gp.X);
			Assert.Equal(5.5f, gp.Y);
			Assert.Equal(6.5f, gp.Z);
		}

		[SkippableFact]
		public void Point3DRoundTrip()
		{
			var original = new SKPoint3(10.25f, 20.75f, 30.5f);

			using var gp = original.ToGraphenePoint3D();
			var roundTripped = gp.ToSKPoint3();

			Assert.Equal(original.X, roundTripped.X);
			Assert.Equal(original.Y, roundTripped.Y);
			Assert.Equal(original.Z, roundTripped.Z);
		}

		// SKColorF conversions (Gdk.RGBA)

		[SkippableFact]
		public void GdkRGBAToSKColorF()
		{
			var rgba = new Gdk.RGBA();
			rgba.Red = 1.0f;
			rgba.Green = 0.5f;
			rgba.Blue = 0.25f;
			rgba.Alpha = 0.75f;

			var skColorF = rgba.ToSKColorF();

			Assert.Equal(1.0f, skColorF.Red);
			Assert.Equal(0.5f, skColorF.Green);
			Assert.Equal(0.25f, skColorF.Blue);
			Assert.Equal(0.75f, skColorF.Alpha);
		}

		[SkippableFact]
		public void SKColorFToGdkRGBA()
		{
			var skColorF = new SKColorF(0.2f, 0.4f, 0.6f, 0.8f);

			var rgba = skColorF.ToGdkRGBA();

			Assert.Equal(0.2f, rgba.Red);
			Assert.Equal(0.4f, rgba.Green);
			Assert.Equal(0.6f, rgba.Blue);
			Assert.Equal(0.8f, rgba.Alpha);
		}

		[SkippableFact]
		public void ColorFRoundTrip()
		{
			var original = new SKColorF(0.1f, 0.2f, 0.3f, 0.4f);

			var rgba = original.ToGdkRGBA();
			var roundTripped = rgba.ToSKColorF();

			Assert.Equal(original.Red, roundTripped.Red);
			Assert.Equal(original.Green, roundTripped.Green);
			Assert.Equal(original.Blue, roundTripped.Blue);
			Assert.Equal(original.Alpha, roundTripped.Alpha);
		}
	}
}
