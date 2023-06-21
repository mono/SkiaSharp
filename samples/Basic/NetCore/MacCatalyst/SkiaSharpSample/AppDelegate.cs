using SkiaSharp;
using SkiaSharp.Views.iOS;

namespace SkiaSharpSample;

[Register("AppDelegate")]
public class AppDelegate : UIApplicationDelegate
{
	public override UIWindow? Window { get; set; }

	public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
	{
		// create a new window instance based on the screen size
		Window = new UIWindow(UIScreen.MainScreen.Bounds);

		// create the canvas view
		var skiaView = new SKCanvasView(Window!.Frame)
		{
			BackgroundColor = UIColor.White,
			IgnorePixelScaling = true,
			AutoresizingMask = UIViewAutoresizing.All,
		};
		skiaView.PaintSurface += OnPaintSurface;

		// create a UIViewController and use the canvas view
		var vc = new UIViewController ();
		vc.View!.AddSubview (skiaView);
		Window.RootViewController = vc;

		// make the window visible
		Window.MakeKeyAndVisible ();

		return true;
	}

	private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
	{
		// the the canvas and properties
		var canvas = e.Surface.Canvas;

		// make sure the canvas is blank
		canvas.Clear(SKColors.White);

		// draw some text
		var paint = new SKPaint
		{
			Color = SKColors.Black,
			IsAntialias = true,
			Style = SKPaintStyle.Fill,
			TextAlign = SKTextAlign.Center,
			TextSize = 24
		};
		var coord = new SKPoint(e.Info.Width / 2, (e.Info.Height + paint.TextSize) / 2);
		canvas.DrawText("SkiaSharp", coord, paint);
	}
}
