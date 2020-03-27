using System;
using System.Runtime.InteropServices;
using Xunit;

namespace SkiaSharp.Tests
{
	public class SKImageFilterTest : SKTest
	{
		[SkippableFact]
		public void MergeFilterAcceptsSourceGraphicInputs()
		{
			var filter = SKImageFilter.CreateMerge(new SKImageFilter[] {null});
			Assert.NotNull(filter);
		}
	}
}
