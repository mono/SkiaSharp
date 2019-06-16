using System;

using Xunit;

namespace HarfBuzzSharp.Tests
{
	public class HBScriptTest : HBTest
	{
		[SkippableFact]
		public void ShouldCreateScriptFromString()
		{
			var script = Script.Parse("Latn");

			Assert.Equal(Script.Latin, script);
		}

		[SkippableFact]
		public void ShouldThrowFromUnknownString()
		{
			Assert.False(Script.TryParse("", out var script));
			Assert.Equal(Script.Invalid, script);
			Assert.Throws<FormatException>(() => Script.Parse(""));
		}
	}
}
