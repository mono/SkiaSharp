using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using HarfBuzzSharp;
using SkiaSharp.Tests;
using Xunit;

namespace SkiaSharp.HarfBuzz.Tests
{
	public class SKShaperTest : SKTest
	{
		[SkippableFact]
		public void DrawShapedTextExtensionMethodDraws()
		{
			using (var bitmap = new SKBitmap(new SKImageInfo(512, 512)))
			using (var canvas = new SKCanvas(bitmap))
			using (var tf = SKTypeface.FromFile(Path.Combine(PathToFonts, "content-font.ttf")))
			using (var shaper = new SKShaper(tf))
			using (var paint = new SKPaint { IsAntialias = true, TextSize = 64, Typeface = tf })
			{
				canvas.Clear(SKColors.White);

				canvas.DrawShapedText(shaper, "متن", 100, 200, paint);

				canvas.Flush();

				Assert.Equal(SKColors.Black, bitmap.GetPixel(110, 210));
				Assert.Equal(SKColors.Black, bitmap.GetPixel(127, 196));
				Assert.Equal(SKColors.Black, bitmap.GetPixel(142, 197));
				Assert.Equal(SKColors.Black, bitmap.GetPixel(155, 195));
				Assert.Equal(SKColors.Black, bitmap.GetPixel(131, 181));
				Assert.Equal(SKColors.White, bitmap.GetPixel(155, 190));
				Assert.Equal(SKColors.White, bitmap.GetPixel(110, 200));
			}
		}

		[SkippableFact]
		public void CorrectlyShapesArabicScriptAtAnOffset()
		{
			var clusters = new uint[] { 4, 2, 0 };
			var codepoints = new uint[] { 629, 668, 891 };
			var points = new SKPoint[] { new SKPoint(100, 200), new SKPoint(128.25f, 200), new SKPoint(142, 200) };

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
			var clusters = new uint[] { 4, 2, 0 };
			var codepoints = new uint[] { 629, 668, 891 };
			var points = new SKPoint[] { new SKPoint(0, 0), new SKPoint(28.25f, 0), new SKPoint(42, 0) };

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

		[SkippableFact]
		public void CanCreateFaceShaperFromTypeface()
		{
			var skiaTypeface = SKTypeface.FromFile(Path.Combine(PathToFonts, "content-font.ttf"));

			var clusters = new uint[] { 4, 2, 0 };
			var codepoints = new uint[] { 629, 668, 891 };

			using (var face = new Face(GetFaceBlob, () => skiaTypeface.Dispose()))
			using (var font = new Font(face))
			using (var buffer = new HarfBuzzSharp.Buffer())
			{
				buffer.AddUtf8("متن");
				buffer.GuessSegmentProperties();

				font.Shape(buffer);

				Assert.Equal(clusters, buffer.GlyphInfos.Select(i => i.Cluster));
				Assert.Equal(codepoints, buffer.GlyphInfos.Select(i => i.Codepoint));
			}

			Blob GetFaceBlob(Face face, Tag tag)
			{
				var size = skiaTypeface.GetTableSize(tag);
				var data = Marshal.AllocCoTaskMem(size);
				skiaTypeface.TryGetTableData(tag, 0, size, data);
				return new Blob(data, size, MemoryMode.Writeable, () => Marshal.FreeCoTaskMem(data));
			}
		}

		[SkippableTheory]
		[InlineData(SKTextAlign.Left, 300)]
		[InlineData(SKTextAlign.Center, 162)]
		[InlineData(SKTextAlign.Right, 23)]
		public void TextAlignMovesTextPosition(SKTextAlign align, int offset)
		{
			var font = Path.Combine(PathToFonts, "segoeui.ttf");
			using var tf = SKTypeface.FromFile(font);

			using var bitmap = new SKBitmap(600, 200);
			using var canvas = new SKCanvas(bitmap);

			canvas.Clear(SKColors.White);

			using var paint = new SKPaint();
			paint.Typeface = tf;
			paint.IsAntialias = true;
			paint.TextSize = 64;
			paint.Color = SKColors.Black;
			paint.TextAlign = align;

			canvas.DrawShapedText("SkiaSharp", 300, 100, paint);

			AssertTextAlign(bitmap, offset, 0);
		}

		private static void AssertTextAlign(SKBitmap bitmap, int x, int y)
		{
			// [S]kia[S]har[p]

			Assert.Equal(SKColors.Black, bitmap.GetPixel(x + 6, y + 66));
			Assert.Equal(SKColors.Black, bitmap.GetPixel(x + 28, y + 87));
			Assert.Equal(SKColors.White, bitmap.GetPixel(x + 28, y + 66));
			Assert.Equal(SKColors.White, bitmap.GetPixel(x + 6, y + 87));

			Assert.Equal(SKColors.Black, bitmap.GetPixel(x + 120, y + 66));
			Assert.Equal(SKColors.Black, bitmap.GetPixel(x + 142, y + 87));
			Assert.Equal(SKColors.White, bitmap.GetPixel(x + 142, y + 66));
			Assert.Equal(SKColors.White, bitmap.GetPixel(x + 120, y + 87));

			Assert.Equal(SKColors.Black, bitmap.GetPixel(x + 246, y + 70));
			Assert.Equal(SKColors.Black, bitmap.GetPixel(x + 246, y + 113));
			Assert.Equal(SKColors.Black, bitmap.GetPixel(x + 271, y + 83));
			Assert.Equal(SKColors.White, bitmap.GetPixel(x + 258, y + 83));
			Assert.Equal(SKColors.White, bitmap.GetPixel(x + 258, y + 113));
		}
	}
}
