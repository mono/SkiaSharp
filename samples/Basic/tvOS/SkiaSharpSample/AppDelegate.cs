namespace SkiaSharpSample;

[Register(nameof(AppDelegate))]
public class AppDelegate : UIApplicationDelegate
{
	public override UIWindow? Window { get; set; }

	public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
	{
		Window = new UIWindow(UIScreen.MainScreen.Bounds);

		var tabVC = new UITabBarController();
		tabVC.ViewControllers = new UIViewController[]
		{
			new CpuViewController { TabBarItem = new UITabBarItem("CPU", null, 0) },
			new GpuGLViewController { TabBarItem = new UITabBarItem("GPU (GL)", null, 1) },
			new GpuMetalViewController { TabBarItem = new UITabBarItem("GPU (Metal)", null, 2) },
		};

		Window.RootViewController = tabVC;
		Window.MakeKeyAndVisible();

		return true;
	}
}
