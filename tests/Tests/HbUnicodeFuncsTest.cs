namespace SkiaSharp.Tests
{
	using HarfBuzzSharp;

	using Xunit;

	public class HbUnicodeFuncsTest : SKTest
	{
		[SkippableFact]
		public void ShouldFindScript()
		{
			using (var unicodeFuncs = UnicodeFunctions.Default)
			{
				var script = unicodeFuncs.GetScript('A');

				Assert.Equal(Script.LATIN, script);
			}
		}
	}
}
