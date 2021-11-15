using System;
using Xunit;

namespace SkiaSharp.Tests
{
	[Obsolete]
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
			rowMajorMatrix.Transpose();
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

			Assert.Equal(matrix.Values, matrix44.Matrix.Values);
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
			Assert.Equal(0, resultRotateValue.X, PRECISION);
			Assert.Equal(25, resultRotateValue.Y, PRECISION);
		}

		[SkippableFact]
		public void MapsPointsBulk()
		{
			var rnd = new Random();

			var matrixTranslate = SKMatrix44.CreateTranslation(10, 25, 0);

			// generate some points
			var points = new SKPoint[1000];
			var results = new SKPoint[points.Length];
			for (var i = 0; i < points.Length; i++)
			{
				points[i] = new SKPoint(rnd.Next(1000) / 10f, rnd.Next(1000) / 10f);
				results[i] = new SKPoint(points[i].X + 10, points[i].Y + 25);
			}

			var actualResults = matrixTranslate.MapPoints(points);

			Assert.Equal(results, actualResults);
		}
	}
}
