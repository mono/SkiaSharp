#if !__WATCHOS__
using System;
using System.ComponentModel;

#if WINDOWS
namespace SkiaSharp.Views.Windows
#elif WINDOWS_UWP || HAS_UNO
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
#endif
{
	public class SKPaintGLSurfaceEventArgs : EventArgs
	{
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

		public SKPaintGLSurfaceEventArgs(SKSurface surface, GRBackendRenderTarget renderTarget, GRSurfaceOrigin origin, SKColorType colorType, GRGlFramebufferInfo glInfo)
		{
			Surface = surface;
			BackendRenderTarget = renderTarget;
			ColorType = colorType;
			Origin = origin;
#pragma warning disable CS0612 // Type or member is obsolete
			rtDesc = CreateDesc(glInfo);
#pragma warning restore CS0612 // Type or member is obsolete
		}

		public SKSurface Surface { get; private set; }

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

		public GRBackendRenderTarget BackendRenderTarget { get; private set; }

		public SKColorType ColorType { get; private set; }

		public GRSurfaceOrigin Origin { get; private set; }
	}
}
#endif
