using SkiaSharp;

namespace SkiaSharpSample.Samples
{
	[Preserve(AllMembers = true)]
	public class MeasureTextSample : SampleBase
	{
		[Preserve]
		public MeasureTextSample()
		{
		}

		public override string Title => "Measure Text";

		public override SampleCategories Category => SampleCategories.Text;

		protected override void OnDrawSample(SKCanvas canvas, int width, int height)
		{
			canvas.DrawColor(SKColors.White);

			using (var paint = new SKPaint())
			{
				paint.TextSize = 64.0f;
				paint.IsAntialias = true;
				paint.Color = (SKColor)0xFF4281A4;
				paint.TextEncoding = SKTextEncoding.Utf32;

				canvas.DrawText("Skia (UTF-32)", 0, 64.0f, paint);

				var bounds = new SKRect();
				paint.MeasureText("Skia (UTF-32)", ref bounds);
				bounds.Top += 64.0f;
				bounds.Bottom += 64.0f;

				paint.IsStroke = true;
				paint.Color = SKColors.Red;

				canvas.DrawRect(bounds, paint);
			}

			using (var paint = new SKPaint())
			{
				paint.TextSize = 64.0f;
				paint.IsAntialias = true;
				paint.Color = (SKColor)0xFF9CAFB7;
				paint.TextEncoding = SKTextEncoding.Utf16;

				canvas.DrawText("Skia (UTF-16)", 0, 144.0f, paint);

				var bounds = new SKRect();
				paint.MeasureText("Skia (UTF-16)", ref bounds);
				bounds.Top += 144.0f;
				bounds.Bottom += 144.0f;

				paint.IsStroke = true;
				paint.Color = SKColors.Red;

				canvas.DrawRect(bounds, paint);
			}

			using (var paint = new SKPaint())
			{
				paint.TextSize = 64.0f;
				paint.IsAntialias = true;
				paint.Color = (SKColor)0xFFE6B89C;
				paint.TextEncoding = SKTextEncoding.Utf8;

				canvas.DrawText("Skia (UTF-8)", 0, 224.0f, paint);

				var bounds = new SKRect();
				paint.MeasureText("Skia (UTF-8)", ref bounds);
				bounds.Top += 224.0f;
				bounds.Bottom += 224.0f;

				paint.IsStroke = true;
				paint.Color = SKColors.Red;

				canvas.DrawRect(bounds, paint);
			}
		}
	}
}
