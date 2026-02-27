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
	internal partial class SKTouchInterop : JSModuleInterop
	{
		private const string ModuleName = "SKTouchInterop";
		private const string JsFilename = "./_content/SkiaSharp.Views.Blazor/SKTouchInterop.js";
		private const string StartSymbol = "SKTouchInterop.start";
		private const string StopSymbol = "SKTouchInterop.stop";

		private readonly ElementReference htmlElement;
		private readonly string htmlElementId;
#if NET7_0_OR_GREATER
		private readonly Func<JSObject, bool> callbackHelper;
#else
		private readonly SKTouchCallbackHelper callbackHelper;

		private DotNetObjectReference<SKTouchCallbackHelper>? callbackReference;
#endif

		public static async Task<SKTouchInterop> ImportAsync(IJSRuntime js, ElementReference element, Func<PointerEventData, bool> callback)
		{
			var interop = new SKTouchInterop(js, element, callback);
			await interop.ImportAsync();
			return interop;
		}

		public SKTouchInterop(IJSRuntime js, ElementReference element, Func<PointerEventData, bool> callback)
			: base(js, ModuleName, JsFilename)
		{
			htmlElement = element;
			htmlElementId = "_bl_" + element.Id;

#if NET7_0_OR_GREATER
			callbackHelper = new((jsObj) =>
			{
				using var _ = jsObj;
				var data = new PointerEventData
				{
					Id = jsObj.GetPropertyAsInt32("id"),
					Action = jsObj.GetPropertyAsInt32("action"),
					DeviceType = jsObj.GetPropertyAsInt32("deviceType"),
					MouseButton = jsObj.GetPropertyAsInt32("mouseButton"),
					X = (float)jsObj.GetPropertyAsDouble("x"),
					Y = (float)jsObj.GetPropertyAsDouble("y"),
					Pressure = (float)jsObj.GetPropertyAsDouble("pressure"),
					InContact = jsObj.GetPropertyAsBoolean("inContact"),
					WheelDelta = jsObj.GetPropertyAsInt32("wheelDelta"),
				};
				return callback(data);
			});
#else
			callbackHelper = new SKTouchCallbackHelper(callback);
#endif
		}

		protected override void OnDisposingModule() =>
			Stop();

#if NET7_0_OR_GREATER
		public void Start() =>
			Start(null, htmlElementId, callbackHelper);

		public void Stop() =>
			Stop(htmlElementId);

		[JSImport(StartSymbol, ModuleName)]
		private static partial void Start(JSObject? element, string elementId, [JSMarshalAs<JSType.Function<JSType.Object, JSType.Boolean>>] Func<JSObject, bool> callback);

		[JSImport(StopSymbol, ModuleName)]
		private static partial void Stop(string elementId);
#else
		public void Start()
		{
			callbackReference ??= DotNetObjectReference.Create(callbackHelper);

			Invoke(StartSymbol, htmlElement, htmlElementId, callbackReference);
		}

		public void Stop()
		{
			if (callbackReference == null)
				return;

			Invoke(StopSymbol, htmlElementId);

			callbackReference?.Dispose();
			callbackReference = null;
		}
#endif
	}
}
