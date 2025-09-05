#nullable disable

using System;

namespace SkiaSharp
{
	/// <summary>
	/// A type of <see cref="T:SkiaSharp.SKCanvas" /> that draws to multiple canvases at the same time.
	/// </summary>
	public class SKNWayCanvas : SKNoDrawCanvas
	{
		internal SKNWayCanvas (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		/// <summary>
		/// Creates a new <see cref="T:SkiaSharp.SKNWayCanvas" /> with the specified dimensions.
		/// </summary>
		/// <param name="width">The width of the canvas.</param>
		/// <param name="height">The height of the canvas.</param>
		public SKNWayCanvas (int width, int height)
			: this (IntPtr.Zero, true)
		{
			Handle = SkiaApi.sk_nway_canvas_new (width, height);
		}

		/// <param name="canvas">The canvas to add.</param>
		public void AddCanvas (SKCanvas canvas)
		{
			if (canvas == null)
				throw new ArgumentNullException (nameof (canvas));

			SkiaApi.sk_nway_canvas_add_canvas (Handle, canvas.Handle);
		}

		/// <param name="canvas">The canvas to remove.</param>
		public void RemoveCanvas (SKCanvas canvas)
		{
			if (canvas == null)
				throw new ArgumentNullException (nameof (canvas));

			SkiaApi.sk_nway_canvas_remove_canvas (Handle, canvas.Handle);
		}

		/// <summary>
		/// Remove all canvases.
		/// </summary>
		public void RemoveAll ()
		{
			SkiaApi.sk_nway_canvas_remove_all (Handle);
		}
	}
}
