using System;
using System.ComponentModel;

#if __MAUI__
using Microsoft.Maui;
#else
using Xamarin.Forms;
#endif

#if __MAUI__
namespace SkiaSharp.Views.Maui
#else
namespace SkiaSharp.Views.Forms
#endif
{
	public class SKPaintGLSurfaceEventArgs : EventArgs
	{
#if !__MAUI__
		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete]
		private GRBackendRenderTargetDesc? rtDesc;

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete("Use SKPaintGLSurfaceEventArgs(SKSurface, GRBackendRenderTarget, SKColorType, GRSurfaceOrigin) instead.")]
		public SKPaintGLSurfaceEventArgs(SKSurface surface, GRBackendRenderTargetDesc renderTarget)
		{
			Surface = surface;
			rtDesc = renderTarget;
			BackendRenderTarget = new GRBackendRenderTarget(GRBackend.OpenGL, renderTarget);
			ColorType = renderTarget.Config.ToColorType();
			Origin = renderTarget.Origin;
		}
#endif

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

#if !__MAUI__
		[EditorBrowsable (EditorBrowsableState.Never)]
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
#endif

		public GRBackendRenderTarget BackendRenderTarget { get; private set; }

		public SKColorType ColorType { get; private set; }

		public GRSurfaceOrigin Origin { get; private set; }
	}
}
