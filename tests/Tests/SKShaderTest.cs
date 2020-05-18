using System;
using Xunit;

namespace SkiaSharp.Tests
{
	public class SKShaderTest : SKTest
	{
		[SkippableTheory]
		[InlineData(null)]
		[InlineData(new[] { 0f, 0.3f, 1f })]
		[InlineData(new[] { 0f, 0f, 1f })]
		[InlineData(new[] { 0f, 1f, 1f })]
		public void CanDrawWithCreateLinearGradientShader(float[] colorPositions)
		{
			var size = 160;
			var colors = new[] { SKColors.Blue, SKColors.Yellow, SKColors.Green };

			using var bitmap = new SKBitmap(new SKImageInfo(size, size));
			using var canvas = new SKCanvas(bitmap);
			using var p = new SKPaint
			{
				Shader = SKShader.CreateLinearGradient(new SKPoint(10, 10), new SKPoint(150, 10), colors, colorPositions, SKShaderTileMode.Clamp)
			};

			canvas.DrawRect(SKRect.Create(size, size), p);
		}

		[SkippableTheory]
		[InlineData(null)]
		[InlineData(new[] { 0f, 0.3f, 1f })]
		[InlineData(new[] { 0f, 0f, 1f })]
		[InlineData(new[] { 0f, 1f, 1f })]
		public void CanDrawWithCreateLinearGradientShaderColorF(float[] colorPositions)
		{
			var size = 160;
			var colors = new SKColorF[] { SKColors.Blue, SKColors.Yellow, SKColors.Green };
			using var colorspace = SKColorSpace.CreateSrgb();

			using var bitmap = new SKBitmap(new SKImageInfo(size, size));
			using var canvas = new SKCanvas(bitmap);
			using var p = new SKPaint
			{
				Shader = SKShader.CreateLinearGradient(new SKPoint(10, 10), new SKPoint(150, 10), colors, colorspace, colorPositions, SKShaderTileMode.Clamp)
			};

			canvas.DrawRect(SKRect.Create(size, size), p);
		}

		[SkippableFact]
		public void LinearShaderDrawColorCorrectly()
		{
			var size = 160;
			var colors = new[] { SKColors.Blue, SKColors.Gray, SKColors.Gray, SKColors.Red };
			var positions = new[] { 0.1f, 0.4f, 0.6f, 0.9f };

			using var bitmap = new SKBitmap(new SKImageInfo(size, size));
			using var canvas = new SKCanvas(bitmap);
			using var p = new SKPaint
			{
				Shader = SKShader.CreateLinearGradient(new SKPoint(10, 10), new SKPoint(150, 10), colors, positions, SKShaderTileMode.Clamp)
			};

			canvas.DrawRect(SKRect.Create(size, size), p);

			Assert.Equal(SKColors.Blue, bitmap.GetPixel(1, 0));
			Assert.Equal(SKColors.Gray, bitmap.GetPixel(80, 0));
			Assert.Equal(SKColors.Red, bitmap.GetPixel(159, 0));
		}

		[SkippableFact]
		public void LinearShaderDrawColorFCorrectly()
		{
			var size = 160;
			var colors = new SKColorF[] { SKColors.Blue, SKColors.Gray, SKColors.Gray, SKColors.Red };
			var positions = new[] { 0.1f, 0.4f, 0.6f, 0.9f };
			using var colorspace = SKColorSpace.CreateSrgb();

			using var bitmap = new SKBitmap(new SKImageInfo(size, size));
			using var canvas = new SKCanvas(bitmap);
			using var p = new SKPaint
			{
				Shader = SKShader.CreateLinearGradient(new SKPoint(10, 10), new SKPoint(150, 10), colors, colorspace, positions, SKShaderTileMode.Clamp)
			};

			canvas.DrawRect(SKRect.Create(size, size), p);

			Assert.Equal(SKColors.Blue, bitmap.GetPixel(1, 0));
			Assert.Equal(SKColors.Gray, bitmap.GetPixel(80, 0));
			Assert.Equal(SKColors.Red, bitmap.GetPixel(159, 0));
		}

		[SkippableFact]
		public void LinearGradientWithIncorrectColorPositionsThrows()
		{
			var colors = new[] { SKColors.Blue, SKColors.Yellow, SKColors.Green };
			var pos = new float[] { 0f, 0.1f, 0.2f, 0.3f, 0.4f, 1f };

			Assert.Throws<ArgumentException>(() => SKShader.CreateLinearGradient(new SKPoint(10, 10), new SKPoint(150, 10), colors, pos, SKShaderTileMode.Clamp));
		}

		[SkippableFact]
		public void LinearGradientWithIncorrectColorPositionsThrowsColorF()
		{
			var colors = new SKColorF[] { SKColors.Blue, SKColors.Yellow, SKColors.Green };
			var pos = new float[] { 0f, 0.1f, 0.2f, 0.3f, 0.4f, 1f };
			using var colorspace = SKColorSpace.CreateSrgb();

			Assert.Throws<ArgumentException>(() => SKShader.CreateLinearGradient(new SKPoint(10, 10), new SKPoint(150, 10), colors, colorspace, pos, SKShaderTileMode.Clamp));
		}

		[SkippableFact]
		public void CanDrawWithCreateSweepGradientShader()
		{
			var size = 160;
			var colors = new[] { SKColors.Blue, SKColors.Yellow, SKColors.Green };
			var pos = new[] { 0.0f, 0.25f, 0.50f };
			var modes = new[] { SKShaderTileMode.Clamp, SKShaderTileMode.Repeat, SKShaderTileMode.Mirror };

			var angles = new[] {
				(start: -330, end: -270),
				(start:   30, end:   90),
				(start:  390, end:  450),
				(start:  -30, end:  800),
			};

			using var bitmap = new SKBitmap(new SKImageInfo(690, 512));
			using var canvas = new SKCanvas(bitmap);
			using var p = new SKPaint();

			var r = SKRect.Create(size, size);

			foreach (var mode in modes)
			{
				using (new SKAutoCanvasRestore(canvas, true))
				{
					foreach (var (start, end) in angles)
					{
						p.Shader = SKShader.CreateSweepGradient(new SKPoint(size / 2f, size / 2f), colors, pos, mode, start, end);

						canvas.DrawRect(r, p);
						canvas.Translate(size * 1.1f, 0);
					}
				}

				canvas.Translate(0, size * 1.1f);
			}
		}

		[SkippableFact]
		public void CanDrawWithCreatePictureShader()
		{
			var breakSize = new SKSize(50, 10);
			var breakRect = SKRect.Create(breakSize);
			var indentHalf = 2;
			var tile = SKRect.Create(0, 0, breakSize.Width * 2 + indentHalf * 4, breakSize.Height * 2 + indentHalf * 4);

			using var bitmap = new SKBitmap(new SKImageInfo((int)(breakSize.Width * 5 + indentHalf * 2 * 5), (int)(breakSize.Height * 5 + indentHalf * 2 * 5)));
			using var canvas = new SKCanvas(bitmap);
			using var pictureRecorder = new SKPictureRecorder();
			using var pictureCanvas = pictureRecorder.BeginRecording(tile);
			using var pictureBreak = new SKPaint { Color = SKColors.DarkRed, Style = SKPaintStyle.Fill };

			pictureCanvas.Translate(indentHalf, indentHalf);
			pictureCanvas.DrawRect(breakRect, pictureBreak);
			pictureCanvas.Translate(breakSize.Width + indentHalf * 2, 0);
			pictureCanvas.DrawRect(breakRect, pictureBreak);
			pictureCanvas.Translate(-(breakSize.Width * 1.5F + indentHalf * 3), breakSize.Height + indentHalf * 2);
			pictureCanvas.DrawRect(breakRect, pictureBreak);
			pictureCanvas.Translate(breakSize.Width + indentHalf * 2, 0);
			pictureCanvas.DrawRect(breakRect, pictureBreak);
			pictureCanvas.Translate(breakSize.Width + indentHalf * 2, 0);
			pictureCanvas.DrawRect(breakRect, pictureBreak);

			using var picture = pictureRecorder.EndRecording();

			using var p = new SKPaint
			{
				Shader = SKShader.CreatePicture(picture, SKShaderTileMode.Repeat, SKShaderTileMode.Repeat, SKMatrix.Identity, tile)
			};
			var r = SKRect.Create(bitmap.Width, bitmap.Height);
			canvas.DrawRect(r, p);
		}
	}
}
