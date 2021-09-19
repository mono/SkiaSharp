using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace SkiaSharp.Views.Blazor.Internal
{
	internal class SKGLViewInterop : IAsyncDisposable
	{
		private const string JsFilename = "./_content/SkiaSharp.Views.Blazor/SKGLView.js";
		private const string InitSymbol = "SKGLView.init";
		private const string RequestAnimationFrameSymbol = "SKGLView.requestAnimationFrame";

		private readonly Lazy<Task<IJSObjectReference>> moduleTask;
		private readonly ActionHelper actionHelper;
		private readonly DotNetObjectReference<ActionHelper> callbackReference;

		public SKGLViewInterop(IJSRuntime js, Action renderFrameCallback)
		{
			moduleTask = new(() => js.InvokeAsync<IJSObjectReference>("import", JsFilename).AsTask());
			actionHelper = new ActionHelper(renderFrameCallback);
			callbackReference = DotNetObjectReference.Create(actionHelper);
		}

		public async ValueTask DisposeAsync()
		{
			callbackReference.Dispose();

			if (!moduleTask.IsValueCreated)
				return;

			var module = await moduleTask.Value;

			await module.DisposeAsync();
		}

		public async Task<Info> InitAsync(ElementReference htmlCanvas)
		{
			var module = await moduleTask.Value;

			return await module.InvokeAsync<Info>(InitSymbol, htmlCanvas, callbackReference);
		}

		public async Task RequestAnimationFrameAsync(ElementReference htmlCanvas, bool enableRenderLoop, int rawWidth, int rawHeight)
		{
			var module = await moduleTask.Value;

			await module.InvokeVoidAsync(RequestAnimationFrameSymbol, htmlCanvas, enableRenderLoop, rawWidth, rawHeight);
		}

		public record Info(int ContextId, uint FboId, int Stencils, int Samples, int Depth);
	}
}
