namespace SkiaSharpSample;

[Register(nameof(AppDelegate))]
public class AppDelegate : UIApplicationDelegate
{
	public override UIWindow? Window { get; set; }

	public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
	{
		// The storyboard creates the UISplitViewController automatically via UIMainStoryboardFile.
		// Configure display mode so the sidebar stays visible on wide screens.
		if (Window?.RootViewController is UISplitViewController splitVC)
			splitVC.PreferredDisplayMode = UISplitViewControllerDisplayMode.OneBesideSecondary;

		return true;
	}
}
