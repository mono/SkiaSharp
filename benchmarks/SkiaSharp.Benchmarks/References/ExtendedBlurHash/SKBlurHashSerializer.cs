using System;
using System.Buffers;

using static SkiaSharp.Extended.SKBlurHashUtils;

namespace SkiaSharp.Extended
{
	internal static class SKBlurHashSerializer
	{
		public static string Serialize(SKBitmap image, int componentsX, int componentsY)
		{
			using var pixmap = image.PeekPixels();
			return Serialize(pixmap, componentsX, componentsY);
		}

		public static string Serialize(SKPixmap image, int componentsX, int componentsY)
		{
			using var wrapper = SKImage.FromPixels(image);
			return Serialize(wrapper, componentsX, componentsY);
		}

		public static string Serialize(SKImage image, int componentsX, int componentsY)
		{
			var width = image.Width;
			var height = image.Height;
			var linearPixels = GetLinearPixels(image);

			var scale = 1.0 / (width * height);

			var colorsLength = componentsX * componentsY;
			var colors = ArrayPool<Pixel>.Shared.Rent(colorsLength);

			for (var j = 0; j < componentsY; j++)
			{
				for (var i = 0; i < componentsX; i++)
				{
					var normalisation = (i == 0 && j == 0) ? 1 : 2;

					var r = 0.0;
					var g = 0.0;
					var b = 0.0;

					for (var x = 0; x < width; x++)
					{
						for (var y = 0; y < height; y++)
						{
							var pixel = linearPixels[y * width + x];

							var basis = normalisation * Math.Cos(Math.PI * i * x / width) * Math.Cos(Math.PI * j * y / height);

							r += basis * pixel.R;
							g += basis * pixel.G;
							b += basis * pixel.B;
						}
					}

					var color = new Pixel(
						r * scale,
						g * scale,
						b * scale);

					colors[j * componentsX + i] = color;
				}
			}

			var hash = SerializePixelData(colors.AsSpan(0, colorsLength), componentsX, componentsY);

			ArrayPool<Pixel>.Shared.Return(colors);
			ArrayPool<Pixel>.Shared.Return(linearPixels);

			return hash;
		}

		private static Pixel[] GetLinearPixels(SKImage image)
		{
			var height = image.Height;
			var width = image.Width;

			var linearPixels = ArrayPool<Pixel>.Shared.Rent(width * height);

			using (var bitmap = new SKBitmap(new SKImageInfo(width, height, SKColorType.Bgra8888)))
			{
				using (var canvas = new SKCanvas(bitmap))
				{
					canvas.DrawImage(image, 0, 0, SKSamplingOptions.Default);
				}

				using (var pixmap = bitmap.PeekPixels())
				{
					var pixels = pixmap.GetPixelSpan<SKColor>();
					for (var i = 0; i < pixels.Length; i++)
					{
						var pixel = pixels[i];
						linearPixels[i] = new Pixel(
							SrgbToLinear(pixel.Red),
							SrgbToLinear(pixel.Green),
							SrgbToLinear(pixel.Blue));
					}
				}
			}

			return linearPixels;
		}

		private static string SerializePixelData(Span<Pixel> colors, int componentsX, int componentsY)
		{
			var dc = colors[0];
			var ac = colors.Slice(1);

			var expectedLength = 4 + 2 * colors.Length;
			var chars = ArrayPool<char>.Shared.Rent(expectedLength);

			// [0] write the size
			var size = (componentsX - 1) + (componentsY - 1) * 9;
			Base83.Encode(size, 1, chars, 0);

			// calculate and write the maximum AC
			double acMax;
			if (ac.Length > 0)
			{
				// get the max of all X, Y, Z out of all AC
				var actualMax = 0.0;
				for (var i = 0; i < ac.Length; i++)
				{
					actualMax = Math.Max(Math.Abs(ac[i].R), actualMax);
					actualMax = Math.Max(Math.Abs(ac[i].G), actualMax);
					actualMax = Math.Max(Math.Abs(ac[i].B), actualMax);
				}

				// [1] write quantaised AC
				var quantisedMax = Clamp(0, 82, (int)(actualMax * 166.0 - 0.5));
				Base83.Encode(quantisedMax, 1, chars, 1);

				acMax = (quantisedMax + 1) / 166.0;
			}
			else
			{
				// [1] write 0
				Base83.Encode(0, 1, chars, 1);

				acMax = 1.0;
			}

			// [2..5] write DC
			Base83.Encode(SerializeDC(dc), 4, chars, 2);

			// [6..] write AC
			for (int i = 0, start = 6; i < ac.Length; i++, start += 2)
			{
				Base83.Encode(SerializeAC(ac[i], acMax), 2, chars, start);
			}

			// create the string
			var hash = new string(chars, 0, expectedLength);

			ArrayPool<char>.Shared.Return(chars);

			return hash;
		}

		private static int SerializeDC(Pixel color)
		{
			var r = LinearToSrgb(color.R);
			var g = LinearToSrgb(color.G);
			var b = LinearToSrgb(color.B);

			return (r << 16) + (g << 8) + b;
		}

		private static int SerializeAC(Pixel color, double acMax)
		{
			var r = Clamp(0, 18, (int)(SignedPow(color.R / acMax, 0.5) * 9.0 + 9.5));
			var g = Clamp(0, 18, (int)(SignedPow(color.G / acMax, 0.5) * 9.0 + 9.5));
			var b = Clamp(0, 18, (int)(SignedPow(color.B / acMax, 0.5) * 9.0 + 9.5));

			return (r * 19 * 19) + (g * 19) + b;
		}
	}
}
