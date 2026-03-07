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
			// Create menu bar
			var mainMenu = new NSMenu();

			var appMenuItem = new NSMenuItem();
			mainMenu.AddItem(appMenuItem);
			var appMenu = new NSMenu();
			appMenu.AddItem(new NSMenuItem("About SkiaSharp", new ObjCRuntime.Selector("orderFrontStandardAboutPanel:"), ""));
			appMenu.AddItem(NSMenuItem.SeparatorItem);
			appMenu.AddItem(new NSMenuItem("Quit SkiaSharp", new ObjCRuntime.Selector("terminate:"), "q"));
			appMenuItem.Submenu = appMenu;

			var windowMenuItem = new NSMenuItem();
			mainMenu.AddItem(windowMenuItem);
			var windowMenu = new NSMenu("Window");
			windowMenu.AddItem(new NSMenuItem("Minimize", new ObjCRuntime.Selector("performMiniaturize:"), "m"));
			windowMenu.AddItem(new NSMenuItem("Zoom", new ObjCRuntime.Selector("performZoom:"), ""));
			windowMenuItem.Submenu = windowMenu;

			NSApplication.SharedApplication.MainMenu = mainMenu;
			NSApplication.SharedApplication.WindowsMenu = windowMenu;

			// Create window with split view
			var style = NSWindowStyle.Titled | NSWindowStyle.Closable | NSWindowStyle.Miniaturizable | NSWindowStyle.Resizable;
			window = new NSWindow(new CGRect(196, 240, 1024, 768), style, NSBackingStore.Buffered, false);
			window.Title = "SkiaSharp on macOS";

			// Create split view manually
			var splitView = new NSSplitView(window.ContentView!.Bounds)
			{
				IsVertical = true,
				DividerStyle = NSSplitViewDividerStyle.Thin,
				AutoresizingMask = NSViewResizingMask.WidthSizable | NSViewResizingMask.HeightSizable,
			};

			var sidebar = new SidebarViewController();
			var sidebarView = sidebar.View; // triggers LoadView() + ViewDidLoad()
			sidebarView.Frame = new CGRect(0, 0, 200, 768);

			var content = new CpuViewController();
			var contentView = content.View; // triggers LoadView() + ViewDidLoad()
			contentView.Frame = new CGRect(200, 0, 824, 768);

			splitView.AddSubview(sidebarView);
			splitView.AddSubview(contentView);
			splitView.SetPositionOfDivider(200, 0);

			window.ContentView = splitView;

			// Store references for sidebar page switching
			sidebar.OnPageChanged = (vc) =>
			{
				var newView = vc.View; // triggers LoadView() + ViewDidLoad()
				newView.Frame = splitView.Subviews[1].Frame;
				splitView.Subviews[1].RemoveFromSuperview();
				splitView.AddSubview(newView);
				splitView.AdjustSubviews();
			};

			window.Center();
			window.MakeKeyAndOrderFront(this);
			NSApplication.SharedApplication.ActivateIgnoringOtherApps(true);
		}

		public override bool ApplicationShouldTerminateAfterLastWindowClosed(NSApplication sender) => true;
	}
}
