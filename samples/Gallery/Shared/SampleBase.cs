using System;
using System.Threading;
using System.Threading.Tasks;
using SkiaSharp;

namespace SkiaSharpSample
{
	public abstract class SampleBase
	{
		protected SKMatrix Matrix = SKMatrix.Identity;

		private SKMatrix startPanMatrix = SKMatrix.Identity;
		private SKMatrix startPinchMatrix = SKMatrix.Identity;
		private SKPoint startPinchOrigin = SKPoint.Empty;
		private float totalPinchScale = 1f;

		public abstract string Title { get; }

		public virtual string Description { get; } = string.Empty;

		public virtual SamplePlatforms SupportedPlatform { get; } = SamplePlatforms.All;

		public virtual SampleBackends SupportedBackends { get; } = SampleBackends.All;

		public virtual SampleCategories Category { get; } = SampleCategories.General;

		public bool IsInitialized { get; private set; } = false;

		public void DrawSample(SKCanvas canvas, int width, int height)
		{
			if (IsInitialized)
			{
				canvas.SetMatrix(Matrix);
				OnDrawSample(canvas, width, height);
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
			return Task.FromResult(true);
		}

		protected virtual void OnDestroy()
		{
		}

		public void Tap()
		{
			if (IsInitialized)
			{
				OnTapped();
			}
		}

		protected virtual void OnTapped()
		{
		}

		public void Pan(GestureState state, SKPoint translation)
		{
			switch (state)
			{
				case GestureState.Started:
					startPanMatrix = Matrix;
					break;
				case GestureState.Running:
					var canvasTranslation = SKMatrix.CreateTranslation(translation.X, translation.Y);
					SKMatrix.Concat(ref Matrix, ref canvasTranslation, ref startPanMatrix);
					break;
				default:
					startPanMatrix = SKMatrix.Identity;
					break;
			}
		}

		public void Pinch(GestureState state, float scale, SKPoint origin)
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
					SKMatrix.Concat(ref canvasCombined, ref canvasScaling, ref canvasTranslation);
					SKMatrix.Concat(ref Matrix, ref canvasCombined, ref startPinchMatrix);
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

	public abstract class AnimatedSampleBase : SampleBase
	{
		private CancellationTokenSource cts;

		[Preserve]
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

	public enum GestureState
	{
		Started,
		Running,
		Completed,
		Canceled
	}
}
