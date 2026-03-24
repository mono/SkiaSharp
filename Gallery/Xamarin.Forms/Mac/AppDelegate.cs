using AppKit;
using Foundation;
using Xamarin.Forms;
using Xamarin.Forms.Platform.MacOS;

namespace SkiaSharpSample.Platform
{
	[Register("AppDelegate")]
	public class AppDelegate : FormsApplicationDelegate
	{
		private NSWindow window;

		public AppDelegate()
		{
			var style = NSWindowStyle.Closable | NSWindowStyle.Resizable | NSWindowStyle.Titled;
			var rect = new CoreGraphics.CGRect(200, 1000, 1024, 768);

			window = new NSWindow(rect, style, NSBackingStore.Buffered, false);
			window.TitleVisibility = NSWindowTitleVisibility.Hidden;
		}

		public override NSWindow MainWindow => window;

		public override bool ApplicationShouldTerminateAfterLastWindowClosed(NSApplication sender) => true;

		public override void DidFinishLaunching(NSNotification notification)
		{
			Forms.Init();

			SamplesInitializer.Init();

			LoadApplication(new App());

			base.DidFinishLaunching(notification);
		}
	}
}
