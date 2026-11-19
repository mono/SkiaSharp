using ObjCRuntime;

namespace SkiaSharpSample;

[Register("GpuViewController")]
public class GpuViewController : UIViewController
{
	/// <summary>
	/// Change this to start the GPU page on a different backend.
	/// </summary>
	public static GpuBackend DefaultGpuBackend
	{
		get => AppDelegate.DefaultGpuBackend;
		set => AppDelegate.DefaultGpuBackend = value;
	}

	private UIViewController? currentChild;

	[Outlet("containerView")]
	UIView containerView { get; set; } = null!;

	[Outlet("backendControl")]
	UISegmentedControl backendControl { get; set; } = null!;

	public GpuViewController(NativeHandle handle) : base(handle) { }

	public override void ViewDidLoad()
	{
		base.ViewDidLoad();

		var backends = Enum.GetValues<GpuBackend>();
		backendControl.RemoveAllSegments();
		for (int i = 0; i < backends.Length; i++)
			backendControl.InsertSegment(backends[i].ToString(), i, false);

		backendControl.SelectedSegment = (nint)(int)DefaultGpuBackend;
		backendControl.ValueChanged += OnBackendChanged;

		ShowBackend(DefaultGpuBackend);
	}

	private void OnBackendChanged(object? sender, EventArgs e)
	{
		var backend = (GpuBackend)(int)backendControl.SelectedSegment;
		ShowBackend(backend);
	}

	private void ShowBackend(GpuBackend backend)
	{
		if (currentChild != null)
		{
			currentChild.WillMoveToParentViewController(null);
			currentChild.View?.RemoveFromSuperview();
			currentChild.RemoveFromParentViewController();
			currentChild.Dispose();
			currentChild = null;
		}

		var identifier = backend switch
		{
			GpuBackend.Metal => "GpuMetalViewController",
			GpuBackend.GL => "GpuGLViewController",
			_ => "GpuMetalViewController",
		};

		var child = Storyboard!.InstantiateViewController(identifier);
		AddChildViewController(child);
		child.View!.TranslatesAutoresizingMaskIntoConstraints = false;
		containerView.AddSubview(child.View);
		NSLayoutConstraint.ActivateConstraints(new[]
		{
			child.View.TopAnchor.ConstraintEqualTo(containerView.TopAnchor),
			child.View.BottomAnchor.ConstraintEqualTo(containerView.BottomAnchor),
			child.View.LeadingAnchor.ConstraintEqualTo(containerView.LeadingAnchor),
			child.View.TrailingAnchor.ConstraintEqualTo(containerView.TrailingAnchor),
		});
		child.DidMoveToParentViewController(this);
		currentChild = child;
	}
}
