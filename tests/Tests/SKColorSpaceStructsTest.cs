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
	}
}
