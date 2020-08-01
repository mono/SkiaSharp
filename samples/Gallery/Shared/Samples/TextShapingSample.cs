#if !HAS_UNO
using SkiaSharp;
using SkiaSharp.HarfBuzz;

namespace SkiaSharpSample.Samples
{
	[Preserve(AllMembers = true)]
	public class TextShapingSample : SampleBase
	{
		[Preserve]
		public TextShapingSample()
		{
		}

		public override string Title => "Text Shaping";

		public override SampleCategories Category => SampleCategories.Text;

		protected override void OnDrawSample(SKCanvas canvas, int width, int height)
		{
			canvas.DrawColor(SKColors.White);

			using (var tf = SKFontManager.Default.MatchCharacter('م'))
			using (var paint = new SKPaint { IsAntialias = true, TextSize = 64 })
			using (var arabicPaint = new SKPaint { IsAntialias = true, TextSize = 64, Typeface = tf })
			{
				// unshaped
				canvas.DrawText("Unshaped:", 100, 100, paint);
				canvas.DrawText("مرحبا بالعالم", 100, 180, arabicPaint);

				// shaped
				using (var shaper = new SKShaper(tf))
				{
					canvas.DrawText("Shaped:", 100, 300, paint);
					canvas.DrawShapedText(shaper, "مرحبا بالعالم", 100, 380, arabicPaint);
				}
			}
		}
	}
}
#endif
