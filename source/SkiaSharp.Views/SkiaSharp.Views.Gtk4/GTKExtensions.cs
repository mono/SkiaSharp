namespace SkiaSharp.Views.Gtk
{
	/// <summary>Provides extension methods for converting between SkiaSharp types and GTK types.</summary>
	/// <remarks />
	public static class GTKExtensions
	{
		// Point (Graphene)

		/// <param name="point">The <see cref="T:Graphene.Point" /> to convert.</param>
		/// <summary>Converts a <see cref="T:Graphene.Point" /> to an <see cref="T:SkiaSharp.SKPoint" />.</summary>
		/// <returns>An <see cref="T:SkiaSharp.SKPoint" /> with the same X and Y coordinates.</returns>
		/// <remarks></remarks>
		public static SKPoint ToSKPoint(this Graphene.Point point)
		{
			return new SKPoint(point.X, point.Y);
		}

		/// <param name="point">The <see cref="T:SkiaSharp.SKPoint" /> to convert.</param>
		/// <summary>Converts an <see cref="T:SkiaSharp.SKPoint" /> to a <see cref="T:Graphene.Point" />.</summary>
		/// <returns>A <see cref="T:Graphene.Point" /> with the same X and Y coordinates.</returns>
		/// <remarks></remarks>
		public static Graphene.Point ToGraphenePoint(this SKPoint point)
		{
			return Graphene.Point.Alloc().Init(point.X, point.Y);
		}

		// Size (Graphene)

		/// <param name="size">The <see cref="T:Graphene.Size" /> to convert.</param>
		/// <summary>Converts a <see cref="T:Graphene.Size" /> to an <see cref="T:SkiaSharp.SKSize" />.</summary>
		/// <returns>An <see cref="T:SkiaSharp.SKSize" /> with the same width and height.</returns>
		/// <remarks></remarks>
		public static SKSize ToSKSize(this Graphene.Size size)
		{
			return new SKSize(size.Width, size.Height);
		}

		/// <param name="size">The <see cref="T:SkiaSharp.SKSize" /> to convert.</param>
		/// <summary>Converts an <see cref="T:SkiaSharp.SKSize" /> to a <see cref="T:Graphene.Size" />.</summary>
		/// <returns>A <see cref="T:Graphene.Size" /> with the same width and height.</returns>
		/// <remarks></remarks>
		public static Graphene.Size ToGrapheneSize(this SKSize size)
		{
			return Graphene.Size.Alloc().Init(size.Width, size.Height);
		}

		// Rect (Graphene)

		/// <param name="rect">The <see cref="T:Graphene.Rect" /> to convert.</param>
		/// <summary>Converts a <see cref="T:Graphene.Rect" /> to an <see cref="T:SkiaSharp.SKRect" />.</summary>
		/// <returns>An <see cref="T:SkiaSharp.SKRect" /> with the same bounds.</returns>
		/// <remarks></remarks>
		public static SKRect ToSKRect(this Graphene.Rect rect)
		{
			return new SKRect(rect.GetX(), rect.GetY(), rect.GetX() + rect.GetWidth(), rect.GetY() + rect.GetHeight());
		}

		/// <param name="rect">The <see cref="T:SkiaSharp.SKRect" /> to convert.</param>
		/// <summary>Converts an <see cref="T:SkiaSharp.SKRect" /> to a <see cref="T:Graphene.Rect" />.</summary>
		/// <returns>A <see cref="T:Graphene.Rect" /> with the same bounds.</returns>
		/// <remarks></remarks>
		public static Graphene.Rect ToGrapheneRect(this SKRect rect)
		{
			return Graphene.Rect.Alloc().Init(rect.Left, rect.Top, rect.Width, rect.Height);
		}

		// Rectangle (Gdk)

		/// <param name="rect">The GDK rectangle to convert.</param>
		/// <summary>Converts a <see cref="T:Gdk.Rectangle" /> to an <see cref="T:SkiaSharp.SKRectI" />.</summary>
		/// <returns>The converted SkiaSharp rectangle.</returns>
		/// <remarks />
		public static SKRectI ToSKRectI(this Gdk.Rectangle rect)
		{
			return new SKRectI(rect.X, rect.Y, rect.X + rect.Width, rect.Y + rect.Height);
		}

		/// <param name="rect">The <see cref="T:SkiaSharp.SKRectI" /> to convert.</param>
		/// <summary>Converts an <see cref="T:SkiaSharp.SKRectI" /> to a <see cref="T:Gdk.Rectangle" />.</summary>
		/// <returns>A <see cref="T:Gdk.Rectangle" /> with the same integer bounds.</returns>
		/// <remarks></remarks>
		public static Gdk.Rectangle ToGdkRectangle(this SKRectI rect)
		{
			var gdkRect = new Gdk.Rectangle();
			gdkRect.X = rect.Left;
			gdkRect.Y = rect.Top;
			gdkRect.Width = rect.Width;
			gdkRect.Height = rect.Height;
			return gdkRect;
		}

		// Point3D (Graphene)

		/// <param name="point">The <see cref="T:Graphene.Point3D" /> to convert.</param>
		/// <summary>Converts a <see cref="T:Graphene.Point3D" /> to an <see cref="T:SkiaSharp.SKPoint3" />.</summary>
		/// <returns>An <see cref="T:SkiaSharp.SKPoint3" /> with the same X, Y, and Z coordinates.</returns>
		/// <remarks></remarks>
		public static SKPoint3 ToSKPoint3(this Graphene.Point3D point)
		{
			return new SKPoint3(point.X, point.Y, point.Z);
		}

		/// <param name="point">The <see cref="T:SkiaSharp.SKPoint3" /> to convert.</param>
		/// <summary>Converts an <see cref="T:SkiaSharp.SKPoint3" /> to a <see cref="T:Graphene.Point3D" />.</summary>
		/// <returns>A <see cref="T:Graphene.Point3D" /> with the same X, Y, and Z coordinates.</returns>
		/// <remarks></remarks>
		public static Graphene.Point3D ToGraphenePoint3D(this SKPoint3 point)
		{
			return Graphene.Point3D.Alloc().Init(point.X, point.Y, point.Z);
		}

		// Color (RGBA → SKColor)

		/// <param name="color">The <see cref="T:Gdk.RGBA" /> to convert.</param>
		/// <summary>Converts a <see cref="T:Gdk.RGBA" /> to an <see cref="T:SkiaSharp.SKColor" />.</summary>
		/// <returns>An <see cref="T:SkiaSharp.SKColor" /> representing the same color.</returns>
		/// <remarks></remarks>
		public static SKColor ToSKColor(this Gdk.RGBA color)
		{
			var r = (byte)(color.Red * 255f);
			var g = (byte)(color.Green * 255f);
			var b = (byte)(color.Blue * 255f);
			var a = (byte)(color.Alpha * 255f);
			return new SKColor(r, g, b, a);
		}

		/// <param name="color">The <see cref="T:SkiaSharp.SKColor" /> to convert.</param>
		/// <summary>Converts an <see cref="T:SkiaSharp.SKColor" /> to a <see cref="T:Gdk.RGBA" />.</summary>
		/// <returns>A <see cref="T:Gdk.RGBA" /> representing the same color with each channel normalized to the range [0, 1].</returns>
		/// <remarks></remarks>
		public static Gdk.RGBA ToGdkRGBA(this SKColor color)
		{
			var rgba = new Gdk.RGBA();
			rgba.Red = color.Red / 255f;
			rgba.Green = color.Green / 255f;
			rgba.Blue = color.Blue / 255f;
			rgba.Alpha = color.Alpha / 255f;
			return rgba;
		}

		// ColorF (RGBA → SKColorF)

		/// <param name="color">The <see cref="T:Gdk.RGBA" /> to convert.</param>
		/// <summary>Converts a <see cref="T:Gdk.RGBA" /> to an <see cref="T:SkiaSharp.SKColorF" />.</summary>
		/// <returns>An <see cref="T:SkiaSharp.SKColorF" /> with the same floating-point channel values.</returns>
		/// <remarks></remarks>
		public static SKColorF ToSKColorF(this Gdk.RGBA color)
		{
			return new SKColorF(color.Red, color.Green, color.Blue, color.Alpha);
		}

		/// <param name="color">The <see cref="T:SkiaSharp.SKColorF" /> to convert.</param>
		/// <summary>Converts an <see cref="T:SkiaSharp.SKColorF" /> to a <see cref="T:Gdk.RGBA" />.</summary>
		/// <returns>A <see cref="T:Gdk.RGBA" /> with the same floating-point channel values.</returns>
		/// <remarks></remarks>
		public static Gdk.RGBA ToGdkRGBA(this SKColorF color)
		{
			var rgba = new Gdk.RGBA();
			rgba.Red = color.Red;
			rgba.Green = color.Green;
			rgba.Blue = color.Blue;
			rgba.Alpha = color.Alpha;
			return rgba;
		}
	}
}
