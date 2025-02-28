using System;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

#if NET7_0_OR_GREATER
using System.Runtime.InteropServices.JavaScript;
#endif

namespace SkiaSharp.Views.Blazor.Internal
{
	[SupportedOSPlatform("browser")]
	internal partial class SizeWatcherInterop : JSModuleInterop
	{
		private const string ModuleName = "SizeWatcher";
		private const string JsFilename = "./_content/SkiaSharp.Views.Blazor/SizeWatcher.js";
		private const string ObserveSymbol = "SizeWatcher.observe";
		private const string UnobserveSymbol = "SizeWatcher.unobserve";

		private readonly ElementReference htmlElement;
		private readonly string htmlElementId;
#if NET7_0_OR_GREATER
		private readonly Action<float, float> callbackHelper;
#else
		private readonly FloatFloatActionHelper callbackHelper;

		private DotNetObjectReference<FloatFloatActionHelper>? callbackReference;
#endif

		public static async Task<SizeWatcherInterop> ImportAsync(IJSRuntime js, ElementReference element, Action<SKSize> callback)
		{
			var interop = new SizeWatcherInterop(js, element, callback);
			await interop.ImportAsync();
			interop.Start();
			return interop;
		}

		public SizeWatcherInterop(IJSRuntime js, ElementReference element, Action<SKSize> callback)
			: base(js, ModuleName, JsFilename)
		{
			htmlElement = element;
			htmlElementId = "_bl_" + element.Id;
			callbackHelper = new((x, y) => callback(new SKSize(x, y)));
		}

		protected override void OnDisposingModule() =>
			Stop();

#if NET7_0_OR_GREATER
		public void Start() =>
			Observe(null, htmlElementId, callbackHelper);

		public void Stop() =>
			Unobserve(htmlElementId);

		[JSImport(ObserveSymbol, ModuleName)]
		private static partial void Observe(JSObject? element, string elementId, [JSMarshalAs<JSType.Function<JSType.Number, JSType.Number>>] Action<float, float> callback);

		[JSImport(UnobserveSymbol, ModuleName)]
		private static partial void Unobserve(string elementId);
#else
		public void Start()
		{
			callbackReference ??= DotNetObjectReference.Create(callbackHelper);

			Invoke(ObserveSymbol, htmlElement, htmlElementId, callbackReference);
		}

		public void Stop()
		{
			if (callbackReference == null)
				return;

			Invoke(UnobserveSymbol, htmlElementId);

			callbackReference?.Dispose();
			callbackReference = null;
		}
#endif
	}
}
