namespace SkiaSharpSample;

[Register("SidebarViewController")]
public class SidebarViewController : UITableViewController
{
	static readonly (Func<UIViewController> Factory, string Title)[] pages =
	{
		(() => new CpuViewController(), "CPU Canvas"),
		(() => new GpuGLViewController(), "GPU (OpenGL)"),
		(() => new GpuMetalViewController(), "GPU (Metal)"),
		(() => new DrawingViewController(), "Drawing"),
	};

	readonly UISplitViewController splitVC;

	public SidebarViewController(UISplitViewController splitVC) : base(UITableViewStyle.InsetGrouped)
	{
		this.splitVC = splitVC;
	}

	public override void ViewDidLoad()
	{
		base.ViewDidLoad();
		Title = "SkiaSharp";
		TableView.RegisterClassForCellReuse(typeof(UITableViewCell), "cell");

		// Auto-select the default page
		TableView.SelectRow(
			NSIndexPath.FromRowSection((int)AppDelegate.DefaultPage, 0),
			false,
			UITableViewScrollPosition.None);
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
		var vc = pages[indexPath.Row].Factory();
		var navVC = new UINavigationController(vc);
		splitVC.SetViewController(navVC, UISplitViewControllerColumn.Secondary);
	}
}
