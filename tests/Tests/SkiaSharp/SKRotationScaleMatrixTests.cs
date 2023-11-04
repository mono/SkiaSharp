using Xunit;

namespace SkiaSharp.Tests
{
	public class SKRotationScaleMatrixTest : SKTest
	{
		[SkippableFact]
		public void TranslationToMatrixIsCorrect()
		{
			var m = SKMatrix.CreateTranslation(5, 7);
			var rsm = SKRotationScaleMatrix.CreateTranslation(5, 7).ToMatrix();

			Assert.Equal(m.Values, rsm.Values);
		}

		[SkippableFact]
		public void RotationToMatrixIsCorrect()
		{
			var m = SKMatrix.CreateRotationDegrees(45);
			var rsm = SKRotationScaleMatrix.CreateRotationDegrees(45, 0, 0).ToMatrix();

			Assert.Equal(m.Values, rsm.Values);
		}

		[SkippableFact]
		public void ScaleToMatrixIsCorrect()
		{
			var m = SKMatrix.CreateScale(3.5f, 3.5f);
			var rsm = SKRotationScaleMatrix.CreateScale(3.5f).ToMatrix();

			Assert.Equal(m.Values, rsm.Values);
		}
	}
}
