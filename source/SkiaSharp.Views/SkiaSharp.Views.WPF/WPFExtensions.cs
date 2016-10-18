using System.Windows;
using System.Windows.Media;

namespace SkiaSharp.Views.WPF
{
	public static class WPFExtensions
	{
		// Point

		public static SKPoint ToSKPoint(this Point point)
		{
			return new SKPoint((float)point.X, (float)point.Y);
		}

		public static Point ToPoint(this SKPoint point)
		{
			return new Point(point.X, point.Y);
		}

		// Rect

		public static SKRect ToSKRect(this Rect rect)
		{
			return new SKRect((float)rect.Left, (float)rect.Top, (float)rect.Right, (float)rect.Bottom);
		}

		public static Rect ToRect(this SKRect rect)
		{
			return new Rect(rect.Left, rect.Top, rect.Right, rect.Bottom);
		}

		// Size

		public static SKSize ToSKSize(this Size size)
		{
			return new SKSize((float)size.Width, (float)size.Height);
		}

		public static Size ToSize(this SKSize size)
		{
			return new Size(size.Width, size.Height);
		}

		// Color

		public static SKColor ToSKColor(this Color color)
		{
			return new SKColor(color.R, color.G, color.B, color.A);
		}

		public static Color ToColor(this SKColor color)
		{
			return Color.FromArgb(color.Alpha, color.Red, color.Green, color.Blue);
		}
	}
}
