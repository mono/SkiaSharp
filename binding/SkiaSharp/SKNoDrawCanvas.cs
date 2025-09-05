#nullable disable

using System;

namespace SkiaSharp
{
	/// <summary>
	/// A type of <see cref="SKCanvas" /> that provides a base type for canvases that do not need to rasterize.
	/// </summary>
	/// <remarks>
	/// These canvases are not backed by any device/pixels and they use conservative clipping (clipping calls only use rectangles).
	/// </remarks>
	public class SKNoDrawCanvas : SKCanvas
	{
		internal SKNoDrawCanvas (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		/// <summary>
		/// Creates a new <see cref="SKNoDrawCanvas" /> with the specified dimensions.
		/// </summary>
		/// <param name="width">The width of the canvas.</param>
		/// <param name="height">The height of the canvas.</param>
		public SKNoDrawCanvas (int width, int height)
			: this (IntPtr.Zero, true)
		{
			Handle = SkiaApi.sk_nodraw_canvas_new (width, height);
		}
	}
}
