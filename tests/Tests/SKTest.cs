using System;
using System.IO;
using System.Linq;
using Xunit;

namespace SkiaSharp.Tests
{
	public abstract class SKTest : BaseTest
	{
		protected const float EPSILON = 0.0001f;
		protected const int PRECISION = 4;

		private static readonly Random random = new Random();

		protected static Stream CreateTestStream(int length = 1024)
		{
			var bytes = new byte[length];
			random.NextBytes(bytes);
			return new MemoryStream(bytes);
		}

		protected static byte[] CreateTestData(int length = 1024)
		{
			var bytes = new byte[length];
			random.NextBytes(bytes);
			return bytes;
		}

		protected static SKStreamAsset CreateTestSKStream(int length = 1024)
		{
			var bytes = new byte[length];
			random.NextBytes(bytes);
			return new SKMemoryStream(bytes);
		}

		protected static void SaveSurface(SKSurface surface, string filename = "output.png")
		{
			using var image = surface.Snapshot();
			SaveImage(image, filename);
		}

		protected static void SaveBitmap(SKBitmap bmp, string filename = "output.png")
		{
			using var image = SKImage.FromBitmap(bmp);
			SaveImage(image, filename);
		}

		protected static void SaveImage(SKImage img, string filename = "output.png")
		{
			using var bitmap = new SKBitmap(img.Width, img.Height);
			using var canvas = new SKCanvas(bitmap);

			canvas.Clear(SKColors.Transparent);
			canvas.DrawImage(img, 0, 0);
			canvas.Flush();

			using var stream = File.OpenWrite(Path.Combine(PathToImages, filename));
			using var image = SKImage.FromBitmap(bitmap);
			using var data = image.Encode();

			data.SaveTo(stream);
		}

		protected static SKBitmap CreateTestBitmap(byte alpha = 255)
		{
			var bmp = new SKBitmap(40, 40);
			bmp.Erase(SKColors.Transparent);

			using (var canvas = new SKCanvas(bmp))
			{
				DrawTestBitmap(canvas, 40, 40, alpha);
			}

			return bmp;
		}

		protected static SKPicture CreateTestPicture(byte alpha = 255)
		{
			using var recorder = new SKPictureRecorder();
			using var canvas = recorder.BeginRecording(SKRect.Create(0, 0, 40, 40));

			DrawTestBitmap(canvas, 40, 40, alpha);

			return recorder.EndRecording();
		}

		private static void DrawTestBitmap(SKCanvas canvas, int width, int height, byte alpha = 255)
		{
			using var paint = new SKPaint();

			var x = width / 2;
			var y = height / 2;

			canvas.Clear(SKColors.Transparent);

			paint.Color = SKColors.Red.WithAlpha(alpha);
			canvas.DrawRect(SKRect.Create(0, 0, x, y), paint);

			paint.Color = SKColors.Green.WithAlpha(alpha);
			canvas.DrawRect(SKRect.Create(x, 0, x, y), paint);

			paint.Color = SKColors.Blue.WithAlpha(alpha);
			canvas.DrawRect(SKRect.Create(0, y, x, y), paint);

			paint.Color = SKColors.Yellow.WithAlpha(alpha);
			canvas.DrawRect(SKRect.Create(x, y, x, y), paint);
		}

		protected static void ValidateTestBitmap(SKBitmap bmp, byte alpha = 255)
		{
			Assert.NotNull(bmp);
			Assert.Equal(40, bmp.Width);
			Assert.Equal(40, bmp.Height);

			Assert.Equal(SKColors.Red.WithAlpha(alpha), bmp.GetPixel(10, 10));
			Assert.Equal(SKColors.Green.WithAlpha(alpha), bmp.GetPixel(30, 10));
			Assert.Equal(SKColors.Blue.WithAlpha(alpha), bmp.GetPixel(10, 30));
			Assert.Equal(SKColors.Yellow.WithAlpha(alpha), bmp.GetPixel(30, 30));
		}

		protected static void ValidateTestPixmap(SKPixmap pix, byte alpha = 255)
		{
			Assert.NotNull(pix);
			Assert.Equal(40, pix.Width);
			Assert.Equal(40, pix.Height);

			Assert.Equal(SKColors.Red.WithAlpha(alpha), pix.GetPixelColor(10, 10));
			Assert.Equal(SKColors.Green.WithAlpha(alpha), pix.GetPixelColor(30, 10));
			Assert.Equal(SKColors.Blue.WithAlpha(alpha), pix.GetPixelColor(10, 30));
			Assert.Equal(SKColors.Yellow.WithAlpha(alpha), pix.GetPixelColor(30, 30));
		}

		protected static void AssertSimilar(ReadOnlySpan<float> expected, ReadOnlySpan<float> actual, int precision = PRECISION)
		{
			var eTrimmed = expected.ToArray()
				.Select(v => (int)(v * precision) / precision);

			var aTrimmed = actual.ToArray()
				.Select(v => (int)(v * precision) / precision);

			Assert.Equal(eTrimmed, aTrimmed);
		}

		protected GlContext CreateGlContext()
		{
			try
			{
				if (IsLinux)
				{
					return new GlxContext();
				}
				else if (IsMac)
				{
					return new CglContext();
				}
				else if (IsWindows)
				{
					return new WglContext();
				}
				else
				{
					throw new PlatformNotSupportedException();
				}
			}
			catch (Exception ex)
			{
				throw new SkipException($"Unable to create GL context: {ex.Message}");
			}
		}

		protected static void VerifyImmediateFinalizers()
		{
			if (IsRuntimeMono)
				throw new SkipException("Mono does not guarantee finalizers are invoked immediately.");
		}

		protected static void VerifySupportsExceptionsInDelegates()
		{
			if (!IsWindows)
				throw new SkipException("Exceptions cannot be thrown in native delegates on non-Windows platforms.");
		}

		protected static void VerifySupportsMatchingTypefaces()
		{
			if (IsMac)
				throw new SkipException("macOS does not support matching typefaces.");
		}

		protected static void VerifySupportsMatchingTypefacesFromStreams()
		{
			if (IsLinux)
				throw new SkipException("Linux does not support matching typefaces from a typeface that was loaded from a stream.");
		}

		protected static bool IsEnumValueDeprected<T>(T value)
			where T : struct
		{
			var enumType = typeof(T);
			var enumValue = enumType.GetMember(value.ToString()).FirstOrDefault();

			var obsoleteAttribute = enumValue.GetCustomAttributes(typeof(ObsoleteAttribute), false).FirstOrDefault() as ObsoleteAttribute;

			return obsoleteAttribute?.IsError == true;
		}
	}
}
