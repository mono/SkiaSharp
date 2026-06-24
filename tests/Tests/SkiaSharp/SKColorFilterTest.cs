using System;
using System.IO;
using System.Linq;
using Xunit;

namespace SkiaSharp.Tests
{
	public class SKColorFilterTest : SKTest
	{
		[Fact]
		[Trait(Traits.Category.Key, Traits.Category.Values.Smoke)]
		public void StaticSrgbToLinearIsReturnedAsTheStaticInstance()
		{
			var expected = SKColorFilter.CreateSrgbToLinearGamma();
			var handle = SkiaApi.sk_colorfilter_new_srgb_to_linear_gamma();
			try
			{
				var cs = SKColorFilter.GetObject(handle);
				Assert.Same(expected, cs);
				Assert.True(cs.IgnorePublicDispose);
			}
			finally
			{
				SkiaApi.sk_refcnt_safe_unref(handle);
			}
		}

		[Fact]
		public void StaticLinearToSrgbIsReturnedAsTheStaticInstance()
		{
			var expected = SKColorFilter.CreateLinearToSrgbGamma();
			var handle = SkiaApi.sk_colorfilter_new_linear_to_srgb_gamma();
			try
			{
				var cs = SKColorFilter.GetObject(handle);
				Assert.Same(expected, cs);
				Assert.True(cs.IgnorePublicDispose);
			}
			finally
			{
				SkiaApi.sk_refcnt_safe_unref(handle);
			}
		}

		[Theory]
		[InlineData(-1, 0)]
		[InlineData(-0.5f, 0)]
		[InlineData(0, 0)]
		[InlineData(0.5f, -1)]
		[InlineData(1, 1)]
		[InlineData(1.5f, 1)]
		public void LerpReturnsCorrectFilter(float weight, int returned)
		{
			var first = SKColorFilter.CreateBlendMode(SKColors.Red, SKBlendMode.SrcOver);
			var second = SKColorFilter.CreateBlendMode(SKColors.Blue, SKBlendMode.SrcOver);
			var filters = new[] { first, second };

			var lerp = SKColorFilter.CreateLerp(weight, first, second);

			Assert.Equal(returned, Array.IndexOf(filters, lerp));
		}

		[Fact]
		public void CreateOverdrawWithValidColorsReturnsFilter()
		{
			var colors = new SKColor[]
			{
				new SKColor(0x00000000),
				new SKColor(0x5A3F0099),
				new SKColor(0x5A2D8F0F),
				new SKColor(0x5ABFBF00),
				new SKColor(0x5ABF6600),
				new SKColor(0x5AFF0000)
			};

			using var filter = SKColorFilter.CreateOverdraw(colors);

			Assert.NotNull(filter);
		}

		[Fact]
		public void CreateOverdrawWithWrongLengthThrows()
		{
			var colors = new SKColor[] { SKColors.Red, SKColors.Blue };

			Assert.Throws<ArgumentException>(() => SKColorFilter.CreateOverdraw(colors));
		}

		[Fact]
		public void CreateOverdrawWithNullArrayThrows()
		{
			SKColor[] colors = null;

			Assert.Throws<ArgumentNullException>(() => SKColorFilter.CreateOverdraw(colors));
		}

		[Fact]
		public void CreateOverdrawSpanOverloadWorks()
		{
			var colors = new SKColor[]
			{
				new SKColor(0x00000000),
				new SKColor(0x5A3F0099),
				new SKColor(0x5A2D8F0F),
				new SKColor(0x5ABFBF00),
				new SKColor(0x5ABF6600),
				new SKColor(0x5AFF0000)
			};

			using var filter1 = SKColorFilter.CreateOverdraw(colors);
			using var filter2 = SKColorFilter.CreateOverdraw(colors.AsSpan());

			Assert.NotNull(filter1);
			Assert.NotNull(filter2);
		}

		[Fact]
		public void OverdrawFilterRendersCorrectly()
		{
			var overdrawColors = new SKColor[]
			{
				new SKColor(0x00000000), // transparent - 0 draws
				new SKColor(0x5A3F0099), // blue - 1 draw
				new SKColor(0x5A2D8F0F), // green - 2 draws
				new SKColor(0x5ABFBF00), // yellow - 3 draws
				new SKColor(0x5ABF6600), // orange - 4 draws
				new SKColor(0x5AFF0000)  // red - 5+ draws
			};

			using var filter = SKColorFilter.CreateOverdraw(overdrawColors);
			Assert.NotNull(filter);

			// Create a surface to capture overdraw counts
			var info = new SKImageInfo(100, 100);
			using var surface = SKSurface.Create(info);
			Assert.NotNull(surface);
			
			using var canvas = surface.Canvas;
			canvas.Clear(SKColors.Transparent);

			// Wrap in overdraw canvas - this tracks draw counts per pixel
			using var overdrawCanvas = new SKOverdrawCanvas(canvas);
			using var paint = new SKPaint();

			// Draw at different locations with known overdraw counts
			// Area at (10, 10) - drawn once
			overdrawCanvas.DrawRect(new SKRect(10, 10, 30, 30), paint);

			// Area at (50, 10) - drawn twice (overlapping)
			overdrawCanvas.DrawRect(new SKRect(50, 10, 70, 30), paint);
			overdrawCanvas.DrawRect(new SKRect(50, 10, 70, 30), paint);

			// Area at (10, 50) - drawn three times
			for (int i = 0; i < 3; i++)
				overdrawCanvas.DrawRect(new SKRect(10, 50, 30, 70), paint);

			// Now the surface alpha channel has overdraw counts
			// Apply the overdraw color filter to visualize
			using var snapshot = surface.Snapshot();
			Assert.NotNull(snapshot);
			
			using var resultSurface = SKSurface.Create(info);
			Assert.NotNull(resultSurface);
			
			using var resultCanvas = resultSurface.Canvas;
			using var filterPaint = new SKPaint { ColorFilter = filter };
			
			// Use modern DrawImage overload with SKSamplingOptions
			resultCanvas.DrawImage(snapshot, 0, 0, SKSamplingOptions.Default, filterPaint);

			// Read pixels and verify the filter was applied
			using var pixmap = resultSurface.PeekPixels();
			Assert.NotNull(pixmap);

			// Verify that pixels are not all transparent (filter was applied)
			var pixel1 = pixmap.GetPixelColor(20, 20); // 1 draw - should be blue-ish
			var pixel2 = pixmap.GetPixelColor(60, 20); // 2 draws - should be green-ish
			var pixel3 = pixmap.GetPixelColor(20, 60); // 3 draws - should be yellow-ish

			// Verify colors are not transparent (filter is working)
			Assert.NotEqual(SKColors.Transparent, pixel1);
			Assert.NotEqual(SKColors.Transparent, pixel2);
			Assert.NotEqual(SKColors.Transparent, pixel3);
			
			// Verify the colors are different from each other (overdraw mapping works)
			Assert.NotEqual(pixel1, pixel2);
			Assert.NotEqual(pixel2, pixel3);
		}
	}
}
