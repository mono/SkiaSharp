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
	FpsCounter fpsCounter = new();
	GraphiteSceneRenderer? renderer;
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
			renderer?.Dispose();
			renderer = null;
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
		renderer ??= new GraphiteSceneRenderer(e);
		renderer.Draw(e, fpsCounter.ElapsedSeconds, touchActive ? touchPoint : null);
		if (fpsCounter.Tick() is double fps)
			BeginInvokeOnMainThread(() => fpsLabel.StringValue = $"  FPS: {fps:F0}  ");
	}

	void OnRenderFailed(object? sender, SKGraphiteRenderFailedEventArgs e)
	{
		renderFailed = true;
		BeginInvokeOnMainThread(() =>
		{
			fpsLabel.StringValue = "  RENDER FAILED  ";
			new NSAlert
			{
				MessageText = "Graphite rendering failed",
				InformativeText = e.Exception.Message,
			}.RunModal();
		});
	}

}
