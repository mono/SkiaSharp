using SkiaSharpSample.Controls;

namespace SkiaSharpSample;

public abstract class InteractiveSampleBase : SampleBase
{
	public virtual IReadOnlyList<SampleControl> Controls => [];

	public void UpdateControl(string id, object value)
	{
		OnControlChanged(id, value);
		Refresh();
	}

	protected virtual void OnControlChanged(string id, object value) { }
}
