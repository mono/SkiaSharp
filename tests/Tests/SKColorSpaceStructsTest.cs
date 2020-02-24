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
		public void PrimariesToMatrix44()
		{
			var primaries = new float[] {
				0.64f, 0.33f,
				0.30f, 0.60f,
				0.15f, 0.06f,
				0.3127f, 0.3290f
			};
			var csp = new SKColorSpacePrimaries(primaries);

			var matrix44 = csp.ToXyzD50();
			Assert.NotNull(matrix44);

			using var srgb = SKColorSpace.CreateSrgb();
			var srgb44 = srgb.ToXyzD50();

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

			var csxyz = new SKColorSpaceTransferFn(values);
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

			var csxyz = new SKColorSpaceTransferFn(values);
			var inverted = csxyz.Invert();

			AssertSimilar(invertedValues, inverted.Values);
		}
	}
}
