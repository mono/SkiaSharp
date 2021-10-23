using System;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace SkiaSharp.Views.Blazor.Internal
{
	internal class JSModuleInterop : IAsyncDisposable
	{
		private readonly Task<IJSObjectReference> moduleTask;
		private IJSObjectReference? module;

		public JSModuleInterop(IJSRuntime js, string filename)
		{
			if (js is not IJSInProcessRuntime)
			{
				RequiresAsync = true;
			}

			moduleTask = js.InvokeAsync<IJSObjectReference>("import", filename).AsTask();
		}

		public async Task ImportAsync()
		{
			module = await moduleTask;
		}

		public async ValueTask DisposeAsync()
		{
			await OnDisposingModuleAsync();
			await Module.DisposeAsync();
		}

		protected IJSObjectReference Module =>
			module ?? throw new InvalidOperationException("Make sure to run ImportAsync() first.");

		public bool RequiresAsync { get; }

		protected ValueTask InvokeAsync(string identifier, params object?[]? args) =>
			Module.InvokeVoidAsync(identifier, args);

		protected ValueTask<TValue> InvokeAsync<TValue>(string identifier, params object?[]? args) =>
			Module.InvokeAsync<TValue>(identifier, args);

		protected virtual Task OnDisposingModuleAsync() =>
			Task.CompletedTask;
	}
}
