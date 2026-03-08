namespace SkiaSharpSample;

[Register("AppDelegate")]
public class AppDelegate : UIApplicationDelegate
{
	[Export("application:configurationForConnectingSceneSession:options:")]
	public UISceneConfiguration GetConfiguration(UIApplication application, UISceneSession connectingSceneSession, UISceneConnectionOptions options)
	{
		return new UISceneConfiguration("Default Configuration", connectingSceneSession.Role);
	}
}
