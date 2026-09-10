using System;

#if HAS_UNO_WINUI
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
#elif WINDOWS
namespace SkiaSharp.Views.Windows
#elif __BLAZOR__
namespace SkiaSharp.Views.Blazor
#endif
{
	#if __ANDROID__
	/// <summary>Provides data for the <see cref="E:SkiaSharp.Views.Android.SKCanvasView.PaintSurface" /> event.</summary>
	/// <remarks />
	#elif (WINDOWS || HAS_UNO_WINUI) && !__DESKTOP__
	/// <summary>Provides data for the <see cref="E:SkiaSharp.Views.Windows.SKXamlCanvas.PaintSurface" /> event.</summary>
	/// <remarks />
	#else
	/// <summary>Provides data for the <c>PaintSurface</c> event.</summary>
	/// <remarks />
	#endif
	public class SKPaintSurfaceEventArgs : EventArgs
	{
		#if __ANDROID__
		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.Views.Android.SKPaintSurfaceEventArgs" /> event arguments.</summary>
		/// <param name="surface">The surface that is being drawn on.</param>
		/// <param name="info">The information about the surface.</param>
		/// <remarks />
		#elif (WINDOWS || HAS_UNO_WINUI) && !__DESKTOP__
		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.Views.Windows.SKPaintSurfaceEventArgs" /> event arguments.</summary>
		/// <param name="surface">The surface that is being drawn on.</param>
		/// <param name="info">The information about the surface.</param>
		/// <remarks />
		#else
		/// <summary>Initializes a new instance of the <see cref="SKPaintSurfaceEventArgs" /> class.</summary>
		/// <param name="surface">The surface that is being drawn on.</param>
		/// <param name="info">The information about the surface.</param>
		/// <remarks />
		#endif
		public SKPaintSurfaceEventArgs(SKSurface surface, SKImageInfo info)
			: this(surface, info, info)
		{
		}

		#if __ANDROID__
		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.Views.Android.SKPaintSurfaceEventArgs" /> event arguments.</summary>
		/// <param name="surface">The surface that is being drawn on.</param>
		/// <param name="info">The information about the surface.</param>
		/// <param name="rawInfo">The raw image information of the surface.</param>
		/// <remarks />
		#elif (WINDOWS || HAS_UNO_WINUI) && !__DESKTOP__
		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.Views.Windows.SKPaintSurfaceEventArgs" /> event arguments with the specified surface, image information, and raw image information.</summary>
		/// <param name="surface">The surface that is being drawn on.</param>
		/// <param name="info">The information about the surface.</param>
		/// <param name="rawInfo">The raw image information of the underlying surface.</param>
		/// <remarks />
		#else
		/// <summary>Initializes a new instance of the <see cref="SKPaintSurfaceEventArgs" /> class.</summary>
		/// <param name="surface">The surface that is being drawn on.</param>
		/// <param name="info">The information about the surface.</param>
		/// <param name="rawInfo">The raw image information of the surface.</param>
		/// <remarks />
		#endif
		public SKPaintSurfaceEventArgs(SKSurface surface, SKImageInfo info, SKImageInfo rawInfo)
		{
			Surface = surface;
			Info = info;
			RawInfo = rawInfo;
		}

		#if __ANDROID__
		/// <summary>Gets the surface that is currently being drawn on.</summary>
		/// <value>The surface being drawn on.</value>
		/// <remarks />
		#else
		/// <summary>Gets the surface that is currently being drawn on.</summary>
		/// <value>The drawing surface.</value>
		/// <remarks />
		#endif
		public SKSurface Surface { get; }

		#if __ANDROID__
		/// <summary>Gets the information about the surface that is currently being drawn.</summary>
		/// <value>The information about the surface.</value>
		/// <remarks />
		#else
		/// <summary>Gets the information about the surface that is currently being drawn.</summary>
		/// <value>The surface information.</value>
		/// <remarks />
		#endif
		public SKImageInfo Info { get; }

		#if __ANDROID__
		/// <summary>Gets the raw image information of the surface.</summary>
		/// <value>The raw image information of the surface.</value>
		/// <remarks />
		#else
		/// <summary>Gets the raw image information of the underlying surface.</summary>
		/// <value>The <see cref="T:SkiaSharp.SKImageInfo" /> containing the actual dimensions and color type of the underlying surface.</value>
		/// <remarks />
		#endif
		public SKImageInfo RawInfo { get; }
	}
}
