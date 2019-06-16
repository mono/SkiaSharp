using System;

using Xunit;

namespace HarfBuzzSharp.Tests
{
	public class HBFeatureTest : HBTest
	{
		[SkippableFact]
		public void ShouldCreateFeatureFromString()
		{
			var feature = Feature.Parse("Kern");

			Assert.Equal(new Tag("Kern"), feature.Tag);
		}

		[SkippableFact]
		public void ShouldThrowFromUnknownString()
		{
			Assert.False(Feature.TryParse("", out var script));
			Assert.Equal(Tag.None, script.Tag);
			Assert.Throws<FormatException>(() => Feature.Parse(""));
		}
	}
}
