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
	/// <summary>
	/// Provides data for the PaintSurface event.
	/// </summary>
	public class SKPaintSurfaceEventArgs : EventArgs
	{
		/// <summary>
		/// Creates a new instance of the <see cref="SKPaintSurfaceEventArgs" /> event arguments.
		/// </summary>
		/// <param name="surface">The surface that is being drawn on.</param>
		/// <param name="info">The information about the surface.</param>
		public SKPaintSurfaceEventArgs(SKSurface surface, SKImageInfo info)
			: this(surface, info, info)
		{
		}

		/// <summary>
		/// Creates a new instance of the <see cref="SKPaintSurfaceEventArgs" /> event arguments.
		/// </summary>
		public SKPaintSurfaceEventArgs(SKSurface surface, SKImageInfo info, SKImageInfo rawInfo)
		{
			Surface = surface;
			Info = info;
			RawInfo = rawInfo;
		}

		/// <summary>
		/// Gets the surface that is currently being drawn on.
		/// </summary>
		public SKSurface Surface { get; }

		/// <summary>
		/// Gets the information about the surface that is currently being drawn.
		/// </summary>
		public SKImageInfo Info { get; }

		public SKImageInfo RawInfo { get; }
	}
}
