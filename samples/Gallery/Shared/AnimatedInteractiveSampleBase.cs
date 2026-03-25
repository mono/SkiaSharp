namespace SkiaSharpSample;

public abstract class AnimatedInteractiveSampleBase : InteractiveSampleBase
{
	private CancellationTokenSource cts;

	protected override async Task OnInit()
	{
		await base.OnInit();

		var scheduler = SynchronizationContext.Current != null
			? TaskScheduler.FromCurrentSynchronizationContext()
			: TaskScheduler.Default;

		cts = new CancellationTokenSource();
		_ = Task.Run(async () =>
		{
			while (!cts.IsCancellationRequested)
			{
				await OnUpdate(cts.Token);
				new Task(Refresh).Start(scheduler);
			}
		}, cts.Token);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		cts?.Cancel();
	}

	protected abstract Task OnUpdate(CancellationToken token);
}
