using HarfBuzzSharp;

using Xunit;

namespace SkiaSharp.Tests
{
	public class HbScriptTest : SKTest
	{
		[SkippableFact]
		public void ShouldCreateScriptFromString()
		{
			var script = Script.FromString("Latn");

			Assert.Equal(Script.LATIN, script);
		}
	}
}
