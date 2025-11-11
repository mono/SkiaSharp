using AppKit;
using CoreGraphics;
using Foundation;

namespace SkiaSharpSample
{
	static class MainClass
	{
		static void Main(string[] args)
		{
			NSApplication.Init();
			var app = NSApplication.SharedApplication;
			var appDelegate = new AppDelegate();
			app.Delegate = appDelegate;
			
			// Create window programmatically (no storyboard)
			var window = CreateWindow();
			var viewController = new ViewController();
			window.ContentViewController = viewController;
			
			window.MakeKeyAndOrderFront(null);
			app.ActivateIgnoringOtherApps(true);
			
			// Process initial events to show window
			app.FinishLaunching();
			
			// Run tight render loop (like C++ motionmark_app)
			appDelegate.RunRenderLoop(window, viewController);
		}
		
		static NSWindow CreateWindow()
		{
			var screenRect = NSScreen.MainScreen.VisibleFrame;
			var windowRect = new CGRect(
				(screenRect.Width - 1280) / 2,
				(screenRect.Height - 960) / 2,
				1280,
				960
			);
			
			var window = new NSWindow(
				windowRect,
				NSWindowStyle.Titled | NSWindowStyle.Closable | NSWindowStyle.Resizable | NSWindowStyle.Miniaturizable,
				NSBackingStore.Buffered,
				false
			);
			
			window.Title = "MotionMark SkiaSharp (OpenGL)";
			window.MinSize = new CGSize(640, 480);
			
			return window;
		}
	}
}
