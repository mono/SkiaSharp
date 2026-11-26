#nullable disable

using System;

namespace SkiaSharp
{
	/// <summary>A type of <see cref="T:SkiaSharp.SKCanvas" /> that draws to multiple canvases at the same time.</summary>
	/// <remarks />
	public class SKNWayCanvas : SKNoDrawCanvas
	{
		internal SKNWayCanvas (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		/// <summary>Creates a new <see cref="T:SkiaSharp.SKNWayCanvas" /> with the specified dimensions.</summary>
		/// <param name="width">The width of the canvas.</param>
		/// <param name="height">The height of the canvas.</param>
		/// <remarks />
		public SKNWayCanvas (int width, int height)
			: this (IntPtr.Zero, true)
		{
			Handle = SkiaApi.sk_nway_canvas_new (width, height);
		}

		/// <summary>Adds a canvas to forward drawing commands to.</summary>
		/// <param name="canvas">The canvas to add.</param>
		/// <remarks />
		public void AddCanvas (SKCanvas canvas)
		{
			if (canvas == null)
				throw new ArgumentNullException (nameof (canvas));

			SkiaApi.sk_nway_canvas_add_canvas (Handle, canvas.Handle);
			GC.KeepAlive (canvas);
			GC.KeepAlive (this);
		}

		/// <summary>Removes a canvas from the list of canvases receiving drawing commands.</summary>
		/// <param name="canvas">The canvas to remove.</param>
		/// <remarks />
		public void RemoveCanvas (SKCanvas canvas)
		{
			if (canvas == null)
				throw new ArgumentNullException (nameof (canvas));

			SkiaApi.sk_nway_canvas_remove_canvas (Handle, canvas.Handle);
			GC.KeepAlive (canvas);
			GC.KeepAlive (this);
		}

		/// <summary>Remove all canvases.</summary>
		/// <remarks />
		public void RemoveAll ()
		{
			SkiaApi.sk_nway_canvas_remove_all (Handle);
			GC.KeepAlive (this);
		}
	}
}
