using AppKit;
using CoreGraphics;
using Foundation;
using ObjCRuntime;
using SkiaSharp;
using SkiaSharp.Views.Mac;

namespace SkiaSharpSample;

[Register("GraphiteViewController")]
public class GraphiteViewController : NSViewController
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
	NSTextField fpsLabel { get; set; } = null!;

	public GraphiteViewController(NativeHandle handle) : base(handle) { }

	public override void ViewDidLoad()
	{
		base.ViewDidLoad();
		skiaView.PaintSurface += OnPaintSurface;
		skiaView.RenderFailed += OnRenderFailed;
		skiaView.Paused = true;
		skiaView.EnableSetNeedsDisplay = false;
		skiaView.PreferredFramesPerSecond = 60;

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
		if (renderFailed)
			return;

		fpsCounter = new FpsCounter();
		fpsCounter.Start();
		skiaView.Paused = false;
	}

	public override void ViewWillDisappear()
	{
		skiaView.Paused = true;
		fpsCounter.Stop();
		base.ViewWillDisappear();
	}

	public override void MouseDown(NSEvent theEvent) => UpdateTouch(theEvent, true);

	public override void MouseDragged(NSEvent theEvent) => UpdateTouch(theEvent, true);

	public override void MouseUp(NSEvent theEvent) => UpdateTouch(theEvent, false);

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

	void UpdateTouch(NSEvent theEvent, bool active)
	{
		touchActive = active;
		if (!active || skiaView.Bounds.Width <= 0 || skiaView.Bounds.Height <= 0)
			return;

		var location = skiaView.ConvertPointFromView(theEvent.LocationInWindow, null);
		touchPoint = new SKPoint(
			(float)(location.X / skiaView.Bounds.Width),
			(float)(1.0 - location.Y / skiaView.Bounds.Height));
	}

	void OnPaintSurface(object? sender, SKPaintGraphiteSurfaceEventArgs e)
	{
		DrawScene(e.Surface.Canvas, e.Info, fpsCounter.ElapsedSeconds, touchPoint, touchActive);
		if (fpsCounter.Tick() is double fps)
			BeginInvokeOnMainThread(() => fpsLabel.StringValue = $"  GRAPHITE / METAL  |  FPS: {fps:F0}  ");
	}

	void OnRenderFailed(object? sender, SKGraphiteRenderFailedEventArgs e)
	{
		renderFailed = true;
		BeginInvokeOnMainThread(() =>
		{
			fpsLabel.StringValue = "  GRAPHITE / METAL  |  RENDER FAILED  ";
			new NSAlert
			{
				MessageText = "Graphite rendering failed",
				InformativeText = e.Exception.Message,
			}.RunModal();
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
