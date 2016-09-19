using Android.Graphics;

namespace SkiaSharp.Views
{
	public static class AndroidExtensions
	{
		// Point*

		public static SKPoint ToSKPoint(this PointF point)
		{
			return new SKPoint(point.X, point.Y);
		}

		public static SKPointI ToSKPoint(this Point point)
		{
			return new SKPointI(point.X, point.Y);
		}

		public static PointF ToPoint(this SKPoint point)
		{
			return new PointF(point.X, point.Y);
		}

		public static Point ToPoint(this SKPointI point)
		{
			return new Point(point.X, point.Y);
		}

		// Rectangle*

		public static SKRect ToSKRect(this RectF rect)
		{
			return new SKRect(rect.Left, rect.Top, rect.Right, rect.Bottom);
		}

		public static SKRectI ToSKRect(this Rect rect)
		{
			return new SKRectI(rect.Left, rect.Top, rect.Right, rect.Bottom);
		}

		public static RectF ToRect(this SKRect rect)
		{
			return new RectF(rect.Left, rect.Top, rect.Right, rect.Bottom);
		}

		public static Rect ToRect(this SKRectI rect)
		{
			return new Rect(rect.Left, rect.Top, rect.Right, rect.Bottom);
		}

		// Color

		public static SKColor ToSKColor(this Color color)
		{
			return (SKColor)(uint)(int)color;
		}

		public static Color ToColor(this SKColor color)
		{
			return new Color((int)(uint)color);
		}

		// Matrix

		public static SKMatrix ToSKMatrix(this Matrix matrix)
		{
			var values = new float[9];
			matrix.GetValues(values);
			return new SKMatrix { Values = values };
		}

		public static Matrix ToMatrix(this SKMatrix matrix)
		{
			var native = new Matrix();
			native.SetValues(matrix.Values);
			return native;
		}
	}
}
