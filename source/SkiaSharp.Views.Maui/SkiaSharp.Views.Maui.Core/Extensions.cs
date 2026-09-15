using Microsoft.Maui.Graphics;

namespace SkiaSharp.Views.Maui
{
	/// <summary>Provides extension methods for converting between SkiaSharp and .NET MAUI Graphics types.</summary>
	/// <remarks>These extension methods allow easy interoperability between SkiaSharp types (SKColor, SKPoint, SKRect, SKSize) and their MAUI Graphics equivalents.</remarks>
	public static class Extensions
	{
		// Point

		/// <summary>Converts an <see cref="T:SkiaSharp.SKPointI" /> to a MAUI <see cref="T:Microsoft.Maui.Graphics.Point" />.</summary>
		/// <param name="point">The SkiaSharp integer point to convert.</param>
		/// <returns>The equivalent MAUI point.</returns>
		/// <remarks />
		public static Point ToMauiPoint(this SKPointI point) =>
			new Point(point.X, point.Y);

		/// <summary>Converts an <see cref="T:SkiaSharp.SKPointI" /> to a MAUI <see cref="T:Microsoft.Maui.Graphics.PointF" />.</summary>
		/// <param name="point">The SkiaSharp integer point to convert.</param>
		/// <returns>The equivalent MAUI floating-point point.</returns>
		/// <remarks />
		public static PointF ToMauiPointF(this SKPointI point) =>
			new PointF(point.X, point.Y);

		/// <summary>Converts an <see cref="T:SkiaSharp.SKPoint" /> to a MAUI <see cref="T:Microsoft.Maui.Graphics.Point" />.</summary>
		/// <param name="point">The SkiaSharp point to convert.</param>
		/// <returns>The equivalent MAUI point.</returns>
		/// <remarks />
		public static Point ToMauiPoint(this SKPoint point) =>
			new Point(point.X, point.Y);

		/// <summary>Converts an <see cref="T:SkiaSharp.SKPoint" /> to a MAUI <see cref="T:Microsoft.Maui.Graphics.PointF" />.</summary>
		/// <param name="point">The SkiaSharp point to convert.</param>
		/// <returns>The equivalent MAUI floating-point point.</returns>
		/// <remarks />
		public static PointF ToMauiPointF(this SKPoint point) =>
			new PointF(point.X, point.Y);

		/// <summary>Converts a MAUI <see cref="T:Microsoft.Maui.Graphics.Point" /> to an <see cref="T:SkiaSharp.SKPoint" />.</summary>
		/// <param name="point">The MAUI point to convert.</param>
		/// <returns>The equivalent SkiaSharp point.</returns>
		/// <remarks />
		public static SKPoint ToSKPoint(this Point point) =>
			new SKPoint((float)point.X, (float)point.Y);

		/// <summary>Converts a MAUI <see cref="T:Microsoft.Maui.Graphics.PointF" /> to an <see cref="T:SkiaSharp.SKPoint" />.</summary>
		/// <param name="point">The MAUI floating-point point to convert.</param>
		/// <returns>The equivalent SkiaSharp point.</returns>
		/// <remarks />
		public static SKPoint ToSKPoint(this PointF point) =>
			new SKPoint(point.X, point.Y);

		// Size

		/// <summary>Converts an <see cref="T:SkiaSharp.SKSizeI" /> to a MAUI <see cref="T:Microsoft.Maui.Graphics.Size" />.</summary>
		/// <param name="size">The SkiaSharp integer size to convert.</param>
		/// <returns>The equivalent MAUI size.</returns>
		/// <remarks />
		public static Size ToMauiSize(this SKSizeI size) =>
			new Size(size.Width, size.Height);

		/// <summary>Converts an <see cref="T:SkiaSharp.SKSizeI" /> to a MAUI <see cref="T:Microsoft.Maui.Graphics.SizeF" />.</summary>
		/// <param name="size">The SkiaSharp integer size to convert.</param>
		/// <returns>The equivalent MAUI floating-point size.</returns>
		/// <remarks />
		public static SizeF ToMauiSizeF(this SKSizeI size) =>
			new SizeF(size.Width, size.Height);

		/// <summary>Converts an <see cref="T:SkiaSharp.SKSize" /> to a MAUI <see cref="T:Microsoft.Maui.Graphics.Size" />.</summary>
		/// <param name="size">The SkiaSharp size to convert.</param>
		/// <returns>The equivalent MAUI size.</returns>
		/// <remarks />
		public static Size ToMauiSize(this SKSize size) =>
			new Size(size.Width, size.Height);

		/// <summary>Converts an <see cref="T:SkiaSharp.SKSize" /> to a MAUI <see cref="T:Microsoft.Maui.Graphics.SizeF" />.</summary>
		/// <param name="size">The SkiaSharp size to convert.</param>
		/// <returns>The equivalent MAUI floating-point size.</returns>
		/// <remarks />
		public static SizeF ToMauiSizeF(this SKSize size) =>
			new SizeF(size.Width, size.Height);

		/// <summary>Converts a MAUI <see cref="T:Microsoft.Maui.Graphics.Size" /> to an <see cref="T:SkiaSharp.SKSize" />.</summary>
		/// <param name="size">The MAUI size to convert.</param>
		/// <returns>The equivalent SkiaSharp size.</returns>
		/// <remarks />
		public static SKSize ToSKSize(this Size size) =>
			new SKSize((float)size.Width, (float)size.Height);

		/// <summary>Converts a MAUI <see cref="T:Microsoft.Maui.Graphics.SizeF" /> to an <see cref="T:SkiaSharp.SKSize" />.</summary>
		/// <param name="size">The MAUI floating-point size to convert.</param>
		/// <returns>The equivalent SkiaSharp size.</returns>
		/// <remarks />
		public static SKSize ToSKSize(this SizeF size) =>
			new SKSize(size.Width, size.Height);

		// Rect

		/// <summary>Converts an <see cref="T:SkiaSharp.SKRectI" /> to a MAUI <see cref="T:Microsoft.Maui.Graphics.Rect" />.</summary>
		/// <param name="rect">The SkiaSharp integer rectangle to convert.</param>
		/// <returns>The equivalent MAUI rectangle.</returns>
		/// <remarks />
		public static Rect ToMauiRectangle(this SKRectI rect) =>
			new Rect(rect.Left, rect.Top, rect.Width, rect.Height);

		/// <summary>Converts an <see cref="T:SkiaSharp.SKRectI" /> to a MAUI <see cref="T:Microsoft.Maui.Graphics.RectF" />.</summary>
		/// <param name="rect">The SkiaSharp integer rectangle to convert.</param>
		/// <returns>The equivalent MAUI floating-point rectangle.</returns>
		/// <remarks />
		public static RectF ToMauiRectangleF(this SKRectI rect) =>
			new RectF(rect.Left, rect.Top, rect.Width, rect.Height);

		/// <summary>Converts an <see cref="T:SkiaSharp.SKRect" /> to a MAUI <see cref="T:Microsoft.Maui.Graphics.Rect" />.</summary>
		/// <param name="rect">The SkiaSharp rectangle to convert.</param>
		/// <returns>The equivalent MAUI rectangle.</returns>
		/// <remarks />
		public static Rect ToMauiRectangle(this SKRect rect) =>
			new Rect(rect.Left, rect.Top, rect.Width, rect.Height);

		/// <summary>Converts an <see cref="T:SkiaSharp.SKRect" /> to a MAUI <see cref="T:Microsoft.Maui.Graphics.RectF" />.</summary>
		/// <param name="rect">The SkiaSharp rectangle to convert.</param>
		/// <returns>The equivalent MAUI floating-point rectangle.</returns>
		/// <remarks />
		public static RectF ToMauiRectangleF(this SKRect rect) =>
			new RectF(rect.Left, rect.Top, rect.Width, rect.Height);

		/// <summary>Converts a MAUI <see cref="T:Microsoft.Maui.Graphics.Rect" /> to an <see cref="T:SkiaSharp.SKRect" />.</summary>
		/// <param name="rect">The MAUI rectangle to convert.</param>
		/// <returns>The equivalent SkiaSharp rectangle.</returns>
		/// <remarks />
		public static SKRect ToSKRect(this Rect rect) =>
			new SKRect((float)rect.Left, (float)rect.Top, (float)rect.Right, (float)rect.Bottom);

		/// <summary>Converts a MAUI <see cref="T:Microsoft.Maui.Graphics.RectF" /> to an <see cref="T:SkiaSharp.SKRect" />.</summary>
		/// <param name="rect">The MAUI floating-point rectangle to convert.</param>
		/// <returns>The equivalent SkiaSharp rectangle.</returns>
		/// <remarks />
		public static SKRect ToSKRect(this RectF rect) =>
			new SKRect(rect.Left, rect.Top, rect.Right, rect.Bottom);

		// Color

		/// <summary>Converts an <see cref="T:SkiaSharp.SKColor" /> to a MAUI <see cref="T:Microsoft.Maui.Graphics.Color" />.</summary>
		/// <param name="color">The SkiaSharp color to convert.</param>
		/// <returns>The equivalent MAUI color.</returns>
		/// <remarks />
		public static Color ToMauiColor(this SKColor color) =>
			new Color(color.Red / 255.0f, color.Green / 255.0f, color.Blue / 255.0f, color.Alpha / 255.0f);

		/// <summary>Converts an <see cref="T:SkiaSharp.SKColorF" /> to a MAUI <see cref="T:Microsoft.Maui.Graphics.Color" />.</summary>
		/// <param name="color">The SkiaSharp floating-point color to convert.</param>
		/// <returns>The equivalent MAUI color.</returns>
		/// <remarks />
		public static Color ToMauiColor(this SKColorF color) =>
			new Color(color.Red, color.Green, color.Blue, color.Alpha);

		/// <summary>Converts a MAUI <see cref="T:Microsoft.Maui.Graphics.Color" /> to an <see cref="T:SkiaSharp.SKColor" />.</summary>
		/// <param name="color">The MAUI color to convert.</param>
		/// <returns>The equivalent SkiaSharp color.</returns>
		/// <remarks />
		public static SKColor ToSKColor(this Color color) =>
			new SKColor((byte)(color.Red * 255), (byte)(color.Green * 255), (byte)(color.Blue * 255), (byte)(color.Alpha * 255));

		/// <summary>Converts a MAUI <see cref="T:Microsoft.Maui.Graphics.Color" /> to an <see cref="T:SkiaSharp.SKColorF" />.</summary>
		/// <param name="color">The MAUI color to convert.</param>
		/// <returns>The equivalent SkiaSharp floating-point color.</returns>
		/// <remarks />
		public static SKColorF ToSKColorF(this Color color) =>
			new SKColorF(color.Red, color.Green, color.Blue, color.Alpha);
	}
}
