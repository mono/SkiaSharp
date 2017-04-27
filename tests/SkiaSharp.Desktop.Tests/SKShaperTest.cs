using System.IO;
using NUnit.Framework;

using HarfBuzzSharp;

namespace SkiaSharp.Tests
{
	public class SKShaperTest : SKTest
	{
		[Test]
		public void HarfBuzzTests()
		{
			using (var surface = SKSurface.Create(new SKImageInfo(512, 512)))
			using (var tf = SKTypeface.FromFamilyName(null))
			using (var shaper = new SKShaper(tf))
			using (var paint = new SKPaint { IsAntialias = true, TextSize = 64, Typeface = SKTypeface.FromFamilyName("Tahoma") })
			{
				surface.Canvas.Clear(SKColors.White);

				surface.Canvas.DrawShapedText(shaper, "A متن !", 100, 100, paint);

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

		[Test]
		public void CorrectlyShapesArabicScriptAtAnOffset()
		{
			var clusters = new uint[] { 4, 2, 0 };
			var codepoints = new byte[] { 230, 3, 152, 3, 227, 3 };
			var points = new SKPoint[] { new SKPoint(100, 200), new SKPoint(148.25f, 200), new SKPoint(170.75f, 200) };

			using (var tf = SKTypeface.FromFamilyName("Tahoma"))
			using (var shaper = new SKShaper(tf))
			using (var paint = new SKPaint { IsAntialias = true, TextSize = 64, Typeface = tf })
			{
				var result = shaper.Shape("متن", 100, 200, paint);

				CollectionAssert.AreEqual(clusters, result.Clusters);
				CollectionAssert.AreEqual(codepoints, result.Codepoints);
				CollectionAssert.AreEqual(points, result.Points);
			}
		}

		[Test]
		public void CorrectlyShapesArabicScript()
		{
			var clusters = new uint[] { 4, 2, 0 };
			var codepoints = new byte[] { 230, 3, 152, 3, 227, 3 };
			var points = new SKPoint[] { new SKPoint(0, 0), new SKPoint(48.25f, 0), new SKPoint(70.75f, 0) };

			using (var tf = SKTypeface.FromFamilyName("Tahoma"))
			using (var shaper = new SKShaper(tf))
			using (var paint = new SKPaint { IsAntialias = true, TextSize = 64, Typeface = tf })
			{
				var result = shaper.Shape("متن", paint);

				CollectionAssert.AreEqual(clusters, result.Clusters);
				CollectionAssert.AreEqual(codepoints, result.Codepoints);
				CollectionAssert.AreEqual(points, result.Points);
			}
		}
	}
}
