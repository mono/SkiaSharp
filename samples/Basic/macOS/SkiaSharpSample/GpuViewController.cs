using AppKit;
using ObjCRuntime;

namespace SkiaSharpSample;

[Register("GpuViewController")]
public class GpuViewController : NSTabViewController
{
	/// <summary>
	/// Change this to start the GPU page on a different backend.
	/// </summary>
	public static GpuBackend DefaultGpuBackend
	{
		get => AppDelegate.DefaultGpuBackend;
		set => AppDelegate.DefaultGpuBackend = value;
	}

	public GpuViewController(NativeHandle handle) : base(handle) { }

	public override void ViewDidLoad()
	{
		base.ViewDidLoad();
		SelectedTabViewItemIndex = (nint)(int)DefaultGpuBackend;
	}
}
