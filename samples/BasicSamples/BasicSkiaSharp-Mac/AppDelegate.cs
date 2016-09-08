using AppKit;
using Foundation;

namespace BasicSkiaSharp
{
	[Register("AppDelegate")]
	public class AppDelegate : NSApplicationDelegate
	{
		MainWindowController mainWindowController;

		public override void DidFinishLaunching(NSNotification notification)
		{
			mainWindowController = new MainWindowController();
			mainWindowController.Window.MakeKeyAndOrderFront(this);
		}

		public override bool ApplicationShouldTerminateAfterLastWindowClosed(NSApplication sender) => true;
	}
}
