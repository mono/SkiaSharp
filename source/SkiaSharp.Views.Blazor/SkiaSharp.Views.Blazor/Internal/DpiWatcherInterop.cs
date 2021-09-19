using Microsoft.JSInterop;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace SkiaSharp.Views.Blazor.Internal
{
	public static class DpiWatcherInterop
	{
		private const string JsFilename = "./_content/SkiaSharp.Views.Blazor/DpiWatcher.js";
		private const string StartSymbol = "DpiWatcher.start";
		private const string StopSymbol = "DpiWatcher.stop";
		private const string GetDpiSymbol = "DpiWatcher.getDpi";

		private static Lazy<Task<IJSObjectReference>> moduleTask = null!;
		private static event Action<double>? DpiChangedInternal;

		public static event Action<double> DpiChanged
		{
			add
			{
				if (DpiChangedInternal == null)
					Start();

				DpiChangedInternal += value;
			}
			remove
			{
				DpiChangedInternal -= value;

				if (DpiChangedInternal == null)
					Stop();
			}
		}

		[JSInvokable]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static void UpdateDpi(double oldDpi, double newDpi)
		{
			DpiChangedInternal?.Invoke(newDpi);
		}

		internal static void Init(IJSRuntime js)
		{
			if (moduleTask != null)
				return;

			moduleTask = new(() => js.InvokeAsync<IJSObjectReference>("import", JsFilename).AsTask());
		}

		private static async void Start()
		{
			var module = await moduleTask.Value;

			await module.InvokeVoidAsync(StartSymbol);
		}

		private static async void Stop()
		{
			var module = await moduleTask.Value;

			await module.InvokeVoidAsync(StopSymbol);
		}

		public static async Task<double> GetDpiAsync()
		{
			var module = await moduleTask.Value;

			return await module.InvokeAsync<double>(GetDpiSymbol);
		}
	}
}
