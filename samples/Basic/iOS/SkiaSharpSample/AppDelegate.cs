namespace SkiaSharpSample;

[Register(nameof(AppDelegate))]
public class AppDelegate : UIApplicationDelegate
{
	/// <summary>
	/// Change this to start the app on a different page.
	/// </summary>
	public static SamplePage DefaultPage { get; set; } = SamplePage.Cpu;

	[Export("application:configurationForConnectingSceneSession:options:")]
	public override UISceneConfiguration GetConfiguration(UIApplication application, UISceneSession connectingSceneSession, UISceneConnectionOptions options)
	{
		return new UISceneConfiguration("Default Configuration", connectingSceneSession.Role);
	}
}
