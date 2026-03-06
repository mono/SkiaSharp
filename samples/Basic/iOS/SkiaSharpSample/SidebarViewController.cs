namespace SkiaSharpSample;

public class SidebarViewController : UITableViewController
{
	static readonly (string Id, string Title)[] pages =
	{
		("cpu", "CPU Canvas"),
		("gpu-gl", "GPU (OpenGL)"),
		("gpu-metal", "GPU (Metal)"),
		("drawing", "Drawing"),
	};

	public override void ViewDidLoad()
	{
		base.ViewDidLoad();
		Title = "SkiaSharp";
		TableView.RegisterClassForCellReuse(typeof(UITableViewCell), "cell");
	}

	public override nint RowsInSection(UITableView tableView, nint section) => pages.Length;

	public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
	{
		var cell = tableView.DequeueReusableCell("cell", indexPath);
		cell.TextLabel!.Text = pages[indexPath.Row].Title;
		cell.Accessory = UITableViewCellAccessory.DisclosureIndicator;
		return cell;
	}

	public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
	{
		tableView.DeselectRow(indexPath, true);

		UIViewController vc = pages[indexPath.Row].Id switch
		{
			"cpu" => new CpuViewController(),
			"gpu-gl" => new GpuGLViewController(),
			"gpu-metal" => new GpuMetalViewController(),
			"drawing" => new DrawingViewController(),
			_ => new CpuViewController(),
		};

		if (SplitViewController is UISplitViewController splitVC)
		{
			var navVC = new UINavigationController(vc);
			splitVC.SetViewController(navVC, UISplitViewControllerColumn.Secondary);
			splitVC.Show(UISplitViewControllerColumn.Secondary);
		}
	}
}
