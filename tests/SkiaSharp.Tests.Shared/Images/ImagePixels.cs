namespace SkiaSharp.Tests.Shared;

public static class ImagePixels
{
	public static int Count(string path, SKRectI region, Func<SKColor, bool> matches)
	{
		if (path == null) throw new ArgumentNullException(nameof(path));
		if (matches == null) throw new ArgumentNullException(nameof(matches));

		using var bitmap = SKBitmap.Decode(path)
			?? throw new InvalidOperationException("Failed to decode image '" + path + "'.");
		if (region.Left < 0 || region.Left >= bitmap.Width) throw new ArgumentOutOfRangeException(nameof(region));
		if (region.Top < 0 || region.Top >= bitmap.Height) throw new ArgumentOutOfRangeException(nameof(region));
		if (region.Right <= region.Left || region.Right > bitmap.Width) throw new ArgumentOutOfRangeException(nameof(region));
		if (region.Bottom <= region.Top || region.Bottom > bitmap.Height) throw new ArgumentOutOfRangeException(nameof(region));

		var count = 0;
		for (var y = region.Top; y < region.Bottom; y++)
		for (var x = region.Left; x < region.Right; x++)
			if (matches(bitmap.GetPixel(x, y)))
				count++;
		return count;
	}
}
