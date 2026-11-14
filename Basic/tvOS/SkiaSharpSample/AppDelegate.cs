namespace SkiaSharpSample;

[Register(nameof(AppDelegate))]
public class AppDelegate : UIApplicationDelegate
{
	public static SamplePage DefaultPage { get; set; } = SamplePage.Cpu;

	public override bool FinishedLaunching(UIApplication application, NSDictionary? launchOptions)
	{
		UITabBar.Appearance.BackgroundColor = UIColor.Clear;

		return true;
	}

	[Export("application:configurationForConnectingSceneSession:options:")]
	public override UISceneConfiguration GetConfiguration(UIApplication application, UISceneSession connectingSceneSession, UISceneConnectionOptions options)
	{
		return new UISceneConfiguration("Default Configuration", connectingSceneSession.Role);
	}
}
