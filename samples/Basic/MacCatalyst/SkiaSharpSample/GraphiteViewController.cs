using ObjCRuntime;
using SkiaSharp;
using SkiaSharp.Views.iOS;

namespace SkiaSharpSample;

[Register("GraphiteViewController")]
public class GraphiteViewController : UIViewController
{
	static readonly SKColor[] colors =
	{
		new(0x35, 0x8C, 0xFF),
		new(0x7C, 0x4D, 0xFF),
		new(0x00, 0xC9, 0xA7),
		new(0xFF, 0xB0, 0x2E),
		new(0xF4, 0x5B, 0x8A),
	};

	FpsCounter fpsCounter = new();
	SKPoint touchPoint = new(0.5f, 0.5f);
	bool touchActive;
	bool renderFailed;

	[Outlet("skiaView")]
	SKGraphiteMetalView skiaView { get; set; } = null!;

	[Outlet("fpsLabel")]
	UILabel fpsLabel { get; set; } = null!;

	public GraphiteViewController(NativeHandle handle) : base(handle) { }

	public override void ViewDidLoad()
	{
		base.ViewDidLoad();
		fpsLabel.Layer.BorderColor = UIColor.White.ColorWithAlpha(0.2f).CGColor;
		skiaView.PaintSurface += OnPaintSurface;
		skiaView.RenderFailed += OnRenderFailed;
		skiaView.Paused = true;
		skiaView.EnableSetNeedsDisplay = false;
		skiaView.PreferredFramesPerSecond = 60;
	}

	public override void ViewDidAppear(bool animated)
	{
		base.ViewDidAppear(animated);
		if (renderFailed)
			return;

		fpsCounter = new FpsCounter();
		fpsCounter.Start();
		skiaView.Paused = false;
	}

	public override void ViewWillDisappear(bool animated)
	{
		skiaView.Paused = true;
		fpsCounter.Stop();
		base.ViewWillDisappear(animated);
	}

	public override void TouchesBegan(NSSet touches, UIEvent? evt)
	{
		base.TouchesBegan(touches, evt);
		UpdateTouch(touches, true);
	}

	public override void TouchesMoved(NSSet touches, UIEvent? evt)
	{
		base.TouchesMoved(touches, evt);
		UpdateTouch(touches, true);
	}

	public override void TouchesEnded(NSSet touches, UIEvent? evt)
	{
		touchActive = false;
		base.TouchesEnded(touches, evt);
	}

	public override void TouchesCancelled(NSSet touches, UIEvent? evt)
	{
		touchActive = false;
		base.TouchesCancelled(touches, evt);
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && skiaView != null)
		{
			skiaView.Paused = true;
			skiaView.PaintSurface -= OnPaintSurface;
			skiaView.RenderFailed -= OnRenderFailed;
		}
		base.Dispose(disposing);
	}

	void UpdateTouch(NSSet touches, bool active)
	{
		if (touches.AnyObject is not UITouch touch || skiaView.Bounds.Width <= 0 || skiaView.Bounds.Height <= 0)
			return;

		var location = touch.LocationInView(skiaView);
		touchPoint = new SKPoint(
			(float)(location.X / skiaView.Bounds.Width),
			(float)(location.Y / skiaView.Bounds.Height));
		touchActive = active;
	}

	void OnPaintSurface(object? sender, SKPaintGraphiteSurfaceEventArgs e)
	{
		DrawScene(e.Surface.Canvas, e.Info, fpsCounter.ElapsedSeconds, touchPoint, touchActive);
		if (fpsCounter.Tick() is double fps)
			BeginInvokeOnMainThread(() => fpsLabel.Text = $"  GRAPHITE / METAL  |  FPS: {fps:F0}  ");
	}

	void OnRenderFailed(object? sender, SKGraphiteRenderFailedEventArgs e)
	{
		renderFailed = true;
		BeginInvokeOnMainThread(() =>
		{
			fpsLabel.Text = "  GRAPHITE / METAL  |  RENDER FAILED  ";
			var alert = UIAlertController.Create("Graphite rendering failed", e.Exception.Message, UIAlertControllerStyle.Alert);
			alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
			PresentViewController(alert, true, null);
		});
	}

	static void DrawScene(SKCanvas canvas, SKImageInfo info, float elapsed, SKPoint touch, bool active)
	{
		var width = info.Width;
		var height = info.Height;
		var scale = Math.Min(width, height);
		canvas.Clear(new SKColor(0x08, 0x0B, 0x1A));

		using var paint = new SKPaint { IsAntialias = true };
		for (var i = 0; i < colors.Length; i++)
		{
			var angle = elapsed * (0.35f + i * 0.07f) + i * 1.25f;
			var x = width * 0.5f + MathF.Cos(angle) * width * (0.18f + i * 0.015f);
			var y = height * 0.5f + MathF.Sin(angle * 1.3f) * height * (0.16f + i * 0.012f);
			paint.Color = colors[i];
			paint.Style = SKPaintStyle.Fill;
			canvas.DrawCircle(x, y, scale * (0.055f + i * 0.008f), paint);
		}

		if (active)
		{
			paint.Color = SKColors.White;
			paint.Style = SKPaintStyle.Stroke;
			paint.StrokeWidth = Math.Max(3f, scale * 0.008f);
			canvas.DrawCircle(touch.X * width, touch.Y * height, scale * 0.09f, paint);
		}

		using var titleFont = new SKFont { Size = scale * 0.11f };
		using var subtitleFont = new SKFont { Size = scale * 0.038f };
		paint.Color = SKColors.White;
		paint.Style = SKPaintStyle.Fill;
		canvas.DrawText("GRAPHITE", width * 0.5f, height * 0.48f, SKTextAlign.Center, titleFont, paint);
		paint.Color = new SKColor(0x8E, 0xC5, 0xFF);
		canvas.DrawText("METAL", width * 0.5f, height * 0.55f, SKTextAlign.Center, subtitleFont, paint);
	}
}
