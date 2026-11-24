namespace SkiaSharpSample;

[Register("AppDelegate")]
public class AppDelegate : UIApplicationDelegate
{
	public static SamplePage DefaultPage { get; set; } = SamplePage.Cpu;

	[Export("application:configurationForConnectingSceneSession:options:")]
	public override UISceneConfiguration GetConfiguration(UIApplication application, UISceneSession connectingSceneSession, UISceneConnectionOptions options)
	{
		return new UISceneConfiguration("Default Configuration", connectingSceneSession.Role);
	}
}
