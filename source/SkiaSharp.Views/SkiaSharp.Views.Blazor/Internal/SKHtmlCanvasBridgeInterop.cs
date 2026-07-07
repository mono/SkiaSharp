using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace SkiaSharp.Views.Blazor.Internal
{
	/// <summary>
	/// Host-agnostic interop for the bridged presentation module (<c>SKHtmlCanvasBridge.js</c>).
	/// Unlike <see cref="JSModuleInterop"/> this uses the standard <see cref="IJSObjectReference"/>
	/// pipeline, so it works over the Blazor Server circuit and inside a Blazor Hybrid WebView —
	/// anywhere that is not WebAssembly.
	/// </summary>
	internal sealed class SKHtmlCanvasBridgeInterop : IAsyncDisposable
	{
		private const string JsFilename = "./_content/SkiaSharp.Views.Blazor/SKHtmlCanvasBridge.js";

		private readonly IJSRuntime js;
		private readonly ElementReference canvas;
		private IJSObjectReference? module;

		public SKHtmlCanvasBridgeInterop(IJSRuntime js, ElementReference canvas)
		{
			this.js = js ?? throw new ArgumentNullException(nameof(js));
			this.canvas = canvas;
		}

		public async Task ImportAsync()
		{
			module = await js.InvokeAsync<IJSObjectReference>("import", JsFilename);
		}

		public async Task InitializeAsync<T>(DotNetObjectReference<T> dotNetRef, bool isGL)
			where T : class
		{
			if (module is null)
				return;

			await module.InvokeVoidAsync("initialize", canvas, dotNetRef, isGL);
		}

		public async Task PresentAsync(byte[] bytes, int width, int height, string format, bool isGL)
		{
			if (module is null)
				return;

			await module.InvokeVoidAsync("present", canvas, bytes, width, height, format, isGL);
		}

		public async ValueTask DisposeAsync()
		{
			if (module is null)
				return;

			try
			{
				await module.InvokeVoidAsync("deinit", canvas);
			}
			catch
			{
				// circuit may already be gone
			}

			try
			{
				await module.DisposeAsync();
			}
			catch
			{
				// no-op
			}

			module = null;
		}
	}
}
