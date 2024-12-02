using System;
using System.Threading.Tasks;
using System.Runtime.Versioning;
using Microsoft.JSInterop;

#if NET7_0_OR_GREATER
using System.Runtime.InteropServices.JavaScript;
#else
using JSObject = Microsoft.JSInterop.IJSUnmarshalledObjectReference;
#endif

namespace SkiaSharp.Views.Blazor.Internal
{
	[SupportedOSPlatform("browser")]
	internal partial class JSModuleInterop : IDisposable
	{
		private readonly Task<JSObject> moduleTask;
		private JSObject? module;

		public JSModuleInterop(IJSRuntime js, string moduleName, string moduleUrl)
		{
#if NET7_0_OR_GREATER
			moduleTask = JSHost.ImportAsync(moduleName, "../" + moduleUrl);
#else
			if (js is not IJSInProcessRuntime)
				throw new NotSupportedException("SkiaSharp currently only works on Web Assembly.");

			moduleTask = js.InvokeAsync<JSObject>("import", moduleUrl).AsTask();
#endif
		}

		public async Task ImportAsync()
		{
			module = await moduleTask;
		}

		public void Dispose()
		{
			OnDisposingModule();
			Module.Dispose();
		}

		protected JSObject Module =>
			module ?? throw new InvalidOperationException("Make sure to run ImportAsync() first.");

#if !NET7_0_OR_GREATER
		protected void Invoke(string identifier, params object?[]? args) =>
			Module.InvokeVoid(identifier, args);

		protected TValue Invoke<TValue>(string identifier, params object?[]? args) =>
			Module.Invoke<TValue>(identifier, args);
#endif

		protected virtual void OnDisposingModule() { }
	}
}
