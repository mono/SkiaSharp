using System;

#if __ANDROID__
namespace SkiaSharp.Views.Android
#elif __TVOS__
namespace SkiaSharp.Views.tvOS
#elif __WATCHOS__
namespace SkiaSharp.Views.watchOS
#elif __IOS__
namespace SkiaSharp.Views.iOS
#elif __DESKTOP__ || __WPF__ || __GTK__
namespace SkiaSharp.Views.Desktop
#elif WINDOWS_UWP
namespace SkiaSharp.Views.UWP
#elif __MACOS__
namespace SkiaSharp.Views.Mac
#elif TIZEN4_0
namespace SkiaSharp.Views.Tizen
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
