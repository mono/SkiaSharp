using System;

using SkiaSharp;

namespace SkiaSharpSample.Samples
{
	[Preserve(AllMembers = true)]
	public class FilledHeptagramSample : SampleBase
	{
		[Preserve]
		public FilledHeptagramSample()
		{
		}

		public override string Title => "Filled Heptagram";

		public override SampleCategories Category => SampleCategories.Paths;

		protected override void OnDrawSample(SKCanvas canvas, int width, int height)
		{
			var size = ((float)height > width ? width : height) * 0.75f;
			var R = 0.45f * size;
			var TAU = 6.2831853f;

			using (var path = new SKPath())
			{
				path.MoveTo(R, 0.0f);
				for (var i = 1; i < 7; ++i)
				{
					var theta = 3f * i * TAU / 7f;
					path.LineTo(R * (float)Math.Cos(theta), R * (float)Math.Sin(theta));
				}
				path.Close();

				using (var paint = new SKPaint())
				{
					paint.IsAntialias = true;
					canvas.Clear(SKColors.White);
					canvas.Translate(width / 2f, height / 2f);
					canvas.DrawPath(path, paint);
				}
			}
		}
	}
}
