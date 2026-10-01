using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using SkiaSharp;

namespace SkiaSharpSample;

public abstract class CanvasSampleBase : SampleBase
{
	private CancellationTokenSource? cts;

	public virtual bool IsAnimated => false;

	protected virtual TimeSpan AnimationInterval => TimeSpan.FromMilliseconds(16);

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

	protected virtual bool OnUpdate(TimeSpan elapsed) => true;

	protected override Task OnInit()
	{
		if (IsAnimated)
		{
			cts = new CancellationTokenSource();
			RunAnimation(cts.Token);
		}

		return Task.CompletedTask;
	}

	private async void RunAnimation(CancellationToken token)
	{
		var lastTick = Stopwatch.GetTimestamp();
		try
		{
			while (!token.IsCancellationRequested)
			{
				var interval = AnimationInterval;
				if (interval <= TimeSpan.Zero)
					throw new InvalidOperationException("Animation intervals must be positive.");
				await Task.Delay(interval < TimeSpan.FromMilliseconds(10) ? TimeSpan.FromMilliseconds(10) : interval, token);
				if (token.IsCancellationRequested)
					break;
				var now = Stopwatch.GetTimestamp();
				var elapsed = Stopwatch.GetElapsedTime(lastTick, now);
				lastTick = now;
				if (OnUpdate(elapsed))
					Refresh();
			}
		}
		catch (OperationCanceledException) when (token.IsCancellationRequested)
		{
		}
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
