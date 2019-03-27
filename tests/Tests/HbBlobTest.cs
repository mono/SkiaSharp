using System.IO;

using HarfBuzzSharp;

using Xunit;

namespace SkiaSharp.Tests
{
	public class HbBlobTest : SKTest
	{
		[SkippableFact]
		public void ShouldCreateFromFileName()
		{
			using (var blob = Blob.FromFile(Path.Combine(PathToFonts, "content-font.ttf")))
			{
				Assert.Equal(246224, blob.Length);
			}
		}

		[SkippableFact]
		public void ShouldCreateFromStream()
		{
			using (var blob = Blob.FromStream(File.Open(Path.Combine(PathToFonts, "content-font.ttf"), FileMode.Open, FileAccess.Read)))
			{
				Assert.Equal(246224, blob.Length);
			}
		}
	}
}
