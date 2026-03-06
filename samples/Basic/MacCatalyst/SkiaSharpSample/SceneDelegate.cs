namespace SkiaSharpSample;

[Register("SceneDelegate")]
public class SceneDelegate : UIResponder, IUIWindowSceneDelegate
{
	[Export("window")]
	public UIWindow? Window { get; set; }

	[Export("scene:willConnectToSession:options:")]
	public void WillConnect(UIScene scene, UISceneSession session, UISceneConnectionOptions options)
	{
		if (scene is not UIWindowScene windowScene)
			return;

		Window = new UIWindow(windowScene);

		var splitVC = new UISplitViewController(UISplitViewControllerStyle.DoubleColumn);
		splitVC.PreferredDisplayMode = UISplitViewControllerDisplayMode.OneBesideSecondary;
		splitVC.PrimaryBackgroundStyle = UISplitViewControllerBackgroundStyle.Sidebar;

		var sidebarVC = new SidebarViewController();
		splitVC.SetViewController(sidebarVC, UISplitViewControllerColumn.Primary);

		var cpuVC = new CpuViewController();
		var navVC = new UINavigationController(cpuVC);
		splitVC.SetViewController(navVC, UISplitViewControllerColumn.Secondary);

		Window.RootViewController = splitVC;
		Window.MakeKeyAndVisible();
	}
}
