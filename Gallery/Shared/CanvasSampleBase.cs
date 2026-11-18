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

	public event EventHandler<Exception>? AnimationFailed;

	protected void Refresh()
	{
		RefreshRequested?.Invoke(this, EventArgs.Empty);
	}

	public void DrawSample(SKCanvas canvas, int width, int height)
	{
		lock (SyncRoot)
		{
			if (IsInitialized)
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
			var token = cts.Token;
			_ = Task.Run(async () =>
			{
				try
				{
					while (!token.IsCancellationRequested)
					{
						await OnUpdate(token);
						token.ThrowIfCancellationRequested();
						await Task.Factory.StartNew(Refresh, token, TaskCreationOptions.DenyChildAttach, scheduler);
					}
				}
				catch (OperationCanceledException) when (token.IsCancellationRequested)
				{
					// Expected when CTS is cancelled during shutdown
				}
				catch (Exception ex)
				{
					System.Diagnostics.Debug.WriteLine($"Animation failed for {Title}: {ex}");
					if (token.IsCancellationRequested)
						return;
					await Task.Factory.StartNew(
						() =>
						{
							if (!token.IsCancellationRequested)
								AnimationFailed?.Invoke(this, ex);
						},
						CancellationToken.None, TaskCreationOptions.DenyChildAttach, scheduler);
				}
			}, token);
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
		lock (SyncRoot)
			OnControlChanged(id, value);
		Refresh();
	}
}
