using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

using SkiaSharp.Extended;

namespace SkiaSharp.Tests
{
	public class SKGeometryTest : SKTest
	{
		[Test]
		public void GeometryGeneratesRectPath()
		{
			var rectPath = SKGeometry.CreateTrianglePath(100);

			Assert.AreEqual(3, rectPath.PointCount);
		}
	}
}
