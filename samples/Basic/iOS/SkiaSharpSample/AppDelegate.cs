namespace SkiaSharpSample;

[Register(nameof(AppDelegate))]
public class AppDelegate : UIApplicationDelegate
{
	/// <summary>
	/// Change this to start the app on a different page.
	/// </summary>
	public static SamplePage DefaultPage { get; set; } = SamplePage.Cpu;

	public override UIWindow? Window { get; set; }

	public override bool FinishedLaunching(UIApplication application, NSDictionary? launchOptions)
	{
		if (DefaultPage != SamplePage.Cpu)
		{
			NSRunLoop.Main.BeginInvokeOnMainThread(() =>
			{
				if (Window?.RootViewController is UITabBarController tabs)
					tabs.SelectedIndex = (nint)(int)DefaultPage;
			});
		}
		return true;
	}
}
