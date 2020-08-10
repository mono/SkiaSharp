using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SkiaSharp.Tests
{
	public class SKMatrixTest : SKTest
	{
		[SkippableFact]
		public void MatrixCanInvert()
		{
			var m = SKMatrix.CreateTranslation(10, 20);
			Assert.True(m.TryInvert(out var inverse));
			Assert.Equal(SKMatrix.CreateTranslation(-10, -20).Values, inverse.Values);
		}

		[SkippableFact]
		public void MatrixCanConcat()
		{
			var a = SKMatrix.CreateTranslation(10, 20);
			var b = SKMatrix.CreateTranslation(5, 7);

			var c = SKMatrix.Concat(a, b);
			
			Assert.Equal(SKMatrix.CreateTranslation(15, 27).Values, c.Values);
		}

		[SkippableFact]
		public void MatrixCanPreConcat()
		{
			var a = SKMatrix.CreateTranslation(10, 20);
			var b = SKMatrix.CreateTranslation(5, 7);

			var c = a.PreConcat(b);
			
			Assert.Equal(SKMatrix.CreateTranslation(15, 27).Values, c.Values);
		}

		[SkippableFact]
		public void MatrixCanPostConcat()
		{
			var a = SKMatrix.CreateTranslation(10, 20);
			var b = SKMatrix.CreateTranslation(5, 7);

			var c = a.PostConcat(b);
			
			Assert.Equal(SKMatrix.CreateTranslation(15, 27).Values, c.Values);
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

			var matrix = SKMatrix.CreateTranslation(10, 10);
			matrix.MapPoints(source, source);

			Assert.Equal(expectedResult, source);
		}

		[SkippableFact]
		public void MapRectCreatesModifiedRect()
		{
			var rect = SKRect.Create(2, 4, 6, 8);

			var matrix = SKMatrix.CreateTranslation(10, 12);
			var mapped = matrix.MapRect(rect);

			Assert.Equal(SKRect.Create(12, 16, 6, 8), mapped);
		}

		[SkippableFact]
		public void MatrixMapsPoint()
		{
			var matrix = SKMatrix.CreateTranslation(2, 4);
			var expectedResult = matrix.MapPoint(3, 6);

			Assert.Equal(new SKPoint(5, 10), expectedResult);
		}

		[SkippableFact]
		public void MatrixMapsVectors()
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

			var matrix = SKMatrix.CreateTranslation(10, 10);
			matrix.MapVectors(source, source);

			Assert.Equal(expectedResult, source);
		}

		[SkippableFact]
		public void MatrixMapsVector()
		{
			var matrix = SKMatrix.CreateTranslation(2, 4);
			var expectedResult = matrix.MapVector(3, 6);

			Assert.Equal(new SKPoint(3, 6), expectedResult);
		}

		[SkippableFact]
		public void ArrayConstructorSetsAllValues()
		{
			var values = new float[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
			var matrix = new SKMatrix(values);

			Assert.Equal(values, matrix.Values);
		}

		[SkippableFact]
		public void ArrayConstructorThrowsOnInvalidLength()
		{
			Assert.Throws<ArgumentException>(() => new SKMatrix(new float[] { 1f, 2f, 3f }));
		}

		[SkippableFact]
		public void ValuesPropertySetsAllValues()
		{
			var matrix = SKMatrix.Identity;

			var values = new float[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
			matrix.Values = values;

			Assert.Equal(values, matrix.Values);
		}

		[SkippableFact]
		public void ValuesPropertyThrowsOnInvalidLength()
		{
			var matrix = SKMatrix.Identity;

			Assert.Throws<ArgumentException>(() => matrix.Values = new float[] { 1f, 2f, 3f });
		}

		[SkippableFact]
		public void IsInvertibleIsFalseForNonInvertableMatrix()
		{
			var matrix = new SKMatrix(
				0.0f, 1.0f, 2.0f,
				0.0f, 1.0f, -3.40277175e+38f,
				1.00003040f, 1.0f, 0.0f);

			Assert.False(matrix.IsInvertible);
		}

		[SkippableFact]
		public void InverseOfMatrixIsCorrect()
		{
			var rowMajor = new float[] {
				1, 2, 3,
				0, 1, 4,
				5, 6, 1,
			};
			var expectedRowMajor = new float[] {
				-11.5f, 8, 2.5f,
				10, -7, -2,
				-2.5f, 2, 0.5f,
			};

			var matrix = new SKMatrix(rowMajor);
			var inverse = matrix.Invert();

			Assert.Equal(expectedRowMajor, inverse.Values);
		}

		[SkippableFact]
		public void EmptyMatrixForNonInvertableMatrix()
		{
			var matrix = SKMatrix.CreateScale(0, 1);

			Assert.Equal(SKMatrix.Empty, matrix.Invert());
		}

		[SkippableFact]
		public void CanRotatePoints()
		{
			var point = new SKPoint(40, -10);

			var matrix = SKMatrix.CreateRotationDegrees(90);
			var newPoint = matrix.MapPoint(point);

			Assert.Equal(10, newPoint.X, PRECISION);
			Assert.Equal(40, newPoint.Y, PRECISION);
		}

		[Obsolete]
		[SkippableFact]
		public void SetScaleTranslateWorksCorrectly()
		{
			var tempMatrix = SKMatrix.MakeIdentity();

			tempMatrix.Values = new float[] { 1, 0, 0, 0, 1, 0, 0, 0, 1 };

			SKMatrix.RotateDegrees(ref tempMatrix, 0);

			tempMatrix.SetScaleTranslate(1.2f, 1.0f, 0, 0);

			Assert.Equal(1.2f, tempMatrix.Values[0]);
		}
	}
}
