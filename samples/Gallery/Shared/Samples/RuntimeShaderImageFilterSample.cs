using System.Collections.Generic;
using System.Threading.Tasks;
using SkiaSharp;
using SkiaSharpSample.Controls;

namespace SkiaSharpSample.Samples;

public class RuntimeShaderImageFilterSample : CanvasSampleBase
{
	private const string InvertShader = """
		uniform shader child;
		half4 main(float2 p) {
			half4 c = child.eval(p);
			return half4(1.0 - c.rgb, c.a);
		}
		""";

	private const string GrayscaleShader = """
		uniform shader child;
		half4 main(float2 p) {
			half4 c = child.eval(p);
			float gray = dot(c.rgb, half3(0.2126, 0.7152, 0.0722));
			return half4(gray, gray, gray, c.a);
		}
		""";

	private const string SepiaShader = """
		uniform shader child;
		uniform float intensity;
		half4 main(float2 p) {
			half4 c = child.eval(p);
			float3 sepia = float3(
				dot(c.rgb, float3(0.393, 0.769, 0.189)),
				dot(c.rgb, float3(0.349, 0.686, 0.168)),
				dot(c.rgb, float3(0.272, 0.534, 0.131)));
			return half4(mix(c.rgb, sepia, intensity), c.a);
		}
		""";

	private const string VignetteShader = """
		uniform shader child;
		uniform float2 resolution;
		uniform float radius;
		uniform float softness;
		half4 main(float2 p) {
			half4 c = child.eval(p);
			float2 uv = p / resolution;
			float2 center = float2(0.5, 0.5);
			float dist = distance(uv, center);
			float vignette = smoothstep(radius, radius - softness, dist);
			return half4(c.rgb * vignette, c.a);
		}
		""";

	private const string EdgeDetectShader = """
		uniform shader child;
		uniform float strength;
		half4 main(float2 p) {
			half4 c  = child.eval(p);
			half4 cx = child.eval(p + float2(1, 0)) - child.eval(p + float2(-1, 0));
			half4 cy = child.eval(p + float2(0, 1)) - child.eval(p + float2(0, -1));
			float edge = length(cx.rgb) + length(cy.rgb);
			return half4(half3(edge * strength), c.a);
		}
		""";

	private static readonly string[] ShaderNames = { "Invert", "Grayscale", "Sepia", "Vignette", "Edge Detect" };
	private static readonly string[] ShaderSources = { InvertShader, GrayscaleShader, SepiaShader, VignetteShader, EdgeDetectShader };

	private SKBitmap? sourceBitmap;
	private SKRuntimeImageFilterBuilder?[] cachedBuilders = new SKRuntimeImageFilterBuilder?[5];
	private int selectedShader;
	private float intensity = 0.8f;
	private float radius = 0.7f;
	private float softness = 0.4f;
	private float strength = 2.0f;

	public override string Title => "Custom SkSL Image Filter";

	public override DateOnly? DateAdded => new DateOnly(2026, 5, 5);

	public override string Description => "Apply custom GPU image processing effects written in SkSL as image filters, composable with built-in filters.";

	public override string Category => SampleManager.ImageFilters;

	public override IReadOnlyList<SampleControl> Controls =>
	[
		new PickerControl("shader", "Effect", ShaderNames, selectedShader),
		new SliderControl("intensity", "Sepia Intensity", 0, 1, intensity, 0.01f, "Sepia effect intensity"),
		new SliderControl("radius", "Vignette Radius", 0, 1, radius, 0.01f, "Vignette inner radius"),
		new SliderControl("softness", "Vignette Softness", 0, 1, softness, 0.01f, "Vignette edge softness"),
		new SliderControl("strength", "Edge Strength", 0, 5, strength, 0.1f, "Edge detection strength"),
	];

	protected override Task OnInit()
	{
		using var stream = new SKManagedStream(SampleMedia.Images.Baboon);
		sourceBitmap = SKBitmap.Decode(stream);
		for (var i = 0; i < ShaderSources.Length; i++)
			cachedBuilders[i] = SKRuntimeEffect.BuildImageFilter(ShaderSources[i]);
		return Task.CompletedTask;
	}

	protected override void OnDestroy()
	{
		sourceBitmap?.Dispose();
		sourceBitmap = null;
		for (var i = 0; i < cachedBuilders.Length; i++)
		{
			cachedBuilders[i]?.Dispose();
			cachedBuilders[i] = null;
		}
	}

	protected override void OnControlChanged(string id, object value)
	{
		switch (id)
		{
			case "shader":
				selectedShader = (int)value;
				break;
			case "intensity":
				intensity = (float)value;
				break;
			case "radius":
				radius = (float)value;
				break;
			case "softness":
				softness = (float)value;
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

		var builder = cachedBuilders[selectedShader];
		if (builder == null)
			return;

		switch (selectedShader)
		{
			case 2: // Sepia
				builder.Uniforms["intensity"] = intensity;
				break;
			case 3: // Vignette
				builder.Uniforms["resolution"] = new float[] { width, height };
				builder.Uniforms["radius"] = radius;
				builder.Uniforms["softness"] = softness;
				break;
			case 4: // Edge Detect
				builder.Uniforms["strength"] = strength;
				break;
		}

		var sampleRadius = selectedShader == 4 ? 1.0f : 0f;

		using var filter = sampleRadius > 0
			? builder.Build(sampleRadius)
			: builder.Build();

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
