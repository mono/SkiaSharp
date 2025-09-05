using System;

using Microsoft.Maui;

namespace SkiaSharp.Views.Maui
{
	public class SKPaintSurfaceEventArgs : EventArgs
	{
		/// <param name="surface"></param>
		/// <param name="info"></param>
		public SKPaintSurfaceEventArgs(SKSurface surface, SKImageInfo info)
			: this(surface, info, info)
		{
		}

		/// <param name="surface"></param>
		/// <param name="info"></param>
		/// <param name="rawInfo"></param>
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
