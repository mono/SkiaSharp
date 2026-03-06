namespace SkiaSharpSample;

[Register("AppDelegate")]
public class AppDelegate : UIApplicationDelegate
{
	[Export("application:configurationForConnectingSceneSession:options:")]
	public UISceneConfiguration GetConfiguration(UIApplication application, UISceneSession connectingSceneSession, UISceneConnectionOptions options)
	{
		var config = new UISceneConfiguration("Default Configuration", connectingSceneSession.Role);
		config.DelegateType = typeof(SceneDelegate);
		return config;
	}
}
