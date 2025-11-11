using AppKit;
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
			
			// Load storyboard and create window
			var storyboard = NSStoryboard.FromName("Main", null);
			var windowController = (NSWindowController?)storyboard?.InstantiateInitialController();
			windowController?.ShowWindow(null);
			app.ActivateIgnoringOtherApps(true);
			
			// Process initial events to show window
			app.FinishLaunching();
			
			// Run tight render loop (like C++ motionmark_app)
			appDelegate.RunRenderLoop(windowController);
		}
	}
}
