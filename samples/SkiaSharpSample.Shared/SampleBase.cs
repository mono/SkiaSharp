using System;
using System.Threading.Tasks;
using SkiaSharp;

namespace SkiaSharpSample
{
	public abstract class SampleBase
	{
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
				OnDrawSample(canvas, width, height);
			}
		}

		protected abstract void OnDrawSample(SKCanvas canvas, int width, int height);

		public async void Init(Action callback = null)
		{
			if (!IsInitialized)
			{
				await OnInit();

				IsInitialized = true;

				callback?.Invoke();
			}
		}

		protected virtual Task OnInit()
		{
			return Task.FromResult(true);
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

		public virtual bool MatchesFilter(string searchText)
		{
			if (string.IsNullOrWhiteSpace(searchText))
				return true;
			
			return
				Title.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) != -1 ||
				Description.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) != -1;
		}
	}
}
