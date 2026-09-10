using ElmSharp;

namespace SkiaSharp.Views.Tizen
{
	/// <summary>Various extension methods to convert between SkiaSharp types and Tizen types.</summary>
	/// <remarks />
	public static class TizenExtensions
	{
		// Point

		/// <param name="point">The Tizen point.</param>
		/// <summary>Converts a Tizen point into a SkiaSharp point.</summary>
		/// <returns>Returns a SkiaSharp point.</returns>
		/// <remarks />
		public static SKPoint ToSKPoint(this Point point)
		{
			return new SKPoint(point.X, point.Y);
		}

		/// <param name="point">The Tizen point.</param>
		/// <summary>Converts a Tizen point into a SkiaSharp point.</summary>
		/// <returns>Returns a SkiaSharp point.</returns>
		/// <remarks />
		public static SKPointI ToSKPointI(this Point point)
		{
			return new SKPointI(point.X, point.Y);
		}

		/// <param name="point">The Tizen NUI position.</param>
		/// <summary>Converts a Tizen NUI position into a SkiaSharp point.</summary>
		/// <returns>Returns a SkiaSharp point.</returns>
		/// <remarks />
		public static SKPoint ToSKPoint(this global::Tizen.NUI.Position point)
		{
			return new SKPoint(point.X, point.Y);
		}

		/// <param name="point">The Tizen NUI 2D position.</param>
		/// <summary>Converts a Tizen NUI 2D position into a SkiaSharp point.</summary>
		/// <returns>Returns a SkiaSharp point.</returns>
		/// <remarks />
		public static SKPointI ToSKPointI(this global::Tizen.NUI.Position2D point)
		{
			return new SKPointI(point.X, point.Y);
		}

		/// <param name="point">The SkiaSharp point.</param>
		/// <summary>Converts a SkiaSharp point into a Tizen point.</summary>
		/// <returns>Returns a Tizen point.</returns>
		/// <remarks />
		public static Point ToPoint(this SKPoint point)
		{
			return new Point { X = (int)point.X, Y = (int)point.Y };
		}

		/// <param name="point">The SkiaSharp point.</param>
		/// <summary>Converts a SkiaSharp point into a Tizen point.</summary>
		/// <returns>Returns a Tizen point.</returns>
		/// <remarks />
		public static Point ToPoint(this SKPointI point)
		{
			return new Point { X = point.X, Y = point.Y };
		}

		// Size

		/// <param name="size">The Tizen NUI size.</param>
		/// <summary>Converts a Tizen NUI size into a SkiaSharp size.</summary>
		/// <returns>Returns a SkiaSharp size.</returns>
		/// <remarks />
		public static SKSize ToSKSize(this Size size)
		{
			return new SKSize(size.Width, size.Height);
		}

		/// <param name="size">The Tizen size.</param>
		/// <summary>Converts a Tizen size into a SkiaSharp size.</summary>
		/// <returns>Returns a SkiaSharp size.</returns>
		/// <remarks />
		public static SKSizeI ToSKSizeI(this Size size)
		{
			return new SKSizeI(size.Width, size.Height);
		}

		/// <param name="size">The Tizen NUI size.</param>
		/// <summary>Converts a Tizen NUI size into a SkiaSharp size.</summary>
		/// <returns>Returns a SkiaSharp size.</returns>
		/// <remarks />
		public static SKSize ToSKSize(this global::Tizen.NUI.Size size)
		{
			return new SKSize(size.Width, size.Height);
		}

		/// <param name="size">The Tizen NUI 2D size.</param>
		/// <summary>Converts a Tizen NUI 2D size into a SkiaSharp size.</summary>
		/// <returns>Returns a SkiaSharp size.</returns>
		/// <remarks />
		public static SKSizeI ToSKSizeI(this global::Tizen.NUI.Size2D size)
		{
			return new SKSizeI(size.Width, size.Height);
		}

		/// <param name="size">The SkiaSharp size.</param>
		/// <summary>Converts a SkiaSharp size into a Tizen size.</summary>
		/// <returns>Returns a Tizen size.</returns>
		/// <remarks />
		public static Size ToSize(this SKSize size)
		{
			return new Size((int)size.Width, (int)size.Height);
		}

		/// <param name="size">The SkiaSharp size.</param>
		/// <summary>Converts a SkiaSharp size into a Tizen size.</summary>
		/// <returns>Returns a Tizen size.</returns>
		/// <remarks />
		public static Size ToSize(this SKSizeI size)
		{
			return new Size(size.Width, size.Height);
		}

		// Rectangle

		/// <param name="rect">The Tizen rectangle.</param>
		/// <summary>Converts a Tizen rectangle into a SkiaSharp rectangle.</summary>
		/// <returns>Returns a SkiaSharp rectangle.</returns>
		/// <remarks />
		public static SKRect ToSKRect(this Rect rect)
		{
			return new SKRect(rect.Left, rect.Top, rect.Right, rect.Bottom);
		}

		/// <param name="rect">The Tizen rectangle.</param>
		/// <summary>Converts a Tizen rectangle into a SkiaSharp rectangle.</summary>
		/// <returns>Returns a SkiaSharp rectangle.</returns>
		/// <remarks />
		public static SKRectI ToSKRectI(this Rect rect)
		{
			return new SKRectI(rect.Left, rect.Top, rect.Right, rect.Bottom);
		}

		/// <param name="rect">The Tizen NUI rectangle.</param>
		/// <summary>Converts a Tizen NUI rectangle into a SkiaSharp rectangle.</summary>
		/// <returns>Returns a SkiaSharp rectangle.</returns>
		/// <remarks />
		public static SKRect ToSKRect(this global::Tizen.NUI.Rectangle rect)
		{
			return SKRect.Create(rect.X, rect.Y, rect.Width, rect.Height);
		}

		/// <param name="rect">The Tizen NUI rectangle.</param>
		/// <summary>Converts a Tizen NUI rectangle into a SkiaSharp rectangle.</summary>
		/// <returns>Returns a SkiaSharp rectangle.</returns>
		/// <remarks />
		public static SKRectI ToSKRectI(this global::Tizen.NUI.Rectangle rect)
		{
			return SKRectI.Create(rect.X, rect.Y, rect.Width, rect.Height);
		}

		/// <param name="rect">The SkiaSharp rectangle.</param>
		/// <summary>Converts a SkiaSharp rectangle into a Tizen rectangle.</summary>
		/// <returns>Returns a Tizen rectangle.</returns>
		/// <remarks />
		public static Rect ToRect(this SKRect rect)
		{
			return new Rect((int)rect.Left, (int)rect.Top, (int)rect.Right, (int)rect.Bottom);
		}

		/// <param name="rect">The SkiaSharp rectangle.</param>
		/// <summary>Converts a SkiaSharp rectangle into a Tizen rectangle.</summary>
		/// <returns>Returns a Tizen rectangle.</returns>
		/// <remarks />
		public static Rect ToRect(this SKRectI rect)
		{
			return new Rect(rect.Left, rect.Top, rect.Right, rect.Bottom);
		}

		// Color

		/// <param name="color">The Tizen color.</param>
		/// <summary>Converts a Tizen color into a SkiaSharp color.</summary>
		/// <returns>Returns a SkiaSharp color.</returns>
		/// <remarks />
		public static SKColor ToSKColor(this Color color)
		{
			return new SKColor((byte)color.R, (byte)color.G, (byte)color.B, (byte)color.A);
		}

		/// <param name="color">The Tizen NUI color.</param>
		/// <summary>Converts a Tizen NUI color into a SkiaSharp color.</summary>
		/// <returns>Returns a SkiaSharp color.</returns>
		/// <remarks />
		public static SKColorF ToSKColorF(this global::Tizen.NUI.Color color)
		{
			return new SKColorF(color.R, color.G, color.B, color.A);
		}

		/// <param name="color">The SkiaSharp color.</param>
		/// <summary>Converts a SkiaSharp color into a Tizen color.</summary>
		/// <returns>Returns a Tizen color.</returns>
		/// <remarks />
		public static Color ToColor(this SKColor color)
		{
			return Color.FromRgba(color.Red, color.Green, color.Blue, color.Alpha);
		}
	}
}
