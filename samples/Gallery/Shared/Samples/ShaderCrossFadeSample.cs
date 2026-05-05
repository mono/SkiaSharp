using System;
using System.Threading;
using System.Threading.Tasks;
using SkiaSharp;
using SkiaSharpSample.Controls;

namespace SkiaSharpSample.Samples;

public class ShaderCrossFadeSample : CanvasSampleBase
{
	public override bool IsAnimated => true;

	private float threshold = 0.5f;
	private float noiseScale = 8f;
	private float edgeSoftness = 0.08f;
	private bool animating = true;
	private float speed = 0.4f;
	private float time;

	private SKRuntimeEffect? effect;
	private string? compileError;

	// SkSL noise-threshold cross-fade shader:
	// Two child image shaders + a threshold uniform. A procedural noise value
	// per-pixel is compared against the threshold to blend between children.
	private const string CrossFadeSkSL = @"
uniform shader imageA;
uniform shader imageB;
uniform float threshold;
uniform float noiseScale;
uniform float softness;
uniform float2 iResolution;

// Simple 2D hash for procedural noise
float hash(float2 p) {
    float3 p3 = fract(float3(p.xyx) * 0.1031);
    p3 += dot(p3, p3.yzx + 33.33);
    return fract((p3.x + p3.y) * p3.z);
}

// Value noise with smooth interpolation
float noise(float2 p) {
    float2 i = floor(p);
    float2 f = fract(p);
    f = f * f * (3.0 - 2.0 * f);
    float a = hash(i);
    float b = hash(i + float2(1.0, 0.0));
    float c = hash(i + float2(0.0, 1.0));
    float d = hash(i + float2(1.0, 1.0));
    return mix(mix(a, b, f.x), mix(c, d, f.x), f.y);
}

// Fractal Brownian Motion for richer noise
float fbm(float2 p) {
    float v = 0.0;
    float a = 0.5;
    float2 shift = float2(100.0, 100.0);
    for (int i = 0; i < 5; i++) {
        v += a * noise(p);
        p = p * 2.0 + shift;
        a *= 0.5;
    }
    return v;
}

half4 main(float2 fragCoord) {
    float2 uv = fragCoord / iResolution;
    float n = fbm(uv * noiseScale);
    float edge = softness;
    float blend = smoothstep(threshold - edge, threshold + edge, n);
    half4 colA = imageA.eval(fragCoord);
    half4 colB = imageB.eval(fragCoord);
    return mix(colA, colB, blend);
}
";

	public override string Title => "Shader Cross-Fade";

	public override DateOnly? DateAdded => new DateOnly(2026, 4, 27);

	public override string Description =>
		"Animated cross-fade between two procedural images using an SkSL noise threshold mask with child shaders.";

	public override string Category => SampleManager.Shaders;

	public override IReadOnlyList<SampleControl> Controls =>
	[
		new ToggleControl("animate", "Animate", animating, Description: "Auto-cycle the threshold."),
		new SliderControl("threshold", "Threshold", 0f, 1f, threshold, 0.01f, Description: "Blend cut-off value."),
		new SliderControl("noiseScale", "Noise Scale", 1f, 20f, noiseScale, 0.5f, Description: "Scale of the noise pattern."),
		new SliderControl("softness", "Edge Softness", 0f, 0.3f, edgeSoftness, 0.01f, Description: "Smoothness of the transition edge."),
		new SliderControl("speed", "Animation Speed", 0.1f, 2f, speed, 0.1f),
	];

	protected override void OnControlChanged(string id, object value)
	{
		switch (id)
		{
			case "animate": animating = (bool)value; break;
			case "threshold": threshold = (float)value; break;
			case "noiseScale": noiseScale = (float)value; break;
			case "softness": edgeSoftness = (float)value; break;
			case "speed": speed = (float)value; break;
		}
	}

	protected override async Task OnInit()
	{
		CompileShader();
		await base.OnInit();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		effect?.Dispose();
		effect = null;
	}

	protected override async Task OnUpdate(CancellationToken token)
	{
		await Task.Delay(16, token);
		if (animating)
		{
			time += 0.016f * speed;
			// Ping-pong threshold between 0 and 1
			threshold = (MathF.Sin(time) + 1f) / 2f;
		}
	}

	protected override void OnDrawSample(SKCanvas canvas, int width, int height)
	{
		canvas.Clear(SKColors.Black);

		if (effect == null)
		{
			DrawError(canvas, width);
			return;
		}

		// Create two procedural "image" shaders as children
		using var shaderA = CreateGradientShader(width, height, isA: true);
		using var shaderB = CreateGradientShader(width, height, isA: false);

		using var uniforms = new SKRuntimeEffectUniforms(effect);
		uniforms["threshold"] = threshold;
		uniforms["noiseScale"] = noiseScale;
		uniforms["softness"] = edgeSoftness;
		uniforms["iResolution"] = new float[] { width, height };

		using var children = new SKRuntimeEffectChildren(effect);
		children["imageA"] = shaderA;
		children["imageB"] = shaderB;

		using var shader = effect.ToShader(uniforms, children);
		using var paint = new SKPaint { Shader = shader };
		canvas.DrawRect(0, 0, width, height, paint);

		// Draw info overlay
		DrawOverlay(canvas, width, height);
	}

	private SKShader CreateGradientShader(int width, int height, bool isA)
	{
		if (isA)
		{
			// Warm sunset gradient
			return SKShader.CreateLinearGradient(
				new SKPoint(0, 0),
				new SKPoint(width, height),
				new SKColor[]
				{
					new(0xFFFF6B35), // orange
					new(0xFFF7C948), // gold
					new(0xFFE83F6F), // pink
					new(0xFF2274A5), // deep blue
				},
				new float[] { 0f, 0.33f, 0.66f, 1f },
				SKShaderTileMode.Clamp);
		}
		else
		{
			// Cool aurora gradient
			return SKShader.CreateLinearGradient(
				new SKPoint(width, 0),
				new SKPoint(0, height),
				new SKColor[]
				{
					new(0xFF32E0C4), // teal
					new(0xFF0D7377), // dark teal
					new(0xFF5C2A9D), // purple
					new(0xFF1B1464), // navy
				},
				new float[] { 0f, 0.33f, 0.66f, 1f },
				SKShaderTileMode.Clamp);
		}
	}

	private void DrawOverlay(SKCanvas canvas, int width, int height)
	{
		using var bgPaint = new SKPaint { Color = new SKColor(0, 0, 0, 140) };
		canvas.DrawRect(0, 0, width, 54, bgPaint);

		using var font = new SKFont { Size = 15 };
		using var paint = new SKPaint { Color = SKColors.White, IsAntialias = true };
		canvas.DrawText("Runtime Shader Threshold Cross-Fade", 12, 20, font, paint);

		using var smallFont = new SKFont { Size = 12 };
		paint.Color = new SKColor(200, 200, 200);
		canvas.DrawText(
			$"threshold: {threshold:F2}  noise: {noiseScale:F1}  softness: {edgeSoftness:F2}",
			12, 42, smallFont, paint);
	}

	private void DrawError(SKCanvas canvas, int width)
	{
		using var paint = new SKPaint { Color = SKColors.Red, IsAntialias = true };
		using var font = new SKFont { Size = 16 };
		canvas.DrawText("Shader compilation failed:", 20, 40, SKTextAlign.Left, font, paint);
		if (compileError != null)
		{
			paint.Color = SKColors.White;
			using var detailFont = new SKFont { Size = 14 };
			canvas.DrawText(compileError, 20, 70, SKTextAlign.Left, detailFont, paint);
		}
	}

	private void CompileShader()
	{
		effect?.Dispose();
		effect = null;
		compileError = null;

		effect = SKRuntimeEffect.CreateShader(CrossFadeSkSL, out var errors);
		if (effect == null)
			compileError = errors ?? "Unknown compilation error";
	}
}
