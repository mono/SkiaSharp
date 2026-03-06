using System;
using AppKit;
using Foundation;

namespace SkiaSharpSample
{
	[Register("SidebarViewController")]
	public class SidebarViewController : NSViewController
	{
		static readonly (string Id, string Title)[] pages =
		{
			("cpu", "CPU Canvas"),
			("gpu-gl", "GPU (OpenGL)"),
			("gpu-metal", "GPU (Metal)"),
			("drawing", "Drawing"),
		};

		[Outlet]
		NSTableView? tableView { get; set; }

		public SidebarViewController(IntPtr handle) : base(handle) { }

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			if (tableView != null)
			{
				tableView.HeaderView = null;
				tableView.DataSource = new SidebarDataSource();
				tableView.Delegate = new SidebarDelegate(OnPageSelected);
				tableView.SelectRow(0, false);
			}
		}

		void OnPageSelected(string pageId)
		{
			if (ParentViewController is not NSSplitViewController splitVC || Storyboard == null)
				return;

			var identifier = pageId switch
			{
				"cpu" => "CpuVC",
				"gpu-gl" => "GpuGLVC",
				"gpu-metal" => "GpuMetalVC",
				"drawing" => "DrawingVC",
				_ => "CpuVC"
			};

			var vc = Storyboard.InstantiateControllerWithIdentifier(identifier) as NSViewController;
			if (vc != null && splitVC.SplitViewItems.Length > 1)
				splitVC.SplitViewItems[1].ViewController = vc;
		}

		class SidebarDataSource : NSTableViewDataSource
		{
			public override nint GetRowCount(NSTableView tableView) => pages.Length;
		}

		class SidebarDelegate : NSTableViewDelegate
		{
			readonly Action<string> onSelect;

			public SidebarDelegate(Action<string> onSelect) => this.onSelect = onSelect;

			public override NSView GetViewForItem(NSTableView tableView, NSTableColumn? tableColumn, nint row)
			{
				var textField = (NSTextField?)tableView.MakeView("SidebarCell", this);
				if (textField == null)
				{
					textField = new NSTextField
					{
						Identifier = "SidebarCell",
						Editable = false,
						Bordered = false,
						BackgroundColor = NSColor.Clear,
						DrawsBackground = false,
					};
				}
				textField.StringValue = pages[row].Title;
				return textField;
			}

			public override void SelectionDidChange(NSNotification notification)
			{
				if (notification.Object is NSTableView tv && tv.SelectedRow >= 0)
					onSelect(pages[tv.SelectedRow].Id);
			}
		}
	}
}
