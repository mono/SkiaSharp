namespace SkiaSharpSample;

public class SidebarViewController : UITableViewController
{
	readonly (string Id, string Title)[] pages =
	{
		("cpu", "CPU Canvas"),
		("gpu-metal", "GPU (Metal)"),
		("drawing", "Drawing"),
	};

	const string CellId = "SidebarCell";

	public SidebarViewController()
		: base(UITableViewStyle.InsetGrouped)
	{
		Title = "SkiaSharp";
	}

	public override void ViewDidLoad()
	{
		base.ViewDidLoad();
		TableView.RegisterClassForCellReuse(typeof(UITableViewCell), CellId);

		// Select the first row by default
		TableView.SelectRow(
			NSIndexPath.FromRowSection(0, 0),
			false,
			UITableViewScrollPosition.None);
	}

	public override nint RowsInSection(UITableView tableView, nint section) => pages.Length;

	public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
	{
		var cell = tableView.DequeueReusableCell(CellId, indexPath);

		var page = pages[indexPath.Row];
		var config = UIListContentConfiguration.SidebarCellConfiguration;
		config.Text = page.Title;

		var imageName = page.Id switch
		{
			"cpu" => "cpu",
			"gpu-metal" => "gpu",
			"drawing" => "pencil.tip",
			_ => "circle"
		};
		config.Image = UIImage.GetSystemImage(imageName);
		cell.ContentConfiguration = config;

		return cell;
	}

	public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
	{
		var page = pages[indexPath.Row];
		UIViewController vc = page.Id switch
		{
			"cpu" => new CpuViewController(),
			"gpu-metal" => new GpuMetalViewController(),
			"drawing" => new DrawingViewController(),
			_ => new CpuViewController()
		};

		var navVC = new UINavigationController(vc);

		if (SplitViewController is UISplitViewController splitVC)
			splitVC.SetViewController(navVC, UISplitViewControllerColumn.Secondary);
	}
}
