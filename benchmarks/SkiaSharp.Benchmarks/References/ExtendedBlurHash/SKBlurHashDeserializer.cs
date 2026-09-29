using System;
using System.Buffers;

using static SkiaSharp.Extended.SKBlurHashUtils;

namespace SkiaSharp.Extended
{
	internal static class SKBlurHashDeserializer
	{
		public static SKImage DeserializeImage(string blurHash, int width, int height, float punch = 1f)
		{
			using var bmp = DeserializeBitmap(blurHash.AsSpan(), width, height, punch);
			bmp.SetImmutable();
			return SKImage.FromBitmap(bmp);
		}

		public static SKImage DeserializeImage(ReadOnlySpan<char> blurHash, int width, int height, float punch = 1f)
		{
			using var bmp = DeserializeBitmap(blurHash, width, height, punch);
			bmp.SetImmutable();
			return SKImage.FromBitmap(bmp);
		}

		public static SKBitmap DeserializeBitmap(string blurHash, int width, int height, float punch = 1f)
		{
			return DeserializeBitmap(blurHash.AsSpan(), width, height, punch);
		}

		public static SKBitmap DeserializeBitmap(ReadOnlySpan<char> blurHash, int width, int height, float punch = 1f)
		{
			var data = ParseData(blurHash, punch);

			var bitmap = new SKBitmap(new SKImageInfo(width, height, SKColorType.Bgra8888));

			using var pixmap = bitmap.PeekPixels();
			DeserializePixelData(data, pixmap);

			data.Return();

			return bitmap;
		}

		private static void DeserializePixelData(Data data, SKPixmap pixmap)
		{
			var pixels = pixmap.GetPixelSpan<SKColor>();

			var height = pixmap.Height;
			var width = pixmap.Width;
			var comp = data.Components;
			var colors = data.Colors;

			var cosinesX = ArrayPool<double>.Shared.Rent(width * comp.X);
			var cosinesY = ArrayPool<double>.Shared.Rent(height * comp.Y);

			for (var y = 0; y < height; y++)
			{
				for (var x = 0; x < width; x++)
				{
					var r = 0.0;
					var g = 0.0;
					var b = 0.0;

					for (var j = 0; j < comp.Y; j++)
					{
						for (var i = 0; i < comp.X; i++)
						{
							var cosX = GetCos(cosinesX, i, comp.X, x, width);
							var cosY = GetCos(cosinesY, j, comp.Y, y, height);

							var basis = cosX * cosY;
							var color = colors[j * comp.X + i];

							r += color.R * basis;
							g += color.G * basis;
							b += color.B * basis;
						}
					}

					pixels[y * width + x] = new SKColor(
						(byte)LinearToSrgb(r),
						(byte)LinearToSrgb(g),
						(byte)LinearToSrgb(b));
				}
			}

			ArrayPool<double>.Shared.Return(cosinesX);
			ArrayPool<double>.Shared.Return(cosinesY);

			static double GetCos(double[] array, int x, int comp, int y, int size) =>
				array[x + (comp * y)] = Math.Cos(Math.PI * y * x / size);
		}

		private static Data ParseData(ReadOnlySpan<char> blurHash, float punch = 1f)
		{
			if (blurHash.Length < 6)
				throw new ArgumentException("BlurHash data must have at least 6 characters.", nameof(blurHash));

			// read components
			var compEnc = Base83.Decode(blurHash, 0, 1);
			var comp = new SKPointI(
				(compEnc % 9) + 1,
				(compEnc / 9) + 1);

			var expectedLength = 4 + 2 * comp.X * comp.Y;
			if (blurHash.Length != expectedLength)
				throw new ArgumentException($"BlurHash contains invalid data. Length should be {expectedLength} but was {blurHash.Length}.", nameof(blurHash));

			// read maximum AC
			var acMaxEnc = Base83.Decode(blurHash, 1, 1);
			var acMax = (acMaxEnc + 1) / 166.0;

			// build color array
			var colorsLength = comp.X * comp.Y;
			var colors = ArrayPool<Pixel>.Shared.Rent(colorsLength);

			// the first color
			var dcEnc = Base83.Decode(blurHash, 2, 4);
			colors[0] = DeserializeDC(dcEnc);

			// the rest of the colors
			for (var i = 1; i < colorsLength; i++)
			{
				var start = 4 + i * 2;
				var acEnc = Base83.Decode(blurHash, start, 2);
				colors[i] = DeserializeAC(acEnc, acMax * punch);
			}

			return new Data(colors, colors.AsSpan(0, colorsLength), comp);
		}

		private static Pixel DeserializeDC(int dc)
		{
			var r = dc >> 16;
			var g = (dc >> 8) & 255;
			var b = dc & 255;

			return new Pixel(
				SrgbToLinear(r),
				SrgbToLinear(g),
				SrgbToLinear(b));
		}

		private static Pixel DeserializeAC(int ac, double acMax)
		{
			var r = ac / (19 * 19);
			var g = (ac / 19) % 19;
			var b = ac % 19;

			return new Pixel(
				SignedPow((r - 9) / 9.0, 2.0) * acMax,
				SignedPow((g - 9) / 9.0, 2.0) * acMax,
				SignedPow((b - 9) / 9.0, 2.0) * acMax);
		}

		private readonly ref struct Data
		{
			private readonly Pixel[] array;

			public Data(Pixel[] poolArray, Span<Pixel> colors, SKPointI comp)
			{
				array = poolArray;
				Colors = colors;
				Components = comp;
			}

			public Span<Pixel> Colors { get; }

			public SKPointI Components { get; }

			public void Return() =>
				ArrayPool<Pixel>.Shared.Return(array);
		}
	}
}
