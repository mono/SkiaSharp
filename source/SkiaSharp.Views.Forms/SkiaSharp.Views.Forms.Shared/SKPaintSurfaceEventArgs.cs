using System;

namespace SkiaSharp.Views
{
	[Obsolete("this should be used from the main SkiaSharp.Views")]
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
