using System;
using AppKit;
using Foundation;

namespace SkiaSharpSample
{
	public class SidebarViewController : NSViewController
	{
		public event Action<string>? PageSelected;

		static readonly (string Id, string Title)[] pages =
		{
			("cpu", "CPU Canvas"),
			("gpu-gl", "GPU (OpenGL)"),
			("gpu-metal", "GPU (Metal)"),
			("drawing", "Drawing"),
		};

		NSTableView? tableView;

		public override void LoadView()
		{
			tableView = new NSTableView
			{
				Style = NSTableViewStyle.SourceList,
			};

			var column = new NSTableColumn("Title") { Title = "Pages" };
			tableView.AddColumn(column);
			tableView.HeaderView = null;

			tableView.DataSource = new SidebarDataSource();
			tableView.Delegate = new SidebarDelegate(id => PageSelected?.Invoke(id));

			var scrollView = new NSScrollView
			{
				DocumentView = tableView,
				HasVerticalScroller = true,
			};
			View = scrollView;

			tableView.SelectRow(0, false);
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
