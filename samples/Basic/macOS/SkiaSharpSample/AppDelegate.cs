using AppKit;
using Foundation;

namespace SkiaSharpSample;

[Register(nameof(AppDelegate))]
public class AppDelegate : NSApplicationDelegate
{
	public static SamplePage DefaultPage { get; set; } = SamplePage.Cpu;

	public override bool ApplicationShouldTerminateAfterLastWindowClosed(NSApplication sender) => true;
}
