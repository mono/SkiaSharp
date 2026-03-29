using AppKit;
using ObjCRuntime;

namespace SkiaSharpSample;

[Register("MainTabViewController")]
public class MainTabViewController : NSTabViewController
{
	public MainTabViewController(NativeHandle handle) : base(handle) { }

	public override void ViewDidLoad()
	{
		base.ViewDidLoad();
		SelectedTabViewItemIndex = (nint)(int)AppDelegate.DefaultPage;
	}
}
