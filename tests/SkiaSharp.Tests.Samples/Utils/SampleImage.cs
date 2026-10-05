using Xunit;

namespace SkiaSharp.Tests.Samples.Utils;

internal static class SampleImage
{
    public static void Validate(ReadOnlySpan<byte> png)
    {
        using var bitmap = SKBitmap.Decode(png);
        Assert.NotNull(bitmap);
        Assert.Equal(800, bitmap.Width);
        Assert.Equal(600, bitmap.Height);
    }
}
