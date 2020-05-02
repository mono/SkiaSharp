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
			var m = SKMatrix.MakeTranslation(10, 20);
			Assert.True(m.TryInvert(out var inverse));
			Assert.Equal(SKMatrix.MakeTranslation(-10, -20).Values, inverse.Values);
		}

		[SkippableFact]
		public void MatrixCanConcat()
		{
			var a = SKMatrix.MakeTranslation(10, 20);
			var b = SKMatrix.MakeTranslation(5, 7);

			var c = SKMatrix.Concat(a, b);

			Assert.Equal(SKMatrix.MakeTranslation(15, 27).Values, c.Values);
		}

		[SkippableFact]
		public void MatrixCanPreConcat()
		{
			var a = SKMatrix.MakeTranslation(10, 20);
			var b = SKMatrix.MakeTranslation(5, 7);

			var c = a.PreConcat(b);

			Assert.Equal(SKMatrix.MakeTranslation(15, 27).Values, c.Values);
		}

		[SkippableFact]
		public void MatrixCanPostConcat()
		{
			var a = SKMatrix.MakeTranslation(10, 20);
			var b = SKMatrix.MakeTranslation(5, 7);

			var c = a.PostConcat(b);

			Assert.Equal(SKMatrix.MakeTranslation(15, 27).Values, c.Values);
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
