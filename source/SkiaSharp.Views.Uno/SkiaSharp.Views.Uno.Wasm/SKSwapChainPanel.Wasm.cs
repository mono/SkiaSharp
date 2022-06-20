using System;
using System.Threading;
using Uno.Foundation;
using Uno.Foundation.Interop;
using Uno.UI.Runtime.WebAssembly;
#if WINUI
using Microsoft.UI.Xaml;
#else
using Windows.UI.Xaml;
#endif

#if WINDOWS || WINUI
namespace SkiaSharp.Views.Windows
#else
namespace SkiaSharp.Views.UWP
#endif
{
	[HtmlElement("canvas")]
	public partial class SKSwapChainPanel : FrameworkElement
	{
#if HAS_UNO_WINUI
		const string SKSwapChainPanelTypeFullName = "SkiaSharp.Views.Windows." + nameof(SKSwapChainPanel);
#else
		const string SKSwapChainPanelTypeFullName = "SkiaSharp.Views.UWP" + nameof(SKSwapChainPanel);
#endif

		private const int ResourceCacheBytes = 256 * 1024 * 1024; // 256 MB
		private const SKColorType colorType = SKColorType.Rgba8888;
		private const GRSurfaceOrigin surfaceOrigin = GRSurfaceOrigin.BottomLeft;

		private readonly SKSwapChainPanelJsInterop jsInterop;

		private GRGlInterface glInterface;
		private GRContext context;
		private JsInfo jsInfo;
		private GRGlFramebufferInfo glInfo;
		private GRBackendRenderTarget renderTarget;
		private SKSurface surface;
		private SKCanvas canvas;

		private SKSizeI lastSize;

		public SKSwapChainPanel()
		{
			jsInterop = new SKSwapChainPanelJsInterop(this);
			Initialize();
		}

		private SKSize GetCanvasSize() => lastSize;

		private GRContext GetGRContext() => context;

		partial void DoLoaded()
		{
			jsInfo = jsInterop.CreateContext();

			Invalidate();
		}

		partial void DoEnableRenderLoop(bool enable) =>
			jsInterop.SetEnableRenderLoop(enable);

		//partial void DoUpdateBounds() =>
		//	jsInterop.ResizeCanvas();

		private void DoInvalidate()
		{
			if (designMode)
				return;

			if (!isVisible)
				return;

			if ((int)ActualWidth <= 0 || (int)ActualHeight <= 0)
				return;

			jsInterop.RequestAnimationFrame(EnableRenderLoop);
		}

		internal void RenderFrame()
		{
			if (!jsInfo.IsValid)
				return;

			// create the SkiaSharp context
			if (context == null)
			{
				glInterface = GRGlInterface.Create();
				context = GRContext.CreateGl(glInterface);

				// bump the default resource cache limit
				context.SetResourceCacheLimit(ResourceCacheBytes);
			}

			// get the new surface size
			var newSize = new SKSizeI((int)(ActualWidth * ContentsScale), (int)(ActualHeight * ContentsScale));

			// manage the drawing surface
			if (renderTarget == null || lastSize != newSize || !renderTarget.IsValid)
			{
				// create or update the dimensions
				lastSize = newSize;

				glInfo = new GRGlFramebufferInfo(jsInfo.FboId, colorType.ToGlSizedFormat());

				// destroy the old surface
				surface?.Dispose();
				surface = null;
				canvas = null;

				// re-create the render target
				renderTarget?.Dispose();
				renderTarget = new GRBackendRenderTarget(newSize.Width, newSize.Height, jsInfo.Samples, jsInfo.Stencil, glInfo);
			}

			// create the surface
			if (surface == null)
			{
				surface = SKSurface.Create(context, renderTarget, surfaceOrigin, colorType);
				canvas = surface.Canvas;
			}

			using (new SKAutoCanvasRestore(canvas, true))
			{
				// start drawing
#pragma warning disable CS0612 // Type or member is obsolete
				OnPaintSurface(new SKPaintGLSurfaceEventArgs(surface, renderTarget, surfaceOrigin, colorType, glInfo));
#pragma warning restore CS0612 // Type or member is obsolete
			}

			// update the control
			canvas.Flush();
			context.Flush();
		}

		private struct JsInfo
		{
			public bool IsValid { get; set; }

			public int ContextId { get; set; }

			public uint FboId { get; set; }

			public int Stencil { get; set; }

			public int Samples { get; set; }

			public int Depth { get; set; }
		}

		private class SKSwapChainPanelJsInterop : IJSObject, IJSObjectMetadata
		{
			private static long handleCounter = 0L;

			private readonly long jsHandle;

			public SKSwapChainPanelJsInterop(SKSwapChainPanel panel)
			{
				Panel = panel ?? throw new ArgumentNullException(nameof(panel));

				jsHandle = Interlocked.Increment(ref handleCounter);
				Handle = JSObjectHandle.Create(this, this);
			}

			public SKSwapChainPanel Panel { get; }

			public JSObjectHandle Handle { get; }

			public void RenderFrame() =>
				Panel.RenderFrame();

			public void RequestAnimationFrame(bool renderLoop) =>
				WebAssemblyRuntime.InvokeJSWithInterop($"{this}.requestAnimationFrame({(renderLoop ? "true" : "false")});");

			public void SetEnableRenderLoop(bool enable) =>
				WebAssemblyRuntime.InvokeJSWithInterop($"{this}.setEnableRenderLoop({(enable ? "true" : "false")});");

			public void ResizeCanvas() =>
				WebAssemblyRuntime.InvokeJSWithInterop($"{this}.resizeCanvas();");

			public JsInfo CreateContext()
			{
				var resultString = WebAssemblyRuntime.InvokeJSWithInterop($"return {this}.createContext('{Panel.GetHtmlId()}');");
				var result = resultString?.Split(',');
				if (result?.Length != 5)
					return default;

				return new JsInfo
				{
					IsValid = true,
					ContextId = int.Parse(result[0]),
					FboId = uint.Parse(result[1]),
					Stencil = int.Parse(result[2]),
					Samples = int.Parse(result[3]),
					Depth = int.Parse(result[4]),
				};
			}

			long IJSObjectMetadata.CreateNativeInstance(IntPtr managedHandle)
			{
				WebAssemblyRuntime.InvokeJS(SKSwapChainPanelTypeFullName + $".createInstance('{managedHandle}', '{jsHandle}')");
				return jsHandle;
			}

			string IJSObjectMetadata.GetNativeInstance(IntPtr managedHandle, long jsHandle) =>
				SKSwapChainPanelTypeFullName + $".getInstance('{jsHandle}')";

			void IJSObjectMetadata.DestroyNativeInstance(IntPtr managedHandle, long jsHandle) =>
				WebAssemblyRuntime.InvokeJS(SKSwapChainPanelTypeFullName + $".destroyInstance('{jsHandle}')");

			object IJSObjectMetadata.InvokeManaged(object instance, string method, string parameters)
			{
				switch (method)
				{
					case nameof(RenderFrame):
						RenderFrame();
						break;

					default:
						throw new ArgumentException($"Unable to execute method: {method}", nameof(method));
				}

				return null;
			}
		}
	}
}
