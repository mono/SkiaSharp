#nullable enable

using System;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace SkiaSharp.Views.Maui.Controls
{
	/// <summary>
	/// Extension methods to convert MAUI geometry types to SkiaSharp paths.
	/// </summary>
	public static class GeometryExtensions
	{
		/// <summary>
		/// Converts a MAUI <see cref="Geometry"/> to an <see cref="SKPath"/>.
		/// </summary>
		/// <param name="geometry">The MAUI geometry to convert.</param>
		/// <returns>An SKPath representing the geometry, or null if conversion is not supported.</returns>
		public static SKPath? ToSKPath(this Geometry? geometry)
		{
			if (geometry == null)
				return null;

			return geometry switch
			{
				EllipseGeometry ellipse => ellipse.ToSKPath(),
				RectangleGeometry rectangle => rectangle.ToSKPath(),
				RoundRectangleGeometry roundRect => roundRect.ToSKPath(),
				LineGeometry line => line.ToSKPath(),
				PathGeometry path => path.ToSKPath(),
				GeometryGroup group => group.ToSKPath(),
				_ => null
			};
		}

		/// <summary>
		/// Converts a MAUI <see cref="EllipseGeometry"/> to an <see cref="SKPath"/>.
		/// </summary>
		public static SKPath ToSKPath(this EllipseGeometry ellipse)
		{
			var path = new SKPath();
			var rect = new SKRect(
				(float)(ellipse.Center.X - ellipse.RadiusX),
				(float)(ellipse.Center.Y - ellipse.RadiusY),
				(float)(ellipse.Center.X + ellipse.RadiusX),
				(float)(ellipse.Center.Y + ellipse.RadiusY));
			path.AddOval(rect);
			return path;
		}

		/// <summary>
		/// Converts a MAUI <see cref="RectangleGeometry"/> to an <see cref="SKPath"/>.
		/// </summary>
		public static SKPath ToSKPath(this RectangleGeometry rectangle)
		{
			var path = new SKPath();
			var rect = new SKRect(
				(float)rectangle.Rect.Left,
				(float)rectangle.Rect.Top,
				(float)rectangle.Rect.Right,
				(float)rectangle.Rect.Bottom);
			path.AddRect(rect);
			return path;
		}

		/// <summary>
		/// Converts a MAUI <see cref="RoundRectangleGeometry"/> to an <see cref="SKPath"/>.
		/// </summary>
		public static SKPath ToSKPath(this RoundRectangleGeometry roundRect)
		{
			var path = new SKPath();
			var rect = new SKRect(
				(float)roundRect.Rect.Left,
				(float)roundRect.Rect.Top,
				(float)roundRect.Rect.Right,
				(float)roundRect.Rect.Bottom);
			var cornerRadius = (float)roundRect.CornerRadius.TopLeft;
			path.AddRoundRect(rect, cornerRadius, cornerRadius);
			return path;
		}

		/// <summary>
		/// Converts a MAUI <see cref="LineGeometry"/> to an <see cref="SKPath"/>.
		/// </summary>
		public static SKPath ToSKPath(this LineGeometry line)
		{
			var path = new SKPath();
			path.MoveTo((float)line.StartPoint.X, (float)line.StartPoint.Y);
			path.LineTo((float)line.EndPoint.X, (float)line.EndPoint.Y);
			return path;
		}

		/// <summary>
		/// Converts a MAUI <see cref="PathGeometry"/> to an <see cref="SKPath"/>.
		/// </summary>
		public static SKPath ToSKPath(this PathGeometry pathGeometry)
		{
			var path = new SKPath();

			if (pathGeometry.Figures == null)
				return path;

			path.FillType = pathGeometry.FillRule == FillRule.EvenOdd 
				? SKPathFillType.EvenOdd 
				: SKPathFillType.Winding;

			foreach (var figure in pathGeometry.Figures)
			{
				path.MoveTo((float)figure.StartPoint.X, (float)figure.StartPoint.Y);

				if (figure.Segments == null)
					continue;

				foreach (var segment in figure.Segments)
				{
					AddSegmentToPath(path, segment);
				}

				if (figure.IsClosed)
				{
					path.Close();
				}
			}

			return path;
		}

		/// <summary>
		/// Converts a MAUI <see cref="GeometryGroup"/> to an <see cref="SKPath"/>.
		/// </summary>
		public static SKPath ToSKPath(this GeometryGroup group)
		{
			var path = new SKPath();

			path.FillType = group.FillRule == FillRule.EvenOdd 
				? SKPathFillType.EvenOdd 
				: SKPathFillType.Winding;

			if (group.Children == null)
				return path;

			foreach (var child in group.Children)
			{
				var childPath = child.ToSKPath();
				if (childPath != null)
				{
					path.AddPath(childPath);
				}
			}

			return path;
		}

		private static void AddSegmentToPath(SKPath path, PathSegment segment)
		{
			switch (segment)
			{
				case LineSegment line:
					path.LineTo((float)line.Point.X, (float)line.Point.Y);
					break;

				case BezierSegment bezier:
					path.CubicTo(
						(float)bezier.Point1.X, (float)bezier.Point1.Y,
						(float)bezier.Point2.X, (float)bezier.Point2.Y,
						(float)bezier.Point3.X, (float)bezier.Point3.Y);
					break;

				case QuadraticBezierSegment quadratic:
					path.QuadTo(
						(float)quadratic.Point1.X, (float)quadratic.Point1.Y,
						(float)quadratic.Point2.X, (float)quadratic.Point2.Y);
					break;

				case ArcSegment arc:
					AddArcToPath(path, arc);
					break;

				case PolyLineSegment polyLine:
					if (polyLine.Points != null)
					{
						foreach (var point in polyLine.Points)
						{
							path.LineTo((float)point.X, (float)point.Y);
						}
					}
					break;

				case PolyBezierSegment polyBezier:
					if (polyBezier.Points != null && polyBezier.Points.Count >= 3)
					{
						for (int i = 0; i + 2 < polyBezier.Points.Count; i += 3)
						{
							path.CubicTo(
								(float)polyBezier.Points[i].X, (float)polyBezier.Points[i].Y,
								(float)polyBezier.Points[i + 1].X, (float)polyBezier.Points[i + 1].Y,
								(float)polyBezier.Points[i + 2].X, (float)polyBezier.Points[i + 2].Y);
						}
					}
					break;

				case PolyQuadraticBezierSegment polyQuadratic:
					if (polyQuadratic.Points != null && polyQuadratic.Points.Count >= 2)
					{
						for (int i = 0; i + 1 < polyQuadratic.Points.Count; i += 2)
						{
							path.QuadTo(
								(float)polyQuadratic.Points[i].X, (float)polyQuadratic.Points[i].Y,
								(float)polyQuadratic.Points[i + 1].X, (float)polyQuadratic.Points[i + 1].Y);
						}
					}
					break;
			}
		}

		private static void AddArcToPath(SKPath path, ArcSegment arc)
		{
			// Get the current point to use as the start of the arc
			path.GetLastPoint(out var lastPoint);
			
			var startX = lastPoint.X;
			var startY = lastPoint.Y;
			var endX = (float)arc.Point.X;
			var endY = (float)arc.Point.Y;
			var radiusX = (float)arc.Size.Width;
			var radiusY = (float)arc.Size.Height;
			var rotation = (float)arc.RotationAngle;
			var largeArc = arc.IsLargeArc ? SKPathArcSize.Large : SKPathArcSize.Small;
			var sweep = arc.SweepDirection == SweepDirection.Clockwise 
				? SKPathDirection.Clockwise 
				: SKPathDirection.CounterClockwise;

			path.ArcTo(radiusX, radiusY, rotation, largeArc, sweep, endX, endY);
		}

		/// <summary>
		/// Converts a MAUI <see cref="Shape"/> to an <see cref="SKPath"/>.
		/// </summary>
		/// <param name="shape">The MAUI shape to convert.</param>
		/// <returns>An SKPath representing the shape.</returns>
		public static SKPath? ToSKPath(this Shape? shape)
		{
			if (shape == null)
				return null;

			return shape switch
			{
				Microsoft.Maui.Controls.Shapes.Rectangle rect => CreateRectanglePath(rect),
				Microsoft.Maui.Controls.Shapes.RoundRectangle roundRect => CreateRoundRectanglePath(roundRect),
				Ellipse ellipse => CreateEllipsePath(ellipse),
				Line line => CreateLinePath(line),
				Polyline polyline => CreatePolylinePath(polyline),
				Polygon polygon => CreatePolygonPath(polygon),
				Microsoft.Maui.Controls.Shapes.Path path => path.Data?.ToSKPath(),
				_ => null
			};
		}

		private static SKPath CreateRectanglePath(Microsoft.Maui.Controls.Shapes.Rectangle rect)
		{
			var path = new SKPath();
			var width = (float)rect.Width;
			var height = (float)rect.Height;
			
			if (double.IsNaN(width) || double.IsInfinity(width))
				width = 0;
			if (double.IsNaN(height) || double.IsInfinity(height))
				height = 0;
				
			path.AddRect(new SKRect(0, 0, width, height));
			return path;
		}

		private static SKPath CreateRoundRectanglePath(Microsoft.Maui.Controls.Shapes.RoundRectangle roundRect)
		{
			var path = new SKPath();
			var width = (float)roundRect.Width;
			var height = (float)roundRect.Height;
			
			if (double.IsNaN(width) || double.IsInfinity(width))
				width = 0;
			if (double.IsNaN(height) || double.IsInfinity(height))
				height = 0;

			var cornerRadius = roundRect.CornerRadius;
			var topLeft = (float)cornerRadius.TopLeft;
			var topRight = (float)cornerRadius.TopRight;
			var bottomRight = (float)cornerRadius.BottomRight;
			var bottomLeft = (float)cornerRadius.BottomLeft;

			// Use the average corner radius if they're all similar, otherwise use RRect
			if (topLeft == topRight && topRight == bottomRight && bottomRight == bottomLeft)
			{
				path.AddRoundRect(new SKRect(0, 0, width, height), topLeft, topLeft);
			}
			else
			{
				var rrect = new SKRoundRect();
				rrect.SetRectRadii(
					new SKRect(0, 0, width, height),
					new[] 
					{
						new SKPoint(topLeft, topLeft),
						new SKPoint(topRight, topRight),
						new SKPoint(bottomRight, bottomRight),
						new SKPoint(bottomLeft, bottomLeft)
					});
				path.AddRoundRect(rrect);
			}
			return path;
		}

		private static SKPath CreateEllipsePath(Ellipse ellipse)
		{
			var path = new SKPath();
			var width = (float)ellipse.Width;
			var height = (float)ellipse.Height;
			
			if (double.IsNaN(width) || double.IsInfinity(width))
				width = 0;
			if (double.IsNaN(height) || double.IsInfinity(height))
				height = 0;
				
			path.AddOval(new SKRect(0, 0, width, height));
			return path;
		}

		private static SKPath CreateLinePath(Line line)
		{
			var path = new SKPath();
			path.MoveTo((float)line.X1, (float)line.Y1);
			path.LineTo((float)line.X2, (float)line.Y2);
			return path;
		}

		private static SKPath CreatePolylinePath(Polyline polyline)
		{
			var path = new SKPath();
			
			if (polyline.Points == null || polyline.Points.Count == 0)
				return path;

			path.MoveTo((float)polyline.Points[0].X, (float)polyline.Points[0].Y);
			
			for (int i = 1; i < polyline.Points.Count; i++)
			{
				path.LineTo((float)polyline.Points[i].X, (float)polyline.Points[i].Y);
			}

			return path;
		}

		private static SKPath CreatePolygonPath(Polygon polygon)
		{
			var path = new SKPath();
			
			if (polygon.Points == null || polygon.Points.Count == 0)
				return path;

			path.FillType = polygon.FillRule == FillRule.EvenOdd 
				? SKPathFillType.EvenOdd 
				: SKPathFillType.Winding;

			path.MoveTo((float)polygon.Points[0].X, (float)polygon.Points[0].Y);
			
			for (int i = 1; i < polygon.Points.Count; i++)
			{
				path.LineTo((float)polygon.Points[i].X, (float)polygon.Points[i].Y);
			}

			path.Close();
			return path;
		}
	}
}
