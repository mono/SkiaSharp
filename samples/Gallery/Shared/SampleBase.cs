using System;
using System.Threading;
using System.Threading.Tasks;
using SkiaSharp;

namespace SkiaSharpSample;

public abstract class SampleBase
{
	protected SKMatrix Matrix = SKMatrix.Identity;

	private SKMatrix startPanMatrix = SKMatrix.Identity;
	private SKMatrix startPinchMatrix = SKMatrix.Identity;
	private SKPoint startPinchOrigin = SKPoint.Empty;
	private float totalPinchScale = 1f;
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
			canvas.Save();
			canvas.Concat(Matrix);
			OnDrawSample(canvas, width, height);
			canvas.Restore();
		}
	}

	protected abstract void OnDrawSample(SKCanvas canvas, int width, int height);

	public async void Init()
	{
		// reset the matrix for the new sample
		Matrix = SKMatrix.Identity;

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

	/// <summary>
	/// Called on each animation tick when <see cref="IsAnimated"/> is true.
	/// Override to update animation state.
	/// </summary>
	protected virtual Task OnUpdate(CancellationToken token) => Task.CompletedTask;

	/// <summary>
	/// Handles tap gestures. Override <see cref="OnTapped"/> to respond to taps.
	/// </summary>
	public virtual void Tap()
	{
		if (IsInitialized)
		{
			OnTapped();
		}
	}

	public void ResetMatrix()
	{
		Matrix = SKMatrix.Identity;
	}

	protected virtual void OnTapped()
	{
	}

	/// <summary>
	/// Handles pan gestures by applying a translation matrix.
	/// Override to implement custom pan behavior (e.g., freehand drawing input).
	/// </summary>
	public virtual void Pan(GestureState state, SKPoint translation)
	{
		switch (state)
		{
			case GestureState.Started:
				startPanMatrix = Matrix;
				break;
			case GestureState.Running:
				var canvasTranslation = SKMatrix.CreateTranslation(translation.X, translation.Y);
				SKMatrix.Concat(ref Matrix, canvasTranslation, startPanMatrix);
				break;
			default:
				startPanMatrix = SKMatrix.Identity;
				break;
		}
	}

	/// <summary>
	/// Handles pinch-to-zoom gestures. Override to implement custom zoom behavior.
	/// </summary>
	public virtual void Pinch(GestureState state, float scale, SKPoint origin)
	{
		switch (state)
		{
			case GestureState.Started:
				startPinchMatrix = Matrix;
				startPinchOrigin = origin;
				totalPinchScale = 1f;
				break;
			case GestureState.Running:
				totalPinchScale *= scale;
				var pinchTranslation = origin - startPinchOrigin;
				var canvasTranslation = SKMatrix.CreateTranslation(pinchTranslation.X, pinchTranslation.Y);
				var canvasScaling = SKMatrix.CreateScale(totalPinchScale, totalPinchScale, origin.X, origin.Y);
				var canvasCombined = SKMatrix.Identity;
				SKMatrix.Concat(ref canvasCombined, canvasScaling, canvasTranslation);
				SKMatrix.Concat(ref Matrix, canvasCombined, startPinchMatrix);
				break;
			default:
				startPinchMatrix = SKMatrix.Identity;
				startPinchOrigin = SKPoint.Empty;
				totalPinchScale = 1f;
				break;
		}
	}

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
