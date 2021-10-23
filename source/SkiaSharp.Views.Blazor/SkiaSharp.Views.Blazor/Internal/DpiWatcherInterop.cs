using System;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace SkiaSharp.Views.Blazor.Internal
{
	internal class DpiWatcherInterop : JSModuleInterop
	{
		private const string JsFilename = "./_content/SkiaSharp.Views.Blazor/DpiWatcher.js";
		private const string StartSymbol = "DpiWatcher.start";
		private const string StopSymbol = "DpiWatcher.stop";
		private const string GetDpiSymbol = "DpiWatcher.getDpi";

		private static DpiWatcherInterop? instance;

		private event Action<double>? callbacksEvent;
		private readonly FloatFloatActionHelper callbackHelper;

		private DotNetObjectReference<FloatFloatActionHelper>? callbackReference;

		public static async Task<DpiWatcherInterop> ImportAsync(IJSRuntime js, Action<double>? callback = null)
		{
			var interop = Get(js);
			await interop.ImportAsync();

			if (callback != null)
				await interop.SubscribeAsync(callback);

			return interop;
		}

		public static DpiWatcherInterop Get(IJSRuntime js) =>
			instance ??= new DpiWatcherInterop(js);

		private DpiWatcherInterop(IJSRuntime js)
			: base(js, JsFilename)
		{
			callbackHelper = new FloatFloatActionHelper((o, n) => callbacksEvent?.Invoke(n));
		}

		protected override async Task OnDisposingModuleAsync() =>
			await StopAsync();

		public async Task SubscribeAsync(Action<double> callback)
		{
			var shouldStart = callbacksEvent == null;

			callbacksEvent += callback;

			var dpi = shouldStart
				? await StartAsync()
				: await GetDpiAsync();

			callback(dpi);
		}

		public async Task UnsubscribeAsync(Action<double> callback)
		{
			callbacksEvent -= callback;

			if (callbacksEvent == null)
				await StopAsync();
		}

		private async Task<double> StartAsync()
		{
			if (callbackReference != null)
				return await GetDpiAsync();

			callbackReference = DotNetObjectReference.Create(callbackHelper);

			return await InvokeAsync<double>(StartSymbol, callbackReference);
		}

		private async Task StopAsync()
		{
			if (callbackReference == null)
				return;

			await InvokeAsync(StopSymbol);

			callbackReference?.Dispose();
			callbackReference = null;
		}

		public async Task<double> GetDpiAsync() =>
			await InvokeAsync<double>(GetDpiSymbol);
	}
}
