using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace SkiaSharp.Tests
{
	public class SKBasicTypesTest : SKTest
	{
		private const float EPSILON = 0.0001f;
		private const int PRECISION = 4;

		[Test]
		public void RectanlgeHasCorrectProperties()
		{
			var rect = new SKRect(15, 25, 55, 75);

			Assert.AreEqual(15f, rect.Left);
			Assert.AreEqual(25f, rect.Top);
			Assert.AreEqual(55f, rect.Right);
			Assert.AreEqual(75f, rect.Bottom);

			Assert.AreEqual(40f, rect.Width);
			Assert.AreEqual(50f, rect.Height);

			Assert.AreEqual(35f, rect.MidX);
			Assert.AreEqual(50f, rect.MidY);
		}

		[Test]
		public void RectanlgeOffsetsCorrectly()
		{
			var expected = new SKRect(25, 30, 65, 80);

			var rect1 = new SKRect(15, 25, 55, 75);
			rect1.Location = new SKPoint(25, 30);

			var rect2 = new SKRect(15, 25, 55, 75);
			rect2.Offset (10, 5);

			Assert.AreEqual(expected, rect1);
			Assert.AreEqual(expected, rect2);
		}

		[Test]
		public void RectanlgeInflatesCorrectly()
		{
			var rect = new SKRect(15, 25, 55, 75);

			Assert.AreEqual(15f, rect.Left);
			Assert.AreEqual(25f, rect.Top);
			Assert.AreEqual(55f, rect.Right);
			Assert.AreEqual(75f, rect.Bottom);

			rect.Inflate(10, 20);

			Assert.AreEqual(5f, rect.Left);
			Assert.AreEqual(5f, rect.Top);
			Assert.AreEqual(65f, rect.Right);
			Assert.AreEqual(95f, rect.Bottom);
		}

		[Test]
		public void RectanlgeStandardizeCorrectly()
		{
			var rect = new SKRect(5, 5, 15, 15);
			Assert.AreEqual(10, rect.Width);
			Assert.AreEqual(10, rect.Height);

			Assert.AreEqual(rect, rect.Standardized);

			var negW = new SKRect(15, 5, 5, 15);
			Assert.AreEqual(-10, negW.Width);
			Assert.AreEqual(10, negW.Height);
			Assert.AreEqual(rect, negW.Standardized);

			var negH = new SKRect(5, 15, 15, 5);
			Assert.AreEqual(10, negH.Width);
			Assert.AreEqual(-10, negH.Height);
			Assert.AreEqual(rect, negH.Standardized);

			var negWH = new SKRect(15, 15, 5, 5);
			Assert.AreEqual(-10, negWH.Width);
			Assert.AreEqual(-10, negWH.Height);
			Assert.AreEqual(rect, negWH.Standardized);
		}

		[Test]
		public void RectanlgeAspectFitIsCorrect()
		{
			var bigRect = SKRect.Create(5, 5, 20, 20);
			var tallSize = new SKSize(5, 10);
			var wideSize = new SKSize(10, 5);

			var fitTall = bigRect.AspectFit(tallSize);
			Assert.AreEqual(5 + 5, fitTall.Left);
			Assert.AreEqual(5 + 0, fitTall.Top);
			Assert.AreEqual(10, fitTall.Width);
			Assert.AreEqual(20, fitTall.Height);

			var fitWide = bigRect.AspectFit(wideSize);
			Assert.AreEqual(5 + 0, fitWide.Left);
			Assert.AreEqual(5 + 5, fitWide.Top);
			Assert.AreEqual(20, fitWide.Width);
			Assert.AreEqual(10, fitWide.Height);
		}

		[Test]
		public void RectanlgeAspectFillIsCorrect()
		{
			var bigRect = SKRect.Create(5, 5, 20, 20);
			var tallSize = new SKSize(5, 10);
			var wideSize = new SKSize(10, 5);

			var fitTall = bigRect.AspectFill(tallSize);
			Assert.AreEqual(5 + 0, fitTall.Left);
			Assert.AreEqual(5 - 10, fitTall.Top);
			Assert.AreEqual(20, fitTall.Width);
			Assert.AreEqual(40, fitTall.Height);

			var fitWide = bigRect.AspectFill(wideSize);
			Assert.AreEqual(5 - 10, fitWide.Left);
			Assert.AreEqual(5 + 0, fitWide.Top);
			Assert.AreEqual(40, fitWide.Width);
			Assert.AreEqual(20, fitWide.Height);
		}
		
		[Test]
		public unsafe void FixedImageMaskIsHandledCorrectly()
		{
			byte rawMask = 1 << 7 | 0 << 6 | 0 << 5 | 1 << 4 | 1 << 3 | 0 << 2 | 1 << 1 | 1;
			var buffer = new byte[] { rawMask };
			var bounds = new SKRectI(0, 0, 8, 1);
			UInt32 rowBytes = 1;
			var format = SKMaskFormat.BW;

			fixed (void* bufferPtr = buffer)
			{
				var mask = SKMask.Create(buffer, bounds, rowBytes, format);

				Assert.AreEqual(rawMask, mask.GetAddr1(0, 0));
			}
		}

		[Test]
		public void MonochromeMaskBufferIsCopied()
		{
			byte rawMask = 1 << 7 | 0 << 6 | 0 << 5 | 1 << 4 | 1 << 3 | 0 << 2 | 1 << 1 | 1;
			var buffer = new byte[] { rawMask };
			var bounds = new SKRectI(0, 0, 8, 1);
			UInt32 rowBytes = 1;
			var format = SKMaskFormat.BW;

			var mask = SKMask.Create(buffer, bounds, rowBytes, format);

			Assert.AreEqual(rawMask, mask.GetAddr1(0, 0));

			mask.FreeImage();
		}

		[Test]
		public void Alpha8MaskBufferIsCopied()
		{
			var buffer = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
			var bounds = new SKRectI(0, 0, 4, 2);
			UInt32 rowBytes = 4;
			var format = SKMaskFormat.A8;

			var mask = SKMask.Create(buffer, bounds, rowBytes, format);

			Assert.AreEqual(buffer[0], mask.GetAddr8(0, 0));
			Assert.AreEqual(buffer[1], mask.GetAddr8(1, 0));
			Assert.AreEqual(buffer[2], mask.GetAddr8(2, 0));
			Assert.AreEqual(buffer[3], mask.GetAddr8(3, 0));
			Assert.AreEqual(buffer[4], mask.GetAddr8(0, 1));
			Assert.AreEqual(buffer[5], mask.GetAddr8(1, 1));
			Assert.AreEqual(buffer[6], mask.GetAddr8(2, 1));
			Assert.AreEqual(buffer[7], mask.GetAddr8(3, 1));

			mask.FreeImage();
		}

		[Test]
		public void ThirtyTwoBitMaskBufferIsCopied()
		{
			var buffer = new byte[]
			{
				0, 0, 255, 255,
				255, 0, 0, 255,
				0, 0, 255, 255,
				255, 0, 0, 255,
				0, 0, 255, 255,
				255, 0, 0, 255,
				0, 0, 255, 255,
				255, 0, 0, 255
			};
			var bounds = new SKRectI(0, 0, 4, 2);
			UInt32 rowBytes = 16;
			var format = SKMaskFormat.Argb32;

			var mask = SKMask.Create(buffer, bounds, rowBytes, format);

			var red = SKColors.Red;
			var blue = SKColors.Blue;
			Assert.AreEqual((uint)red, mask.GetAddr32(0, 0));
			Assert.AreEqual((uint)blue, mask.GetAddr32(1, 0));
			Assert.AreEqual((uint)red, mask.GetAddr32(2, 0));
			Assert.AreEqual((uint)blue, mask.GetAddr32(3, 0));
			Assert.AreEqual((uint)red, mask.GetAddr32(0, 1));
			Assert.AreEqual((uint)blue, mask.GetAddr32(1, 1));
			Assert.AreEqual((uint)red, mask.GetAddr32(2, 1));
			Assert.AreEqual((uint)blue, mask.GetAddr32(3, 1));

			mask.FreeImage();
		}

		[Test]
		public void Matrix44CreatesIdentity()
		{
			var matrix = SKMatrix44.MakeIdentity();

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

			var matrix = SKMatrix44.MakeFromRowMajor(rowMajor);

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

			var matrix44 = SKMatrix44.MakeFromRowMajor(rowMajor44);

			Assert.AreEqual(rowMajor, matrix44.Matrix.Values);

			matrix44 = SKMatrix44.MakeRotationDegrees(0, 0, 1, 45);
			Assert.AreEqual(SKMatrix.MakeRotationDegrees(45).Values, matrix44.Matrix.Values);
		}

		[Test]
		public void Matrix44MapsScalars()
		{
			// translate
			var matrixTranslate = SKMatrix44.MakeTranslate(10, 20, 0);

			var resultTranslateZero = matrixTranslate.MapScalars(0, 0, 0, 1);
			var resultTranslateValue = matrixTranslate.MapScalars(5, 25, 0, 1);

			Assert.AreEqual(new[] { 10f, 20f, 0f, 1f }, resultTranslateZero);
			Assert.AreEqual(new[] { 15f, 45f, 0f, 1f }, resultTranslateValue);

			// rotate
			var matrixRotate = SKMatrix44.MakeRotationDegrees(0, 1, 0, 90);

			var resultRotateZero = matrixRotate.MapScalars(0, 0, 0, 1);
			var resultRotateValue = matrixRotate.MapScalars(5, 25, 0, 1);

			Assert.AreEqual(new[] { 0f, 0f, 0f, 1f }, resultRotateZero);
			Assert.AreEqual(new[] { 0f, 25f, -5f, 1f }, resultRotateValue.Select(v => (int)(v / EPSILON) * EPSILON));
		}

		[Test]
		public void Matrix44MapsPoints()
		{
			// translate
			var matrixTranslate = SKMatrix44.MakeTranslate(10, 20, 0);

			var resultTranslateZero = matrixTranslate.MapPoint(SKPoint.Empty);
			var resultTranslateValue = matrixTranslate.MapPoint(new SKPoint(5, 25));

			Assert.AreEqual(new SKPoint(10f, 20f), resultTranslateZero);
			Assert.AreEqual(new SKPoint(15f, 45f), resultTranslateValue);

			// rotate
			var matrixRotate = SKMatrix44.MakeRotationDegrees(0, 1, 0, 90);

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

			var matrixTranslate = SKMatrix44.MakeTranslate(10, 25, 0);

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
