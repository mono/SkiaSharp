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
	#if __ANDROID__ && !HAS_UNO
	/// <summary>Provides data for the <c>PaintSurface</c> events raised by <see cref="SkiaSharp.Views.Android.SKGLSurfaceView" /> and <see cref="SkiaSharp.Views.Android.SKGLTextureView" />.</summary>
	/// <remarks />
	#elif (WINDOWS || HAS_UNO_WINUI) && !__DESKTOP__
	/// <summary>Provides data for the <see cref="E:SkiaSharp.Views.Windows.SKSwapChainPanel.PaintSurface" /> event.</summary>
	/// <remarks />
	#else
	/// <summary>Provides data for the <c>PaintSurface</c> event.</summary>
	/// <remarks />
	#endif
	public class SKPaintGLSurfaceEventArgs : EventArgs
	{
		#if __ANDROID__
		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.Views.Android.SKPaintGLSurfaceEventArgs" /> event arguments.</summary>
		/// <param name="surface">The surface that is being drawn on.</param>
		/// <param name="renderTarget">The render target that is currently being drawn.</param>
		/// <remarks />
		#elif (WINDOWS || HAS_UNO_WINUI) && !__DESKTOP__
		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.Views.Windows.SKPaintGLSurfaceEventArgs" /> event arguments.</summary>
		/// <param name="surface">The surface that is being drawn on.</param>
		/// <param name="renderTarget">The render target that is currently being drawn.</param>
		/// <remarks />
		#else
		/// <summary>Initializes a new instance of the <see cref="SKPaintGLSurfaceEventArgs" /> class.</summary>
		/// <param name="surface">The surface that is being drawn on.</param>
		/// <param name="renderTarget">The render target that is currently being drawn.</param>
		/// <remarks />
		#endif
		public SKPaintGLSurfaceEventArgs(SKSurface surface, GRBackendRenderTarget renderTarget)
			: this(surface, renderTarget, GRSurfaceOrigin.BottomLeft, SKColorType.Rgba8888)
		{
		}

		#if __ANDROID__
		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.Views.Android.SKPaintGLSurfaceEventArgs" /> event arguments.</summary>
		/// <param name="surface">The surface that is being drawn on.</param>
		/// <param name="renderTarget">The render target that is currently being drawn.</param>
		/// <param name="origin">The surface origin of the render target.</param>
		/// <param name="colorType">The color type of the render target.</param>
		/// <remarks />
		#elif (WINDOWS || HAS_UNO_WINUI) && !__DESKTOP__
		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.Views.Windows.SKPaintGLSurfaceEventArgs" /> event arguments.</summary>
		/// <param name="surface">The surface that is being drawn on.</param>
		/// <param name="renderTarget">The render target that is currently being drawn.</param>
		/// <param name="origin">The surface origin of the render target.</param>
		/// <param name="colorType">The color type of the render target.</param>
		/// <remarks />
		#else
		/// <summary>Initializes a new instance of the <see cref="SKPaintGLSurfaceEventArgs" /> class.</summary>
		/// <param name="surface">The surface that is being drawn on.</param>
		/// <param name="renderTarget">The render target that is currently being drawn.</param>
		/// <param name="origin">The surface origin of the render target.</param>
		/// <param name="colorType">The color type of the render target.</param>
		/// <remarks />
		#endif
		public SKPaintGLSurfaceEventArgs(SKSurface surface, GRBackendRenderTarget renderTarget, GRSurfaceOrigin origin, SKColorType colorType)
		{
			Surface = surface;
			BackendRenderTarget = renderTarget;
			ColorType = colorType;
			Origin = origin;
			Info = new SKImageInfo(renderTarget.Width, renderTarget.Height, ColorType);
			RawInfo = Info;
		}

		#if __ANDROID__
		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.Views.Android.SKPaintGLSurfaceEventArgs" /> event arguments.</summary>
		/// <param name="surface">The surface that is being drawn on.</param>
		/// <param name="renderTarget">The render target that is currently being drawn.</param>
		/// <param name="origin">The surface origin of the render target.</param>
		/// <param name="info">The image information of the surface.</param>
		/// <remarks />
		#elif (WINDOWS || HAS_UNO_WINUI) && !__DESKTOP__
		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.Views.Windows.SKPaintGLSurfaceEventArgs" /> event arguments with the specified surface, render target, origin, and image information.</summary>
		/// <param name="surface">The surface that is being drawn on.</param>
		/// <param name="renderTarget">The render target that is currently being drawn.</param>
		/// <param name="origin">The surface origin of the render target.</param>
		/// <param name="info">The image information for the surface.</param>
		/// <remarks />
		#else
		/// <summary>Initializes a new instance of the <see cref="SKPaintGLSurfaceEventArgs" /> class.</summary>
		/// <param name="surface">The surface that is being drawn on.</param>
		/// <param name="renderTarget">The render target that is currently being drawn.</param>
		/// <param name="origin">The surface origin of the render target.</param>
		/// <param name="info">The image information of the surface.</param>
		/// <remarks />
		#endif
		public SKPaintGLSurfaceEventArgs(SKSurface surface, GRBackendRenderTarget renderTarget, GRSurfaceOrigin origin, SKImageInfo info)
			: this(surface, renderTarget, origin, info, info)
		{
		}

		#if __ANDROID__
		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.Views.Android.SKPaintGLSurfaceEventArgs" /> event arguments.</summary>
		/// <param name="surface">The surface that is being drawn on.</param>
		/// <param name="renderTarget">The render target that is currently being drawn.</param>
		/// <param name="origin">The surface origin of the render target.</param>
		/// <param name="info">The image information of the surface.</param>
		/// <param name="rawInfo">The raw image information of the surface.</param>
		/// <remarks />
		#elif (WINDOWS || HAS_UNO_WINUI) && !__DESKTOP__
		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.Views.Windows.SKPaintGLSurfaceEventArgs" /> event arguments with the specified surface, render target, origin, image information, and raw image information.</summary>
		/// <param name="surface">The surface that is being drawn on.</param>
		/// <param name="renderTarget">The render target that is currently being drawn.</param>
		/// <param name="origin">The surface origin of the render target.</param>
		/// <param name="info">The image information for the surface.</param>
		/// <param name="rawInfo">The raw image information of the underlying render target.</param>
		/// <remarks />
		#else
		/// <summary>Initializes a new instance of the <see cref="SKPaintGLSurfaceEventArgs" /> class.</summary>
		/// <param name="surface">The surface that is being drawn on.</param>
		/// <param name="renderTarget">The render target that is currently being drawn.</param>
		/// <param name="origin">The surface origin of the render target.</param>
		/// <param name="info">The image information of the surface.</param>
		/// <param name="rawInfo">The raw image information of the surface.</param>
		/// <remarks />
		#endif
		public SKPaintGLSurfaceEventArgs(SKSurface surface, GRBackendRenderTarget renderTarget, GRSurfaceOrigin origin, SKImageInfo info, SKImageInfo rawInfo)
		{
			Surface = surface;
			BackendRenderTarget = renderTarget;
			ColorType = info.ColorType;
			Origin = origin;
			Info = info;
			RawInfo = rawInfo;
		}

		#if __ANDROID__
		/// <summary>Gets the surface that is currently being drawn on.</summary>
		/// <value>The current surface.</value>
		/// <remarks />
		#else
		/// <summary>Gets the surface that is currently being drawn on.</summary>
		/// <value>The current surface.</value>
		/// <remarks />
		#endif
		public SKSurface Surface { get; private set; }

		#if __ANDROID__
		/// <summary>Gets the render target that is currently being drawn.</summary>
		/// <value>The current render target.</value>
		/// <remarks />
		#else
		/// <summary>Gets the render target that is currently being drawn.</summary>
		/// <value>The current render target.</value>
		/// <remarks />
		#endif
		public GRBackendRenderTarget BackendRenderTarget { get; private set; }

		/// <summary>Gets the color type of the render target.</summary>
		/// <value>The color type of the render target.</value>
		/// <remarks />
		public SKColorType ColorType { get; private set; }

		/// <summary>Gets the surface origin of the render target.</summary>
		/// <value>The surface origin of the render target.</value>
		/// <remarks />
		public GRSurfaceOrigin Origin { get; private set; }

		#if __ANDROID__
		/// <summary>Gets the image information of the surface.</summary>
		/// <value>The <see cref="T:SkiaSharp.SKImageInfo" /> containing the dimensions and color type of the surface.</value>
		/// <remarks />
		#else
		/// <summary>Gets the image information of the surface.</summary>
		/// <value>The <see cref="T:SkiaSharp.SKImageInfo" /> containing the dimensions and color type of the surface.</value>
		/// <remarks />
		#endif
		public SKImageInfo Info { get; private set; }

		#if __ANDROID__
		/// <summary>Gets the raw image information of the underlying render target.</summary>
		/// <value>The <see cref="T:SkiaSharp.SKImageInfo" /> containing the actual dimensions and color type of the underlying render target.</value>
		/// <remarks />
		#else
		/// <summary>Gets the raw image information of the underlying render target.</summary>
		/// <value>The <see cref="T:SkiaSharp.SKImageInfo" /> containing the actual dimensions and color type of the underlying render target.</value>
		/// <remarks />
		#endif
		public SKImageInfo RawInfo { get; private set; }
	}
}
#endif
