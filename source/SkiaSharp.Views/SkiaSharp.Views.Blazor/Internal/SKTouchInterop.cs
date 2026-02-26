using System;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace SkiaSharp.Views.Blazor.Internal
{
	[SupportedOSPlatform("browser")]
	internal sealed class SKTouchInterop : IAsyncDisposable
	{
		private const string JsFilename = "./_content/SkiaSharp.Views.Blazor/SKTouchInterop.js";

		private IJSObjectReference? module;
		private DotNetObjectReference<SKTouchCallbackHelper>? callbackRef;
		private ElementReference element;

		public static async Task<SKTouchInterop> CreateAsync(
			IJSRuntime js,
			ElementReference element,
			Func<SKTouchCallbackHelper.PointerEventData, Task> callback)
		{
			var interop = new SKTouchInterop();
			interop.element = element;
			interop.module = await js.InvokeAsync<IJSObjectReference>("import", JsFilename);
			interop.callbackRef = DotNetObjectReference.Create(new SKTouchCallbackHelper(callback));
			await interop.module.InvokeVoidAsync("initializeTouchEvents", element, interop.callbackRef);
			return interop;
		}

		public async ValueTask DisposeAsync()
		{
			if (module is not null)
			{
				try
				{
					await module.InvokeVoidAsync("disposeTouchEvents", element);
				}
				catch (JSDisconnectedException) { }
				catch (ObjectDisposedException) { }

				try
				{
					await module.DisposeAsync();
				}
				catch (JSDisconnectedException) { }
				catch (ObjectDisposedException) { }

				module = null;
			}

			callbackRef?.Dispose();
			callbackRef = null;
		}
	}
}
