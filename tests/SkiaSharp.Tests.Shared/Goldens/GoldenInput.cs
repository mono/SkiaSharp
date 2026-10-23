using System.Runtime.InteropServices;

namespace SkiaSharp.Tests.Shared;

/// <summary>Describes how a golden verification obtains one image.</summary>
public sealed class GoldenInput
{
	private readonly string? path;
	private readonly byte[]? bytes;
	private readonly SKImageInfo imageInfo;
	private readonly Func<Task<string>>? capturePath;
	private readonly string name;
	private readonly bool pixels;

	private GoldenInput(string name, string? path = null, byte[]? bytes = null,
		SKImageInfo imageInfo = default, Func<Task<string>>? capturePath = null, bool pixels = false)
	{
		this.name = name;
		this.path = path;
		this.bytes = bytes;
		this.imageInfo = imageInfo;
		this.capturePath = capturePath;
		this.pixels = pixels;
	}

	public static GoldenInput Path(string path)
	{
		if (path == null) throw new ArgumentNullException(nameof(path));
		return new(path, path: path);
	}

	public static GoldenInput Bytes(byte[] bytes, string name = "image.png")
	{
		if (bytes == null) throw new ArgumentNullException(nameof(bytes));
		ValidateName(name);
		return new(name, bytes: bytes);
	}

	public static GoldenInput Bytes(ReadOnlySpan<byte> bytes, string name = "image.png") =>
		Bytes(bytes.ToArray(), name);

	public static GoldenInput Pixels(byte[] pixels, SKImageInfo imageInfo, string name = "image.png")
	{
		if (pixels == null) throw new ArgumentNullException(nameof(pixels));
		ValidateName(name);
		if (imageInfo.Width <= 0 || imageInfo.Height <= 0 || imageInfo.BytesSize64 <= 0)
			throw new ArgumentOutOfRangeException(nameof(imageInfo));
		if (pixels.LongLength < imageInfo.BytesSize64)
			throw new ArgumentException(
				$"Expected at least {imageInfo.BytesSize64} bytes for {imageInfo.Width}x{imageInfo.Height} pixels.",
				nameof(pixels));
		return new(name, bytes: pixels, imageInfo: imageInfo, pixels: true);
	}

	public static GoldenInput Pixels(ReadOnlySpan<byte> pixels, SKImageInfo imageInfo, string name = "image.png") =>
		Pixels(pixels.ToArray(), imageInfo, name);

	public static GoldenInput Capture(Func<Task<string>> capturePath)
	{
		if (capturePath == null) throw new ArgumentNullException(nameof(capturePath));
		return new("capture.png", capturePath: capturePath);
	}

	internal async Task<Image> ReadAsync() =>
		capturePath == null ? Read() : Path(await capturePath()).Read();

	internal Image Read()
	{
		if (capturePath != null)
			throw new InvalidOperationException("Capture inputs require asynchronous verification.");
		if (path != null)
			return ReadEncoded(File.ReadAllBytes(path), path, path);
		if (pixels)
			return new(EncodePixels(bytes!, imageInfo), name, null, PngName(name));
		return ReadEncoded(bytes!, name, null);
	}

	internal static bool HasPngSignature(byte[] value)
	{
		ReadOnlySpan<byte> signature = stackalloc byte[] { 137, 80, 78, 71, 13, 10, 26, 10 };
		return value.AsSpan().StartsWith(signature);
	}

	private static Image ReadEncoded(byte[] encoded, string label, string? filePath)
	{
		if (HasPngSignature(encoded))
			return new(encoded, label, filePath, System.IO.Path.GetFileName(label));
		using var image = SKImage.FromEncodedData(encoded)
			?? throw new InvalidOperationException($"Failed to decode image: {label}.");
		return new(Encode(image), label, null, PngName(label));
	}

	private static byte[] EncodePixels(byte[] value, SKImageInfo info)
	{
		var handle = GCHandle.Alloc(value, GCHandleType.Pinned);
		try
		{
			using var pixmap = new SKPixmap(info, handle.AddrOfPinnedObject());
			using var image = SKImage.FromPixels(pixmap)
				?? throw new InvalidOperationException("Failed to create an image from raw pixels.");
			return Encode(image);
		}
		finally
		{
			handle.Free();
		}
	}

	private static byte[] Encode(SKImage image)
	{
		using var data = image.Encode(SKEncodedImageFormat.Png, 100);
		return data.ToArray();
	}

	private static string PngName(string value) =>
		System.IO.Path.ChangeExtension(System.IO.Path.GetFileName(value), ".png");

	private static void ValidateName(string value)
	{
		if (value == null) throw new ArgumentNullException(nameof(value));
		if (value.Length == 0) throw new ArgumentException("Expected an image name.", nameof(value));
	}

	internal sealed record Image(
		byte[] Png,
		string Label,
		string? FilePath,
		string AttachmentName);
}
