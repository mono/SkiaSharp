using System;
using System.Threading;
using System.Threading.Tasks;
using SkiaSharp;

namespace SkiaSharpSample;

public abstract class SampleBase
{
	private CancellationTokenSource cts;

	public abstract string Title { get; }

	public virtual string Description { get; } = string.Empty;

	public virtual string Category { get; } = SampleCategories.General;

	public virtual bool IsAnimated => false;

	public virtual bool IsSupported => true;

	public bool IsInitialized { get; private set; } = false;

	public void DrawSample(SKCanvas canvas, int width, int height)
	{
		if (IsInitialized)
		{
			OnDrawSample(canvas, width, height);
		}
	}

	protected abstract void OnDrawSample(SKCanvas canvas, int width, int height);

	public async void Init()
	{
		if (!IsInitialized)
		{
			await OnInit();

			IsInitialized = true;

			Refresh();
		}
	}

	public void Destroy()
	{
		if (IsInitialized)
		{
			OnDestroy();

			IsInitialized = false;
		}
	}

	protected virtual Task OnInit()
	{
		if (IsAnimated)
		{
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

		return Task.CompletedTask;
	}

	protected virtual void OnDestroy()
	{
		cts?.Cancel();
	}

	protected virtual Task OnUpdate(CancellationToken token) => Task.CompletedTask;

	public virtual bool MatchesFilter(string searchText)
	{
		if (string.IsNullOrWhiteSpace(searchText))
			return true;
		
		return
			Title.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) != -1 ||
			Description.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) != -1;
	}

	public event EventHandler RefreshRequested;

	protected void Refresh()
	{
		RefreshRequested?.Invoke(this, EventArgs.Empty);
	}
}
