using SkiaSharp;
using SkiaSharpSample.Controls;
using SkiaSharpSample.ImagePlaceholders;

namespace SkiaSharpSample.Samples;

public sealed class BlurHashSample : ImagePlaceholderSampleBase
{
	private const int InitialComponentsX = 4;
	private const int InitialComponentsY = 3;
	private const float InitialPunch = 1f;
	private int componentsX = InitialComponentsX;
	private int componentsY = InitialComponentsY;
	private float punch = InitialPunch;

	public override string Title => "BlurHash Playground";
	public override string Description => "Compare a BlurHash preview with its image and adjust the component count.";

	protected override string EncodeHash(SKBitmap source) => BlurHashCodec.Encode(source, componentsX, componentsY);
	protected override SKBitmap DecodeHash(string value, int width, int height) => BlurHashCodec.DecodeBitmap(value, width, height, punch);
	protected override IReadOnlyList<SampleControl> CodecControls =>
	[
		new SliderControl("componentsX", "Horizontal components", 1, 9, componentsX, 1),
		new SliderControl("componentsY", "Vertical components", 1, 9, componentsY, 1),
		new SliderControl("punch", "Preview punch", 0, 2, punch, 0.1f),
	];

	protected override bool UpdatePreviewControl(string id, object value)
	{
		if (id != "punch")
			return false;
		punch = (float)value;
		return true;
	}

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
