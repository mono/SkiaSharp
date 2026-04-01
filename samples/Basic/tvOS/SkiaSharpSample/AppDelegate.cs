namespace SkiaSharpSample;

[Register(nameof(AppDelegate))]
public class AppDelegate : UIApplicationDelegate
{
	public static SamplePage DefaultPage { get; set; } = SamplePage.Cpu;

	public override UIWindow? Window { get; set; }

	public override bool FinishedLaunching(UIApplication application, NSDictionary? launchOptions)
	{
		UITabBar.Appearance.BackgroundColor = UIColor.Clear;

		if (Window?.RootViewController is UITabBarController tabBarController)
			tabBarController.SelectedIndex = (int)DefaultPage;

		return true;
	}
}
