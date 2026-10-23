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

}
