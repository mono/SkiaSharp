namespace SkiaSharpSample;

[Register("SceneDelegate")]
public class SceneDelegate : UIResponder, IUIWindowSceneDelegate
{
	[Export("window")]
	public UIWindow? Window { get; set; }

	[Export("scene:willConnectToSession:options:")]
	public void WillConnect(UIScene scene, UISceneSession session, UISceneConnectionOptions options)
	{
		if (AppDelegate.DefaultPage != SamplePage.Cpu &&
			Window?.RootViewController is UITabBarController tabs)
		{
			tabs.SelectedIndex = (nint)(int)AppDelegate.DefaultPage;
		}
	}
}
