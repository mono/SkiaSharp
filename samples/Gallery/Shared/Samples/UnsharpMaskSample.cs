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
	private float blurRadius = 1.0f;
	private float strength = 4.0f;

	public override string Title => "Unsharp Mask";

	public override string Description => "Multi-child runtime shader image filter: sharpens an image by subtracting a blurred version from the original.";

	public override string Category => SampleCategories.ImageFilters;

	public override IReadOnlyList<SampleControl> Controls =>
	[
		new SliderControl("blur", "Blur Radius", 0.5f, 10, blurRadius, 0.5f, "Radius of the blur used for the mask"),
		new SliderControl("strength", "Sharpen Strength", 0, 10, strength, 0.5f, "How much to amplify the difference"),
	];

	protected override Task OnInit()
	{
		using var stream = new SKManagedStream(SampleMedia.Images.Baboon);
		sourceBitmap = SKBitmap.Decode(stream);
		return Task.CompletedTask;
	}

	protected override void OnDestroy()
	{
		sourceBitmap?.Dispose();
		sourceBitmap = null;
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

		using var builder = SKRuntimeEffect.BuildImageFilter(UnsharpShader);
		if (builder.Effect == null)
			return;

		builder.Uniforms["strength"] = strength;

		using var blur = SKImageFilter.CreateBlur(blurRadius, blurRadius);
		using var filter = builder.Build(
			new[] { "content", "blurred" },
			new SKImageFilter?[] { null, blur });

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
