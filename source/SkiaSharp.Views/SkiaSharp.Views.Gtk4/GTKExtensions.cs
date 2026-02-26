using System;

namespace SkiaSharp.Views.Gtk
{
	public static class GTKExtensions
	{
		// Rectangle

		public static SKRectI ToSKRectI(this Gdk.Rectangle rect)
		{
			return new SKRectI(rect.X, rect.Y, rect.X + rect.Width, rect.Y + rect.Height);
		}

		public static Gdk.Rectangle ToGdkRectangle(this SKRectI rect)
		{
			var gdkRect = new Gdk.Rectangle();
			gdkRect.X = rect.Left;
			gdkRect.Y = rect.Top;
			gdkRect.Width = rect.Width;
			gdkRect.Height = rect.Height;
			return gdkRect;
		}

		// Color (RGBA)

		public static SKColor ToSKColor(this Gdk.RGBA color)
		{
			var r = (byte)(color.Red * 255f);
			var g = (byte)(color.Green * 255f);
			var b = (byte)(color.Blue * 255f);
			var a = (byte)(color.Alpha * 255f);
			return new SKColor(r, g, b, a);
		}

		public static Gdk.RGBA ToGdkRGBA(this SKColor color)
		{
			var rgba = new Gdk.RGBA();
			rgba.Red = color.Red / 255f;
			rgba.Green = color.Green / 255f;
			rgba.Blue = color.Blue / 255f;
			rgba.Alpha = color.Alpha / 255f;
			return rgba;
		}
	}
}
