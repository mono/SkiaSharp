using SkiaSharp;

namespace SkiaSharpSample.Samples
{
	[Preserve(AllMembers = true)]
	public class PathMeasureSample : SampleBase
	{
		[Preserve]
		public PathMeasureSample()
		{
		}

		public override string Title => "Path Measure";

		public override SampleCategories Category => SampleCategories.Paths;

		protected override void OnDrawSample(SKCanvas canvas, int width, int height)
		{
			canvas.Clear(SKColors.White);

			using (var paint = new SKPaint())
			using (var path = new SKPath())
			{
				paint.Style = SKPaintStyle.Stroke;
				paint.StrokeWidth = 10;
				paint.IsAntialias = true;
				paint.StrokeCap = SKStrokeCap.Round;
				paint.StrokeJoin = SKStrokeJoin.Round;

				path.MoveTo(20, 20);
				path.LineTo(400, 50);
				path.LineTo(80, 100);
				path.LineTo(300, 150);

				paint.Color = SampleMedia.Colors.XamarinDarkBlue;
				canvas.DrawPath(path, paint);

				using (var measure = new SKPathMeasure(path, false))
				using (var dst = new SKPath())
				{
					var length = measure.Length;

					dst.Reset();
					measure.GetSegment(length * 0.05f, length * 0.2f, dst, true);
					paint.Color = SampleMedia.Colors.XamarinPurple;
					canvas.DrawPath(dst, paint);

					dst.Reset();
					measure.GetSegment(length * 0.2f, length * 0.8f, dst, true);
					paint.Color = SampleMedia.Colors.XamarinGreen;
					canvas.DrawPath(dst, paint);

					dst.Reset();
					measure.GetSegment(length * 0.8f, length * 0.95f, dst, true);
					paint.Color = SampleMedia.Colors.XamarinLightBlue;
					canvas.DrawPath(dst, paint);
				}
			}
		}
	}
}
