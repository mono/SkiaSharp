using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using SkiaSharp;
using SkiaSharp.Views.Desktop;

namespace SkiaSharpSample;

public partial class GpuPage : UserControl
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
	readonly Lazy<SKRuntimeShaderBuilder> shaderBuilder = new(() => SKRuntimeEffect.BuildShader(SkslSource));

	DispatcherTimer? renderTimer;
	SKPoint touchPos;
	bool touchActive;

	public GpuPage()
	{
		InitializeComponent();
	}

	private void OnLoaded(object sender, RoutedEventArgs e)
	{
		GlView.MouseDown += OnMouseDown;
		GlView.MouseMove += OnMouseMove;
		GlView.MouseUp += OnMouseUp;
		StartAnimation();
	}

	private void OnUnloaded(object sender, RoutedEventArgs e)
	{
		StopAnimation();
		GlView.MouseDown -= OnMouseDown;
		GlView.MouseMove -= OnMouseMove;
		GlView.MouseUp -= OnMouseUp;
	}

	void StartAnimation()
	{
		renderTimer?.Stop();
		fpsCounter.Start();
		FpsText.Text = "FPS: --";
		renderTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
		renderTimer.Tick += (_, _) => GlView.InvalidateVisual();
		renderTimer.Start();
	}

	void StopAnimation()
	{
		renderTimer?.Stop();
		renderTimer = null;
		fpsCounter.Stop();
	}

	void OnPaintSurface(object sender, SKPaintGLSurfaceEventArgs e)
	{
		var canvas = e.Surface.Canvas;
		var width = e.BackendRenderTarget.Width;
		var height = e.BackendRenderTarget.Height;

		var builder = shaderBuilder.Value;

		builder.Uniforms["iTime"] = fpsCounter.ElapsedSeconds;
		builder.Uniforms["iResolution"] = new float[] { width, height };
		builder.Uniforms["iTouchPos"] = new float[] { touchPos.X, touchPos.Y };
		builder.Uniforms["iTouchActive"] = touchActive ? 1f : 0f;
		builder.Uniforms["iColors"] = blobColors;

		using var shader = builder.Build();
		shaderPaint.Shader = shader;
		canvas.DrawRect(0, 0, width, height, shaderPaint);

		if (fpsCounter.Tick() is double fps)
			Dispatcher.BeginInvoke(() => FpsText.Text = $"FPS: {fps:F0}");
	}

	void OnMouseDown(object sender, MouseButtonEventArgs e)
	{
		if (e.LeftButton == MouseButtonState.Pressed)
		{
			touchActive = true;
			UpdateTouchPosition(e.GetPosition(GlView));
			GlView.CaptureMouse();
		}
	}

	void OnMouseMove(object sender, MouseEventArgs e)
	{
		if (e.LeftButton == MouseButtonState.Pressed)
			UpdateTouchPosition(e.GetPosition(GlView));
	}

	void OnMouseUp(object sender, MouseButtonEventArgs e)
	{
		touchActive = false;
		GlView.ReleaseMouseCapture();
	}

	void UpdateTouchPosition(Point pos)
	{
		var w = GlView.ActualWidth;
		var h = GlView.ActualHeight;
		if (w > 0 && h > 0)
			touchPos = new SKPoint((float)(pos.X / w), (float)(pos.Y / h));
	}
}
