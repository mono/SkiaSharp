using AppKit;
using CoreGraphics;
using Foundation;
using ObjCRuntime;
using SkiaSharp;
using SkiaSharp.Views.Mac;

namespace SkiaSharpSample;

[Register("GpuMetalViewController")]
public class GpuMetalViewController : NSViewController
{
	const string ShaderSource = @"
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

	[Outlet("skiaView")]
	SKMetalView skiaView { get; set; } = null!;

	[Outlet("fpsLabel")]
	NSTextField fpsLabel { get; set; } = null!;

	readonly Lazy<SKRuntimeShaderBuilder> shaderBuilder = new(() => SKRuntimeEffect.BuildShader(ShaderSource));
	SKPoint touchPos = new(-1f, -1f);
	bool touchActive;
	double lastFps;

	public GpuMetalViewController(NativeHandle handle) : base(handle) { }

	public override void ViewDidLoad()
	{
		base.ViewDidLoad();
		skiaView.PaintSurface += OnPaintSurface;
		skiaView.Paused = true;
		skiaView.EnableSetNeedsDisplay = false;
		skiaView.PreferredFramesPerSecond = 60;

		// Style FPS label as pill
		fpsLabel.WantsLayer = true;
		fpsLabel.Layer!.BackgroundColor = new CGColor(0f, 0f, 0f, 0.5f);
		fpsLabel.Layer.CornerRadius = 12;
		fpsLabel.Layer.BorderWidth = 1;
		fpsLabel.Layer.BorderColor = new CGColor(1f, 1f, 1f, 0.2f);
		fpsLabel.TextColor = NSColor.White;
		fpsLabel.DrawsBackground = false;
		fpsLabel.Editable = false;
		fpsLabel.Bezeled = false;
	}

	public override void ViewDidAppear()
	{
		base.ViewDidAppear();
		fpsCounter.Start();
		skiaView.Paused = false;
	}

	public override void ViewWillDisappear()
	{
		base.ViewWillDisappear();
		skiaView.Paused = true;
		fpsCounter.Stop();
	}

	public override void MouseDown(NSEvent theEvent) => UpdateTouch(theEvent, true);
	public override void MouseDragged(NSEvent theEvent) => UpdateTouch(theEvent, true);
	public override void MouseUp(NSEvent theEvent) => UpdateTouch(theEvent, false);

	void UpdateTouch(NSEvent theEvent, bool active)
	{
		touchActive = active;
		if (!active || View == null) return;
		var loc = View.ConvertPointFromView(theEvent.LocationInWindow, null);
		touchPos = new SKPoint((float)(loc.X / View.Bounds.Width), (float)(1.0 - loc.Y / View.Bounds.Height));
	}

	void OnPaintSurface(object? sender, SKPaintMetalSurfaceEventArgs e)
	{
		var canvas = e.Surface.Canvas;
		var width = e.Info.Width;
		var height = e.Info.Height;

		var builder = shaderBuilder.Value;

		builder.Uniforms["iTime"] = fpsCounter.ElapsedSeconds;
		builder.Uniforms["iResolution"] = new float[] { width, height };
		builder.Uniforms["iTouchPos"] = new float[] { touchPos.X, touchPos.Y };
		builder.Uniforms["iTouchActive"] = touchActive ? 1f : 0f;
		builder.Uniforms["iColors"] = blobColors;

		using var shader = builder.Build();
		using var paint = new SKPaint { Shader = shader };
		canvas.DrawRect(0, 0, width, height, paint);

		if (fpsCounter.Tick() is double fps)
			lastFps = fps;

		BeginInvokeOnMainThread(() => fpsLabel.StringValue = $"  FPS: {lastFps:F0} (Metal)  ");
	}
}
