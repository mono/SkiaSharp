using SkiaSharp;

namespace SkiaSharpSample.Samples
{
	[Preserve(AllMembers = true)]
	public class TextSample : SampleBase
	{
		[Preserve]
		public TextSample()
		{
		}

		public override string Title => "Text";

		public override SampleCategories Category => SampleCategories.Text;

		protected override void OnDrawSample(SKCanvas canvas, int width, int height)
		{
			canvas.DrawColor(SKColors.White);

			using (var paint = new SKPaint())
			{
				paint.TextSize = 64.0f;
				paint.IsAntialias = true;
				paint.Color = (SKColor)0xFF4281A4;
				paint.IsStroke = false;

				canvas.DrawText("SkiaSharp", width / 2f, 64.0f, paint);
			}

			using (var paint = new SKPaint())
			{
				paint.TextSize = 64.0f;
				paint.IsAntialias = true;
				paint.Color = (SKColor)0xFF9CAFB7;
				paint.IsStroke = true;
				paint.StrokeWidth = 3;
				paint.TextAlign = SKTextAlign.Center;

				canvas.DrawText("SkiaSharp", width / 2f, 144.0f, paint);
			}

			using (var paint = new SKPaint())
			{
				paint.TextSize = 64.0f;
				paint.IsAntialias = true;
				paint.Color = (SKColor)0xFFE6B89C;
				paint.TextScaleX = 1.5f;
				paint.TextAlign = SKTextAlign.Right;

				canvas.DrawText("SkiaSharp", width / 2f, 224.0f, paint);
			}
		}
	}
}
