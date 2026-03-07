using System.Diagnostics;
using Microsoft.UI.Xaml.Input;
using SkiaSharp;
using SkiaSharp.Views.Windows;

namespace SkiaSharpSample;

public sealed partial class GpuPage : Page
{
	private const string SkslSource = @"
uniform float iTime;
uniform float2 iResolution;
uniform float2 iTouchPos;
uniform float iTouchActive;

half4 main(float2 fragCoord) {
    float2 uv = fragCoord / iResolution;
    float aspect = iResolution.x / iResolution.y;
    float2 st = float2(uv.x * aspect, uv.y);
    float t = iTime;
    float field = 0.0;
    float3 weighted = float3(0.0);
    float3 colors[6];
    colors[0] = float3(1.0, 0.3, 0.4);
    colors[1] = float3(0.3, 0.7, 1.0);
    colors[2] = float3(1.0, 0.6, 0.1);
    colors[3] = float3(0.4, 1.0, 0.7);
    colors[4] = float3(0.7, 0.3, 1.0);
    colors[5] = float3(1.0, 0.9, 0.2);
    for (int i = 0; i < 6; i++) {
        float fi = float(i);
        float phase = fi * 1.047;
        float speed = 0.3 + fi * 0.07;
        float2 center = float2(
            aspect * 0.5 + 0.4 * sin(t * speed + phase) * cos(t * speed * 0.6 + fi),
            0.5 + 0.4 * cos(t * speed * 0.8 + phase * 1.3) * sin(t * speed * 0.4 + fi * 0.7));
        float2 d = st - center;
        float r = length(d);
        float strength = 0.030 / (r * r + 0.002);
        field += strength;
        weighted += colors[i] * strength;
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

	private readonly Stopwatch stopwatch = new Stopwatch();

	private SKRuntimeShaderBuilder? shaderBuilder;
	private int frameCount;
	private double lastFpsTime;
	private float touchX;
	private float touchY;
	private float touchActive;

	public GpuPage()
	{
		InitializeComponent();

		stopwatch.Start();
		Unloaded += OnUnloaded;
	}

	private void OnUnloaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
	{
		stopwatch.Stop();
		shaderBuilder?.Dispose();
		shaderBuilder = null;
	}

	private void OnPaintSurface(object? sender, SKPaintGLSurfaceEventArgs e)
	{
		var canvas = e.Surface.Canvas;
		var width = e.BackendRenderTarget.Width;
		var height = e.BackendRenderTarget.Height;

		if (shaderBuilder == null)
		{
			shaderBuilder = SKRuntimeEffect.BuildShader(SkslSource);
		}

		if (shaderBuilder == null)
		{
			canvas.Clear(SKColors.Magenta);
			return;
		}

		shaderBuilder["iTime"] = (float)stopwatch.Elapsed.TotalSeconds;
		shaderBuilder["iResolution"] = new float[] { (float)width, (float)height };
		shaderBuilder["iTouchPos"] = new float[] { touchX, touchY };
		shaderBuilder["iTouchActive"] = touchActive;

		using var shader = shaderBuilder.Build();
		using var paint = new SKPaint { Shader = shader };
		canvas.DrawRect(0, 0, width, height, paint);

		UpdateFps();
	}

	private void UpdateFps()
	{
		frameCount++;
		var elapsed = stopwatch.Elapsed.TotalSeconds;
		if (elapsed - lastFpsTime >= 0.5)
		{
			var fps = frameCount / (elapsed - lastFpsTime);
			if (fpsText != null)
				DispatcherQueue.TryEnqueue(() => fpsText.Text = $"FPS: {fps:F0}");
			frameCount = 0;
			lastFpsTime = elapsed;
		}
	}

	private void OnPointerPressed(object? sender, PointerRoutedEventArgs e)
	{
		touchActive = 1f;
		UpdateTouchPosition(e);
		skiaView.CapturePointer(e.Pointer);
	}

	private void OnPointerMoved(object? sender, PointerRoutedEventArgs e)
	{
		if (touchActive > 0.5f)
			UpdateTouchPosition(e);
	}

	private void OnPointerReleased(object? sender, PointerRoutedEventArgs e)
	{
		touchActive = 0f;
		skiaView.ReleasePointerCapture(e.Pointer);
	}

	private void UpdateTouchPosition(PointerRoutedEventArgs e)
	{
		var point = e.GetCurrentPoint(skiaView);
		var w = skiaView.ActualWidth;
		var h = skiaView.ActualHeight;
		if (w > 0 && h > 0)
		{
			touchX = (float)(point.Position.X / w);
			touchY = (float)(point.Position.Y / h);
		}
	}
}
