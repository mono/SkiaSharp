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

	private SKMetalView? skiaView;

	private SKRuntimeShaderBuilder? shaderBuilder;
	private long startTime;

	// Touch interaction
	private float touchX;
	private float touchY;
	private float touchActive;

	// FPS tracking
	private UILabel? fpsLabel;
	private int frameCount;
	private double lastFpsUpdate;

	public GpuMetalViewController() : base() { }

	public override void ViewDidLoad()
	{
		base.ViewDidLoad();
		Title = "GPU (Metal)";

		View!.BackgroundColor = UIColor.Black;
		skiaView = new SKMetalView(View.Bounds);
		skiaView.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
		View.AddSubview(skiaView);

		skiaView.PaintSurface += OnPaintSurface;
		skiaView.Paused = true;
		skiaView.EnableSetNeedsDisplay = false;

		// FPS label added programmatically on top of the storyboard layout
		fpsLabel = new UILabel
		{
			TranslatesAutoresizingMaskIntoConstraints = false,
			Text = "FPS: --",
			TextColor = UIColor.White,
			Font = UIFont.MonospacedDigitSystemFontOfSize(14, UIFontWeight.Regular),
			BackgroundColor = UIColor.FromWhiteAlpha(0, 0.5f),
		};
		View!.AddSubview(fpsLabel);

		NSLayoutConstraint.ActivateConstraints(new[]
		{
			fpsLabel.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor, 8),
			fpsLabel.TrailingAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TrailingAnchor, -8),
		});
	}

	public override void ViewDidAppear(bool animated)
	{
		base.ViewDidAppear(animated);

		try
		{
			shaderBuilder = SKRuntimeEffect.BuildShader(ShaderSource);
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"Shader compilation failed: {ex.Message}");
			return;
		}

		startTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
		frameCount = 0;
		lastFpsUpdate = 0;

		// Start continuous rendering via MTKView
		if (skiaView != null)
		{
			skiaView.Paused = false;
			skiaView.EnableSetNeedsDisplay = false;
		}
	}

	public override void ViewWillDisappear(bool animated)
	{
		base.ViewWillDisappear(animated);

		if (skiaView != null)
			skiaView.Paused = true;

		shaderBuilder?.Dispose();
		shaderBuilder = null;
	}

	private void OnPaintSurface(object? sender, SKPaintMetalSurfaceEventArgs e)
	{
		var canvas = e.Surface.Canvas;
		var width = e.BackendRenderTarget.Width;
		var height = e.BackendRenderTarget.Height;

		canvas.Clear(SKColors.Black);

		if (shaderBuilder == null)
			return;

		var time = (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - startTime) / 1000f;

		shaderBuilder.Uniforms["iTime"] = time;
		shaderBuilder.Uniforms["iResolution"] = new float[] { width, height };
		shaderBuilder.Uniforms["iTouchPos"] = new float[] { touchX, touchY };
		shaderBuilder.Uniforms["iTouchActive"] = touchActive;

		using var shader = shaderBuilder.Build();
		using var paint = new SKPaint { Shader = shader };
		canvas.DrawRect(0, 0, width, height, paint);

		UpdateFps(time);
	}

	private void UpdateFps(float currentTime)
	{
		frameCount++;
		if (currentTime - lastFpsUpdate >= 1.0)
		{
			var fps = frameCount / (currentTime - lastFpsUpdate);
			InvokeOnMainThread(() =>
			{
				if (fpsLabel != null)
					fpsLabel.Text = $"FPS: {fps:F0}";
			});
			frameCount = 0;
			lastFpsUpdate = currentTime;
		}
	}

	public override void TouchesBegan(NSSet touches, UIEvent? evt)
	{
		base.TouchesBegan(touches, evt);
		if (touches.AnyObject is UITouch touch && View != null)
		{
			var loc = touch.LocationInView(View);
			touchX = (float)(loc.X / View.Bounds.Width);
			touchY = (float)(loc.Y / View.Bounds.Height);
			touchActive = 1f;
		}
	}

	public override void TouchesMoved(NSSet touches, UIEvent? evt)
	{
		base.TouchesMoved(touches, evt);
		if (touches.AnyObject is UITouch touch && View != null)
		{
			var loc = touch.LocationInView(View);
			touchX = (float)(loc.X / View.Bounds.Width);
			touchY = (float)(loc.Y / View.Bounds.Height);
		}
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
}
