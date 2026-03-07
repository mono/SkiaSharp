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
		splitVC.PreferredPrimaryColumnWidthFraction = (nfloat)0.3;

		var sidebar = new SidebarViewController(splitVC);
		splitVC.SetViewController(new UINavigationController(sidebar), UISplitViewControllerColumn.Primary);

		splitVC.SetViewController(new UINavigationController(new CpuViewController()), UISplitViewControllerColumn.Secondary);

		Window.RootViewController = splitVC;
		Window.MakeKeyAndVisible();

		return true;
	}
}
