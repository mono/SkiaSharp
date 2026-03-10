using System;
using System.Runtime.InteropServices;
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
	internal partial class SKHtmlCanvasInterop : JSModuleInterop
	{
		private const string ModuleName = "SKHtmlCanvas";
		private const string JsFilename = "./_content/SkiaSharp.Views.Blazor/SKHtmlCanvas.js";
		private const string InitGLSymbol = "SKHtmlCanvas.initGL";
		private const string InitRasterSymbol = "SKHtmlCanvas.initRaster";
		private const string DeinitSymbol = "SKHtmlCanvas.deinit";
		private const string RequestAnimationFrameSymbol = "SKHtmlCanvas.requestAnimationFrame";
		private const string PutImageDataSymbol = "SKHtmlCanvas.putImageData";

		private readonly ElementReference htmlCanvas;
		private readonly string htmlElementId;
#if NET7_0_OR_GREATER
		private readonly Action callbackHelper;
#else
		private readonly ActionHelper callbackHelper;

		private DotNetObjectReference<ActionHelper>? callbackReference;
#endif

		public static async Task<SKHtmlCanvasInterop> ImportAsync(IJSRuntime js, ElementReference element, Action callback)
		{
			var interop = new SKHtmlCanvasInterop(js, element, callback);
			await interop.ImportAsync();
			return interop;
		}

		public SKHtmlCanvasInterop(IJSRuntime js, ElementReference element, Action renderFrameCallback)
			: base(js, ModuleName, JsFilename)
		{
			htmlCanvas = element;
			htmlElementId = "_bl_" + element.Id;

			callbackHelper = new(renderFrameCallback);
		}

		protected override void OnDisposingModule() =>
			Deinit();

#if NET7_0_OR_GREATER
		public GLInfo InitGL()
		{
			Init();

			var obj = InitGL(null, htmlElementId, callbackHelper);
			var info = new GLInfo(
				obj.GetPropertyAsInt32("contextId"),
				(uint)obj.GetPropertyAsInt32("fboId"),
				obj.GetPropertyAsInt32("stencils"),
				obj.GetPropertyAsInt32("samples"),
				obj.GetPropertyAsInt32("depth"));
			return info;
		}

		[JSImport(InitGLSymbol, ModuleName)]
		public static partial JSObject InitGL(JSObject? element, string elementId, [JSMarshalAs<JSType.Function>] Action callback);

		public bool InitRaster()
		{
			Init();

			return InitRaster(null, htmlElementId, callbackHelper);
		}

		[JSImport(InitRasterSymbol, ModuleName)]
		public static partial bool InitRaster(JSObject? element, string elementId, [JSMarshalAs<JSType.Function>] Action callback);

		public void Deinit() =>
			Deinit(htmlElementId);

		[JSImport(DeinitSymbol, ModuleName)]
		public static partial void Deinit(string elementId);

		public void RequestAnimationFrame(bool enableRenderLoop, int rawWidth, int rawHeight) =>
			RequestAnimationFrame(htmlElementId, enableRenderLoop, rawWidth, rawHeight);

		[JSImport(RequestAnimationFrameSymbol, ModuleName)]
		public static partial void RequestAnimationFrame(string elementId, bool enableRenderLoop, int rawWidth, int rawHeight);

		public void PutImageData(IntPtr intPtr, SKSizeI rawSize) =>
			PutImageData(htmlElementId, intPtr, rawSize.Width, rawSize.Height);

		[JSImport(PutImageDataSymbol, ModuleName)]
		public static partial void PutImageData(string elementId, IntPtr intPtr, int rawWidth, int rawHeight);
#else
		public GLInfo InitGL()
		{
			if (callbackReference != null)
				throw new InvalidOperationException("Unable to initialize the same canvas more than once.");

			Init();

			callbackReference = DotNetObjectReference.Create(callbackHelper);

			return Invoke<GLInfo>(InitGLSymbol, htmlCanvas, htmlElementId, callbackReference);
		}

		public bool InitRaster()
		{
			if (callbackReference != null)
				throw new InvalidOperationException("Unable to initialize the same canvas more than once.");

			Init();

			callbackReference = DotNetObjectReference.Create(callbackHelper);

			return Invoke<bool>(InitRasterSymbol, htmlCanvas, htmlElementId, callbackReference);
		}

		public void Deinit()
		{
			if (callbackReference == null)
				return;

			Invoke(DeinitSymbol, htmlElementId);

			callbackReference?.Dispose();
		}

		public void RequestAnimationFrame(bool enableRenderLoop, int rawWidth, int rawHeight) =>
			Invoke(RequestAnimationFrameSymbol, htmlElementId, enableRenderLoop, rawWidth, rawHeight);

		public void PutImageData(IntPtr intPtr, SKSizeI rawSize) =>
			Invoke(PutImageDataSymbol, htmlElementId, intPtr.ToInt64(), rawSize.Width, rawSize.Height);
#endif

		public record GLInfo(int ContextId, uint FboId, int Stencils, int Samples, int Depth);

		static void Init()
		{
			try
			{
				InterceptBrowserObjects();
			}
			catch
			{
				// no-op
			}
		}

		// Workaround for https://github.com/dotnet/runtime/issues/76077
		[DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
		static extern void InterceptBrowserObjects();
	}
}
