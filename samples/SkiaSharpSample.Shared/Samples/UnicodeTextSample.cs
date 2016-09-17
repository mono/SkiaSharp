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
			// stops at 0x530 on Android
			string text = "\u03A3 and \u0750";

			using (var paint = new SKPaint())
			using (var tf = SKTypeface.FromFamilyName("Tahoma"))
			{
				canvas.DrawColor(SKColors.White);

				paint.IsAntialias = true;
				paint.TextSize = 60;
				paint.Typeface = tf;
				canvas.DrawText(text, 50, 100, paint);
			}

			using (var paint = new SKPaint())
			using (var tf = SKTypeface.FromFamilyName("Times New Roman"))
			{
				paint.Color = SampleMedia.Colors.XamarinDarkBlue;

				paint.IsAntialias = true;
				paint.TextSize = 60;
				paint.Typeface = tf;
				canvas.DrawText(text, 50, 200, paint);
			}
		}
	}
}
