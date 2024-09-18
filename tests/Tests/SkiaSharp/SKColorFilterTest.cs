using System;
using System.IO;
using System.Linq;
using Xunit;

namespace SkiaSharp.Tests
{
	public class SKColorFilterTest : SKTest
	{
		[SkippableFact]
		public void StaticSrgbToLinearIsReturnedAsTheStaticInstance()
		{
			var handle = SkiaApi.sk_colorfilter_new_srgb_to_linear_gamma();
			try
			{
				var cs = SKColorFilter.GetObject(handle);
				Assert.Equal("SKColorFilterStatic", cs.GetType().Name);
			}
			finally
			{
				SkiaApi.sk_refcnt_safe_unref(handle);
			}
		}

		[SkippableFact]
		public void StaticLinearToSrgbIsReturnedAsTheStaticInstance()
		{
			var handle = SkiaApi.sk_colorfilter_new_linear_to_srgb_gamma();
			try
			{
				var cs = SKColorFilter.GetObject(handle);
				Assert.Equal("SKColorFilterStatic", cs.GetType().Name);
			}
			finally
			{
				SkiaApi.sk_refcnt_safe_unref(handle);
			}
		}

		[SkippableTheory]
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
