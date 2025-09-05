#if !MACCATALYST || HAS_UNO_WINUI
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
	/// <summary>
	/// Provides data for the <see cref="SKGLView.PaintSurface" /> event.
	/// </summary>
	/// <remarks>The event does not yet exist nor is this type currently in use, but exists for cross-platform compatibility.</remarks>
	public class SKPaintGLSurfaceEventArgs : EventArgs
	{
		/// <summary>
		/// Creates a new instance of the <see cref="SKPaintGLSurfaceEventArgs" /> event arguments.
		/// </summary>
		public SKPaintGLSurfaceEventArgs(SKSurface surface, GRBackendRenderTarget renderTarget)
			: this(surface, renderTarget, GRSurfaceOrigin.BottomLeft, SKColorType.Rgba8888)
		{
		}

		/// <summary>
		/// Creates a new instance of the <see cref="SKPaintGLSurfaceEventArgs" /> event arguments.
		/// </summary>
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

		/// <summary>
		/// Gets the surface that is currently being drawn on.
		/// </summary>
		public SKSurface Surface { get; private set; }

		/// <summary>
		/// Gets the render target that is currently being drawn.
		/// </summary>
		public GRBackendRenderTarget BackendRenderTarget { get; private set; }

		/// <summary>
		/// Gets the color type of the render target.
		/// </summary>
		public SKColorType ColorType { get; private set; }

		/// <summary>
		/// Gets the surface origin of the render target.
		/// </summary>
		public GRSurfaceOrigin Origin { get; private set; }

		public SKImageInfo Info { get; private set; }

		public SKImageInfo RawInfo { get; private set; }
	}
}
#endif
