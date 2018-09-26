#if !__WATCHOS__
using System;

#if __ANDROID__
namespace SkiaSharp.Views.Android
#elif __TVOS__
namespace SkiaSharp.Views.tvOS
#elif __IOS__
namespace SkiaSharp.Views.iOS
#elif __DESKTOP__ || __WPF__ || __GTK__
namespace SkiaSharp.Views.Desktop
#elif WINDOWS_UWP
namespace SkiaSharp.Views.UWP
#elif __MACOS__
namespace SkiaSharp.Views.Mac
#elif __TIZEN__
namespace SkiaSharp.Views.Tizen
#endif
{
	public class SKPaintGLSurfaceEventArgs : EventArgs
	{
		[Obsolete]
		private GRBackendRenderTargetDesc? rtDesc;

		[Obsolete("Use SKPaintGLSurfaceEventArgs(SKSurface, GRBackendRenderTarget, SKColorType, GRSurfaceOrigin) instead.")]
		public SKPaintGLSurfaceEventArgs(SKSurface surface, GRBackendRenderTargetDesc renderTarget)
		{
			Surface = surface;
			rtDesc = renderTarget;
			BackendRenderTarget = new GRBackendRenderTarget(GRBackend.OpenGL, renderTarget);
			ColorType = renderTarget.Config.ToColorType();
			Origin = renderTarget.Origin;
		}

		public SKPaintGLSurfaceEventArgs(SKSurface surface, GRBackendRenderTarget renderTarget)
			: this(surface, renderTarget, GRSurfaceOrigin.BottomLeft, SKColorType.Rgba8888)
		{
		}

		public SKPaintGLSurfaceEventArgs(SKSurface surface, GRBackendRenderTarget renderTarget, GRSurfaceOrigin origin, SKColorType colorType)
		{
			Surface = surface;
			BackendRenderTarget = renderTarget;
			ColorType = colorType;
			Origin = origin;
		}

		public SKSurface Surface { get; private set; }

		[Obsolete("Use BackendRenderTarget instead.")]
		public GRBackendRenderTargetDesc RenderTarget
		{
			get
			{
				if (!rtDesc.HasValue)
				{
					var rth = IntPtr.Zero;
					if (BackendRenderTarget.GetGlFramebufferInfo(out var glInfo))
					{
						rth = (IntPtr)glInfo.FramebufferObjectId;
					}

					rtDesc = new GRBackendRenderTargetDesc
					{
						Width = BackendRenderTarget.Width,
						Height = BackendRenderTarget.Height,
						RenderTargetHandle = rth,
						SampleCount = BackendRenderTarget.SampleCount,
						StencilBits = BackendRenderTarget.StencilBits,
						Config = ColorType.ToPixelConfig(),
						Origin = Origin,
					};
				}

				return rtDesc.Value;
			}
		}

		public GRBackendRenderTarget BackendRenderTarget { get; private set; }

		public SKColorType ColorType { get; private set; }

		public GRSurfaceOrigin Origin { get; private set; }
	}
}
#endif
