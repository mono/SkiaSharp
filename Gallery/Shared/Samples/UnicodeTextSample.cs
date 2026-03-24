using SkiaSharp;

namespace SkiaSharpSample.Samples
{
	[Preserve(AllMembers = true)]
	public class UnicodeTextSample : SampleBase
	{
		[Preserve]
		public UnicodeTextSample()
		{
		}

		public override string Title => "Unicode Text";

		public override SampleCategories Category => SampleCategories.Text;

		protected override void OnDrawSample(SKCanvas canvas, int width, int height)
		{
			canvas.DrawColor(SKColors.White);

			var basePaint = new SKPaint();
			basePaint.IsAntialias = true;
			basePaint.TextSize = 60;

			// first, we can just use a font (system default if not exists)
			using (var paint = basePaint.Clone())
			using (var courier = SKTypeface.FromFamilyName("Courier New"))
			{
				paint.Typeface = courier;
				canvas.DrawText("'A', 'ݐ', '年'", 40, 100, paint);
			}

			// the font manager finds fonts
			var fontManager = SKFontManager.Default;

			// or, we can try and make sure that we have the character, and hope it has the others
			using (var paint = basePaint.Clone())
			using (var courier = fontManager.MatchCharacter("Courier New", 'ݐ'))
			{
				paint.Typeface = courier;
				canvas.DrawText("'A', 'ݐ', '年'", 40, 200, paint);
			}

			// if we know the font doesn't have the character, or don't want to fall back
			using (var paint = basePaint.Clone())
			using (var courier = SKTypeface.FromFamilyName("Courier New"))
			using (var arabic = fontManager.MatchCharacter("Courier New", 'ݐ'))
			using (var japanese = fontManager.MatchCharacter("Courier New", '年'))
			{
				var first = "'A', '";
				var arabChar = "ݐ";
				var mid = "', '";
				var japChar = "年";
				var last = "'";

				float x = 40;

				// draw the first bit
				paint.Typeface = courier;
				canvas.DrawText(first, x, 300, paint);
				x += paint.MeasureText(first);

				// the arab character
				paint.Typeface = arabic;
				canvas.DrawText(arabChar, x, 300, paint);
				x += paint.MeasureText(arabChar);

				// draw the next bit
				paint.Typeface = courier;
				canvas.DrawText(mid, x, 300, paint);
				x += paint.MeasureText(mid);

				// the japanese character
				paint.Typeface = japanese;
				canvas.DrawText(japChar, x, 300, paint);
				x += paint.MeasureText(japChar);

				// the end
				paint.Typeface = courier;
				canvas.DrawText(last, x, 300, paint);
			}

			// let's draw some emojis (UTF-32 characters)
			var emojiChar = StringUtilities.GetUnicodeCharacterCode("🚀", SKTextEncoding.Utf32);
			using (var paint = basePaint.Clone())
			using (var emoji = fontManager.MatchCharacter(emojiChar))
			{
				paint.Typeface = emoji;
				canvas.DrawText("🌐 🍪 🍕 🚀", 40, 400, paint);
			}
		}
	}
}
