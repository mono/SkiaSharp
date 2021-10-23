using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace SkiaSharp.Views.Blazor.Internal
{
	internal class SizeWatcherInterop : JSModuleInterop
	{
		private const string JsFilename = "./_content/SkiaSharp.Views.Blazor/SizeWatcher.js";
		private const string ObserveSymbol = "SizeWatcher.observe";
		private const string UnobserveSymbol = "SizeWatcher.unobserve";

		private readonly ElementReference htmlElement;
		private readonly string htmlElementId;
		private readonly FloatFloatActionHelper callbackHelper;

		private DotNetObjectReference<FloatFloatActionHelper>? callbackReference;

		public static async Task<SizeWatcherInterop> ImportAsync(IJSRuntime js, ElementReference element, Action<SKSize> callback)
		{
			var interop = new SizeWatcherInterop(js, element, callback);
			await interop.ImportAsync();
			await interop.StartAsync();
			return interop;
		}

		public SizeWatcherInterop(IJSRuntime js, ElementReference element, Action<SKSize> callback)
			: base(js, JsFilename)
		{
			htmlElement = element;
			htmlElementId = element.Id;
			callbackHelper = new FloatFloatActionHelper((x, y) => callback(new SKSize(x, y)));
		}

		protected override async Task OnDisposingModuleAsync() =>
			await StopAsync();

		public async Task StartAsync()
		{
			if (callbackReference != null)
				return;

			callbackReference = DotNetObjectReference.Create(callbackHelper);

			await InvokeAsync(ObserveSymbol, htmlElement, htmlElementId, callbackReference);
		}

		public async Task StopAsync()
		{
			if (callbackReference == null)
				return;

			await InvokeAsync(UnobserveSymbol, htmlElementId);

			callbackReference?.Dispose();
			callbackReference = null;
		}
	}
}
