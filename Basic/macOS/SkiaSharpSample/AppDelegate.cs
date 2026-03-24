using AppKit;
using Foundation;

namespace SkiaSharpSample
{
	[Register(nameof(AppDelegate))]
	public class AppDelegate : NSApplicationDelegate
	{
		public override void DidFinishLaunching(NSNotification notification)
		{
			// Insert code here to initialize your application
		}

		public override void WillTerminate(NSNotification notification)
		{
			// Insert code here to tear down your application
		}

		public override bool ApplicationShouldTerminateAfterLastWindowClosed(NSApplication sender) => true;
	}
}
