#nullable disable

using System;

namespace SkiaSharp
{
	/// <summary>A canvas that captures all drawing commands, and rather than draw the actual content, it increments the alpha channel of each pixel every time it would have been touched by a draw call.</summary>
	/// <remarks>This is useful for detecting overdraw.</remarks>
	public class SKOverdrawCanvas : SKNWayCanvas
	{
		internal SKOverdrawCanvas (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		/// <summary>Creates a new <see cref="T:SkiaSharp.SKOverdrawCanvas" /> that wraps the specified <see cref="T:SkiaSharp.SKCanvas" />.</summary>
		/// <param name="canvas">The canvas to draw on.</param>
		/// <remarks />
		public SKOverdrawCanvas (SKCanvas canvas)
			: this (IntPtr.Zero, true)
		{
			if (canvas == null)
				throw new ArgumentNullException (nameof (canvas));

			Handle = SkiaApi.sk_overdraw_canvas_new (canvas.Handle);
			// The native SkOverdrawCanvas stores a raw, non-owning pointer to the wrapped
			// canvas, so root the managed SKCanvas for the lifetime of this wrapper to
			// prevent it being finalized (and its native canvas destroyed) too early.
			Referenced (this, canvas);
			GC.KeepAlive (canvas);
		}
	}
}
