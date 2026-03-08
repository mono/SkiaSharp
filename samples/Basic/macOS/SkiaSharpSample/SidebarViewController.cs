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

		NSTableView? tableView;

		public Action<NSViewController>? OnPageChanged { get; set; }

		public override void LoadView()
		{
			tableView = new NSTableView
			{
				AllowsTypeSelect = false,
			};
			tableView.AddColumn(new NSTableColumn("Title"));

			View = new NSScrollView
			{
				HasVerticalScroller = true,
				BorderType = NSBorderType.NoBorder,
				DocumentView = tableView,
			};
		}

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
			NSViewController vc = pageId switch
			{
				"cpu" => new CpuViewController(),
				"gpu-gl" => new GpuGLViewController(),
				"gpu-metal" => new GpuMetalViewController(),
				"drawing" => new DrawingViewController(),
				_ => new CpuViewController()
			};

			OnPageChanged?.Invoke(vc);
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
