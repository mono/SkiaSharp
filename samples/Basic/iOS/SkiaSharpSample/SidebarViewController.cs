namespace SkiaSharpSample;

[Register("SidebarViewController")]
public class SidebarViewController : UITableViewController
{
	static readonly (string StoryboardId, string Title)[] pages =
	{
		("CpuVC", "CPU Canvas"),
		("GpuGLVC", "GPU (OpenGL)"),
		("GpuMetalVC", "GPU (Metal)"),
		("DrawingVC", "Drawing"),
	};

	public SidebarViewController(IntPtr handle) : base(handle) { }

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

		var vc = Storyboard!.InstantiateViewController(pages[indexPath.Row].StoryboardId);
		var navVC = new UINavigationController(vc);
		ShowDetailViewController(navVC, this);
	}
}
