namespace SkiaSharpSample;

public abstract class AnimatedSampleBase : SampleBase
{
	private CancellationTokenSource cts;

	public AnimatedSampleBase()
	{
	}

	protected override async Task OnInit()
	{
		await base.OnInit();

#if !__WASM__
		var scheduler = TaskScheduler.FromCurrentSynchronizationContext();
#endif
		cts = new CancellationTokenSource();
		var loop = Task.Run(async () =>
		{
			while (!cts.IsCancellationRequested)
			{
				await OnUpdate(cts.Token);

				new Task(Refresh)
#if !__WASM__
				.Start(scheduler);
#else
				.Start();
#endif
			}
		}, cts.Token);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		cts.Cancel();
	}

	protected abstract Task OnUpdate(CancellationToken token);
}
