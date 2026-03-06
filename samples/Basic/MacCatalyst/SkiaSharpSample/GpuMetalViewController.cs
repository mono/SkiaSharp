using System.Diagnostics;
using SkiaSharp;
using SkiaSharp.Views.iOS;

namespace SkiaSharpSample;

[Register("GpuMetalViewController")]
public class GpuMetalViewController : UIViewController
{
	const string SkslSource = @"
uniform float2 iResolution;
uniform float iTime;
uniform float2 iTouchPos;
uniform float iTouchActive;

float metaball(float2 p, float2 center, float radius) {
    float d = length(p - center);
    return radius * radius / (d * d + 0.0001);
}

half4 main(float2 fragCoord) {
    float2 p = fragCoord;
    float t = iTime;

    float v = 0.0;
    v += metaball(p, iResolution * float2(0.5 + 0.30 * sin(t * 0.7), 0.5 + 0.30 * cos(t * 0.8)),  iResolution.x * 0.08);
    v += metaball(p, iResolution * float2(0.5 + 0.25 * cos(t * 0.5), 0.5 + 0.25 * sin(t * 0.6)),  iResolution.x * 0.07);
    v += metaball(p, iResolution * float2(0.5 + 0.35 * sin(t * 0.9), 0.5 + 0.20 * cos(t * 1.1)),  iResolution.x * 0.06);
    v += metaball(p, iResolution * float2(0.3 + 0.20 * cos(t * 0.4), 0.7 + 0.20 * sin(t * 0.5)),  iResolution.x * 0.065);
    v += metaball(p, iResolution * float2(0.7 + 0.20 * sin(t * 0.6), 0.3 + 0.20 * cos(t * 0.7)),  iResolution.x * 0.055);

    if (iTouchActive > 0.5) {
        v += metaball(p, iTouchPos, iResolution.x * 0.10);
    }

    half3 col;
    if (v > 1.0) {
        float blend = smoothstep(1.0, 2.5, v);
        col = half3(mix(float3(0.9, 0.2, 0.05), float3(1.0, 0.8, 0.1), blend));
    } else {
        float glow = smoothstep(0.3, 1.0, v);
        col = half3(mix(float3(0.05, 0.02, 0.10), float3(0.5, 0.1, 0.02), glow));
    }

    return half4(col, 1.0);
}
";

	[Outlet]
	SKMetalView? skiaView { get; set; }

	SKRuntimeShaderBuilder? shaderBuilder;
	readonly Stopwatch stopwatch = new();

	float touchX, touchY;
	float touchActive;

	// FPS tracking
	int frameCount;
	double lastFpsTime;
	double currentFps;

	public GpuMetalViewController(IntPtr handle)
		: base(handle)
	{
	}

	public override void ViewDidLoad()
	{
		base.ViewDidLoad();

		try
		{
			shaderBuilder = SKRuntimeEffect.BuildShader(SkslSource);
		}
		catch (Exception ex)
		{
			ShowError($"Shader compilation failed: {ex.Message}");
			return;
		}

		skiaView!.EnableSetNeedsDisplay = false;
		skiaView.Paused = true;
		skiaView.PreferredFramesPerSecond = 60;
		skiaView.PaintSurface += OnPaintSurface;

		stopwatch.Start();
	}

	public override void ViewDidAppear(bool animated)
	{
		base.ViewDidAppear(animated);
		if (skiaView != null)
			skiaView.Paused = false;
	}

	public override void ViewWillDisappear(bool animated)
	{
		base.ViewWillDisappear(animated);
		if (skiaView != null)
			skiaView.Paused = true;
	}

	void ShowError(string message)
	{
		var label = new UILabel
		{
			Text = message,
			TextColor = UIColor.SystemRed,
			TextAlignment = UITextAlignment.Center,
			Lines = 0,
			TranslatesAutoresizingMaskIntoConstraints = false,
		};
		View!.AddSubview(label);
		NSLayoutConstraint.ActivateConstraints(new[]
		{
			label.CenterXAnchor.ConstraintEqualTo(View.CenterXAnchor),
			label.CenterYAnchor.ConstraintEqualTo(View.CenterYAnchor),
			label.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor, 20),
			label.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor, -20),
		});
	}

	void OnPaintSurface(object? sender, SKPaintMetalSurfaceEventArgs e)
	{
		if (shaderBuilder == null)
			return;

		var canvas = e.Surface.Canvas;
		var size = e.BackendRenderTarget.Size;
		canvas.Clear(SKColors.Black);

		float width = size.Width;
		float height = size.Height;

		shaderBuilder.Uniforms["iResolution"] = new float[] { width, height };
		shaderBuilder.Uniforms["iTime"] = (float)stopwatch.Elapsed.TotalSeconds;
		shaderBuilder.Uniforms["iTouchPos"] = new float[] { touchX, touchY };
		shaderBuilder.Uniforms["iTouchActive"] = touchActive;

		using var shader = shaderBuilder.Build();
		using var paint = new SKPaint { Shader = shader };
		canvas.DrawRect(0, 0, width, height, paint);

		// FPS counter — reset every second
		frameCount++;
		double now = stopwatch.Elapsed.TotalSeconds;
		if (now - lastFpsTime >= 1.0)
		{
			currentFps = frameCount / (now - lastFpsTime);
			frameCount = 0;
			lastFpsTime = now;
		}

		using var fpsPaint = new SKPaint
		{
			Color = SKColors.White,
			IsAntialias = true,
		};
		using var fpsFont = new SKFont { Size = 32 };
		canvas.DrawText($"Metal · {currentFps:F0} FPS", new SKPoint(20, 50), SKTextAlign.Left, fpsFont, fpsPaint);

		using var hintPaint = new SKPaint
		{
			Color = new SKColor(0xFF, 0xFF, 0xFF, 0x99),
			IsAntialias = true,
		};
		using var hintFont = new SKFont { Size = 24 };
		string hint = touchActive > 0.5f ? "Touch to attract" : "Touch or click to interact";
		canvas.DrawText(hint, new SKPoint(20, height - 20), SKTextAlign.Left, hintFont, hintPaint);
	}

	public override void TouchesBegan(NSSet touches, UIEvent? evt)
	{
		base.TouchesBegan(touches, evt);
		UpdateTouch(touches);
	}

	public override void TouchesMoved(NSSet touches, UIEvent? evt)
	{
		base.TouchesMoved(touches, evt);
		UpdateTouch(touches);
	}

	public override void TouchesEnded(NSSet touches, UIEvent? evt)
	{
		base.TouchesEnded(touches, evt);
		touchActive = 0f;
	}

	public override void TouchesCancelled(NSSet touches, UIEvent? evt)
	{
		base.TouchesCancelled(touches, evt);
		touchActive = 0f;
	}

	void UpdateTouch(NSSet touches)
	{
		if (touches.AnyObject is not UITouch touch || skiaView == null)
			return;

		var loc = touch.LocationInView(skiaView);
		var scale = skiaView.ContentScaleFactor;
		touchX = (float)(loc.X * scale);
		touchY = (float)(loc.Y * scale);
		touchActive = 1f;
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			shaderBuilder?.Dispose();
			shaderBuilder = null;
		}
		base.Dispose(disposing);
	}
}
