using System.IO;
using Xunit;

using SkiaSharp;
using SkiaSharp.HarfBuzz;
using SkiaSharp.Tests;

namespace HarfBuzzSharp.Tests
{
	public class SKShaperTest : SKTest
	{
		[SkippableFact]
		public void DrawShapedTextExtensionMethodDraws()
		{
			using (var surface = SKSurface.Create(new SKImageInfo(512, 512)))
			using (var tf = SKTypeface.FromFile(Path.Combine(PathToFonts, "content-font.ttf")))
			using (var shaper = new SKShaper(tf))
			using (var paint = new SKPaint { IsAntialias = true, TextSize = 64, Typeface = tf })
			{
				surface.Canvas.Clear(SKColors.White);

				surface.Canvas.DrawShapedText(shaper, "متن", 100, 200, paint);

				surface.Canvas.Flush();

				using (var img = surface.Snapshot())
				using (var data = img.Encode(SKEncodedImageFormat.Png, 100))
				using (var stream = File.OpenWrite(Path.Combine(PathToImages, "test.png")))
				{
					data.AsStream().CopyTo(stream);
				}
			}
		}

		[SkippableFact]
		public void CorrectlyShapesArabicScriptAtAnOffset()
		{
			var clusters = new int[] { 4, 2, 0 };
			var codepoints = new int[] { 629, 668, 891 };
			var points = new SKPoint[] { new SKPoint(100, 200), new SKPoint(128.375f, 200), new SKPoint(142.125f, 200) };

			using (var tf = SKTypeface.FromFile(Path.Combine(PathToFonts, "content-font.ttf")))
			using (var shaper = new SKShaper(tf))
			using (var paint = new SKPaint { IsAntialias = true, TextSize = 64, Typeface = tf })
			{
				var result = shaper.Shape("متن", 100, 200, paint);

				Assert.Equal(clusters, result.Clusters);
				Assert.Equal(codepoints, result.Codepoints);
				Assert.Equal(points, result.Points);
			}
		}

		[SkippableFact]
		public void CorrectlyShapesArabicScript()
		{
			var clusters = new int[] { 4, 2, 0 };
			var codepoints = new int[] { 629, 668, 891 };
			var points = new SKPoint[] { new SKPoint(0, 0), new SKPoint(28.375f, 0), new SKPoint(42.125f, 0) };

			using (var tf = SKTypeface.FromFile(Path.Combine(PathToFonts, "content-font.ttf")))
			using (var shaper = new SKShaper(tf))
			using (var paint = new SKPaint { IsAntialias = true, TextSize = 64, Typeface = tf })
			{
				var result = shaper.Shape("متن", paint);

				Assert.Equal(clusters, result.Clusters);
				Assert.Equal(codepoints, result.Codepoints);
				Assert.Equal(points, result.Points);
			}
		}
	}
}
