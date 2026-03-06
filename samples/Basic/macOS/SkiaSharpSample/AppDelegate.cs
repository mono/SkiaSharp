using System;
using AppKit;
using CoreGraphics;
using Foundation;
using ObjCRuntime;

namespace SkiaSharpSample
{
	[Register(nameof(AppDelegate))]
	public class AppDelegate : NSApplicationDelegate
	{
		NSWindow? window;
		NSSplitViewController? splitVC;

		public override void DidFinishLaunching(NSNotification notification)
		{
			SetupMainMenu();

			window = new NSWindow(
				new CGRect(100, 100, 1024, 768),
				NSWindowStyle.Titled | NSWindowStyle.Closable | NSWindowStyle.Resizable | NSWindowStyle.Miniaturizable,
				NSBackingStore.Buffered, false)
			{
				Title = "SkiaSharp on macOS"
			};

			splitVC = new NSSplitViewController();

			var sidebarVC = new SidebarViewController();
			sidebarVC.PageSelected += OnPageSelected;
			var sidebarItem = NSSplitViewItem.CreateSidebar(sidebarVC);
			sidebarItem.MinimumThickness = 200;
			splitVC.AddSplitViewItem(sidebarItem);

			var cpuVC = new CpuViewController();
			var contentItem = NSSplitViewItem.CreateContentList(cpuVC);
			splitVC.AddSplitViewItem(contentItem);

			window.ContentViewController = splitVC;
			window.Center();
			window.MakeKeyAndOrderFront(this);
		}

		void SetupMainMenu()
		{
			var menuBar = new NSMenu();

			var appMenuItem = new NSMenuItem();
			menuBar.AddItem(appMenuItem);
			var appMenu = new NSMenu("SkiaSharp");
			appMenu.AddItem(new NSMenuItem("About SkiaSharp", new Selector("orderFrontStandardAboutPanel:"), ""));
			appMenu.AddItem(NSMenuItem.SeparatorItem);
			appMenu.AddItem(new NSMenuItem("Quit SkiaSharp", new Selector("terminate:"), "q"));
			appMenuItem.Submenu = appMenu;

			var windowMenuItem = new NSMenuItem();
			menuBar.AddItem(windowMenuItem);
			var windowMenu = new NSMenu("Window");
			windowMenu.AddItem(new NSMenuItem("Minimize", new Selector("performMiniaturize:"), "m"));
			windowMenu.AddItem(new NSMenuItem("Zoom", new Selector("performZoom:"), ""));
			windowMenu.AddItem(NSMenuItem.SeparatorItem);
			windowMenu.AddItem(new NSMenuItem("Bring All to Front", new Selector("arrangeInFront:"), ""));
			windowMenuItem.Submenu = windowMenu;
			NSApplication.SharedApplication.WindowsMenu = windowMenu;

			NSApplication.SharedApplication.MainMenu = menuBar;
		}

		void OnPageSelected(string pageId)
		{
			if (splitVC == null) return;

			NSViewController vc = pageId switch
			{
				"cpu" => new CpuViewController(),
				"gpu-gl" => new GpuGLViewController(),
				"gpu-metal" => new GpuMetalViewController(),
				"drawing" => new DrawingViewController(),
				_ => new CpuViewController()
			};

			splitVC.SplitViewItems[1].ViewController = vc;
		}

		public override bool ApplicationShouldTerminateAfterLastWindowClosed(NSApplication sender) => true;
	}
}
