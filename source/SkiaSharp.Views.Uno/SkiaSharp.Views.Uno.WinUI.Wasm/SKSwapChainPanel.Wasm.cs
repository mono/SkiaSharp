using System;
using System.Threading;
using Uno.Foundation;
using Uno.UI.Runtime.WebAssembly;
using System.Runtime.InteropServices;

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
#if NET7_0_OR_GREATER
	using System.Runtime.InteropServices.JavaScript;
	using NativeSwapChainPanel = System.Runtime.InteropServices.JavaScript.JSObject;
#else
	using NativeSwapChainPanel = SKSwapChainPanel.NativeMethods.SKSwapChainPanelJsInterop;
#endif

	[HtmlElement("canvas")]
	public partial class SKSwapChainPanel : FrameworkElement
	{
#if HAS_UNO_WINUI
		const string SKSwapChainPanelTypeFullName = "SkiaSharp.Views.Windows." + nameof(SKSwapChainPanel);
#else
		const string SKSwapChainPanelTypeFullName = "SkiaSharp.Views.UWP." + nameof(SKSwapChainPanel);
#endif

		private const int ResourceCacheBytes = 256 * 1024 * 1024; // 256 MB
		private const SKColorType colorType = SKColorType.Rgba8888;
		private const GRSurfaceOrigin surfaceOrigin = GRSurfaceOrigin.BottomLeft;

		private readonly NativeSwapChainPanel nativeSwapChainPanel;

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
			nativeSwapChainPanel = NativeMethods.CreateInstance(this);
			Initialize();
		}

		private SKSize GetCanvasSize() => lastSize;

		private GRContext GetGRContext() => context;

		partial void DoLoaded()
		{
			jsInfo = NativeMethods.CreateContext(this, nativeSwapChainPanel);

			Invalidate();
		}

		partial void DoEnableRenderLoop(bool enable) =>
			NativeMethods.SetEnableRenderLoop(nativeSwapChainPanel, enable);

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

			NativeMethods.SetEnableRenderLoop(nativeSwapChainPanel, true);
		}

#if NET7_0_OR_GREATER
		[JSExport()]
		internal static void RenderFrame([JSMarshalAs<JSType.Any>] object instance)
		{
			if(instance is SKSwapChainPanel panel)
			{
				panel.RenderFrame();
			}
		}
#endif

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

			// stop the render loop if it has been disabled
			if (!EnableRenderLoop)
				DoEnableRenderLoop(false);
		}

		internal struct JsInfo
		{
			public bool IsValid { get; set; }

			public int ContextId { get; set; }

			public uint FboId { get; set; }

			public int Stencil { get; set; }

			public int Samples { get; set; }

			public int Depth { get; set; }
		}

		internal static partial class NativeMethods
		{
			public static NativeSwapChainPanel CreateInstance(SKSwapChainPanel owner)
			{
#if NET7_0_OR_GREATER
				return CreateInstanceInternal(owner);
#else
				return new SKSwapChainPanelJsInterop(owner);
#endif
			}

#if NET7_0_OR_GREATER
			[JSImport("globalThis.SkiaSharp.Views.Windows.SKSwapChainPanel.createInstance")]
			public static partial NativeSwapChainPanel CreateInstanceInternal([JSMarshalAs<JSType.Any>] object owner);
#endif

			public static JsInfo CreateContext(SKSwapChainPanel owner, NativeSwapChainPanel nativeSwapChainPanel)
			{
#if NET7_0_OR_GREATER
				var jsInfo = new JsInfo();
				var jsObject = CreateContextInternal(nativeSwapChainPanel, owner.GetHtmlId());

				jsInfo.IsValid = true;
				jsInfo.ContextId = jsObject.GetPropertyAsInt32("contextId");
				jsInfo.FboId = (uint)jsObject.GetPropertyAsInt32("fboId");
				jsInfo.Stencil = jsObject.GetPropertyAsInt32("stencil");
				jsInfo.Samples = jsObject.GetPropertyAsInt32("samples");
				jsInfo.Depth = jsObject.GetPropertyAsInt32("depth");
				return jsInfo;
#else
				return nativeSwapChainPanel.CreateContext();
#endif
			}

#if NET7_0_OR_GREATER
			[JSImport("globalThis.SkiaSharp.Views.Windows.SKSwapChainPanel.createContextStatic")]
			private static partial NativeSwapChainPanel CreateContextInternal(NativeSwapChainPanel nativeSwapChainPanel, string canvasId);
#endif

#if NET7_0_OR_GREATER
			[JSImport("globalThis.SkiaSharp.Views.Windows.SKSwapChainPanel.setEnableRenderLoop")]
			internal static partial void SetEnableRenderLoop(NativeSwapChainPanel nativeSwapChainPanel, bool enable);
#else
			internal static void SetEnableRenderLoop(NativeSwapChainPanel nativeSwapChainPanel, bool enable)
			{
				nativeSwapChainPanel.SetEnableRenderLoop(enable);
			}
#endif

#if NETSTANDARD2_0 || NET6_0 || !WINUI
			internal class SKSwapChainPanelJsInterop : Uno.Foundation.Interop.IJSObject, Uno.Foundation.Interop.IJSObjectMetadata
			{
				private static long handleCounter = 0L;

				private readonly long jsHandle;

				public SKSwapChainPanelJsInterop(SKSwapChainPanel panel)
				{
					Panel = panel ?? throw new ArgumentNullException(nameof(panel));

					jsHandle = Interlocked.Increment(ref handleCounter);
					Handle = Uno.Foundation.Interop.JSObjectHandle.Create(this, this);
				}

				public SKSwapChainPanel Panel { get; }

				public Uno.Foundation.Interop.JSObjectHandle Handle { get; }

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
					var resultString = WebAssemblyRuntime.InvokeJSWithInterop($"return {this}.createContextLegacy('{Panel.GetHtmlId()}');");
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

				long Uno.Foundation.Interop.IJSObjectMetadata.CreateNativeInstance(IntPtr managedHandle)
				{
					WebAssemblyRuntime.InvokeJS(SKSwapChainPanelTypeFullName + $".createInstanceLegacy('{managedHandle}', '{jsHandle}')");
					return jsHandle;
				}

				string Uno.Foundation.Interop.IJSObjectMetadata.GetNativeInstance(IntPtr managedHandle, long jsHandle) =>
					SKSwapChainPanelTypeFullName + $".getInstanceLegacy('{jsHandle}')";

				void Uno.Foundation.Interop.IJSObjectMetadata.DestroyNativeInstance(IntPtr managedHandle, long jsHandle) =>
					WebAssemblyRuntime.InvokeJS(SKSwapChainPanelTypeFullName + $".destroyInstanceLegacy('{jsHandle}')");

				object Uno.Foundation.Interop.IJSObjectMetadata.InvokeManaged(object instance, string method, string parameters)
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
#endif
		}
	}
}
