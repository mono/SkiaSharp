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
		public void StaticSrgbIsReturnedAsTheStaticInstance()
		{
			var handle = SkiaApi.sk_colorspace_new_srgb();
			try
			{
				var cs = SKObject.GetObject<SKColorSpace>(handle, unrefExisting: false);
				Assert.Equal("SKColorSpaceStatic", cs.GetType().Name);
			}
			finally
			{
				SkiaApi.sk_refcnt_safe_unref(handle);
			}
		}

		[SkippableFact]
		public void ImageInfoHasColorSpace()
		{
			var colorspace = SKColorSpace.CreateSrgb();

			var info = new SKImageInfo(100, 100, SKImageInfo.PlatformColorType, SKAlphaType.Premul, colorspace);
			Assert.Same(colorspace, info.ColorSpace);

			var image = SKImage.Create(info);
			Assert.Same(colorspace, image.PeekPixels().ColorSpace);
		}

		[SkippableFact]
		public void ImageInfoColorSpaceIsReferencedCorrectly()
		{
			VerifyImmediateFinalizers();

			var img = DoWork(out var colorspaceHandle);

			CollectGarbage();

			Assert.Equal(1, colorspaceHandle.GetReferenceCount(true));

			Check();

			CollectGarbage();

			Assert.Equal(1, colorspaceHandle.GetReferenceCount(true));

			void Check()
			{
				var peek = img.PeekPixels();
				Assert.Equal(2, colorspaceHandle.GetReferenceCount(true));

				// get the info and color space
				var info1 = peek.Info;
				var cs1 = info1.ColorSpace;
				Assert.Equal(3, colorspaceHandle.GetReferenceCount(true));
				Assert.NotNull(cs1);

				// get the info and color space again and make sure we are all using the same things
				var info2 = peek.Info;
				var cs2 = info2.ColorSpace;
				Assert.Equal(3, colorspaceHandle.GetReferenceCount(true));
				Assert.NotNull(cs2);

				Assert.Same(cs1, cs2);
			}

			SKImage DoWork(out IntPtr handle)
			{
				var colorspace = SKColorSpace.CreateRgb(
					new SKColorSpaceTransferFn { A = 0.6f, B = 0.5f, C = 0.4f, D = 0.3f, E = 0.2f, F = 0.1f },
					SKMatrix44.CreateIdentity());

				Assert.NotNull(colorspace);

				handle = colorspace.Handle;
				Assert.Equal(1, handle.GetReferenceCount(true));

				var info = new SKImageInfo(100, 100, SKImageInfo.PlatformColorType, SKAlphaType.Premul, colorspace);
				Assert.Equal(1, handle.GetReferenceCount(true));

				var image = SKImage.Create(info);
				Assert.Equal(2, handle.GetReferenceCount(true));

				return image;
			}
		}

		[SkippableFact]
		public void ColorSpaceIsNotDisposedPrematurely()
		{
			VerifyImmediateFinalizers();

			var img = DoWork(out var colorSpaceHandle, out var weakColorspace);

			CheckBeforeCollection(colorSpaceHandle);

			CheckExistingImage(3, img, colorSpaceHandle);

			CollectGarbage();

			Assert.Null(weakColorspace.Target);
			Assert.Equal(1, colorSpaceHandle.GetReferenceCount(true));

			CheckExistingImage(2, img, colorSpaceHandle);

			CollectGarbage();

			Assert.Null(weakColorspace.Target);
			Assert.Equal(1, colorSpaceHandle.GetReferenceCount(true));

			CollectGarbage();

			Assert.Equal(1, colorSpaceHandle.GetReferenceCount(true));

			GC.KeepAlive(img);

			void CheckBeforeCollection(IntPtr csh)
			{
				Assert.NotNull(weakColorspace.Target);
				Assert.Equal(2, csh.GetReferenceCount(true));
			}

			void CheckExistingImage(int expected, SKImage image, IntPtr csh)
			{
				var peek = image.PeekPixels();
				Assert.Equal(expected, csh.GetReferenceCount(true));

				var info = peek.Info;
				Assert.Equal(3, csh.GetReferenceCount(true));

				var cs = info.ColorSpace;
				Assert.Equal(3, csh.GetReferenceCount(true));
				Assert.NotNull(cs);
			}

			SKImage DoWork(out IntPtr handle, out WeakReference weak)
			{
				var colorspace = SKColorSpace.CreateRgb(
					new SKColorSpaceTransferFn { A = 0.1f, B = 0.2f, C = 0.3f, D = 0.4f, E = 0.5f, F = 0.6f },
					SKMatrix44.CreateIdentity());

				Assert.NotNull(colorspace);

				handle = colorspace.Handle;
				weak = new WeakReference(colorspace);

				Assert.Equal(1, handle.GetReferenceCount(true));

				var info = new SKImageInfo(100, 100, SKImageInfo.PlatformColorType, SKAlphaType.Premul, colorspace);

				Assert.Equal(1, handle.GetReferenceCount(true));

				var image = SKImage.Create(info);

				Assert.Equal(2, handle.GetReferenceCount(true));

				return image;
			}
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

			var matrix = new SKMatrix44();
			Assert.True(colorspace.ToXyzD50(matrix));
			AssertMatrix(toXYZ, matrix);

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

		[SkippableFact]
		public void ColorSpaceCorrectlyReferencesSrgbSingleton()
		{
			var handle1 = SkiaApi.sk_colorspace_new_srgb();

			var colorspace1 = SKColorSpace.CreateSrgb();

			Assert.Equal(colorspace1.Handle, handle1);

			var colorspace2 = SKColorSpace.CreateSrgb();

			Assert.Same(colorspace1, colorspace2);
			Assert.Equal(handle1, colorspace2.Handle);

			colorspace2.Dispose();
			Assert.False(colorspace2.IsDisposed);
			Assert.False(colorspace1.IsDisposed);

			SkiaApi.sk_refcnt_safe_unref(handle1);
		}

		[SkippableFact]
		public void SameColorSpaceCreatedDifferentWaysAreTheSameObject()
		{
			var colorspace1 = SKColorSpace.CreateSrgbLinear();
			Assert.Equal("SkiaSharp.SKColorSpace+SKColorSpaceStatic", colorspace1.GetType().FullName);
			Assert.Equal(2, colorspace1.GetReferenceCount());

			var colorspace2 = SKColorSpace.CreateRgb(SKNamedGamma.Linear, SKColorSpaceGamut.Srgb);
			Assert.Equal("SkiaSharp.SKColorSpace+SKColorSpaceStatic", colorspace2.GetType().FullName);
			Assert.Equal(2, colorspace2.GetReferenceCount());

			Assert.Same(colorspace1, colorspace2);

			var colorspace3 = SKColorSpace.CreateRgb(
				new SKColorSpaceTransferFn { A = 0.6f, B = 0.5f, C = 0.4f, D = 0.3f, E = 0.2f, F = 0.1f },
				SKMatrix44.CreateIdentity());
			Assert.NotSame(colorspace1, colorspace3);

			colorspace3.Dispose();
			Assert.True(colorspace3.IsDisposed);
			Assert.Equal(2, colorspace1.GetReferenceCount());

			colorspace2.Dispose();
			Assert.False(colorspace2.IsDisposed);
			Assert.Equal(2, colorspace1.GetReferenceCount());

			colorspace1.Dispose();
			Assert.False(colorspace1.IsDisposed);
			Assert.Equal(2, colorspace1.GetReferenceCount());
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
