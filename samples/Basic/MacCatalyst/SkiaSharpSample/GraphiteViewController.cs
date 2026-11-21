using SkiaSharp;
using SkiaSharp.Views.iOS;

namespace SkiaSharpSample;

public sealed class GraphiteViewController : UIViewController
{
	private SKGraphiteView? graphiteView;
	private readonly FpsCounter fps = new();
	private SKRuntimeShaderBuilder? builder;
	private SKPoint touch;
	private bool touching;

	public GraphiteViewController()
	{
		TabBarItem = new UITabBarItem("Graphite", UIImage.GetSystemImage("cube.transparent"), 3);
	}

	public override void ViewDidLoad()
	{
		base.ViewDidLoad();

		graphiteView = new SKGraphiteView(View!.Bounds)
		{
			AutoresizingMask = UIViewAutoresizing.FlexibleDimensions,
			AccessibilityIdentifier = "SkiaCanvas",
		};
		graphiteView.PaintSurface += OnPaintSurface;
		View.AddSubview(graphiteView);
	}

	public override void ViewDidAppear(bool animated)
	{
		base.ViewDidAppear(animated);

		builder ??= SKRuntimeEffect.BuildShader(GaneshViewController.ShaderSource);
		fps.Start();
		graphiteView!.HasRenderLoop = true;
	}

	public override void ViewWillDisappear(bool animated)
	{
		graphiteView!.HasRenderLoop = false;
		fps.Stop();

		base.ViewWillDisappear(animated);
	}

	private void OnPaintSurface(object? sender, SKPaintGraphiteSurfaceEventArgs e)
	{
		if (builder is null)
			return;

		builder.Uniforms["iTime"] = fps.ElapsedSeconds;
		builder.Uniforms["iResolution"] = new float[] { e.Info.Width, e.Info.Height };
		builder.Uniforms["iTouchPos"] = new float[] { touch.X, touch.Y };
		builder.Uniforms["iTouchActive"] = touching ? 1f : 0f;
		builder.Uniforms["iColors"] = GaneshViewController.blobColors;

		using var shader = builder.Build();
		using var paint = new SKPaint { Shader = shader };
		e.Surface.Canvas.DrawRect(0, 0, e.Info.Width, e.Info.Height, paint);
	}

	public override void TouchesBegan(NSSet touches, UIEvent? evt)
	{
		touching = true;
		UpdateTouch(touches);
		base.TouchesBegan(touches, evt);
	}

	public override void TouchesMoved(NSSet touches, UIEvent? evt)
	{
		UpdateTouch(touches);
		base.TouchesMoved(touches, evt);
	}

	public override void TouchesEnded(NSSet touches, UIEvent? evt)
	{
		touching = false;
		base.TouchesEnded(touches, evt);
	}

	public override void TouchesCancelled(NSSet touches, UIEvent? evt)
	{
		touching = false;
		base.TouchesCancelled(touches, evt);
	}

	private void UpdateTouch(NSSet touches)
	{
		if (touches.AnyObject is UITouch input && View is { } view)
		{
			var point = input.LocationInView(view);
			touch = new SKPoint((float)(point.X / view.Bounds.Width), (float)(point.Y / view.Bounds.Height));
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			graphiteView?.Dispose();
			builder?.Dispose();
		}

		base.Dispose(disposing);
	}
}
