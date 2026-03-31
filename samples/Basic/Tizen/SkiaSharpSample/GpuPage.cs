using System;
using Tizen.NUI;
using Tizen.NUI.BaseComponents;

using SkiaSharp;
using SkiaSharp.Views.Tizen;
using SKCanvasView = SkiaSharp.Views.Tizen.NUI.SKCanvasView;

namespace SkiaSharpSample;

public class GpuPage : View
{
	const string SkslSource = @"
uniform float iTime;
uniform float2 iResolution;
uniform float2 iTouchPos;
uniform float iTouchActive;
uniform float3 iColors[6];

half4 main(float2 fragCoord) {
	float2 uv = fragCoord / iResolution;
	float aspect = iResolution.x / iResolution.y;
	float2 st = float2(uv.x * aspect, uv.y);
	float t = iTime;
	float field = 0.0;
	float3 weighted = float3(0.0);
	for (int i = 0; i < 6; i++) {
		float fi = float(i);
		float phase = fi * 1.047;
		float speed = 0.3 + fi * 0.07;
		float2 center = float2(
			aspect * 0.5 + 0.4 * sin(t * speed + phase) * cos(t * speed * 0.6 + fi),
			0.5 + 0.4 * cos(t * speed * 0.8 + phase * 1.3) * sin(t * speed * 0.4 + fi * 0.7)
		);
		float2 d = st - center;
		float r = length(d);
		float strength = 0.030 / (r * r + 0.002);
		field += strength;
		weighted += iColors[i] * strength;
	}
	if (iTouchActive > 0.5) {
		float2 touchSt = float2(iTouchPos.x * aspect, iTouchPos.y);
		float2 d = st - touchSt;
		float r = length(d);
		float strength = 0.050 / (r * r + 0.002);
		field += strength;
		weighted += float3(1.0, 0.95, 0.9) * strength;
	}
	float3 blobColor = weighted / max(field, 0.001);
	float edge = smoothstep(5.0, 8.0, field);
	float innerGlow = smoothstep(8.0, 20.0, field) * 0.3;
	float3 bg = float3(0.03, 0.02, 0.08);
	bg += float3(0.02, 0.01, 0.03) * sin(t * 0.2 + uv.y * 3.0);
	float halo = smoothstep(3.0, 5.0, field) * (1.0 - edge);
	float3 result = bg;
	result += blobColor * halo * 0.4;
	result = mix(result, blobColor * (1.0 + innerGlow), edge);
	float2 vc = uv - 0.5;
	float vignette = 1.0 - dot(vc, vc) * 0.8;
	result *= vignette;
	return half4(clamp(result, 0.0, 1.0), 1.0);
}
";

	static readonly float[] blobColors =
	{
		1.0f, 0.3f, 0.4f,   // rose
		0.3f, 0.7f, 1.0f,   // sky blue
		1.0f, 0.6f, 0.1f,   // amber
		0.4f, 1.0f, 0.7f,   // mint
		0.7f, 0.3f, 1.0f,   // violet
		1.0f, 0.9f, 0.2f,   // yellow
	};

	readonly FpsCounter fpsCounter = new();
	readonly SKPaint shaderPaint = new();
	readonly TextLabel fpsLabel;
	readonly SKCanvasView skiaView;

	SKRuntimeShaderBuilder? shaderBuilder;
	Timer? renderTimer;
	SKPoint touchPos;
	bool touchActive;

	public GpuPage()
	{
		WidthSpecification = LayoutParamPolicies.MatchParent;
		HeightSpecification = LayoutParamPolicies.MatchParent;

		skiaView = new SKCanvasView
		{
			WidthSpecification = LayoutParamPolicies.MatchParent,
			HeightSpecification = LayoutParamPolicies.MatchParent,
		};
		skiaView.PaintSurface += OnPaintSurface;
		skiaView.TouchEvent += OnTouch;

		fpsLabel = new TextLabel
		{
			Text = "FPS: --",
			TextColor = Tizen.NUI.Color.White,
			PointSize = 10,
			Position = new Position(10, 10),
		};

		Add(skiaView);
		Add(fpsLabel);

		// Build the SkSL shader (may fail on some devices)
		try
		{
			shaderBuilder = SKRuntimeEffect.BuildShader(SkslSource);
		}
		catch
		{
			// Shader compilation not supported — will show fallback
		}

		StartAnimation();
	}

	public void StartAnimation()
	{
		renderTimer?.Stop();
		fpsCounter.Start();
		fpsLabel.Text = "FPS: --";
		renderTimer = new Timer(16); // ~60 fps
		renderTimer.Tick += (s, e) =>
		{
			skiaView.Invalidate();
			return true;
		};
		renderTimer.Start();
	}

	public void StopAnimation()
	{
		renderTimer?.Stop();
		renderTimer = null;
		fpsCounter.Stop();
	}

	void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
	{
		var canvas = e.Surface.Canvas;
		var width = e.Info.Width;
		var height = e.Info.Height;

		if (shaderBuilder == null)
		{
			// Fallback when SkSL is not available
			canvas.Clear(new SKColor(0x08, 0x05, 0x14));
			using var paint = new SKPaint { Color = SKColors.White, IsAntialias = true };
			using var font = new SKFont { Size = width * 0.06f };
			canvas.DrawText("SkSL shader not supported",
				width / 2f, height / 2f, SKTextAlign.Center, font, paint);
			return;
		}

		shaderBuilder.Uniforms["iTime"] = fpsCounter.ElapsedSeconds;
		shaderBuilder.Uniforms["iResolution"] = new float[] { width, height };
		shaderBuilder.Uniforms["iTouchPos"] = new float[] { touchPos.X, touchPos.Y };
		shaderBuilder.Uniforms["iTouchActive"] = touchActive ? 1f : 0f;
		shaderBuilder.Uniforms["iColors"] = blobColors;

		using var shader = shaderBuilder.Build();
		shaderPaint.Shader = shader;
		canvas.DrawRect(0, 0, width, height, shaderPaint);

		if (fpsCounter.Tick() is double fps)
			fpsLabel.Text = $"FPS: {fps:F0}";
	}

	bool OnTouch(object sender, View.TouchEventArgs e)
	{
		var touch = e.Touch;
		if (touch.GetPointCount() < 1)
			return true;

		var state = touch.GetState(0);
		var pos = touch.GetLocalPosition(0);
		var w = skiaView.Size.Width;
		var h = skiaView.Size.Height;

		switch (state)
		{
			case PointStateType.Down:
			case PointStateType.Motion:
				touchActive = true;
				if (w > 0 && h > 0)
					touchPos = new SKPoint(pos.X / w, pos.Y / h);
				break;
			case PointStateType.Up:
			case PointStateType.Leave:
				touchActive = false;
				break;
		}

		return true;
	}
}
