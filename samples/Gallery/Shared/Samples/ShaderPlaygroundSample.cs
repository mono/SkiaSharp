using System;
using System.Threading;
using System.Threading.Tasks;
using SkiaSharp;
using SkiaSharpSample.Controls;

namespace SkiaSharpSample.Samples;

public class ShaderPlaygroundSample : CanvasSampleBase
{
	public override bool IsAnimated => true;

	private int shaderIndex;
	private float speed = 1f;
	private float param1 = 10f;
	private float param2 = 5f;
	private float time;

	private SKRuntimeEffect? effect;
	private string? compileError;
	private readonly float[] resolutionBuffer = new float[2];

	private static readonly string[] ShaderNames = { "Ripple", "Plasma", "Spiral", "Warp Grid", "Color Wheel" };

	private static readonly string[] ShaderSources =
	{
		// Ripple
		@"
uniform float iTime;
uniform float2 iResolution;
uniform float param1;
uniform float param2;

half4 main(float2 fragCoord) {
    float2 uv = fragCoord / iResolution;
    float2 center = float2(0.5, 0.5);
    float dist = distance(uv, center);
    float wave = sin(dist * param1 * 6.2832 - iTime * param2) * 0.5 + 0.5;
    float3 col = mix(float3(0.1, 0.2, 0.5), float3(0.9, 0.4, 0.1), wave);
    return half4(half3(col), 1.0);
}
",
		// Plasma
		@"
uniform float iTime;
uniform float2 iResolution;
uniform float param1;
uniform float param2;

half4 main(float2 fragCoord) {
    float2 uv = fragCoord / iResolution * param1;
    float v1 = sin(uv.x * 5.0 + iTime);
    float v2 = sin(uv.y * 5.0 + iTime);
    float v3 = sin(uv.x * 5.0 + uv.y * 5.0 + iTime);
    float v4 = sin(length(uv - float2(param1 * 0.5, param2 * 0.5)) * param2);
    float v = (v1 + v2 + v3 + v4) * 0.25;
    float3 col = float3(
        sin(v * 3.14159) * 0.5 + 0.5,
        sin(v * 3.14159 + 2.094) * 0.5 + 0.5,
        sin(v * 3.14159 + 4.189) * 0.5 + 0.5
    );
    return half4(half3(col), 1.0);
}
",
		// Spiral
		@"
uniform float iTime;
uniform float2 iResolution;
uniform float param1;
uniform float param2;

half4 main(float2 fragCoord) {
    float2 uv = (fragCoord - iResolution * 0.5) / min(iResolution.x, iResolution.y);
    float angle = atan(uv.y, uv.x);
    float radius = length(uv);
    float spiral = sin(angle * param1 + radius * param2 * 10.0 - iTime * 3.0);
    float3 col = float3(
        spiral * 0.5 + 0.5,
        sin(spiral * 3.14159 + 1.0) * 0.5 + 0.5,
        cos(spiral * 3.14159) * 0.5 + 0.5
    );
    col *= smoothstep(0.5, 0.0, radius);
    return half4(half3(col), 1.0);
}
",
		// Warp Grid
		@"
uniform float iTime;
uniform float2 iResolution;
uniform float param1;
uniform float param2;

half4 main(float2 fragCoord) {
    float2 uv = fragCoord / iResolution;
    uv.x += sin(uv.y * param1 + iTime) * 0.05 * param2;
    uv.y += cos(uv.x * param1 + iTime) * 0.05 * param2;
    float gridX = step(0.95, fract(uv.x * param1));
    float gridY = step(0.95, fract(uv.y * param1));
    float grid = max(gridX, gridY);
    float3 bg = float3(0.05, 0.05, 0.15);
    float3 line = float3(0.2, 0.8, 0.6);
    float3 col = mix(bg, line, grid);
    return half4(half3(col), 1.0);
}
",
		// Color Wheel
		@"
uniform float iTime;
uniform float2 iResolution;
uniform float param1;
uniform float param2;

half4 main(float2 fragCoord) {
    float2 uv = (fragCoord - iResolution * 0.5) / min(iResolution.x, iResolution.y);
    float angle = atan(uv.y, uv.x) + iTime * 0.5;
    float radius = length(uv);
    float hue = angle / 6.2832 + 0.5;
    float sat = smoothstep(0.0, 0.4 * param1 / 10.0, radius);
    float val = smoothstep(0.5 * param2 / 10.0, 0.0, radius - 0.4);
    // HSV to RGB
    float3 c = float3(hue, sat, val);
    float4 K = float4(1.0, 2.0/3.0, 1.0/3.0, 3.0);
    float3 p = abs(fract(c.xxx + K.xyz) * 6.0 - K.www);
    float3 col = c.z * mix(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
    return half4(half3(col), 1.0);
}
",
	};

	public override string Title => "Shader Playground";

	public override string Description => "Write and visualize animated SkSL runtime effect shaders with adjustable parameters.";

	public override string Category => SampleCategories.Shaders;

	public override IReadOnlyList<SampleControl> Controls =>
	[
		new PickerControl("shader", "Shader Preset", ShaderNames, shaderIndex),
		new SliderControl("param1", "Parameter 1", 0, 20, param1, Description: "Primary shader parameter — controls frequency or scale."),
		new SliderControl("param2", "Parameter 2", 0, 20, param2, Description: "Secondary shader parameter — controls intensity or speed."),
		new SliderControl("speed", "Animation Speed", 0, 3, speed, 0.1f),
	];

	protected override void OnControlChanged(string id, object value)
	{
		switch (id)
		{
			case "shader":
				shaderIndex = (int)value;
				CompileShader();
				break;
			case "speed":
				speed = (float)value;
				break;
			case "param1":
				param1 = (float)value;
				break;
			case "param2":
				param2 = (float)value;
				break;
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
		time += 0.016f * speed;
	}

	protected override void OnDrawSample(SKCanvas canvas, int width, int height)
	{
		canvas.Clear(SKColors.Black);

		if (effect == null)
		{
			using var errorPaint = new SKPaint
			{
				Color = SKColors.Red,
				IsAntialias = true,
			};
			using var errorFont = new SKFont { Size = 16 };
			canvas.DrawText("Shader compilation failed:", 20, 40, SKTextAlign.Left, errorFont, errorPaint);
			if (compileError != null)
			{
				errorPaint.Color = SKColors.White;
				using var detailFont = new SKFont { Size = 14 };
				canvas.DrawText(compileError, 20, 70, SKTextAlign.Left, detailFont, errorPaint);
			}
			return;
		}

		using var uniforms = new SKRuntimeEffectUniforms(effect);
		uniforms["iTime"] = time;
		resolutionBuffer[0] = width;
		resolutionBuffer[1] = height;
		uniforms["iResolution"] = resolutionBuffer;
		uniforms["param1"] = param1;
		uniforms["param2"] = param2;

		using var shader = effect.ToShader(uniforms);
		using var paint = new SKPaint
		{
			Shader = shader,
		};
		canvas.DrawRect(0, 0, width, height, paint);
	}

	private void CompileShader()
	{
		effect?.Dispose();
		effect = null;
		compileError = null;

		var source = ShaderSources[shaderIndex];
		effect = SKRuntimeEffect.CreateShader(source, out var errors);

		if (effect == null)
			compileError = errors ?? "Unknown compilation error";
	}
}
