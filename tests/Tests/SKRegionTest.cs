using System;
using System.Collections.Generic;
using Xunit;

namespace SkiaSharp.Tests
{
	public class SKRegionTest : SKTest
	{
		[SkippableFact]
		public void FromPathWithoutClipDoesNotCreateEmptyRegion()
		{
			var path = new SKPath();
			path.AddRect(SKRect.Create(10, 20, 30, 40));

			var region = new SKRegion(path);

			Assert.NotEqual(SKRectI.Empty, region.Bounds);
			Assert.Equal(SKRectI.Truncate(path.Bounds), region.Bounds);
		}

		[SkippableFact]
		public void SetPathWithoutClipDoesNotCreateEmptyRegion()
		{
			var path = new SKPath();
			path.AddRect(SKRect.Create(10, 20, 30, 40));

			var region = new SKRegion();
			var isNonEmpty = region.SetPath(path);

			Assert.True(isNonEmpty);
			Assert.Equal(SKRectI.Truncate(path.Bounds), region.Bounds);
		}

		[SkippableFact]
		public void SetPathWithEmptyClipDoesCreatesEmptyRegion()
		{
			var path = new SKPath();
			path.AddRect(SKRect.Create(10, 20, 30, 40));

			var region = new SKRegion();
			var isNonEmpty = region.SetPath(path, new SKRegion());

			Assert.False(isNonEmpty);
			Assert.Equal(SKRectI.Empty, region.Bounds);
		}

		[SkippableFact]
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

			Assert.True(isNonEmpty);
			Assert.Equal(SKRectI.Intersect(clipRect, rect), region.Bounds);
		}

		[SkippableFact]
		public void EmptyRegionDoesNotIntersectsWithRectI()
		{
			var region = new SKRegion();

			var rect = new SKRectI(10, 20, 30, 40);

			Assert.False(region.Intersects(rect));
		}

		// This was added for https://github.com/mono/SkiaSharp/issues/770
		[SkippableFact]
		public void RegionIntersectsWithRectI()
		{
			var region = new SKRegion();
			region.SetRect(new SKRectI(25, 25, 50, 50));

			var rect = new SKRectI(10, 20, 30, 40);

			Assert.True(region.Intersects(rect));
		}
	}
}
