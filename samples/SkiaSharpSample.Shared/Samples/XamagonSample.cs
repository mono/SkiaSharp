using SkiaSharp;

namespace SkiaSharpSample.Samples
{
	[Preserve(AllMembers = true)]
	public class XamagonSample : SampleBase
	{
		[Preserve]
		public XamagonSample()
		{
		}

		public override string Title => "\"Xamagon\"";

		public override SampleCategories Category => SampleCategories.Paths | SampleCategories.Showcases;

		protected override void OnDrawSample(SKCanvas canvas, int width, int height)
		{
			// Width 41.6587026 => 144.34135
			// Height 56 => 147

			var paddingFactor = .6f;
			var imageLeft = 41.6587026f;
			var imageRight = 144.34135f;
			var imageTop = 56f;
			var imageBottom = 147f;

			var imageWidth = imageRight - imageLeft;

			var scale = (((float)height > width ? width : height) / imageWidth) * paddingFactor;

			var translateX = (imageLeft + imageRight) / -2 + width / scale * 1 / 2;
			var translateY = (imageBottom + imageTop) / -2 + height / scale * 1 / 2;

			canvas.Scale(scale, scale);
			canvas.Translate(translateX, translateY);

			using (var paint = new SKPaint())
			{
				paint.IsAntialias = true;
				paint.Color = SKColors.White;
				paint.StrokeCap = SKStrokeCap.Round;

				canvas.DrawPaint(paint);

				using (var path = new SKPath())
				{
					path.MoveTo(71.4311121f, 56f);
					path.CubicTo(68.6763107f, 56.0058575f, 65.9796704f, 57.5737917f, 64.5928855f, 59.965729f);
					path.LineTo(43.0238921f, 97.5342563f);
					path.CubicTo(41.6587026f, 99.9325978f, 41.6587026f, 103.067402f, 43.0238921f, 105.465744f);
					path.LineTo(64.5928855f, 143.034271f);
					path.CubicTo(65.9798162f, 145.426228f, 68.6763107f, 146.994582f, 71.4311121f, 147f);
					path.LineTo(114.568946f, 147f);
					path.CubicTo(117.323748f, 146.994143f, 120.020241f, 145.426228f, 121.407172f, 143.034271f);
					path.LineTo(142.976161f, 105.465744f);
					path.CubicTo(144.34135f, 103.067402f, 144.341209f, 99.9325978f, 142.976161f, 97.5342563f);
					path.LineTo(121.407172f, 59.965729f);
					path.CubicTo(120.020241f, 57.5737917f, 117.323748f, 56.0054182f, 114.568946f, 56f);
					path.LineTo(71.4311121f, 56f);
					path.Close();

					paint.Color = SampleMedia.Colors.XamarinDarkBlue;
					canvas.DrawPath(path, paint);
				}

				using (var path = new SKPath())
				{
					path.MoveTo(71.8225901f, 77.9780432f);
					path.CubicTo(71.8818491f, 77.9721857f, 71.9440029f, 77.9721857f, 72.0034464f, 77.9780432f);
					path.LineTo(79.444074f, 77.9780432f);
					path.CubicTo(79.773437f, 77.9848769f, 80.0929203f, 78.1757336f, 80.2573978f, 78.4623994f);
					path.LineTo(92.8795281f, 101.015639f);
					path.CubicTo(92.9430615f, 101.127146f, 92.9839987f, 101.251384f, 92.9995323f, 101.378901f);
					path.CubicTo(93.0150756f, 101.251354f, 93.055974f, 101.127107f, 93.1195365f, 101.015639f);
					path.LineTo(105.711456f, 78.4623994f);
					path.CubicTo(105.881153f, 78.167045f, 106.215602f, 77.975134f, 106.554853f, 77.9780432f);
					path.LineTo(113.995483f, 77.9780432f);
					path.CubicTo(114.654359f, 77.9839007f, 115.147775f, 78.8160066f, 114.839019f, 79.4008677f);
					path.LineTo(102.518299f, 101.500005f);
					path.LineTo(114.839019f, 123.568869f);
					path.CubicTo(115.176999f, 124.157088f, 114.671442f, 125.027775f, 113.995483f, 125.021957f);
					path.LineTo(106.554853f, 125.021957f);
					path.CubicTo(106.209673f, 125.019028f, 105.873247f, 124.81384f, 105.711456f, 124.507327f);
					path.LineTo(93.1195365f, 101.954088f);
					path.CubicTo(93.0560031f, 101.84258f, 93.0150659f, 101.718333f, 92.9995323f, 101.590825f);
					path.CubicTo(92.983989f, 101.718363f, 92.9430906f, 101.842629f, 92.8795281f, 101.954088f);
					path.LineTo(80.2573978f, 124.507327f);
					path.CubicTo(80.1004103f, 124.805171f, 79.7792269f, 125.008397f, 79.444074f, 125.021957f);
					path.LineTo(72.0034464f, 125.021957f);
					path.CubicTo(71.3274867f, 125.027814f, 70.8220664f, 124.157088f, 71.1600463f, 123.568869f);
					path.LineTo(83.4807624f, 101.500005f);
					path.LineTo(71.1600463f, 79.400867f);
					path.CubicTo(70.8647037f, 78.86725f, 71.2250368f, 78.0919422f, 71.8225901f, 77.9780432f);
					path.LineTo(71.8225901f, 77.9780432f);
					path.Close();

					paint.Color = SKColors.White;
					canvas.DrawPath(path, paint);
				}
			}
		}
	}
}
