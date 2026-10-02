using System;

using Microsoft.Maui;

namespace SkiaSharp.Views.Maui
{
	/// <summary>Provides data for the <see cref="E:SkiaSharp.Views.Maui.Controls.SKCanvasView.PaintSurface" /> event.</summary>
	/// <remarks />
	public class SKPaintSurfaceEventArgs : EventArgs
	{
		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.Views.Maui.SKPaintSurfaceEventArgs" /> class.</summary>
		/// <param name="surface">The surface to draw on.</param>
		/// <param name="info">The image information describing the surface.</param>
		/// <remarks />
		public SKPaintSurfaceEventArgs(SKSurface surface, SKImageInfo info)
			: this(surface, info, info)
		{
		}

		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.Views.Maui.SKPaintSurfaceEventArgs" /> class with raw image information.</summary>
		/// <param name="surface">The surface to draw on.</param>
		/// <param name="info">The image information describing the surface (scaled for pixel density).</param>
		/// <param name="rawInfo">The raw image information describing the actual surface size.</param>
		/// <remarks />
		public SKPaintSurfaceEventArgs(SKSurface surface, SKImageInfo info, SKImageInfo rawInfo)
		{
			Surface = surface;
			Info = info;
			RawInfo = rawInfo;
		}

		/// <summary>Gets the surface to draw on.</summary>
		/// <value>The <see cref="T:SkiaSharp.SKSurface" /> that provides the canvas for drawing.</value>
		/// <remarks>Use <c>Surface.Canvas</c> to access the <see cref="T:SkiaSharp.SKCanvas" /> for drawing operations.</remarks>
		public SKSurface Surface { get; }

		/// <summary>Gets the image information describing the surface.</summary>
		/// <value>The image information that describes the size and format of the surface.</value>
		/// <remarks>When <see cref="P:SkiaSharp.Views.Maui.Controls.SKCanvasView.IgnorePixelScaling" /> is <see langword="true" />, this returns the user-visible size (logical pixels). Otherwise, it returns the same as <see cref="P:SkiaSharp.Views.Maui.SKPaintSurfaceEventArgs.RawInfo" />.</remarks>
		public SKImageInfo Info { get; }

		/// <summary>Gets the raw image information describing the actual surface size in physical pixels.</summary>
		/// <value>The raw image information representing the actual surface dimensions in physical device pixels.</value>
		/// <remarks>This is the actual pixel buffer size regardless of the <see cref="P:SkiaSharp.Views.Maui.Controls.SKCanvasView.IgnorePixelScaling" /> setting. On high-DPI devices, this will be larger than the logical view size.</remarks>
		public SKImageInfo RawInfo { get; }
	}
}
