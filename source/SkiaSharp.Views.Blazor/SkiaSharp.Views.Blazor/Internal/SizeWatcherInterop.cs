using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

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

		public SizeWatcherInterop(IJSRuntime js, ElementReference element, Action<SKSize> callback)
			: base(js, JsFilename)
		{
			htmlElement = element;
			htmlElementId = element.Id;
			callbackHelper = new FloatFloatActionHelper((x, y) => callback(new SKSize(x, y)));
		}

		protected override Task OnDisposingModuleAsync() =>
			StopAsync();

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
