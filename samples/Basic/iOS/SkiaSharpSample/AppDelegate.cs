namespace SkiaSharpSample;

[Register(nameof(AppDelegate))]
public class AppDelegate : UIApplicationDelegate
{
	public override UIWindow? Window { get; set; }

	public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
	{
		// Support selecting a tab via launch argument: -page Cpu|GpuGL|GpuMetal|Drawing
		var args = NSProcessInfo.ProcessInfo.Arguments;
		for (int i = 0; i < args.Length - 1; i++)
		{
			if (args[i] == "-page" && Enum.TryParse<SamplePage>(args[i + 1], true, out var page))
			{
				// Defer so the storyboard tab bar controller is fully loaded
				NSRunLoop.Main.BeginInvokeOnMainThread(() =>
				{
					if (Window?.RootViewController is UITabBarController tabs)
						tabs.SelectedIndex = (nint)(int)page;
				});
				break;
			}
		}
		return true;
	}
}
