#if INCLUDE_GPU_VIEWS
using System;
using System.Threading;
using Uno.Foundation;
using Uno.Foundation.Interop;
using Windows.UI.Xaml;

namespace SkiaSharp.Views.UWP
{
	public partial class SKSwapChainPanel : FrameworkElement
	{
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
			: base("canvas")
		{
			jsInterop = new SKSwapChainPanelJsInterop(this);
			Initialize();
		}

		partial void DoLoaded()
		{
			Invalidate();
		}

		private void DoInvalidate()
		{
			if (designMode)
				return;

			if (!isVisible)
				return;

			if ((int)ActualWidth <= 0 || (int)ActualHeight <= 0)
				return;

			jsInterop.RequestAnimationFrame();
		}

		internal void RenderFrame()
		{
			// create the WebGL context
			if (!jsInfo.IsValid)
			{
				jsInfo = jsInterop.CreateContext();
				Console.WriteLine("CreateContext");
			}
			if (!jsInfo.IsValid)
			{
				Console.WriteLine("jsInfo.IsValid == FALSE?");

				return;
			}

			// create the SkiaSharp context
			if (context == null)
			{
				glInterface = GRGlInterface.Create();
				context = GRContext.CreateGl(glInterface);

				// bump the default resource cache limit
				var RESOURCE_CACHE_BYTES = 256 * 1024 * 1024;
				context.SetResourceCacheLimit(RESOURCE_CACHE_BYTES);
			}

			// get the new surface size
			var newSize = new SKSizeI((int)ActualWidth, (int)ActualHeight);

			Console.WriteLine("newSize: " + newSize);

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
				OnPaintSurface(new SKPaintGLSurfaceEventArgs(surface, renderTarget, surfaceOrigin, colorType, glInfo));
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

			public void RequestAnimationFrame() =>
				WebAssemblyRuntime.InvokeJSWithInterop($"{this}.requestAnimationFrame();");

			public JsInfo CreateContext()
			{
				var resultString = WebAssemblyRuntime.InvokeJSWithInterop($"return {this}.createContext('{Panel.HtmlId}');");
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
				WebAssemblyRuntime.InvokeJS($"SkiaSharp.Views.UWP.SKSwapChainPanel.createInstance('{managedHandle}', '{jsHandle}')");
				return jsHandle;
			}

			string IJSObjectMetadata.GetNativeInstance(IntPtr managedHandle, long jsHandle) =>
				$"SkiaSharp.Views.UWP.SKSwapChainPanel.getInstance('{jsHandle}')";

			void IJSObjectMetadata.DestroyNativeInstance(IntPtr managedHandle, long jsHandle) =>
				WebAssemblyRuntime.InvokeJS($"SkiaSharp.Views.UWP.SKSwapChainPanel.destroyInstance('{jsHandle}')");

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
#endif
