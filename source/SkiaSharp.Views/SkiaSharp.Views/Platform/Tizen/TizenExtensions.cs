using ElmSharp;

namespace SkiaSharp.Views.Tizen
{
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

		public static Point ToPoint(this SKPoint point)
		{
			return new Point { X = (int)point.X, Y = (int)point.Y };
		}

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

		public static Size ToSize(this SKSize size)
		{
			return new Size((int)size.Width, (int)size.Height);
		}

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

		public static Rect ToRect(this SKRect rect)
		{
			return new Rect((int)rect.Left, (int)rect.Top, (int)rect.Right, (int)rect.Bottom);
		}

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

		public static Color ToColor(this SKColor color)
		{
			return Color.FromRgba(color.Red, color.Green, color.Blue, color.Alpha);
		}
	}
}
