namespace SkiaSharpSample;

[Register(nameof(AppDelegate))]
public class AppDelegate : UIApplicationDelegate
{
	public static SamplePage DefaultPage { get; set; } = SamplePage.Cpu;

	public override UIWindow? Window { get; set; }

	public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
	{
		Window = new UIWindow(UIScreen.MainScreen.Bounds);

		var splitVC = new UISplitViewController(UISplitViewControllerStyle.DoubleColumn);
		splitVC.PreferredDisplayMode = UISplitViewControllerDisplayMode.OneBesideSecondary;
		splitVC.PreferredPrimaryColumnWidthFraction = (nfloat)0.3;

		var sidebar = new SidebarViewController(splitVC);
		splitVC.SetViewController(new UINavigationController(sidebar), UISplitViewControllerColumn.Primary);

		UIViewController initialVC = DefaultPage switch
		{
			SamplePage.GpuGL => new GpuGLViewController(),
			SamplePage.GpuMetal => new GpuMetalViewController(),
			SamplePage.Drawing => new DrawingViewController(),
			_ => new CpuViewController(),
		};
		splitVC.SetViewController(new UINavigationController(initialVC), UISplitViewControllerColumn.Secondary);

		Window.RootViewController = splitVC;
		Window.MakeKeyAndVisible();

		return true;
	}
}
