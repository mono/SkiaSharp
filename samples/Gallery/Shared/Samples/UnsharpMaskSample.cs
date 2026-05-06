using System.Collections.Generic;
using System.Threading.Tasks;
using SkiaSharp;
using SkiaSharpSample.Controls;

namespace SkiaSharpSample.Samples;

public class UnsharpMaskSample : CanvasSampleBase
{
	private const string UnsharpShader = """
		uniform shader content;
		uniform shader blurred;
		uniform float strength;
		vec4 main(vec2 coord) {
			vec4 c = content.eval(coord);
			vec4 b = blurred.eval(coord);
			return c + (c - b) * strength;
		}
		""";

	private SKBitmap? sourceBitmap;
	private SKRuntimeImageFilterBuilder? builder;
	private float blurRadius = 1.0f;
	private float strength = 4.0f;

	public override string Title => "Unsharp Mask";

	public override DateOnly? DateAdded => new DateOnly(2026, 5, 5);

	public override string Description => "Sharpens an image using a custom SkSL image filter with two child inputs: the original content and a blurred version.";

	public override string Category => SampleManager.ImageFilters;

	public override IReadOnlyList<SampleControl> Controls =>
	[
		new SliderControl("blur", "Blur Radius", 0.5f, 10, blurRadius, 0.5f, "Radius of the blur used for the mask"),
		new SliderControl("strength", "Sharpen Strength", 0, 10, strength, 0.5f, "How much to amplify the difference"),
	];

	protected override Task OnInit()
	{
		using var stream = new SKManagedStream(SampleMedia.Images.Baboon);
		sourceBitmap = SKBitmap.Decode(stream);
		builder = SKRuntimeEffect.BuildImageFilter(UnsharpShader);
		return Task.CompletedTask;
	}

	protected override void OnDestroy()
	{
		sourceBitmap?.Dispose();
		sourceBitmap = null;
		builder?.Dispose();
		builder = null;
	}

	protected override void OnControlChanged(string id, object value)
	{
		switch (id)
		{
			case "blur":
				blurRadius = (float)value;
				break;
			case "strength":
				strength = (float)value;
				break;
		}
	}

	protected override void OnDrawSample(SKCanvas canvas, int width, int height)
	{
		if (sourceBitmap == null)
			return;

		canvas.Clear(SKColors.Black);

		if (builder == null)
			return;

		builder.Uniforms["strength"] = strength;

		using var blur = SKImageFilter.CreateBlur(blurRadius, blurRadius);
		builder.Inputs.Reset();
		builder.Inputs["content"] = null;
		builder.Inputs["blurred"] = blur;
		using var filter = builder.Build();

		if (filter == null)
			return;

		using var paint = new SKPaint();
		paint.ImageFilter = filter;

		var destRect = SKRect.Create(width, height);
		var srcRect = SKRect.Create(sourceBitmap.Width, sourceBitmap.Height);

		canvas.SaveLayer(paint);
		canvas.DrawBitmap(sourceBitmap, srcRect, destRect);
		canvas.Restore();
	}
}
