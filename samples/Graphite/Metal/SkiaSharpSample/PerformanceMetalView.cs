using CoreGraphics;
using Metal;
using MetalKit;

namespace SkiaSharpSample;

internal abstract class PerformanceMetalView : MTKView, IMTKViewDelegate
{
	private readonly FrameMeter frameMeter = new();
	private bool renderFailed;
	private bool isRendering;

	protected PerformanceMetalView(
		CGRect frame,
		RenderSettingsStore settings)
		: base(frame, MTLDevice.SystemDefault)
	{
		Settings = settings;

		if (Device is null)
			throw new PlatformNotSupportedException(
				"Metal is not supported on this Mac.");

		ColorPixelFormat = MTLPixelFormat.BGRA8Unorm;
		DepthStencilPixelFormat = MTLPixelFormat.Invalid;
		SampleCount = 1;
		FramebufferOnly = false;
		EnableSetNeedsDisplay = false;
		Paused = true;
		Delegate = this;
	}

	protected RenderSettingsStore Settings { get; }

	public event EventHandler<RenderMetrics>? MetricsUpdated;

	public event EventHandler<Exception>? RenderFailed;

	public void SetRendering(bool rendering)
	{
		if (renderFailed)
			return;
		if (isRendering == rendering)
			return;

		isRendering = rendering;
		Paused = !rendering;
		if (rendering)
		{
			frameMeter.Reset();
			NeedsDisplay = true;
		}
	}

	public void StopAndDrain()
	{
		isRendering = false;
		Paused = true;
		try
		{
			QuiesceRenderer();
		}
		catch (Exception exception)
		{
			renderFailed = true;
			RenderFailed?.Invoke(this, exception);
		}
	}

	void IMTKViewDelegate.DrawableSizeWillChange(MTKView view, CGSize size)
	{
	}

	void IMTKViewDelegate.Draw(MTKView view)
	{
		if (renderFailed)
			return;

		try
		{
			var settings = Settings.Current;
			PreferredFramesPerSecond = settings.FramesPerSecond;
			if (DrawFrame(settings) is { } measurement &&
				frameMeter.Add(measurement) is { } metrics)
			{
				MetricsUpdated?.Invoke(this, metrics);
			}
		}
		catch (Exception exception)
		{
			renderFailed = true;
			Paused = true;
			RenderFailed?.Invoke(this, exception);
		}
	}

	protected abstract FrameMeasurement? DrawFrame(RenderSettings settings);

	protected abstract void QuiesceRenderer();

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			Paused = true;
			Delegate = null;
			DisposeRenderer();
		}

		base.Dispose(disposing);
	}

	protected abstract void DisposeRenderer();
}
