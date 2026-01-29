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
	}
}