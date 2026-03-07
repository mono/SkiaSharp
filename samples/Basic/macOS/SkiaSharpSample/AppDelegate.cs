using AppKit;
using CoreGraphics;
using Foundation;
using SkiaSharp.Views.Mac;

namespace SkiaSharpSample
{
	[Register(nameof(AppDelegate))]
	public class AppDelegate : NSApplicationDelegate
	{
		public override void DidFinishLaunching(NSNotification notification)
		{
			// The window and menu bar are loaded from Main.storyboard.
			// We just need to add the split view content to the storyboard-created window.
			var window = NSApplication.SharedApplication.KeyWindow
				?? NSApplication.SharedApplication.MainWindow;

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
		}

		public override bool ApplicationShouldTerminateAfterLastWindowClosed(NSApplication sender) => true;
	}
}
