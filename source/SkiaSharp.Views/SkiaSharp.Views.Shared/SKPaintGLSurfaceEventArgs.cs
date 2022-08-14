#if !__WATCHOS__
using System;
using System.ComponentModel;

#if HAS_UNO_WINUI
namespace SkiaSharp.Views.Windows
#elif (WINDOWS_UWP || HAS_UNO)
namespace SkiaSharp.Views.UWP
#elif __ANDROID__
namespace SkiaSharp.Views.Android
#elif __TVOS__
namespace SkiaSharp.Views.tvOS
#elif __IOS__
namespace SkiaSharp.Views.iOS
#elif __DESKTOP__
namespace SkiaSharp.Views.Desktop
#elif __MACOS__
namespace SkiaSharp.Views.Mac
#elif __TIZEN__
namespace SkiaSharp.Views.Tizen
#elif WINDOWS
namespace SkiaSharp.Views.Windows
#elif __BLAZOR__
namespace SkiaSharp.Views.Blazor
#endif
{
	public class SKPaintGLSurfaceEventArgs : EventArgs
	{
#if !__BLAZOR__
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete]
		private GRBackendRenderTargetDesc? rtDesc;

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("Use SKPaintGLSurfaceEventArgs(SKSurface, GRBackendRenderTarget, SKColorType, GRSurfaceOrigin) instead.")]
		public SKPaintGLSurfaceEventArgs(SKSurface surface, GRBackendRenderTargetDesc renderTarget)
		{
			Surface = surface;
			rtDesc = renderTarget;
			BackendRenderTarget = new GRBackendRenderTarget(GRBackend.OpenGL, renderTarget);
			ColorType = renderTarget.Config.ToColorType();
			Origin = renderTarget.Origin;
			Info = new SKImageInfo(renderTarget.Width, renderTarget.Height, ColorType);
			RawInfo = Info;
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
			Info = new SKImageInfo(renderTarget.Width, renderTarget.Height, ColorType);
			RawInfo = Info;
		}

		public SKPaintGLSurfaceEventArgs(SKSurface surface, GRBackendRenderTarget renderTarget, GRSurfaceOrigin origin, SKImageInfo info)
			: this(surface, renderTarget, origin, info, info)
		{
		}

		public SKPaintGLSurfaceEventArgs(SKSurface surface, GRBackendRenderTarget renderTarget, GRSurfaceOrigin origin, SKImageInfo info, SKImageInfo rawInfo)
		{
			Surface = surface;
			BackendRenderTarget = renderTarget;
			ColorType = info.ColorType;
			Origin = origin;
			Info = info;
			RawInfo = rawInfo;
		}

#if !__BLAZOR__
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("Use SKPaintGLSurfaceEventArgs(SKSurface, GRBackendRenderTarget, GRSurfaceOrigin, SKColorType) instead.")]
		public SKPaintGLSurfaceEventArgs(SKSurface surface, GRBackendRenderTarget renderTarget, GRSurfaceOrigin origin, SKColorType colorType, GRGlFramebufferInfo glInfo)
		{
			Surface = surface;
			BackendRenderTarget = renderTarget;
			ColorType = colorType;
			Origin = origin;
			rtDesc = CreateDesc(glInfo);
			Info = new SKImageInfo(renderTarget.Width, renderTarget.Height, colorType);
			RawInfo = Info;
		}
#endif

		public SKSurface Surface { get; private set; }

#if !__BLAZOR__
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("Use BackendRenderTarget instead.")]
		public GRBackendRenderTargetDesc RenderTarget => rtDesc ??= CreateDesc(BackendRenderTarget.GetGlFramebufferInfo());

		[Obsolete]
		private GRBackendRenderTargetDesc CreateDesc(GRGlFramebufferInfo glInfo) =>
			new GRBackendRenderTargetDesc
			{
				Width = BackendRenderTarget.Width,
				Height = BackendRenderTarget.Height,
				RenderTargetHandle = (IntPtr)glInfo.FramebufferObjectId,
				SampleCount = BackendRenderTarget.SampleCount,
				StencilBits = BackendRenderTarget.StencilBits,
				Config = ColorType.ToPixelConfig(),
				Origin = Origin,
			};
#endif

		public GRBackendRenderTarget BackendRenderTarget { get; private set; }

		public SKColorType ColorType { get; private set; }

		public GRSurfaceOrigin Origin { get; private set; }

		public SKImageInfo Info { get; private set; }

		public SKImageInfo RawInfo { get; private set; }
	}
}
#endif
