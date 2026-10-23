using System;
using System.Linq;

namespace SkiaSharp.Tests.Shared;

public static class GoldenDiagnostics
{
	public const string ImageMarker = "##SKIA-GOLDEN##";

	public static string CaptureKey(GoldenRequest request)
	{
		if (request == null) throw new ArgumentNullException(nameof(request));
		return request.Platform + "/" + request.FileNames().First();
	}

	public static string Marker(string role, GoldenRequest request, int width, int height, byte[] bytes)
	{
		if (role != "actual" && role != "reference" && role != "diff") throw new ArgumentException("Unknown image role.", nameof(role));
		if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
		if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));
		return ImageMarker + " role=" + role + " key=" + CaptureKey(request) +
			" size=" + width + "x" + height + " base64=" + Convert.ToBase64String(bytes);
	}
}
