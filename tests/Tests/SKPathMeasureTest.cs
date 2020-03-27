using System;
using Xunit;

namespace SkiaSharp.Tests
{
	public class SKPathMeasureTest : SKTest
	{
		[SkippableFact]
		public void ConstructorThrowsOnNullPathArgument()
		{
			Assert.Throws<ArgumentNullException>(() => new SKPathMeasure(null));
		}
	}
}
