using System;

#if __ANDROID__
namespace SkiaSharp.Views.Android
#elif __TVOS__
namespace SkiaSharp.Views.tvOS
#elif __IOS__
namespace SkiaSharp.Views.iOS
#elif __DESKTOP__ || __WPF__
namespace SkiaSharp.Views.Desktop
#elif WINDOWS_UWP
namespace SkiaSharp.Views.UWP
#elif __MACOS__
namespace SkiaSharp.Views.Mac
#endif
{
    public class SKPaintSurfaceEventArgs : EventArgs
	{
		public SKPaintSurfaceEventArgs(SKSurface surface, SKImageInfo info)
		{
			Surface = surface;
			Info = info;
		}

		public SKSurface Surface { get; private set; }

		public SKImageInfo Info { get; private set; }
	}
}
