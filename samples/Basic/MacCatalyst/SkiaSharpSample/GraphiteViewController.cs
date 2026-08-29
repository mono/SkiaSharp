using ObjCRuntime;
using SkiaSharp;
using SkiaSharp.Views.iOS;

namespace SkiaSharpSample;

[Register("GraphiteViewController")]
public class GraphiteViewController : UIViewController
{
	FpsCounter fpsCounter = new();
	GraphiteSceneRenderer? renderer;
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
			renderer?.Dispose();
			renderer = null;
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
		renderer ??= new GraphiteSceneRenderer(e);
		renderer.Draw(e, fpsCounter.ElapsedSeconds, touchActive ? touchPoint : null);
		if (fpsCounter.Tick() is double fps)
			BeginInvokeOnMainThread(() => fpsLabel.Text = $"  FPS: {fps:F0}  ");
	}

	void OnRenderFailed(object? sender, SKGraphiteRenderFailedEventArgs e)
	{
		renderFailed = true;
		BeginInvokeOnMainThread(() =>
		{
			fpsLabel.Text = "  RENDER FAILED  ";
			var alert = UIAlertController.Create("Graphite rendering failed", e.Exception.Message, UIAlertControllerStyle.Alert);
			alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
			PresentViewController(alert, true, null);
		});
	}

}
