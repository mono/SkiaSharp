using System;
using System.IO;
using Xunit;

namespace SkiaSharp.Tests
{
	public class SKColorSpaceStructsTest : SKTest
	{
		// SKColorSpacePrimaries

		[SkippableFact]
		public void PrimariesGoFullCircle()
		{
			var primaries = new float[] { 1, 2, 3, 4, 5, 6, 7, 8 };

			var csp = new SKColorSpacePrimaries(primaries);

			Assert.Equal(primaries, csp.Values);
		}

		[SkippableFact]
		public void PrimariesToXyz()
		{
			var primaries = new float[] {
				0.64f, 0.33f,
				0.30f, 0.60f,
				0.15f, 0.06f,
				0.3127f, 0.3290f
			};
			var csp = new SKColorSpacePrimaries(primaries);

			var xyz = csp.ToColorSpaceXyz();

			AssertSimilar(SKColorSpaceXyz.Srgb.Values, xyz.Values, PRECISION);
		}

		[SkippableFact]
		public void PrimariesToMatrix44()
		{
			var primaries = new float[] {
				0.64f, 0.33f,
				0.30f, 0.60f,
				0.15f, 0.06f,
				0.3127f, 0.3290f
			};
			var csp = new SKColorSpacePrimaries(primaries);

			var matrix44 = csp.ToMatrix44();
			Assert.NotNull(matrix44);

			var srgb44 = SKColorSpaceXyz.Srgb.ToMatrix44();

			AssertSimilar(srgb44.ToRowMajor(), matrix44.ToRowMajor(), PRECISION);
		}

		// SKColorSpaceTransferFn

		[SkippableFact]
		public void TransferFnIsFullCircle()
		{
			var values = new float[7] { 1, 2, 3, 4, 5, 6, 7 };

			var tf = new SKColorSpaceTransferFn(values);
			var tfValues = tf.Values;

			Assert.Equal(values, tfValues);
		}

		// SKColorSpaceXyz

		[SkippableFact]
		public void XyzIndexerWorks()
		{
			var values = new float[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
			var csxyz = new SKColorSpaceXyz(values);

			var counter = 0f;
			for (var y = 0; y < 3; y++)
			{
				for (var x = 0; x < 3; x++)
				{
					Assert.Equal(++counter, csxyz[x, y]);
				}
			}
		}

		[SkippableFact]
		public void XyzCanInvert()
		{
			var values = new float[] {
				1, 2, 3,
				0, 1, 4,
				5, 6, 1,
			};
			var invertedValues = new float[] {
				-11.5f, 8, 2.5f,
				10, -7, -2,
				-2.5f, 2, 0.5f,
			};

			var csxyz = new SKColorSpaceXyz(values);
			var inverted = csxyz.Invert();

			Assert.Equal(invertedValues, inverted.Values);
		}

		[SkippableFact]
		public void XyzCanInverts()
		{
			var values = new[]
			{
				0.60974f, 0.20528f, 0.14919f,
				0.31111f, 0.62567f, 0.06322f,
				0.01947f, 0.06087f, 0.74457f,
			};
			var invertedValues = new[]
			{
				1.96253f, -0.61068f, -0.34137f,
				-0.97876f, 1.91615f, 0.03342f,
				0.02869f, -0.14067f, 1.34926f,
			};

			var csxyz = new SKColorSpaceXyz(values);
			var inverted = csxyz.Invert();

			AssertSimilar(invertedValues, inverted.Values);
		}

		[SkippableFact]
		public void XyzCanConcat()
		{
			var expected = new SKColorSpaceXyz(new float[] {
				1, 16, 0,
				0, 18, 0,
				0, 0, 1
			});

			var a = new SKColorSpaceXyz(new float[] {
				1, 2, 0,
				0, 3, 0,
				0, 0, 1
			});
			var b = new SKColorSpaceXyz(new float[] {
				1, 4, 0,
				0, 6, 0,
				0, 0, 1
			});

			var concat = SKColorSpaceXyz.Concat(a, b);

			Assert.Equal(expected, concat);
		}

		[SkippableFact]
		public void XyzToMatrix44IsCorrect()
		{
			var values44 = new float[] {
				1, 2, 3, 0,
				4, 5, 6, 0,
				7, 8, 9, 0,
				0, 0, 0, 1
			};
			var values = new float[] {
				1, 2, 3,
				4, 5, 6,
				7, 8, 9
			};

			var csxyz = new SKColorSpaceXyz(values);

			var matrix44 = csxyz.ToMatrix44();

			Assert.Equal(values44, matrix44.ToRowMajor());
		}

		[SkippableFact]
		public void NamedGamutIsCorrect()
		{
			var csxyz = SKColorSpaceXyz.DisplayP3;

			var values = new float[] {
				0.515102f, 0.291965f, 0.157153f,
				0.241182f, 0.692236f, 0.0665819f,
				-0.00104941f, 0.0418818f, 0.784378f
			};

			Assert.Equal(values, csxyz.Values);
		}

		// SKColorSpaceIccProfile

		[SkippableFact]
		public void IccCanBeConstructedFromData()
		{
			var path = Path.Combine(PathToImages, "AdobeRGB1998.icc");
			using var data = SKData.Create(path);

			using var icc = SKColorSpaceIccProfile.Create(data);

			Assert.NotNull(icc);
			Assert.NotEqual(IntPtr.Zero, icc.Buffer);
			Assert.Equal(data.Size, icc.Size);
		}

		[SkippableFact]
		public void IccToXyzIsCorrect()
		{
			var path = Path.Combine(PathToImages, "AdobeRGB1998.icc");
			using var data = SKData.Create(path);

			using var icc = SKColorSpaceIccProfile.Create(data);
			var xyz = icc.ToColorSpaceXyz();

			Assert.Equal(SKColorSpaceXyz.AdobeRgb, xyz);
		}

		[SkippableFact]
		public void IccIsNullWithInvalidData()
		{
			var data = CreateTestData();

			using var icc = SKColorSpaceIccProfile.Create(data);

			Assert.Null(icc);
		}
	}
}
