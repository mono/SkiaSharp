using SkiaSharp;

namespace SkiaSharpSample.Samples
{
	[Preserve(AllMembers = true)]
	public class PathConicToQuadsSample : SampleBase
	{
		[Preserve]
		public PathConicToQuadsSample()
		{
		}

		public override string Title => "Path (conic to quads)";

		public override SampleCategories Category => SampleCategories.Paths;

		protected override void OnDrawSample(SKCanvas canvas, int width, int height)
		{
			canvas.Clear(SKColors.White);
			canvas.Scale(2);

			var points = new[]
			{
				new SKPoint(10, 10),
				new SKPoint(50, 20),
				new SKPoint(100, 150)
			};

			using (var paint = new SKPaint())
			using (var textPaint = new SKPaint())
			{
				paint.Style = SKPaintStyle.Stroke;
				paint.StrokeWidth = 5;
				paint.IsAntialias = true;
				paint.StrokeCap = SKStrokeCap.Round;

				textPaint.IsAntialias = true;

				using (var path = new SKPath())
				{
					// create a conic path
					path.MoveTo(points[0]);
					path.ConicTo(points[1], points[2], 10);

					// draw the conic-based path
					paint.Color = SampleMedia.Colors.XamarinDarkBlue;
					canvas.DrawPath(path, paint);

					// get the quads from the conic points
					var quads = SKPath.ConvertConicToQuads(points[0], points[1], points[2], 10, out var pts, 2);

					// move the points on a bit
					for (var i = 0; i < pts.Length; i++)
						pts[i].Offset(120, 0);
					// draw the quad-based path
					using (var quadsPath = new SKPath())
					{
						quadsPath.MoveTo(pts[0].X, pts[0].Y);
						for (var i = 0; i < quads; i++)
						{
							var idx = i * 2;
							quadsPath.CubicTo(
								pts[idx].X, pts[idx].Y,
								pts[idx + 1].X, pts[idx + 1].Y,
								pts[idx + 2].X, pts[idx + 2].Y);
						}

						paint.Color = SampleMedia.Colors.XamarinPurple;
						canvas.DrawPath(quadsPath, paint);
					}

					// move the points on a bit
					for (var i = 0; i < pts.Length; i++)
						pts[i].Offset(120, 0);
					// draw the dots
					paint.Color = SampleMedia.Colors.XamarinGreen;
					canvas.DrawPoints(SKPointMode.Points, pts, paint);
				}
			}
		}
	}
}
