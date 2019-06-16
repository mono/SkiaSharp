using HarfBuzzSharp;

using Xunit;

namespace SkiaSharp.Tests
{
	public class HBScriptTest : SKTest
	{
		[SkippableFact]
		public void ShouldCreateScriptFromString()
		{
			var script = Script.FromString("Latn");

			Assert.Equal(Script.Latin, script);
		}
	}
}
