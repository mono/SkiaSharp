using Xunit;

namespace SkiaSharp.Tests
{
	public class SKImageFilterTest : SKTest
	{
		[SkippableFact]
		public void MergeFilterAcceptsNullFilterArray()
		{
			var filter = SKImageFilter.CreateMerge(new SKImageFilter[] { null });
			Assert.NotNull(filter);
		}

		[SkippableFact]
		public void MergeFilterAcceptsNullParams()
		{
			var filter = SKImageFilter.CreateMerge((SKImageFilter)null, null);
			Assert.NotNull(filter);
		}

		[SkippableFact]
		public void ShaderFilterAcceptsNullParams()
		{
			var filter = SKImageFilter.CreateShader(null);
			Assert.NotNull(filter);
		}
	}
}
