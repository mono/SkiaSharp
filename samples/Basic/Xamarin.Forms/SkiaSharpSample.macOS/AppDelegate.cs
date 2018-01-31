using AppKit;
using Foundation;
using Xamarin.Forms;
using Xamarin.Forms.Platform.MacOS;

namespace SkiaSharpSample.macOS
{
	[Register(nameof(AppDelegate))]
	public class AppDelegate : FormsApplicationDelegate
	{
		private NSWindow window;

		public AppDelegate()
		{
			var style = NSWindowStyle.Closable | NSWindowStyle.Resizable | NSWindowStyle.Titled;

			var screenSize = NSScreen.MainScreen.VisibleFrame;
			var w = 640;
			var h = 480;
			var rect = new CoreGraphics.CGRect((screenSize.Width - w) / 2, (screenSize.Height - h) / 2, w, h);

			window = new NSWindow(rect, style, NSBackingStore.Buffered, false);
			window.Title = "SkiaSharpSample.macOS";
			window.TitleVisibility = NSWindowTitleVisibility.Hidden;
		}

		public override NSWindow MainWindow => window;

		public override bool ApplicationShouldTerminateAfterLastWindowClosed(NSApplication sender) => true;

		public override void DidFinishLaunching(NSNotification notification)
		{
			Forms.Init();

			LoadApplication(new App());

			base.DidFinishLaunching(notification);
		}
	}
}
