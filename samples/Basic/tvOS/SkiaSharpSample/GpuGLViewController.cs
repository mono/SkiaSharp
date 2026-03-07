using System.Diagnostics;
using CoreAnimation;
using Foundation;
using SkiaSharp;
using SkiaSharp.Views.tvOS;

namespace SkiaSharpSample;

[Register("GpuGLViewController")]
public class GpuGLViewController : UIViewController
{
	private const string ShaderSource = @"
uniform float iTime;
uniform float2 iResolution;

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
            0.5 + 0.4 * cos(t * speed * 0.8 + phase * 1.3) * sin(t * speed * 0.4 + fi * 0.7)
        );
        float2 d = st - center;
        float r = length(d);
        float strength = 0.030 / (r * r + 0.002);
        field += strength;
        weighted += colors[i] * strength;
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

	[Outlet]
	SKGLView skiaView { get; set; } = null!;

	private CADisplayLink? displayLink;
	private readonly Stopwatch stopwatch = new();
	private SKRuntimeEffect? shaderEffect;

	public GpuGLViewController(IntPtr handle) : base(handle) { }

	public override void ViewDidLoad()
	{
		base.ViewDidLoad();

		shaderEffect = SKRuntimeEffect.CreateShader(ShaderSource, out var errors);
		if (errors != null)
			Console.WriteLine($"Shader compile error: {errors}");

		skiaView.PaintSurface += OnPaintSurface;

		displayLink = CADisplayLink.Create(() => skiaView.Display());
		displayLink.PreferredFramesPerSecond = 60;
		displayLink.AddToRunLoop(NSRunLoop.Current, NSRunLoopMode.Common);

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
		if (displayLink == null)
		{
			displayLink = CADisplayLink.Create(() => skiaView.Display());
			displayLink.PreferredFramesPerSecond = 60;
			displayLink.AddToRunLoop(NSRunLoop.Current, NSRunLoopMode.Common);
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
