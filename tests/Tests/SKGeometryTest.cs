using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

using SkiaSharp.Extended;

namespace SkiaSharp.Tests
{
	public class SKGeometryTest : SKTest
	{
		[Fact]
		public void GeometryGeneratesRectPath()
		{
			var rectPath = SKGeometry.CreateTrianglePath(100);

			Assert.Equal(3, rectPath.PointCount);
		}
	}
}
