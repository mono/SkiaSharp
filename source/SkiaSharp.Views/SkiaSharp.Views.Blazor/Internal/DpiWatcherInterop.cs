using System;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Microsoft.JSInterop;

#if NET7_0_OR_GREATER
using System.Runtime.InteropServices.JavaScript;
#endif

namespace SkiaSharp.Views.Blazor.Internal
{
	[SupportedOSPlatform("browser")]
	internal partial class DpiWatcherInterop : JSModuleInterop
	{
		private const string ModuleName = "DpiWatcher";
		private const string JsFilename = "./_content/SkiaSharp.Views.Blazor/DpiWatcher.js";
		private const string StartSymbol = "DpiWatcher.start";
		private const string StopSymbol = "DpiWatcher.stop";
		private const string GetDpiSymbol = "DpiWatcher.getDpi";

		private static DpiWatcherInterop? instance;

		private event Action<double>? callbacksEvent;
#if NET7_0_OR_GREATER
		private readonly Action<double, double> callbackHelper;
#else
		private readonly FloatFloatActionHelper callbackHelper;

		private DotNetObjectReference<FloatFloatActionHelper>? callbackReference;
#endif

		public static async Task<DpiWatcherInterop> ImportAsync(IJSRuntime js, Action<double>? callback = null)
		{
			var interop = Get(js);
			await interop.ImportAsync();
			if (callback != null)
				interop.Subscribe(callback);
			return interop;
		}

		public static DpiWatcherInterop Get(IJSRuntime js) =>
			instance ??= new DpiWatcherInterop(js);

		private DpiWatcherInterop(IJSRuntime js)
			: base(js, ModuleName, JsFilename)
		{
			callbackHelper = new((o, n) => callbacksEvent?.Invoke((float)n));
		}

		protected override void OnDisposingModule() =>
			Stop();

		public void Subscribe(Action<double> callback)
		{
			var shouldStart = callbacksEvent == null;

			callbacksEvent += callback;

			var dpi = shouldStart
				? Start()
				: GetDpi();

			callback(dpi);
		}

		public void Unsubscribe(Action<double> callback)
		{
			callbacksEvent -= callback;

			if (callbacksEvent == null)
				Stop();
		}

#if NET7_0_OR_GREATER
		private double Start() =>
			Start(callbackHelper);

		[JSImport(StartSymbol, ModuleName)]
		private static partial double Start([JSMarshalAs<JSType.Function<JSType.Number, JSType.Number>>] Action<double, double> callback);

		[JSImport(StopSymbol, ModuleName)]
		private static partial void Stop();

		[JSImport(GetDpiSymbol, ModuleName)]
		public static partial double GetDpi();
#else
		private double Start()
		{
			callbackReference ??= DotNetObjectReference.Create(callbackHelper);

			return Invoke<double>(StartSymbol, callbackReference);
		}

		private void Stop()
		{
			Invoke(StopSymbol);

			callbackReference?.Dispose();
			callbackReference = null;
		}

		public double GetDpi() =>
			Invoke<double>(GetDpiSymbol);
#endif
	}
}
