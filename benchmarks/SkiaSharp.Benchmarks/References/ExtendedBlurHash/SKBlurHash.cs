using System;

namespace SkiaSharp.Extended
{
	/// <summary>
	/// Provides methods to encode and decode BlurHash strings, a compact representation of image placeholders.
	/// </summary>
	public static class SKBlurHash
	{
		// Deserialize

		/// <summary>
		/// Decodes a BlurHash string into an <see cref="SKBitmap"/> of the specified dimensions.
		/// </summary>
		/// <param name="blurHash">The BlurHash string to decode.</param>
		/// <param name="width">The width of the resulting bitmap in pixels.</param>
		/// <param name="height">The height of the resulting bitmap in pixels.</param>
		/// <param name="punch">A factor that adjusts color intensity. Values greater than 1 increase contrast.</param>
		/// <returns>An <see cref="SKBitmap"/> representing the decoded BlurHash.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="blurHash"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="width"/> or <paramref name="height"/> is less than or equal to zero.</exception>
		public static SKBitmap DeserializeBitmap(string? blurHash, int width, int height, float punch = 1f)
		{
			if (blurHash == null)
				throw new ArgumentNullException(nameof(blurHash));
			if (width <= 0)
				throw new ArgumentOutOfRangeException(nameof(width));
			if (height <= 0)
				throw new ArgumentOutOfRangeException(nameof(height));

			return SKBlurHashDeserializer.DeserializeBitmap(blurHash.AsSpan(), width, height, punch);
		}

		/// <summary>
		/// Decodes a BlurHash string into an <see cref="SKBitmap"/> of the specified dimensions.
		/// </summary>
		/// <param name="blurHash">The BlurHash character span to decode.</param>
		/// <param name="width">The width of the resulting bitmap in pixels.</param>
		/// <param name="height">The height of the resulting bitmap in pixels.</param>
		/// <param name="punch">A factor that adjusts color intensity. Values greater than 1 increase contrast.</param>
		/// <returns>An <see cref="SKBitmap"/> representing the decoded BlurHash.</returns>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="width"/> or <paramref name="height"/> is less than or equal to zero.</exception>
		public static SKBitmap DeserializeBitmap(ReadOnlySpan<char> blurHash, int width, int height, float punch = 1f)
		{
			if (width <= 0)
				throw new ArgumentOutOfRangeException(nameof(width));
			if (height <= 0)
				throw new ArgumentOutOfRangeException(nameof(height));

			return SKBlurHashDeserializer.DeserializeBitmap(blurHash, width, height, punch);
		}

		/// <summary>
		/// Decodes a BlurHash string into an <see cref="SKImage"/> of the specified dimensions.
		/// </summary>
		/// <param name="blurHash">The BlurHash string to decode.</param>
		/// <param name="width">The width of the resulting image in pixels.</param>
		/// <param name="height">The height of the resulting image in pixels.</param>
		/// <param name="punch">A factor that adjusts color intensity. Values greater than 1 increase contrast.</param>
		/// <returns>An <see cref="SKImage"/> representing the decoded BlurHash.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="blurHash"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="width"/> or <paramref name="height"/> is less than or equal to zero.</exception>
		public static SKImage DeserializeImage(string? blurHash, int width, int height, float punch = 1f)
		{
			if (blurHash == null)
				throw new ArgumentNullException(nameof(blurHash));
			if (width <= 0)
				throw new ArgumentOutOfRangeException(nameof(width));
			if (height <= 0)
				throw new ArgumentOutOfRangeException(nameof(height));

			return SKBlurHashDeserializer.DeserializeImage(blurHash.AsSpan(), width, height, punch);
		}

		/// <summary>
		/// Decodes a BlurHash string into an <see cref="SKImage"/> of the specified dimensions.
		/// </summary>
		/// <param name="blurHash">The BlurHash character span to decode.</param>
		/// <param name="width">The width of the resulting image in pixels.</param>
		/// <param name="height">The height of the resulting image in pixels.</param>
		/// <param name="punch">A factor that adjusts color intensity. Values greater than 1 increase contrast.</param>
		/// <returns>An <see cref="SKImage"/> representing the decoded BlurHash.</returns>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="width"/> or <paramref name="height"/> is less than or equal to zero.</exception>
		public static SKImage DeserializeImage(ReadOnlySpan<char> blurHash, int width, int height, float punch = 1f)
		{
			if (width <= 0)
				throw new ArgumentOutOfRangeException(nameof(width));
			if (height <= 0)
				throw new ArgumentOutOfRangeException(nameof(height));

			return SKBlurHashDeserializer.DeserializeImage(blurHash, width, height, punch);
		}

		// Serialize

		/// <summary>
		/// Encodes an <see cref="SKBitmap"/> into a BlurHash string.
		/// </summary>
		/// <param name="bitmap">The bitmap to encode.</param>
		/// <param name="componentsX">The number of horizontal components (must be in the range [2..9]).</param>
		/// <param name="componentsY">The number of vertical components (must be in the range [2..9]).</param>
		/// <returns>A BlurHash string representing the image.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="bitmap"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="componentsX"/> or <paramref name="componentsY"/> is outside the range [2..9].</exception>
		public static string Serialize(SKBitmap? bitmap, int componentsX, int componentsY)
		{
			if (bitmap == null)
				throw new ArgumentNullException(nameof(bitmap));
			if (componentsX <= 1 || componentsX > 9)
				throw new ArgumentOutOfRangeException(nameof(componentsX), "The X components must be in the range [1..9].");
			if (componentsY <= 1 || componentsY > 9)
				throw new ArgumentOutOfRangeException(nameof(componentsY), "The Y components must be in the range [1..9].");

			return SKBlurHashSerializer.Serialize(bitmap, componentsX, componentsY);
		}

		/// <summary>
		/// Encodes an <see cref="SKImage"/> into a BlurHash string.
		/// </summary>
		/// <param name="image">The image to encode.</param>
		/// <param name="componentsX">The number of horizontal components (must be in the range [2..9]).</param>
		/// <param name="componentsY">The number of vertical components (must be in the range [2..9]).</param>
		/// <returns>A BlurHash string representing the image.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="image"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="componentsX"/> or <paramref name="componentsY"/> is outside the range [2..9].</exception>
		public static string Serialize(SKImage? image, int componentsX, int componentsY)
		{
			if (image == null)
				throw new ArgumentNullException(nameof(image));
			if (componentsX <= 1 || componentsX > 9)
				throw new ArgumentOutOfRangeException(nameof(componentsX), "The X components must be in the range [1..9].");
			if (componentsY <= 1 || componentsY > 9)
				throw new ArgumentOutOfRangeException(nameof(componentsY), "The Y components must be in the range [1..9].");

			return SKBlurHashSerializer.Serialize(image, componentsX, componentsY);
		}

		/// <summary>
		/// Encodes an <see cref="SKPixmap"/> into a BlurHash string.
		/// </summary>
		/// <param name="pixmap">The pixmap to encode.</param>
		/// <param name="componentsX">The number of horizontal components (must be in the range [2..9]).</param>
		/// <param name="componentsY">The number of vertical components (must be in the range [2..9]).</param>
		/// <returns>A BlurHash string representing the image.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="pixmap"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="componentsX"/> or <paramref name="componentsY"/> is outside the range [2..9].</exception>
		public static string Serialize(SKPixmap? pixmap, int componentsX, int componentsY)
		{
			if (pixmap == null)
				throw new ArgumentNullException(nameof(pixmap));
			if (componentsX <= 1 || componentsX > 9)
				throw new ArgumentOutOfRangeException(nameof(componentsX), "The X components must be in the range [1..9].");
			if (componentsY <= 1 || componentsY > 9)
				throw new ArgumentOutOfRangeException(nameof(componentsY), "The Y components must be in the range [1..9].");

			return SKBlurHashSerializer.Serialize(pixmap, componentsX, componentsY);
		}
	}
}
