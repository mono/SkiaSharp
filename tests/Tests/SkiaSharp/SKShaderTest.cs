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
		[Trait(Traits.Category.Key, Traits.Category.Values.Smoke)]
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

		// Gradient Interpolation Tests

		[SkippableTheory]
		[InlineData(SKGradientInterpolationColorSpace.Destination)]
		[InlineData(SKGradientInterpolationColorSpace.SrgbLinear)]
		[InlineData(SKGradientInterpolationColorSpace.OKLab)]
		[InlineData(SKGradientInterpolationColorSpace.OKLCH)]
		[InlineData(SKGradientInterpolationColorSpace.Srgb)]
		[InlineData(SKGradientInterpolationColorSpace.HSL)]
		[InlineData(SKGradientInterpolationColorSpace.HWB)]
		[InlineData(SKGradientInterpolationColorSpace.Lab)]
		[InlineData(SKGradientInterpolationColorSpace.LCH)]
		public void LinearGradientWithInterpolationColorSpaceCreatesShader(SKGradientInterpolationColorSpace colorSpace)
		{
			var size = 160;
			var colors = new SKColorF[] { new(1, 0, 0), new(0, 0, 1) };
			using var cs = SKColorSpace.CreateSrgb();

			var interpolation = new SKGradientInterpolation { ColorSpace = colorSpace };

			using var shader = SKShader.CreateLinearGradient(
				new SKPoint(0, 0), new SKPoint(size, 0),
				colors, cs, null, SKShaderTileMode.Clamp, interpolation);

			Assert.NotNull(shader);

			using var bitmap = new SKBitmap(new SKImageInfo(size, size));
			using var canvas = new SKCanvas(bitmap);
			using var paint = new SKPaint { Shader = shader };
			canvas.DrawRect(SKRect.Create(size, size), paint);

			var left = bitmap.GetPixel(0, 0);
			var right = bitmap.GetPixel(size - 1, 0);
			Assert.InRange(left.Red, 250, 255);
			Assert.InRange(right.Blue, 250, 255);
		}

		[SkippableTheory]
		[InlineData(SKGradientInterpolationColorSpace.OKLabGamutMap)]
		[InlineData(SKGradientInterpolationColorSpace.OKLCHGamutMap)]
		public void LinearGradientWithGamutMapColorSpaceCreatesShader(SKGradientInterpolationColorSpace colorSpace)
		{
			var size = 160;
			var colors = new SKColorF[] { new(1, 0, 0), new(0, 0, 1) };
			using var cs = SKColorSpace.CreateSrgb();

			var interpolation = new SKGradientInterpolation { ColorSpace = colorSpace };

			using var shader = SKShader.CreateLinearGradient(
				new SKPoint(0, 0), new SKPoint(size, 0),
				colors, cs, null, SKShaderTileMode.Clamp, interpolation);

			Assert.NotNull(shader);

			using var bitmap = new SKBitmap(new SKImageInfo(size, size));
			using var canvas = new SKCanvas(bitmap);
			using var paint = new SKPaint { Shader = shader };
			canvas.DrawRect(SKRect.Create(size, size), paint);
		}

		[SkippableFact]
		public void LinearGradientOKLabInterpolationAvoidsMuddyMiddle()
		{
			var size = 256;
			var colors = new SKColorF[] { new(0, 1, 0), new(0, 0, 1) };
			using var cs = SKColorSpace.CreateSrgb();

			var srgbInterp = new SKGradientInterpolation
			{
				ColorSpace = SKGradientInterpolationColorSpace.Destination
			};
			var oklabInterp = new SKGradientInterpolation
			{
				ColorSpace = SKGradientInterpolationColorSpace.OKLab
			};

			using var srgbShader = SKShader.CreateLinearGradient(
				new SKPoint(0, 0), new SKPoint(size, 0),
				colors, cs, null, SKShaderTileMode.Clamp, srgbInterp);
			using var oklabShader = SKShader.CreateLinearGradient(
				new SKPoint(0, 0), new SKPoint(size, 0),
				colors, cs, null, SKShaderTileMode.Clamp, oklabInterp);

			using var srgbBitmap = new SKBitmap(new SKImageInfo(size, 1));
			using var oklabBitmap = new SKBitmap(new SKImageInfo(size, 1));
			using var srgbCanvas = new SKCanvas(srgbBitmap);
			using var oklabCanvas = new SKCanvas(oklabBitmap);
			using var srgbPaint = new SKPaint { Shader = srgbShader };
			using var oklabPaint = new SKPaint { Shader = oklabShader };

			srgbCanvas.DrawRect(SKRect.Create(size, 1), srgbPaint);
			oklabCanvas.DrawRect(SKRect.Create(size, 1), oklabPaint);

			var srgbMid = srgbBitmap.GetPixel(size / 2, 0);
			var oklabMid = oklabBitmap.GetPixel(size / 2, 0);

			// OKLab should produce a more saturated (less gray) midpoint than sRGB
			int SaturationProxy(SKColor c) => Math.Max(c.Red, Math.Max(c.Green, c.Blue)) - Math.Min(c.Red, Math.Min(c.Green, c.Blue));
			Assert.True(SaturationProxy(oklabMid) > SaturationProxy(srgbMid),
				$"OKLab midpoint should be more saturated than sRGB. OKLab={oklabMid}, sRGB={srgbMid}");
		}

		[SkippableFact]
		public void RadialGradientWithInterpolationCreatesShader()
		{
			var size = 160;
			var colors = new SKColorF[] { new(1, 0, 0), new(0, 0, 1) };
			using var cs = SKColorSpace.CreateSrgb();

			var interpolation = new SKGradientInterpolation
			{
				ColorSpace = SKGradientInterpolationColorSpace.OKLab
			};

			using var shader = SKShader.CreateRadialGradient(
				new SKPoint(80, 80), 80,
				colors, cs, null, SKShaderTileMode.Clamp, interpolation);

			Assert.NotNull(shader);

			using var bitmap = new SKBitmap(new SKImageInfo(size, size));
			using var canvas = new SKCanvas(bitmap);
			using var paint = new SKPaint { Shader = shader };
			canvas.DrawRect(SKRect.Create(size, size), paint);

			Assert.InRange(bitmap.GetPixel(80, 80).Red, 250, 255);
		}

		[SkippableFact]
		public void SweepGradientWithInterpolationCreatesShader()
		{
			var size = 160;
			var colors = new SKColorF[] { new(1, 0, 0), new(0, 1, 0), new(0, 0, 1) };
			using var cs = SKColorSpace.CreateSrgb();

			var interpolation = new SKGradientInterpolation
			{
				ColorSpace = SKGradientInterpolationColorSpace.OKLCH,
				HueMethod = SKGradientInterpolationHueMethod.Shorter
			};

			using var shader = SKShader.CreateSweepGradient(
				new SKPoint(80, 80), colors, cs, null,
				SKShaderTileMode.Clamp, 0, 360, interpolation);

			Assert.NotNull(shader);

			using var bitmap = new SKBitmap(new SKImageInfo(size, size));
			using var canvas = new SKCanvas(bitmap);
			using var paint = new SKPaint { Shader = shader };
			canvas.DrawRect(SKRect.Create(size, size), paint);
		}

		[SkippableFact]
		public void TwoPointConicalGradientWithInterpolationCreatesShader()
		{
			var size = 160;
			var colors = new SKColorF[] { new(1, 1, 0), new(0, 1, 1) };
			using var cs = SKColorSpace.CreateSrgb();

			var interpolation = new SKGradientInterpolation
			{
				ColorSpace = SKGradientInterpolationColorSpace.SrgbLinear
			};

			using var shader = SKShader.CreateTwoPointConicalGradient(
				new SKPoint(80, 80), 10,
				new SKPoint(80, 80), 80,
				colors, cs, null, SKShaderTileMode.Clamp, interpolation);

			Assert.NotNull(shader);

			using var bitmap = new SKBitmap(new SKImageInfo(size, size));
			using var canvas = new SKCanvas(bitmap);
			using var paint = new SKPaint { Shader = shader };
			canvas.DrawRect(SKRect.Create(size, size), paint);
		}

		[SkippableFact]
		public void LinearGradientWithInterpolationAndLocalMatrixCreatesShader()
		{
			var size = 160;
			var colors = new SKColorF[] { new(1, 0, 0), new(0, 0, 1) };
			using var cs = SKColorSpace.CreateSrgb();

			var interpolation = new SKGradientInterpolation
			{
				ColorSpace = SKGradientInterpolationColorSpace.OKLab
			};
			var matrix = SKMatrix.CreateRotationDegrees(45, 80, 80);

			using var shader = SKShader.CreateLinearGradient(
				new SKPoint(0, 0), new SKPoint(size, 0),
				colors, cs, null, SKShaderTileMode.Clamp, interpolation, matrix);

			Assert.NotNull(shader);

			using var bitmap = new SKBitmap(new SKImageInfo(size, size));
			using var canvas = new SKCanvas(bitmap);
			using var paint = new SKPaint { Shader = shader };
			canvas.DrawRect(SKRect.Create(size, size), paint);
		}

		[SkippableTheory]
		[InlineData(SKGradientInterpolationHueMethod.Shorter)]
		[InlineData(SKGradientInterpolationHueMethod.Longer)]
		[InlineData(SKGradientInterpolationHueMethod.Increasing)]
		[InlineData(SKGradientInterpolationHueMethod.Decreasing)]
		public void SweepGradientWithHueMethodCreatesShader(SKGradientInterpolationHueMethod hueMethod)
		{
			var size = 160;
			var colors = new SKColorF[] { new(1, 0, 0), new(0, 0, 1) };
			using var cs = SKColorSpace.CreateSrgb();

			var interpolation = new SKGradientInterpolation
			{
				ColorSpace = SKGradientInterpolationColorSpace.OKLCH,
				HueMethod = hueMethod
			};

			using var shader = SKShader.CreateSweepGradient(
				new SKPoint(80, 80), colors, cs, null,
				SKShaderTileMode.Clamp, 0, 360, interpolation);

			Assert.NotNull(shader);
		}

		[SkippableFact]
		public void GradientInterpolationStructDefaultValues()
		{
			var interpolation = new SKGradientInterpolation();

			Assert.Equal(SKGradientInterpolationColorSpace.Destination, interpolation.ColorSpace);
			Assert.Equal(SKGradientInterpolationHueMethod.Shorter, interpolation.HueMethod);
			Assert.False(interpolation.InPremul);
		}

		[SkippableFact]
		public void GradientInterpolationStructEquality()
		{
			var a = new SKGradientInterpolation
			{
				ColorSpace = SKGradientInterpolationColorSpace.OKLab,
				HueMethod = SKGradientInterpolationHueMethod.Shorter,
				InPremul = false
			};
			var b = new SKGradientInterpolation
			{
				ColorSpace = SKGradientInterpolationColorSpace.OKLab,
				HueMethod = SKGradientInterpolationHueMethod.Shorter,
				InPremul = false
			};
			var c = new SKGradientInterpolation
			{
				ColorSpace = SKGradientInterpolationColorSpace.Srgb,
				HueMethod = SKGradientInterpolationHueMethod.Longer,
				InPremul = true
			};

			Assert.Equal(a, b);
			Assert.True(a == b);
			Assert.Equal(a.GetHashCode(), b.GetHashCode());
			Assert.NotEqual(a, c);
			Assert.True(a != c);
		}

		[SkippableFact]
		public void LinearGradientWithNullColorsThrowsInterpolation()
		{
			using var cs = SKColorSpace.CreateSrgb();
			var interpolation = new SKGradientInterpolation { ColorSpace = SKGradientInterpolationColorSpace.OKLab };

			Assert.Throws<ArgumentNullException>(() => SKShader.CreateLinearGradient(
				new SKPoint(0, 0), new SKPoint(100, 0), null, cs, null, SKShaderTileMode.Clamp, interpolation));
		}

		[SkippableFact]
		public void LinearGradientWithPremulInterpolationCreatesShader()
		{
			var size = 160;
			var colors = new SKColorF[] { new(1, 0, 0, 0.5f), new(0, 0, 1, 0.5f) };
			using var cs = SKColorSpace.CreateSrgb();

			var interpolation = new SKGradientInterpolation
			{
				ColorSpace = SKGradientInterpolationColorSpace.OKLab,
				InPremul = true
			};

			using var shader = SKShader.CreateLinearGradient(
				new SKPoint(0, 0), new SKPoint(size, 0),
				colors, cs, null, SKShaderTileMode.Clamp, interpolation);

			Assert.NotNull(shader);

			using var bitmap = new SKBitmap(new SKImageInfo(size, size, SKColorType.Rgba8888, SKAlphaType.Premul));
			using var canvas = new SKCanvas(bitmap);
			using var paint = new SKPaint { Shader = shader };
			canvas.Clear(SKColors.Transparent);
			canvas.DrawRect(SKRect.Create(size, size), paint);
		}

		[SkippableFact]
		public void LinearGradientWithIncorrectColorPositionsThrowsInterpolation()
		{
			var colors = new SKColorF[] { new(1, 0, 0), new(0, 1, 0), new(0, 0, 1) };
			var pos = new float[] { 0f, 0.1f, 0.2f, 0.3f, 0.4f, 1f };
			using var cs = SKColorSpace.CreateSrgb();
			var interpolation = new SKGradientInterpolation { ColorSpace = SKGradientInterpolationColorSpace.OKLab };

			Assert.Throws<ArgumentException>(() => SKShader.CreateLinearGradient(
				new SKPoint(0, 0), new SKPoint(100, 0), colors, cs, pos, SKShaderTileMode.Clamp, interpolation));
		}
	}
}
