using System;

namespace SkiaSharp.Extended
{
	public static class SKPixelComparer
	{
		public static SKPixelComparisonResult Compare(string firstFilename, string secondFilename)
		{
			using var first = SKImage.FromEncodedData(firstFilename);
			using var second = SKImage.FromEncodedData(secondFilename);
			return Compare(first, second);
		}

		public static SKPixelComparisonResult Compare(SKBitmap first, SKBitmap second)
		{
			using var firstPixmap = first.PeekPixels();
			using var secondPixmap = second.PeekPixels();
			return Compare(firstPixmap, secondPixmap);
		}

		public static SKPixelComparisonResult Compare(SKPixmap first, SKPixmap second)
		{
			using var firstWrapper = SKImage.FromPixels(first);
			using var secondWrapper = SKImage.FromPixels(second);
			return Compare(firstWrapper, secondWrapper);
		}

		public static SKPixelComparisonResult Compare(SKImage first, SKImage second)
		{
			Validate(first, second);

			var width = first.Width;
			var height = first.Height;

			var totalPixels = width * height;
			var errorPixels = 0;
			var absoluteError = 0;

			using var firstBitmap = GetNormalizedBitmap(first);
			using var firstPixmap = firstBitmap.PeekPixels();
			var firstPixels = firstPixmap.GetPixelSpan<SKColor>();

			using var secondBitmap = GetNormalizedBitmap(second);
			using var secondPixmap = secondBitmap.PeekPixels();
			var secondPixels = secondPixmap.GetPixelSpan<SKColor>();

			for (var idx = 0; idx < totalPixels; idx++)
			{
				var firstPixel = firstPixels[idx];
				var secondPixel = secondPixels[idx];

				var r = Math.Abs(secondPixel.Red - firstPixel.Red);
				var g = Math.Abs(secondPixel.Green - firstPixel.Green);
				var b = Math.Abs(secondPixel.Blue - firstPixel.Blue);
				var d = r + g + b;

				absoluteError += d;
				if (d > 0)
					errorPixels++;
			}

			return new SKPixelComparisonResult(totalPixels, errorPixels, absoluteError);
		}

		public static SKPixelComparisonResult Compare(string firstFilename, string secondFilename, string maskFilename)
		{
			using var first = SKImage.FromEncodedData(firstFilename);
			using var second = SKImage.FromEncodedData(secondFilename);
			using var mask = SKImage.FromEncodedData(maskFilename);
			return Compare(first, second, mask);
		}

		public static SKPixelComparisonResult Compare(SKBitmap first, SKBitmap second, SKBitmap mask)
		{
			using var firstPixmap = first.PeekPixels();
			using var secondPixmap = second.PeekPixels();
			using var maskPixmap = mask.PeekPixels();
			return Compare(firstPixmap, secondPixmap, maskPixmap);
		}

		public static SKPixelComparisonResult Compare(SKPixmap first, SKPixmap second, SKPixmap mask)
		{
			using var firstWrapper = SKImage.FromPixels(first);
			using var secondWrapper = SKImage.FromPixels(second);
			using var maskWrapper = SKImage.FromPixels(mask);
			return Compare(firstWrapper, secondWrapper, maskWrapper);
		}

		public static SKPixelComparisonResult Compare(SKImage first, SKImage second, SKImage mask)
		{
			Validate(first, second);
			ValidateMask(first, mask);

			var width = first.Width;
			var height = first.Height;

			var totalPixels = width * height;
			var errorPixels = 0;
			var absoluteError = 0;

			using var firstBitmap = GetNormalizedBitmap(first);
			using var firstPixmap = firstBitmap.PeekPixels();
			var firstPixels = firstPixmap.GetPixelSpan<SKColor>();

			using var secondBitmap = GetNormalizedBitmap(second);
			using var secondPixmap = secondBitmap.PeekPixels();
			var secondPixels = secondPixmap.GetPixelSpan<SKColor>();

			using var maskBitmap = GetNormalizedBitmap(mask);
			using var maskPixmap = maskBitmap.PeekPixels();
			var maskPixels = maskPixmap.GetPixelSpan<SKColor>();

			for (var idx = 0; idx < totalPixels; idx++)
			{
				var firstPixel = firstPixels[idx];
				var secondPixel = secondPixels[idx];
				var maskPixel = maskPixels[idx];

				var r = Math.Abs(secondPixel.Red - firstPixel.Red);
				var g = Math.Abs(secondPixel.Green - firstPixel.Green);
				var b = Math.Abs(secondPixel.Blue - firstPixel.Blue);

				var d = 0;
				if (r > maskPixel.Red)
					d += r;
				if (g > maskPixel.Green)
					d += g;
				if (b > maskPixel.Blue)
					d += b;

				absoluteError += d;
				if (d > 0)
					errorPixels++;
			}

			return new SKPixelComparisonResult(totalPixels, errorPixels, absoluteError);
		}

		public static SKImage GenerateDifferenceMask(string firstFilename, string secondFilename)
		{
			using var first = SKImage.FromEncodedData(firstFilename);
			using var second = SKImage.FromEncodedData(secondFilename);
			return GenerateDifferenceMask(first, second);
		}

		public static SKImage GenerateDifferenceMask(SKBitmap first, SKBitmap second)
		{
			using var firstPixmap = first.PeekPixels();
			using var secondPixmap = second.PeekPixels();
			return GenerateDifferenceMask(firstPixmap, secondPixmap);
		}

		public static SKImage GenerateDifferenceMask(SKPixmap first, SKPixmap second)
		{
			using var firstWrapper = SKImage.FromPixels(first);
			using var secondWrapper = SKImage.FromPixels(second);
			return GenerateDifferenceMask(firstWrapper, secondWrapper);
		}

		public static SKImage GenerateDifferenceMask(SKImage first, SKImage second)
		{
			Validate(first, second);

			var width = first.Width;
			var height = first.Height;

			var totalPixels = width * height;

			using var firstBitmap = GetNormalizedBitmap(first);
			using var firstPixmap = firstBitmap.PeekPixels();
			var firstPixels = firstPixmap.GetPixelSpan<SKColor>();

			using var secondBitmap = GetNormalizedBitmap(second);
			using var secondPixmap = secondBitmap.PeekPixels();
			var secondPixels = secondPixmap.GetPixelSpan<SKColor>();

			var diffBitmap = new SKBitmap(new SKImageInfo(width, height));
			using var diffPixmap = diffBitmap.PeekPixels();
			var diffPixels = diffPixmap.GetPixelSpan<SKColor>();

			for (var idx = 0; idx < totalPixels; idx++)
			{
				var firstPixel = firstPixels[idx];
				var secondPixel = secondPixels[idx];

				var r = (byte)Math.Abs(secondPixel.Red - firstPixel.Red);
				var g = (byte)Math.Abs(secondPixel.Green - firstPixel.Green);
				var b = (byte)Math.Abs(secondPixel.Blue - firstPixel.Blue);

				diffPixels[idx] = (r + g + b) > 0 ? SKColors.White : SKColors.Black;
			}

			return SKImage.FromBitmap(diffBitmap);
		}

		private static void ValidateMask(SKImage first, SKImage mask)
		{
			_ = first ?? throw new ArgumentNullException(nameof(first));
			_ = mask ?? throw new ArgumentNullException(nameof(mask));

			var s1 = first.Info.Size;
			var s2 = mask.Info.Size;

			if (s1 != s2)
				throw new InvalidOperationException($"Unable to compare using mask of a different size: {s1.Width}x{s1.Height} vs {s2.Width}x{s2.Height}.");
		}

		private static void Validate(SKImage first, SKImage second)
		{
			_ = first ?? throw new ArgumentNullException(nameof(first));
			_ = second ?? throw new ArgumentNullException(nameof(second));

			var s1 = first.Info.Size;
			var s2 = second.Info.Size;

			if (s1 != s2)
				throw new InvalidOperationException($"Unable to compare images of different sizes: {s1.Width}x{s1.Height} vs {s2.Width}x{s2.Height}.");
		}

		private static SKBitmap GetNormalizedBitmap(SKImage image)
		{
			var width = image.Width;
			var height = image.Height;

			var bitmap = new SKBitmap(new SKImageInfo(width, height, SKColorType.Bgra8888));

			using (var canvas = new SKCanvas(bitmap))
				canvas.DrawImage(image, 0, 0);

			return bitmap;
		}
	}

	public class SKPixelComparisonResult
	{
		public SKPixelComparisonResult(int totalPixels, int errorPixelCount, int absoluteError)
		{
			TotalPixels = totalPixels;
			ErrorPixelCount = errorPixelCount;
			AbsoluteError = absoluteError;
		}

		public int TotalPixels { get; }

		public int ErrorPixelCount { get; }

		public double ErrorPixelPercentage =>
			(double)ErrorPixelCount / TotalPixels;

		public int AbsoluteError { get; }
	}
}
