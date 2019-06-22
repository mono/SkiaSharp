using System;
using System.IO;
using System.Linq;
using Xunit;

namespace SkiaSharp.Tests
{
	public class SKColorSpaceTest : SKTest
	{
		[SkippableFact]
		public void CanCreateSrgb()
		{
			var colorspace = SKColorSpace.CreateSrgb();

			Assert.NotNull(colorspace);
			Assert.True(SKColorSpace.Equal(colorspace, colorspace));
		}

		[SkippableFact]
		public void ImageInfoHasColorSpace()
		{
			var colorspace = SKColorSpace.CreateSrgb();

			var info = new SKImageInfo(100, 100, SKImageInfo.PlatformColorType, SKAlphaType.Premul, colorspace);
			Assert.Equal(colorspace, info.ColorSpace);

			var image = SKImage.Create(info);
			Assert.Equal(colorspace, image.PeekPixels().ColorSpace);
		}

		[SkippableFact]
		public void SrgbColorsSpaceIsNamedSrgb()
		{
			var colorspace = SKColorSpace.CreateSrgb();

			Assert.Equal(SKNamedGamma.Srgb, colorspace.NamedGamma);
			Assert.Equal(SKColorSpaceType.Rgb, colorspace.Type);
		}

		[SkippableFact]
		public void AdobeRGB1998IsRGB()
		{
			var icc = Path.Combine(PathToImages, "AdobeRGB1998.icc");

			var colorspace = SKColorSpace.CreateIcc(File.ReadAllBytes(icc));

			Assert.Equal(SKNamedGamma.TwoDotTwoCurve, colorspace.NamedGamma);
			Assert.Equal(SKColorSpaceType.Rgb, colorspace.Type);

			var fnValues = new[] { 2.2f, 1f, 0f, 0f, 0f, 0f, 0f };
			Assert.True(colorspace.GetNumericalTransferFunction(out var fn));
			Assert.Equal(fnValues, fn.Values);

			var toXYZ = new[]
			{
				0.60974f, 0.20528f, 0.14919f, 0f,
				0.31111f, 0.62567f, 0.06322f, 0f,
				0.01947f, 0.06087f, 0.74457f, 0f,
				0f, 0f, 0f, 1f,
			};
			AssertMatrix(toXYZ, colorspace.ToXyzD50());

			var fromXYZ = new[]
			{
				1.96253f, -0.61068f, -0.34137f, 0f,
				-0.97876f, 1.91615f, 0.03342f, 0f,
				0.02869f, -0.14067f, 1.34926f, 0f,
				0f, 0f, 0f, 1f,
			};
			AssertMatrix(fromXYZ, colorspace.FromXyzD50());
		}

		[SkippableFact]
		public void USWebCoatedSWOPIsCMYK()
		{
			var icc = Path.Combine(PathToImages, "USWebCoatedSWOP.icc");

			var colorspace = SKColorSpace.CreateIcc(File.ReadAllBytes(icc));

			Assert.Equal(SKNamedGamma.NonStandard, colorspace.NamedGamma);
			Assert.Equal(SKColorSpaceType.Cmyk, colorspace.Type);

			var fnValues = new[] { 0f, 0f, 0f, 0f, 0f, 0f, 0f };
			Assert.False(colorspace.GetNumericalTransferFunction(out var fn));
			Assert.Equal(fnValues, fn.Values);

			Assert.Null(colorspace.ToXyzD50());
		}

		[SkippableFact]
		public void SrgbColorSpaceIsCloseToSrgb()
		{
			var colorspace = SKColorSpace.CreateSrgb();

			Assert.True(colorspace.GammaIsCloseToSrgb);
		}

		private static void AssertMatrix(float[] expected, SKMatrix44 actual)
		{
			var actualArray = actual
				.ToRowMajor()
				.Select(x => (float)Math.Round(x, 5))
				.ToArray();

			Assert.Equal(expected, actualArray);
		}
	}
}
