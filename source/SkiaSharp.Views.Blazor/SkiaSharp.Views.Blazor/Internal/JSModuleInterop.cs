using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace SkiaSharp.Views.Blazor.Internal
{
	internal class JSModuleInterop : IAsyncDisposable
	{
		private readonly Lazy<Task<IJSObjectReference>> moduleTask;

		public JSModuleInterop(IJSRuntime js, string filename)
		{
			moduleTask = new(() => js.InvokeAsync<IJSObjectReference>("import", filename).AsTask());
		}

		public async ValueTask DisposeAsync()
		{
			if (!ModuleImported)
				return;

			await OnDisposingModuleAsync();

			var module = await GetModuleAsync();

			await module.DisposeAsync();
		}

		public bool ModuleImported =>
			moduleTask.IsValueCreated;

		protected Task<IJSObjectReference> GetModuleAsync() =>
			moduleTask.Value;

		protected async Task InvokeAsync(string identifier, params object?[]? args)
		{
			var module = await GetModuleAsync();
			await module.InvokeVoidAsync(identifier, args);
		}

		protected async Task<TValue> InvokeAsync<TValue>(string identifier, params object?[]? args)
		{
			var module = await GetModuleAsync();
			return await module.InvokeAsync<TValue>(identifier, args);
		}

		protected virtual Task OnDisposingModuleAsync() =>
			Task.CompletedTask;
	}
}
