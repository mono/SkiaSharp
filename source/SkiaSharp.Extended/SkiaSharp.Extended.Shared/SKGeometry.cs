using System;
using System.Collections.Generic;
using System.Linq;

namespace SkiaSharp.Extended
{
	public static class SKGeometry
	{
		public const float PI = (float)Math.PI;

		private const float UprightAngle = PI / 2f;
		private const float TotalAngle = 2f * PI;

		public static SKPoint GetCirclePoint(float r, float angle)
		{
			return new SKPoint(r * (float)Math.Cos(angle), r * (float)Math.Sin(angle));
		}

		public static SKPath CreateSectorPath(float start, float end, float outerRadius, float innerRadius = 0.0f, float margin = 0.0f, float explodeDistance = 0.0f, SKPathDirection direction = SKPathDirection.Clockwise)
		{
			var path = new SKPath();

			// if the sector has no size, then it has no path
			if (start == end)
			{
				return path;
			}

			// the the sector is a full circle, then do that
			if (end - start == 1.0f)
			{
				path.AddCircle(0, 0, outerRadius, direction);
				path.AddCircle(0, 0, innerRadius, direction);
				path.FillType = SKPathFillType.EvenOdd;
				return path;
			}

			// calculate the angles
			var startAngle = TotalAngle * start - UprightAngle;
			var endAngle = TotalAngle * end - UprightAngle;
			var large = endAngle - startAngle > PI ? SKPathArcSize.Large : SKPathArcSize.Small;
			var sectorCenterAngle = (endAngle - startAngle) / 2f + startAngle;

			//// get the radius bits
			//var sectorCenterRadius = (outerRadius - innerRadius) / 2f + innerRadius;

			// move explosion around 90 degrees, since matrix use down as 0
			var explosionMatrix = SKMatrix.MakeRotation(sectorCenterAngle - (PI / 2f));
			var offset = explosionMatrix.MapPoint(new SKPoint(0, explodeDistance));

			// calculate the angle for the margins
			margin = direction == SKPathDirection.Clockwise ? margin : -margin;
			var offsetR = outerRadius == 0 ? 0 : ((margin / (TotalAngle * outerRadius)) * TotalAngle);
			var offsetr = innerRadius == 0 ? 0 : ((margin / (TotalAngle * innerRadius)) * TotalAngle);

			// get the points
			var a = GetCirclePoint(outerRadius, startAngle + offsetR) + offset;
			var b = GetCirclePoint(outerRadius, endAngle - offsetR) + offset;
			var c = GetCirclePoint(innerRadius, endAngle - offsetr) + offset;
			var d = GetCirclePoint(innerRadius, startAngle + offsetr) + offset;

			// add the points to the path
			path.MoveTo(a);
			path.ArcTo(outerRadius, outerRadius, 0, large, direction, b.X, b.Y);
			path.LineTo(c);
			if (innerRadius == 0.0f)
			{
				// take a short cut
				path.LineTo(d);
			}
			else
			{
				var reverseDirection = direction == SKPathDirection.Clockwise ? SKPathDirection.CounterClockwise : SKPathDirection.Clockwise;
				path.ArcTo(innerRadius, innerRadius, 0, large, reverseDirection, d.X, d.Y);
			}
			path.Close();

			return path;
		}

		public static SKPath CreatePiePath(IEnumerable<float> sectorSizes, float outerRadius, float innerRadius = 0.0f, float spacing = 0.0f, float explodeDistance = 0.0f, SKPathDirection direction = SKPathDirection.Clockwise)
		{
			var path = new SKPath();

			float cursor = 0;
			//var sum = sectorSizes.Sum();
			foreach (var sectorSize in sectorSizes)
			{
				var sector = CreateSectorPath(cursor, cursor + sectorSize, outerRadius, innerRadius, spacing / 2f, explodeDistance, direction);

				cursor += sectorSize;

				path.AddPath(sector, SKPathAddMode.Append);
			}

			return path;
		}

		public static SKPath CreateSquarePath(float side, SKPathDirection direction = SKPathDirection.Clockwise)
		{
			return CreateRectanglePath(side, side, direction);
		}

		public static SKPath CreateRectanglePath(float width, float height, SKPathDirection direction = SKPathDirection.Clockwise)
		{
			var path = new SKPath();
			path.AddRect(new SKRect(width / -2, height / -2, width / 2, height / 2), direction);
			path.Close();
			return path;
		}

		public static SKPath CreateTrianglePath(float width, float height, SKPathDirection direction = SKPathDirection.Clockwise)
		{
			var path = new SKPath();
			path.MoveTo(0, height / -2);
			if (direction == SKPathDirection.Clockwise)
			{
				path.LineTo(width / -2, height / 2);
				path.LineTo(width / 2, height / 2);
			}
			else
			{
				path.LineTo(width / 2, height / 2);
				path.LineTo(width / -2, height / 2);
			}
			path.Close();
			return path;
		}

		public static SKPath CreateTrianglePath(float radius, SKPathDirection direction = SKPathDirection.Clockwise)
		{
			return CreateRegularPolygonPath(radius, 3, direction);
		}

		public static SKPath CreateRegularPolygonPath(float radius, int points, SKPathDirection direction = SKPathDirection.Clockwise)
		{
			var path = new SKPath();

			float stepAngle = TotalAngle / points;
			if (direction == SKPathDirection.CounterClockwise)
			{
				stepAngle = -stepAngle;
			}

			for (int p = 0; p < points; p++)
			{
				float angle = stepAngle * p - UprightAngle;
				float x = radius * (float)Math.Cos(angle);
				float y = radius * (float)Math.Sin(angle);

				if (p == 0)
					path.MoveTo(x, y);
				else
					path.LineTo(x, y);
			}

			path.Close();
			return path;
		}

		public static SKPath CreateRegularStarPath(float outerRadius, float innerRadius, int points, SKPathDirection direction = SKPathDirection.Clockwise)
		{
			var path = new SKPath();

			bool isInner = false;
			points *= 2;

			float stepAngle = TotalAngle / points;
			if (direction == SKPathDirection.CounterClockwise)
			{
				stepAngle = -stepAngle;
			}

			for (int p = 0; p < points; p++)
			{
				float radius = isInner ? innerRadius : outerRadius;

				float angle = stepAngle * p - UprightAngle;
				float x = radius * (float)Math.Cos(angle);
				float y = radius * (float)Math.Sin(angle);

				if (p == 0)
					path.MoveTo(x, y);
				else
					path.LineTo(x, y);

				isInner = !isInner;
			}

			path.Close();
			return path;
		}
	}
}
