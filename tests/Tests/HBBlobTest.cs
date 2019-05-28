using System.IO;

using HarfBuzzSharp;

using Xunit;

namespace SkiaSharp.Tests
{
	public class HBBlobTest : SKTest
	{
		[SkippableFact]
		public void ShouldCreateFromFileName()
		{
			using (var blob = Blob.FromFile(Path.Combine(PathToFonts, "Distortable.ttf")))
			{
				Assert.Equal(16384, blob.Length);
			}
		}

		[SkippableFact]
		public void ShouldCreateFromStream()
		{
			using (var blob = Blob.FromStream(File.Open(Path.Combine(PathToFonts, "Funkster.ttf"), FileMode.Open, FileAccess.Read)))
			{
				Assert.Equal(236808, blob.Length);
			}
		}
	}
}
