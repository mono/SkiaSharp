using HarfBuzzSharp;

using Xunit;

namespace SkiaSharp.Tests
{
	public class HBFeatureTest : SKTest
	{
		[SkippableFact]
		public void ShouldCreateFeatureFromString()
		{
			var feature = Feature.FromString("Kern");

			Assert.Equal(new Tag("Kern"), feature.Tag);
		}
	}
}
