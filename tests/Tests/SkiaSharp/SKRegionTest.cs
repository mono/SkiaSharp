using Xunit;

namespace SkiaSharp.Tests
{
	public class SKRegionTest : SKTest
	{
		[SkippableFact]
		public void EmptyRegionIsEmpty()
		{
			using var region = new SKRegion();

			Assert.True(region.IsEmpty);
			Assert.False(region.IsRect);
			Assert.False(region.IsComplex);
		}

		[SkippableFact]
		public void RectRegionIsRect()
		{
			using var region = new SKRegion(SKRectI.Create(10, 10, 100, 100));

			Assert.False(region.IsEmpty);
			Assert.True(region.IsRect);
			Assert.False(region.IsComplex);
		}

		[SkippableFact]
		public void TwoRectRegionIsComplex()
		{
			using var region = new SKRegion(SKRectI.Create(10, 10, 100, 100));
			region.Op(SKRectI.Create(50, 50, 100, 100), SKRegionOperation.Union);

			Assert.False(region.IsEmpty);
			Assert.False(region.IsRect);
			Assert.True(region.IsComplex);
		}

		[SkippableFact]
		public void QuickContainsIsCorrect()
		{
			using var region = new SKRegion(SKRectI.Create(10, 10, 100, 100));

			Assert.False(region.QuickContains(SKRectI.Create(50, 50, 100, 100)));
			Assert.True(region.QuickContains(SKRectI.Create(20, 20, 50, 50)));
			Assert.False(region.QuickContains(SKRectI.Create(200, 20, 50, 50)));
		}

		[SkippableFact]
		public void QuickRejectIsCorrect()
		{
			using var region = new SKRegion(SKRectI.Create(10, 10, 100, 100));

			Assert.False(region.QuickReject(SKRectI.Create(50, 50, 100, 100)));
			Assert.False(region.QuickReject(SKRectI.Create(20, 20, 50, 50)));
			Assert.True(region.QuickReject(SKRectI.Create(200, 20, 50, 50)));
		}

		[SkippableFact]
		public void GetBoundaryPathReturnsNotNullPath()
		{
			using var region = new SKRegion(SKRectI.Create(10, 10, 100, 100));
			region.Op(SKRectI.Create(50, 50, 100, 100), SKRegionOperation.Union);

			using var path = region.GetBoundaryPath();

			Assert.NotNull(path);
		}

		[SkippableFact]
		public void FromPathWithoutClipDoesNotCreateEmptyRegion()
		{
			using var path = new SKPath();
			path.AddRect(SKRect.Create(10, 20, 30, 40));

			using var region = new SKRegion(path);

			Assert.False(region.IsEmpty);
			Assert.NotEqual(SKRectI.Empty, region.Bounds);
			Assert.Equal(SKRectI.Truncate(path.Bounds), region.Bounds);
		}

		[SkippableFact]
		public void SetPathWithoutClipDoesNotCreateEmptyRegion()
		{
			using var path = new SKPath();
			path.AddRect(SKRect.Create(10, 20, 30, 40));

			using var region = new SKRegion();
			var isNonEmpty = region.SetPath(path);

			Assert.True(isNonEmpty);
			Assert.Equal(SKRectI.Truncate(path.Bounds), region.Bounds);
		}

		[SkippableFact]
		public void SetPathWithEmptyClipDoesCreatesEmptyRegion()
		{
			using var path = new SKPath();
			path.AddRect(SKRect.Create(10, 20, 30, 40));

			using var region = new SKRegion();
			var isNonEmpty = region.SetPath(path, new SKRegion());

			Assert.True(region.IsEmpty);
			Assert.False(isNonEmpty);
			Assert.Equal(SKRectI.Empty, region.Bounds);
		}

		[SkippableFact]
		public void SetPathWithClipDoesCreatesCorrectRegion()
		{
			var clipRect = new SKRectI(25, 25, 50, 50);
			using var clip = new SKRegion();
			clip.SetRect(clipRect);

			var rect = new SKRectI(10, 20, 30, 40);
			using var path = new SKPath();
			path.AddRect(rect);

			using var region = new SKRegion();
			var isNonEmpty = region.SetPath(path, clip);

			Assert.True(isNonEmpty);
			Assert.Equal(SKRectI.Intersect(clipRect, rect), region.Bounds);
		}

		[SkippableFact]
		public void EmptyRegionDoesNotIntersectsWithRectI()
		{
			using var region = new SKRegion();

			var rect = new SKRectI(10, 20, 30, 40);

			Assert.False(region.Intersects(rect));
		}

		// This was added for https://github.com/mono/SkiaSharp/issues/770
		[SkippableFact]
		public void RegionIntersectsWithRectI()
		{
			using var region = new SKRegion();
			region.SetRect(new SKRectI(25, 25, 50, 50));

			var rect = new SKRectI(10, 20, 30, 40);

			Assert.True(region.Intersects(rect));
		}

		[SkippableFact]
		public void ContainsReturnsFalseIfItDoesNotContain()
		{
			using var region = new SKRegion(new SKRectI(30, 30, 40, 40));
			using var region2 = new SKRegion(new SKRectI(25, 25, 50, 50));

			Assert.False(region.Contains(region2));
		}

		[SkippableFact]
		public void ContainsReturnsTrueIfItDoesContain()
		{
			using var region = new SKRegion(new SKRectI(25, 25, 50, 50));
			using var region2 = new SKRegion(new SKRectI(30, 30, 40, 40));

			Assert.True(region.Contains(region2));
		}

		[SkippableFact]
		public void ContainsReturnsFalseIfItDoesNotContainPoint()
		{
			using var region = new SKRegion(new SKRectI(30, 30, 40, 40));

			Assert.False(region.Contains(60, 60));
		}

		[SkippableFact]
		public void ContainsReturnsTrueIfItDoesContainPoint()
		{
			using var region = new SKRegion(new SKRectI(25, 25, 50, 50));

			Assert.True(region.Contains(40, 40));
		}

		[SkippableFact]
		public void QuickContainsReturnsTrueIfItDoesContain()
		{
			using var region = new SKRegion(new SKRectI(25, 25, 50, 50));

			Assert.True(region.QuickContains(new SKRectI(30, 30, 40, 40)));
		}

		[SkippableFact]
		public void RectIteratorHasCorrectRectsForEmpty()
		{
			using var region = new SKRegion();

			using var iterator = region.CreateRectIterator();

			Assert.False(iterator.Next(out var rect));
			Assert.Equal(SKRect.Empty, rect);

			Assert.False(iterator.Next(out rect));
			Assert.Equal(SKRect.Empty, rect);
		}

		[SkippableFact]
		public void RectIteratorHasCorrectRects()
		{
			var rectA = SKRectI.Create(10, 10, 100, 100);
			var rectB = SKRectI.Create(50, 50, 100, 100);

			using var region = new SKRegion(rectA);
			region.Op(rectB, SKRegionOperation.Union);

			using var iterator = region.CreateRectIterator();

			Assert.True(iterator.Next(out var rect));
			Assert.Equal(SKRectI.Create(10, 10, 100, 40), rect);

			Assert.True(iterator.Next(out rect));
			Assert.Equal(SKRectI.Create(10, 50, 140, 60), rect);

			Assert.True(iterator.Next(out rect));
			Assert.Equal(SKRectI.Create(50, 110, 100, 40), rect);

			Assert.False(iterator.Next(out rect));
			Assert.Equal(SKRect.Empty, rect);

			Assert.False(iterator.Next(out rect));
			Assert.Equal(SKRect.Empty, rect);
		}

		[SkippableFact]
		public void ClipIteratorHasCorrectRects()
		{
			var rectA = SKRectI.Create(10, 10, 100, 100);
			var rectB = SKRectI.Create(50, 50, 100, 100);

			using var region = new SKRegion(rectA);
			region.Op(rectB, SKRegionOperation.Union);

			using var iterator = region.CreateClipIterator(SKRectI.Create(5, 5, 65, 65));

			Assert.True(iterator.Next(out var rect));
			Assert.Equal(SKRectI.Create(10, 10, 100, 40), rect);

			Assert.True(iterator.Next(out rect));
			Assert.Equal(SKRectI.Create(10, 50, 140, 60), rect);

			Assert.False(iterator.Next(out rect));
			Assert.Equal(SKRect.Empty, rect);

			Assert.False(iterator.Next(out rect));
			Assert.Equal(SKRect.Empty, rect);
		}

		[SkippableFact]
		public void SpanIteratorHasCorrectIntercepts()
		{
			var rectA = SKRectI.Create(10, 10, 100, 100);
			var rectB = SKRectI.Create(50, 50, 100, 100);

			using var region = new SKRegion(rectA);
			region.Op(rectB, SKRegionOperation.Union);

			using var iterator = region.CreateSpanIterator(30, 5, 200);

			Assert.True(iterator.Next(out var left, out var right));
			Assert.Equal(10, left);
			Assert.Equal(110, right);

			Assert.False(iterator.Next(out left, out right));
			Assert.Equal(0, left);
			Assert.Equal(0, right);

			Assert.False(iterator.Next(out left, out right));
			Assert.Equal(0, left);
			Assert.Equal(0, right);
		}
	}
}
