using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SkiaSharp.Tests
{
	public class SKMatrixTest : SKTest
	{
		private const float EPSILON = 0.0001f;
		private const int PRECISION = 4;

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

			matrix44 = SKMatrix44.CreateRotationDegrees(0, 0, 1, 45);
			Assert.Equal(SKMatrix.MakeRotationDegrees(45).Values, matrix44.Matrix.Values);
		}

		[SkippableFact]
		public void Matrix44MapsScalars()
		{
			// translate
			var matrixTranslate = SKMatrix44.CreateTranslate(10, 20, 0);

			var resultTranslateZero = matrixTranslate.MapScalars(0, 0, 0, 1);
			var resultTranslateValue = matrixTranslate.MapScalars(5, 25, 0, 1);

			Assert.Equal(new[] { 10f, 20f, 0f, 1f }, resultTranslateZero);
			Assert.Equal(new[] { 15f, 45f, 0f, 1f }, resultTranslateValue);

			// rotate
			var matrixRotate = SKMatrix44.CreateRotationDegrees(0, 1, 0, 90);

			var resultRotateZero = matrixRotate.MapScalars(0, 0, 0, 1);
			var resultRotateValue = matrixRotate.MapScalars(5, 25, 0, 1);

			Assert.Equal(new[] { 0f, 0f, 0f, 1f }, resultRotateZero);
			Assert.Equal(new[] { 0f, 25f, -5f, 1f }, resultRotateValue.Select(v => (int)(v / EPSILON) * EPSILON));
		}

		[SkippableFact]
		public void Matrix44MapsPoints()
		{
			// translate
			var matrixTranslate = SKMatrix44.CreateTranslate(10, 20, 0);

			var resultTranslateZero = matrixTranslate.MapPoint(SKPoint.Empty);
			var resultTranslateValue = matrixTranslate.MapPoint(new SKPoint(5, 25));

			Assert.Equal(new SKPoint(10f, 20f), resultTranslateZero);
			Assert.Equal(new SKPoint(15f, 45f), resultTranslateValue);

			// rotate
			var matrixRotate = SKMatrix44.CreateRotationDegrees(0, 1, 0, 90);

			var resultRotateZero = matrixRotate.MapPoint(SKPoint.Empty);
			var resultRotateValue = matrixRotate.MapPoint(new SKPoint(5, 25));

			Assert.Equal(new SKPoint(0f, 0f), resultRotateZero);
			Assert.Equal(0, resultRotateValue.X, PRECISION);
			Assert.Equal(25, resultRotateValue.Y, PRECISION);
		}

		[SkippableFact]
		public void Matrix44MapsPointsBulk()
		{
			var rnd = new Random();

			var matrixTranslate = SKMatrix44.CreateTranslate(10, 25, 0);

			// generate some points
			var points = new SKPoint[1000];
			var results = new SKPoint[points.Length];
			for (int i = 0; i < points.Length; i++)
			{
				points[i] = new SKPoint(rnd.Next(1000) / 10f, rnd.Next(1000) / 10f);
				results[i] = new SKPoint(points[i].X + 10, points[i].Y + 25);
			}

			var actualResults = matrixTranslate.MapPoints(points);

			Assert.Equal(results, actualResults);
		}

		[SkippableFact]
		public void MatrixCanInvert()
		{
			var m = SKMatrix.MakeTranslation(10, 20);
			Assert.True(m.TryInvert(out var inverse));
			Assert.Equal(SKMatrix.MakeTranslation(-10, -20).Values, inverse.Values);
		}

		[SkippableFact]
		public void MatrixCanConcat()
		{
			var a = SKMatrix.MakeTranslation(10, 20);
			var b = SKMatrix.MakeTranslation(5, 7);
			var c = new SKMatrix();

			SKMatrix.Concat(ref c, ref a, ref b);
			
			Assert.Equal(SKMatrix.MakeTranslation(15, 27).Values, c.Values);
		}

		[SkippableFact]
		public void MatrixCanPreConcat()
		{
			var a = SKMatrix.MakeTranslation(10, 20);
			var b = SKMatrix.MakeTranslation(5, 7);

			SKMatrix.PreConcat(ref a, ref b);
			
			Assert.Equal(SKMatrix.MakeTranslation(15, 27).Values, a.Values);
		}

		[SkippableFact]
		public void MatrixCanPostConcat()
		{
			var a = SKMatrix.MakeTranslation(10, 20);
			var b = SKMatrix.MakeTranslation(5, 7);

			SKMatrix.PostConcat(ref a, ref b);
			
			Assert.Equal(SKMatrix.MakeTranslation(15, 27).Values, a.Values);
		}

		[SkippableFact]
		public void SKRotationScaleMatrixTranslationToMatrixIsCorrect()
		{
			var m = SKMatrix.MakeTranslation(5, 7);
			var rsm = SKRotationScaleMatrix.CreateTranslate(5, 7).ToMatrix();

			Assert.Equal(m.Values, rsm.Values);
		}

		[SkippableFact]
		public void SKRotationScaleMatrixRotationToMatrixIsCorrect()
		{
			var m = SKMatrix.MakeRotationDegrees(45);
			var rsm = SKRotationScaleMatrix.CreateRotationDegrees(45, 0, 0).ToMatrix();

			Assert.Equal(m.Values, rsm.Values);
		}

		[SkippableFact]
		public void SKRotationScaleMatrixScaleToMatrixIsCorrect()
		{
			var m = SKMatrix.MakeScale(3.5f, 3.5f);
			var rsm = SKRotationScaleMatrix.CreateScale(3.5f).ToMatrix();

			Assert.Equal(m.Values, rsm.Values);
		}

		[SkippableFact]
		public void MatrixMapsPoints()
		{
			var source = new[] {
				new SKPoint(0, 0),
				new SKPoint(-10, -10),
				new SKPoint(-10, 10),
				new SKPoint(10, -10),
				new SKPoint(10, 10),
				new SKPoint(-5, -5),
				new SKPoint(-5, 5),
				new SKPoint(5, -5),
				new SKPoint(5, 5),
			};

			var expectedResult = new[] {
				new SKPoint(10, 10),
				new SKPoint(0, 0),
				new SKPoint(0, 20),
				new SKPoint(20, 0),
				new SKPoint(20, 20),
				new SKPoint(5, 5),
				new SKPoint(5, 15),
				new SKPoint(15, 5),
				new SKPoint(15, 15),
			};

			var matrix = SKMatrix.MakeTranslation(10, 10);
			matrix.MapPoints(source, source);

			Assert.Equal(expectedResult, source);
		}
	}
}
