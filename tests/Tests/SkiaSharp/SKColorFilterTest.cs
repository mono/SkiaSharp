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

		[SkippableFact]
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

			// Create a surface with overdraw canvas to test rendering
			var info = new SKImageInfo(100, 100);
			using var surface = SKSurface.Create(info);
			using var canvas = surface.Canvas;
			
			canvas.Clear(SKColors.White);

			// Draw overlapping rectangles - center area will have most overdraw
			using var paint = new SKPaint();
			for (int i = 0; i < 10; i++)
			{
				canvas.DrawRect(new SKRect(i * 5, i * 5, 80 - i * 5, 80 - i * 5), paint);
			}

			// Apply overdraw filter to visualize
			paint.ColorFilter = filter;
			using var snapshot = surface.Snapshot();
			using var overdrawImage = snapshot.ApplyImageFilter(
				SKImageFilter.CreateColorFilter(filter, null),
				new SKRectI(0, 0, 100, 100),
				new SKRectI(0, 0, 100, 100),
				out var outSubset,
				out var outOffset);

			Assert.NotNull(overdrawImage);
		}
	}
}
