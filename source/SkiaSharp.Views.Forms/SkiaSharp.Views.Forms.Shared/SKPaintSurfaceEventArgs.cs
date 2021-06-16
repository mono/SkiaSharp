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
		{
			Surface = surface;
			Info = info;
		}

		public SKSurface Surface { get; private set; }

		public SKImageInfo Info { get; private set; }
	}
}
