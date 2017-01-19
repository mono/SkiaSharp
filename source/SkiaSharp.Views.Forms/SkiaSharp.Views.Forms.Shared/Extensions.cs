using Xamarin.Forms;

namespace SkiaSharp.Views.Forms
{
	public static class Extensions
	{
		public static Point ToFormsPoint(this SKPointI point)
		{
			return new Point(point.X, point.Y);
		}

		public static Point ToFormsPoint(this SKPoint point)
		{
			return new Point(point.X, point.Y);
		}

		public static SKPoint ToSKPoint(this Point point)
		{
			return new SKPoint((float)point.X, (float)point.Y);
		}

		// Xamarin.Forms.Point

		public static Size ToFormsSize(this SKSizeI size)
		{
			return new Size(size.Width, size.Height);
		}

		public static Size ToFormsSize(this SKSize size)
		{
			return new Size(size.Width, size.Height);
		}

		public static SKSize ToSKSize(this Size size)
		{
			return new SKSize((float)size.Width, (float)size.Height);
		}

		// Xamarin.Forms.Size

		public static Rectangle ToFormsRect(this SKRectI rect)
		{
			return new Rectangle(rect.Left, rect.Top, rect.Width, rect.Height);
		}

		public static Rectangle ToFormsRect(this SKRect rect)
		{
			return new Rectangle(rect.Left, rect.Top, rect.Width, rect.Height);
		}

		public static SKRect ToSKRect(this Rectangle rect)
		{
			return new SKRect((float)rect.Left, (float)rect.Top, (float)rect.Right, (float)rect.Bottom);
		}

		// Xamarin.Forms.Color

		public static Color ToFormsColor(this SKColor color)
		{
			return new Color(color.Red / 255.0, color.Green / 255.0, color.Blue / 255.0, color.Alpha / 255.0);
		}

		public static SKColor ToSKColor(this Color color)
		{
			return new SKColor((byte)(color.R * 255), (byte)(color.G * 255), (byte)(color.B * 255), (byte)(color.A * 255));
		}
	}
}
