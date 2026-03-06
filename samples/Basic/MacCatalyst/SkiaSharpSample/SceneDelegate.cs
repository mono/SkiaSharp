namespace SkiaSharpSample;

[Register("SceneDelegate")]
public class SceneDelegate : UIResponder, IUIWindowSceneDelegate
{
	[Export("window")]
	public UIWindow? Window { get; set; }

	[Export("scene:willConnectToSession:options:")]
	public void WillConnect(UIScene scene, UISceneSession session, UISceneConnectionOptions options)
	{
		// The storyboard creates the window and root view controller automatically.
		// Configure split view controller display properties.
		if (Window?.RootViewController is UISplitViewController splitVC)
		{
			splitVC.PreferredDisplayMode = UISplitViewControllerDisplayMode.OneBesideSecondary;
			splitVC.PrimaryBackgroundStyle = UISplitViewControllerBackgroundStyle.Sidebar;
		}
	}
}
