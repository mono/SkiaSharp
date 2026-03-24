using SkiaSharp;

namespace SkiaSharpSample.Samples
{
	[Preserve(AllMembers = true)]
	public class PathBoundsSample : SampleBase
	{
		[Preserve]
		public PathBoundsSample()
		{
		}

		public override string Title => "Path Bounds";

		public override SampleCategories Category => SampleCategories.Paths;

		protected override void OnDrawSample(SKCanvas canvas, int width, int height)
		{
			canvas.Clear(SKColors.White);
			canvas.Scale(2, 2);

			using (var paint = new SKPaint())
			using (var textPaint = new SKPaint())
			{
				paint.Style = SKPaintStyle.Stroke;
				paint.StrokeWidth = 1;
				paint.IsAntialias = true;
				paint.StrokeCap = SKStrokeCap.Round;

				textPaint.IsAntialias = true;

				using (var path = new SKPath())
				{
					path.MoveTo(-6.2157825e-7f, -25.814698f);
					path.RCubicTo(-34.64102137842175f, 19.9999998f, 0f, 40f, 0f, 40f);
					path.Offset(50, 35);

					// draw using GetBounds
					paint.Color = SampleMedia.Colors.XamarinLightBlue;
					canvas.DrawPath(path, paint);

					path.GetBounds(out var rect);

					paint.Color = SampleMedia.Colors.XamarinDarkBlue;
					canvas.DrawRect(rect, paint);

					canvas.DrawText("Bounds", rect.Left, rect.Bottom + paint.TextSize + 10, textPaint);

					// move for next curve
					path.Offset(100, 0);

					// draw using GetTightBounds
					paint.Color = SampleMedia.Colors.XamarinLightBlue;
					canvas.DrawPath(path, paint);

					path.GetTightBounds(out rect);

					paint.Color = SampleMedia.Colors.XamarinDarkBlue;
					canvas.DrawRect(rect, paint);

					canvas.DrawText("TightBounds", rect.Left, rect.Bottom + paint.TextSize + 10, textPaint);
				}
			}
		}
	}
}
