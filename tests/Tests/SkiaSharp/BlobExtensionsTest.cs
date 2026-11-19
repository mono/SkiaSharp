using System.IO;

using SkiaSharp.Tests;

using Xunit;

namespace SkiaSharp.HarfBuzz.Tests
{
	public class BlobExtensionsTest : SKTest
	{
		[Fact]
		public void ToHarfBuzzBlobDisposesAssetWithoutMemoryBaseWhenBlobDisposed()
		{
			var bytes = File.ReadAllBytes(Path.Combine(PathToFonts, "content-font.ttf"));

			// A managed stream reports no native memory base, so ToHarfBuzzBlob
			// takes the copy (else) branch that must still own and dispose the asset.
			var asset = new SKManagedStream(new MemoryStream(bytes), true);

			Assert.Equal(nint.Zero, asset.GetMemoryBase());

			var blob = asset.ToHarfBuzzBlob();

			Assert.False(asset.IsDisposed);

			blob.Dispose();

			// Disposing the blob must release its owning asset; otherwise the
			// native SKStreamAsset leaks (only the finalizer would ever free it).
			Assert.True(asset.IsDisposed);
		}
	}
}
