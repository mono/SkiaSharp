#nullable disable

using System;

namespace SkiaSharp
{
	public class SKNWayCanvas : SKNoDrawCanvas
	{
		internal SKNWayCanvas (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		public SKNWayCanvas (int width, int height)
			: this (IntPtr.Zero, true)
		{
			Handle = SkiaApi.sk_nway_canvas_new (width, height);
		}

		public void AddCanvas (SKCanvas canvas)
		{
			if (canvas == null)
				throw new ArgumentNullException (nameof (canvas));

			SkiaApi.sk_nway_canvas_add_canvas (Handle, canvas.Handle);
			// The native SkNWayCanvas stores a raw, non-owning pointer to the added
			// canvas, so root the managed SKCanvas for the lifetime of this wrapper to
			// prevent it being finalized (and its native canvas destroyed) too early.
			Referenced (this, canvas);
			GC.KeepAlive (canvas);
			GC.KeepAlive (this);
		}

		public void RemoveCanvas (SKCanvas canvas)
		{
			if (canvas == null)
				throw new ArgumentNullException (nameof (canvas));

			SkiaApi.sk_nway_canvas_remove_canvas (Handle, canvas.Handle);
			Unreferenced (this, canvas);
			GC.KeepAlive (canvas);
			GC.KeepAlive (this);
		}

		public void RemoveAll ()
		{
			SkiaApi.sk_nway_canvas_remove_all (Handle);
			UnreferencedAll (this);
			GC.KeepAlive (this);
		}
	}
}
