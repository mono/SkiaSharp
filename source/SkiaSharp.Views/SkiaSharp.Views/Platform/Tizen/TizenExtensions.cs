using ElmSharp;

namespace SkiaSharp.Views.Tizen
{
	/// <summary>
	/// Various extension methods to convert between SkiaSharp types and Tizen types.
	/// </summary>
	public static class TizenExtensions
	{
		// Point

		public static SKPoint ToSKPoint(this Point point)
		{
			return new SKPoint(point.X, point.Y);
		}

		public static SKPointI ToSKPointI(this Point point)
		{
			return new SKPointI(point.X, point.Y);
		}

		public static SKPoint ToSKPoint(this global::Tizen.NUI.Position point)
		{
			return new SKPoint(point.X, point.Y);
		}

		public static SKPointI ToSKPointI(this global::Tizen.NUI.Position2D point)
		{
			return new SKPointI(point.X, point.Y);
		}

		/// <summary>
		/// Converts a SkiaSharp point into a Tizen point.
		/// </summary>
		/// <param name="point">The SkiaSharp point.</param>
		/// <returns>Returns a Tizen point.</returns>
		public static Point ToPoint(this SKPoint point)
		{
			return new Point { X = (int)point.X, Y = (int)point.Y };
		}

		/// <summary>
		/// Converts a SkiaSharp point into a Tizen point.
		/// </summary>
		/// <param name="point">The SkiaSharp point.</param>
		/// <returns>Returns a Tizen point.</returns>
		public static Point ToPoint(this SKPointI point)
		{
			return new Point { X = point.X, Y = point.Y };
		}

		// Size

		public static SKSize ToSKSize(this Size size)
		{
			return new SKSize(size.Width, size.Height);
		}

		public static SKSizeI ToSKSizeI(this Size size)
		{
			return new SKSizeI(size.Width, size.Height);
		}

		public static SKSize ToSKSize(this global::Tizen.NUI.Size size)
		{
			return new SKSize(size.Width, size.Height);
		}

		public static SKSizeI ToSKSizeI(this global::Tizen.NUI.Size2D size)
		{
			return new SKSizeI(size.Width, size.Height);
		}

		/// <summary>
		/// Converts a SkiaSharp size into a Tizen size.
		/// </summary>
		/// <param name="size">The SkiaSharp size.</param>
		/// <returns>Returns a Tizen size.</returns>
		public static Size ToSize(this SKSize size)
		{
			return new Size((int)size.Width, (int)size.Height);
		}

		/// <summary>
		/// Converts a SkiaSharp size into a Tizen size.
		/// </summary>
		/// <param name="size">The SkiaSharp size.</param>
		/// <returns>Returns a Tizen size.</returns>
		public static Size ToSize(this SKSizeI size)
		{
			return new Size(size.Width, size.Height);
		}

		// Rectangle

		public static SKRect ToSKRect(this Rect rect)
		{
			return new SKRect(rect.Left, rect.Top, rect.Right, rect.Bottom);
		}

		public static SKRectI ToSKRectI(this Rect rect)
		{
			return new SKRectI(rect.Left, rect.Top, rect.Right, rect.Bottom);
		}

		public static SKRect ToSKRect(this global::Tizen.NUI.Rectangle rect)
		{
			return SKRect.Create(rect.X, rect.Y, rect.Width, rect.Height);
		}

		public static SKRectI ToSKRectI(this global::Tizen.NUI.Rectangle rect)
		{
			return SKRectI.Create(rect.X, rect.Y, rect.Width, rect.Height);
		}

		/// <summary>
		/// Converts a SkiaSharp rectangle into a Tizen rectangle.
		/// </summary>
		/// <param name="rect">The SkiaSharp rectangle.</param>
		/// <returns>Returns a Tizen rectangle.</returns>
		public static Rect ToRect(this SKRect rect)
		{
			return new Rect((int)rect.Left, (int)rect.Top, (int)rect.Right, (int)rect.Bottom);
		}

		/// <summary>
		/// Converts a SkiaSharp rectangle into a Tizen rectangle.
		/// </summary>
		/// <param name="rect">The SkiaSharp rectangle.</param>
		/// <returns>Returns a Tizen rectangle.</returns>
		public static Rect ToRect(this SKRectI rect)
		{
			return new Rect(rect.Left, rect.Top, rect.Right, rect.Bottom);
		}

		// Color

		public static SKColor ToSKColor(this Color color)
		{
			return new SKColor((byte)color.R, (byte)color.G, (byte)color.B, (byte)color.A);
		}

		public static SKColorF ToSKColorF(this global::Tizen.NUI.Color color)
		{
			return new SKColorF(color.R, color.G, color.B, color.A);
		}

		/// <summary>
		/// Converts a SkiaSharp color into a Tizen color.
		/// </summary>
		/// <param name="color">The SkiaSharp color.</param>
		/// <returns>Returns a Tizen color.</returns>
		public static Color ToColor(this SKColor color)
		{
			return Color.FromRgba(color.Red, color.Green, color.Blue, color.Alpha);
		}
	}
}
