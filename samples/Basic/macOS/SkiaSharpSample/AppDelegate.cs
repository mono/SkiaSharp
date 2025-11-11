using AppKit;
using Foundation;

namespace SkiaSharpSample
{
	[Register(nameof(AppDelegate))]
	public class AppDelegate : NSApplicationDelegate
	{
		private volatile bool _shouldTerminate;

		public override void WillTerminate(NSNotification notification)
		{
			_shouldTerminate = true;
		}

		public override bool ApplicationShouldTerminateAfterLastWindowClosed(NSApplication sender) => true;

		public void RunRenderLoop(NSWindow? window, ViewController? viewController)
		{
			if (window == null || viewController == null)
				return;

			var app = NSApplication.SharedApplication;
			
			// Tight render loop matching C++ implementation
			while (!_shouldTerminate && window.IsVisible)
			{
				// 1. Process all pending events (non-blocking)
				NSEvent? ev;
				do
				{
					ev = app.NextEvent(NSEventMask.AnyEvent, NSDate.DistantPast, NSRunLoopMode.Default, true);
					if (ev != null)
					{
						app.SendEvent(ev);
						app.UpdateWindows();
					}
				} while (ev != null);
				
				// 2. Paint windows directly (like C++ PaintWindows())
				viewController.RenderFrame();
				
				// Small yield to prevent 100% CPU (can be removed for max FPS)
				System.Threading.Thread.Yield();
			}
			
			app.Terminate(this);
		}
	}
}
