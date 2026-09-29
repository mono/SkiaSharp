using System;
using SkiaSharp;
using SkiaSharpSample.Controls;
using SkiaSharpSample.ImagePlaceholders;

namespace SkiaSharpSample.Samples;

public sealed class ThumbHashSample : ImagePlaceholderSampleBase
{
	public override string Title => "ThumbHash Playground";
	public override string Description => "Compare a ThumbHash preview with its image, including transparency.";
	protected override bool SupportsAlpha => true;
	protected override string EncodeHash(SKBitmap source) => Convert.ToBase64String(ThumbHashCodec.Encode(source));
	protected override SKBitmap DecodeHash(string value, int width, int height) =>
		ThumbHashCodec.DecodeBitmap(Convert.FromBase64String(value), width, height);
}
