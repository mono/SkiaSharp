using System.IO;

using SkiaSharp;

namespace SkiaSharpSample.Samples
{
	[Preserve(AllMembers = true)]
	public class CustomFontsSample : SampleBase
	{
		[Preserve]
		public CustomFontsSample()
		{
		}

		public override string Title => "Custom Fonts";

		public override SampleCategories Category => SampleCategories.Fonts | SampleCategories.Text;

		protected override void OnDrawSample(SKCanvas canvas, int width, int height)
		{
			var text = "\u03A3 and \u0750";

			canvas.Clear(SKColors.White);

			using (var paint = new SKPaint())
			{
				paint.IsAntialias = true;

				using (var tf = SKTypeface.FromFile(SampleMedia.Fonts.ContentFontPath))
				{
					paint.Color = SampleMedia.Colors.XamarinGreen;
					paint.TextSize = 60;
					paint.Typeface = tf;

					canvas.DrawText(text, 50, 50, paint);
				}

				using (var fileStream = new SKFileStream(SampleMedia.Fonts.ContentFontPath))
				using (var tf = SKTypeface.FromStream(fileStream))
				{
					paint.Color = SampleMedia.Colors.XamarinDarkBlue;
					paint.TextSize = 60;
					paint.Typeface = tf;

					canvas.DrawText(text, 50, 100, paint);
				}

				using (var resource = SampleMedia.Fonts.EmbeddedFont)
				using (var memory = new MemoryStream())
				{
					resource.CopyTo(memory);
					var bytes = memory.ToArray();

					using (var stream = new SKMemoryStream(bytes))
					using (var tf = SKTypeface.FromStream(stream))
					{
						paint.Color = SampleMedia.Colors.XamarinLightBlue;
						paint.TextSize = 60;
						paint.Typeface = tf;

						canvas.DrawText(text, 50, 150, paint);
					}
				}

				using (var managedResource = SampleMedia.Fonts.EmbeddedFont)
				using (var managedStream = new SKManagedStream(managedResource, true))
				using (var tf = SKTypeface.FromStream(managedStream))
				{
					paint.Color = SampleMedia.Colors.XamarinPurple;
					paint.TextSize = 60;
					paint.Typeface = tf;

					canvas.DrawText(text, 50, 200, paint);
				}
			}
		}
	}
}
