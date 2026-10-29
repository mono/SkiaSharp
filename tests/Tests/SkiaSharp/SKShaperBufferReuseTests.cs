using System;
using System.IO;
using SkiaSharp.Tests;
using Xunit;

namespace SkiaSharp.HarfBuzz.Tests
{
	public class SKShaperBufferReuseTests : SKTest
	{
		[Fact]
		public void RepeatedShapesMatchFreshShaperResults()
		{
			using var typeface = SKTypeface.FromFile(Path.Combine(PathToFonts, "content-font.ttf"));
			using var font = new SKFont { Size = 64, Typeface = typeface };
			using var reusedShaper = new SKShaper(typeface);

			var cases = new[] {
				("متن", 0f, 0f),
				("The quick brown fox.", 12.5f, -7.25f),
				("متن متن متن", 100f, 200f),
				("a", -0f, float.Epsilon),
				("office", float.MaxValue, float.MinValue),
				(" متن ", -123.5f, 456.25f),
			};

			foreach (var (text, x, y) in cases)
			{
				using var freshShaper = new SKShaper(typeface);
				var expected = freshShaper.Shape(text, x, y, font);
				var actual = reusedShaper.Shape(text, x, y, font);

				Assert.Equal(expected.Codepoints, actual.Codepoints);
				Assert.Equal(expected.Clusters, actual.Clusters);
				Assert.Equal(expected.Points.Length, actual.Points.Length);
				for (var i = 0; i < expected.Points.Length; i++)
				{
					Assert.Equal(BitConverter.SingleToInt32Bits(expected.Points[i].X), BitConverter.SingleToInt32Bits(actual.Points[i].X));
					Assert.Equal(BitConverter.SingleToInt32Bits(expected.Points[i].Y), BitConverter.SingleToInt32Bits(actual.Points[i].Y));
				}
				Assert.Equal(BitConverter.SingleToInt32Bits(expected.Width), BitConverter.SingleToInt32Bits(actual.Width));
			}
		}
	}
}
