using AppKit;

namespace SkiaSharpSample;

internal static class Program
{
	[STAThread]
	public static void Main(string[] args)
	{
		NSApplication.Init();

		var screenshotPath = ParseScreenshotPath(args);
		var app = NSApplication.SharedApplication;
		var appDelegate = new AppDelegate(screenshotPath);
		app.Delegate = appDelegate;
		app.Run();
		GC.KeepAlive(appDelegate);
	}

	private static string? ParseScreenshotPath(string[] args)
	{
		if (args.Length == 0)
			return null;
		if (args.Length == 2 && args[0] == "--screenshot")
			return args[1];

		throw new ArgumentException(
			"Usage: SkiaSharpSample [--screenshot <output.png>]");
	}
}
