using System.Diagnostics;
using CoreAnimation;
using Foundation;
using SkiaSharp;
using SkiaSharp.Views.tvOS;

namespace SkiaSharpSample;

public class GpuGLViewController : UIViewController
{
	private const string ShaderSource = @"
uniform float iTime;
uniform float2 iResolution;

half4 main(float2 fragCoord) {
    float2 uv = fragCoord / iResolution;
    float aspect = iResolution.x / iResolution.y;
    uv.x *= aspect;

    float cx = aspect * 0.5;
    float cy = 0.5;

    float2 c1 = float2(cx + 0.25 * cos(iTime * 0.7), cy + 0.25 * sin(iTime * 0.9));
    float2 c2 = float2(cx + 0.25 * sin(iTime * 0.8), cy + 0.25 * cos(iTime * 0.6));
    float2 c3 = float2(cx + 0.20 * cos(iTime * 1.1 + 2.0), cy + 0.20 * sin(iTime * 0.7 + 1.0));
    float2 c4 = float2(cx + 0.15 * sin(iTime * 0.5 + 3.0), cy + 0.15 * cos(iTime * 1.3 + 2.0));

    float field = 0.0;
    field += 0.015 / (dot(uv - c1, uv - c1) + 0.0001);
    field += 0.015 / (dot(uv - c2, uv - c2) + 0.0001);
    field += 0.010 / (dot(uv - c3, uv - c3) + 0.0001);
    field += 0.008 / (dot(uv - c4, uv - c4) + 0.0001);

    float3 col = float3(0.02, 0.02, 0.05);

    if (field > 2.5) {
        float t = clamp((field - 2.5) / 8.0, 0.0, 1.0);
        float3 a = float3(0.05, 0.25, 0.6);
        float3 b = float3(0.8, 0.1, 0.4);
        float3 c_col = float3(1.0, 0.8, 0.3);
        col = mix(a, b, smoothstep(0.0, 0.5, t));
        col = mix(col, c_col, smoothstep(0.5, 1.0, t));
    }

    if (field > 2.0 && field < 3.0) {
        float glow = smoothstep(2.0, 2.5, field) * smoothstep(3.0, 2.5, field);
        col += float3(0.1, 0.2, 0.5) * glow;
    }

    return half4(half3(col), 1.0);
}";

	private SKGLView? glView;
	private CADisplayLink? displayLink;
	private readonly Stopwatch stopwatch = new();
	private SKRuntimeEffect? shaderEffect;

	public override void ViewDidLoad()
	{
		base.ViewDidLoad();

		shaderEffect = SKRuntimeEffect.CreateShader(ShaderSource, out var errors);
		if (errors != null)
			Console.WriteLine($"Shader compile error: {errors}");

#pragma warning disable CA1422 // Validate platform compatibility
		glView = new SKGLView(View!.Bounds)
		{
			AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight
		};
#pragma warning restore CA1422

		glView.PaintSurface += OnPaintSurface;
		View.AddSubview(glView);

		displayLink = CADisplayLink.Create(() => glView.Display());
		displayLink.PreferredFramesPerSecond = 60;
		displayLink.AddToRunLoop(NSRunLoop.Current, NSRunLoopMode.Default);

		stopwatch.Start();
	}

	public override void ViewWillDisappear(bool animated)
	{
		base.ViewWillDisappear(animated);
		displayLink?.Invalidate();
		displayLink = null;
		stopwatch.Stop();
	}

	public override void ViewWillAppear(bool animated)
	{
		base.ViewWillAppear(animated);
		if (displayLink == null && glView != null)
		{
			displayLink = CADisplayLink.Create(() => glView.Display());
			displayLink.PreferredFramesPerSecond = 60;
			displayLink.AddToRunLoop(NSRunLoop.Current, NSRunLoopMode.Default);
		}
		stopwatch.Start();
	}

	private void OnPaintSurface(object? sender, SKPaintGLSurfaceEventArgs e)
	{
		var canvas = e.Surface.Canvas;
		var info = e.Info;
		canvas.Clear(SKColors.Black);

		if (shaderEffect == null)
			return;

		float elapsed = (float)stopwatch.Elapsed.TotalSeconds;

		var uniforms = new SKRuntimeEffectUniforms(shaderEffect);
		uniforms["iTime"] = elapsed;
		uniforms["iResolution"] = new float[] { info.Width, info.Height };

		using var shader = shaderEffect.ToShader(uniforms);
		using var paint = new SKPaint { Shader = shader };
		canvas.DrawRect(SKRect.Create(info.Width, info.Height), paint);

		// Label
		using var textPaint = new SKPaint { Color = new SKColor(255, 255, 255, 180), IsAntialias = true };
		using var font = new SKFont { Size = 36 };
		canvas.DrawText("GPU \u2014 OpenGL ES", new SKPoint(info.Width / 2f, info.Height - 60), SKTextAlign.Center, font, textPaint);
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			displayLink?.Invalidate();
			displayLink = null;
			shaderEffect?.Dispose();
		}
		base.Dispose(disposing);
	}
}
