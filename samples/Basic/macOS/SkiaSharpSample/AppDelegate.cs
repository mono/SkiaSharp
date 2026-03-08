using AppKit;
using CoreGraphics;
using Foundation;
using SkiaSharp.Views.Mac;

namespace SkiaSharpSample
{
	[Register(nameof(AppDelegate))]
	public class AppDelegate : NSApplicationDelegate
	{
		NSWindow? window;

		public override void DidFinishLaunching(NSNotification notification)
		{
			// The menu bar is loaded from Main.storyboard via NSMainStoryboardFile.
			// Create the window programmatically for the SkiaSharp content.
			var style = NSWindowStyle.Titled | NSWindowStyle.Closable | NSWindowStyle.Miniaturizable | NSWindowStyle.Resizable;
			window = new NSWindow(new CGRect(196, 240, 1024, 768), style, NSBackingStore.Buffered, false)
			{
				Title = "SkiaSharp on macOS",
				MinSize = new CGSize(600, 400),
			};

			var splitView = new NSSplitView(window.ContentView!.Bounds)
			{
				IsVertical = true,
				DividerStyle = NSSplitViewDividerStyle.Thin,
				AutoresizingMask = NSViewResizingMask.WidthSizable | NSViewResizingMask.HeightSizable,
			};

			var sidebar = new SidebarViewController();
			var sidebarView = sidebar.View;
			sidebarView.Frame = new CGRect(0, 0, 200, window.ContentView.Bounds.Height);

			var content = new CpuViewController();
			var contentView = content.View;
			contentView.Frame = new CGRect(200, 0, window.ContentView.Bounds.Width - 200, window.ContentView.Bounds.Height);

			splitView.AddSubview(sidebarView);
			splitView.AddSubview(contentView);
			splitView.SetPositionOfDivider(200, 0);

			window.ContentView = splitView;

			sidebar.OnPageChanged = (vc) =>
			{
				var newView = vc.View;
				newView.Frame = splitView.Subviews[1].Frame;
				splitView.Subviews[1].RemoveFromSuperview();
				splitView.AddSubview(newView);
				splitView.AdjustSubviews();
			};

			window.Center();
			window.MakeKeyAndOrderFront(this);
		}

		public override bool ApplicationShouldTerminateAfterLastWindowClosed(NSApplication sender) => true;
	}
}
