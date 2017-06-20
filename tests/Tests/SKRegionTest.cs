using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace SkiaSharp.Tests
{
	public class SKRegionTest : SKTest
	{
		[Test]
		public void SetPathWithoutClipDoesNotCreateEmptyRegion()
		{
			var path = new SKPath();
			path.AddRect(SKRect.Create(10, 20, 30, 40));

			var region = new SKRegion();
			var isNonEmpty = region.SetPath(path);

			Assert.IsTrue(isNonEmpty);
			Assert.AreEqual(SKRectI.Truncate(path.Bounds), region.Bounds);
		}

		[Test]
		public void SetPathWithEmptyClipDoesCreatesEmptyRegion()
		{
			var path = new SKPath();
			path.AddRect(SKRect.Create(10, 20, 30, 40));

			var region = new SKRegion();
			var isNonEmpty = region.SetPath(path, new SKRegion());

			Assert.IsFalse(isNonEmpty);
			Assert.AreEqual(SKRectI.Empty, region.Bounds);
		}

		[Test]
		public void SetPathWithClipDoesCreatesCorrectRegion()
		{
			var clipRect = new SKRectI(25, 25, 50, 50);
			var clip = new SKRegion();
			clip.SetRect(clipRect);

			var rect = new SKRectI(10, 20, 30, 40);
			var path = new SKPath();
			path.AddRect(rect);

			var region = new SKRegion();
			var isNonEmpty = region.SetPath(path, clip);

			Assert.IsTrue(isNonEmpty);
			Assert.AreEqual(SKRectI.Intersect(clipRect, rect), region.Bounds);
		}
	}
}
