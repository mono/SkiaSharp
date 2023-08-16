using Microsoft.Maui.Graphics;

namespace SkiaSharp.Views.Maui
{
	public static class Extensions
	{
		// Point

		public static Point ToMauiPoint(this SKPointI point) =>
			new Point(point.X, point.Y);

		public static PointF ToMauiPointF(this SKPointI point) =>
			new PointF(point.X, point.Y);

		public static Point ToMauiPoint(this SKPoint point) =>
			new Point(point.X, point.Y);

		public static PointF ToMauiPointF(this SKPoint point) =>
			new PointF(point.X, point.Y);

		public static SKPoint ToSKPoint(this Point point) =>
			new SKPoint((float)point.X, (float)point.Y);

		public static SKPoint ToSKPoint(this PointF point) =>
			new SKPoint(point.X, point.Y);

		// Size

		public static Size ToMauiSize(this SKSizeI size) =>
			new Size(size.Width, size.Height);

		public static SizeF ToMauiSizeF(this SKSizeI size) =>
			new SizeF(size.Width, size.Height);

		public static Size ToMauiSize(this SKSize size) =>
			new Size(size.Width, size.Height);

		public static SizeF ToMauiSizeF(this SKSize size) =>
			new SizeF(size.Width, size.Height);

		public static SKSize ToSKSize(this Size size) =>
			new SKSize((float)size.Width, (float)size.Height);

		public static SKSize ToSKSize(this SizeF size) =>
			new SKSize(size.Width, size.Height);

		// Rect

		public static Rect ToMauiRectangle(this SKRectI rect) =>
			new Rect(rect.Left, rect.Top, rect.Width, rect.Height);

		public static RectF ToMauiRectangleF(this SKRectI rect) =>
			new RectF(rect.Left, rect.Top, rect.Width, rect.Height);

		public static Rect ToMauiRectangle(this SKRect rect) =>
			new Rect(rect.Left, rect.Top, rect.Width, rect.Height);

		public static RectF ToMauiRectangleF(this SKRect rect) =>
			new RectF(rect.Left, rect.Top, rect.Width, rect.Height);

		public static SKRect ToSKRect(this Rect rect) =>
			new SKRect((float)rect.Left, (float)rect.Top, (float)rect.Right, (float)rect.Bottom);

		public static SKRect ToSKRect(this RectF rect) =>
			new SKRect(rect.Left, rect.Top, rect.Right, rect.Bottom);

		// Color

		public static Color ToMauiColor(this SKColor color) =>
			new Color(color.Red / 255.0f, color.Green / 255.0f, color.Blue / 255.0f, color.Alpha / 255.0f);

		public static Color ToMauiColor(this SKColorF color) =>
			new Color(color.Red, color.Green, color.Blue, color.Alpha);

		public static SKColor ToSKColor(this Color color) =>
			new SKColor((byte)(color.Red * 255), (byte)(color.Green * 255), (byte)(color.Blue * 255), (byte)(color.Alpha * 255));

		public static SKColorF ToSKColorF(this Color color) =>
			new SKColorF(color.Red, color.Green, color.Blue, color.Alpha);
	}
}
