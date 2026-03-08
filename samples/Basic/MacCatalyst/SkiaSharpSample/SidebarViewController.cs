namespace SkiaSharpSample;

[Register("SidebarViewController")]
public class SidebarViewController : UITableViewController
{
	readonly (string StoryboardId, string Title)[] pages =
	{
		("CpuViewController", "CPU Canvas"),
		("GpuMetalViewController", "GPU (Metal)"),
		("DrawingViewController", "Drawing"),
	};

	const string CellId = "SidebarCell";

	public SidebarViewController(IntPtr handle)
		: base(handle)
	{
	}

	public override void ViewDidLoad()
	{
		base.ViewDidLoad();
		TableView.RegisterClassForCellReuse(typeof(UITableViewCell), CellId);

		// Select the default page
		var defaultIndex = (int)AppDelegate.DefaultPage;
		TableView.SelectRow(
			NSIndexPath.FromRowSection(defaultIndex, 0),
			false,
			UITableViewScrollPosition.None);

		// Navigate to the default page
		if (defaultIndex != 0)
			RowSelected(TableView, NSIndexPath.FromRowSection(defaultIndex, 0));
	}

	public override nint RowsInSection(UITableView tableView, nint section) => pages.Length;

	public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
	{
		var cell = tableView.DequeueReusableCell(CellId, indexPath);

		var page = pages[indexPath.Row];
		var config = UIListContentConfiguration.SidebarCellConfiguration;
		config.Text = page.Title;

		var imageName = page.StoryboardId switch
		{
			"CpuViewController" => "cpu",
			"GpuMetalViewController" => "square.stack.3d.up",
			"DrawingViewController" => "pencil.tip",
			_ => "circle"
		};
		config.Image = UIImage.GetSystemImage(imageName);
		cell.ContentConfiguration = config;

		return cell;
	}

	public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
	{
		var page = pages[indexPath.Row];
		var vc = Storyboard!.InstantiateViewController(page.StoryboardId);
		var navVC = new UINavigationController(vc);

		if (SplitViewController is UISplitViewController splitVC)
			splitVC.SetViewController(navVC, UISplitViewControllerColumn.Secondary);
	}
}
