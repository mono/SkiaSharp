using System;
using System.Numerics;
using Xunit;

namespace SkiaSharp.Tests
{
	public class SKMatrix44Test : SKTest
	{
		[SkippableFact]
		public void Matrix44CreatesIdentity()
		{
			var matrix = SKMatrix44.CreateIdentity();

			var expectedRowMajor = new float[] {
				1, 0, 0, 0,
				0, 1, 0, 0,
				0, 0, 1, 0,
				0, 0, 0, 1,
			};
			var rowMajor = matrix.ToRowMajor();

			Assert.Equal(expectedRowMajor, rowMajor);
		}

		[SkippableFact]
		public void RowMajorColumnMajorTransposes()
		{
			var rowMajor = new float[] {
				1, 2, 3, 0,
				0, 1, 4, 0,
				5, 6, 1, 0,
				0, 0, 0, 1,
			};
			var colMajor = new float[] {
				1, 0, 5, 0,
				2, 1, 6, 0,
				3, 4, 1, 0,
				0, 0, 0, 1,
			};

			var rowMajorMatrix = SKMatrix44.FromRowMajor(rowMajor);
			var colMajorMatrix = rowMajorMatrix.ToColumnMajor();

			Assert.Equal(colMajor, colMajorMatrix);
		}

		[SkippableFact]
		public void MatrixGoesFullCircle()
		{
			var rowMajor = new float[] {
				1, 2, 3, 0,
				0, 1, 4, 0,
				5, 6, 1, 0,
				0, 0, 0, 1,
			};

			var matrix = SKMatrix44.FromRowMajor(rowMajor);
			var result = matrix.ToRowMajor();

			Assert.Equal(rowMajor, result);
		}

		[SkippableFact]
		public void TransposeWorks()
		{
			var rowMajor = new float[] {
				1, 2, 3, 0,
				0, 1, 4, 0,
				5, 6, 1, 0,
				0, 0, 0, 1,
			};
			var colMajor = new float[] {
				1, 0, 5, 0,
				2, 1, 6, 0,
				3, 4, 1, 0,
				0, 0, 0, 1,
			};

			var rowMajorMatrix = SKMatrix44.FromRowMajor(rowMajor);
			rowMajorMatrix = rowMajorMatrix.Transpose();
			var colMajorMatrix = rowMajorMatrix.ToRowMajor();

			Assert.Equal(colMajor, colMajorMatrix);
		}

		[SkippableFact]
		public void ColumnMajorToRowMajorTransposes()
		{
			var rowMajor = new float[] {
				1, 2, 3, 0,
				0, 1, 4, 0,
				5, 6, 1, 0,
				0, 0, 0, 1,
			};
			var colMajor = new float[] {
				1, 0, 5, 0,
				2, 1, 6, 0,
				3, 4, 1, 0,
				0, 0, 0, 1,
			};

			var colMajorMatrix = SKMatrix44.FromColumnMajor(colMajor);
			var rowMajorMatrix = colMajorMatrix.ToRowMajor();

			Assert.Equal(rowMajor, rowMajorMatrix);
		}

		[SkippableFact]
		public void Matrix44Inverts()
		{
			var rowMajor = new float[] {
				1, 2, 3, 0,
				0, 1, 4, 0,
				5, 6, 1, 0,
				0, 0, 0, 1,
			};
			var expectedRowMajor = new float[] {
				-11.5f, 8, 2.5f, 0,
				10, -7, -2, 0,
				-2.5f, 2, 0.5f, 0,
				0, 0, 0, 1,
			};
			var determinant = 2f;

			var matrix = SKMatrix44.FromRowMajor(rowMajor);

			Assert.Equal(rowMajor, matrix.ToRowMajor());
			Assert.Equal(determinant, matrix.Determinant());

			var inverted = matrix.Invert();

			Assert.Equal(1f / determinant, inverted.Determinant());

			var actualRowMajor = inverted.ToRowMajor();
			Assert.Equal(expectedRowMajor, actualRowMajor);
		}

		[SkippableFact]
		public void Matrix44ConvertsToMatrix()
		{
			var rowMajor44 = new float[] {
				2, 3, 4, 5,
				4, 6, 8, 10,
				6, 9, 12, 15,
				8, 12, 16, 20,
			};
			var rowMajor = new float[] {
				rowMajor44[0], rowMajor44[1], rowMajor44[3],
				rowMajor44[4], rowMajor44[5], rowMajor44[7],
				rowMajor44[12], rowMajor44[13], rowMajor44[15],
			};

			var matrix44 = SKMatrix44.FromRowMajor(rowMajor44);

			Assert.Equal(rowMajor, matrix44.Matrix.Values);
		}

		[SkippableFact]
		public void TransformConvertsToMatrix()
		{
			var matrix44 = SKMatrix44.CreateRotationDegrees(0, 0, 1, 45);
			var matrix = SKMatrix.CreateRotationDegrees(45);

			AssertSimilar(matrix.Values, matrix44.Matrix.Values, 6);
		}

		[SkippableFact]
		public void ImplicitFromMatrix()
		{
			var matrix = SKMatrix.CreateRotationDegrees(45);
			var matrix44 = (SKMatrix44)matrix;

			Assert.Equal(matrix.Values, matrix44.Matrix.Values);
		}

		[SkippableFact]
		public void ImplicitFromRotationScale()
		{
			var rs = SKRotationScaleMatrix.CreateRotationDegrees(45, 0, 0);
			var matrix = SKMatrix.CreateRotationDegrees(45);
			var matrix44 = (SKMatrix44)rs.ToMatrix();

			Assert.Equal(matrix.Values, matrix44.Matrix.Values);
		}

#if NET5_0_OR_GREATER

		[SkippableFact]
		public void IndicesAreCorrectOnIdentity()
		{
			var skm = SKMatrix44.CreateIdentity();
			var m4x4 = Matrix4x4.Identity;

			for (var row = 0; row < 4; row++)
			{
				for (var col = 0; col < 4; col++)
				{
					var sk = skm[row, col];
					var m = m4x4[row, col];
					Assert.Equal(m, sk);
				}
			}
		}

		[SkippableFact]
		public void IndicesAreCorrectOnTranslate()
		{
			var skm = SKMatrix44.CreateTranslation(10, 20, 30);
			var m4x4 = Matrix4x4.CreateTranslation(10, 20, 30);

			for (var row = 0; row < 4; row++)
			{
				for (var col = 0; col < 4; col++)
				{
					var sk = skm[row, col];
					var m = m4x4[row, col];
					Assert.Equal(m, sk);
				}
			}
		}

		[SkippableFact]
		public void IndicesAreCorrectOnScale()
		{
			var skm = SKMatrix44.CreateScale(10, 20, 30);
			var m4x4 = Matrix4x4.CreateScale(10, 20, 30);

			for (var row = 0; row < 4; row++)
			{
				for (var col = 0; col < 4; col++)
				{
					var sk = skm[row, col];
					var m = m4x4[row, col];
					Assert.Equal(m, sk);
				}
			}
		}

#endif

		[SkippableFact]
		public void TranslationMapsScalars()
		{
			var matrixTranslate = SKMatrix44.CreateTranslation(10, 20, 0);

			var resultTranslateZero = matrixTranslate.MapScalars(0, 0, 0, 1);
			var resultTranslateValue = matrixTranslate.MapScalars(5, 25, 0, 1);

			Assert.Equal(new[] { 10f, 20f, 0f, 1f }, resultTranslateZero);
			Assert.Equal(new[] { 15f, 45f, 0f, 1f }, resultTranslateValue);
		}

		[SkippableFact]
		public void RotationMapsScalars()
		{
			var matrixRotate = SKMatrix44.CreateRotationDegrees(0, 1, 0, 90);

			var resultRotateZero = matrixRotate.MapScalars(0, 0, 0, 1);
			var resultRotateValue = matrixRotate.MapScalars(5, 25, 0, 1);

			Assert.Equal(new[] { 0f, 0f, 0f, 1f }, resultRotateZero);
			AssertSimilar(new[] { 0f, 25f, -5f, 1f }, resultRotateValue, PRECISION);
		}

		[SkippableFact]
		public void TranslationMapsPoints()
		{
			var matrixTranslate = SKMatrix44.CreateTranslation(10, 20, 0);

			var resultTranslateZero = matrixTranslate.MapPoint(SKPoint.Empty);
			var resultTranslateValue = matrixTranslate.MapPoint(new SKPoint(5, 25));

			Assert.Equal(new SKPoint(10f, 20f), resultTranslateZero);
			Assert.Equal(new SKPoint(15f, 45f), resultTranslateValue);
		}

		[SkippableFact]
		public void RotationMapsPoints()
		{
			var matrixRotate = SKMatrix44.CreateRotationDegrees(0, 1, 0, 90);

			var resultRotateZero = matrixRotate.MapPoint(SKPoint.Empty);
			var resultRotateValue = matrixRotate.MapPoint(new SKPoint(5, 25));

			Assert.Equal(new SKPoint(0f, 0f), resultRotateZero);
			Assert.Equal((double)0, (double)resultRotateValue.X, PRECISION);
			Assert.Equal((double)25, (double)resultRotateValue.Y, PRECISION);
		}

		[SkippableFact]
		public void IdentityIsCorrectLayout()
		{
			var matrix = SKMatrix44.Identity;
			var rect = SKRectI.Create(25, 25, 50, 50);

			using var bmp = DrawMatrixBitmap(rect, matrix);

			AssertMatrixBitmap(bmp, rect);
		}

		[SkippableTheory]
		[InlineData(0, 0, 0)]
		[InlineData(10, 0, 0)]
		[InlineData(-10, 0, 0)]
		[InlineData(0, 10, 0)]
		[InlineData(0, -10, 0)]
		[InlineData(0, 0, 10)]
		[InlineData(0, 0, -10)]
		[InlineData(10, 10, 0)]
		[InlineData(-10, -10, 0)]
		public void TranslationIsCorrectLayout(int x, int y, int z)
		{
			var matrix = SKMatrix44.CreateTranslation(x, y, z);

			using var bmp = DrawMatrixBitmap(SKRectI.Create(25, 25, 50, 50), matrix);

			AssertMatrixBitmap(bmp, SKRectI.Create(25 + x, 25 + y, 50, 50));
		}

		[SkippableTheory]
		[InlineData(1, 1, 1)]
		[InlineData(2, 1, 1)]
		[InlineData(1, 2, 1)]
		[InlineData(1, 1, 2)]
		[InlineData(2, 2, 1)]
		public void ScaleIsCorrectLayoutWithOriginCenter(int x, int y, int z)
		{
			var matrix = SKMatrix44.CreateScale(x, y, z, 50, 50, 50);

			const int s = 30;
			const int o = 35;

			using var bmp = DrawMatrixBitmap(SKRectI.Create(o, o, s, s), matrix);

			AssertMatrixBitmap(bmp, SKRectI.Create(o - ((s * x) - s) / 2, o - ((s * y) - s) / 2, s * x, s * y));
		}

		[SkippableTheory]
		[InlineData(1, 1, 1)]
		[InlineData(2, 1, 1)]
		[InlineData(1, 2, 1)]
		[InlineData(1, 1, 2)]
		[InlineData(2, 2, 1)]
		public void ScaleIsCorrectLayoutWithOriginTopLeft(int x, int y, int z)
		{
			var matrix = SKMatrix44.CreateScale(x, y, z);

			using var bmp = DrawMatrixBitmap(SKRectI.Create(30, 30, 10, 10), matrix);

			AssertMatrixBitmap(bmp, SKRectI.Create(30 * x, 30 * y, 10 * x, 10 * y));
		}

		private static SKBitmap DrawMatrixBitmap(SKRect rect, SKMatrix44 matrix)
		{
			var bmp = new SKBitmap(100, 100);
			using var canvas = new SKCanvas(bmp);

			canvas.Clear(SKColors.Red);
			canvas.SetMatrix(matrix);

			using var paint = new SKPaint { Color = SKColors.Green };
			canvas.DrawRect(rect, paint);

			return bmp;
		}

		private static void AssertMatrixBitmap(SKBitmap bmp, SKRectI rect)
		{
			// edges
			Assert.Equal(SKColors.Red, bmp.GetPixel(0, 0));
			Assert.Equal(SKColors.Red, bmp.GetPixel(99, 0));
			Assert.Equal(SKColors.Red, bmp.GetPixel(99, 99));
			Assert.Equal(SKColors.Red, bmp.GetPixel(99, 0));

			// bounds of rect
			Assert.Equal(SKColors.Green, bmp.GetPixel(rect.Left, rect.Top));
			Assert.Equal(SKColors.Green, bmp.GetPixel(rect.Right - 1, rect.Top));
			Assert.Equal(SKColors.Green, bmp.GetPixel(rect.Right - 1, rect.Bottom - 1));
			Assert.Equal(SKColors.Green, bmp.GetPixel(rect.Left, rect.Bottom - 1));
		}
	}
}
