using System.IO;
using HarfBuzzSharp;
using SkiaSharp.Tests;
using Xunit;

namespace SkiaSharp.HarfBuzz.Tests
{
	public class BlobExtensionsTest : SKTest
	{
		[SkippableFact]
		public void ToHarfBuzzBlobDoesNotDisposeStream()
		{
			var fontFileName = Path.Combine(PathToFonts, "content-font.ttf");
			using var tf = SKTypeface.FromFile(fontFileName);
			using var stream = tf.OpenStream(out var index);
			using (var blob = stream.ToHarfBuzzBlob())
			{
			}

			Assert.False(stream.IsDisposed);
		}

		[SkippableFact]
		public void ToHarfBuzzBlobBlobCanBeUsedOutsideStreamLifetime()
		{
			static Blob GetBlob(SKTypeface typeface)
			{
				using var stream = typeface.OpenStream(out var index);
				return stream.ToHarfBuzzBlob();
			}

			var fontFile = Path.Combine(PathToFonts, "content-font.ttf");
			using var tf = SKTypeface.FromFile(fontFile);
			using var blob = GetBlob(tf);
			Assert.Equal(1, blob.FaceCount);
		}
	}
}