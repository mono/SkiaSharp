using System;
using System.Threading;
using System.Threading.Tasks;
using SkiaSharp;

namespace SkiaSharpSample;

public abstract class CanvasSampleBase : SampleBase
{
	private CancellationTokenSource? cts;

	public virtual bool IsAnimated => false;

	public event EventHandler? RefreshRequested;

	protected void Refresh()
	{
		RefreshRequested?.Invoke(this, EventArgs.Empty);
	}

	public void DrawSample(SKCanvas canvas, int width, int height)
	{
		if (IsInitialized)
		{
			OnDrawSample(canvas, width, height);
		}
	}

	protected abstract void OnDrawSample(SKCanvas canvas, int width, int height);

	protected virtual Task OnUpdate(CancellationToken token) => Task.CompletedTask;

	protected override Task OnInit()
	{
		if (IsAnimated)
		{
			var scheduler = SynchronizationContext.Current != null
				? TaskScheduler.FromCurrentSynchronizationContext()
				: TaskScheduler.Default;

			cts = new CancellationTokenSource();
			_ = Task.Run(async () =>
			{
				try
				{
					while (!cts.IsCancellationRequested)
					{
						await OnUpdate(cts.Token);
						new Task(Refresh).Start(scheduler);
					}
				}
				catch (OperationCanceledException)
				{
					// Expected when CTS is cancelled during shutdown
				}
			}, cts.Token);
		}

		return Task.CompletedTask;
	}

	protected override void OnDestroy()
	{
		cts?.Cancel();
		cts?.Dispose();
		cts = null;
	}

	public override void UpdateControl(string id, object value)
	{
		OnControlChanged(id, value);
		Refresh();
	}
}
