namespace SkiaSharpSample;

[Register(nameof(AppDelegate))]
public class AppDelegate : UIApplicationDelegate
{
	public override UIWindow? Window { get; set; }

	public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
	{
		Window = new UIWindow(UIScreen.MainScreen.Bounds);

		var splitVC = new UISplitViewController(UISplitViewControllerStyle.DoubleColumn);
		splitVC.PreferredDisplayMode = UISplitViewControllerDisplayMode.OneBesideSecondary;
		splitVC.PreferredSplitBehavior = UISplitViewControllerSplitBehavior.Tile;

		var sidebarVC = new SidebarViewController();
		splitVC.SetViewController(sidebarVC, UISplitViewControllerColumn.Primary);

		var cpuVC = new CpuViewController();
		var navVC = new UINavigationController(cpuVC);
		splitVC.SetViewController(navVC, UISplitViewControllerColumn.Secondary);

		// On compact width (iPhone), show the canvas first
		splitVC.Show(UISplitViewControllerColumn.Secondary);

		Window.RootViewController = splitVC;
		Window.MakeKeyAndVisible();
		return true;
	}
}
