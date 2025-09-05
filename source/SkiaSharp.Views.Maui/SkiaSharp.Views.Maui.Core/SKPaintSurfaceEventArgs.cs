using System;

using Microsoft.Maui;

namespace SkiaSharp.Views.Maui
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
