using SkiaSharp;
using SkiaSharpSample.Controls;
using SkiaSharpSample.ImagePlaceholders;

namespace SkiaSharpSample.Samples;

public sealed class BlurHashSample : ImagePlaceholderSampleBase
{
	private int componentsX = 4;
	private int componentsY = 3;

	public override string Title => "BlurHash Playground";
	public override string Description => "Compare a BlurHash preview with its image and adjust the component count.";

	protected override string EncodeHash(SKBitmap source) => BlurHashCodec.Encode(source, componentsX, componentsY);
	protected override SKBitmap DecodeHash(string value, int width, int height) => BlurHashCodec.DecodeBitmap(value, width, height);
	protected override IReadOnlyList<SampleControl> CodecControls =>
	[
		new SliderControl("componentsX", "Horizontal components", 1, 9, componentsX, 1),
		new SliderControl("componentsY", "Vertical components", 1, 9, componentsY, 1),
	];

	protected override bool UpdateCodecControl(string id, object value)
	{
		switch (id)
		{
			case "componentsX": componentsX = (int)(float)value; return true;
			case "componentsY": componentsY = (int)(float)value; return true;
			default: return false;
		}
	}
}
