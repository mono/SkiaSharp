using System;
using System.Diagnostics;
using Gtk;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using SkiaSharp.Views.Gtk;

namespace SkiaSharpSample;

public class GpuPage : Box
{
	private const string SkslSource = @"
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
}";

	private static readonly float[] blobColors =
	{
		1.0f, 0.3f, 0.4f,
		0.3f, 0.7f, 1.0f,
		1.0f, 0.6f, 0.1f,
		0.4f, 1.0f, 0.7f,
		0.7f, 0.3f, 1.0f,
		1.0f, 0.9f, 0.2f,
	};

	private readonly SKGLArea skiaView;
	private readonly Label fpsLabel;
	private readonly Stopwatch stopwatch = new();
	private readonly GestureClick click;
	private readonly EventControllerMotion motion;
	private readonly SKPaint shaderPaint = new();
	private SKRuntimeShaderBuilder shaderBuilder;
	private SKPoint touchPos;
	private bool touchActive;
	private int frameCount;
	private double lastSampleTime;
	private uint fpsCallback;

	public GpuPage()
		: base(new GObject.ConstructArgument[] { })
	{
		Hexpand = true;
		Vexpand = true;

		var builder = MainWindow.LoadBuilder("GpuPage.ui");
		var overlay = (Overlay)builder.GetObject("gpuOverlay");
		var container = (Box)builder.GetObject("gpuContainer");
		var fpsPill = (Box)builder.GetObject("fpsPill");
		fpsLabel = (Label)builder.GetObject("fpsLabel");
		var fpsStyle = new CssProvider();
		fpsStyle.LoadFromData(
			"box { background-color: rgba(0, 0, 0, 0.667); border: 1px solid rgba(255, 255, 255, 0.267); " +
			"border-radius: 12px; padding: 6px 12px; font-size: 14px; }",
			-1);
		fpsPill.GetStyleContext().AddProvider(fpsStyle, 600);

		skiaView = new SKGLArea { Hexpand = true, Vexpand = true };
		skiaView.PaintSurface += OnPaintSurface;
		container.Append(skiaView);

		click = GestureClick.New();
		click.OnPressed += OnPressed;
		click.OnReleased += OnReleased;
		click.OnCancel += OnCancel;
		skiaView.AddController(click);

		motion = EventControllerMotion.New();
		motion.OnMotion += OnMotion;
		motion.OnLeave += OnLeave;
		skiaView.AddController(motion);

		OnMap += OnMapped;
		OnUnmap += OnUnmapped;
		Append(overlay);
	}

	private void OnMapped(Widget sender, EventArgs args)
	{
		stopwatch.Start();
		lastSampleTime = stopwatch.Elapsed.TotalSeconds;
		frameCount = 0;
		fpsLabel.SetMarkup("<span foreground='white'>FPS: --</span>");
		fpsCallback = AddTickCallback((widget, clock) =>
		{
			var now = stopwatch.Elapsed.TotalSeconds;
			if (now - lastSampleTime >= 0.5)
			{
				fpsLabel.SetMarkup($"<span foreground='white'>FPS: {frameCount / (now - lastSampleTime):F0}</span>");
				frameCount = 0;
				lastSampleTime = now;
			}
			return true;
		});
		skiaView.EnableRenderLoop = true;
	}

	private void OnUnmapped(Widget sender, EventArgs args)
	{
		skiaView.EnableRenderLoop = false;
		if (fpsCallback != 0)
		{
			RemoveTickCallback(fpsCallback);
			fpsCallback = 0;
		}
		stopwatch.Stop();
		touchActive = false;
	}

	private void OnPaintSurface(object sender, SKPaintGLSurfaceEventArgs e)
	{
		var width = e.RawInfo.Width;
		var height = e.RawInfo.Height;

		shaderBuilder ??= SKRuntimeEffect.BuildShader(SkslSource);
		shaderBuilder.Uniforms["iTime"] = (float)stopwatch.Elapsed.TotalSeconds;
		shaderBuilder.Uniforms["iResolution"] = new float[] { width, height };
		shaderBuilder.Uniforms["iTouchPos"] = new float[] { touchPos.X, touchPos.Y };
		shaderBuilder.Uniforms["iTouchActive"] = touchActive ? 1f : 0f;
		shaderBuilder.Uniforms["iColors"] = blobColors;

		using var shader = shaderBuilder.Build();
		shaderPaint.Shader = shader;
		e.Surface.Canvas.DrawRect(0, 0, width, height, shaderPaint);
		shaderPaint.Shader = null;

		frameCount++;
	}

	private void OnPressed(GestureClick sender, GestureClick.PressedSignalArgs args)
	{
		if (sender.GetCurrentButton() != 1)
			return;

		touchActive = true;
		UpdateTouchPosition(args.X, args.Y);
	}

	private void OnReleased(GestureClick sender, GestureClick.ReleasedSignalArgs args)
	{
		touchActive = false;
	}

	private void OnCancel(Gesture sender, Gesture.CancelSignalArgs args) => touchActive = false;

	private void OnMotion(EventControllerMotion sender, EventControllerMotion.MotionSignalArgs args)
	{
		if (touchActive)
			UpdateTouchPosition(args.X, args.Y);
	}

	private void OnLeave(EventControllerMotion sender, EventArgs args) => touchActive = false;

	private void UpdateTouchPosition(double x, double y)
	{
		var width = skiaView.GetWidth();
		var height = skiaView.GetHeight();
		if (width > 0 && height > 0)
			touchPos = new SKPoint((float)(x / width), (float)(y / height));
	}

	public override void Dispose()
	{
		skiaView.EnableRenderLoop = false;
		if (fpsCallback != 0)
		{
			RemoveTickCallback(fpsCallback);
			fpsCallback = 0;
		}
		OnMap -= OnMapped;
		OnUnmap -= OnUnmapped;
		skiaView.PaintSurface -= OnPaintSurface;
		click.OnPressed -= OnPressed;
		click.OnReleased -= OnReleased;
		click.OnCancel -= OnCancel;
		skiaView.RemoveController(click);
		motion.OnMotion -= OnMotion;
		motion.OnLeave -= OnLeave;
		skiaView.RemoveController(motion);
		shaderPaint.Dispose();
		shaderBuilder?.Dispose();
		base.Dispose();
	}
}
