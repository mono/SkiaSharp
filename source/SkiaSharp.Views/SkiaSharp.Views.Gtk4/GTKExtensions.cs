using System;

namespace SkiaSharp.Views.Gtk
{
	public static class GTKExtensions
	{
		// Point (Graphene)

		public static SKPoint ToSKPoint(this Graphene.Point point)
		{
			return new SKPoint(point.X, point.Y);
		}

		public static Graphene.Point ToGraphenePoint(this SKPoint point)
		{
			var gp = Graphene.Point.Alloc();
			gp = gp.Init(point.X, point.Y);
			return gp;
		}

		// Size (Graphene)

		public static SKSize ToSKSize(this Graphene.Size size)
		{
			return new SKSize(size.Width, size.Height);
		}

		public static Graphene.Size ToGrapheneSize(this SKSize size)
		{
			var gs = Graphene.Size.Alloc();
			gs = gs.Init(size.Width, size.Height);
			return gs;
		}

		// Rect (Graphene)

		public static SKRect ToSKRect(this Graphene.Rect rect)
		{
			return new SKRect(rect.GetX(), rect.GetY(), rect.GetX() + rect.GetWidth(), rect.GetY() + rect.GetHeight());
		}

		public static Graphene.Rect ToGrapheneRect(this SKRect rect)
		{
			var gr = Graphene.Rect.Alloc();
			gr = gr.Init(rect.Left, rect.Top, rect.Width, rect.Height);
			return gr;
		}

		// Rectangle (Gdk)

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
