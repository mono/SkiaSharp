using System;
using System.Runtime.InteropServices;

namespace SkiaSharp.Tests.Shared;

/// <summary>Provides shared PNG decoding, encoding, comparison and difference generation.</summary>
public static class GoldenImage
{
	public static SKImage Decode(byte[] png, string location)
	{
		if (png == null) throw new ArgumentNullException(nameof(png));
		return TryDecode(png)
			?? throw new InvalidOperationException("Failed to decode golden image '" + location + "'.");
	}

	public static SKImage? TryDecode(byte[] png)
	{
		if (png == null) throw new ArgumentNullException(nameof(png));
		return SKImage.FromEncodedData(png);
	}

	public static byte[] Encode(SKImage image)
	{
		if (image == null) throw new ArgumentNullException(nameof(image));
		using var data = image.Encode(SKEncodedImageFormat.Png, 100);
		return data.ToArray();
	}

	public static byte[] EncodePixels(byte[] rgba, SKImageInfo info)
	{
		if (rgba == null) throw new ArgumentNullException(nameof(rgba));
		var handle = GCHandle.Alloc(rgba, GCHandleType.Pinned);
		try
		{
			using var pixmap = new SKPixmap(info, handle.AddrOfPinnedObject());
			using var image = SKImage.FromPixels(pixmap);
			return Encode(image);
		}
		finally
		{
			handle.Free();
		}
	}

	public static GoldenComparison Compare(SKImage expected, SKImage actual, GoldenComparisonOptions options)
	{
		if (expected == null) throw new ArgumentNullException(nameof(expected));
		if (actual == null) throw new ArgumentNullException(nameof(actual));
		ValidateOptions(options);
		if (expected.Width != actual.Width || expected.Height != actual.Height)
			throw new InvalidOperationException(
				$"Unable to compare images of different sizes: {expected.Width}x{expected.Height} vs {actual.Width}x{actual.Height}.");
		if (options.ExpectedSize is { } size &&
			(actual.Width != size.Width || actual.Height != size.Height))
			throw new InvalidOperationException(
				$"Image is {actual.Width}x{actual.Height}, expected {size.Width}x{size.Height}.");

		return new GoldenComparison(
			SKPixelComparer.Compare(expected, actual, options.ChannelTolerance), options);
	}

	public static SKImage Difference(SKImage expected, SKImage actual, GoldenComparisonOptions options)
	{
		ValidateOptions(options);
		return SKPixelComparer.GenerateDifferenceImage(expected, actual, options.ChannelTolerance);
	}

	private static void ValidateOptions(GoldenComparisonOptions options)
	{
		if (options == null) throw new ArgumentNullException(nameof(options));
		if (options.ChannelTolerance < 0 || options.ChannelTolerance > byte.MaxValue)
			throw new ArgumentOutOfRangeException(nameof(options.ChannelTolerance));
		if (double.IsNaN(options.MaxErrorPixelFraction) ||
			options.MaxErrorPixelFraction < 0 || options.MaxErrorPixelFraction > 1)
			throw new ArgumentOutOfRangeException(nameof(options.MaxErrorPixelFraction));
		if (options.MaxAverageColorErrorFraction is { } average &&
			(double.IsNaN(average) || average < 0 || average > 1))
			throw new ArgumentOutOfRangeException(nameof(options.MaxAverageColorErrorFraction));
	}
}
