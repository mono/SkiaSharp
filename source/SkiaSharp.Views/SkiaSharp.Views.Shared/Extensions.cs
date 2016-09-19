namespace SkiaSharp.Views
{
	public static class Extensions
	{
#if !WINDOWS_UWP
		// System.Drawing.Point*

		public static SKPoint ToSKPoint(this System.Drawing.PointF point)
		{
			return new SKPoint(point.X, point.Y);
		}

		public static SKPointI ToSKPoint(this System.Drawing.Point point)
		{
			return new SKPointI(point.X, point.Y);
		}

		public static System.Drawing.PointF ToDrawingPoint(this SKPoint point)
		{
			return new System.Drawing.PointF(point.X, point.Y);
		}

		public static System.Drawing.Point ToDrawingPoint(this SKPointI point)
		{
			return new System.Drawing.Point(point.X, point.Y);
		}

		// System.Drawing.Rectangle*

		public static SKRect ToSKRect(this System.Drawing.RectangleF rect)
		{
			return new SKRect(rect.Left, rect.Top, rect.Right, rect.Bottom);
		}

		public static SKRectI ToSKRect(this System.Drawing.Rectangle rect)
		{
			return new SKRectI(rect.Left, rect.Top, rect.Right, rect.Bottom);
		}

		public static System.Drawing.RectangleF ToDrawingRect(this SKRect rect)
		{
			return System.Drawing.RectangleF.FromLTRB(rect.Left, rect.Top, rect.Right, rect.Bottom);
		}

		public static System.Drawing.Rectangle ToDrawingRect(this SKRectI rect)
		{
			return System.Drawing.Rectangle.FromLTRB(rect.Left, rect.Top, rect.Right, rect.Bottom);
		}

		// System.Drawing.Size*

		public static SKSize ToSKSize(this System.Drawing.SizeF size)
		{
			return new SKSize(size.Width, size.Height);
		}

		public static SKSizeI ToSKSize(this System.Drawing.Size size)
		{
			return new SKSizeI(size.Width, size.Height);
		}

		public static System.Drawing.SizeF ToDrawingSize(this SKSize size)
		{
			return new System.Drawing.SizeF(size.Width, size.Height);
		}

		public static System.Drawing.Size ToDrawingSize(this SKSizeI size)
		{
			return new System.Drawing.Size(size.Width, size.Height);
		}

		// System.Drawing.Color

		public static SKColor ToSKColor(this System.Drawing.Color color)
		{
			return (SKColor)(uint)color.ToArgb();
		}

		public static System.Drawing.Color ToDrawingColor(this SKColor color)
		{
			return System.Drawing.Color.FromArgb((int)(uint)color);
		}
#endif
	}
}
