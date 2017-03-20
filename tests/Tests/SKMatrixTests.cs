using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace SkiaSharp.Tests
{
	public class SKMatrixTest : SKTest
	{
		private const float EPSILON = 0.0001f;
		private const int PRECISION = 4;

		[Test]
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

			Assert.AreEqual(expectedRowMajor, rowMajor);
		}

		[Test]
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

			Assert.AreEqual(rowMajor, matrix.ToRowMajor());
			Assert.AreEqual(determinant, matrix.Determinant());

			var inverted = matrix.Invert();

			Assert.AreEqual(1f / determinant, inverted.Determinant());

			var actualRowMajor = inverted.ToRowMajor();
			Assert.AreEqual(expectedRowMajor, actualRowMajor);
		}

		[Test]
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

			Assert.AreEqual(rowMajor, matrix44.Matrix.Values);

			matrix44 = SKMatrix44.CreateRotationDegrees(0, 0, 1, 45);
			Assert.AreEqual(SKMatrix.MakeRotationDegrees(45).Values, matrix44.Matrix.Values);
		}

		[Test]
		public void Matrix44MapsScalars()
		{
			// translate
			var matrixTranslate = SKMatrix44.CreateTranslate(10, 20, 0);

			var resultTranslateZero = matrixTranslate.MapScalars(0, 0, 0, 1);
			var resultTranslateValue = matrixTranslate.MapScalars(5, 25, 0, 1);

			Assert.AreEqual(new[] { 10f, 20f, 0f, 1f }, resultTranslateZero);
			Assert.AreEqual(new[] { 15f, 45f, 0f, 1f }, resultTranslateValue);

			// rotate
			var matrixRotate = SKMatrix44.CreateRotationDegrees(0, 1, 0, 90);

			var resultRotateZero = matrixRotate.MapScalars(0, 0, 0, 1);
			var resultRotateValue = matrixRotate.MapScalars(5, 25, 0, 1);

			Assert.AreEqual(new[] { 0f, 0f, 0f, 1f }, resultRotateZero);
			Assert.AreEqual(new[] { 0f, 25f, -5f, 1f }, resultRotateValue.Select(v => (int)(v / EPSILON) * EPSILON));
		}

		[Test]
		public void Matrix44MapsPoints()
		{
			// translate
			var matrixTranslate = SKMatrix44.CreateTranslate(10, 20, 0);

			var resultTranslateZero = matrixTranslate.MapPoint(SKPoint.Empty);
			var resultTranslateValue = matrixTranslate.MapPoint(new SKPoint(5, 25));

			Assert.AreEqual(new SKPoint(10f, 20f), resultTranslateZero);
			Assert.AreEqual(new SKPoint(15f, 45f), resultTranslateValue);

			// rotate
			var matrixRotate = SKMatrix44.CreateRotationDegrees(0, 1, 0, 90);

			var resultRotateZero = matrixRotate.MapPoint(SKPoint.Empty);
			var resultRotateValue = matrixRotate.MapPoint(new SKPoint(5, 25));

			Assert.AreEqual(new SKPoint(0f, 0f), resultRotateZero);
			Assert.AreEqual(0, resultRotateValue.X, PRECISION);
			Assert.AreEqual(25, resultRotateValue.Y, PRECISION);
		}

		[Test]
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

			Assert.AreEqual(results, actualResults);
		}
	}
}
