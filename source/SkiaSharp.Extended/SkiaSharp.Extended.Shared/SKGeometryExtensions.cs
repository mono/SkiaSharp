namespace SkiaSharp.Extended
{
	public static class SKGeometryExtensions
	{
		public static void DrawSquare(this SKCanvas canvas, float cx, float cy, float side, SKPaint paint)
		{
			var path = SKGeometry.CreateSquarePath(side);
			path.Offset(cx, cy);
			canvas.DrawPath(path, paint);
		}

		public static void DrawRect(this SKCanvas canvas, float cx, float cy, float width, float height, SKPaint paint)
		{
			var path = SKGeometry.CreateRectanglePath(width, height);
			path.Offset(cx, cy);
			canvas.DrawPath(path, paint);
		}

		public static void DrawTriangle(this SKCanvas canvas, float cx, float cy, float width, float height, SKPaint paint)
		{
			var path = SKGeometry.CreateTrianglePath(width, height);
			path.Offset(cx, cy);
			canvas.DrawPath(path, paint);
		}

		public static void DrawTriangle(this SKCanvas canvas, float cx, float cy, float radius, SKPaint paint)
		{
			var path = SKGeometry.CreateTrianglePath(radius);
			path.Offset(cx, cy);
			canvas.DrawPath(path, paint);
		}

		public static void DrawRegularPolygon(this SKCanvas canvas, float cx, float cy, float radius, int points, SKPaint paint)
		{
			var path = SKGeometry.CreateRegularPolygonPath(radius, points);
			path.Offset(cx, cy);
			canvas.DrawPath(path, paint);
		}

		public static void DrawStar(this SKCanvas canvas, float cx, float cy, float outerRadius, float innerRadius, int points, SKPaint paint)
		{
			var path = SKGeometry.CreateRegularStarPath(outerRadius, innerRadius, points);
			path.Offset(cx, cy);
			canvas.DrawPath(path, paint);
		}
	}
}
