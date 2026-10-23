using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SkiaSharp.Tests.Shared;

/// <summary>Provides shared PNG decoding, encoding, comparison and difference generation.</summary>
public static class GoldenImage
{
	public const string MarkerPrefix = "##SKIA-GOLDEN##";
	private static readonly byte[] PngSignature = { 137, 80, 78, 71, 13, 10, 26, 10 };

	public static bool HasPngSignature(byte[] bytes)
	{
		if (bytes == null) throw new ArgumentNullException(nameof(bytes));
		if (bytes.Length < PngSignature.Length)
			return false;
		for (var i = 0; i < PngSignature.Length; i++)
			if (bytes[i] != PngSignature[i])
				return false;
		return true;
	}

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

	public static bool TryGetSize(byte[] png, out SKSizeI size)
	{
		using var image = TryDecode(png);
		if (image == null)
		{
			size = default;
			return false;
		}

		size = new SKSizeI(image.Width, image.Height);
		return true;
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

	public static void Write(string path, byte[] png)
	{
		if (path == null) throw new ArgumentNullException(nameof(path));
		if (png == null) throw new ArgumentNullException(nameof(png));
		Directory.CreateDirectory(Path.GetDirectoryName(path)!);
		File.WriteAllBytes(path, png);
	}

	public static string Marker(string role, GoldenRequest request, byte[] png)
	{
		if (role != "actual" && role != "reference" && role != "diff")
			throw new ArgumentException("Unknown image role.", nameof(role));
		if (request == null) throw new ArgumentNullException(nameof(request));
		if (png == null) throw new ArgumentNullException(nameof(png));

		using var image = Decode(png, role + " marker");
		return MarkerPrefix + " role=" + role + " key=" + request.CaptureKey +
			" size=" + image.Width + "x" + image.Height + " base64=" + Convert.ToBase64String(png);
	}

	public static string? TryMarker(string role, GoldenRequest request, byte[] png)
	{
		using var image = TryDecode(png);
		return image == null
			? null
			: MarkerPrefix + " role=" + role + " key=" + request.CaptureKey +
				" size=" + image.Width + "x" + image.Height + " base64=" + Convert.ToBase64String(png);
	}

	public static IEnumerable<string> Markers(
		GoldenResolution resolution,
		byte[]? actualPng = null,
		byte[]? differencePng = null)
	{
		if (resolution == null) throw new ArgumentNullException(nameof(resolution));

		if (actualPng != null && TryMarker("actual", resolution.Request, actualPng) is { } actual)
			yield return actual;
		if (resolution.Key != null && TryMarker("reference", resolution.Request, resolution.ReadBytes()) is { } reference)
			yield return reference;
		if (differencePng != null && TryMarker("diff", resolution.Request, differencePng) is { } difference)
			yield return difference;
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

	public static GoldenComparison Compare(
		byte[] expectedPng,
		byte[] actualPng,
		GoldenComparisonOptions options,
		string expectedLocation,
		string actualLocation)
	{
		using var expected = Decode(expectedPng, expectedLocation);
		using var actual = Decode(actualPng, actualLocation);
		return Compare(expected, actual, options);
	}

	public static SKImage Difference(SKImage expected, SKImage actual, GoldenComparisonOptions options)
	{
		ValidateOptions(options);
		return SKPixelComparer.GenerateDifferenceImage(expected, actual, options.ChannelTolerance);
	}

	public static byte[] Difference(
		byte[] expectedPng,
		byte[] actualPng,
		GoldenComparisonOptions options,
		string expectedLocation,
		string actualLocation)
	{
		using var expected = Decode(expectedPng, expectedLocation);
		using var actual = Decode(actualPng, actualLocation);
		using var difference = Difference(expected, actual, options);
		return Encode(difference);
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
