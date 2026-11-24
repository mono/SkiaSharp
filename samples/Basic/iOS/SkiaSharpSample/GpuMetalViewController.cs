using ObjCRuntime;
using SkiaSharp;
using SkiaSharp.Views.iOS;

namespace SkiaSharpSample;

[Register("GpuMetalViewController")]
public class GpuMetalViewController : UIViewController
{
	private const string ShaderSource = @"
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
		1.0f, 0.3f, 0.4f,
		0.3f, 0.7f, 1.0f,
		1.0f, 0.6f, 0.1f,
		0.4f, 1.0f, 0.7f,
		0.7f, 0.3f, 1.0f,
		1.0f, 0.9f, 0.2f,
	};

	private readonly FpsCounter fpsCounter = new();
	private Lazy<SKRuntimeShaderBuilder>? shaderBuilder;
	private SKPoint touchPos;
	private bool touchActive;

	[Outlet("skiaView")]
	SKMetalView skiaView { get; set; } = null!;

	[Outlet("fpsLabel")]
	UILabel fpsLabel { get; set; } = null!;

	public GpuMetalViewController(NativeHandle handle) : base(handle) { }

	public override void ViewDidLoad()
	{
		base.ViewDidLoad();
		fpsLabel.Layer.BorderColor = UIColor.White.ColorWithAlpha(0.2f).CGColor;
		skiaView.PaintSurface += OnPaintSurface;
		skiaView.Paused = true;
		skiaView.EnableSetNeedsDisplay = false;
	}

	public override void ViewDidAppear(bool animated)
	{
		base.ViewDidAppear(animated);
		shaderBuilder ??= new Lazy<SKRuntimeShaderBuilder>(() => SKRuntimeEffect.BuildShader(ShaderSource));
		fpsCounter.Start();
		skiaView.Paused = false;
	}

	public override void ViewWillDisappear(bool animated)
	{
		base.ViewWillDisappear(animated);
		skiaView.Paused = true;
		fpsCounter.Stop();
	}

	private void OnPaintSurface(object? sender, SKPaintMetalSurfaceEventArgs e)
	{
		var canvas = e.Surface.Canvas;
		var width = e.BackendRenderTarget.Width;
		var height = e.BackendRenderTarget.Height;
		canvas.Clear(SKColors.Black);

		if (shaderBuilder == null) return;
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
			InvokeOnMainThread(() => fpsLabel.Text = $"  FPS: {fps:F0}  ");
	}

	public override void TouchesBegan(NSSet touches, UIEvent? evt)
	{
		base.TouchesBegan(touches, evt);
		if (touches.AnyObject is UITouch touch && View != null)
		{
			var loc = touch.LocationInView(View);
			touchPos = new SKPoint((float)(loc.X / View.Bounds.Width), (float)(loc.Y / View.Bounds.Height));
			touchActive = true;
		}
	}

	public override void TouchesMoved(NSSet touches, UIEvent? evt)
	{
		base.TouchesMoved(touches, evt);
		if (touches.AnyObject is UITouch touch && View != null)
		{
			var loc = touch.LocationInView(View);
			touchPos = new SKPoint((float)(loc.X / View.Bounds.Width), (float)(loc.Y / View.Bounds.Height));
		}
	}

	public override void TouchesEnded(NSSet touches, UIEvent? evt)
	{
		base.TouchesEnded(touches, evt);
		touchActive = false;
	}

	public override void TouchesCancelled(NSSet touches, UIEvent? evt)
	{
		base.TouchesCancelled(touches, evt);
		touchActive = false;
	}
}
