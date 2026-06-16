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
	}
}
