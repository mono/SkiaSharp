using System.Collections.Concurrent;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using SkiaSharp.Tests;
using Xunit;

namespace SkiaSharp.HarfBuzz.Tests
{
	public class CanvasExtensionTest : SKTest
	{
		private SKCanvas canvas;
		private SKFont font;
		private SKPaint paint;

		#region Access private fields

		FieldInfo? shaperCacheField = null;
		public ConcurrentDictionary<int, (SKShaper shaper, DateTime cachedAt)> ShaperCache
		{
			get
			{
				if (shaperCacheField is null)
					shaperCacheField = typeof(CanvasExtensions).GetField("shaperCache", BindingFlags.NonPublic | BindingFlags.Static);

				return (ConcurrentDictionary<int, (SKShaper shaper, DateTime cachedAt)>)shaperCacheField!.GetValue(null)!;
			}
		}

		FieldInfo? shapeResultCacheField = null;
		public ConcurrentDictionary<int, (SKShaper.Result shapeResult, DateTime cachedAt)> ShapeResultCache
		{
			get
			{
				if (shapeResultCacheField is null)
					shapeResultCacheField = typeof(CanvasExtensions).GetField("shapeResultCache", BindingFlags.NonPublic | BindingFlags.Static);

				return (ConcurrentDictionary<int, (SKShaper.Result shapeResult, DateTime cachedAt)>)shapeResultCacheField!.GetValue(null)!;
			}
		}

		#endregion Access private fields


		public CanvasExtensionTest()
		{
			canvas = new SKCanvas(new SKBitmap(320, 200));
			font = SKTypeface.FromFile(Path.Combine(PathToFonts, "content-font.ttf")).ToFont(20);
			paint = new SKPaint() { Color = SKColors.LimeGreen, IsAntialias = true, IsStroke = false };
		}

		[SkippableFact]
		public void CacheGetsFilled()
		{
			canvas.SetShaperCacheDuration(0);
			canvas.SetShaperCacheDuration(30_000);

			Assert.Empty(ShaperCache);
			Assert.Empty(ShapeResultCache);

			canvas.DrawShapedText("Hello world!", 0, 0, font, paint);

			Assert.Single(ShaperCache);
			Assert.Single(ShapeResultCache);

			canvas.SetShaperCacheDuration(0);
		}

		[SkippableFact]
		public async Task CacheGetsClearedWhenSettingDurationToZero()
		{
			canvas.SetShaperCacheDuration(0);
			canvas.SetShaperCacheDuration(30_000);

			canvas.DrawShapedText("Hello world!", 0, 0, font, paint);

			Assert.Single(ShaperCache);
			Assert.Single(ShapeResultCache);

			canvas.SetShaperCacheDuration(0);
			await Task.Delay(10);		// give Timer a chance to run

			Assert.Empty(ShaperCache);
			Assert.Empty(ShapeResultCache);
		}

		[SkippableFact]
		public async Task CacheGetsClearedAutomatically()
		{
			canvas.SetShaperCacheDuration(0);
			canvas.SetShaperCacheDuration(30);

			canvas.DrawShapedText("Hello world!", 0, 0, font, paint);

			Assert.Single(ShaperCache);
			Assert.Single(ShapeResultCache);

			await Task.Delay(50);

			Assert.Empty(ShaperCache);
			Assert.Empty(ShapeResultCache);

			canvas.SetShaperCacheDuration(0);
		}

		[SkippableFact]
		public void TwoStringsGetBothCached()
		{
			canvas.SetShaperCacheDuration(0);
			canvas.SetShaperCacheDuration(30_000);

			Assert.Empty(ShaperCache);
			Assert.Empty(ShapeResultCache);

			canvas.DrawShapedText("Hello", 0, 0, font, paint);
			canvas.DrawShapedText("world!", 100, 100, font, paint);

			Assert.Single(ShaperCache);
			Assert.Equal(2, ShapeResultCache.Count);

			canvas.SetShaperCacheDuration(0);
		}

		[SkippableFact]
		public void ShaperGetsCachedForDifferentFonts()
		{
			var font2 = SKTypeface.FromFile(Path.Combine(PathToFonts, "segoeui.ttf")).ToFont(20);

			canvas.SetShaperCacheDuration(0);
			canvas.SetShaperCacheDuration(30_000);

			Assert.Empty(ShaperCache);
			Assert.Empty(ShapeResultCache);

			canvas.DrawShapedText("Hello", 0, 0, font, paint);

			Assert.Single(ShaperCache);
			Assert.Single(ShapeResultCache);

			canvas.DrawShapedText("world!", 100, 0, font2, paint);

			Assert.Equal(2, ShaperCache.Count);
			Assert.Equal(2, ShapeResultCache.Count);

			canvas.SetShaperCacheDuration(0);
		}
	}
}
