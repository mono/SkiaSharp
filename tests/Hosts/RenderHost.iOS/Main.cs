using UIKit;

namespace SkiaSharp.Tests.RenderHost.iOS;

public class Application
{
	public static void Main (string[] args)
	{
		UIApplication.Main (args, null, typeof (AppDelegate));
	}
}
