using System;
using Xunit;

namespace SkiaSharp.Tests
{
	public class SKPathMeasureTest : SKTest
	{
		[Fact]
		public void ConstructorThrowsOnNullPathArgument()
		{
			var ex = Assert.Throws<ArgumentNullException>(() => new SKPathMeasure(null));
			Assert.Equal("path", ex.ParamName);
		}

		[Fact]
		public void ConstructorDoesNotThrownOnNonNullPathArgument()
		{
			var path = new SKPath();
			var pm = new SKPathMeasure(path);
			Assert.NotNull(pm);
		}

		[Fact]
		public void EmptyConstructorDoesNotThrow()
		{
			var pm = new SKPathMeasure();
			Assert.NotNull(pm);
		}
	}
}
