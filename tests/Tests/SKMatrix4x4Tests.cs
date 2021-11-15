using System;
using System.Numerics;
using Xunit;

namespace SkiaSharp.Tests
{
	public class SKMatrix4x4Test : SKTest
	{
		[SkippableFact]
		public void ConstructorSetsCorrectProperties()
		{
			var m = new SKMatrix4x4(
				1, 2, 3, 4,
				5, 6, 7, 8,
				9, 10, 11, 12,
				13, 14, 15, 16);

			var actual = new float[]
			{
				m.M11, m.M12, m.M13, m.M14,
				m.M21, m.M22, m.M23, m.M24,
				m.M31, m.M32, m.M33, m.M34,
				m.M41, m.M42, m.M43, m.M44,
			};

			var expected = new float[]
			{
				1, 2, 3, 4,
				5, 6, 7, 8,
				9, 10, 11, 12,
				13, 14, 15, 16
			};

			Assert.Equal(expected, actual);
		}

		[SkippableFact]
		public void Matrix4x4CreatesIdentity()
		{
			var sysMatrix = Matrix4x4.Identity;

			var matrix = SKMatrix4x4.CreateIdentity();

			AssertMatrix(sysMatrix, matrix);
		}

		[SkippableTheory]
		[InlineData(0, 0, 0)]
		[InlineData(0, 0, 5)]
		[InlineData(0, 5, 0)]
		[InlineData(5, 0, 0)]
		[InlineData(0, 5, 5)]
		[InlineData(5, 5, 0)]
		[InlineData(5, 0, 5)]
		[InlineData(5, 5, 5)]
		[InlineData(2, 4, 6)]
		public void Matrix4x4CreatesTranslation(float x, float y, float z)
		{
			var sysMatrix = Matrix4x4.CreateTranslation(x, y, z);

			var matrix = SKMatrix4x4.CreateTranslation(x, y, z);

			AssertMatrix(sysMatrix, matrix);
		}

		[SkippableTheory]
		[InlineData(1, 1, 1)]
		[InlineData(1, 1, 5)]
		[InlineData(1, 5, 1)]
		[InlineData(5, 1, 1)]
		[InlineData(1, 5, 5)]
		[InlineData(5, 5, 1)]
		[InlineData(5, 1, 5)]
		[InlineData(5, 5, 5)]
		[InlineData(2, 4, 6)]
		public void Matrix4x4CreatesScale(float x, float y, float z)
		{
			var sysMatrix = Matrix4x4.CreateScale(x, y, z);

			var matrix = SKMatrix4x4.CreateScale(x, y, z);

			AssertMatrix(sysMatrix, matrix);
		}

		[SkippableTheory]
		[InlineData(-5)]
		[InlineData(-1)]
		[InlineData(0)]
		[InlineData(1)]
		[InlineData(5)]
		public void Matrix4x4CreatesRotationX(float degrees)
		{
			var radians = degrees * SKMatrix.DegreesToRadians;

			var sysMatrix = Matrix4x4.CreateRotationX(radians);

			var matrix = SKMatrix4x4.CreateRotation(1, 0, 0, radians);

			AssertMatrix(sysMatrix, matrix);
		}

		[SkippableTheory]
		[InlineData(-5)]
		[InlineData(-1)]
		[InlineData(0)]
		[InlineData(1)]
		[InlineData(5)]
		public void Matrix4x4CreatesRotationY(float degrees)
		{
			var radians = degrees * SKMatrix.DegreesToRadians;

			var sysMatrix = Matrix4x4.CreateRotationY(radians);

			var matrix = SKMatrix4x4.CreateRotation(0, 1, 0, radians);

			AssertMatrix(sysMatrix, matrix);
		}

		[SkippableTheory]
		[InlineData(-5)]
		[InlineData(-1)]
		[InlineData(0)]
		[InlineData(1)]
		[InlineData(5)]
		public void Matrix4x4CreatesRotationZ(float degrees)
		{
			var radians = degrees * SKMatrix.DegreesToRadians;

			var sysMatrix = Matrix4x4.CreateRotationZ(radians);

			var matrix = SKMatrix4x4.CreateRotation(0, 0, 1, radians);

			AssertMatrix(sysMatrix, matrix);
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

			var rowMajorMatrix = SKMatrix4x4.FromRowMajor(rowMajor);
			var colMajorMatrix = rowMajorMatrix.ToColumnMajor();

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

			var colMajorMatrix = SKMatrix4x4.FromColumnMajor(colMajor);
			var rowMajorMatrix = colMajorMatrix.ToRowMajor();

			Assert.Equal(rowMajor, rowMajorMatrix);
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

			var matrix = SKMatrix4x4.FromRowMajor(rowMajor);
			var result = matrix.ToRowMajor();

			Assert.Equal(rowMajor, result);
		}


		[SkippableFact]
		public void TransposeWorks()
		{
			var rowMajorMatrix = new SKMatrix4x4(
				1, 2, 3, 0,
				0, 1, 4, 0,
				5, 6, 1, 0,
				0, 0, 0, 1);

			var colMajorMatrix = new SKMatrix4x4(
				1, 0, 5, 0,
				2, 1, 6, 0,
				3, 4, 1, 0,
				0, 0, 0, 1);

			var transposed = rowMajorMatrix;
			transposed.Transpose();

			AssertMatrix(colMajorMatrix, transposed);
		}

		[SkippableFact]
		public void ToRowMajorIsCorrect()
		{
			var rowMajor = new float[] {
				1, 2, 3, 0,
				0, 1, 4, 0,
				5, 6, 1, 0,
				0, 0, 0, 1,
			};

			var matrix = new SKMatrix4x4(
				1, 0, 5, 0,
				2, 1, 6, 0,
				3, 4, 1, 0,
				0, 0, 0, 1);

			Assert.Equal(rowMajor, matrix.ToRowMajor());
		}

		[SkippableFact]
		public void ToColMajorIsCorrect()
		{
			var colMajor = new float[] {
				1, 2, 3, 0,
				0, 1, 4, 0,
				5, 6, 1, 0,
				0, 0, 0, 1,
			};

			var matrix = new SKMatrix4x4(
				1, 2, 3, 0,
				0, 1, 4, 0,
				5, 6, 1, 0,
				0, 0, 0, 1);

			Assert.Equal(colMajor, matrix.ToColumnMajor());
		}

		[SkippableFact]
		public void FromRowMajorIsCorrect()
		{
			var colMajor = new SKMatrix4x4(
				1, 0, 5, 0,
				2, 1, 6, 0,
				3, 4, 1, 0,
				0, 0, 0, 1);

			var fromRowMajorMatrix = SKMatrix4x4.FromRowMajor(new float[] {
				1, 2, 3, 0,
				0, 1, 4, 0,
				5, 6, 1, 0,
				0, 0, 0, 1,
			});

			Assert.Equal(colMajor, fromRowMajorMatrix);
		}

		[SkippableFact]
		public void FromColMajorIsCorrect()
		{
			var colMajor = new SKMatrix4x4(
				1, 2, 3, 0,
				0, 1, 4, 0,
				5, 6, 1, 0,
				0, 0, 0, 1);

			var fromColumnMajorMatrix = SKMatrix4x4.FromColumnMajor(new float[] {
				1, 2, 3, 0,
				0, 1, 4, 0,
				5, 6, 1, 0,
				0, 0, 0, 1,
			});

			Assert.Equal(colMajor, fromColumnMajorMatrix);
		}

		[SkippableFact]
		public void FromColMajorToColMajorIsUnchanged()
		{
			var colMajor = new float[] {
				1, 2, 3, 0,
				0, 1, 4, 0,
				5, 6, 1, 0,
				0, 0, 0, 1,
			};

			var fromColumnMajorMatrix = SKMatrix4x4.FromColumnMajor(new float[] {
				1, 2, 3, 0,
				0, 1, 4, 0,
				5, 6, 1, 0,
				0, 0, 0, 1,
			});

			var toColMajor = fromColumnMajorMatrix.ToColumnMajor();

			Assert.Equal(colMajor, toColMajor);
		}

		[SkippableFact]
		public void FromRowMajorToRowMajorIsUnchanged()
		{
			var rowMajor = new float[] {
				1, 2, 3, 0,
				0, 1, 4, 0,
				5, 6, 1, 0,
				0, 0, 0, 1,
			};

			var fromRowMajorMatrix = SKMatrix4x4.FromRowMajor(new float[] {
				1, 2, 3, 0,
				0, 1, 4, 0,
				5, 6, 1, 0,
				0, 0, 0, 1,
			});

			var toRowMajor = fromRowMajorMatrix.ToRowMajor();

			Assert.Equal(rowMajor, toRowMajor);
		}

		[SkippableFact]
		public void FromRowMajorToColMajorIsCorrect()
		{
			var colMajor = new float[] {
				1, 0, 5, 0,
				2, 1, 6, 0,
				3, 4, 1, 0,
				0, 0, 0, 1
			};

			var fromRowMajorMatrix = SKMatrix4x4.FromRowMajor(new float[] {
				1, 2, 3, 0,
				0, 1, 4, 0,
				5, 6, 1, 0,
				0, 0, 0, 1,
			});

			var toColMajor = fromRowMajorMatrix.ToColumnMajor();

			Assert.Equal(colMajor, toColMajor);
		}

		[SkippableFact]
		public void FromColMajorToRowMajorIsCorrect()
		{
			var rowMajor = new float[] {
				1, 0, 5, 0,
				2, 1, 6, 0,
				3, 4, 1, 0,
				0, 0, 0, 1
			};

			var fromColMajorMatrix = SKMatrix4x4.FromColumnMajor(new float[] {
				1, 2, 3, 0,
				0, 1, 4, 0,
				5, 6, 1, 0,
				0, 0, 0, 1,
			});

			var toColMajor = fromColMajorMatrix.ToRowMajor();

			Assert.Equal(rowMajor, toColMajor);
		}

		//[SkippableFact]
		//public void Matrix4x4Inverts()
		//{
		//	var rowMajor = new float[] {
		//		1, 2, 3, 0,
		//		0, 1, 4, 0,
		//		5, 6, 1, 0,
		//		0, 0, 0, 1,
		//	};
		//	var expectedRowMajor = new float[] {
		//		-11.5f, 8, 2.5f, 0,
		//		10, -7, -2, 0,
		//		-2.5f, 2, 0.5f, 0,
		//		0, 0, 0, 1,
		//	};
		//	var determinant = 2f;

		//	var matrix = SKMatrix4x4.FromRowMajor(rowMajor);

		//	Assert.Equal(rowMajor, matrix.ToRowMajor());
		//	Assert.Equal(determinant, matrix.GetDeterminant());

		//	var inverted = matrix.Invert();

		//	Assert.Equal(1f / determinant, inverted.GetDeterminant());

		//	var actualRowMajor = inverted.ToRowMajor();
		//	Assert.Equal(expectedRowMajor, actualRowMajor);
		//}

		[SkippableFact]
		public void CorrectDeterminant()
		{
			var rowMajor = new float[] {
				1, 2, 3, 0,
				0, 1, 4, 0,
				5, 6, 1, 0,
				0, 0, 0, 1,
			};

			var determinant = 2f;

			var matrix = SKMatrix4x4.FromRowMajor(rowMajor);

			Assert.Equal(determinant, matrix.GetDeterminant());
		}

		[SkippableFact]
		public void Matrix4x4ConvertsToMatrix()
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

			var matrix4x4 = SKMatrix4x4.FromRowMajor(rowMajor44);
			var matrix3x3 = matrix4x4.Matrix;

			Assert.Equal(rowMajor, matrix3x3.Values);
		}

		[SkippableFact]
		public void TransformConvertsToMatrix()
		{
			var matrix4x4 = SKMatrix4x4.CreateRotationDegrees(0, 0, 1, 45);
			var matrix = SKMatrix.CreateRotationDegrees(45);
			var matrix3x3 = matrix4x4.Matrix;

			Assert.Equal(matrix.Values, matrix3x3.Values);
		}

		[SkippableFact]
		public void FromMatrix3x3ToMatrix4x4ToMatrix3x3()
		{
			var matrix3x3 = SKMatrix.CreateRotationDegrees(45);

			var toMatrix4x4 = (SKMatrix4x4)matrix3x3;
			var toMatrix3x3 = toMatrix4x4.Matrix;

			Assert.Equal(matrix3x3.Values, toMatrix3x3.Values);
		}

		[SkippableFact]
		public void FromRotationScaleToMatrix3x3ToMatrix4x4ToMatrix3x3()
		{
			var rs = SKRotationScaleMatrix.CreateRotationDegrees(45, 0, 0);
			var matrix3x3 = SKMatrix.CreateRotationDegrees(45);

			var toMatrix4x4 = (SKMatrix4x4)rs.ToMatrix();
			var toMatrix3x3 = toMatrix4x4.Matrix;

			Assert.Equal(matrix3x3.Values, toMatrix3x3.Values);
		}

		[SkippableFact]
		public void FromRotationScaleToMatrix4x4()
		{
			var matrix4x4 = SKMatrix4x4.CreateRotationDegrees(0, 0, 1, 45);
			var rs = SKRotationScaleMatrix.CreateRotationDegrees(45, 0, 0);

			var toMatrix4x4 = rs.ToMatrix4x4();

			Assert.Equal(matrix4x4, toMatrix4x4);
		}

		[SkippableFact]
		public void TranslationMapsMultipleVector4()
		{
			var matrix = SKMatrix4x4.CreateTranslation(10, 20, 0);

			var input = new float[] { 0, 0, 0, 1, 5, 25, 0, 1 };
			var expected = new float[] { 10, 20, 0, 1, 15, 45, 0, 1 };

			Span<float> result = stackalloc float[8];

			matrix.MapVector4(input, result);

			AssertSimilar(expected, result);
		}

		[SkippableTheory]
		[InlineData(new float[] { 0, 0, 0, 1 }, new[] { 10f, 20f, 0f, 1f })]
		[InlineData(new float[] { 5, 25, 0, 1 }, new[] { 15f, 45f, 0f, 1f })]
		[InlineData(new float[] { 0, 0, 0, 1, 5, 25, 0, 1 }, new[] { 10f, 20f, 0f, 1f, 15f, 45f, 0f, 1f })]
		public void TranslationMapsVector4(float[] input, float[] expected)
		{
			var matrix = SKMatrix4x4.CreateTranslation(10, 20, 0);

			var result = new float[input.Length];
			matrix.MapVector4(input, result);

			AssertSimilar(expected, result);
		}

		[SkippableTheory]
		[InlineData(new float[] { 0, 0, 0, 1 }, new[] { 0f, 0f, 0f, 1f })]
		[InlineData(new float[] { 5, 25, 0, 1 }, new[] { 0f, 25f, -5f, 1f })]
		[InlineData(new float[] { 5, 25, 0, 1, 0, 0, 0, 1 }, new[] { 0f, 25f, -5f, 1f, 0f, 0f, 0f, 1f })]
		public void RotationMapsVector4(float[] input, float[] expected)
		{
			var matrix = SKMatrix4x4.CreateRotationDegrees(0, 1, 0, 90);

			var result = new float[input.Length];
			matrix.MapVector4(input, result);

			AssertSimilar(expected, result);
		}

		[SkippableTheory]
		[InlineData(new float[] { 0, 0 }, new[] { 10f, 20f, 0f, 1f })]
		[InlineData(new float[] { 5, 25 }, new[] { 15f, 45f, 0f, 1f })]
		[InlineData(new float[] { 0, 0, 5, 25 }, new[] { 10f, 20f, 0f, 1f, 15f, 45f, 0f, 1f })]
		public void TranslationMapsVector2(float[] input, float[] expected)
		{
			var matrix = SKMatrix4x4.CreateTranslation(10, 20, 0);

			var result = new float[input.Length * 2];
			matrix.MapVector2(input, result);

			AssertSimilar(expected, result);
		}

		[SkippableTheory]
		[InlineData(new float[] { 0, 0 }, new[] { 0f, 0f, 0f, 1f })]
		[InlineData(new float[] { 5, 25 }, new[] { 0f, 25f, -5f, 1f })]
		[InlineData(new float[] { 5, 25, 0, 0 }, new[] { 0f, 25f, -5f, 1f, 0f, 0f, 0f, 1f })]
		public void RotationMapsVector2(float[] input, float[] expected)
		{
			var matrix = SKMatrix4x4.CreateRotationDegrees(0, 1, 0, 90);

			var result = new float[input.Length * 2];
			matrix.MapVector2(input, result);

			AssertSimilar(expected, result);
		}

		[SkippableTheory]
		[InlineData(0, 0, 10, 20)]
		[InlineData(5, 25, 15, 45)]
		public void TranslationMapsPoints(float x, float y, float expectedX, float expectedY)
		{
			var matrix = SKMatrix4x4.CreateTranslation(10, 20, 0);

			var result = matrix.MapPoint(new SKPoint(x, y));

			Assert.Equal(new SKPoint(expectedX, expectedY), result);
		}

		[SkippableTheory]
		[InlineData(0, 0, 0, 0)]
		[InlineData(5, 25, 0, 25)]
		public void RotationMapsPoints(float x, float y, float expectedX, float expectedY)
		{
			var matrix = SKMatrix4x4.CreateRotationDegrees(0, 1, 0, 90);

			var result = matrix.MapPoint(new SKPoint(x, y));

			Assert.Equal(expectedX, result.X, PRECISION);
			Assert.Equal(expectedY, result.Y, PRECISION);
		}

		[SkippableFact]
		public void MapsPointsBulk()
		{
			var rnd = new Random();

			var matrixTranslate = SKMatrix4x4.CreateTranslation(10, 25, 0);

			// generate some points
			var points = new SKPoint[1000];
			var results = new SKPoint[points.Length];
			for (var i = 0; i < points.Length; i++)
			{
				points[i] = new SKPoint(rnd.Next(1000) / 10f, rnd.Next(1000) / 10f);
				results[i] = new SKPoint(points[i].X + 10, points[i].Y + 25);
			}

			var actualResults = new SKPoint[points.Length];

			matrixTranslate.MapPoints(points, actualResults);

			Assert.Equal(results, actualResults);
		}

		[SkippableTheory]
		[InlineData(-5)]
		[InlineData(-1)]
		[InlineData(0)]
		[InlineData(1)]
		[InlineData(5)]
		public void TranslationXDrawsCorrectly(int x)
		{
			using var bitmap = new SKBitmap(30, 30);
			using var canvas = new SKCanvas(bitmap);
			canvas.Clear(SKColors.White);

			var matrix = SKMatrix4x4.CreateTranslation(x, 0, 0);
			canvas.Concat(matrix);

			var rect = SKRectI.Create(10, 10, 10, 10);
			canvas.DrawRect(rect, new SKPaint());

			Assert.Equal(SKColors.White, bitmap.GetPixel(x + rect.Left - 1, rect.MidY));
			Assert.Equal(SKColors.Black, bitmap.GetPixel(x + rect.Left + 0, rect.MidY));
			Assert.Equal(SKColors.Black, bitmap.GetPixel(x + rect.Left + 1, rect.MidY));
			Assert.Equal(SKColors.Black, bitmap.GetPixel(x + rect.Right - 1, rect.MidY));
			Assert.Equal(SKColors.White, bitmap.GetPixel(x + rect.Right + 0, rect.MidY));
			Assert.Equal(SKColors.White, bitmap.GetPixel(x + rect.Right + 1, rect.MidY));

			Assert.Equal(SKColors.White, bitmap.GetPixel(x + rect.MidX, rect.Top - 1));
			Assert.Equal(SKColors.Black, bitmap.GetPixel(x + rect.MidX, rect.Top + 0));
			Assert.Equal(SKColors.Black, bitmap.GetPixel(x + rect.MidX, rect.Top + 1));
			Assert.Equal(SKColors.Black, bitmap.GetPixel(x + rect.MidX, rect.Bottom - 1));
			Assert.Equal(SKColors.White, bitmap.GetPixel(x + rect.MidX, rect.Bottom + 0));
			Assert.Equal(SKColors.White, bitmap.GetPixel(x + rect.MidX, rect.Bottom + 1));
		}

		[SkippableTheory]
		[InlineData(-5)]
		[InlineData(-1)]
		[InlineData(0)]
		[InlineData(1)]
		[InlineData(5)]
		public void TranslationYDrawsCorrectly(int y)
		{
			using var bitmap = new SKBitmap(30, 30);
			using var canvas = new SKCanvas(bitmap);
			canvas.Clear(SKColors.White);

			var matrix = SKMatrix4x4.CreateTranslation(0, y, 0);
			canvas.Concat(matrix);

			var rect = SKRectI.Create(10, 10, 10, 10);
			canvas.DrawRect(rect, new SKPaint());

			Assert.Equal(SKColors.White, bitmap.GetPixel(rect.Left - 1, y + rect.MidY));
			Assert.Equal(SKColors.Black, bitmap.GetPixel(rect.Left + 0, y + rect.MidY));
			Assert.Equal(SKColors.Black, bitmap.GetPixel(rect.Left + 1, y + rect.MidY));
			Assert.Equal(SKColors.Black, bitmap.GetPixel(rect.Right - 1, y + rect.MidY));
			Assert.Equal(SKColors.White, bitmap.GetPixel(rect.Right + 0, y + rect.MidY));
			Assert.Equal(SKColors.White, bitmap.GetPixel(rect.Right + 1, y + rect.MidY));

			Assert.Equal(SKColors.White, bitmap.GetPixel(rect.MidX, y + rect.Top - 1));
			Assert.Equal(SKColors.Black, bitmap.GetPixel(rect.MidX, y + rect.Top + 0));
			Assert.Equal(SKColors.Black, bitmap.GetPixel(rect.MidX, y + rect.Top + 1));
			Assert.Equal(SKColors.Black, bitmap.GetPixel(rect.MidX, y + rect.Bottom - 1));
			Assert.Equal(SKColors.White, bitmap.GetPixel(rect.MidX, y + rect.Bottom + 0));
			Assert.Equal(SKColors.White, bitmap.GetPixel(rect.MidX, y + rect.Bottom + 1));
		}

		[SkippableTheory]
		[InlineData(-50)]
		[InlineData(-1)]
		[InlineData(0)]
		[InlineData(1)]
		[InlineData(50)]
		public void TranslationZOrthoDrawsCorrectly(int z)
		{
			using var bitmap = new SKBitmap(30, 30);
			using var canvas = new SKCanvas(bitmap);
			canvas.Clear(SKColors.White);

			var matrix = SKMatrix4x4.CreateTranslation(0, 0, z);
			canvas.Concat(matrix);

			var rect = SKRectI.Create(10, 10, 10, 10);
			canvas.DrawRect(rect, new SKPaint());

			Assert.Equal(SKColors.White, bitmap.GetPixel(rect.Left - 1, rect.MidY));
			Assert.Equal(SKColors.Black, bitmap.GetPixel(rect.Left + 0, rect.MidY));
			Assert.Equal(SKColors.Black, bitmap.GetPixel(rect.Left + 1, rect.MidY));
			Assert.Equal(SKColors.Black, bitmap.GetPixel(rect.Right - 1, rect.MidY));
			Assert.Equal(SKColors.White, bitmap.GetPixel(rect.Right + 0, rect.MidY));
			Assert.Equal(SKColors.White, bitmap.GetPixel(rect.Right + 1, rect.MidY));

			Assert.Equal(SKColors.White, bitmap.GetPixel(rect.MidX, rect.Top - 1));
			Assert.Equal(SKColors.Black, bitmap.GetPixel(rect.MidX, rect.Top + 0));
			Assert.Equal(SKColors.Black, bitmap.GetPixel(rect.MidX, rect.Top + 1));
			Assert.Equal(SKColors.Black, bitmap.GetPixel(rect.MidX, rect.Bottom - 1));
			Assert.Equal(SKColors.White, bitmap.GetPixel(rect.MidX, rect.Bottom + 0));
			Assert.Equal(SKColors.White, bitmap.GetPixel(rect.MidX, rect.Bottom + 1));
		}

		[SkippableFact]
		public void PostConcatIdentityIsCorrect()
		{
			var expected = new SKMatrix4x4(
				1, 2, 3, 0,
				0, 1, 4, 0,
				5, 6, 1, 0,
				0, 0, 0, 1);

			var input = expected;
			input.PostConcat(SKMatrix4x4.CreateIdentity());

			Assert.Equal(expected, input);
		}

		[SkippableFact]
		public void PreConcatIdentityIsCorrect()
		{
			var expected = new SKMatrix4x4(
				1, 2, 3, 0,
				0, 1, 4, 0,
				5, 6, 1, 0,
				0, 0, 0, 1);

			var input = expected;
			input.PreConcat(SKMatrix4x4.CreateIdentity());

			Assert.Equal(expected, input);
		}

		[SkippableFact]
		public void RotationIsCorrect()
		{
			var matrix = SKMatrix4x4.CreateRotationDegrees(1, 2, 3, 30);

			var expected = new Matrix4x4(
				0.875595033f, 0.420031041f, -0.2385524f, 0,
				-0.38175258f, 0.904303849f, 0.1910483f, 0,
				0.295970082f, -0.07621294f, 0.952151954f, 0,
				0, 0, 0, 1);

			AssertMatrix(expected, matrix);
		}

		[SkippableFact]
		public void CanvasRespectsMatrix()
		{
			using var bitmap = new SKBitmap(30, 30);
			using var canvas = new SKCanvas(bitmap);
			canvas.Clear(SKColors.White);

			var matrix = new SKMatrix4x4(
				1, 2, 3, 0,
				0, 1, 4, 0,
				5, 6, 1, 0,
				0, 0, 0, 1);

			canvas.Concat(matrix);

			AssertMatrix(matrix, canvas.TotalMatrix4x4);
		}
	}
}
