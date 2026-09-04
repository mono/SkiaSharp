using AppKit;
using CoreGraphics;
using Foundation;

namespace SkiaSharpSample;

[Register(nameof(AppDelegate))]
internal sealed class AppDelegate : NSApplicationDelegate
{
	private readonly string? screenshotPath;
	private NSWindow? window;
	private PerformanceViewController? controller;
	private System.Threading.Timer? screenshotTimer;

	public AppDelegate(string? screenshotPath)
	{
		this.screenshotPath = screenshotPath;
	}

	public override void DidFinishLaunching(NSNotification notification)
	{
		controller = new PerformanceViewController();
		window = new NSWindow(
			new CGRect(0, 0, 1440, 900),
			NSWindowStyle.Titled |
			NSWindowStyle.Closable |
			NSWindowStyle.Miniaturizable |
			NSWindowStyle.Resizable,
			NSBackingStore.Buffered,
			deferCreation: false)
		{
			Title = "SkiaSharp Graphite Performance Lab",
			ContentViewController = controller,
			MinSize = new CGSize(980, 640),
			SharingType = NSWindowSharingType.ReadOnly,
		};

		window.Center();
		window.MakeKeyAndOrderFront(null);
		if (OperatingSystem.IsMacOSVersionAtLeast(14))
			NSApplication.SharedApplication.Activate();
		else
			NSApplication.SharedApplication.ActivateIgnoringOtherApps(true);

		if (screenshotPath is not null)
		{
			screenshotTimer = new System.Threading.Timer(
				_ => BeginInvokeOnMainThread(CaptureScreenshot),
				null,
				TimeSpan.FromSeconds(7),
				Timeout.InfiniteTimeSpan);
		}
	}

	public override bool ApplicationShouldTerminateAfterLastWindowClosed(
		NSApplication sender) => true;

	public override void WillTerminate(NSNotification notification)
	{
		screenshotTimer?.Dispose();
		screenshotTimer = null;
		controller?.Dispose();
		controller = null;
		window?.Dispose();
		window = null;
	}

	private void CaptureScreenshot()
	{
		screenshotTimer?.Dispose();
		screenshotTimer = null;

		var appWindow = window
			?? throw new InvalidOperationException("The app window is unavailable.");
#pragma warning disable CA1422 // App-owned window capture for the optional screenshot mode.
		using var screenshot = CGImage.ScreenImage(
			(int)appWindow.WindowNumber,
			CGRect.Null,
			CGWindowListOption.IncludingWindow,
			CGWindowImageOption.BoundsIgnoreFraming)
			?? throw new InvalidOperationException("Unable to capture the app window.");
#pragma warning restore CA1422
		using var image = new NSBitmapImageRep(screenshot);
		using var properties = new NSDictionary();
		using var data = image.RepresentationUsingTypeProperties(
			NSBitmapImageFileType.Png,
			properties)
			?? throw new InvalidOperationException("Unable to encode the app screenshot.");

		var fullPath = Path.GetFullPath(screenshotPath!);
		Directory.CreateDirectory(
			Path.GetDirectoryName(fullPath)
				?? throw new InvalidOperationException("The screenshot path has no directory."));
		File.WriteAllBytes(fullPath, data.ToArray());
		Console.WriteLine(fullPath);
		NSApplication.SharedApplication.Terminate(this);
	}
}
