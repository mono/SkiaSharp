using System;

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
	public class SKPaintSurfaceEventArgs : EventArgs
	{
		public SKPaintSurfaceEventArgs(SKSurface surface, SKImageInfo info)
			: this(surface, info, info)
		{
		}

		public SKPaintSurfaceEventArgs(SKSurface surface, SKImageInfo info, SKImageInfo rawInfo)
		{
			Surface = surface;
			Info = info;
			RawInfo = rawInfo;
		}

		public SKSurface Surface { get; }

		public SKImageInfo Info { get; }

		public SKImageInfo RawInfo { get; }
	}
}
