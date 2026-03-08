using AppKit;

namespace SkiaSharpSample
{
	static class MainClass
	{
		static void Main(string[] args)
		{
			NSApplication.Init();
			var app = NSApplication.SharedApplication;
			app.ActivationPolicy = NSApplicationActivationPolicy.Regular;
			app.Delegate = new AppDelegate();
			app.Run();
		}
	}
}
